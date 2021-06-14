﻿using GIGLS.CORE.DTO;
using System.Collections.Generic;

namespace GIGLS.Core.DTO
{
    public class CountryDTO : BaseDomainDTO
    {
        public CountryDTO()
        {
            States = new List<StateDTO>();
        }
        public int CountryId { get; set; }
        public string CountryName { get; set; }
        public string CountryCode { get; set; }

        public List<StateDTO> States { get; set; }

        public string CurrencySymbol { get; set; }
        public string CurrencyCode { get; set; }
        public decimal CurrencyRatio { get; set; }
        public bool IsActive { get; set; }
        public string PhoneNumberCode { get; set; }
        public string ContactNumber { get; set; }
        public string ContactEmail { get; set; }
        public string TermAndConditionAmount { get; set; }
        public string TermAndConditionCountry { get; set; }
        public string CountryFlag { get; set; }
        public bool IsInternationalShippingCountry { get; set; }
        public string CourierEnable { get; set; }
        public List<string> CourierList { get; set; }
        public string CountryShortCode { get; set; }

    }
    public class NewCountryDTO 
    {
        
        public int CountryId { get; set; }
        public string CountryName { get; set; }
        public bool IsActive { get; set; }
        
    }

    public class CountryByStationDTO 
    {
        public string CurrencySymbol { get; set; }
        public string CurrencyCode { get; set; }
        public int StationId { get; set; }
    }
}