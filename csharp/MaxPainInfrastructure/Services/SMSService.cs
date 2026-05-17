using IO.ClickSend.ClickSend.Api;
using IO.ClickSend.ClickSend.Model;
using IO.ClickSend.Client;
using MaxPainInfrastructure.Code;
using Telegram.Bot;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace MaxPainInfrastructure.Services
{
    public class SMSService : ISMSService
    {
        private readonly ISecretService _secretService;

        public SMSService(ISecretService secretService)
        {
            _secretService = secretService;
        }

        public async Task<string> SendTelegram(string content)
        {
            string chatId = await _secretService.GetValue("TelegramChatId");
            return await SendTelegram(chatId, content);
        }

        public async Task<string> SendTelegram(string chatId, string content)
        {
            string token = await _secretService.GetValue("TelegramBotToken");
            var bot = new TelegramBotClient(token);
            var message = await bot.SendMessage(chatId, content);
            return DBHelper.Serialize(message);
        }

        // Kept for reference — routes through Telegram instead of Twilio
        public async Task<string> SendWhatsapp(string content)
        {
            return await SendTelegram(content);
        }

        public async Task<string> SendWhatsapp(string phone, string content)
        {
            return await SendTelegram(content);
        }

        public async Task<string> SendTextMessage(string content)
        {
            return await SendTextMessage("4043086715", content);
        }

        public async Task<string> SendTextMessage(string phone, string content)
        {
            // Replace with your ClickSend API username and API key
            string username = await _secretService.GetValue("ClickSendUsername");
            string apiKey = await _secretService.GetValue("ClickSendAPIKey");

            // Configure the ClickSend API client with your credentials
            var configuration = new Configuration()
            {
                Username = username,
                Password = apiKey
            };

            // Create an instance of the SMS API
            var smsApi = new SMSApi(configuration);

            try
            {
                // Create an SMS message object
                var smsMessage = new SmsMessage(
                    to: $"+1{phone}",
                    body: content
                );

                // Create a collection of SMS messages (even for a single message)
                var smsCollection = new SmsMessageCollection(
                    messages: new List<SmsMessage> { smsMessage }
                );

                // Send the SMS message(s)
                var result = await smsApi.SmsSendPostAsync(smsCollection);

                // Serialize the result to JSON using JsonConvert
                return DBHelper.Serialize(result);
            }
            catch
            {
                throw;
            }
        }
    }
}