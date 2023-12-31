﻿using POST.Core.DTO.ServiceCentres;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POST.Core.IServices.ServiceCentres
{
    public interface ILGAService : IServiceDependencyMarker
    {
        Task<object> AddLGA(LGADTO lgaDto);
        Task<LGADTO> GetLGAById(int lgaId);
        Task<IEnumerable<LGADTO>> GetLGAs();
        Task UpdateLGA(int lgaId, LGADTO lgaDto);
        Task UpdateLGA(int lgaId, bool status);
        Task DeleteLGA(int lgaId);
        Task<IEnumerable<LGADTO>> GetActiveLGAs();
        Task<IEnumerable<LGADTO>> GetLGAByState(int stateId);
        Task UpdateHomeDeliveryLocation(int lgaId, bool status);
        Task<IEnumerable<LGADTO>> GetActiveHomeDeliveryLocations();
        Task<bool> CheckHomeDeliveryAllowed(int lgaID);
    }
}
