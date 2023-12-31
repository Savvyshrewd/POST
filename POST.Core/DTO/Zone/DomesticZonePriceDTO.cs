﻿using POST.Core.DTO.User;
using POST.Core.Enums;
using POST.CORE.DTO;
using System.Collections.Generic;

namespace POST.Core.DTO.Zone
{
    public class DomesticZonePriceDTO : BaseDomainDTO
    {
        public DomesticZonePriceDTO()
        {
            UserDetail = new List<UserDTO>();
        }
        public int DomesticZonePriceId { get; set; }
        public decimal Weight { get; set; }
        public int ZoneId { get; set; }
        public string ZoneName { get; set; }
        public decimal Price { get; set; }
        public RegularEcommerceType RegularEcommerceType { get; set; }

        public string UserId { get; set; }
        public string UserName { get; set; }

        public List<UserDTO> UserDetail { get; set; }

        public int CountryId { get; set; }
        public CountryDTO Country { get; set; }
    }
}
