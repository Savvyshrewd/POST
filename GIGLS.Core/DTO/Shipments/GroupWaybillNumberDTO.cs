﻿using GIGL.GIGLS.Core.Domain;
using GIGLS.CORE.DTO;

namespace GIGLS.Core.DTO.Shipments
{
    public class GroupWaybillNumberDTO : BaseDomainDTO
    {
        public int GroupWaybillNumberId { get; set; }
        public string GroupWaybillCode { get; set; }
        public bool IsActive { get; set; }

        public string UserId { get; set; }

        public int ServiceCentreId { get; set; }
        public string ServiceCentreCode { get; set; }

        //
        public ServiceCentre DepartureServiceCentre { get; set; }
        public ServiceCentre DestinationServiceCentre { get; set; }
    }
}
