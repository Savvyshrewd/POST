﻿using POST.CORE.DTO;
using Newtonsoft.Json;

namespace POST.Core.DTO.Wallet
{
    public class TransactionResult : BaseDomainDTO
    {
        public bool status { get; set; }
        public string message { get; set; }
        public Data data { get; set; }
    }

    public class Data
    {
        [JsonProperty("authorization_url")]
        public string authorization_url { get; set; }

        [JsonProperty("access_code")]
        public string access_code { get; set; }

        [JsonProperty("reference")]
        public string reference { get; set; }
    }

}
