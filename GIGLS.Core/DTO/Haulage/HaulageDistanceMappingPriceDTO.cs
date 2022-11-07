﻿using POST.CORE.DTO;

namespace POST.Core.DTO.Haulage
{
    public class HaulageDistanceMappingPriceDTO : BaseDomainDTO
    {
        public int HaulageDistanceMappingPriceId { get; set; }

        public int StartRange { get; set; }
        public int EndRange { get; set; }

        public int HaulageId { get; set; }
        public HaulageDTO Haulage { get; set; }

        public decimal Price { get; set; }

        public int CountryId { get; set; }
    }
}
