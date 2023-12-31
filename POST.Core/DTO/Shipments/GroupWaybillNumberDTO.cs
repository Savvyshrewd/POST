﻿using GIGL.POST.Core.Domain;
using POST.Core.Enums;
using POST.CORE.DTO;
using System.Collections.Generic;

namespace POST.Core.DTO.Shipments
{
    public class GroupWaybillNumberDTO : BaseDomainDTO
    {
        public int GroupWaybillNumberId { get; set; }
        public string GroupWaybillCode { get; set; }
        public bool IsActive { get; set; }

        public string UserId { get; set; }

        public int ServiceCentreId { get; set; }
        public string ServiceCentreCode { get; set; }

        public bool HasManifest { get; set; }

        //
        public int DepartureServiceCentreId { get; set; }
        public ServiceCentre DepartureServiceCentre { get; set; }
        public ServiceCentre DestinationServiceCentre { get; set; }

        public List<string> WaybillNumbers { get; set; }
        public List<object> WaybillsWithDate { get; set; }
    }

    public class MovementManifestNumberDTO : BaseDomainDTO 
    {
        public int MovementManifestNumberId { get; set; } 
        public string MovementManifestCode { get; set; } 
        public bool IsActive { get; set; }

        public string UserId { get; set; }

        public int ServiceCentreId { get; set; }
        public string ServiceCentreCode { get; set; }

        public bool HasManifest { get; set; }
        public MovementStatus MovementStatus { get; set; }
        //
        public int DepartureServiceCentreId { get; set; }
        public ServiceCentre DepartureServiceCentre { get; set; }
        public ServiceCentre DestinationServiceCentre { get; set; }

        public List<string> ManifestNumbers { get; set; } 
        public List<object> ManifestNumbersWithDate { get; set; }

        public string DriverCode { get; set; }
        public string DestinationServiceCentreCode { get; set; }
        public bool Dispatched { get; set; }
    }

    public class ReleaseMovementManifestDto
    {
        // string movementManifestCode, string code, string userid, string flag
        public string movementManifestCode { get; set; }
        public string code { get; set; }
        public string userid { get; set; }
        public MovementManifestActivationTypes  flag { get; set; } 
    }

    public class GroupWaybillAndWaybillDTO 
    {
        public string GroupWaybillCode { get; set; }
        public List<WaybillInGroupWaybillDTO> WaybillsDTO { get; set; }
    }

    public class WaybillInGroupWaybillDTO
    {
        public string Waybill { get; set; }
        public decimal? Value { get; set; }
        public string Description { get; set; }
        public double Weight { get; set; }
        public string DepartureServiceCentre { get; set; }
        public string DestinationServiceCentre { get; set; }
        public string PaymentMethod { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
    }
}