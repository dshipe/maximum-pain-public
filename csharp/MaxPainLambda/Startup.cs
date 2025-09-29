using Amazon.Lambda.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System.Text.Json.Serialization;

namespace MaxPainLambda
{
    [LambdaStartup]
    public class Startup
    {
        /// <summary>
        /// Services for Lambda functions can be registered in the services dependency injection container in this method. 
        ///
        /// The services can be injected into the Lambda function through the containing type's constructor or as a
        /// parameter in the Lambda function using the FromService attribute. Services injected for the constructor have
        /// the lifetime of the Lambda compute container. Services injected as parameters are created within the scope
        /// of the function invocation.
        /// </summary>
        public void ConfigureServices(IServiceCollection services)
        {
            //// Example of creating the IConfiguration object and
            //// adding it to the dependency injection container.
            //var builder = new ConfigurationBuilder()
            //                    .AddJsonFile("appsettings.json", true);

            //// Add AWS Systems Manager as a potential provider for the configuration. This is 
            //// available with the Amazon.Extensions.Configuration.SystemsManager NuGet package.
            //builder.AddSystemsManager("/app/settings");

            //var configuration = builder.Build();
            //services.AddSingleton<IConfiguration>(configuration);

            //// Example of using the AWSSDK.Extensions.NETCore.Setup NuGet package to add
            //// the Amazon S3 service client to the dependency injection container.
            //services.AddAWSService<Amazon.S3.IAmazonS3>();

            services.AddOptions();

            // add logging
            services.AddLogging(x => x.AddSerilog(dispose: true));

            services.Configure<System.Text.Json.JsonSerializerOptions>(options =>
            {
                // Set this to true to ignore null or default values
                options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.PropertyNameCaseInsensitive = true;
                options.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            });

            /*
            serviceCollection
                .AddDefaultAWSOptions(sp => sp.GetRequiredService<IConfiguration>().GetAWSOptions())
                .AddAWSService<IAmazonS3>()
                .AddAWSService<IAmazonSimpleNotificationService>()
                .AddAWSService<IAmazonSecretsManager>()
                .AddAWSService<IAmazonKinesis>();

            serviceCollection
                .AddAwsSecretsManagerGateway()
                .AddSlackGateway()
                .AddDynamoDb(config);
            */

            /*
            serviceCollection.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
            string AWSConnection = Configuration.GetConnectionString("AWSConnection");
            string HomeConnection = Configuration.GetConnectionString("HomeConnection");
            */

            // add services
            //serviceCollection.AddSingleton<IDateTimeService, UtcDateTimeService>();
        }
    }
}