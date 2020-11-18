﻿using GIGL.GIGLS.Core.Repositories;
using GIGLS.Core.Domain;
using GIGLS.Core.DTO.Fleets;
using GIGLS.Core.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GIGLS.Core.IRepositories.Fleets
{
    public interface IDispatchRepository : IRepository<Dispatch>
    {
        Task<List<DispatchDTO>> GetDispatchAsync(int[] serviceCentreIds);
        Task<List<DispatchDTO>> CheckForOutstandingDispatch(string driverId);
        Task<List<DispatchDTO>> GetDeliveryDispatchForPartner(string userId, ManifestType manifestType);
    }

    public interface IMovementDispatchRepository : IRepository<MovementDispatch>   
    {
        Task<List<MovementDispatchDTO>> GetDispatchAsync(int[] serviceCentreIds);
        //Task<List<MovementDispatchDTO>> CheckForOutstandingDispatch(string driverId); 
    }
}
