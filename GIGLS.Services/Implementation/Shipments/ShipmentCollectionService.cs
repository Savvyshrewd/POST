﻿using GIGLS.CORE.IServices.Shipments;
using System.Collections.Generic;
using System.Threading.Tasks;
using GIGLS.CORE.DTO.Shipments;
using GIGLS.Core;
using GIGLS.Infrastructure;
using GIGLS.CORE.Domain;
using AutoMapper;
using GIGL.GIGLS.Core.Domain;
using GIGLS.Core.Enums;
using System;
using GIGLS.Core.IServices.User;
using GIGLS.Core.IServices.CashOnDeliveryAccount;
using GIGLS.Core.DTO.Wallet;

namespace GIGLS.Services.Implementation.Shipments
{
    public class ShipmentCollectionService : IShipmentCollectionService
    {
        private readonly IUnitOfWork _uow;
        private IUserService _userService;
        private ICashOnDeliveryAccountService _cashOnDeliveryAccountService;

        public ShipmentCollectionService(IUnitOfWork uow, IUserService userService,
            ICashOnDeliveryAccountService cashOnDeliveryAccountService)
        {
            _uow = uow;
            _userService = userService;
            _cashOnDeliveryAccountService = cashOnDeliveryAccountService;
            MapperConfig.Initialize();
        }

        public async Task AddShipmentCollection(ShipmentCollectionDTO shipmentCollection)
        {
            shipmentCollection.Waybill = shipmentCollection.Waybill.Trim().ToLower();

            if (await _uow.ShipmentCollection.ExistAsync(v => v.Waybill.ToLower() == shipmentCollection.Waybill))
            {
                throw new GenericException($"Waybill {shipmentCollection.Waybill} already exist");
            }

            var currentUserId = await _userService.GetCurrentUserId();

            var updateShipmentTracking = new ShipmentTracking
            {
                Waybill = shipmentCollection.Waybill,
                //Location = tracking.Location,
                Status = ShipmentScanStatus.Collected.ToString(),
                UserId = currentUserId,
                DateTime = DateTime.Now
            };

            var data = Mapper.Map<ShipmentCollection>(shipmentCollection);
            data.UserId = currentUserId;
            
            _uow.ShipmentCollection.Add(data);
            _uow.ShipmentTracking.Add(updateShipmentTracking);
            await _uow.CompleteAsync();
        }

        public async Task<ShipmentCollectionDTO> GetShipmentCollectionById(string waybill)
        {
            var shipmentCollection = await _uow.ShipmentCollection.GetAsync(x => x.Waybill.Equals(waybill));

            if (shipmentCollection == null)
            {
                throw new GenericException($"Shipment with waybill: {waybill} does not exist");
            }
            return Mapper.Map<ShipmentCollectionDTO>(shipmentCollection);

        }

        public Task<IEnumerable<ShipmentCollectionDTO>> GetShipmentCollections()
        {
            var shipmentCollection = _uow.ShipmentCollection.GetAll();
            var shipmentCollectionDto = Mapper.Map<IEnumerable<ShipmentCollectionDTO>>(shipmentCollection);
            return Task.FromResult(shipmentCollectionDto);
        }

        public async Task<IEnumerable<ShipmentCollectionDTO>> GetShipmentWaitingForCollection()
        {
            var shipmentCollection = await _uow.ShipmentCollection.FindAsync(x => x.ShipmentScanStatus == ShipmentScanStatus.Delivered);
            var shipmentCollectionDto = Mapper.Map<IEnumerable<ShipmentCollectionDTO>>(shipmentCollection);
            return await Task.FromResult(shipmentCollectionDto);
        }

        public async Task RemoveShipmentCollection(string waybill)
        {
            var shipmentCollection = await _uow.ShipmentCollection.GetAsync(x => x.Waybill.Equals(waybill));

            if (shipmentCollection == null)
            {
                throw new GenericException($"Shipment with waybill: {waybill} does not exist");
            }
            _uow.ShipmentCollection.Remove(shipmentCollection);
            await _uow.CompleteAsync();
        }

        public async Task UpdateShipmentCollection(ShipmentCollectionDTO shipmentCollectionDto)
        {
            var shipmentCollection = await _uow.ShipmentCollection.GetAsync(x => x.Waybill.Equals(shipmentCollectionDto.Waybill));

            if (shipmentCollection == null)
            {
                throw new GenericException("INFORMATION DOES NOT EXIST");
            }

            if (shipmentCollectionDto.UserId == null)
            {
                shipmentCollectionDto.UserId = await _userService.GetCurrentUserId();
            }

            shipmentCollection.Name = shipmentCollectionDto.Name;
            shipmentCollection.PhoneNumber = shipmentCollectionDto.PhoneNumber;
            shipmentCollection.Email = shipmentCollectionDto.Email;
            shipmentCollection.State = shipmentCollectionDto.State;
            shipmentCollection.City = shipmentCollectionDto.City;
            shipmentCollection.Address = shipmentCollectionDto.Address;
            shipmentCollection.IndentificationUrl = shipmentCollectionDto.IndentificationUrl;
            shipmentCollection.ShipmentScanStatus = shipmentCollectionDto.ShipmentScanStatus;
            shipmentCollection.UserId = shipmentCollectionDto.UserId;

            //cash collected on Delivery
            if (shipmentCollectionDto.IsCashOnDelivery == true)
            {
                await _cashOnDeliveryAccountService.AddCashOnDeliveryAccount(new CashOnDeliveryAccountDTO
                {
                    Amount = (decimal)shipmentCollectionDto.CashOnDeliveryAmount,
                    CreditDebitType = CreditDebitType.Credit,
                    UserId = shipmentCollectionDto.UserId,
                    Wallet = new WalletDTO
                    {
                        WalletNumber = shipmentCollectionDto.WalletNumber
                    },
                    Description = shipmentCollectionDto.Description
                });
            }

            await _uow.CompleteAsync();
        }


        //Check if the Shipment has not been collected before Processing Return Shipment
        public async Task CheckShipmentCollection(string waybill)
        {
            var shipmentCollection = await _uow.ShipmentCollection.GetAsync(x => x.Waybill.Equals(waybill) && x.ShipmentScanStatus == ShipmentScanStatus.Collected);

            if (shipmentCollection != null)
            {
                throw new GenericException($"Shipment with waybill: {waybill} had been collected");
            }

            var shipmentDelivered = await _uow.ShipmentCollection.GetAsync(x => x.Waybill.Equals(waybill) && x.ShipmentScanStatus == ShipmentScanStatus.Delivered);

            if (shipmentDelivered == null)
            {
                throw new GenericException($"Shipment with waybill: {waybill} is not available for Return Processing");
            }
        }
        
    }
}
