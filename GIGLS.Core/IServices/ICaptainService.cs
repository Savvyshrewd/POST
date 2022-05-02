﻿using GIGLS.Core.Domain.Partnership;
using GIGLS.Core.DTO.Captains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GIGLS.Core.IServices
{
    public interface ICaptainService : IServiceDependencyMarker
    {
        Task DeleteCaptainByIdAsync(int captainId);
        Task<IReadOnlyList<ViewCaptainsDTO>> GetCaptainsByDateAsync(DateTime? date);
        Task<object> RegisterCaptainAsync(RegCaptainDTO captainDTO);
        Task<object> GetCaptainByIdAsync(int partnerId);
        Task EditCaptainAsync(UpdateCaptainDTO partner);
        Task<bool> RegisterVehicleAsync(RegisterVehicleDTO vehicleDTO);
        Task<IReadOnlyList<CaptainDetailsDTO>> GetAllCaptainsAsync();
    }
}
