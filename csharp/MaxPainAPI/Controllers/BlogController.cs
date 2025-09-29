using MaxPainInfrastructure.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace MaxPainAPI.Controllers
{
    [Route("api/[controller]")]
    public class BlogController : Controller
    {
        private readonly HomeContext _homeContext;
        private readonly AwsContext _awsContext;
        private readonly IWebHostEnvironment _env;

        public BlogController(
            AwsContext awsContext,
            HomeContext homeContext,
            IWebHostEnvironment env)
        {
            _awsContext = awsContext;
            _homeContext = homeContext;
            _env = env;
        }

        [HttpGet("Entries")]
        public async Task<JsonResult> Entries()
        {
            List<BlogEntry> entries = await _awsContext.BlogEntry.ToListAsync();
            return Json(entries);
        }

        [HttpGet("Entry")]
        public async Task<JsonResult> Entry(long id)
        {
            BlogEntry entry = await _awsContext.BlogEntry.FirstOrDefaultAsync(x => x.Id == id);
            return Json(entry);
        }

        [HttpGet("EntryByTitle")]
        public async Task<JsonResult> EntryByTitle(string title)
        {
            string noDash = title.Replace("-", string.Empty);
            BlogEntry entry = await _awsContext.BlogEntry.FirstOrDefaultAsync(x => x.Title.ToLower() == noDash.ToLower());
            return Json(entry);
        }


        [HttpPost("Upsert")]
        public async Task<JsonResult> Upsert(string json, string pw)
        {
            BlogEntry entry = JsonConvert.DeserializeObject<BlogEntry>(json);

            DateTime currentDate = DateTime.UtcNow;
            entry.ModifiedOn = currentDate;

            bool isNew = true;
            if (entry.Id > 0) isNew = false;

            if (isNew)
            {
                entry.CreatedOn = currentDate;
                entry.ModifiedOn = currentDate;

                _awsContext.BlogEntry.Add(entry);
                _awsContext.Entry(entry).State = Microsoft.EntityFrameworkCore.EntityState.Added;
                _awsContext.SaveChanges();

                string title = entry.Title;
                BlogEntry result = await _awsContext.BlogEntry.FirstOrDefaultAsync(x => x.Title == title);
                return Json(result);
            }

            entry.ModifiedOn = currentDate;
            BlogEntry entity = await _awsContext.BlogEntry.FirstOrDefaultAsync(x => x.Id == entry.Id);
            CopyObject(entry, entity);

            _awsContext.Entry(entity).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            _awsContext.SaveChanges();

            entity = await _awsContext.BlogEntry.FirstOrDefaultAsync(x => x.Id == entry.Id);
            return Json(entity);
        }

        private void CopyObject(BlogEntry src, BlogEntry dst)
        {
            dst.Title = src.Title;
            dst.ImageUrl = src.ImageUrl;
            dst.Ordinal = src.Ordinal;
            dst.IsActive = src.IsActive;
            dst.ShowOnHome = src.ShowOnHome;
            dst.CreatedOn = src.CreatedOn;
            dst.ModifiedOn = src.ModifiedOn;
            dst.Content = src.Content;
        }
    }
}

