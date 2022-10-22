﻿using POST.Core.DTO.ServiceCentres;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace POST.Core.IServices.ServiceCentres
{
    public interface IRegionServiceCentreMappingService : IServiceDependencyMarker
    {
        Task<List<RegionServiceCentreMappingDTO>> GetAllRegionServiceCentreMappings();
        Task MappingServiceCentreToRegion(int regionId, List<int> serviceCentreId);
        Task<RegionServiceCentreMappingDTO> GetRegionForServiceCentre(int serviceCentreId);
        Task<List<RegionServiceCentreMappingDTO>> GetServiceCentresInRegion(int regionId);
        Task RemoveServiceCentreFromRegion(int regionId, int serviceCentreId);

        Task<List<ServiceCentreDTO>> GetUnassignedServiceCentres();
    }
}
