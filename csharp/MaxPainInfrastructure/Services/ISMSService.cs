namespace MaxPainInfrastructure.Services
{
    public interface ISMSService
    {
        public Task<string> SendTextMessage(string content);
        public Task<string> SendTextMessage(string phone, string content);
        public Task<string> SendWhatsapp(string content);
        public Task<string> SendWhatsapp(string phone, string content);
    }
}
