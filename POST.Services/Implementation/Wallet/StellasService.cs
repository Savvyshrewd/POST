﻿using AutoMapper;
using POST.Core;
using POST.Core.Domain;
using POST.Core.Domain.Utility;
using POST.Core.Domain.Wallet;
using POST.Core.DTO;
using POST.Core.DTO.Customers;
using POST.Core.DTO.Partnership;
using POST.Core.DTO.Wallet;
using POST.Core.Enums;
using POST.Core.IServices.User;
using POST.Core.IServices.Utility;
using POST.Core.IServices.Wallet;
using POST.CORE.Enums;
using POST.Infrastructure;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace POST.Services.Implementation.Wallet
{
    public class StellasService : IStellasService
    {
        private readonly IUserService _userService;
        private readonly IUnitOfWork _uow;
        private readonly IGlobalPropertyService _globalPropertyService;
        private static string _auth = System.Web.HttpContext.Current.Server.MapPath("~");
        private static readonly HttpClient client = new HttpClient();

        public StellasService(IUserService userService, IUnitOfWork uow, IGlobalPropertyService globalPropertyService)
        {
            _userService = userService;
            _uow = uow;
            _globalPropertyService = globalPropertyService;
            MapperConfig.Initialize();
        }

        public async Task<StellasResponseDTO> CreateStellasAccount(CreateStellaAccountDTO createStellaAccountDTO)
        {
            string secretKey = ConfigurationManager.AppSettings["StellasSecretKey"];
            string url = ConfigurationManager.AppSettings["StellasCreateAccount"];
            string bizId = ConfigurationManager.AppSettings["BusinessID"];
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            var result = new StellasResponseDTO();
            string authorization = await GetToken();
            if (String.IsNullOrEmpty(authorization))
            {
                var auth = await Authenticate();
                if (auth.Key)
                {
                    authorization = await GetToken();
                }
                else
                {
                    throw new GenericException(auth.ToString());
                }
            }
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("SECRET_KEY", secretKey);
                client.DefaultRequestHeaders.Add("businessId", bizId);
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authorization}");
                var json = JsonConvert.SerializeObject(createStellaAccountDTO);
                StringContent data = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, data);
                string message = await response.Content.ReadAsStringAsync();
                if (message.Contains("Session Expired! Please login again"))
                {
                    var retry = await Retry(url,"post", data);
                    if (retry.ContainsKey(true))
                    {
                        var res = JsonConvert.DeserializeObject<CreateStellaAccounResponsetDTO>(retry.FirstOrDefault().Value);
                        result.data = res;
                        result.message = res.message;
                        result.status = res.status;
                        return result;
                    }
                    else
                    {
                        var res = JsonConvert.DeserializeObject<StellasErrorResponse>(retry.FirstOrDefault().Value);
                        result.errors.AddRange(res.Errors);
                        result.message = "an error occured";
                        result.status = false;
                        return result;
                    }
                }
                else if (response.IsSuccessStatusCode)
                {
                    var res = JsonConvert.DeserializeObject<CreateStellaAccounResponsetDTO>(message);
                    result.data = res;
                    result.message = res.message;
                    result.status = res.status;
                    return result;
                }
                else
                {
                    var res = JsonConvert.DeserializeObject<StellasErrorResponse>(message);
                    result.errors.AddRange(res.Errors);
                    result.message = "an error occured";
                    result.status = false;
                    return result;
                }
            }
            return result;
        }

        public async Task<StellasResponseDTO> GetCustomerStellasAccount(string accountNo)
        {
            string secretKey = ConfigurationManager.AppSettings["StellasSecretKey"];
            string url = ConfigurationManager.AppSettings["StellasCreateAccount"];
            string bizId = ConfigurationManager.AppSettings["BusinessID"];
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            var result = new StellasResponseDTO();
            string authorization = await GetToken();
            if (String.IsNullOrEmpty(authorization))
            {
                var auth = await Authenticate();
                if (auth.Key)
                {
                    authorization = await GetToken();
                }
                else
                {
                    throw new GenericException(auth.ToString());
                }
            }
            url = $"{url}balance/{accountNo}";
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            var client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("SECRET_KEY", secretKey);
            client.DefaultRequestHeaders.Add("businessId", bizId);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authorization}");
            var response = await client.GetAsync(url);
            string responseResult = await response.Content.ReadAsStringAsync();
            if (responseResult.Contains("Session Expired! Please login again"))
            {
                var retry = await Retry(url, "get", null);
                if (retry.ContainsKey(true))
                {
                    var res = JsonConvert.DeserializeObject<GetCustomerBalanceDTO>(retry.FirstOrDefault().Value);
                    result.data = res;
                    result.message = res.message;
                    result.status = res.status;
                    return result;
                }
                else
                {
                    var res = JsonConvert.DeserializeObject<StellasErrorResponse>(retry.FirstOrDefault().Value);
                    result.errors.AddRange(res.Errors);
                    result.message = "an error occured";
                    result.status = false;
                    return result;
                }
            }
            else if (response.IsSuccessStatusCode)
            {
                var res = JsonConvert.DeserializeObject<GetCustomerBalanceDTO>(responseResult);
                result.data = res.data;
                result.message = res.message;
                result.status = res.status;
                return result;
            }
            else
            {
                var res = JsonConvert.DeserializeObject<StellasErrorResponse>(responseResult);
                result.errors.AddRange(res.Errors);
                result.message = "an error occured";
                result.status = false;
                return result;
            }
            return result;
        }



        private static async Task<KeyValuePair<bool, string>> Authenticate()
        {
            using (var client = new HttpClient())
            {
                var authModel = new AuthModel();
                string username = ConfigurationManager.AppSettings["StellasUsername"];
                string pwd = ConfigurationManager.AppSettings["StellasPwd"];
                authModel.email = username;
                authModel.password = pwd;
                var credentials = JToken.FromObject(authModel);
                var data = JsonConvert.SerializeObject(credentials);
                KeyValuePair<string, string>[] nameValueCollection = new KeyValuePair<string, string>[] { new KeyValuePair<string, string>("grant_type", "password"), new KeyValuePair<string, string>("email", authModel.email), new KeyValuePair<string, string>("password", authModel.password) };
                FormUrlEncodedContent content = new FormUrlEncodedContent(nameValueCollection);
                string authUrl = ConfigurationManager.AppSettings["StellasAuth"];

                var path = _auth + @"\Auth";
                var response = await client.PostAsync(authUrl, content);
                try
                {
                    if (response.IsSuccessStatusCode)
                    {
                        var result = JToken.Parse(await response.Content.ReadAsStringAsync());
                        // save the result in a file
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                        using (StreamWriter sw = new StreamWriter($"{path}\\config.txt"))
                        {
                            sw.Write(result);
                        }
                        return new KeyValuePair<bool, string>(true, result.ToString());
                    }
                }
                catch (Exception exp)
                {
                    return new KeyValuePair<bool, string>(false, exp.Message);
                }
                var ex = await response.Content.ReadAsStringAsync();
                return new KeyValuePair<bool, string>(false, ex); 
            }
        }

        private static async Task<string> GetToken()
        {
            try
            {
                var loc = _auth + @"\Auth";
                string path = $"{loc}\\config.txt";
                if (File.Exists(path))
                {
                    using (StreamReader sr = new StreamReader(path))
                    {
                        var obj = await sr.ReadToEndAsync().ContinueWith(t => JObject.Parse(t.Result));
                        return (string)obj["data"]["accessToken"];
                    }
                }
            }
            catch (Exception ex)
            {
                return String.Empty;
            }
            return String.Empty;
        }

        private async Task<Dictionary<bool,string>> Retry(string url, string action, StringContent data)
        {
            string secretKey = ConfigurationManager.AppSettings["StellasSecretKey"];
            string bizId = ConfigurationManager.AppSettings["BusinessID"];
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            var res = new Dictionary<bool, string>();
            var token = String.Empty;
            var auth = await Authenticate();
            var authorization = await GetToken();
            
            
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("SECRET_KEY", secretKey);
            client.DefaultRequestHeaders.Add("businessId", bizId);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authorization}");

            if (action == "get")
            {
                var response = await client.GetAsync(url);
                if (response.IsSuccessStatusCode)
                {
                    string retrialResponseResult = await response.Content.ReadAsStringAsync();
                    res.Add(true, retrialResponseResult);
                    return res;
                }
                else
                {
                    string retrialResponseResult = await response.Content.ReadAsStringAsync();
                    res.Add(false, retrialResponseResult);
                    return res;
                }
            }
            else if (action == "post")
            {
                var response = await client.PostAsync(url, data);
                if (response.IsSuccessStatusCode)
                {
                    string retrialResponseResult = await response.Content.ReadAsStringAsync();
                    res.Add(true, retrialResponseResult);
                    return res;
                }
                else
                {
                    string retrialResponseResult = await response.Content.ReadAsStringAsync();
                    res.Add(false, retrialResponseResult);
                    return res;
                }
            }
            res.Add(false, "error occured");
            return res;
        }


        public async Task<StellasResponseDTO> GetBanks()
        {
            string secretKey = ConfigurationManager.AppSettings["StellasSecretKey"];
            string url = ConfigurationManager.AppSettings["StellasSandBox"];
            string bizId = ConfigurationManager.AppSettings["BusinessID"];
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            var result = new StellasResponseDTO();
            string authorization = await GetToken();
            if (String.IsNullOrEmpty(authorization))
            {
                var auth = await Authenticate();
                if (auth.Key)
                {
                    authorization = await GetToken();
                }
                else
                {
                    throw new GenericException(auth.ToString());
                }
            }
            url = $"{url}banks";
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("SECRET_KEY", secretKey);
            client.DefaultRequestHeaders.Add("businessId", bizId);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authorization}");
            var response = await client.GetAsync(url);
            string responseResult = await response.Content.ReadAsStringAsync();
            if (responseResult.Contains("Session Expired! Please login again"))
            {
                var retry = await Retry(url, "get", null);
                if (retry.ContainsKey(true))
                {
                    var res = JsonConvert.DeserializeObject<StellassBankResponse>(retry.FirstOrDefault().Value);
                    result.data = res.Data;
                    result.message = res.Message;
                    result.status = res.Status;
                    return result;
                }
                else
                {
                    var res = JsonConvert.DeserializeObject<StellasErrorResponse>(retry.FirstOrDefault().Value);
                    result.errors.AddRange(res.Errors);
                    result.message = "an error occured";
                    result.status = false;
                    return result;
                }
            }
            else if (response.IsSuccessStatusCode)
            {
                var res = JsonConvert.DeserializeObject<StellassBankResponse>(responseResult);
                result.data = res.Data;
                result.message = res.Message;
                result.status = res.Status;
                return result;
            }
            else
            {
                var res = JsonConvert.DeserializeObject<StellasErrorResponse>(responseResult);
                result.errors.AddRange(res.Errors);
                result.message = "an error occured";
                result.status = false;
                return result;
            }
           
            return result;
        }

        public async Task<StellasResponseDTO> StellasTransfer(StellasTransferDTO createStellaAccountDTO)
        {
            string secretKey = ConfigurationManager.AppSettings["StellasSecretKey"];
            string url = ConfigurationManager.AppSettings["StellasTransfer"];
            string bizId = ConfigurationManager.AppSettings["BusinessID"];
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            var result = new StellasResponseDTO();
            string authorization = await GetToken();
            if (String.IsNullOrEmpty(authorization))
            {
                var auth = await Authenticate();
                if (auth.Key)
                {
                    authorization = await GetToken();
                }
                else
                {
                    throw new GenericException(auth.ToString());
                }
            }
            
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("SECRET_KEY", secretKey);
            client.DefaultRequestHeaders.Add("businessId", bizId);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authorization}");
            var json = JsonConvert.SerializeObject(createStellaAccountDTO);
            StringContent data = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, data);
            string message = await response.Content.ReadAsStringAsync();
            if (message.Contains("Session Expired! Please login again"))
            {
                var retry = await Retry(url, "post", data);
                if (retry.ContainsKey(true))
                {
                    var res = JsonConvert.DeserializeObject<WithdrawAndTransferSucessDTO>(retry.FirstOrDefault().Value);
                    result.data = res;
                    result.message = res.Message;
                    result.status = res.Status;
                    return result;
                }
                else
                {
                    var res = JsonConvert.DeserializeObject<StellasErrorResponse>(retry.FirstOrDefault().Value);
                    result.errors.AddRange(res.Errors);
                    result.message = "an error occured";
                    result.status = false;
                    return result;
                }

            }
            else
            {
                if (response.IsSuccessStatusCode)
                {
                    var res = JsonConvert.DeserializeObject<WithdrawAndTransferSucessDTO>(message);
                    result.data = res;
                    result.message = res.Message;
                    result.status = res.Status;
                    return result;
                }
                else
                {
                    var res = JsonConvert.DeserializeObject<StellasErrorResponse>(message);
                    result.errors.AddRange(res.Errors);
                    result.message = "an error occured";
                    result.status = false;
                    return result;
                }
            }
            return result;
        }


        public async Task<StellasResponseDTO> StellasValidateBankName(ValidateBankNameDTO validateBankNameDTO)
        {
            string secretKey = ConfigurationManager.AppSettings["StellasSecretKey"];
            string url = ConfigurationManager.AppSettings["StellasCreateAccount"];
            string bizId = ConfigurationManager.AppSettings["BusinessID"];
            url = $"{url}name-enquiry";
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            var result = new StellasResponseDTO();
            string authorization = await GetToken();
            if (String.IsNullOrEmpty(authorization))
            {
                var auth = await Authenticate();
                if (auth.Key)
                {
                    authorization = await GetToken();
                }
                else
                {
                    throw new GenericException(auth.ToString());
                }
            }

            
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("SECRET_KEY", secretKey);
            client.DefaultRequestHeaders.Add("businessId", bizId);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authorization}");
            var json = JsonConvert.SerializeObject(validateBankNameDTO);
            StringContent data = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, data);
            string message = await response.Content.ReadAsStringAsync();
            if (message.Contains("Session Expired! Please login again"))
            {
                var retry = await Retry(url, "post", data);
                if (retry.ContainsKey(true))
                {
                    var res = JsonConvert.DeserializeObject<ValidateBankNameResponse>(retry.FirstOrDefault().Value);
                    result.data = res.Data;
                    result.message = res.Message;
                    result.status = res.Status;
                    return result;
                }
                else
                {
                    var res = JsonConvert.DeserializeObject<StellasErrorResponse>(retry.FirstOrDefault().Value);
                    result.errors.AddRange(res.Errors);
                    result.message = "an error occured";
                    result.status = false;
                    return result;
                }
            }
            else
            {
                if (response.IsSuccessStatusCode)
                {
                    var res = JsonConvert.DeserializeObject<ValidateBankNameResponse>(message);
                    result.data = res.Data;
                    result.message = res.Message;
                    result.status = res.Status;
                    return result;
                }
                else
                {
                    var res = JsonConvert.DeserializeObject<StellasErrorResponse>(message);
                    result.errors.AddRange(res.Errors);
                    result.message = "an error occured";
                    result.status = false;
                    return result;
                }
            }
            return result;
        }


        public async Task<StellasResponseDTO> StellasWithdrawal(StellasWithdrawalDTO createStellaAccountDTO)
        {
            string secretKey = ConfigurationManager.AppSettings["StellasSecretKey"];
            string url = ConfigurationManager.AppSettings["StellasWithdraw"];
            string bizId = ConfigurationManager.AppSettings["BusinessID"];
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            var result = new StellasResponseDTO();
            string authorization = await GetToken();
            if (String.IsNullOrEmpty(authorization))
            {
                var auth = await Authenticate();
                if (auth.Key)
                {
                    authorization = await GetToken();
                }
                else
                {
                    throw new GenericException(auth.ToString());
                }
            }
            
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("SECRET_KEY", secretKey);
            client.DefaultRequestHeaders.Add("businessId", bizId);
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authorization}");
            var json = JsonConvert.SerializeObject(createStellaAccountDTO);
            StringContent data = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await client.PostAsync(url, data);
            string message = await response.Content.ReadAsStringAsync();
            if (message.Contains("Session Expired! Please login again"))
            {
                var retry = await Retry(url, "post", data);
                if (retry.ContainsKey(true))
                {
                    var res = JsonConvert.DeserializeObject<WithdrawAndTransferSucessDTO>(retry.FirstOrDefault().Value);
                    result.data = res.Data;
                    result.message = res.Message;
                    result.status = res.Status;
                    return result;
                }
                else
                {
                    var res = JsonConvert.DeserializeObject<StellasErrorResponse>(retry.FirstOrDefault().Value);
                    result.errors.AddRange(res.Errors);
                    result.message = "an error occured";
                    result.status = false;
                    return result;
                }
            }
            else
            {
                if (response.IsSuccessStatusCode)
                {
                    var res = JsonConvert.DeserializeObject<WithdrawAndTransferSucessDTO>(message);
                    result.data = res.Data;
                    result.message = res.Message;
                    result.status = res.Status;
                    return result;
                }
                else
                {
                    var res = JsonConvert.DeserializeObject<StellasErrorResponse>(message);
                    result.errors.AddRange(res.Errors);
                    result.message = "an error occured";
                    result.status = false;
                    return result;
                }
            }
            return result;
        }

        private class AuthModel
        {
            public string email { get; set; } 
            public string password { get; set; }

        }

        public async Task<StellasResponseDTO> ValidateBVNNumber(ValidateCustomerBVN payload)
        {
            string secretKey = ConfigurationManager.AppSettings["StellasSecretKey"];
            string url = ConfigurationManager.AppSettings["StellasSandBox"];
            string validatebvn = ConfigurationManager.AppSettings["StellasValidateBVN"];
            string bizId = ConfigurationManager.AppSettings["BusinessID"];
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            string authorization = await GetToken();
            var result = new StellasResponseDTO();
            if (String.IsNullOrEmpty(authorization))
            {
                var auth = await Authenticate();
                if (auth.Key)
                {
                    authorization = await GetToken();
                }
                else
                {
                    throw new GenericException(auth.ToString());
                }
            }
            url = $"{url}{validatebvn}";
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("SECRET_KEY", secretKey);
                client.DefaultRequestHeaders.Add("businessId", bizId);
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authorization}");
                var json = JsonConvert.SerializeObject(payload);
                StringContent data = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, data);
                string responseResult = await response.Content.ReadAsStringAsync();
                if (responseResult.Contains("Session Expired! Please login again"))
                {
                    var retry = await Retry(url, "post", data);
                    if (retry.ContainsKey(true))
                    {
                        var res = JsonConvert.DeserializeObject<ValidateBVNResponseDTO>(retry.FirstOrDefault().Value);
                        result.data = res.Data;
                        result.message = res.Message;
                        result.status = res.Status;
                        return result;
                    }
                    else
                    {
                        var res = JsonConvert.DeserializeObject<StellasErrorResponse>(retry.FirstOrDefault().Value);
                        result.errors.AddRange(res.Errors);
                        result.message = "an error occured";
                        result.status = false;
                        return result;
                    }
                }
                else if (response.IsSuccessStatusCode)
                {
                    var res = JsonConvert.DeserializeObject<ValidateBVNResponseDTO>(responseResult);
                    result.data = res.Data;
                    result.message = res.Message;
                    result.status = res.Status;
                    return result;
                }
                else
                {
                    var res = JsonConvert.DeserializeObject<StellasErrorResponse>(responseResult);
                    result.errors.AddRange(res.Errors);
                    result.message = "an error occured";
                    result.status = false;
                    return result;
                }
            }
        }

        public async Task<StellasResponseDTO> CreateAccountOnCoreBanking(CreateAccountCoreBankingDTO payload)
        {
            string secretKey = ConfigurationManager.AppSettings["StellasSecretKey"];
            string url = ConfigurationManager.AppSettings["StellasSandBox"];
            string coreBanking = ConfigurationManager.AppSettings["StellasCoreBanking"];
            string bizId = ConfigurationManager.AppSettings["BusinessID"];
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            string authorization = await GetToken();
            var result = new StellasResponseDTO();
            if (String.IsNullOrEmpty(authorization))
            {
                var auth = await Authenticate();
                if (auth.Key)
                {
                    authorization = await GetToken();
                }
                else
                {
                    throw new GenericException(auth.ToString());
                }
            }
            url = $"{url}{coreBanking}";
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            using (var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.DefaultRequestHeaders.Add("SECRET_KEY", secretKey);
                client.DefaultRequestHeaders.Add("businessId", bizId);
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {authorization}");
                var json = JsonConvert.SerializeObject(payload);
                StringContent data = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await client.PostAsync(url, data);
                string responseResult = await response.Content.ReadAsStringAsync();
                if (responseResult.Contains("Session Expired! Please login again"))
                {
                    var retry = await Retry(url, "post", data);
                    if (retry.ContainsKey(true))
                    {
                        var res = JsonConvert.DeserializeObject<CreateAccountCoreBankingResponseDTO>(retry.FirstOrDefault().Value);
                        result.data = res;
                        result.message = res.Message;
                        result.status = res.Status;
                        return result;
                    }
                    else
                    {
                        var res = JsonConvert.DeserializeObject<StellasErrorResponse>(retry.FirstOrDefault().Value);
                        result.errors.AddRange(res.Errors);
                        result.message = "an error occured";
                        result.status = false;
                        return result;
                    }
                }
                else if (response.IsSuccessStatusCode)
                {
                    var res = JsonConvert.DeserializeObject<CreateAccountCoreBankingResponseDTO>(responseResult);
                    result.data = res;
                    result.message = res.Message;
                    result.status = res.Status;
                    return result;
                }
                else
                {
                    var res = JsonConvert.DeserializeObject<StellasErrorResponse>(responseResult);
                    result.errors.AddRange(res.Errors);
                    result.message = "an error occured";
                    result.status = false;
                    return result;
                }
            }
        }
    }
}