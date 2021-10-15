﻿using GIGLS.Core.Enums;
using System.Collections.Generic;

namespace GIGLS.Core.DTO.Shipments
{
    public class MobilePriceDTO
    {
        public decimal? DeliveryPrice { get; set; }
        public decimal? InsuranceValue { get; set; }
        public decimal? Vat { get; set; }
        public decimal? GrandTotal { get; set; }
        public decimal? Discount { get; set; }
        public PreShipmentMobileDTO PreshipmentMobile { get; set; }
        public List<PreShipmentMobileDTO> PreshipmentMobileList { get; set; }

        public decimal? MainCharge { get; set; }

        public decimal? PickUpCharge { get; set; }

        public string CurrencySymbol { get; set; }

        public string CurrencyCode { get; set; }
        public bool? IsWithinProcessingTime { get; set; }
        public decimal? InternationalShippingCost { get; set; }
    }

    public class MultipleMobilePriceDTO
    {
        public decimal? InsuranceValue { get; set; }
        public decimal? GrandTotal { get; set; }
        public decimal? Discount { get; set; }
        public decimal? MainCharge { get; set; }
        public decimal? PickUpCharge { get; set; }
        public string CurrencySymbol { get; set; }
        public string CurrencyCode { get; set; }
        public List<MobilePricePerItemDTO> itemPriceDetails { get; set; }
        public bool IsWithinProcessingTime { get; set; }
    }
    public class MobilePricePerItemDTO
    {
        public decimal ItemWeight { get; set; }
        public string ItemName { get; set; }
        public string ItemDescription { get; set; }
        public int ItemQuantity { get; set; }
        public ShipmentType ItemShipmentType { get; set; }
        public decimal? ItemCalculatedPrice { get; set; }
        public string ItemRecever { get; set; }
    }
    

}
