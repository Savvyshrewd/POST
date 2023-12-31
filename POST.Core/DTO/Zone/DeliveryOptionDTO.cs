﻿using POST.Core.DTO.User;
using POST.CORE.DTO;
using POST.CORE.Enums;
using System.Collections.Generic;

namespace POST.Core.DTO.Zone
{
    public class DeliveryOptionDTO : BaseDomainDTO
    {
        public DeliveryOptionDTO()
        {
            UserDetail = new List<UserDTO>();
        }
        public int DeliveryOptionId { get; set; }
        public string Code { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public FilterCustomerType CustomerType { get; set; }

        public string UserId { get; set; }
        public string UserName { get; set; }

        public List<UserDTO> UserDetail { get; set; }
    }
}