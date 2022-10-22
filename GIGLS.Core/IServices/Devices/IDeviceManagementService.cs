﻿using POST.Core.DTO.Devices;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace POST.Core.IServices.Devices
{
    public interface IDeviceManagementService : IServiceDependencyMarker
    {
        Task<List<DeviceManagementDTO>> GetDeviceManagements();
        Task<List<DeviceManagementDTO>> GetActiveDeviceManagements();
        Task<DeviceManagementDTO> GetDeviceManagementById(int deviceManagementId);
        //Task AssignDeviceToUser(string userId, int deviceId);

        Task AssignDeviceToUser(DeviceManagementDTO deviceManagementDTO);
        Task UnAssignDeviceFromUser(int deviceManagementId);
    }
}
