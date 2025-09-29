using Amazon.Lambda.APIGatewayEvents;
using MaxPainLambda;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading.Tasks;

namespace UnitTestProject1
{
    [TestClass]
    public class LambdaTests : BaseTests
    {
        protected ILambdaService LambdaSvc { get; set; }

        public LambdaTests()
        {
            LambdaSvc = new LambdaService(
                _awsContext,
                _homeContext,
                LoggerSvc,
                CalculationSvc,
                ChartSvc,
                ConfigurationSvc,
                ControllerSvc,
                EmailSvc,
                FinDataSvc,
                FinImportSvc,
                HistorySvc,
                SchwabSvc,
                SecretSvc,
                SMSSvc
            );
        }

        [TestMethod]
        public async Task HandleRequest()
        {
            var requestFilename = @$"{Directory.GetCurrentDirectory()}\APIGatewayHttpApiV2ProxyRequest.json";
            var json = System.IO.File.ReadAllText(requestFilename);

            //var model = Newtonsoft.Json.JsonConvert.DeserializeObject<RequestModel>(json);
            var request = Newtonsoft.Json.JsonConvert.DeserializeObject<APIGatewayHttpApiV2ProxyRequest>(json);

            var response = LambdaSvc.HandleRequest(request);
        }
    }
}