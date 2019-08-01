﻿namespace GIGLS.Core.Domain
{
    public class Country : BaseDomain, IAuditable
    {
        public int CountryId { get; set; }
        public string CountryName { get; set; }
        public string CountryCode { get; set; }
        public string CurrencySymbol { get; set; }
        public string CurrencyCode { get; set; }
        public decimal CurrencyRatio { get; set; }
        public bool IsActive { get; set; }
    }
}
