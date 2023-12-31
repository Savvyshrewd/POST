﻿using POST.Core.Enums;
using POST.CORE.DTO;

namespace POST.Core.DTO.Zone
{
    public class CountryRouteZoneMapDTO : BaseDomainDTO
    {
        public int CountryRouteZoneMapId { get; set; }

        public int ZoneId { get; set; }
        public virtual ZoneDTO Zone { get; set; }

        public int DepartureId { get; set; }        
        public virtual CountryDTO Departure { get; set; }

        public int DestinationId { get; set; }        
        public virtual CountryDTO Destination { get; set; }

        public double Rate { get; set; }
        public bool Status { get; set; }
        public CompanyMap CompanyMap { get; set; }
    }

    public class CountryMapQueryDTO 
    {
        public int DepartureId { get; set; }
        public int DestinationId { get; set; }
        public CompanyMap CompanyMap { get; set; }
    }

}
