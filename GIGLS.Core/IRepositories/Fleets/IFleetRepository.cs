﻿using GIGL.GIGLS.Core.Domain;
using GIGL.GIGLS.Core.Repositories;
using GIGLS.Core.DTO.Fleets;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GIGLS.Core.IRepositories.Fleets
{
    public interface IFleetRepository : IRepository<Fleet>
    {
        Task<List<FleetDTO>> GetFleets();
    }
}
