﻿using POST.CORE.DTO;

namespace POST.Core.DTO.Zone
{
    public class DeliveryOptionPriceDTO : BaseDomainDTO
    {
        public int DeliveryOptionPriceId { get; set; }

        public int ZoneId { get; set; }
        public string ZoneName { get; set; }

        public int DeliveryOptionId { get; set; }
        public string DeliveryOption { get; set; }

        public decimal Price { get; set; }

        public int CountryId { get; set; }

        public CountryDTO CountryDTO { get; set; }
    }
}