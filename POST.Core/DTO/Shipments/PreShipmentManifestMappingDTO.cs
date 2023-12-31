﻿using POST.Core.DTO.ServiceCentres;
using POST.Core.Enums;
using POST.CORE.DTO;
using System.Collections.Generic;

namespace POST.Core.DTO.Shipments
{
    public class PreShipmentManifestMappingDTO : BaseDomainDTO
    {
        public int PreShipmentManifestMappingId { get; set; }

        //Manifest
        public string ManifestCode { get; set; }

        //PreShipment
        public int PreShipmentId { get; set; }
        public PreShipmentDTO PreShipmentDTO { get; set; }

        public string Waybill { get; set; }
        public bool IsActive { get; set; }

        //Dispatch
        public string RegistrationNumber { get; set; }
        public string DriverDetail { get; set; }
        public string DispatchedBy { get; set; }
        public string ReceivedBy { get; set; }

        public List<string> Waybills { get; set; }
    }
}
