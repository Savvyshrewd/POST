﻿using GIGLS.Core.IServices.Fleets;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GIGLS.Core.DTO.Fleets;
using GIGLS.Core;
using AutoMapper;
using GIGLS.Core.Domain;
using GIGLS.Infrastructure;
using GIGLS.Core.IServices.User;
using GIGLS.Core.IServices.Wallet;
using GIGLS.Core.Enums;
using GIGLS.Core.DTO.ServiceCentres;
using System.Linq;
using GIGL.GIGLS.Core.Domain;
using GIGLS.Core.DTO.User;
using GIGLS.Core.Domain.Expenses;
using System.Net;
using GIGLS.Core.IServices.Shipments;
using GIGLS.Core.DTO.Shipments;

namespace GIGLS.Services.Implementation.Fleets
{
    public class DispatchService : IDispatchService
    {
        private readonly IUserService _userService;
        private readonly IWalletService _walletService;
        private readonly IUnitOfWork _uow;
        private readonly IPreShipmentMobileService _preshipmentMobileService;

        public DispatchService(IUserService userService, IWalletService walletService, IUnitOfWork uow, IPreShipmentMobileService preshipmentMobileService)
        {
            _walletService = walletService;
            _userService = userService;
            _uow = uow;
            _preshipmentMobileService = preshipmentMobileService;
            MapperConfig.Initialize();
        }

        /// <summary>
        /// This method creates a new dispatch, updates the manifest and system wallet information
        /// </summary>
        /// <param name="dispatchDTO"></param>
        /// <returns></returns>
        public async Task<object> AddDispatch(DispatchDTO dispatchDTO)
        {
            {
                int userServiceCentreId;
                int dispatchId;

                if(dispatchDTO.ManifestType == ManifestType.Pickup || dispatchDTO.ManifestType == ManifestType.PickupForDelivery)
                {
                    var gigGOServiceCenter = await _userService.GetGIGGOServiceCentre();
                    userServiceCentreId = gigGOServiceCenter.ServiceCentreId;
                }
                else
                {
                    var serviceCenterIds = await _userService.GetPriviledgeServiceCenters();
                    userServiceCentreId = serviceCenterIds[0];
                }

                //get the login user
                var currentUserId = await _userService.GetCurrentUserId();
                var currentUserDetail = await _userService.GetUserById(currentUserId);

                //check to see if there is a pending manifest for the user
               if(dispatchDTO.ManifestType == ManifestType.PickupForDelivery)
                {
                    var pendingDispatch = _uow.Dispatch.GetAll().Where(x => x.ReceivedBy == null && x.DriverDetail == dispatchDTO.UserId).ToList();
                    if (pendingDispatch.Any())
                    {
                        var manifests = pendingDispatch.Select(x => x.ManifestNumber);
                        throw new GenericException($"Error: Dispatch not registered. " +
                                   $"The following manifests [{string.Join(", ", manifests.ToList())}] has not been signed off");
                    }
                }

                //check for the type of delivery manifest to know which type of process to do
                if (dispatchDTO.ManifestType == ManifestType.Delivery)
                {
                    var checkForOutstanding = await CheckForOutstandingDispatch(dispatchDTO);

                    //filter all the ways in the delivery manifest for scanning processing
                    var ret = await FilterWaybillsInDeliveryManifest(dispatchDTO, currentUserId, userServiceCentreId);
                }
                else 
                {
                    if (dispatchDTO.ManifestType != ManifestType.Pickup && !dispatchDTO.IsSuperManifest)
                    {
                        //Verify that all waybills are not cancelled and scan all the waybills in case none was cancelled
                        var ret2 = await VerifyWaybillsInGroupWaybillInManifest(dispatchDTO.ManifestNumber, currentUserId, userServiceCentreId);
                    }                    
                }

                var dispatchObj = _uow.Dispatch.SingleOrDefault(s => s.ManifestNumber == dispatchDTO.ManifestNumber);
                if (dispatchObj != null )
                {
                    dispatchObj.DriverDetail = dispatchDTO.DriverDetail;
                    dispatchObj.Amount = dispatchDTO.Amount;
                    dispatchObj.DispatchedBy = currentUserDetail.FirstName + " " + currentUserDetail.LastName;
                    dispatchId = dispatchObj.DispatchId;
                    dispatchObj.DriverDetail = dispatchDTO.UserId == null ? dispatchDTO.DriverDetail : dispatchDTO.UserId;
                }
                else
                {
                    // create dispatch
                    var newDispatch = Mapper.Map<Dispatch>(dispatchDTO);
                    newDispatch.DispatchedBy = currentUserDetail.FirstName + " " + currentUserDetail.LastName;
                    newDispatch.ServiceCentreId = userServiceCentreId;

                    if (dispatchDTO.ManifestType == ManifestType.PickupForDelivery)
                    {
                        newDispatch.DriverDetail = dispatchDTO.UserId;
                    }
                    newDispatch.DriverDetail = dispatchDTO.UserId == null ? dispatchDTO.DriverDetail : dispatchDTO.UserId;
                    //Set Departure Service Center
                    newDispatch.DepartureServiceCenterId = userServiceCentreId;
                    newDispatch.DepartureId = _uow.ServiceCentre.GetAllAsQueryable().Where(x => x.ServiceCentreId == newDispatch.DepartureServiceCenterId).Select(x => x.StationId).FirstOrDefault();
                    //newDispatch.DepartureServiceCenterId = dispatchDTO.DepartureServiceCenterId;
                    newDispatch.DestinationServiceCenterId = dispatchDTO.DestinationServiceCenterId;
                    _uow.Dispatch.Add(newDispatch);
                    dispatchId = newDispatch.DispatchId;
                }

                Manifest manifestObj = null;
                List<Manifest> manifestObjs = new List<Manifest>();
                // update manifest

                if (dispatchDTO.IsSuperManifest)
                {
                    manifestObjs =  _uow.Manifest.GetAllAsQueryable().Where(s => s.SuperManifestCode == dispatchDTO.ManifestNumber).ToList();
                }
                else
                {
                    manifestObj = _uow.Manifest.SingleOrDefault(s => s.ManifestCode == dispatchDTO.ManifestNumber);
                }
               
                
                if (manifestObj == null)
                {
                    var pickupManifestObj = _uow.PickupManifest.SingleOrDefault(s => s.ManifestCode == dispatchDTO.ManifestNumber);
                    if(pickupManifestObj != null)
                    {
                        var pickupManifestEntity = _uow.PickupManifest.Get(pickupManifestObj.PickupManifestId);
                        pickupManifestEntity.DispatchedById = currentUserId;
                        pickupManifestEntity.IsDispatched = true;
                        pickupManifestEntity.ManifestStatus = ManifestStatus.Pending;
                        pickupManifestEntity.ManifestType = dispatchDTO.ManifestType;
                    }
                }
                if (manifestObj != null)
                {
                    var manifestEntity = _uow.Manifest.Get(manifestObj.ManifestId);
                    manifestEntity.DispatchedById = currentUserId;
                    manifestEntity.IsDispatched = true;
                    manifestEntity.ManifestType = dispatchDTO.ManifestType;
                    manifestEntity.DestinationServiceCentreId = dispatchDTO.DestinationServiceCenterId;

                    if (dispatchDTO.IsSuperManifest)
                    {
                        manifestEntity.SuperManifestStatus = SuperManifestStatus.Dispatched;
                    }
                }

                List<Dispatch> data = new List<Dispatch>();
                
                if (dispatchDTO.IsSuperManifest)
                {
                    foreach (var manifest in manifestObjs)
                    {
                        var ret2 = await VerifyWaybillsInGroupWaybillInManifest(manifest.ManifestCode, currentUserId, userServiceCentreId);
                    }
                }

                //Update for manifest not dispatched in super manifest list 
                foreach (var manifest in manifestObjs)
                {
                    var manifestdispatchObj = _uow.Dispatch.SingleOrDefault(s => s.ManifestNumber == manifest.ManifestCode);
                    if(manifestdispatchObj == null)
                    {
                        // create dispatch
                        var newDispatch = Mapper.Map<Dispatch>(dispatchDTO);
                        newDispatch.ManifestNumber = manifest.ManifestCode;
                        newDispatch.DispatchedBy = currentUserDetail.FirstName + " " + currentUserDetail.LastName;
                        newDispatch.ServiceCentreId = userServiceCentreId;
                        newDispatch.DepartureServiceCenterId = dispatchDTO.DepartureServiceCenterId;
                        newDispatch.DestinationServiceCenterId = dispatchDTO.DestinationServiceCenterId;
                        newDispatch.IsSuperManifest = false;
                        data.Add(newDispatch);
                        //_uow.Dispatch.Add(newDispatch);
                    }
                }
                _uow.Dispatch.AddRange(data);

                //Update for Super Manifest
                if (manifestObjs != null)
                {
                    manifestObjs.ForEach(x => x.DispatchedById = currentUserId);
                    manifestObjs.ForEach(x => x.IsDispatched = true);
                    manifestObjs.ForEach(x => x.ManifestType = dispatchDTO.ManifestType);
                    manifestObjs.ForEach(x => x.SuperManifestStatus = SuperManifestStatus.Dispatched);
                    manifestObjs.ForEach(x => x.DestinationServiceCentreId = dispatchDTO.DestinationServiceCenterId);                    
                }
                
                ////--start--///Set the DepartureCountryId
                int countryIdFromServiceCentreId = 0;
                try
                {
                    var departureCountry = await _uow.Country.GetCountryByServiceCentreId(userServiceCentreId);
                    countryIdFromServiceCentreId = departureCountry.CountryId;
                }
                catch (Exception) { }
                ////--end--///Set the DepartureCountryId


                //update General Ledger
                var generalLedger = new GeneralLedger()
                {
                    DateOfEntry = DateTime.Now,
                    ServiceCentreId = userServiceCentreId,
                    CountryId = countryIdFromServiceCentreId,
                    UserId = currentUserId,
                    Amount = dispatchDTO.Amount,
                    CreditDebitType = CreditDebitType.Debit,
                    Description = "Debit from Dispatch :" + dispatchDTO.ManifestNumber,
                    IsDeferred = false,
                    PaymentServiceType = PaymentServiceType.Dispatch
                };
                _uow.GeneralLedger.Add(generalLedger);

                if(dispatchDTO.Amount > 0)
                {
                    //add record to Expenditure 
                    var expenditure = new Expenditure
                    {
                        Amount = dispatchDTO.Amount,
                        ExpenseTypeId = 11, //Id number for dispatch on Expense type
                        ServiceCentreId = userServiceCentreId,
                        UserId = currentUserId,
                        Description = "Dispatch fee for " + dispatchDTO.ManifestType.ToString() + " Manifest " + dispatchDTO.ManifestNumber
                    };
                    _uow.Expenditure.Add(expenditure);
                }

                // commit transaction
                await _uow.CompleteAsync();
                return new { Id = dispatchId };
            }
        }


        /// <summary>
        /// This method creates a new dispatch, updates the manifest and system wallet information
        /// </summary>
        /// <param name="MovementdispatchDTO"></param>
        /// <returns></returns>
        public async Task<object> AddMovementDispatch(MovementDispatchDTO dispatchDTO) 
        {
            {
                int userServiceCentreId;
                int dispatchId;
                var serviceCenterIds = await _userService.GetPriviledgeServiceCenters();
                userServiceCentreId = serviceCenterIds[0];

                //get the login user
                var currentUserId = await _userService.GetCurrentUserId();
                var currentUserDetail = await _userService.GetUserById(currentUserId);
                //var dispatchObj = _uow.MovementDispatch.SingleOrDefault(s => s.MovementManifestNumber == dispatchDTO.MovementManifestNumber);

                // create dispatch
                var newDispatch = Mapper.Map<MovementDispatch>(dispatchDTO);
                newDispatch.DispatchedBy = currentUserDetail.FirstName + " " + currentUserDetail.LastName;
                newDispatch.ServiceCentreId = userServiceCentreId;

                //Set Departure Service Center
                newDispatch.DepartureServiceCenterId = userServiceCentreId;
                newDispatch.DepartureId = _uow.ServiceCentre.GetAllAsQueryable().Where(x => x.ServiceCentreId == newDispatch.DepartureServiceCenterId)
                    .Select(x => x.StationId).FirstOrDefault();

                //newDispatch.DepartureServiceCenterId = dispatchDTO.DepartureServiceCenterId;
                newDispatch.DestinationServiceCenterId = dispatchDTO.DestinationServiceCenterId;

                _uow.MovementDispatch.Add(newDispatch); 
                dispatchId = newDispatch.DispatchId;

                ////--start--///Set the DepartureCountryId
                int countryIdFromServiceCentreId = 0;
                try
                {
                    var departureCountry = await _uow.Country.GetCountryByServiceCentreId(userServiceCentreId);
                    countryIdFromServiceCentreId = departureCountry.CountryId;
                }
                catch (Exception) { }

                //Scan movement manifest waybill
                await UpdateMovementManifestWaybillScanStatus(dispatchDTO.MovementManifestNumber, currentUserId, userServiceCentreId);

                //Write to Fleet trip table
                var fleetTrip = new FleetTrip
                {
                    MovementManifestId = dispatchId,
                    DispatchAmount = newDispatch.Amount,
                    DestinationStationId = newDispatch.DestinationId,
                    DepartureStationId = newDispatch.DepartureId,
                    FleetRegistrationNumber = newDispatch.RegistrationNumber,
                    CaptainId = newDispatch.DriverDetail,
                    DepartureServiceCenterId = newDispatch.DepartureServiceCenterId,
                    DestinationServiceCenterId = newDispatch.DestinationServiceCenterId
                };

                _uow.FleetTrip.Add(fleetTrip);

                // commit transaction
                await _uow.CompleteAsync();
                return new { Id = dispatchId };
            }
        }


        /// <summary>
        /// This method ensures that all waybills attached to the manifestNumber 
        /// are filter for scanning processing.
        /// </summary>
        /// <param name="manifestNumber"></param>
        private async Task<int> FilterWaybillsInDeliveryManifest(DispatchDTO dispatchDTO, string currentUserId, int userServiceCentreId)
        {
            // manifest ->  waybill
            var manifestWaybillMappings = await _uow.ManifestWaybillMapping.FindAsync(s => s.ManifestCode == dispatchDTO.ManifestNumber);
            var listOfWaybills = manifestWaybillMappings.Select(s => s.Waybill).ToList();

            if (listOfWaybills.Count > 0)
            {
                //Scan all waybills attached to this manifestNumber
                //ShipmentScanStatus status = ShipmentScanStatus.WC;
                await ScanWaybillsInManifest(listOfWaybills, currentUserId, userServiceCentreId, ShipmentScanStatus.WC);
            }

            return 0;
        }


        /// <summary>
        /// This method ensures that all waybills attached to groupwaybill in the manifestNumber 
        /// are not in the cancelled status.
        /// </summary>
        /// <param name="manifestNumber"></param>
        private async Task<int> VerifyWaybillsInGroupWaybillInManifest(string manifestNumber, string currentUserId, int userServiceCentreId)
        {
            // manifest -> groupwaybill -> waybill
            //manifest
            var manifestMappings = await _uow.ManifestGroupWaybillNumberMapping.FindAsync(s => s.ManifestCode == manifestNumber);
            var listOfGroupWaybills = manifestMappings.Select(s => s.GroupWaybillNumber);

            //groupwaybill
            var groupwaybillMappings = await _uow.GroupWaybillNumberMapping.FindAsync(s => listOfGroupWaybills.Contains(s.GroupWaybillNumber));
            var listOfWaybills = groupwaybillMappings.Select(s => s.WaybillNumber);

            //waybill - from shipmentCancel entity
            var cancelledWaybills = await _uow.ShipmentCancel.FindAsync(s => listOfWaybills.Contains(s.Waybill));
            if (cancelledWaybills.ToList().Count > 0)
            {
                var waybills = cancelledWaybills.ToList().ToString();
                throw new GenericException($"{waybills} : The waybill has been cancelled. " +
                    $"Please remove from the manifest and try again.");
            }
            else
            {
                //Scan all waybills attached to this manifestNumber
                //string status = ShipmentScanStatus.DSC.ToString();
                await ScanWaybillsInManifest(listOfWaybills.ToList(), currentUserId, userServiceCentreId, ShipmentScanStatus.DSC);
            }
            return 0;
        }

        private async Task<int> UpdateMovementManifestWaybillScanStatus(string movementManifestNumber, string currentUserId, int userServiceCentreId)
        {

            //movement manifest -> manifest -> groupwaybill -> waybill
            var movementManifestMappings = await _uow.MovementManifestNumberMapping.FindAsync(s => s.MovementManifestCode == movementManifestNumber);
            var listOfManifestNumbers = movementManifestMappings.Select(s => s.ManifestNumber);

            // manifest -> groupwaybill -> waybill
            var manifestMappings = await _uow.ManifestGroupWaybillNumberMapping.FindAsync(s => listOfManifestNumbers.Contains(s.ManifestCode));
            var listOfGroupWaybills = manifestMappings.Select(s => s.GroupWaybillNumber);

            //groupwaybill -> waybill
            var groupwaybillMappings = await _uow.GroupWaybillNumberMapping.FindAsync(s => listOfGroupWaybills.Contains(s.GroupWaybillNumber));
            var listOfWaybills = groupwaybillMappings.Select(s => s.WaybillNumber);

            //waybill - from shipmentCancel entity
            var cancelledWaybills = await _uow.ShipmentCancel.FindAsync(s => listOfWaybills.Contains(s.Waybill));
            if (cancelledWaybills.ToList().Count > 0)
            {
                var waybills = cancelledWaybills.ToList().ToString();
                throw new GenericException($"{waybills} : The waybill has been cancelled. " +
                    $"Please remove from the manifest and try again.");
            }
            else
            {
                var serviceCenter = await _uow.ServiceCentre.GetAsync(userServiceCentreId);

                ShipmentScanStatus scanStatus = serviceCenter.IsGateway ? ShipmentScanStatus.DCC : ShipmentScanStatus.DPC;

                //Scan all waybills attached to this movement manifest Number
                await ScanWaybillsInManifest(listOfWaybills.ToList(), currentUserId, userServiceCentreId, scanStatus);
            }
            return 0;
        }
        /// <summary>
        /// This method ensures that all waybills attached to this manifestNumber
        /// are scan.
        /// </summary>

        private async Task ScanWaybillsInManifest(List<string> waybills, string currentUserId, int userServiceCentreId, ShipmentScanStatus scanStatus)
        {
            var serviceCenter = await _uow.ServiceCentre.GetAsync(userServiceCentreId);

            foreach (var item in waybills)
            {
                var newShipmentTracking = new ShipmentTracking
                {
                    Waybill = item,
                    Location = serviceCenter.Name,
                    Status = scanStatus.ToString(),
                    DateTime = DateTime.Now,
                    UserId = currentUserId,
                    ServiceCentreId = serviceCenter.ServiceCentreId
                };

                _uow.ShipmentTracking.Add(newShipmentTracking);

                //use to optimise shipment progress for shipment that has depart service centre
                //update shipment table if the scan status contain any of the following : TRO, DSC, DTR
                if (scanStatus.Equals(ShipmentScanStatus.DSC) || scanStatus.Equals(ShipmentScanStatus.TRO) || scanStatus.Equals(ShipmentScanStatus.DTR))
                {
                    //Get shipment Details
                    var shipment = await _uow.Shipment.GetAsync(x => x.Waybill.Equals(item));

                    //update shipment if the user belong to original departure service centre
                    if (shipment.DepartureServiceCentreId == serviceCenter.ServiceCentreId && shipment.ShipmentScanStatus != scanStatus)
                    {
                        shipment.ShipmentScanStatus = scanStatus; 
                    }

                    //update the international table for those waybill that has been dispatch
                    if(shipment.InternationalShipmentType == InternationalShipmentType.DHL)
                    {
                        var internationalShipment = await _uow.InternationalShipmentWaybill.GetAsync(x => x.Waybill == item);
                        if(internationalShipment != null)
                        {
                            internationalShipment.InternationalShipmentStatus = InternationalShipmentStatus.Processing;
                        }
                    }
                }
            }
        }

        public async Task DeleteDispatch(int dispatchId)
        {
            try
            {
                var dispatch = await _uow.Dispatch.GetAsync(dispatchId);
                if (dispatch == null)
                {
                    throw new GenericException("Information does not Exist");
                }
                _uow.Dispatch.Remove(dispatch);
                await _uow.CompleteAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<DispatchDTO> GetDispatchById(int dispatchId)
        {
            try
            {
                var dispatch = await _uow.Dispatch.GetAsync(dispatchId);
                if (dispatch == null)
                {
                    throw new GenericException("Information does not Exist");
                }

                var dispatchDTO = Mapper.Map<DispatchDTO>(dispatch);

                //get the ManifestType
                var manifestObj = await _uow.Manifest.GetAsync(x => x.ManifestCode.Equals(dispatch.ManifestNumber));
                dispatchDTO.ManifestType = manifestObj.ManifestType;

                return dispatchDTO;

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<MovementDispatchDTO> GetMovementDispatchManifestCode(string movementmanifestcode)  
        {
            try
            {
                var dispatchResult = await _uow.MovementDispatch.FindAsync(x => x.MovementManifestNumber.Equals(movementmanifestcode));
                var dispatch = dispatchResult.FirstOrDefault();
                if (dispatch == null)
                {
                    return null;
                }

                var dispatchDTO = Mapper.Map<MovementDispatchDTO>(dispatch);

                //get User detail
                var user = await _uow.User.GetUserById(dispatch.DriverDetail);

                if (user != null)
                {
                    dispatchDTO.UserDetail = Mapper.Map<UserDTO>(user);
                    dispatchDTO.DriverDetail = user.FirstName + " " + user.LastName;
                }

                //get the country by service centre
                if (dispatchDTO.ServiceCentreId > 0)
                {
                    int serviceCentre = (int)dispatchDTO.ServiceCentreId;
                    var country = await _uow.Country.GetCountryByServiceCentreId(serviceCentre);
                    dispatchDTO.Country = country;
                }
                //get Station Detail
                var destStation = await _uow.Station.GetAsync(x => x.StationId == dispatch.DestinationId);
                if (destStation != null)
                {
                    dispatchDTO.Destination = Mapper.Map<StationDTO>(destStation);
                }

                var deptStation = await _uow.Station.GetAsync(x => x.StationId == dispatch.DepartureId);
                if (deptStation != null)
                {
                    dispatchDTO.Departure = Mapper.Map<StationDTO>(deptStation);
                }
                //get Service Center Detail
                var destService = await _uow.ServiceCentre.GetAsync(x => x.ServiceCentreId == dispatch.DestinationServiceCenterId);
                if (destService != null)
                {
                    dispatchDTO.DestinationService = Mapper.Map<ServiceCentreDTO>(destService);
                }

                var deptService = await _uow.ServiceCentre.GetAsync(x => x.ServiceCentreId == dispatch.DepartureServiceCenterId);
                if (deptService != null)
                {
                    dispatchDTO.DepartureService = Mapper.Map<ServiceCentreDTO>(deptService);
                }

                return dispatchDTO;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<DispatchDTO> GetDispatchManifestCode(string manifest)
        {
            try
            {
                var dispatchResult = await _uow.Dispatch.FindAsync(x => x.ManifestNumber.Equals(manifest));
                var dispatch = dispatchResult.FirstOrDefault();
                if (dispatch == null)
                {
                    return null;
                }

                var dispatchDTO = Mapper.Map<DispatchDTO>(dispatch);

                //get User detail
                var user = await _uow.User.GetUserById(dispatch.DriverDetail);

                if (user != null)
                {
                    dispatchDTO.UserDetail = Mapper.Map<UserDTO>(user);
                    dispatchDTO.DriverDetail = user.FirstName + " " + user.LastName;
                }

                //get the country by service centre
                if(dispatchDTO.ServiceCentreId > 0)
                {
                    int serviceCentre = (int)dispatchDTO.ServiceCentreId;
                    var country = await _uow.Country.GetCountryByServiceCentreId(serviceCentre);
                    dispatchDTO.Country = country;
                }
                //get Station Detail
                var destStation = await _uow.Station.GetAsync(x => x.StationId == dispatch.DestinationId);
                if (destStation != null)
                {
                    dispatchDTO.Destination = Mapper.Map<StationDTO>(destStation);
                }

                var deptStation = await _uow.Station.GetAsync(x => x.StationId == dispatch.DepartureId);
                if (deptStation != null)
                {
                    dispatchDTO.Departure = Mapper.Map<StationDTO>(deptStation);
                }
                //get Service Center Detail
                var destService = await _uow.ServiceCentre.GetAsync(x => x.ServiceCentreId == dispatch.DestinationServiceCenterId);
                if (destService != null)
                {
                    dispatchDTO.DestinationService = Mapper.Map<ServiceCentreDTO>(destService);
                }

                var deptService = await _uow.ServiceCentre.GetAsync(x => x.ServiceCentreId == dispatch.DepartureServiceCenterId);
                if(deptService != null)
                {
                    dispatchDTO.DepartureService = Mapper.Map<ServiceCentreDTO>(deptService);
                }

                //get the ManifestType
                var manifestObj = await _uow.Manifest.GetAsync(x => x.ManifestCode.Equals(manifest));
                if (manifestObj == null && !dispatch.IsSuperManifest)
                {
                    var pickupManifestObject = await _uow.PickupManifest.GetAsync(x => x.ManifestCode.Equals(manifest));
                    dispatchDTO.ManifestType = pickupManifestObject.ManifestType;
                }
                else if (dispatch.IsSuperManifest)
                {
                    var manifestObjs = await _uow.Manifest.FindAsync(x => x.SuperManifestCode.Equals(manifest));
                    manifestObj = manifestObjs.FirstOrDefault();
                    dispatchDTO.ManifestType = manifestObj.ManifestType;

                }
                else 
                {
                    dispatchDTO.ManifestType = manifestObj.ManifestType;
                }
                
                return dispatchDTO;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<DispatchDTO>> GetDispatchCaptainByName(string captain)
        {
            try
            {
                var dispatchResult = await _uow.Dispatch.FindAsync(x => x.DriverDetail.Equals(captain));
               
                if (dispatchResult.Count() == 0)
                {
                    return null;
                }
                var dispatchDTO = Mapper.Map<List<DispatchDTO>>(dispatchResult);

                return dispatchDTO;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<DispatchDTO>> GetDispatchs()
        {
            var serviceCenterIds = await _userService.GetPriviledgeServiceCenters();
            var dispatchs = await _uow.Dispatch.GetDispatchAsync(serviceCenterIds);

            foreach (var item in dispatchs)
            {
                // get the service cenre
                var departureSC = await _uow.Station.GetAsync((int)item.DepartureId);
                var destinationSC = await _uow.Station.GetAsync((int)item.DestinationId);

                item.Departure = Mapper.Map<StationDTO>(departureSC);
                item.Destination = Mapper.Map<StationDTO>(destinationSC);
            }
            return dispatchs;
        }

        public async Task UpdateDispatch(int dispatchId, DispatchDTO dispatchDTO)
        {
            try
            {
                var dispatch = await _uow.Dispatch.GetAsync(dispatchId);
                if (dispatch == null || dispatchDTO.DispatchId != dispatchId)
                {
                    throw new GenericException("Information does not Exist");
                }

                dispatch.DispatchId = dispatchDTO.DispatchId;
                dispatch.RegistrationNumber = dispatchDTO.RegistrationNumber;
                dispatch.ManifestNumber = dispatchDTO.ManifestNumber;
                dispatch.Amount = dispatchDTO.Amount;
                dispatch.RescuedDispatchId = dispatchDTO.RescuedDispatchId;
                dispatch.DriverDetail = dispatchDTO.DriverDetail;
                dispatch.DispatchedBy = dispatchDTO.DispatchedBy;
                dispatch.ReceivedBy = dispatchDTO.ReceivedBy;
                dispatch.DispatchCategory = dispatchDTO.DispatchCategory;
                dispatch.DepartureId = dispatchDTO.DepartureId;
                dispatch.DestinationId = dispatchDTO.DestinationId;
                dispatch.ServiceCentreId = dispatchDTO.ServiceCentreId;

                _uow.Complete();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<bool> CheckForOutstandingDispatch(DispatchDTO dispatchDTO)
        {
            var dispatch = await _uow.Dispatch.CheckForOutstandingDispatch(dispatchDTO.DriverDetail);
            
            if (dispatch != null)
            {
                foreach(var item in dispatch)
                {
                    if(item.DateModified.Date == DateTime.Now.Date)
                    {
                        return true;
                    }
                    else if(item.DateModified.Date != DateTime.Now.Date)
                    {
                        throw new GenericException("The Dispatch Partner has some unsign Off Delivery Manifest(s)");
                    }
                }
            }
            return true;
        }

        public async Task UpdatePickupManifestStatus(ManifestStatusDTO manifestStatusDTO)
        {
            try
            {
                var userId = await _userService.GetCurrentUserId();
                
                var dispatch = await _uow.Dispatch.GetAsync(s => s.ManifestNumber == manifestStatusDTO.ManifestCode && s.DriverDetail == userId);
                if(dispatch == null)
                {
                    throw new GenericException("This manifest is not assigned to you", $"{(int)HttpStatusCode.Forbidden}");
                }
                else
                {
                    var pickupManifestObj = _uow.PickupManifest.SingleOrDefault(s => s.ManifestCode == manifestStatusDTO.ManifestCode);
                    if (pickupManifestObj != null)
                    {
                        if (pickupManifestObj.ManifestStatus == ManifestStatus.Delivered)
                        {
                            throw new GenericException($"Manifest {manifestStatusDTO.ManifestCode} has been delivered", $"{(int)HttpStatusCode.Forbidden}");
                        }
                        else
                        {
                            //This condition need to be review  
                            if (pickupManifestObj.ManifestStatus == ManifestStatus.Accepted &&
                                (manifestStatusDTO.ManifestStatus == ManifestStatus.Rejected || manifestStatusDTO.ManifestStatus == ManifestStatus.Pending))
                            {
                                throw new GenericException($"Manifest {manifestStatusDTO.ManifestCode} has been accepted by you", $"{(int)HttpStatusCode.Forbidden}");
                            }
                            else
                            {
                                pickupManifestObj.ManifestStatus = manifestStatusDTO.ManifestStatus;
                                await _uow.CompleteAsync();
                            }
                        }                       
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> UpdatePreshipmentMobileStatusToPickedup(string manifestNumber, List<string> waybills)
        {
            try
            {
                string user = await _userService.GetCurrentUserId();
                if (String.IsNullOrEmpty(manifestNumber) || waybills == null || waybills.Count < 1)
                {
                    throw new GenericException("No waybill number(s) provided");
                }


                //check if manifest has already been picked
                var manifest = await _uow.PickupManifest.GetAsync(x => x.ManifestCode == manifestNumber);
                if (manifest != null && manifest.Picked)
                {
                    return true;
                }

                //check to see if all waybill in manifest was fufilled
                var unFufilled = _uow.PickupManifestWaybillMapping.GetAll().Where(x => !waybills.Contains(x.Waybill) && x.ManifestCode == manifestNumber).ToList();
                if (unFufilled.Any())
                {
                    //change contained waybill status back to shipment created
                    var waybillsToRemove = unFufilled.Select(s => s.Waybill).ToList();
                    var preshipmentWaybills = _uow.PreShipmentMobile.GetAll().Where(x => waybillsToRemove.Contains(x.Waybill)).ToList();
                    foreach (var item in preshipmentWaybills)
                    {
                        item.shipmentstatus = "Shipment created";
                    }
                    //then delete it from PickupManifestWaybillMapping
                    _uow.PickupManifestWaybillMapping.RemoveRange(unFufilled);
                }
                //update waybill to pickedup
                var waybillsToUpdateToPickup = _uow.PreShipmentMobile.GetAll().Where(x => waybills.Contains(x.Waybill)).ToList();
                var pickupRequestList = new List<MobilePickUpRequests>();
                foreach (var item in waybillsToUpdateToPickup)
                {
                    item.shipmentstatus = MobilePickUpRequestStatus.PickedUp.ToString();
                    // add preshipment record to mobilepickuprequest table
                    var request = new MobilePickUpRequests();
                    request.Status = item.shipmentstatus;
                    request.Waybill = item.Waybill;
                    request.UserId = user;
                    request.DateCreated = DateTime.Now;
                    request.DateModified = DateTime.Now;
                    pickupRequestList.Add(request);
                }
                _uow.MobilePickUpRequests.AddRange(pickupRequestList);
                await _uow.CompleteAsync();

                //send sms to all receiver
                for (int i = 0; i < waybillsToUpdateToPickup.Count; i++)
                {
                    var preshipment = waybillsToUpdateToPickup[i];
                    await _preshipmentMobileService.ScanMobileShipment(new ScanDTO
                    {
                        WaybillNumber = preshipment.Waybill,
                        ShipmentScanStatus = ShipmentScanStatus.PICKED
                    });
                    //Generate Receiver Delivery Code
                    var deliveryNumber = await _preshipmentMobileService.GenerateDeliveryCode();
                    //Send SMS To Receiver with Delivery Code
                    await _preshipmentMobileService.SendReceiverDeliveryCodeBySMS(waybillsToUpdateToPickup[i], deliveryNumber);
                }
                manifest.Picked = true;
                await _uow.CompleteAsync();
                return true;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
