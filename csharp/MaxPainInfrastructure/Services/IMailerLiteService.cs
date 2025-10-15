namespace MaxPainInfrastructure.Services
{
    public interface IMailerLiteService
    {
        string APIKey { get; set; }
        bool DebugMode { get; set; }
        string FromEmail { get; set; }
        string FromName { get; set; }
        string GroupID { get; set; }
        string GroupIDDebug { get; set; }
        string GroupIDMain { get; set; }

        Task<bool> SendEmail(string subject, string html);
        Task<bool> Subscribe(string emailAddr, string name);
        Task<bool> Unsubscribe(string emailAddr);
    }
}