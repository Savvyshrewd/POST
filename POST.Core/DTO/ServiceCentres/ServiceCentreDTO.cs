﻿using POST.Core.DTO.Shipments;
using POST.Core.DTO.User;
using POST.CORE.DTO;
using System.Collections.Generic;

namespace POST.Core.DTO.ServiceCentres
{
    public class ServiceCentreDTO : BaseDomainDTO
    {
        public ServiceCentreDTO()
        {
            Users = new List<UserDTO>();
            Shipments = new List<ShipmentDTO>();
        }
        public int ServiceCentreId { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string Code { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public bool IsActive { get; set; }
        public int StationId { get; set; }
        public string StationName { get; set; }
        public int SupperServiceCentreId { get; set; } 
        public string StationCode { get; set; }
        public int CountryId { get; set; }
        public int LGAId { get; set; }
        public string Country { get; set; }
        public StationDTO Station { get; set; }
        public List<UserDTO> Users { get; set; }
        public List<ShipmentDTO> Shipments { get; set; }
        public decimal TargetAmount { get; set; }
        public int TargetOrder { get; set; }
        public bool IsDefault { get; set; }
        public double? Latitude { get; set; }

        public double? Longitude { get; set; }
        public bool IsHUB { get; set; }
        public string FormattedServiceCentreName { get; set; }
        public CountryDTO CountryDTO { get; set; }
        public bool IsPublic { get; set; }
        public bool HomeDeliveryStatus { get; set; }
        public bool IsGateway { get; set; }
        public bool IsConsignable { get; set; }
        public string CrAccount { get; set; }
    }


    public class WebsiteServiceCentreDTO
    {

        public int ServiceCentreId { get; set; }
        public int StationId { get; set; }
        public string Name { get; set; }
        public string Code { get; set; }
        public string Address { get; set; }
        public bool IsActive { get; set; }
        public string FormattedServiceCentreName { get; set; }

    }
}
