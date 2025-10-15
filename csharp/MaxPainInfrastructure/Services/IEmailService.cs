using MaxPainInfrastructure.Models;
using System.Xml;


namespace MaxPainInfrastructure.Services
{
    public interface IEmailService
    {
        #region Standard Email
        public Task<string> SendEmail(string from, string to, string cc, string bcc, string subject, string body, string attachment);

        public Task<string> SendEmail(string from, string to, string cc, string bcc, string subject, string body, string attachment, bool isHtml);
        #endregion

        #region Email List
        public Task<bool> Subscribe(string name, string email);

        public Task<bool> Confirm(string name, string email);

        public Task<bool> Unsubscribe(string name, string email);
        #endregion

        #region Daily Screener
        public Task<bool> SendBulkEmail(string subject, string content, bool debugMode);

        public Task<string> ScreenerGenerate(bool runNow, bool useShortUrls, string message, bool debug);

        public Task<XmlDocument> GetScreenerXml(List<MostActive> actives, List<OutsideOIWalls> walls, List<Mx> pains, string imageTicker, byte[] buffer);

        public Task<string> GetScreenerHtml(XmlDocument xmlDom, string xslContent, bool useShortUrls);

        public Task<byte[]> GetEmailImage(string imageTicker);
        #endregion
    }
}
