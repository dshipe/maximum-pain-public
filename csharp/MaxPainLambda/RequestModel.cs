namespace MaxPainLambda
{
    public class RequestModel
    {
        public string Version { get; set; }
        public string RouteKey { get; set; }
        public string RawPath { get; set; }
        public string RawQueryString { get; set; }
        public IDictionary<string, string> Headers { get; set; }
        public Requestcontext RequestContext { get; set; }
        public string Body { get; set; }
        public IDictionary<string, string> QueryStringParameters { get; set; }
    }
    public class Requestcontext
    {
        public string AccountId { get; set; }
        public string ApiId { get; set; }
        public string DomainName { get; set; }
        public string DomainPrefix { get; set; }
        public Http Http { get; set; }
        public string RequestId { get; set; }
        public string RouteKey { get; set; }
        public string Stage { get; set; }
        public string Time { get; set; }
        public long TimeEpoch { get; set; }
    }

    public class Http
    {
        public string Method { get; set; }
        public string Path { get; set; }
        public string Protocol { get; set; }
        public string SourceIp { get; set; }
        public string UserAgent { get; set; }
    }
}
