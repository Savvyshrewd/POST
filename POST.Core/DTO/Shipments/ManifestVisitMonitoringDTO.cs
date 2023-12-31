﻿using POST.Core.DTO.ServiceCentres;
using POST.CORE.DTO;

namespace POST.Core.DTO.Shipments
{
    public class ManifestVisitMonitoringDTO : BaseDomainDTO
    {
        public int ManifestVisitMonitoringId { get; set; }
        public string Waybill { get; set; }        
        public string Address { get; set; }
        public string ReceiverName { get; set; }
        public string ReceiverPhoneNumber { get; set; }
        public string Status { get; set; }
        public string Signature { get; set; }
        public string UserId { get; set; }

        public int ServiceCentreId { get; set; }
        public ServiceCentreDTO ServiceCentre { get; set; }
    }
}
