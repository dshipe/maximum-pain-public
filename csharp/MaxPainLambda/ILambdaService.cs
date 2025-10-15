using Amazon.Lambda.APIGatewayEvents;

namespace MaxPainLambda
{
    public interface ILambdaService
    {
        public Task<APIGatewayHttpApiV2ProxyResponse> HandleRequest(APIGatewayHttpApiV2ProxyRequest request);
        public string BuildLogMessage(APIGatewayHttpApiV2ProxyRequest request, bool isSummary = false);
        public APIGatewayHttpApiV2ProxyResponse ReturnError(int statusCode, string errMsg);
        public Task<string> MessageGet(string? param);
    }
}
