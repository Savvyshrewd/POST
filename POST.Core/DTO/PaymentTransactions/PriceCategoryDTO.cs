﻿using POST.Core.Enums;
using System.Collections.Generic;

namespace POST.Core.DTO.PaymentTransactions
{
    public class PriceCategoryDTO
    {
        public PriceCategoryDTO()
        {
            DeliveryType = DeliveryType.GOSTANDARDED;
        }
        public int PriceCategoryId { get; set; }
        public int CountryId { get; set; }
        public string CountryName { get; set; }
        public string PriceCategoryName { get; set; }
        public decimal CategoryMinimumWeight { get; set; }
        public decimal PricePerWeight { get; set; }
        public decimal CategoryMinimumPrice { get; set; }
        public bool IsActive { get; set; }
        public int DepartureCountryId { get; set; }
        public string DepartureCountryName { get; set; }
        public decimal SubminimumWeight { get; set; }
        public decimal SubminimumPrice { get; set; }
        public bool IsHazardous { get; set; }
        public DeliveryType DeliveryType { get; set; }
    }

    public class QuickQuotePriceDTO
    {
        public List<int> PriceCategoryId { get; set; }
        public int DestinationCountryId { get; set; }
        public int DepartureCountryId { get; set; }
        public int Quantity { get; set; }
        public decimal Weight { get; set; }
        public string CurrencySymbol { get; set; }
        public string CurrencyCode { get; set; }
        public decimal Price { get; set; }
    }



}
