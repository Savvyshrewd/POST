﻿using System;
using System.Threading.Tasks;
using POST.Core.DTO.Shipments;
using POST.Core.IServices.Shipments;
using POST.Core;
using POST.Core.IServices.User;
using POST.Infrastructure;
using AutoMapper;
using POST.Core.Domain;
using System.Collections.Generic;
using System.Linq;
using POST.Core.Enums;
using POST.CORE.IServices.Shipments;
using POST.Core.IServices.Business;
using POST.Core.DTO.Report;
using System.Net;
using Newtonsoft.Json.Linq;

namespace POST.Services.Implementation.Shipments
{
    public class ShipmentContactService : IShipmentContactService
    {
        private readonly IUnitOfWork _uow;
        private IUserService _userService;
        private readonly IShipmentCollectionService _shipmentCollectionService;

        public ShipmentContactService(IUnitOfWork uow, IUserService userService,
            IShipmentCollectionService shipmentCollectionService)
        {
            _uow = uow;
            _userService = userService;
            _shipmentCollectionService = shipmentCollectionService;          
            MapperConfig.Initialize();
        }

        public async Task<List<ShipmentContactDTO>> GetShipmentContact(ShipmentContactFilterCriteria baseFilterCriteria)
        {
            var today = DateTime.Now;
            var shipmentContacts = new List<ShipmentContactDTO>();
            var shipmentDto = await _shipmentCollectionService.GetShipmentsCollectionForContact(baseFilterCriteria);
            if (shipmentDto.Any())
            {
                foreach (var item in shipmentDto)
                {
                    var shcDto = new ShipmentContactDTO();

                    if (item.ShipmentContactId > 0)
                    {
                        shcDto.ContactedBy = item.ContactedBy;
                        shcDto.Status = ShipmentContactStatus.Contacted;
                        shcDto.NoOfContact = item.NoOfContact;
                    }
                    else
                    {
                        shcDto.ContactedBy = "";
                        shcDto.Status = ShipmentContactStatus.NotContacted;
                        shcDto.NoOfContact = 0;
                    }

                    shcDto.DestinationServiceCentre = item.DestinationServiceCentre;
                    shcDto.DepartureServiceCentre = item.DepartsureServiceCentre;
                    shcDto.Age = (int)(today - item.DateCreated).Days;
                    shcDto.ReceiverName = item.ReceiverName;
                    shcDto.Waybill = item.Waybill;
                    shcDto.ReceiverPhoneNumber = item.ReceiverPhoneNumber;
                    shcDto.ShipmentCreatedDate = item.DateCreated;
                    shcDto.ShipmentStatus = item.ShipmentScanStatus.ToString();
                    shipmentContacts.Add(shcDto);
                }
            }

            return shipmentContacts.OrderByDescending(x => x.Age).ToList();
        }


        public async Task<bool> AddOrUpdateShipmentContactAndHistory(ShipmentContactDTO shipmentContactDTO)
        {
            try
            {
                if (shipmentContactDTO == null)
                {
                    throw new GenericException("Invalid payload", $"{(int)HttpStatusCode.BadRequest}");
                }
                var userId = await _userService.GetCurrentUserId();
                var userInfo = await _userService.GetUserById(userId);
                shipmentContactDTO.UserId = userId;
                var contact = await _uow.ShipmentContact.GetAsync(x => x.Waybill == shipmentContactDTO.Waybill);
                if (contact != null)
                {
                    contact.NoOfContact = contact.NoOfContact + 1;
                    contact.ContactedBy = $"{userInfo.FirstName} {userInfo.LastName}";
                    contact.DateModified = DateTime.Now;
                    contact.UserId = userId;

                    //also update history
                    var contactHistory = await _uow.ShipmentContactHistory.GetAsync(x => x.Waybill == shipmentContactDTO.Waybill && x.UserId == userId);
                    if (contactHistory != null)
                    {
                        contactHistory.NoOfContact = contactHistory.NoOfContact + 1;
                        contactHistory.DateModified = DateTime.Now;
                    }
                    else
                    {
                        var newHistory = new ShipmentContactHistory();
                        newHistory.DateCreated = DateTime.Now;
                        newHistory.ContactedBy = $"{userInfo.FirstName} {userInfo.LastName}";
                        newHistory.Waybill = shipmentContactDTO.Waybill;
                        newHistory.UserId = userId;
                        newHistory.NoOfContact = newHistory.NoOfContact + 1;
                        _uow.ShipmentContactHistory.Add(newHistory);
                    }


                }
                else
                {
                    var newContact = JObject.FromObject(shipmentContactDTO).ToObject<ShipmentContact>();
                    newContact.Status = ShipmentContactStatus.Contacted;
                    newContact.NoOfContact = newContact.NoOfContact + 1;
                    newContact.ContactedBy = $"{userInfo.FirstName} {userInfo.LastName}";
                    _uow.ShipmentContact.Add(newContact);

                    //also insert history
                    var newHistory = new ShipmentContactHistory();
                    newHistory.DateCreated = DateTime.Now;
                    newHistory.ContactedBy = $"{userInfo.FirstName} {userInfo.LastName}";
                    newHistory.Waybill = shipmentContactDTO.Waybill;
                    newHistory.UserId = userId;
                    newHistory.NoOfContact = newHistory.NoOfContact + 1;
                    _uow.ShipmentContactHistory.Add(newHistory);
                }
                await _uow.CompleteAsync();
                return true;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<ShipmentContactHistoryDTO>> GetShipmentContactHistoryByWaybill(string waybill)
        {
            if (String.IsNullOrEmpty(waybill))
            {
                throw new GenericException("Invalid param", $"{(int)HttpStatusCode.BadRequest}");
            }
            var history = _uow.ShipmentContactHistory.GetAll().Where(x => x.Waybill == waybill).ToList();
            var historyDto = JArray.FromObject(history).ToObject<List<ShipmentContactHistoryDTO>>();

            return historyDto;
        }
    }
}
