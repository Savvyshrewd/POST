﻿using GIGL.POST.Core.Repositories;
using POST.Core.Domain.Partnership;
using POST.Core.DTO.Partnership;
using POST.Core.DTO.Report;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace POST.Core.IRepositories.Partnership
{
    public interface IPartnerRepository : IRepository<Partner>
    {
        Task<List<PartnerDTO>> GetPartnersAsync();
        Task<Partner> GetLastValidPartnerCode();
        Task<PartnerDTO> GetPartnerByIdWithCountry(int customerId);
        Task<List<PartnerDTO>> GetExternalPartnersAsync();
        Task<List<PartnerDTO>> GetPartnerBySearchParameters(string parameter);
        Task<PartnerDTO> GetPartnerByUserId(string partnerId);
        Task<List<VehicleTypeDTO>> GetPartnersAsync(string fleetCode, bool? isActivated);
        Task<List<VehicleTypeDTO>> GetPartnersAsync(string fleetCode, bool? isActivated, ShipmentCollectionFilterCriteria filterCriteria);
        Task<List<Partner>> GetPartnerByEmail(string email);
    }
}
