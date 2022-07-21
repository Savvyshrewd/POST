﻿using GIGL.GIGLS.Core.Repositories;
using GIGLS.Core.Domain.Partnership;
using GIGLS.Core.DTO.Captains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GIGLS.Core.IRepositories
{
    public interface ICaptainRepository : IRepository<Partner>
    {
        Task<IReadOnlyList<Partner>> GetAllCaptainsAsync();
        Task<IReadOnlyList<ViewCaptainsDTO>> GetAllCaptainsByDateAsync(DateTime? date);
        Task<IReadOnlyList<VehicleDTO>> GetAllVehiclesByDateAsync(DateTime? date);
        Task<Partner> GetCaptainByIdAsync(int partnerId);
        Task<VehicleDetailsDTO> GetVehicleByRegistrationNumberAsync(string regNum);
    }
}