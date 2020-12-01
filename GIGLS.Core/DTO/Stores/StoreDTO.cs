﻿using GIGLS.CORE.DTO;
using System;

namespace GIGLS.Core.DTO.Stores
{
    public class StoreDTO : BaseDomainDTO
    {
        public int StoreId { get; set; }
        public string StoreName { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string URL { get; set; }
        public int CountryId { get; set; }
    }
}
