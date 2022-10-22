﻿using POST.Core.DTO.ServiceCentres;
using POST.CORE.DTO;

namespace POST.Core.DTO.Shipments
{
    public class TransitWaybillNumberDTO : BaseDomainDTO
    {
        public int TransitWaybillNumberId { get; set; }
        public string WaybillNumber { get; set; }

        public int ServiceCentreId { get; set; }
        public ServiceCentreDTO ServiceCentre { get; set; }

        public string UserId { get; set; }
        public bool IsGrouped { get; set; }
        public bool IsTransitCompleted { get; set; }
    }
}
