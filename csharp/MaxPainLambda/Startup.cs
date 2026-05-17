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
            services.AddOptions();

            services.AddLogging(x => x.AddSerilog(dispose: true));

            services.Configure<System.Text.Json.JsonSerializerOptions>(options =>
            {
                options.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.PropertyNameCaseInsensitive = true;
                options.ReferenceHandler = ReferenceHandler.IgnoreCycles;
            });
        }
    }
}
