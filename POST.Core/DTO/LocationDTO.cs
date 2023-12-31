﻿using POST.CORE.DTO;

namespace POST.Core.DTO
{
    public class LocationDTO : BaseDomainDTO
    {
        public int LocationId { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        public double OriginLatitude { get; set; }
        public double OriginLongitude { get; set; }

        public double DestinationLatitude { get; set; }
        public double DestinationLongitude { get; set; }

        public string Name { get; set; }
        public string FormattedAddress { get; set; }
        public string LGA { get; set; }
    }


    public class GoogleAddressDTO
    {
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public string FormattedAddress { get; set; }
        public string Address { get; set; }
        public string Locality { get; set; }
    }
}
