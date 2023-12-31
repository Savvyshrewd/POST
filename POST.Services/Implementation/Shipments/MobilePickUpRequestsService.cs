﻿using AutoMapper;
using POST.Core;
using POST.Core.Domain;
using POST.Core.DTO;
using POST.Core.DTO.Shipments;
using POST.Core.Enums;
using POST.Core.IServices.CustomerPortal;
using POST.Core.IServices.Shipments;
using POST.Core.IServices.User;
using POST.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace POST.Services.Implementation.Shipments
{
    public class MobilePickUpRequestsService : IMobilePickUpRequestsService
    {

        private readonly IUnitOfWork _uow;
        private readonly IUserService _userservice;

        public MobilePickUpRequestsService(IUnitOfWork uow, IUserService userservice)
        {
            _uow = uow;
            _userservice = userservice;
            MapperConfig.Initialize();
        }

        public async Task AddMobilePickUpRequests(MobilePickUpRequestsDTO PickUpRequest)
        {
            try
            {
                var newMobilePickUpRequest = Mapper.Map<MobilePickUpRequests>(PickUpRequest);
                _uow.MobilePickUpRequests.Add(newMobilePickUpRequest);
                await _uow.CompleteAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task AddOrUpdateMobilePickUpRequests(MobilePickUpRequestsDTO PickUpRequest)
        {
            var request = await _uow.MobilePickUpRequests.GetAsync(s => s.Waybill == PickUpRequest.Waybill && s.UserId == PickUpRequest.UserId);

            if (request == null)
            {
                await AddMobilePickUpRequests(PickUpRequest);
            }
            else
            {
                request.Status = PickUpRequest.Status;
                await _uow.CompleteAsync();
            }
        }

        public async Task AddOrUpdateMobilePickUpRequestsMultipleShipments(MobilePickUpRequestsDTO pickUpRequest, List<string> waybillList)
        {
            var request = _uow.MobilePickUpRequests.GetAllAsQueryable().Where(s => waybillList.Contains(s.Waybill) && s.UserId == pickUpRequest.UserId).ToList();

            if (request.Any())
            {
                request.ForEach(x => x.Status = pickUpRequest.Status);
            }
            else
            {
                if (waybillList.Any())
                {
                    List<MobilePickUpRequests> mobilePickUpRequests = new List<MobilePickUpRequests>();

                    foreach (var waybill in waybillList)
                    {
                        pickUpRequest.Waybill = waybill;
                        var newRequest = Mapper.Map<MobilePickUpRequests>(pickUpRequest);
                        mobilePickUpRequests.Add(newRequest);
                    }
                    _uow.MobilePickUpRequests.AddRange(mobilePickUpRequests);
                }
            }
            await _uow.CompleteAsync();
        }

        public async Task<List<MobilePickUpRequestsDTO>> GetAllMobilePickUpRequests()
        {
            try
            {
                var userid = await _userservice.GetCurrentUserId();
                var mobilerequests = await _uow.MobilePickUpRequests.GetMobilePickUpRequestsAsync(userid);
                foreach (var item in mobilerequests)
                {
                    if (item.PreShipment.ServiceCentreAddress != null)
                    {
                        item.PreShipment.ReceiverLocation.Longitude = item.PreShipment.serviceCentreLocation.Longitude;
                        item.PreShipment.ReceiverLocation.Latitude = item.PreShipment.serviceCentreLocation.Latitude;
                        item.PreShipment.ReceiverAddress = item.PreShipment.ServiceCentreAddress;
                    }
                }
                return mobilerequests;
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<Partnerdto> GetMonthlyTransactions()
        {
            try
            {
                var CurrencyCode = "";
                var CurrencySymbol = "";
                var userid = await _userservice.GetCurrentUserId();
                var user = await _userservice.GetUserById(userid);
                var Country = await _uow.Country.GetAsync(s => s.CountryId == user.UserActiveCountryId);
                if (Country != null)
                {
                    CurrencyCode = Country.CurrencyCode;
                    CurrencySymbol = Country.CurrencySymbol;
                }

                var mobilerequests = await _uow.MobilePickUpRequests.GetMobilePickUpRequestsAsyncMonthly(userid);
                foreach (var item in mobilerequests)
                {
                    if (item.PreShipment.ServiceCentreAddress != null)
                    {
                        item.PreShipment.ReceiverLocation.Longitude = item.PreShipment.serviceCentreLocation.Longitude;
                        item.PreShipment.ReceiverLocation.Latitude = item.PreShipment.serviceCentreLocation.Latitude;
                        item.PreShipment.ReceiverAddress = item.PreShipment.ServiceCentreAddress;
                    }
                }
                var Count = await _uow.MobilePickUpRequests.FindAsync(x => x.UserId == userid && x.DateCreated.Month == DateTime.Now.Month && x.DateCreated.Year == DateTime.Now.Year && x.Status == "Delivered");
                int TotalDelivery = Count.Count();
                var TotalEarnings = await _uow.PartnerTransactions.FindAsync(s => s.UserId == userid && s.DateCreated.Month == DateTime.Now.Month && s.DateCreated.Year == DateTime.Now.Year);
                var TotalEarning = TotalEarnings.Sum(x => x.AmountReceived);
                var totaltransactions = new Partnerdto
                {
                    CurrencyCode = CurrencyCode,
                    CurrencySymbol = CurrencySymbol,
                    MonthlyDelivery = mobilerequests,
                    TotalDelivery = TotalDelivery,
                    MonthlyTransactions = TotalEarning
                };
                return totaltransactions;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task UpdateMobilePickUpRequests(MobilePickUpRequestsDTO pickUpRequest, string userId)
        {
            try
            {
                var MobilePickupRequests = await _uow.MobilePickUpRequests.GetAsync(s => s.Waybill == pickUpRequest.Waybill && s.UserId == userId && s.Status != MobilePickUpRequestStatus.Rejected.ToString());
                if (MobilePickupRequests != null)
                {
                    MobilePickupRequests.Status = pickUpRequest.Status;
                    if(pickUpRequest.Status == MobilePickUpRequestStatus.EnrouteToPickUp.ToString())
                    {
                        var preshipmentmobile = await _uow.PreShipmentMobile.GetAsync(s => s.Waybill == pickUpRequest.Waybill);
                        preshipmentmobile.TimePickedUp = DateTime.Now;
                    }
                    await _uow.CompleteAsync();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<PreShipmentMobile> UpdatePreShipmentMobileStatus(List<string> waybillList, string status)
        {
            try
            {
                var preshipmentmobile = _uow.PreShipmentMobile.GetAllAsQueryable().Where(s => waybillList.Contains(s.Waybill)).ToList();
                preshipmentmobile.ForEach(u => u.shipmentstatus = status);
                await _uow.CompleteAsync();
                return preshipmentmobile.FirstOrDefault();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public void UpdateMobilePickUpRequestsForWaybillList(List<string> waybills, string userId, string status)
        {
            try
            {
                var mobilePickupRequests = _uow.MobilePickUpRequests.GetAllAsQueryable().Where(s => waybills.Contains(s.Waybill) && s.UserId == userId && s.Status != MobilePickUpRequestStatus.Rejected.ToString()).ToList();

                if (mobilePickupRequests.Any())
                {
                    mobilePickupRequests.ForEach(u => u.Status = status);
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<MobilePickUpRequestsDTO>> GetAllMobilePickUpRequestsPaginated(ShipmentAndPreShipmentParamDTO shipmentAndPreShipmentParamDTO)
        {
            try
            {
                var mobilerequests = new List<MobilePickUpRequests>();
                var mobilerequestsDTO = new List<MobilePickUpRequestsDTO>();
                var userid = await _userservice.GetCurrentUserId();
                int totalCount;
                //set default values if payload is null
                if (shipmentAndPreShipmentParamDTO == null)
                {
                    shipmentAndPreShipmentParamDTO = new ShipmentAndPreShipmentParamDTO
                    {
                        Page = 1,
                        PageSize = 20,
                        StartDate = null,
                        EndDate = null
                    };
                }
                if (shipmentAndPreShipmentParamDTO.PageSize < 1)
                {
                    shipmentAndPreShipmentParamDTO.PageSize = 20;
                }
                if (shipmentAndPreShipmentParamDTO.Page < 1)
                {
                    shipmentAndPreShipmentParamDTO.Page = 1;
                }

                if (shipmentAndPreShipmentParamDTO.StartDate != null && shipmentAndPreShipmentParamDTO.EndDate != null)
                {

                    mobilerequests = _uow.MobilePickUpRequests.Query(x => x.UserId == userid && x.DateCreated >= shipmentAndPreShipmentParamDTO.StartDate && x.DateCreated <= shipmentAndPreShipmentParamDTO.EndDate).SelectPage(shipmentAndPreShipmentParamDTO.Page, shipmentAndPreShipmentParamDTO.PageSize, out totalCount).ToList();
                }
                else
                {
                    mobilerequests = _uow.MobilePickUpRequests.Query(x => x.UserId == userid).SelectPage(shipmentAndPreShipmentParamDTO.Page, shipmentAndPreShipmentParamDTO.PageSize, out totalCount).ToList();
                }

                mobilerequestsDTO = Mapper.Map<List<MobilePickUpRequestsDTO>>(mobilerequests);
                return mobilerequestsDTO;
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
