﻿using AutoMapper;
using GIGL.GIGLS.Core.Domain;
using GIGLS.Core;
using GIGLS.Core.Domain;
using GIGLS.Core.DTO;
using GIGLS.Core.DTO.Fleets;
using GIGLS.Core.DTO.ServiceCentres;
using GIGLS.Core.DTO.Shipments;
using GIGLS.Core.Enums;
using GIGLS.Core.IMessageService;
using GIGLS.Core.IServices.Shipments;
using GIGLS.Core.IServices.User;
using GIGLS.CORE.DTO.Report;
using GIGLS.CORE.DTO.Shipments;
using GIGLS.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GIGLS.Services.Implementation.Shipments
{
    public class ManifestGroupWaybillNumberMappingService : IManifestGroupWaybillNumberMappingService
    {
        private readonly IUnitOfWork _uow;
        private readonly IManifestService _manifestService;
        private readonly IGroupWaybillNumberService _groupWaybillNumberService;
        private readonly IUserService _userService;
        private readonly IManifestWaybillMappingService _manifestWaybillMappingService;
        private readonly IShipmentTrackingService _trackingService;
        private readonly IMessageSenderService _messageSenderService;


        public ManifestGroupWaybillNumberMappingService(IUnitOfWork uow,
            IManifestService manifestService, IGroupWaybillNumberService groupWaybillNumberService, IUserService userService,
            IManifestWaybillMappingService manifestWaybillMappingService, IShipmentTrackingService trackingService, IMessageSenderService messageSenderService)
        {
            _uow = uow;
            _manifestService = manifestService;
            _groupWaybillNumberService = groupWaybillNumberService;
            _userService = userService;
            _manifestWaybillMappingService = manifestWaybillMappingService;
            _trackingService = trackingService;
            _messageSenderService = messageSenderService;
            MapperConfig.Initialize();
        }

        //Get Manifest For GroupWaybillNumber
        public async Task<ManifestDTO> GetManifestForGroupWaybillNumber(int groupWaybillNumberId)
        {
            try
            {
                var groupWaybillNumberDTO = await _groupWaybillNumberService.GetGroupWayBillNumberById(groupWaybillNumberId);
                var manifestGroupWaybillNumberMapping = await _uow.ManifestGroupWaybillNumberMapping.GetAsync(x => x.GroupWaybillNumber == groupWaybillNumberDTO.GroupWaybillCode);

                if (manifestGroupWaybillNumberMapping == null)
                {
                    throw new GenericException($"No Manifest exists for this GroupWaybill Id: {groupWaybillNumberId}");
                }

                var manifestDTO = await _manifestService.GetManifestByCode(manifestGroupWaybillNumberMapping.ManifestCode);
                return manifestDTO;
            }
            catch (Exception)
            {
                throw;
            }
        }

        //Get Manifest For GroupWaybillNumber
        public async Task<ManifestDTO> GetManifestForGroupWaybillNumber(string groupWaybillNumber)
        {
            try
            {
                var groupWaybillNumberDTO = await _groupWaybillNumberService.GetGroupWayBillNumberById(groupWaybillNumber);
                var manifestGroupWaybillNumberMapping = await _uow.ManifestGroupWaybillNumberMapping.GetAsync(x => x.GroupWaybillNumber == groupWaybillNumberDTO.GroupWaybillCode);

                if (manifestGroupWaybillNumberMapping == null)
                {
                    throw new GenericException($"No Manifest exists for this GroupWaybill : {groupWaybillNumber}");
                }

                var manifestDTO = await _manifestService.GetManifestByCode(manifestGroupWaybillNumberMapping.ManifestCode);
                return manifestDTO;
            }
            catch (Exception)
            {
                throw;
            }
        }


        //Get GroupWaybillNumbers In Manifest
        public async Task<List<GroupWaybillNumberDTO>> GetGroupWaybillNumbersInManifest(int manifestId)
        {
            try
            {
                var manifestDTO = await _manifestService.GetManifestById(manifestId);
                var manifestGroupWaybillNumberMappingList = await _uow.ManifestGroupWaybillNumberMapping.FindAsync(x => x.ManifestCode == manifestDTO.ManifestCode);

                //add to list
                var resultSet = new HashSet<string>();
                List<GroupWaybillNumberDTO> resultList = new List<GroupWaybillNumberDTO>();
                foreach (var manifestGroupWaybillNumberMapping in manifestGroupWaybillNumberMappingList)
                {
                    var groupWaybillNumberDTO = await _groupWaybillNumberService.GetGroupWayBillNumberById(manifestGroupWaybillNumberMapping.GroupWaybillNumber);
                    if (resultSet.Add(groupWaybillNumberDTO.GroupWaybillCode))
                    {
                        resultList.Add(groupWaybillNumberDTO);
                    }
                }

                return resultList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<MovementManifestNumberMappingDTOTwo>> GetManifestNumbersInMovementManifest(string movementmanifestCode)
        {
            try
            {
                var movementmanifestMappingList = await _uow.MovementManifestNumberMapping.FindAsync(x => x.MovementManifestCode == movementmanifestCode);
                var movementManifestList = movementmanifestMappingList.ToList(); 

                var arrOfVals = movementManifestList.Select(s => s.ManifestNumber).ToArray();
                var dispatch = await _uow.Dispatch.FindAsync(x => arrOfVals.Contains(x.ManifestNumber));

                var dispatchList = dispatch.ToList();

                var result = movementManifestList.Join(dispatchList, arg => arg.ManifestNumber, arg => arg.ManifestNumber,
                    (first, second) => new
                    {
                        first.MovementManifestNumberMappingId,
                        first.MovementManifestCode,
                        first.ManifestNumber,
                        second.DepartureServiceCenterId,
                        second.DestinationServiceCenterId,
                        second.Departure,
                        second.Destination,
                    });

                var servicecenter = _uow.ServiceCentre.GetAll();

                var resultList = result.Select(s => new MovementManifestNumberMappingDTOTwo
                {
                    MovementManifestNumberMappingId = s.MovementManifestNumberMappingId,
                    MovementManifestCode = s.MovementManifestCode,
                    ManifestNumbers = s.ManifestNumber,
                    DepartureServiceCentreId = s.DepartureServiceCenterId,
                    DestinationServiceCentreId = s.DestinationServiceCenterId,
                    DepartureServiceCentre = servicecenter.Where(c => c.ServiceCentreId == s.DepartureServiceCenterId).FirstOrDefault(),
                    DestinationServiceCentre = servicecenter.Where(c => c.ServiceCentreId == s.DestinationServiceCenterId).FirstOrDefault()
                });

                return resultList.ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        //Get WaybillNumbers In Group 
        public async Task<List<GroupWaybillNumberDTO>> GetGroupWaybillNumbersInManifest(string manifest)
        {
            try
            {
                var manifestDTO = await _manifestService.GetManifestByCode(manifest);
                var manifestGroupWaybillNumberMappingList = await _uow.ManifestGroupWaybillNumberMapping.FindAsync(x => x.ManifestCode == manifestDTO.ManifestCode);

                //add to list
                var resultSet = new HashSet<string>();
                List<GroupWaybillNumberDTO> resultList = new List<GroupWaybillNumberDTO>();
                foreach (var manifestGroupWaybillNumberMapping in manifestGroupWaybillNumberMappingList)
                {
                    var groupWaybillNumberDTO = await _groupWaybillNumberService.GetGroupWayBillNumberById(manifestGroupWaybillNumberMapping.GroupWaybillNumber);
                    if (resultSet.Add(groupWaybillNumberDTO.GroupWaybillCode))
                    {
                        resultList.Add(groupWaybillNumberDTO);
                    }
                }

                return resultList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        //Get Manifests attached to Super Manifest
        public async Task<List<ManifestDTO>> GetManifestsInSuperManifest(string superManifestCode)
        {
            try
            {
                var manifestMappingList = await _uow.Manifest.FindAsync(x => x.SuperManifestCode == superManifestCode);

                var manifestDTOs = Mapper.Map<List<ManifestDTO>>(manifestMappingList);
                foreach (var manifest in manifestDTOs)
                {
                    var departureServiceCentre = await _uow.ServiceCentre.GetAsync(s => s.ServiceCentreId == manifest.DepartureServiceCentreId);
                    var destinationServiceCentre = await _uow.ServiceCentre.GetAsync(s => s.ServiceCentreId == manifest.DestinationServiceCentreId);

                    manifest.DepartureServiceCentre = Mapper.Map<ServiceCentreDTO>(departureServiceCentre);
                    manifest.DestinationServiceCentre = Mapper.Map<ServiceCentreDTO>(destinationServiceCentre);
                }

                return manifestDTOs;
            }
            catch (Exception)
            {
                throw;
            }
        }

        //To get manifests attached to Super Manifest and some group waybill information
        public async Task<List<ManifestDTO>> GetManifestsInSuperManifestDetails(string superManifestCode)
        {
            try
            {
                var manifests = await _uow.Manifest.FindAsync(x => x.SuperManifestCode == superManifestCode);

                //add to list
                var resultSet = new HashSet<string>();
                List<ManifestDTO> resultList = new List<ManifestDTO>();
                foreach (var manifest in manifests)
                {
                    var manifestData = await _uow.Manifest.GetAsync(x => x.ManifestCode == manifest.ManifestCode);

                    if (manifestData == null)
                    {
                        throw new GenericException("Manifest information does not exist");
                    }

                    var manifestDataDTO = Mapper.Map<ManifestDTO>(manifestData);

                    //set departure and destination service centres
                    var dept = await _uow.ServiceCentre.GetAsync(x => x.ServiceCentreId == manifestData.DepartureServiceCentreId, "Station");
                    var dest = await _uow.ServiceCentre.GetAsync(x => x.ServiceCentreId == manifestData.DestinationServiceCentreId, "Station");

                    var deptDTO = Mapper.Map<ServiceCentreDTO>(dept);
                    var destDTO = Mapper.Map<ServiceCentreDTO>(dest);

                    manifestDataDTO.DepartureServiceCentre = deptDTO;
                    manifestDataDTO.DestinationServiceCentre = destDTO;

                    if (resultSet.Add(manifestData.ManifestCode))
                    {
                        resultList.Add(manifestDataDTO);
                    }
                }

                return resultList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        //Get  30 Waybills in a list of objects containing manifests for Riders Delivery Progress Page
        public async Task<List<ManifestWaybillMappingDTO>> GetWaybillsInListOfManifest(string captainId)
        {
            try
            {
                var baselineDate = DateTime.Today.AddDays(-30);


                //1. get all dispatch for that captain -- update this to get the data from the database instead of app memory
                var dispatchResult = _uow.Dispatch.GetAll().Where(x => x.DriverDetail.Equals(captainId) && x.DateCreated >= baselineDate)
                                                            .ToList();

                var dispatchResultDTO = Mapper.Map<List<DispatchDTO>>(dispatchResult);

                //2. get the manifest waybill mapping
                var finalResult = await GetWaybillsInManifestForDispatch(dispatchResultDTO);
                return finalResult;
            }
            catch (Exception)
            {
                throw;
            }
        }

        //Get waybills according to date range selected for Riders Delivery Progress Page
        public async Task<List<ManifestWaybillMappingDTO>> GetAllWaybillsinListOfManifest(string captainId, DateFilterCriteria dateFilterCriteria)
        {
            try
            {
                var dispatchresult = await _uow.ManifestWaybillMapping.GetWaybillsinManifestMappings(captainId, dateFilterCriteria);

                //2. get the manifest waybill mapping
                var finalResult = await GetWaybillsInManifestForDispatch(dispatchresult);
                return finalResult;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<List<ManifestWaybillMappingDTO>> GetWaybillsInManifestForDispatch(IEnumerable<DispatchDTO> dispatchList)
        {
            try
            {
                List<ManifestWaybillMappingDTO> finalResult = new List<ManifestWaybillMappingDTO>();

                foreach (var dispatch in dispatchList)
                {
                    //for only delivery manifest
                    var manifest = await _uow.Manifest.GetAsync(s => s.ManifestCode == dispatch.ManifestNumber && s.ManifestType == Core.Enums.ManifestType.Delivery);

                    if (manifest != null)
                    {
                        var mwp = await _manifestWaybillMappingService.GetWaybillsInManifest(dispatch.ManifestNumber);
                        finalResult.AddRange(mwp);
                    }

                    //Assuming the want both
                    //var manifest = await _uow.Manifest.GetAsync(s => s.ManifestCode == dispatch.ManifestNumber);

                    //if (manifest.ManifestType == Core.Enums.ManifestType.Delivery)
                    //{
                    //    var deliveryManifest = await _manifestWaybillMappingService.GetWaybillsInManifest(dispatch.ManifestNumber);
                    //    finalResult.AddRange(deliveryManifest);
                    //}
                    //else
                    //{
                    //    //get all waybills in other manifest 
                    //}
                }
                return finalResult;
            }
            catch (Exception)
            {
                throw;
            }
        }

        //map groupWaybillNumber to Manifest
        public async Task MappingManifestToGroupWaybillNumber(string manifest, List<string> groupWaybillNumberList)
        {
            try
            {
                var userId = await _userService.GetCurrentUserId();

                //var manifestDTO = await _manifestService.GetManifestByCode(manifest);
                var manifestObj = await _uow.Manifest.GetAsync(x => x.ManifestCode.Equals(manifest));
                var newManifest = new Manifest();

                //validate the ids are in the system
                if (manifestObj == null)
                {
                    //var newManifest = new Manifest
                    //{
                    //    DateTime = DateTime.Now,
                    //    ManifestCode = manifest
                    //};
                    newManifest = new Manifest
                    {
                        DateTime = DateTime.Now,
                        ManifestCode = manifest
                    };
                    //_uow.Manifest.Add(newManifest);
                }
                else
                {
                    //ensure that the Manifest containing the Groupwaybill has not been dispatched
                    if (manifestObj.IsDispatched)
                    {
                        throw new GenericException($"Error: The Manifest: {manifestObj.ManifestCode} assigned to this Group Waybill has already been dispatched.");
                    }
                }

                //convert the list to HashSet to remove duplicate
                var newGroupWaybillNumberList = new HashSet<string>(groupWaybillNumberList);

                foreach (var groupWaybillNumber in newGroupWaybillNumberList)
                {
                    var groupWaybillNumberDTO = await _groupWaybillNumberService.GetGroupWayBillNumberById(groupWaybillNumber);

                    if (groupWaybillNumberDTO == null)
                    {
                        throw new GenericException($"No GroupWaybill exists for this number: {groupWaybillNumber}");
                    }

                    //check if GroupWaybill has not been added to manifest 
                    var isGroupWaybillMapped = await _uow.ManifestGroupWaybillNumberMapping.ExistAsync(x => x.ManifestCode == manifest && x.GroupWaybillNumber == groupWaybillNumber);

                    //the waybill has not been added to manifest, add it
                    if (!isGroupWaybillMapped)
                    {
                        //Add new Mapping
                        var newMapping = new ManifestGroupWaybillNumberMapping
                        {
                            ManifestCode = manifest,
                            GroupWaybillNumber = groupWaybillNumberDTO.GroupWaybillCode,
                            IsActive = true,
                            DateMapped = DateTime.Now
                        };

                        _uow.ManifestGroupWaybillNumberMapping.Add(newMapping);

                        //Update The Group Waybill HasManifest to True
                        var groupWaybill = await _uow.GroupWaybillNumber.GetAsync(groupWaybillNumberDTO.GroupWaybillNumberId);
                        groupWaybill.HasManifest = true;

                        if (manifestObj == null)
                        {
                            newManifest.DepartureServiceCentreId = groupWaybillNumberDTO.DepartureServiceCentreId;
                            _uow.Manifest.Add(newManifest);
                        }
                    }
                }

                await _uow.CompleteAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task UpdateMappingMovementManifestToManifest(string movementmanifestCode, List<string> manifestList, int destinationScId) 
        {
            try
            {
                var userId = await _userService.GetCurrentUserId();
                var serviceCenters = await _userService.GetPriviledgeServiceCenters();
                var currentServiceCentre = await _userService.GetCurrentServiceCenter();

                var manifestBySc = _uow.Manifest.GetAllAsQueryable().Where(x => x.IsDispatched == true && manifestList.Contains(x.ManifestCode));
                //&&x.MovementStatus == MovementStatus.NoMovement && serviceCenters.Contains(x.DepartureServiceCentreId)

                var manifestByScList = manifestBySc.Select(x => x.ManifestCode).Distinct().ToList();

                //remove the manifest mapping as it was there before
                var movementmanifestmappings = _uow.MovementManifestNumberMapping.GetAll().Where(s => s.MovementManifestCode == movementmanifestCode).ToList();
                _uow.MovementManifestNumberMapping.RemoveRange(movementmanifestmappings);
                await _uow.CompleteAsync();

                //convert the list to HashSet to remove duplicate
                var newManifestList = new HashSet<string>(manifestList);
                var today = DateTime.Now;

                foreach (var manifestCode in newManifestList)
                {
                    var manifest = await _uow.Manifest.GetAsync(x => x.ManifestCode == manifestCode);

                    if (manifest == null)
                    {
                        throw new GenericException($"No Manifest exists for this number: {manifestCode}");
                    }

                    //Update The Manifest 
                    manifest.MovementStatus = MovementStatus.InProgress;

                    //insert into movement manifest mapping table
                    var resultMap = new MovementManifestNumberMapping()
                    {
                        MovementManifestCode = movementmanifestCode,
                        ManifestNumber = manifestCode,
                        UserId = userId
                    };
                    _uow.MovementManifestNumberMapping.Add(resultMap);
                }
                await _uow.CompleteAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        //map Manifest to Super Manifest
        public async Task MappingMovementManifestToManifest(string movementmanifestCode, List<string> manifestList, int destinationScId)
        {
            try
            {
                var userId = await _userService.GetCurrentUserId();
                var serviceCenters = await _userService.GetPriviledgeServiceCenters();
                var currentServiceCentre = await _userService.GetCurrentServiceCenter();

                var manifestBySc = _uow.Manifest.GetAllAsQueryable().Where(x => x.IsDispatched == true && manifestList.Contains(x.ManifestCode));
                //&&x.MovementStatus == MovementStatus.NoMovement && serviceCenters.Contains(x.DepartureServiceCentreId)

                var manifestByScList = manifestBySc.Select(x => x.ManifestCode).Distinct().ToList();

                //int manifestByScListCount = manifestByScList.Count;

                //convert the list to HashSet to remove duplicate
                var newManifestList = new HashSet<string>(manifestList);
                var today = DateTime.Now;

                foreach (var manifestCode in newManifestList)
                {
                    var manifest = await _uow.Manifest.GetAsync(x => x.ManifestCode == manifestCode);

                    if (manifest == null)
                    {
                        throw new GenericException($"No Manifest exists for this number: {manifestCode}");
                    }

                    //Update The Manifest 
                    manifest.MovementStatus = MovementStatus.InProgress;

                    //insert into movement manifest mapping table
                    var resultMap = new MovementManifestNumberMapping()
                    {
                        MovementManifestCode = movementmanifestCode,
                        ManifestNumber = manifestCode,
                        UserId = userId
                    };
                    _uow.MovementManifestNumberMapping.Add(resultMap);
                }

                //create Code for validation and release code for shipment
                var driverCode = await GenerateDeliveryCode();
                var destServiceCentreCode = await GenerateDeliveryCode();

                var message = new MovementManifestMessageDTO
                {
                    MovementManifestCode = movementmanifestCode,
                    DepartureServiceCentre = currentServiceCentre[0]
                    //DestinationServiceCentre = await _userService.getServiceCenterById(destinationScId)
                };

                //message.QRCode = deliveryNumber.SenderCode;
                await SendSMSForMobileShipmentCreation(message, MessageType.MCS);

                var MovementManifestNumberResult = new MovementManifestNumber()
                {
                    DepartureServiceCentreId = serviceCenters[0],
                    DestinationServiceCentreId = destinationScId,
                    MovementManifestCode = movementmanifestCode,
                    UserId = userId,
                    MovementStatus = MovementStatus.InProgress,
                    DriverCode = driverCode,
                    DestinationServiceCentreCode = destServiceCentreCode
                };

                _uow.MovementManifestNumber.Add(MovementManifestNumberResult);
                await _uow.CompleteAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task SendSMSForMobileShipmentCreation(MovementManifestMessageDTO smsMessageExtensionDTO, MessageType messageType)
        {
            await _messageSenderService.SendMessage(messageType, EmailSmsType.SMS, smsMessageExtensionDTO);
        }

        public async Task<string> GenerateDeliveryCode()
        {
            try
            {
                int maxSize = 6;
                char[] chars = new char[54];
                string a;
                a = "abcdefghjkmnpqrstuvwxyzABCDEFGHJKMNPQRSTUVWXYZ23456789";
                chars = a.ToCharArray();
                int size = maxSize;
                byte[] data = new byte[1];
                RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();
                crypto.GetNonZeroBytes(data);
                size = maxSize;
                data = new byte[size];
                crypto.GetNonZeroBytes(data);
                StringBuilder result = new StringBuilder(size);
                foreach (byte b in data)
                { result.Append(chars[b % (chars.Length - 1)]); }
                var strippedText = result.ToString();
                var number = "DN" + strippedText.ToUpper();
                return number;
            }
            catch
            {
                throw;
            }
        }

        //map Manifest to Super Manifest
        public async Task MappingSuperManifestToManifest(string superManifest, List<string> manifestList)
        {
            try
            {
                var userId = await _userService.GetCurrentUserId();
                var serviceCenters = await _userService.GetPriviledgeServiceCenters();

                var manifestBySc = _uow.Manifest.GetAllAsQueryable().Where(x => x.HasSuperManifest == false && manifestList.Contains(x.ManifestCode) && serviceCenters.Contains(x.DepartureServiceCentreId));

                var manifestByScList = manifestBySc.Select(x => x.ManifestCode).Distinct().ToList();

                //optimise these 3 line of code. you can't fetch all the data into memory when you only need to check for boolean value
                var dispatchList = _uow.Dispatch.GetAllAsQueryable().Where(x => manifestByScList.Contains(x.ManifestNumber)).Select(x => x.DestinationId).ToList();
                var allAreSame = dispatchList.All(x => x == dispatchList.First());

                if (allAreSame == false)
                {
                    throw new GenericException($"Error: Manifest belong to different Stations. ");
                }


                int manifestByScListCount = manifestByScList.Count;
                if (manifestByScListCount == 0)
                {
                    throw new GenericException($"No manifest available for Processing");
                }

                if (manifestByScListCount != manifestList.Count)
                {
                    var result = manifestList.Where(x => !manifestByScList.Contains(x));

                    if (result.Any())
                    {
                        throw new GenericException($"Error: Super Manifest cannot be created. " +
                            $"The following manifests [{string.Join(", ", result.ToList())}] are not available for Processing");
                    }
                }

                //convert the list to HashSet to remove duplicate
                var newManifestList = new HashSet<string>(manifestList);

                foreach (var manifestCode in newManifestList)
                {
                    var manifest = await _uow.Manifest.GetAsync(x => x.ManifestCode == manifestCode);

                    if (manifest == null)
                    {
                        throw new GenericException($"No Manifest exists for this number: {manifestCode}");
                    }

                    if (manifest.SuperManifestStatus == SuperManifestStatus.Dispatched)
                    {
                        throw new GenericException($"Error: The Super Manifest: {manifest.SuperManifestCode} assigned to this {manifest.ManifestCode} has already been dispatched.");
                    }

                    //Update The Manifest 
                    manifest.HasSuperManifest = true;
                    manifest.SuperManifestCode = superManifest;
                    manifest.SuperManifestStatus = SuperManifestStatus.AssignedSuperManifest;

                }
                await _uow.CompleteAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task RemoveGroupWaybillNumberFromManifest(string manifest, string groupWaybillNumber)
        {
            try
            {
                var manifestDTO = await _manifestService.GetManifestByCode(manifest);
                var groupWaybillNumberDTO = await _groupWaybillNumberService.GetGroupWayBillNumberById(groupWaybillNumber);

                var manifestGroupWaybillNumberMapping = _uow.ManifestGroupWaybillNumberMapping.SingleOrDefault(x => x.ManifestCode == manifest && x.GroupWaybillNumber == groupWaybillNumber);
                if (manifestGroupWaybillNumberMapping != null)
                {
                    _uow.ManifestGroupWaybillNumberMapping.Remove(manifestGroupWaybillNumberMapping);

                    //set GroupWaybill HasManifest to false
                    var groupwaybill = _uow.GroupWaybillNumber.SingleOrDefault(x => x.GroupWaybillCode == groupWaybillNumber);
                    groupwaybill.HasManifest = false;
                    _uow.Complete();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        //remove groupWaybillNumber from manifest
        public async Task RemoveManifestFromMovementManifest(string manifest, string movementmanifestcode)
        {
            try
            {
                var movementmanifestDTO = await _manifestService.GetMovementManifestByCode(movementmanifestcode);
                var manifestDTO = await _manifestService.GetManifestByCode(manifest);

                var movementManifestNumberMapping = _uow.MovementManifestNumberMapping.SingleOrDefault(x => x.ManifestNumber == manifest && x.MovementManifestCode == movementmanifestcode);
                if (movementManifestNumberMapping != null)
                {
                    _uow.MovementManifestNumberMapping.Remove(movementManifestNumberMapping);
                    _uow.Complete();
                }
                 
                var movementManifestNumberMapping2 = await _manifestService.GetMovementManifestByCode(movementmanifestcode);
                if (movementManifestNumberMapping2 == null)
                {
                    var movementmanifest = _uow.MovementManifestNumber.SingleOrDefault(s => s.MovementManifestCode == movementmanifestcode || 
                    s.MovementStatus == MovementStatus.InProgress || s.MovementStatus == MovementStatus.EnRoute);

                    movementmanifest.MovementStatus = MovementStatus.ProcessEnded;
                    _uow.Complete();
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        //remove manifest from super manifest
        public async Task RemoveManifestFromSuperManifest(string superManifest, string manifest)
        {
            try
            {
                var manifestObj = _uow.Manifest.SingleOrDefault(x => x.ManifestCode == manifest && x.SuperManifestCode == superManifest);

                if (manifestObj != null)
                {
                    manifestObj.HasSuperManifest = false;
                    manifestObj.SuperManifestCode = null;
                    manifestObj.SuperManifestStatus = SuperManifestStatus.ArrivedScan;
                }

                _uow.CompleteAsync();

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<ManifestGroupWaybillNumberMappingDTO>> GetAllManifestGroupWayBillNumberMappings()
        {
            try
            {
                var serviceCenters = await _userService.GetPriviledgeServiceCenters();
                var manifestGroupWaybillMapings = await _uow.ManifestGroupWaybillNumberMapping.GetManifestGroupWaybillNumberMappings(serviceCenters);
                return manifestGroupWaybillMapings;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<ManifestGroupWaybillNumberMappingDTO>> GetAllManifestGroupWayBillNumberMappings(DateFilterCriteria dateFilterCriteria)
        {
            try
            {
                var serviceCenters = await _userService.GetPriviledgeServiceCenters();
                var manifestGroupWaybillMapings = await _uow.ManifestGroupWaybillNumberMapping.GetManifestGroupWaybillNumberMappings(serviceCenters, dateFilterCriteria);

                //group the result by manifest                
                var resultGroup = manifestGroupWaybillMapings.GroupBy(x => x.ManifestCode).ToList();
                var result = new List<ManifestGroupWaybillNumberMappingDTO>();

                foreach (var resultGrp in resultGroup)
                {
                    result.Add(resultGrp.FirstOrDefault());
                }
                if (result.Any())
                {
                    foreach (var item in result)
                    {
                        if (!item.ManifestDetails.IsBulky)
                        {
                            var group = _uow.GroupWaybillNumber.GetAllAsQueryable().Where(x => x.GroupWaybillCode == item.GroupWaybillNumber).FirstOrDefault();
                            if (group != null)
                            {
                                item.ManifestDetails.DestinationServiceCentre = _uow.ServiceCentre.GetAllAsQueryable().Where(r => r.ServiceCentreId == group.ServiceCentreId).Select(d => new ServiceCentreDTO
                                {
                                    Name = d.Name,
                                    FormattedServiceCentreName = d.FormattedServiceCentreName,
                                }).FirstOrDefault();
                            } 
                        }
                    }
                }


                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<MovementManifestNumberDTO>> GetAllManifestMovementManifestNumberMappings(DateFilterCriteria dateFilterCriteria)
        {
            try
            {
                var serviceCenters = await _userService.GetPriviledgeServiceCenters();
                var manifestManifests = await _uow.ManifestGroupWaybillNumberMapping.GetManifestMovementNumberMappings(serviceCenters, dateFilterCriteria);

                return manifestManifests;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<MovementManifestNumberDTO>> GetExpectedManifestMovementManifestNumberMappings(DateFilterCriteria dateFilterCriteria) 
        {
            try
            {
                var serviceCenters = await _userService.GetPriviledgeServiceCenters();
                var manifestManifests = await _uow.ManifestGroupWaybillNumberMapping.GetExpectedManifestMovementNumberMappings(serviceCenters, dateFilterCriteria);

                return manifestManifests;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<ManifestDTO>> GetAllManifestSuperManifestMappings(DateFilterCriteria dateFilterCriteria)
        {
            try
            {
                var serviceCenters = await _userService.GetPriviledgeServiceCenters();
                var manifestSuperManifestMappings = await _uow.ManifestGroupWaybillNumberMapping.GetManifestSuperManifestMappings(serviceCenters, dateFilterCriteria);

                //group the result by manifest                
                var resultGroup = manifestSuperManifestMappings.GroupBy(x => x.SuperManifestCode).ToList();
                var result = new List<ManifestDTO>();

                foreach (var resultGrp in resultGroup)
                {
                    result.Add(resultGrp.FirstOrDefault());
                }

                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }


        //Get Manifest For GroupWaybillNumber
        public async Task<ManifestGroupWaybillNumberMappingDTO> GetManifestForWaybill(string waybill)
        {
            //1. Get waybill in a Group Waybill
            var groupWaybillNumberMapping = await _uow.GroupWaybillNumberMapping.FindAsync(x => x.WaybillNumber == waybill);
            if (groupWaybillNumberMapping == null)
            {
                throw new GenericException($"No Manifest exists for this Waybill: {waybill}");
            }

            //check if the user is at the service centre
            var serviceCentreIds = await _userService.GetPriviledgeServiceCenters();
            string groupwaybill = null;

            if (serviceCentreIds.Length > 0)
            {
                foreach (var s in groupWaybillNumberMapping.ToList())
                {
                    if (serviceCentreIds.Contains(s.DepartureServiceCentreId))
                    {
                        groupwaybill = s.GroupWaybillNumber.ToString();
                    }
                }
            }

            //2. Use the Groupwaybill to get manifest
            var manifestGroupWaybillMapings = await _uow.ManifestGroupWaybillNumberMapping.GetManifestGroupWaybillNumberMappingsUsingGroupWaybill(groupwaybill);

            if (manifestGroupWaybillMapings == null)
            {
                throw new GenericException($"No Manifest exists for this Waybill in your service centre: {waybill}");
            }

            return manifestGroupWaybillMapings;
        }

        //Get Super Manifest For Manifest
        public async Task<ManifestDTO> GetSuperManifestForManifest(string manifest)
        {
            //1. Get manifest in a Super Manifest 
            var manifestMapping = await _uow.Manifest.GetAsync(x => x.ManifestCode == manifest && x.HasSuperManifest == true);
            if (manifestMapping == null)
            {
                throw new GenericException($"No Super Manifest exists for this Manifest: {manifest}");
            }

            //check if the user is at the service centre
            var serviceCentreIds = await _userService.GetPriviledgeServiceCenters();
            ManifestDTO manifestDTO = null;

            if (serviceCentreIds.Length > 0)
            {
                if (serviceCentreIds.Contains(manifestMapping.DepartureServiceCentreId))
                {
                    manifestDTO = Mapper.Map<ManifestDTO>(manifestMapping);

                }
                else
                {
                    throw new GenericException($"No Manifest exists for this Waybill in your service centre: {manifest}");
                }
            }
            return manifestDTO;

        }

        //Search For Manifest
        public async Task<ManifestDTO> GetManifestSearch(string manifestCode)
        {
            try
            {
                var manifestDTO = await _manifestService.GetManifestByCode(manifestCode);

                var DispatchName = await _uow.User.GetUserById(manifestDTO.DispatchedBy);
                if (DispatchName != null)
                {
                    manifestDTO.DispatchedBy = DispatchName.FirstName + " " + DispatchName.LastName;
                }

                var RecieverName = await _uow.User.GetUserById(manifestDTO.ReceiverBy);
                if (RecieverName != null)
                {
                    manifestDTO.ReceiverBy = RecieverName.FirstName + " " + RecieverName.LastName;
                }

                return manifestDTO;
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<string> MoveManifestDetailToNewManifest(string manifestCode)
        {
            string newManifestCode = null;

            //Get the manifest details
            var manifest = await _uow.Manifest.GetAsync(x => x.ManifestCode == manifestCode && x.IsDispatched == true
                && (x.ManifestType == ManifestType.External || x.ManifestType == ManifestType.Transit));

            if (manifest == null)
            {
                throw new GenericException("Manifest can not be process for conversion");
            }

            //Get Login User Details
            var serviceCenters = await _userService.GetPriviledgeServiceCenters();

            //Get ManifestGroupWaybill Detail
            var manifestGroupWaybills = await _uow.ManifestGroupWaybillNumberMapping.FindAsync(x => x.ManifestCode == manifestCode);

            if (manifestGroupWaybills.Any())
            {
                var arrGrpWaybills = manifestGroupWaybills.Select(x => x.GroupWaybillNumber).ToArray();

                //Update Departure SC to new SC on groupwaybill   
                var group = _uow.GroupWaybillNumber.GetAllAsQueryable().Where(x => arrGrpWaybills.Contains(x.GroupWaybillCode)).ToList();
                group.ForEach(x => x.DepartureServiceCentreId = serviceCenters[0]);

                //On GroupWaybillMapping,  Update Departure SC to new SC
                var groupWaybillMapping = _uow.GroupWaybillNumberMapping.GetAllAsQueryable().Where(x => arrGrpWaybills.Contains(x.GroupWaybillNumber)).ToList();
                groupWaybillMapping.ForEach(x => x.DepartureServiceCentreId = serviceCenters[0]);

                // Update Manifest GW Mapping with the new Manifest code
                //Generate new Manifest 
                ManifestDTO manifestDTO = new ManifestDTO();
                newManifestCode = await _manifestService.GenerateManifestCode(manifestDTO);

                manifestGroupWaybills.ToList().ForEach(x => x.ManifestCode = newManifestCode);

                //create new manifest
                var newManifest = new Manifest
                {
                    ManifestCode = newManifestCode,
                    DateTime = DateTime.Now
                };
                _uow.Manifest.Add(newManifest);

                //add tracking history
                var arrWaybills = groupWaybillMapping.Select(x => x.WaybillNumber).ToList();
                await ProcessScanning(arrWaybills, serviceCenters[0]);
            }

            if (newManifestCode == null)
            {
                throw new GenericException($"No Waybill was attached to the Manifest {manifestCode}");
            }

            return newManifestCode;
        }

        private async Task ProcessScanning(List<string> waybills, int serviceCentre)
        {
            var currentUserId = await _userService.GetCurrentUserId();
            var serviceCenterDetail = await _uow.ServiceCentre.GetAsync(serviceCentre);

            List<ShipmentTracking> data = new List<ShipmentTracking>();

            foreach (var waybill in waybills)
            {
                var newShipmentTracking = new ShipmentTracking
                {
                    Waybill = waybill,
                    Location = serviceCenterDetail.Name,
                    Status = ShipmentScanStatus.AST.ToString(),
                    DateTime = DateTime.Now,
                    UserId = currentUserId,
                    ServiceCentreId = serviceCentre
                };

                data.Add(newShipmentTracking);
            }

            _uow.ShipmentTracking.AddRange(data);
            await _uow.CompleteAsync();
        }

        //Get Waybill Info In Group Waybill Usin manifest
        public async Task<List<GroupWaybillAndWaybillDTO>> GetGroupWaybillDataInManifest(string manifest)
        {
            try
            {
                var manifestObj = await _uow.Manifest.GetAsync(x => x.ManifestCode == manifest);

                if (manifestObj == null)
                {
                    throw new GenericException($"No Manifest exists for this code: {manifest}");
                }

                var resultData = await GetGroupWaybillNumbersInManifest(manifestObj.ManifestId);

                var overallResult = new List<GroupWaybillAndWaybillDTO>();

                //Think of an Optimisation
                if (resultData.Any())
                {
                    foreach (var groupShipment in resultData)
                    {
                        var groupWaybillData = new GroupWaybillAndWaybillDTO()
                        {
                            GroupWaybillCode = groupShipment.GroupWaybillCode,
                            WaybillsDTO = new List<WaybillInGroupWaybillDTO>()
                        };
                        if (groupShipment.WaybillNumbers.Any())
                        {
                            foreach (var waybill in groupShipment.WaybillNumbers)
                            {
                                var shipment = await _uow.Shipment.GetAsync(x => x.Waybill == waybill);
                                var dept = await _uow.ServiceCentre.GetAsync(s => s.ServiceCentreId == shipment.DepartureServiceCentreId);
                                var dest = await _uow.ServiceCentre.GetAsync(s => s.ServiceCentreId == shipment.DestinationServiceCentreId);
                                var invoice = await _uow.Invoice.GetAsync(x => x.Waybill == waybill);
                                var newWaybillInfo = new WaybillInGroupWaybillDTO()
                                {
                                    Value = shipment.DeclarationOfValueCheck == null ? 0.00M : shipment.DeclarationOfValueCheck,
                                    Weight = shipment.ApproximateItemsWeight,
                                    Waybill = shipment.Waybill,
                                    Description = shipment.Description,
                                    DepartureServiceCentre = dept.Name,
                                    DestinationServiceCentre = dest.Name,
                                    PaymentMethod = shipment.PaymentMethod != null ? shipment.PaymentMethod : "N/A",
                                    PaymentStatus = invoice.PaymentStatus
                                };
                                groupWaybillData.WaybillsDTO.Add(newWaybillInfo);
                            }
                        }
                        overallResult.Add(groupWaybillData);
                    }
                }
                return overallResult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<AllManifestAndGroupWaybillDTO>> GetDetailForAllTypesOfManifest(string code, string type)
        {
            var result = new List<AllManifestAndGroupWaybillDTO>();
            try
            {
                if (String.IsNullOrEmpty(code) || String.IsNullOrEmpty(type))
                {
                    throw new GenericException($"invalid parameters");
                }
                if (type.ToLower() == "supermanifest")
                {
                    result = await _uow.Manifest.GetManifestsInSuperManifests(code);
                    return result;
                }
                else if(type.ToLower() == "manifest")
                {
                    result = await _uow.GroupWaybillNumberMapping.GetGroupWaybillMappings(code);
                    return result;
                }
                else if (type.ToLower() == "groupwaybill")
                {
                    result = await _uow.GroupWaybillNumberMapping.GetGroupWaybillMappings(code);
                    return result;
                }
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }



    }
}
