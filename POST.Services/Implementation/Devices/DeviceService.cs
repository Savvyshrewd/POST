﻿using POST.Core.IServices.Devices;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using POST.Core.DTO.Devices;
using POST.Core;
using POST.Infrastructure;
using AutoMapper;
using POST.Core.Domain.Devices;
using System.Linq;

namespace POST.Services.Implementation.Devices
{
    public class DeviceService : IDeviceService
    {
        private readonly IUnitOfWork _uow;

        public DeviceService(IUnitOfWork uow)
        {
            _uow = uow;
            MapperConfig.Initialize();
        }

        public async Task<object> AddDevice(DeviceDTO deviceDto)
        {
            try
            {
                //check for unique serial number
                var device = await _uow.Device.GetAsync(x => x.SerialNumber == deviceDto.SerialNumber);
                if (device != null)
                {
                    throw new GenericException("Device information already exist");
                }

                var newDevice = Mapper.Map<Device>(deviceDto);
                newDevice.Active = true;
                _uow.Device.Add(newDevice);
                await _uow.CompleteAsync();
                return new { Id = newDevice.DeviceId };
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<DeviceDTO> GetDeviceById(int deviceId)
        {
            try
            {
                var device = await _uow.Device.GetAsync(deviceId);
                if (device == null)
                {
                    throw new GenericException("Device information does not exist");
                }

                var deviceDto = Mapper.Map<DeviceDTO>(device);
                return deviceDto;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task<IEnumerable<DeviceDTO>> GetDevices()
        {
            var devices = _uow.Device.GetAll().OrderBy(x => x.SerialNumber);
            return Task.FromResult(Mapper.Map<IEnumerable<DeviceDTO>>(devices));
        }

        public async Task RemoveDevice(int deviceId)
        {
            try
            {
                var device = await _uow.Device.GetAsync(deviceId);
                if (device == null)
                {
                    throw new GenericException("Device information does not exist");
                }
                _uow.Device.Remove(device);
                _uow.Complete();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task UpdateDevice(int deviceId, DeviceDTO deviceDto)
        {
            try
            {
                var device = await _uow.Device.GetAsync(deviceId);

                if (device == null || deviceDto.DeviceId != deviceId)
                {
                    throw new GenericException("Device information does not exist");
                }

                device.Name = deviceDto.Name;
                device.Active = deviceDto.Active;
                device.Description = deviceDto.Description;
                device.HandStarp = deviceDto.HandStarp;
                device.PowerAdapter = deviceDto.PowerAdapter;
                device.SerialNumber = deviceDto.SerialNumber;
                device.UsbCable = deviceDto.UsbCable;
                device.SimCardNumber = deviceDto.SimCardNumber;
                device.NetworkProvider = deviceDto.NetworkProvider;
                device.IMEI = deviceDto.IMEI;
                device.IMEI2 = deviceDto.IMEI2;
                _uow.Complete();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task UpdateDeviceStatus(int deviceId, bool status)
        {
            try
            {
                var device = await _uow.Device.GetAsync(deviceId);
                if (device == null)
                {
                    throw new GenericException("Device information does not exist");
                }
                device.Active = status;
                _uow.Complete();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
