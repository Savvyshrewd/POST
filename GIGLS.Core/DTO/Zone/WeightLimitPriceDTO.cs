﻿using GIGLS.CORE.DTO;

namespace GIGLS.Core.DTO.Zone
{
    public class WeightLimitPriceDTO : BaseDomainDTO
    {
        public int WeightLimitPriceId { get; set; }
        
        public int ZoneId { get; set; }

        public string ZoneName { get; set; }

        public decimal Price { get; set; }

        public decimal Weight { get; set; }
    }
}
