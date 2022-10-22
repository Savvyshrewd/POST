﻿using POST.Core.DTO.ServiceCentres;
using POST.Core.Enums;
using POST.CORE.DTO;
using System.Collections.Generic;

namespace POST.Core.DTO.Shipments
{
    public class ManifestWaybillMappingDTO : BaseDomainDTO
    {
        public int ManifestWaybillMappingId { get; set; }
        public bool IsActive { get; set; }

        public string ManifestCode { get; set; }
        public ManifestDTO ManifestDetails { get; set; }

        public string Waybill { get; set; }
        public ShipmentDTO Shipment { get; set; }

        public List<string> Waybills { get; set; }

        public int ServiceCentreId { get; set; }
        public ServiceCentreDTO ServiceCentre { get; set; }

        public ShipmentScanStatus ShipmentScanStatus { get; set; }
        public string ScanDescription { get; set;}

        public string DispatchRider { get; set; }

        public string CODManualAccount { get; set; }
    }
}
