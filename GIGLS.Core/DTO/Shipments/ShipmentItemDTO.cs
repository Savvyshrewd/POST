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

        //To handle volumetric weight
        public bool IsVolumetric { get; set; }
        public double Length { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        //Foreign key information
        public int ShipmentId { get; set; }

    }

    public class IntlShipmentRequestItemDTO : BaseDomainDTO
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

        //To handle volumetric weight
        public bool IsVolumetric { get; set; }
        public double Length { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        //Foreign key information
        public int ShipmentId { get; set; }
        public virtual ShipmentDTO Shipment { get; set; }
    }
}
