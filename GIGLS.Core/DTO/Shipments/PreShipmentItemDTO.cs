﻿using POST.Core.Enums;
using POST.CORE.DTO;

namespace POST.Core.DTO.Shipments
{
    public class PreShipmentItemDTO : BaseDomainDTO
    {
        public int PreShipmentItemId { get; set; }
        public string Description { get; set; }
        public string Description_s { get; set; }
        public ShipmentType ShipmentType { get; set; }
        public double Weight { get; set; }
        public string Nature { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int SerialNumber { get; set; }
        public int? SpecialPackageId { get; set; }

        //To handle volumetric weight
        public bool IsVolumetric { get; set; }
        public double Length { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public decimal ItemValue { get; set; }

        //Foreign key information
        public int PreShipmentId { get; set; }

        //Agility Calculations
        public decimal CalculatedPrice { get; set; }


    }
}
