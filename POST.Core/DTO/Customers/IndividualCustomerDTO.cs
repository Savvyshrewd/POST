﻿using POST.Core.DTO.Shipments;
using POST.Core.Enums;
using POST.CORE.DTO;
using System.Collections.Generic;

namespace POST.Core.DTO.Customers
{
    public class IndividualCustomerDTO : BaseDomainDTO
    {
        public IndividualCustomerDTO()
        {
            CustomerShipments = new List<ShipmentDTO>();
        }
        public int IndividualCustomerId { get; set; }
        public string userId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Gender Gender { get; set; }
        public string Email { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public string PictureUrl { get; set; }
        public string PicData { get; set; }
        public string CustomerCode { get; set; }
        public List<ShipmentDTO> CustomerShipments { get; set; }

        public string Password { get; set; }

        //User Active CountryId
        public int UserActiveCountryId { get; set; }
        public string UserActiveCountryName { get; set; }
        public bool IsRegisteredFromMobile { get; set; }
        public bool IsFromMobile { get; set; }
        public CountryDTO Country { get; set; }
        public string AccountName { get; set; }

        public string AccountNumber { get; set; }

        public string BankName { get; set; }
        public CustomerType CustomerType { get; set; } = CustomerType.IndividualCustomer;
    }
}
