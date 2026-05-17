using MaxPainInfrastructure.Code;
using MaxPainInfrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;

namespace UnitTestProject1
{
    [TestClass]
    public class FinImportTests : BaseTests
    {
        [TestMethod]
        public async Task IO_Date()
        {
            DateTime est = Convert.ToDateTime("2026-01-30 07:00:00");
            await FinImportSvc.IO_CalcESTDate(est);
            Assert.AreEqual(Convert.ToDateTime("2026-01-29 07:00:00"), FinImportSvc.MarketDate);
            Assert.IsFalse(FinImportSvc.IsWeekend);
            Assert.IsTrue(FinImportSvc.IsMorning);

            est = Convert.ToDateTime("2026-01-30 17:00:00");
            await FinImportSvc.IO_CalcESTDate(est);
            Assert.AreEqual(Convert.ToDateTime("2026-01-30 17:00:00"), FinImportSvc.MarketDate);
            Assert.IsFalse(FinImportSvc.IsWeekend);
            Assert.IsFalse(FinImportSvc.IsMorning);

            est = Convert.ToDateTime("2026-01-31 07:00:00");
            await FinImportSvc.IO_CalcESTDate(est);
            Assert.AreEqual(Convert.ToDateTime("2026-01-30 07:00:00"), FinImportSvc.MarketDate);
            Assert.IsTrue(FinImportSvc.IsWeekend);
            Assert.IsTrue(FinImportSvc.IsMorning);
        }


        [TestMethod]
        public async Task ShowMarketCalendar()
        {
            var result = await _homeContext.MarketCalendar
                .OrderByDescending(c => c.Date)
                .Take(30)
                .ToListAsync();
            Assert.AreNotEqual(0, result.Count);
        }

        [TestMethod]
        public async Task IO_ProcessChar()
        {
            var est = Convert.ToDateTime("2026-03-20 00:00:00");
            await FinImportSvc.IO_ProcessChar(est, 'B');
        }

        [TestMethod]
        public async Task IO_PostProcess()
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();

            var est = Convert.ToDateTime("2026-03-30 9:00:00");
            FinImportSvc.IsDebug = true;
            string log = await FinImportSvc.IO_PostProcess(est, true);

            timer.Stop();
            OpenInNotepad($"{timer.ElapsedMilliseconds.ToString()}\r\n\r\n{log}");
        }

        [TestMethod]
        public async Task MostActive()
        {
            Stopwatch timer = new Stopwatch();
            timer.Start();

            var chains = await FinImportSvc.FetchImportStaging();
            var sb = new System.Text.StringBuilder();
            var mostActive = await FinImportSvc.MostActive(chains, sb, Convert.ToDateTime("2026-04-07"), Convert.ToDateTime("2026-04-06"), true);

            timer.Stop();

            var json = DBHelper.Serialize(mostActive);
            OpenInNotepad($"{timer.ElapsedMilliseconds.ToString()}\r\n\r\n{json}");
        }

        /// <summary>
        /// Lightweight benchmark for MostActive. Runs warmup + N timed iterations
        /// using the same in-memory data so the run measures CPU/allocations,
        /// not DB I/O. Uses IsDebug=true so BuildMA and TRUNCATE/spMPOutsideOIWallsXML
        /// are skipped.
        /// </summary>
        [TestMethod]
        public async Task MostActive_Benchmark()
        {
            const int warmup = 1;
            const int iterations = 5;

            FinImportSvc.IsDebug = true;

            var importDate = Convert.ToDateTime("2026-04-07");
            var previousDate = Convert.ToDateTime("2026-04-06");

            // Load the chains once so each iteration measures MostActive only.
            var loadTimer = Stopwatch.StartNew();
            var chains = await FinImportSvc.FetchImportStaging();
            loadTimer.Stop();

            // Warmup (JIT, dictionary, etc.)
            for (int i = 0; i < warmup; i++)
            {
                var wsb = new System.Text.StringBuilder();
                await FinImportSvc.MostActive(chains, wsb, importDate, previousDate, true);
            }

            var samples = new long[iterations];
            int lastCount = 0;
            long beforeAllocBytes = GC.GetTotalAllocatedBytes(precise: true);

            for (int i = 0; i < iterations; i++)
            {
                var sb = new System.Text.StringBuilder();
                var sw = Stopwatch.StartNew();
                var result = await FinImportSvc.MostActive(chains, sb, importDate, previousDate, true);
                sw.Stop();
                samples[i] = sw.ElapsedMilliseconds;
                lastCount = result.Count;
            }

            long afterAllocBytes = GC.GetTotalAllocatedBytes(precise: true);

            Array.Sort(samples);
            long total = 0;
            for (int i = 0; i < samples.Length; i++) total += samples[i];
            double avg = total / (double)samples.Length;
            long min = samples[0];
            long max = samples[samples.Length - 1];
            long median = samples[samples.Length / 2];
            double allocMb = (afterAllocBytes - beforeAllocBytes) / 1024d / 1024d;

            var report = new System.Text.StringBuilder();
            report.AppendLine($"MostActive_Benchmark");
            report.AppendLine($"  FetchImportStaging: {loadTimer.ElapsedMilliseconds} ms (chains={chains.Count})");
            report.AppendLine($"  iterations:         {iterations} (warmup={warmup})");
            report.AppendLine($"  result count:       {lastCount}");
            report.AppendLine($"  min/median/avg/max: {min} / {median} / {avg:F1} / {max} ms");
            report.AppendLine($"  samples (ms):       {string.Join(", ", samples)}");
            report.AppendLine($"  total allocations:  {allocMb:F1} MB across {iterations} iterations");

            OpenInNotepad(report.ToString());
        }
    }
}
