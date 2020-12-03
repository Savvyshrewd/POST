﻿using GIGLS.Core.Enums;
using GIGLS.CORE.DTO;

namespace GIGLS.Core.DTO.Shipments
{
    public class ShipmentItemDTO : BaseDomainDTO
    {
        public int ShipmentItemId { get; set; }
        public string Description { get; set; }
        public string Description_s { get; set; }
        public ShipmentType ShipmentType { get; set; }
        public double Weight { get; set; }
        public string Nature { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int SerialNumber { get; set; }
        public int ShipmentPackagePriceId { get; set; }
        public int PackageQuantity { get; set; }

        //To handle volumetric weight
        public bool IsVolumetric { get; set; }
        public double Length { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        //Foreign key information
        public int ShipmentId { get; set; }
        public int? SpecialPackageId { get; set; }

    }

    public class IntlShipmentRequestItemDTO : BaseDomainDTO
    {
        public int IntlShipmentRequestItemId { get; set; }
        public string Description { get; set; }
        public string ItemName { get; set; }
        public string TrackingId { get; set; }
        public string storeName { get; set; }
        public ShipmentType ShipmentType { get; set; }
        public double Weight { get; set; }
        public string Nature { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int SerialNumber { get; set; }

        //To handle volumetric weight
        public bool IsVolumetric { get; set; }
        public double Length { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        //Foreign key information
        public int IntlShipmentRequestId { get; set; }

        public bool RequiresInsurance { get; set; } 
        public dynamic ItemValue { get; set; }
    }
}
