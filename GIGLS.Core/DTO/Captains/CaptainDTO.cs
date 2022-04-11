﻿using GIGLS.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GIGLS.Core.DTO.Captains
{
    public class CaptainDTO
    {
        public string LoggedInUserId { get; set; }

        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public Gender Gender { get; set; }
        public UserType UserType { get; set; }
        public string Designation { get; set; }
        public string Department { get; set; }
        public string PictureUrl { get; set; }
        public int Status { get; set; }
        public string Organisation { get; set; }
        public bool IsActive { get; set; }

        // Captain other info
        public string HomeAddress { get; set; }
        public string BankName { get; set; }
        public string AccountNumber { get; set; }
        public string AccountName { get; set; }
        public int Age { get; set; }
        public DateTime EmploymentDate { get; set; }
    }

    public class ViewCaptainsDTO
    {
        public int PartnerId { get; set; }
        public string Name { get; set; }
        public string CaptainCode { get; set; }
        public string Email { get; set; }
        public string VehicleAssigned { get; set; }
        public string Status { get; set; }
        public DateTime EmploymentDate { get; set; }
    }

    public class CaptainDetailsDTO
    {
        public int PartnerId { get; set; }
        public string CaptainName { get; set; }
        public string CaptainPhoneNumber { get; set; }
        public string PictureUrl { get; set; }
        public int CaptainAge { get; set; }
        public string CaptainCode { get; set; }
        public string Email { get; set; }
        public string AssignedVehicleName { get; set; }
        public string AssignedVehicleNumber { get; set; }
        public string Status { get; set; }
        public DateTime EmploymentDate { get; set; }
    }
}
