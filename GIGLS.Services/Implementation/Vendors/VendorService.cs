﻿using System;
using System.Threading.Tasks;
using POST.Core.DTO.Vendors;
using POST.Core.IServices.Vendors;

namespace POST.Services.Implementation.Vendors
{
    public class VendorService : IVendorService
    {
        public Task<object> AddFleet(VendorDTO vendor)
        {
            throw new NotImplementedException();
        }

        public Task DeleteVendor(int vendorId)
        {
            throw new NotImplementedException();
        }

        public Task<VendorDTO> GetVendorById(int vendorId)
        {
            throw new NotImplementedException();
        }

        public Task<VendorDTO> GetVendors()
        {
            throw new NotImplementedException();
        }

        public Task UpdateVendor(int vendorId, VendorDTO vendor)
        {
            throw new NotImplementedException();
        }
    }
}
