﻿using POST.Core.Enums;
using POST.CORE.DTO;
using System.Collections.Generic;

namespace POST.Core.DTO.PaymentTransactions
{
    public class PricingDTO : BaseDomainDTO
    {
        public int DepartureServiceCentreId { get; set; }
        public int DestinationServiceCentreId { get; set; }
        public int DeliveryOptionId { get; set; }
        public List<int> DeliveryOptionIds { get; set; }
        public decimal Weight { get; set; }
        public ShipmentType ShipmentType { get; set; }
        public int SpecialPackageId { get; set; }
        public bool IsInternationalDocument { get; set; }
        
        //To handle volumetric weight
        public bool IsVolumetric { get; set; }
        public decimal Length { get; set; } = 0;
        public decimal Width { get; set; } = 0;
        public decimal Height { get; set; } = 0;

        //Added for ThirdParty API
        public int DepartureStationId { get; set; }
        public int DestinationStationId { get; set; }

        public int CountryId { get; set; }
        public string CustomerCode { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
    }


    public class UKPricingDTO : BaseDomainDTO
    {
        public int DepartureServiceCentreId { get; set; }
        public int DestinationServiceCentreId { get; set; }
        public int DeliveryOptionId { get; set; }
        public List<int> DeliveryOptionIds { get; set; }
        public decimal Weight { get; set; }
        public ShipmentType ShipmentType { get; set; }
        public int SpecialPackageId { get; set; }
        public bool IsInternationalDocument { get; set; }

        //To handle volumetric weight
        public bool IsVolumetric { get; set; }
        public decimal Length { get; set; } = 0;
        public decimal Width { get; set; } = 0;
        public decimal Height { get; set; } = 0;

        //Added for ThirdParty API
        public int DepartureStationId { get; set; }
        public int DestinationStationId { get; set; }

        public int CountryId { get; set; }
        public string CustomerCode { get; set; }
        public GlobalPropertyType ItemCategory { get; set; }
        public int PriceCategoryId { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public int DepartureCountryId { get; set; }
        public DeliveryType DeliveryType { get; set; }
        public string PriceCategoryName { get; set; }
    }

    public class NewPricingDTO
    {
      
        //To handle volumetric weight
        public decimal Total { get; set; } = 0;
        public decimal GrandTotal { get; set; } = 0;
        public decimal Vat { get; set; } = 0;
        public decimal Insurance { get; set; } = 0;
        public decimal DiscountedValue { get; set; } = 0;
    }
}
