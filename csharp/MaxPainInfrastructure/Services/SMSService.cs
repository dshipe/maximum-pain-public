using IO.ClickSend.ClickSend.Api;
using IO.ClickSend.ClickSend.Model;
using IO.ClickSend.Client;
using MaxPainInfrastructure.Code;
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

        public async Task<string> SendWhatsapp(string content)
        {
            return await SendWhatsapp("4043086715", content);
        }

        public async Task<string> SendWhatsapp(string phone, string content)
        {
            string authToken = await _secretService.GetValue("TwilioAuthToken");
            string accountSid = await _secretService.GetValue("TwilioAccountSid");

            TwilioClient.Init(accountSid, authToken);

            var messageOptions = new CreateMessageOptions(
              new PhoneNumber($"whatsapp:+1{phone}"));

            messageOptions.From = new PhoneNumber("whatsapp:+14155238886");
            messageOptions.Body = content;

            var message = MessageResource.Create(messageOptions);
            return DBHelper.Serialize(message);
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
            catch (Exception e)
            {
                throw;
            }
        }
    }
}