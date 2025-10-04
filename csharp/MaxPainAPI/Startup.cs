using MaxPainInfrastructure.Code;
using MaxPainInfrastructure.Models;
using MaxPainInfrastructure.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.IO.Compression;
using System.Linq;


namespace MaxPainAPI
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));

            string AWSConnection = Configuration.GetConnectionString("AWSConnection");
            string HomeConnection = Configuration.GetConnectionString("HomeConnection");

            services.AddDbContext<AwsContext>(options => options.UseSqlServer(AWSConnection));
            services.AddDbContext<HomeContext>(options => options.UseSqlServer(HomeConnection));

            services.AddSingleton<IAppDbContextFactory, AppDbContextFactory>();

            // the DB Context is scoped, so any service using the DB should be scoped
            services.AddScoped<IChartService, ChartService>();
            services.AddScoped<IConfigurationService, ConfigurationService>();
            services.AddScoped<IControllerService, ControllerService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IFinDataService, FinDataService>();
            services.AddScoped<IFinImportService, FinImportService>();
            services.AddScoped<ILoggerService, LoggerService>();
            services.AddScoped<IMailerLiteService, MailerLiteService>();
            services.AddScoped<IUrlShortService, UrlShortService>();
            services.AddScoped<IHistoryService, HistoryService>();
            services.AddScoped<ISchwabService, SchwabService>();
            services.AddScoped<ISMSService, SMSService>();

            //services.AddScoped<TwitterHelper>();

            services.AddSingleton<ICalculationService, CalculationService>();
            services.AddSingleton<IDateService, DateService>();
            services.AddSingleton<ISecretService, SecretService>();

            // required for node.js to run javascript
            //services.AddNodeServices();

            services.AddRazorPages();

            //services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            services.AddDistributedMemoryCache(); // Adds a default in-memory implementation of IDistributedCache
            services.AddSession();

            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });


            services.Configure<GzipCompressionProviderOptions>(options =>
            {
                options.Level = CompressionLevel.Optimal;
            });
            services.AddResponseCompression(options =>
            {
                options.EnableForHttps = true;
                options.Providers.Add<GzipCompressionProvider>();
                options.Providers.Add<BrotliCompressionProvider>();
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] {
                    "application/xhtml+xml",
                    "application/atom+xml",
                    "image/svg+xml",
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseSpaStaticFiles();

            // IMPORTANT: This session call MUST go before UseMvc()
            app.UseSession();

            app.UseRouting();
            app.UseAuthorization();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
            /*
            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller}/{action=Index}/{id?}");
            });
            */

            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";

                if (env.IsDevelopment())
                {
                    //spa.UseAngularCliServer(npmScript: "start");
                }
            });

            app.UseResponseCompression();
        }
    }
}