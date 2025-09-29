using MaxPainInfrastructure.Models;
using MaxPainInfrastructure.Services;
using MaxPainLambda;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.IO;

namespace UnitTestProject1
{
    [TestClass]
    public class BaseTests
    {

        public readonly HomeContext _homeContext;
        public readonly AwsContext _awsContext;

        protected ICalculationService CalculationSvc { get; set; }
        protected IChartService ChartSvc { get; set; }
        protected IConfigurationService ConfigurationSvc { get; set; }
        protected IControllerService ControllerSvc { get; set; }
        protected IEmailService EmailSvc { get; set; }
        protected IFinDataService FinDataSvc { get; set; }
        protected IFinImportService FinImportSvc { get; set; }
        protected ILambdaService LambdaSvc { get; set; }
        protected ILoggerService LoggerSvc { get; set; }
        protected IMailerLiteService MailerLiteSvc { get; set; }
        protected ISchwabService SchwabSvc { get; set; }
        protected ISecretService SecretSvc { get; set; }
        protected ISMSService SMSSvc { get; set; }
        protected IUrlShortService UrlShortSvc { get; set; }

        protected IHistoryService HistorySvc { get; set; }

        public BaseTests()
        {
            var config = new Config();
            var ConnectionStringsConfig = new Options<ConnectionStrings>(config.Get<ConnectionStrings>());

            var optionsBuilder = new DbContextOptionsBuilder<AwsContext>();
            optionsBuilder.UseSqlServer(ConnectionStringsConfig.Value.AWSConnection);
            _awsContext = new AwsContext(optionsBuilder.Options);

            var optionsBuilder2 = new DbContextOptionsBuilder<HomeContext>();
            optionsBuilder2.UseSqlServer(ConnectionStringsConfig.Value.HomeConnection);
            _homeContext = new HomeContext(optionsBuilder2.Options);


            IConfiguration configuration =
                new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json").Build();

            SecretSvc = new SecretService();
            UrlShortSvc = new UrlShortService(SecretSvc);
            LoggerSvc = new LoggerService(_awsContext);
            CalculationSvc = new CalculationService();
            ChartSvc = new ChartService(_awsContext, _homeContext, LoggerSvc, ConfigurationSvc, CalculationSvc, FinDataSvc, SecretSvc);
            ConfigurationSvc = new ConfigurationService(_awsContext, LoggerSvc, configuration);
            HistorySvc = new HistoryService(_awsContext, _homeContext, LoggerSvc, ConfigurationSvc, CalculationSvc);

            SchwabSvc = new SchwabService(LoggerSvc, SecretSvc);

            FinDataSvc = new FinDataService(_awsContext, ConfigurationSvc, LoggerSvc, CalculationSvc, SchwabSvc, SecretSvc);
            FinImportSvc = new FinImportService(_awsContext, _homeContext, LoggerSvc, ConfigurationSvc, CalculationSvc, FinDataSvc, HistorySvc);

            MailerLiteSvc = new MailerLiteService(SecretSvc, LoggerSvc);
            EmailSvc = new EmailService(LoggerSvc, MailerLiteSvc, SecretSvc, UrlShortSvc);

            SMSSvc = new SMSService(SecretSvc);
            ControllerSvc = new ControllerService(_awsContext, _homeContext, LoggerSvc, CalculationSvc, ChartSvc, ConfigurationSvc, EmailSvc, FinDataSvc, FinImportSvc, HistorySvc, SecretSvc, SMSSvc);
            LambdaSvc = new LambdaService(_awsContext, _homeContext, LoggerSvc, CalculationSvc, ChartSvc, ConfigurationSvc, ControllerSvc, EmailSvc, FinDataSvc, FinImportSvc, HistorySvc, SchwabSvc, SecretSvc, SMSSvc);

        }

        public static bool OpenInNotepad(string content)
        {
            string filename = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "OpenInNotepad.txt");
            File.WriteAllText(filename, content);

            //System.Diagnostics.Process.Start($"Notepad.exe {filename}");

            var process = new Process();
            process.StartInfo = new ProcessStartInfo()
            {
                UseShellExecute = true,
                FileName = filename
            };

            process.Start();
            //process.WaitForExit();

            return true;
        }
    }
}
