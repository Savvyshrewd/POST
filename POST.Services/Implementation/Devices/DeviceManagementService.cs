﻿using POST.Core.IServices.Devices;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using POST.Core.DTO.Devices;
using POST.Core;
using POST.Infrastructure;
using POST.Core.Domain.Devices;

namespace POST.Services.Implementation.Devices
{
    public class DeviceManagementService : IDeviceManagementService
    {
        private readonly IUnitOfWork _uow;

        public DeviceManagementService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        //public async Task AssignDeviceToUser(string userId, int deviceId)
        //{
        //    try
        //    {
        //        var userDetail = await _uow.User.GetUserById(userId);
        //        if (userDetail == null)
        //        {
        //            throw new GenericException("User information does not exist");
        //        }

        //        var device = await _uow.Device.GetAsync(deviceId);
        //        if (device == null)
        //        {
        //            throw new GenericException("Device information does not exist");
        //        }

        //        //check if the device has not been assign to someone 
        //        var userActive = await _uow.DeviceManagement.GetAsync(x => x.IsActive == true && x.DeviceId == deviceId);

        //        if (userActive != null)
        //        {
        //            var assignedUser = await _uow.User.GetUserById(userActive.UserId);
        //            throw new GenericException($"{device.Name} already been assigned to: {assignedUser.FirstName} {assignedUser.LastName}");
        //        }
                
        //        //Add new device management
        //        var newManagement = new DeviceManagement
        //        {
        //            DeviceId = device.DeviceId,
        //            UserId = userDetail.Id,
        //            IsActive = true
        //        };
        //        _uow.DeviceManagement.Add(newManagement);
        //        _uow.Complete();
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        public async Task AssignDeviceToUser(DeviceManagementDTO deviceManagementDTO)
        {
            try
            {
                var userDetail = await _uow.User.GetUserById(deviceManagementDTO.UserId);
                if (userDetail == null)
                {
                    throw new GenericException("User information does not exist");
                }

                var device = await _uow.Device.GetAsync(deviceManagementDTO.DeviceId);
                if (device == null)
                {
                    throw new GenericException("Device information does not exist");
                }
                var stations = await _uow.ServiceCentre.GetAsync(deviceManagementDTO.LocationId);

                //check if the device has not been assign to someone 
                var userActive = await _uow.DeviceManagement.GetAsync(x => x.IsActive == true && x.DeviceId == deviceManagementDTO.DeviceId);

                if (userActive != null)
                {
                    var assignedUser = await _uow.User.GetUserById(userActive.UserId);
                    throw new GenericException($"{device.Name} already been assigned to: {assignedUser.FirstName} {assignedUser.LastName}");
                }
                
                //Add new device management
                var newManagement = new DeviceManagement
                {
                    DeviceId = device.DeviceId,
                    UserId = userDetail.Id,
                    IsActive = true,
                    LocationId = stations.ServiceCentreId,
                    DataSimCardNumber = deviceManagementDTO.DataSimCardNumber
                };
                _uow.DeviceManagement.Add(newManagement);
                _uow.Complete();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task UnAssignDeviceFromUser(int deviceManagementId)
        {
            try
            {
                var userActive = await _uow.DeviceManagement.GetAsync(deviceManagementId);

                if (userActive == null)
                {
                    throw new GenericException("Device information does not exist");
                }

                userActive.IsActive = false;
                _uow.Complete();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<DeviceManagementDTO>> GetActiveDeviceManagements()
        {
            return await _uow.DeviceManagement.GetActiveDeviceManagementAsync();
        }

        public async Task<DeviceManagementDTO> GetDeviceManagementById(int deviceManagementId)
        {
            return await _uow.DeviceManagement.GetDeviceManagementById(deviceManagementId);
        }

        public async Task<List<DeviceManagementDTO>> GetDeviceManagements()
        {
            return await _uow.DeviceManagement.GetDeviceManagementAsync();
        }

    }
}
