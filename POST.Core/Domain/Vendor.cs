﻿using POST.Core.Enums;
using POST.Core;
using POST.Core.Domain;
using System.ComponentModel.DataAnnotations;

namespace GIGL.POST.Core.Domain
{
    public class Vendor : BaseDomain, IAuditable
    {
        public int VendorId { get; set; }
        public string VendorName { get; set; }
        public string ContactName { get; set; }
        public string Address { get; set; }
        public string Email { get; set; }

        [MaxLength(100)]
        public string PhoneNumber { get; set; }
        public string CompanyRegistrationNumber { get; set; }
        public string BankName { get; set; }
        public string BankAccountNumber { get; set; }
        public VendorType VendorType { get; set; }
    }
}