﻿using System.Threading.Tasks;
using System.Configuration;
using POST.Core.IMessage;
using POST.Core.DTO;
using System.Net;
using System;
using System.IO;
using System.Net.Http;
using POST.Core.IServices.MessagingLog;
using POST.Core.DTO.MessagingLog;
using POST.Core.Enums;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;
using Twilio.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace POST.Messaging.MessageService
{
    public class SMSService : ISMSService
    {
        private readonly ISmsSendLogService _iSmsSendLogService;

        public SMSService(ISmsSendLogService iSmsSendLogService)
        {
            _iSmsSendLogService = iSmsSendLogService;
        }

        public async Task<string> SendAsync(MessageDTO message)
        {
            var result = "";

            if (message.To != null)
            {
                switch (message.SMSSenderPlatform)
                {
                    case SMSSenderPlatform.OGOSMS:
                        result = await SendSMSUsingOGOSMSAsync(message);
                        break;

                    case SMSSenderPlatform.TWILIO:
                        result = await SendSMSUsingTwilioAsync(message);
                        break;

                    case SMSSenderPlatform.OGOSMSBANKROUTE:
                        result = await SendSMSUsingOGOSMSBankChannelAsync(message);
                        break;

                    case SMSSenderPlatform.ROUTESMS:
                        result = await SendSMSUsingROUTESMSBankChannelAsync(message);
                        break;

                    default:
                        break;
                }
            }
                        
            return result;
        }

        // Use OGO Sms
        private async Task<string> SendSMSUsingOGOSMSAsync(MessageDTO message)
        {
            string result = "";

            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

                var smsURL = await ReturnValidUrl(message);
                var smsApiKey = ConfigurationManager.AppSettings["smsApiKey"];

                //ogosms url format
                var finalURL = $"{smsURL}&password={smsApiKey}&sender={message.From}&numbers={message.To}&message={message.FinalBody}&response=json&unicode=0";
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(finalURL);

                using (var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse())
                {
                    using (var sr = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        result = sr.ReadToEnd();
                    }
                }

                result = GetOGOSMSResponseMessage(result);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return await Task.FromResult(result);
        }

        private string GetOGOSMSResponseMessage(string reponseMessage)
        {
            var message = ConfigurationManager.AppSettings[reponseMessage];

            if (message == null)
            {
                message = reponseMessage;
            }
            return message;
        }

        private async Task<string> ReturnValidUrl(MessageDTO message)
        {
            string smsURL = ConfigurationManager.AppSettings["smsURL"];

            bool result = await IsValidUri(smsURL);

            if (!result)
            {
                await _iSmsSendLogService.AddSmsSendLog(new SmsSendLogDTO()
                {
                    From = message.From,
                    To = message.To,
                    Waybill = message.Waybill,
                    Message = message.FinalBody,
                    Status = MessagingLogStatus.Failed,
                    ResultDescription = $"NOT REACHABLE {smsURL}"
                });

                smsURL = ConfigurationManager.AppSettings["smsURLNet"];
            }

            return smsURL;
        }

        private async Task<bool> IsValidUri(string url)
        {
            try
            {
                Uri uri = new Uri(url);

                using (HttpClient client = new HttpClient())
                {
                    HttpResponseMessage result = await client.GetAsync(uri);
                    HttpStatusCode StatusCode = result.StatusCode;

                    switch (StatusCode)
                    {
                        case HttpStatusCode.Accepted:
                            return true;
                        case HttpStatusCode.OK:
                            return true;
                        default:
                            return false;
                    }
                }
            }
            catch
            {
                return false;
            }
        }

        // Use Scriptwall Sms
        private async Task<string> ConfigSendGridasyncScriptwallSMS(MessageDTO message)
        {
            string result = "";

            try
            {
                var smsURL = ConfigurationManager.AppSettings["smsURL"];
                var smsApiKey = ConfigurationManager.AppSettings["smsApiKey"];
                var smsFrom = ConfigurationManager.AppSettings["smsFrom"];

                //Scriptwall url format
                var finalURL = $"{smsURL}&api_key={smsApiKey}&to={message.To}&from={smsFrom}&sms={message.FinalBody}&response=json&unicode=0";
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(finalURL);

                using (var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse())
                {
                    result = httpResponse.StatusCode.ToString();
                }
            }
            catch (Exception ex)
            {
                // An exception occurred making the REST call
                throw ex;
            }

            return await Task.FromResult(result);
        }

        //// Use Twilio SMS
        private async Task<string> SendSMSUsingTwilioAsync(MessageDTO message)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            string accountSID = ConfigurationManager.AppSettings["smsService:accountSID"];
            string authToken = ConfigurationManager.AppSettings["smsService:authToken"];
            string fromNumber = ConfigurationManager.AppSettings["smsFrom"];

            // Initialize the TwilioClient.
            TwilioClient.Init(accountSID, authToken);

            string result;
            try
            {
                // Send an SMS message.
                var msg = MessageResource.Create(
                    to: new PhoneNumber(message.To),
                    from: new PhoneNumber(fromNumber),
                    body: message.FinalBody
                );

                result = msg.Status.ToString();
            }
            catch (TwilioException ex)
            {
                // An exception occurred making the REST call
                throw ex;
            }

            return await Task.FromResult(result);
        }

        public async Task SendVoiceMessageAsync(string phoneNumber)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            string accountSID = ConfigurationManager.AppSettings["smsService:accountSID"];
            string authToken = ConfigurationManager.AppSettings["smsService:authToken"];
            string voicePhone = ConfigurationManager.AppSettings["smsService:voicePhone"];
            string twiml = ConfigurationManager.AppSettings["smsService:voiceData"];

            TwilioClient.Init(accountSID, authToken);

            string result;

            try
            {
                // Send an SMS message.
                var call = CallResource.Create(
                    url: new Uri(twiml),
                    to: new PhoneNumber(phoneNumber),
                    from: new PhoneNumber(voicePhone)
                );

                result = call.Status.ToString();
            }
            catch (TwilioException ex)
            {
                throw ex;
            }

            await Task.FromResult(result);
        }

        // Use OGO Sms Bank Channel
        private async Task<string> SendSMSUsingOGOSMSBankChannelAsync(MessageDTO message)
        {
            string result = "";

            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                var smsURL = ConfigurationManager.AppSettings["smsURLBankChannel"];
                var smsApiKey = ConfigurationManager.AppSettings["smsApiKey"];

                //ogosms url format
                var finalURL = $"{smsURL}&password={smsApiKey}&sender={message.From}&numbers={message.To}&message={message.FinalBody}&response=json&unicode=0";
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(finalURL);

                using (var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse())
                {
                    using (var sr = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        result = sr.ReadToEnd();
                    }
                }

                result = FomatBankChannelResponseMessage(result);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return await Task.FromResult(result);
        }

        private string FomatBankChannelResponseMessage(string reponseMessage)
        {
            var response = JsonConvert.DeserializeObject<ResponseData>(reponseMessage);

            var dataJsonResult = response.Data.ToString();
            var dataJson = JToken.Parse(dataJsonResult);

            if (dataJson is JArray)
            {
                var obj = dataJson.ToObject<List<Data>>();
                response.MessageData.AddRange(obj);
            }

            if (dataJson is JObject)
            {
                var obj = dataJson.ToObject<Data>();
                response.MessageData.Add(obj);
            }

            var result = $"{response.Message} | {response.Status}";

            foreach (var data in response.MessageData)
            {
                result = $"{result} | {data.Cost} | {data.Pages} | {data.NumberOfReceiver} | {data.MessageId}";
            }

            return result;
        }

        // Use Route Sms Bank Channel
        private async Task<string> SendSMSUsingROUTESMSBankChannelAsync(MessageDTO message)
        {
            string result = "";

            try
            {
                //ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                var routeURL = ConfigurationManager.AppSettings["RouteBaseUrl"];
                var password = ConfigurationManager.AppSettings["RoutePassword"];
                var username = ConfigurationManager.AppSettings["RouteUsername"];
                var source = ConfigurationManager.AppSettings["RouteSource"];
                var type = ConfigurationManager.AppSettings["RouteType"];

                //Replace + with empty char
                if (message.To.Contains("+"))
                {
                    message.To = message.To.Replace("+", string.Empty);
                }

                var finalBodyEncoded = HttpUtility.UrlEncodeUnicode(message.FinalBody);

                //Set final Url with parameters
                var finalURL = $"{routeURL}?username={username}&password={password}&type={type}&dlr={0}&destination={message.To}&source={source}&message={finalBodyEncoded}";
                var httpWebRequest = (HttpWebRequest)WebRequest.Create(finalURL);

                using (var httpResponse = (HttpWebResponse)httpWebRequest.GetResponse())
                {
                    using (var sr = new StreamReader(httpResponse.GetResponseStream()))
                    {
                        result = sr.ReadToEnd();
                    }
                }

                //result = FomatBankChannelResponseMessage(result);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return await Task.FromResult(result);
        }
    }
}
