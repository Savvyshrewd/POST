﻿using POST.CORE.DTO;
using System.Collections.Generic;

namespace POST.Core.DTO.ServiceCentres
{
    public class RegionServiceCentreMappingDTO : BaseDomainDTO
    {
        public int RegionServiceCentreMappingId { get; set; }

        public int RegionId { get; set; }
        public int ServiceCentreId { get; set; }

        public RegionDTO Region { get; set; }
        public ServiceCentreDTO ServiceCentre { get; set; }

        public List<int> ServiceCentreIds { get; set; }
        public List<ServiceCentreDTO> ServiceCentres { get; set; }
        public List<string> ServiceCentreNames { get; set; }
    }
}
