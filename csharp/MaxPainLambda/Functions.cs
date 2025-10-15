using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Amazon.Lambda.Model;
using MaxPainInfrastructure.Models;
using MaxPainInfrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace MaxPainLambda
{
    public class Functions
    {
        private readonly ILogger<Functions> _logger;
        private readonly AwsContext _awsContext;
        private readonly HomeContext _homeContext;

        private readonly ICalculationService _calculationSvc;
        private readonly IChartService _chartSvc;
        private readonly IConfigurationService _configurationSvc;
        private readonly IControllerService _controllerSvc;
        //private readonly IDateService _dateSvc;
        private readonly IEmailService _emailSvc;
        private readonly IFinDataService _finDataSvc;
        private readonly IFinImportService _finImportSvc;
        private readonly IHistoryService _historySvc;
        private readonly ILambdaService _lambdaSvc;
        private readonly ILoggerService _loggerSvc;
        //private readonly IMailerLiteService _mailerLiteSvc;
        private readonly ISchwabService _schwabSvc;
        private readonly ISecretService _secretSvc;
        private readonly ISMSService _smsService;

        //private readonly IUrlShortService _urlShortSvc;

        /// <summary>
        /// Default constructor that Lambda will invoke.
        /// </summary>
        public Functions()
        {
            var app = Host.CreateDefaultBuilder()
                .ConfigureServices(ConfigureServices)
                .Build();

            var serviceProvider = app.Services;
            _logger = serviceProvider.GetRequiredService<ILogger<Functions>>();
            //_logger.LogInformation("Function: empty constructor");

            _awsContext = serviceProvider.GetServices<AwsContext>().First();
            _homeContext = serviceProvider.GetServices<HomeContext>().First();

            _lambdaSvc = serviceProvider.GetRequiredService<ILambdaService>();

            _calculationSvc = serviceProvider.GetRequiredService<ICalculationService>();
            _chartSvc = serviceProvider.GetRequiredService<IChartService>();
            _configurationSvc = serviceProvider.GetRequiredService<IConfigurationService>();
            _controllerSvc = serviceProvider.GetRequiredService<IControllerService>();
            //_dateSvc = serviceProvider.GetRequiredService<IDateService>();
            _emailSvc = serviceProvider.GetRequiredService<IEmailService>();
            _finDataSvc = serviceProvider.GetRequiredService<IFinDataService>();
            _finImportSvc = serviceProvider.GetRequiredService<IFinImportService>();
            _historySvc = serviceProvider.GetRequiredService<IHistoryService>();
            _loggerSvc = serviceProvider.GetRequiredService<ILoggerService>();
            //_mailerLiteSvc = serviceProvider.GetRequiredService<IMailerLiteService>();
            _schwabSvc = serviceProvider.GetRequiredService<ISchwabService>();
            _secretSvc = serviceProvider.GetRequiredService<ISecretService>();
            _smsService = serviceProvider.GetRequiredService<ISMSService>();
            //_urlShortSvc = serviceProvider.GetRequiredService<IUrlShortService>();
        }

        public void ConfigureServices(HostBuilderContext context, IServiceCollection serviceCollection)
        {
            var config = context.Configuration;

            serviceCollection.AddOptions();

            // add logging
            serviceCollection.AddLogging(builder => builder.AddSerilog(dispose: true));

            var secretService = new SecretService();

            string AWSConnection = secretService.GetValue("CONNSTR_AWS").Result;
            serviceCollection.AddDbContext<AwsContext>(options => options.UseSqlServer(AWSConnection));
            string HomeConnection = secretService.GetValue("CONNSTR_HOME").Result;
            serviceCollection.AddDbContext<HomeContext>(options => options.UseSqlServer(HomeConnection));

            // add services
            serviceCollection.AddScoped<ILambdaService, LambdaService>();

            serviceCollection.AddScoped<ICalculationService, CalculationService>();
            serviceCollection.AddScoped<IChartService, ChartService>();
            serviceCollection.AddScoped<IConfigurationService, ConfigurationService>();
            serviceCollection.AddScoped<IControllerService, ControllerService>();
            serviceCollection.AddScoped<IDateService, DateService>();
            serviceCollection.AddScoped<IEmailService, EmailService>();
            serviceCollection.AddScoped<IFinDataService, FinDataService>();
            serviceCollection.AddScoped<IFinImportService, FinImportService>();
            serviceCollection.AddScoped<IHistoryService, HistoryService>();
            serviceCollection.AddScoped<ILoggerService, LoggerService>();
            serviceCollection.AddScoped<IMailerLiteService, MailerLiteService>();
            serviceCollection.AddScoped<ISchwabService, SchwabService>();
            serviceCollection.AddScoped<ISecretService, SecretService>();
            serviceCollection.AddScoped<ISMSService, SMSService>();
            serviceCollection.AddScoped<IUrlShortService, UrlShortService>();
        }

        /// <summary>
        /// A Lambda function to respond to HTTP Get methods from API Gateway
        /// </summary>
        /// <remarks>
        /// This uses the <see href="https://github.com/aws/aws-lambda-dotnet/blob/master/Libraries/src/Amazon.Lambda.Annotations/README.md">Lambda Annotations</see> 
        /// programming model to bridge the gap between the Lambda programming model and a more idiomatic .NET model.
        /// 
        /// This automatically handles reading parameters from an APIGatewayProxyRequest
        /// as well as syncing the function definitions to serverless.template each time you build.
        /// 
        /// If you do not wish to use this model and need to manipulate the API Gateway 
        /// objects directly, see the accompanying Readme.md for instructions.
        /// </remarks>
        /// <param name="context">Information about the invocation, function, and execution environment</param>
        /// <returns>The response as an implicit <see cref="APIGatewayProxyResponse"/></returns>

        //[LambdaFunction]
        //[RestApi(LambdaHttpMethod.Get, "/")]
        public APIGatewayHttpApiV2ProxyResponse GetHandler(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            APIGatewayHttpApiV2ProxyRequest x = new APIGatewayHttpApiV2ProxyRequest();

            return GetHandlerAsync(request, context).Result;
        }

        public async Task<APIGatewayHttpApiV2ProxyResponse> GetHandlerAsync(APIGatewayHttpApiV2ProxyRequest request, ILambdaContext context)
        {
            try
            {
                return await _lambdaSvc.HandleRequest(request);
            }
            catch (RequestTooLargeException tooLargeEx)
            {
                string msg = $"{tooLargeEx.ToString()}\r\n\r\n{_lambdaSvc.BuildLogMessage(request)}";
                context.Logger.LogError(msg);
                throw;
            }
            catch (Exception ex)
            {
                string msg = $"{ex.ToString()}\r\n\r\n{_lambdaSvc.BuildLogMessage(request)}";
                context.Logger.LogError(msg);
                return _lambdaSvc.ReturnError(502, msg);
            }
        }
    }
}