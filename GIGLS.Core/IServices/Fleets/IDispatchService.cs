﻿using POST.Core.DTO.Fleets;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace POST.Core.IServices.Fleets
{
    public interface IDispatchService : IServiceDependencyMarker
    {
        Task<List<DispatchDTO>> GetDispatchs();
        Task<DispatchDTO> GetDispatchById(int DispatchId);
        Task<DispatchDTO> GetDispatchManifestCode(string manifest);
        Task <List<DispatchDTO>> GetDispatchCaptainByName(string captain);
        Task<object> AddDispatch(DispatchDTO Dispatch);
        Task UpdateDispatch(int DispatchId, DispatchDTO Dispatch);
        Task DeleteDispatch(int DispatchId);
        Task UpdatePickupManifestStatus(ManifestStatusDTO manifestStatusDTO);
        Task<bool> UpdatePreshipmentMobileStatusToPickedup(string manifestNumber, List<string> waybills);
        Task<object> AddMovementDispatch(MovementDispatchDTO dispatchDTO);
        Task<MovementDispatchDTO> GetMovementDispatchManifestCode(string movementmanifestcode);
    }
}
