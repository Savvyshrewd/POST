﻿using POST.Core.IServices.Fleets;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using POST.Core.DTO.Fleets;

namespace POST.Services.Implementation.Fleets
{
    public class FleetManagementService : IFleetManagementService
    {
        public Task<IEnumerable<FleetLocationDTO>> GetFleetLocations()
        {
            throw new NotImplementedException();
        }

        public Task<FleetLocationDTO> GetFleetLocationById(int fleetId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<FleetLocationDTO>> GetFleetLocationInformation(int fleetId, DateTime date)
        {
            throw new NotImplementedException();
        }
    }
}
