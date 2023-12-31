﻿using AutoMapper;
using GIGL.POST.Core.Domain;
using POST.Core;
using POST.Core.Domain;
using POST.Core.Domain.Wallet;
using POST.Core.DTO.Shipments;
using POST.Core.DTO.Wallet;
using POST.Core.Enums;
using POST.Core.IServices.Business;
using POST.Core.IServices.Fleets;
using POST.Core.IServices.Shipments;
using POST.Core.IServices.ShipmentScan;
using POST.Core.IServices.User;
using POST.Core.IServices.Utility;
using POST.Core.IServices.Wallet;
using POST.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace POST.Services.Business.Scanning
{
    public class ScanService : IScanService
    {
        private readonly IShipmentTrackingService _shipmentTrackingService;
        private readonly IShipmentService _shipmentService;
        private readonly IGroupWaybillNumberMappingService _groupService;
        private readonly IGroupWaybillNumberService _groupWaybill;
        private readonly IManifestService _manifestService;
        private readonly IManifestGroupWaybillNumberMappingService _groupManifest;
        private IUserService _userService;
        private readonly IScanStatusService _scanService;
        private readonly IDispatchService _dispatchService;
        private readonly ITransitWaybillNumberService _transitWaybillNumberService;
        private readonly IManifestWaybillMappingService _manifestWaybillService;
        private readonly IHUBManifestWaybillMappingService _hubmanifestWaybillService;
        private readonly IWaybillPaymentLogService _waybillPaymentLogService;
        private readonly IAutoManifestAndGroupingService _autoManifestAndGroupingService;
        private readonly IUnitOfWork _uow;

        public ScanService(IShipmentService shipmentService, IShipmentTrackingService shipmentTrackingService,
            IGroupWaybillNumberMappingService groupService, IGroupWaybillNumberService groupWaybill,
            IManifestService manifestService, IManifestGroupWaybillNumberMappingService groupManifest,
            IUserService userService, IScanStatusService scanService, IDispatchService dispatchService,
            ITransitWaybillNumberService transitWaybillNumberService, IManifestWaybillMappingService manifestWaybillService,
            IHUBManifestWaybillMappingService hubmanifestWaybillService, IWaybillPaymentLogService waybillPaymentLogService, IAutoManifestAndGroupingService autoManifestAndGroupingService, IUnitOfWork uow)
        {
            _shipmentService = shipmentService;
            _shipmentTrackingService = shipmentTrackingService;
            _groupService = groupService;
            _groupWaybill = groupWaybill;
            _manifestService = manifestService;
            _groupManifest = groupManifest;
            _userService = userService;
            _scanService = scanService;
            _dispatchService = dispatchService;
            _transitWaybillNumberService = transitWaybillNumberService;
            _manifestWaybillService = manifestWaybillService;
            _hubmanifestWaybillService = hubmanifestWaybillService;
            _waybillPaymentLogService = waybillPaymentLogService;
            _autoManifestAndGroupingService = autoManifestAndGroupingService;
            _uow = uow;
            MapperConfig.Initialize();
        }

        public async Task<bool> ScanMultipleShipment(List<ScanDTO> scanList)
        {
            bool result = false;

            //loop through and call scan for each item
            foreach (var item in scanList)
            {
                result = await ScanShipment(item);
            }
            return result;
        }

        public async Task<bool> ScanShipment(ScanDTO scan)
        {
            if (scan == null)
                return false;

            //1. check if the waybill number exists in the system
            if (scan.WaybillNumber != null)
            {
                scan.WaybillNumber = scan.WaybillNumber.Trim();
            }

            var shipment = await _shipmentService.GetShipmentForScan(scan.WaybillNumber);

            //2. check if the shipment has not been cancelled (DSC)
            if (shipment != null && shipment.IsCancelled)
            {
                //Send Email to Regional Managers whenever this occurs
                scan.CancelledOrCollected = "Cancelled";
                bool emailSentResult = await SendEmailOnAttemptedScanOfCancelledShipment(scan);

                throw new GenericException($"Shipment with waybill: {scan.WaybillNumber} already cancelled, no further scan is required!");
            }

            //2. check if the shipment has not been cancelled (DSC)
            if (shipment != null && shipment.PickupOptions == PickupOptions.HOMEDELIVERY && scan.ShipmentScanStatus == ShipmentScanStatus.AD)
            {
                throw new GenericException($"Shipment with waybill: {scan.WaybillNumber} is for Home Delivery, Kindly assign a Dispatch rider to deliver it to the customer within 24 hours!");
            }

            var serviceCenters = await _userService.GetCurrentServiceCenter();
            var currentCenter = serviceCenters[0].ServiceCentreId;
            var cashondeliveryinfo = new List<CashOnDeliveryRegisterAccount>();

            //block scanning if the waybill has been collected
            if (scan.ShipmentScanStatus != ShipmentScanStatus.SMIM)
            {
                await BlockAnyScanOnCollectedShipment(scan.WaybillNumber, scan);
            }

            string scanStatus = scan.ShipmentScanStatus.ToString();

            /////////////////////////1. Shipment
            if (shipment != null)
            {
                ////// ShipmentCheck  - CheckIfUserIsAtShipmentFinalDestination
                await CheckIfUserIsAtShipmentFinalDestination(scan, shipment.DestinationServiceCentreId);

                //If the scan status is SRC - Shipment received from Dispatch
                if (scan.ShipmentScanStatus == ShipmentScanStatus.SRC)
                {
                    //Process Shipment Return to Service centre for repackaging
                    await ProcessReturnWaybillFromDispatch(shipment.Waybill);
                    return true;
                }
                else if (scan.ShipmentScanStatus == ShipmentScanStatus.SMIM)
                {
                    //Missed shipment during transit manifest arrival
                    await ProcessMissedWaybillFromTransitManifest(shipment.Waybill, scan.ShipmentScanStatus, currentCenter, serviceCenters[0].Name);
                }
                else if (scan.ShipmentScanStatus == ShipmentScanStatus.FMS)
                {
                    //Found Missed shipment
                    await ProcessFoundMissedWaybill(shipment.Waybill, scan.ShipmentScanStatus, currentCenter, serviceCenters[0].Name);
                }
                else if (shipment.ShipmentPickupPrice > 0 && (scan.ShipmentScanStatus == ShipmentScanStatus.AST || scan.ShipmentScanStatus == ShipmentScanStatus.APT))
                {
                    //2. Check for Scan related to Transit Manifest
                    await ProcessPickUpShipment(shipment, scan, currentCenter, serviceCenters[0].Name);
                }
                else if (scan.ShipmentScanStatus == ShipmentScanStatus.DUBC && shipment.PickupOptions == PickupOptions.SERVICECENTER)
                {
                    //Scan related to Delayed pick up by customer for terminal pickup
                    await ProcessDelayedPickUpShipment(shipment, scan, currentCenter, serviceCenters[0].Name);
                    return true;
                }
                else
                {
                    //check if the waybill has not been scan for the same status before
                    var checkTrack = await _shipmentTrackingService.CheckShipmentTracking(scan.WaybillNumber, scanStatus);

                    if (!checkTrack || scan.ShipmentScanStatus.Equals(ShipmentScanStatus.AD))
                    {
                        //To handle the DHL International from Sending message at arrive final destination
                        TrackingType trackingType = TrackingType.InBound;
                        if (shipment.InternationalShipmentType == InternationalShipmentType.DHL || shipment.InternationalShipmentType == InternationalShipmentType.UPS)
                        {
                            trackingType = TrackingType.OutBound;
                        }

                        var newShipmentTracking = await _shipmentTrackingService.AddShipmentTracking(new ShipmentTrackingDTO
                        {
                            DateTime = DateTime.Now,
                            Status = scanStatus,
                            Waybill = scan.WaybillNumber,
                            isInternalShipment = shipment.isInternalShipment,
                            TrackingType = trackingType
                        }, scan.ShipmentScanStatus); ;

                        //For Store Shipment Arrive Final Destination
                        if (shipment.isInternalShipment == true && scan.ShipmentScanStatus == ShipmentScanStatus.ARF)
                        {
                            await UpdateShipmentPackageForServiceCenter(shipment);
                        }

                        if (scan.ShipmentScanStatus == ShipmentScanStatus.AISN && shipment.IsInternational == true)
                        {
                            var invoice = await _uow.Invoice.GetAsync(x => x.Waybill == shipment.Waybill);

                            if (invoice.PaymentStatus != PaymentStatus.Paid)
                            {
                                //Get the two possible payment links for Waybill(Nigeria  and US)
                                var waybillPayment = new WaybillPaymentLogDTO()
                                {
                                    Waybill = shipment.Waybill,
                                    OnlinePaymentType = OnlinePaymentType.Paystack,
                                    Email = shipment.ReceiverEmail
                                };

                                int[] listOfCountryForPayment = { 1, 207 };
                                List<string> paymentLinks = new List<string>();
                                foreach (var country in listOfCountryForPayment)
                                {
                                    waybillPayment.PaymentCountryId = country;
                                    waybillPayment.PaystackCountrySecret = "PayStackLiveSecret";
                                    var response = await _waybillPaymentLogService.AddWaybillPaymentLogForIntlShipment(waybillPayment);
                                    paymentLinks.Add(response.data.Authorization_url);
                                }

                                var shipmentDTO = Mapper.Map<ShipmentDTO>(shipment);

                                //Mail and Sms
                                await _shipmentTrackingService.SendEmailToCustomerForIntlShipmentArriveNigeria(shipmentDTO, paymentLinks);
                            }

                        }
                        //Send Email on Shipment Arrive final Destination
                        //await _shipmentTrackingService.SendEmailShipmentArriveFinalDestination(Mapper.Map<ShipmentDTO>(shipment));
                        //Send Email on Shipment Arrive final Destination for Home delivery option

                        //Email inplementation moved into the shipment tracking method on line 159
                        //if (shipment != null && shipment.PickupOptions == PickupOptions.HOMEDELIVERY && scan.ShipmentScanStatus == ShipmentScanStatus.ARF)
                        //{
                        //    await _shipmentTrackingService.SendEmailShipmentARFHomeDelivery(Mapper.Map<ShipmentDTO>(shipment));
                        //}
                        ////Send Email on Shipment Arrive final Destination for Terminal pickup option
                        //if (shipment != null && shipment.PickupOptions == PickupOptions.SERVICECENTER && scan.ShipmentScanStatus == ShipmentScanStatus.ARF)
                        //{
                        //    await _shipmentTrackingService.SendEmailShipmentARFTerminalPickup(Mapper.Map<ShipmentDTO>(shipment));
                        //}
                        return true;
                    }
                    else
                    {
                        var scanResult = await _scanService.GetScanStatusByCode(scanStatus);
                        throw new GenericException($"Shipment with waybill: {scan.WaybillNumber} already scan for { scanResult.Incident }");
                    }
                }
            }

            /////////////////////////2. GroupShipment
            // check if the group waybill number exists in the system
            var groupWaybill = await _groupWaybill.GetGroupWayBillNumberForScan(scan.WaybillNumber);
            var waybillsInGroupWaybill = new HashSet<string>();

            if (groupWaybill != null)
            {
                var groupMappingShipmentList = await _groupService.GetWaybillNumbersInGroup(scan.WaybillNumber);

                var groupShipmentList = groupMappingShipmentList.Shipments;

                //In case no shipment attached to the group waybill  
                if (groupShipmentList.Any())
                {
                    ////// GroupShipmentCheck  - CheckIfUserIsAtShipmentFinalDestination
                    foreach (var item in groupShipmentList)
                    {
                        //block scanning if any of the waybill has been collected
                        await BlockAnyScanOnCollectedShipment(item.Waybill, scan);

                        await CheckIfUserIsAtShipmentFinalDestination(scan, item.DestinationServiceCentreId);
                    }

                    foreach (var groupShipment in groupShipmentList)
                    {
                        var checkTrack = await _shipmentTrackingService.CheckShipmentTracking(groupShipment.Waybill, scanStatus);
                        if (!checkTrack)
                        {
                            //To handle the DHL International from Sending message at arrive final destination
                            TrackingType trackingType = TrackingType.InBound;
                            if (shipment != null)
                            {
                                if (groupShipment.InternationalShipmentType == InternationalShipmentType.DHL || shipment.InternationalShipmentType == InternationalShipmentType.UPS)
                                {
                                    trackingType = TrackingType.OutBound;
                                } 
                            }

                            await _shipmentTrackingService.AddShipmentTracking(new ShipmentTrackingDTO
                            {
                                DateTime = DateTime.Now,
                                Status = scanStatus,
                                Waybill = groupShipment.Waybill,
                                TrackingType = trackingType
                            }, scan.ShipmentScanStatus);
                        }

                        //add to waybillsInManifest
                        waybillsInGroupWaybill.Add(groupShipment.Waybill);
                    }
                }
                else
                {
                    throw new GenericException($"No Shipment for Group waybill: {scan.WaybillNumber} ");
                }
            }

            var manifest = await _manifestService.GetManifestCodeForScan(scan.WaybillNumber);

            //Enter only if manifest in super manifest is not found
            if (scan.ShipmentScanStatus == ShipmentScanStatus.MNT && manifest != null)
            {
                if (manifest.HasSuperManifest == true)
                {
                    manifest.SuperManifestStatus = SuperManifestStatus.Pending;
                    manifest.SuperManifestCode = null;
                }
                else
                {
                    throw new GenericException($"Invalid Scan. No Super Manifest attached to the Manifest: {scan.WaybillNumber} ");
                }
            }

            //Enter only if they want to scan a manifest "Arrived Collation Center"
            else if (scan.ShipmentScanStatus == ShipmentScanStatus.ACC && manifest != null)
            {
                //do this for super flow 
                await ScanACCForManifest(scan.WaybillNumber, scan, scan.ShipmentScanStatus.ToString());
                await CheckAndCreateManifestEntriesForSuperManifest(manifest);
            }
            else
            {
                /////////////////////////3. Manifest
                // check if the manifest number exists in the system
                //var manifest = await _manifestService.GetManifestCodeForScan(scan.WaybillNumber);
                var waybillsInManifest = new HashSet<string>();

                if (manifest != null)
                {
                    if (!manifest.IsDispatched)
                    {
                        throw new GenericException($"Manifest: {manifest.ManifestCode} was not dispatched. Kindly inform your Regional Manager");
                    }

                    if (manifest.ManifestType == ManifestType.External || manifest.ManifestType == ManifestType.Transit)
                    {
                        var groupWaybillInManifestList = await _groupManifest.GetGroupWaybillNumbersInManifest(manifest.ManifestId);

                        //In case no shipment attached to the manifest  
                        if (groupWaybillInManifestList.Any())
                        {
                            //block scanning if any of the waybill has been collected
                            foreach (var groupShipment in groupWaybillInManifestList)
                            {
                                if (groupShipment.WaybillNumbers.Any())
                                {
                                    foreach (var waybill in groupShipment.WaybillNumbers)
                                    {
                                        await BlockAnyScanOnCollectedShipment(waybill, scan);
                                    }
                                }
                            }

                            foreach (var groupShipment in groupWaybillInManifestList)
                            {
                                if (groupShipment.WaybillNumbers.Any())
                                {
                                    //add DHL shipment to list in order to exclude send message to customer
                                    var internationalShipmentList = new List<string>();

                                    ////// ManifestCheck  - CheckIfUserIsAtShipmentFinalDestination
                                    if (scan.ShipmentScanStatus == ShipmentScanStatus.ARF)
                                    {
                                        foreach (var waybill in groupShipment.WaybillNumbers)
                                        {
                                            var shipmentItem = await _shipmentService.GetShipmentForScan(waybill);

                                            if (shipment != null)
                                            {
                                                if (shipmentItem.InternationalShipmentType == InternationalShipmentType.DHL || shipment.InternationalShipmentType == InternationalShipmentType.UPS)
                                                {
                                                    internationalShipmentList.Add(shipmentItem.Waybill);
                                                } 
                                            }

                                            // For Shipment Check if user has rights to this action
                                            await CheckIfUserIsAtShipmentFinalDestination(scan, shipmentItem.DestinationServiceCentreId);
                                        }
                                    }

                                    foreach (var waybill in groupShipment.WaybillNumbers)
                                    {
                                        //All Transit scan to exist for different service centre
                                        //check already scanned manifest
                                        var checkTrack = await _shipmentTrackingService.CheckShipmentTracking(waybill, scanStatus);

                                        if (!checkTrack || scan.ShipmentScanStatus.Equals(ShipmentScanStatus.AD) || scan.ShipmentScanStatus.Equals(ShipmentScanStatus.AST)
                                            || scan.ShipmentScanStatus.Equals(ShipmentScanStatus.ARP) || scan.ShipmentScanStatus.Equals(ShipmentScanStatus.APT))
                                        {
                                            //To handle the DHL International from Sending message at arrive final destination
                                            TrackingType trackingType = TrackingType.InBound;
                                            if (internationalShipmentList.Contains(waybill))
                                            {
                                                trackingType = TrackingType.OutBound;
                                            }

                                            await _shipmentTrackingService.AddShipmentTracking(new ShipmentTrackingDTO
                                            {
                                                DateTime = DateTime.Now,
                                                Status = scanStatus,
                                                Waybill = waybill,
                                                TrackingType = trackingType
                                            }, scan.ShipmentScanStatus);
                                        }

                                        //if the shipment scan status is shipment arrive final destination then
                                        //update Dispatch Receiver and manifest receiver id
                                        if (scan.ShipmentScanStatus == ShipmentScanStatus.ARF)
                                        {
                                            var dispatch = await _dispatchService.GetDispatchManifestCode(scan.WaybillNumber);
                                            if (dispatch != null)
                                            {
                                                //get the user that login
                                                var userId = await _userService.GetCurrentUserId();
                                                var user = await _userService.GetUserById(userId);

                                                string reciever = user.FirstName + " " + user.LastName;
                                                dispatch.ReceivedBy = reciever;

                                                //update manifest also
                                                var manifestObj = await _manifestService.GetManifestByCode(scan.WaybillNumber);
                                                if (manifestObj != null)
                                                {
                                                    manifestObj.IsReceived = true;
                                                    manifestObj.ReceiverBy = userId;
                                                    await _manifestService.UpdateManifest(manifestObj.ManifestId, manifestObj);
                                                }

                                                await _dispatchService.UpdateDispatch(dispatch.DispatchId, dispatch);
                                            }
                                        }

                                        //add to waybillsInManifest
                                        waybillsInManifest.Add(waybill);
                                    }
                                }
                            }
                        }
                    }

                    //Delivery Manifest
                    if (manifest.ManifestType == ManifestType.Delivery)
                    {
                        var waybillInManifestList = await _manifestWaybillService.GetWaybillsInManifest(manifest.ManifestCode);

                        if (waybillInManifestList.Any())
                        {
                            //block scanning if any of the waybill has been collected
                            //foreach (var item in waybillInManifestList)
                            //{
                            //    await BlockAnyScanOnCollectedShipment(item.Waybill, scan);
                            //}

                            //update dispatch to scan for Shipment recieved by Courier for delivery manifest
                            if (scan.ShipmentScanStatus == ShipmentScanStatus.SRC) // ||scan.ShipmentScanStatus == ShipmentScanStatus.WC )
                            {
                                var dispatch = await _dispatchService.GetDispatchManifestCode(manifest.ManifestCode);

                                //verify the delivery manifest has been signed off
                                if (dispatch != null && dispatch.ReceivedBy != null)
                                {
                                    throw new GenericException($"This Manifest has already been signed off.");
                                }

                                if (dispatch != null && dispatch.ReceivedBy == null)
                                {
                                    //get the user that login
                                    var userId = await _userService.GetCurrentUserId();
                                    var user = await _userService.GetUserById(userId);

                                    string reciever = user.FirstName + " " + user.LastName;
                                    dispatch.ReceivedBy = reciever;

                                    //update manifest also
                                    var manifestObj = await _manifestService.GetManifestByCode(manifest.ManifestCode);
                                    if (manifestObj != null && manifestObj.ReceiverBy == null)
                                    {
                                        manifestObj.IsReceived = true;
                                        manifestObj.ReceiverBy = userId;
                                        await _manifestService.UpdateManifest(manifestObj.ManifestId, manifestObj);
                                    }

                                    await _dispatchService.UpdateDispatch(dispatch.DispatchId, dispatch);
                                }

                                //If the scan status is SRC - Shipment received from Dispatch
                                foreach (var itemWaybillDTO in waybillInManifestList)
                                {
                                    if (scan.ShipmentScanStatus == ShipmentScanStatus.SRC)
                                    {
                                        //Process Shipment Return to Service centre for repackaging
                                        await ProcessReturnWaybillFromDispatch(itemWaybillDTO.Waybill);
                                    }

                                    //get the cod info using the waybill in the itemWaybillDTO
                                    var allCODs = _uow.CashOnDeliveryRegisterAccount.GetCODAsQueryable();
                                    var allCODsResult = allCODs.Where(s => s.Waybill == itemWaybillDTO.Waybill).FirstOrDefault();

                                    //check if cod info exist in cash on delivery register account
                                    if (allCODsResult != null)
                                    {
                                        //update  cash on delivery register account
                                        cashondeliveryinfo.Add(allCODsResult);
                                    }
                                }
                            }
                            else
                            {
                                throw new GenericException($"Wrong Waybill Number {scan.WaybillNumber} ");
                            }
                        }
                        else
                        {
                            throw new GenericException($"No Shipment attached to this Manifest: {scan.WaybillNumber} ");
                        }

                        return true;
                    }

                    //HUB Manifest
                    if (manifest.ManifestType == ManifestType.HUB)
                    {
                        var waybillInHUBManifestList = await _hubmanifestWaybillService.GetWaybillsInManifest(manifest.ManifestCode);
                        if (waybillInHUBManifestList.Any())
                        {
                            List<string> waybills = new List<string>();

                            //block scanning if any of the waybill has been collected
                            foreach (var hubManifest in waybillInHUBManifestList)
                            {
                                waybills.Add(hubManifest.Waybill);
                                await BlockAnyScanOnCollectedShipment(hubManifest.Waybill, scan);
                            }

                            //if the shipment scan status is shipment arrive HUB then
                            //update Dispatch Receiver and manifest receiver id
                            if (scan.ShipmentScanStatus == ShipmentScanStatus.ARP)
                            {
                                //var waybills = waybillInHUBManifestList.Select(s => s.Waybill).ToList();

                                //Process Shipment Return to Service centre for repackaging
                                await _hubmanifestWaybillService.ReturnWaybillsInManifest(manifest.ManifestCode, waybills);

                                var dispatch = await _dispatchService.GetDispatchManifestCode(scan.WaybillNumber);
                                if (dispatch != null)
                                {
                                    //get the user that login
                                    var userId = await _userService.GetCurrentUserId();
                                    var user = await _userService.GetUserById(userId);

                                    string reciever = user.FirstName + " " + user.LastName;
                                    dispatch.ReceivedBy = reciever;

                                    //update manifest also
                                    var manifestObj = await _manifestService.GetManifestByCode(scan.WaybillNumber);
                                    if (manifestObj != null)
                                    {
                                        manifestObj.IsReceived = true;
                                        manifestObj.ReceiverBy = userId;
                                        await _manifestService.UpdateManifest(manifestObj.ManifestId, manifestObj);
                                    }

                                    await _dispatchService.UpdateDispatch(dispatch.DispatchId, dispatch);
                                }
                            }
                            return true;
                        }
                    }

                    //Update for every other scan status apart from "Arrived Collation Center"
                    manifest.SuperManifestStatus = SuperManifestStatus.RegularProcess;
                }

                //Movement Manifest
                var movementManifest = await _uow.MovementManifestNumber.GetAsync(x => x.MovementManifestCode == scan.WaybillNumber);
                //Make movement manifest available to service center
                if (movementManifest != null && scan.ShipmentScanStatus == ShipmentScanStatus.ACC)
                {

                    //call this methos to scan the manifest as arrive collation centre
                    await ScanACCForManifest(scan.WaybillNumber, scan, scanStatus);

                    await UpdateMovementManifests(movementManifest.MovementManifestCode);
                    return true;
                }

                /////////////////////////3. Super Manifest
                var result = await ScanWithSuperManifest(scan.WaybillNumber, scan, scanStatus);
                if (result.WaybillsInManifest.Any())
                {
                    waybillsInManifest = result.WaybillsInManifest;
                }

                if (shipment == null && groupWaybill == null && manifest == null && (!result.ListOfManifests.Any()))
                {
                    throw new GenericException($"Information does not exist for  {scan.WaybillNumber} ");
                }

                if (scan.ShipmentScanStatus == ShipmentScanStatus.ACC)
                {
                    if (result.ListOfManifests.Any())
                    {
                        await CheckAndCreateManifestEntriesForSuperManifest(result.ListOfManifests);
                    }
                    else if (manifest != null)
                    {
                        if (manifest.ManifestType == ManifestType.Transit && manifest.IsDispatched == true)
                        {
                            //Create Entries for Scan status with ACC
                            await CheckAndCreateManifestEntriesForSuperManifest(manifest);
                        }

                    }
                }
                else
                {
                    //////////////////////4. Check and Create Entries for Transit M0anifest
                    await CheckAndCreateEntriesForTransitManifest(scan, manifest, waybillsInManifest);
                }

                //5. Update the waybill to show transit waybill has complete transit process when it arrived Final Destination in TransitWaybill
                await CompleteTransitWaybillProcess(scan, waybillsInGroupWaybill, waybillsInManifest);

                cashondeliveryinfo.ForEach(a => a.ServiceCenterId = currentCenter);
            }


            await _uow.CompleteAsync();
            //Send mail to receiver on 
            return true;
        }

        private async Task<SuperManifestScanDTO> ScanWithSuperManifest(string superManifest, ScanDTO scan, string scanStatus)
        {
            // check if the manifest number exists in the system
            var listOfManifestsinSuperManifest = _uow.Manifest.GetAllAsQueryable().Where(x => x.SuperManifestCode == superManifest && x.SuperManifestStatus == SuperManifestStatus.Dispatched);
            var waybillsInManifest = new HashSet<string>();

            var listOfManifests = listOfManifestsinSuperManifest.Where(p => p.IsDispatched == true && p.ManifestType == ManifestType.Transit).ToList();

            foreach (var manifest in listOfManifests)
            {
                var groupWaybillInManifestList = await _groupManifest.GetGroupWaybillNumbersInManifest(manifest.ManifestId);

                //In case no shipment attached to the manifest  
                if (groupWaybillInManifestList.Any())
                {
                    //block scanning if any of the waybill has been collected
                    foreach (var groupShipment in groupWaybillInManifestList)
                    {
                        if (groupShipment.WaybillNumbers.Any())
                        {
                            foreach (var waybill in groupShipment.WaybillNumbers)
                            {
                                await BlockAnyScanOnCollectedShipment(waybill, scan);
                            }
                        }
                    }

                    foreach (var groupShipment in groupWaybillInManifestList)
                    {
                        if (groupShipment.WaybillNumbers.Any())
                        {
                            ////// ManifestCheck  - CheckIfUserIsAtShipmentFinalDestination
                            if (scan.ShipmentScanStatus == ShipmentScanStatus.ARF)
                            {
                                foreach (var waybill in groupShipment.WaybillNumbers)
                                {
                                    var shipmentItem = await _shipmentService.GetShipmentForScan(waybill);
                                    // For Shipment Check if user has rights to this action
                                    await CheckIfUserIsAtShipmentFinalDestination(scan, shipmentItem.DestinationServiceCentreId);
                                }
                            }

                            foreach (var waybill in groupShipment.WaybillNumbers)
                            {
                                //All Transit scan to exist for different service centre
                                //check already scanned manifest
                                var checkTrack = await _shipmentTrackingService.CheckShipmentTracking(waybill, scanStatus);

                                if (!checkTrack || scan.ShipmentScanStatus.Equals(ShipmentScanStatus.AD) || scan.ShipmentScanStatus.Equals(ShipmentScanStatus.AST)
                                    || scan.ShipmentScanStatus.Equals(ShipmentScanStatus.ARP) || scan.ShipmentScanStatus.Equals(ShipmentScanStatus.APT))
                                {
                                    await _shipmentTrackingService.AddShipmentTracking(new ShipmentTrackingDTO
                                    {
                                        DateTime = DateTime.Now,
                                        Status = scanStatus,
                                        Waybill = waybill,
                                    }, scan.ShipmentScanStatus);
                                }

                                //if the shipment scan status is shipment arrive final destination then
                                //update Dispatch Receiver and manifest receiver id
                                if (scan.ShipmentScanStatus == ShipmentScanStatus.ARF)
                                {
                                    var dispatch = await _dispatchService.GetDispatchManifestCode(scan.WaybillNumber);
                                    if (dispatch != null)
                                    {
                                        //get the user that login
                                        var userId = await _userService.GetCurrentUserId();
                                        var user = await _userService.GetUserById(userId);

                                        string reciever = user.FirstName + " " + user.LastName;
                                        dispatch.ReceivedBy = reciever;

                                        //update manifest also
                                        var manifestObj = await _manifestService.GetManifestByCode(scan.WaybillNumber);
                                        if (manifestObj != null)
                                        {
                                            manifestObj.IsReceived = true;
                                            manifestObj.ReceiverBy = userId;
                                            await _manifestService.UpdateManifest(manifestObj.ManifestId, manifestObj);
                                        }

                                        await _dispatchService.UpdateDispatch(dispatch.DispatchId, dispatch);
                                    }
                                }

                                //add to waybillsInManifest
                                waybillsInManifest.Add(waybill);
                            }
                        }
                    }
                }
            }

            var result = new SuperManifestScanDTO
            {
                ListOfManifests = listOfManifests,
                WaybillsInManifest = waybillsInManifest
            };

            return result;
        }

        //Scan Arrived Collation Center for Manifest
        private async Task ScanACCForManifest(string manifestCode, ScanDTO scan, string scanStatus)
        {
            string code = string.Empty;
            var movementCode = await _uow.MovementManifestNumber.GetAsync(x => x.MovementManifestCode.Equals(manifestCode));
            if (movementCode is null)
            {
                var manifestInfo = await _uow.Manifest.GetAsync(x => x.ManifestCode.Equals(manifestCode));
                if (manifestCode != null)
                {
                    code = manifestInfo.ManifestCode;
                }
            }
            else
            {
                var manifestNumber = await _uow.MovementManifestNumberMapping.GetAsync(x => x.MovementManifestCode.Equals(movementCode.MovementManifestCode));
                code = manifestNumber.ManifestNumber;
            }

           

            var manifest = await _uow.Manifest.GetAsync(x => x.ManifestCode.Equals(code));

            //foreach (var manifest in listOfManifests)
            //{
            var groupWaybillInManifestList = await _groupManifest.GetGroupWaybillNumbersInManifest(manifest.ManifestId);

            //In case no shipment attached to the manifest  
            if (groupWaybillInManifestList.Any())
            {
                //block scanning if any of the waybill has been collected
                foreach (var groupShipment in groupWaybillInManifestList)
                {
                    if (groupShipment.WaybillNumbers.Any())
                    {
                        foreach (var waybill in groupShipment.WaybillNumbers)
                        {
                            await BlockAnyScanOnCollectedShipment(waybill, scan);
                        }
                    }
                }

                foreach (var groupShipment in groupWaybillInManifestList)
                {
                    if (groupShipment.WaybillNumbers.Any())
                    {
                        ////// ManifestCheck  - CheckIfUserIsAtShipmentFinalDestination
                        if (scan.ShipmentScanStatus == ShipmentScanStatus.ARF)
                        {
                            foreach (var waybill in groupShipment.WaybillNumbers)
                            {
                                var shipmentItem = await _shipmentService.GetShipmentForScan(waybill);
                                // For Shipment Check if user has rights to this action
                                await CheckIfUserIsAtShipmentFinalDestination(scan, shipmentItem.DestinationServiceCentreId);
                            }
                        }

                        foreach (var waybill in groupShipment.WaybillNumbers)
                        {
                            //All Transit scan to exist for different service centre
                            //check already scanned manifest
                            var checkTrack = await _shipmentTrackingService.CheckShipmentTracking(waybill, scanStatus);

                            if (!checkTrack || scan.ShipmentScanStatus.Equals(ShipmentScanStatus.AD) || scan.ShipmentScanStatus.Equals(ShipmentScanStatus.AST)
                                || scan.ShipmentScanStatus.Equals(ShipmentScanStatus.ARP) || scan.ShipmentScanStatus.Equals(ShipmentScanStatus.APT))
                            {
                                await _shipmentTrackingService.AddShipmentTracking(new ShipmentTrackingDTO
                                {
                                    DateTime = DateTime.Now,
                                    Status = scanStatus,
                                    Waybill = waybill,
                                }, scan.ShipmentScanStatus);
                            }

                            //if the shipment scan status is shipment arrive final destination then
                            //update Dispatch Receiver and manifest receiver id
                            if (scan.ShipmentScanStatus == ShipmentScanStatus.ARF)
                            {
                                var dispatch = await _dispatchService.GetDispatchManifestCode(scan.WaybillNumber);
                                if (dispatch != null)
                                {
                                    //get the user that login
                                    var userId = await _userService.GetCurrentUserId();
                                    var user = await _userService.GetUserById(userId);

                                    string reciever = user.FirstName + " " + user.LastName;
                                    dispatch.ReceivedBy = reciever;

                                    //update manifest also
                                    var manifestObj = await _manifestService.GetManifestByCode(scan.WaybillNumber);
                                    if (manifestObj != null)
                                    {
                                        manifestObj.IsReceived = true;
                                        manifestObj.ReceiverBy = userId;
                                        await _manifestService.UpdateManifest(manifestObj.ManifestId, manifestObj);
                                    }

                                    await _dispatchService.UpdateDispatch(dispatch.DispatchId, dispatch);
                                }
                            }


                            if (scan.ShipmentScanStatus == ShipmentScanStatus.ACC)
                            {
                                await _shipmentTrackingService.AddShipmentTracking(new ShipmentTrackingDTO
                                {
                                    DateTime = DateTime.Now,
                                    Status = scanStatus,
                                    Waybill = waybill,
                                }, scan.ShipmentScanStatus);
                            }

                        }
                    }
                }
            }

            //if (movementCode != null && scan.ShipmentScanStatus == ShipmentScanStatus.ACC)
            //{
            //  await  _autoManifestAndGroupingService.MapMoveManifest(movementCode.MovementManifestCode);
            //}
        }

        private async Task<bool> SendEmailOnAttemptedScanOfCancelledShipment(ScanDTO scan)
        {
            //send emails
            var result = await _shipmentTrackingService.SendEmailForAttemptedScanOfCancelledShipments(scan);
            return result;
        }

        private async Task ProcessReturnWaybillFromDispatch(string waybill)
        {
            var getManifest = await _manifestWaybillService.GetActiveManifestForWaybill(waybill);

            List<string> waybills = new List<string>();
            waybills.Add(waybill);

            //call ReturnWaybillsInManifest in ManifestWaybillMappingService
            await _manifestWaybillService.ReturnWaybillsInManifest(getManifest.ManifestCode, waybills);
        }

        private async Task<bool> CheckIfUserIsAtShipmentFinalDestination(ScanDTO scan, int destinationServiceCentreId)
        {
            //1. For Shipment Check if user has rights to this action
            {
                if (scan.ShipmentScanStatus == ShipmentScanStatus.ARF || scan.ShipmentScanStatus == ShipmentScanStatus.SRC)
                {
                    //Check if the user is a staff at final destination
                    var serviceCenters = await _userService.GetPriviledgeServiceCenters();
                    if (serviceCenters.Length == 1 && serviceCenters[0] == destinationServiceCentreId)
                    {
                        //do nothing
                    }
                    else
                    {
                        //added for GWA and GWARIMPA service centres
                        //{
                        //    if (serviceCenters.Length == 1)
                        //    {
                        //        if (serviceCenters[0] == 4 || serviceCenters[0] == 294)
                        //        {
                        //            if (serviceCenters[0] == destinationServiceCentreId)
                        //            {
                        //                serviceCenters = new int[] { 4, 294 };
                        //                return true;
                        //            }
                        //        }
                        //    }
                        //}

                        throw new GenericException("Error processing request. The login user is not at the final Destination nor has the right privilege");
                    }
                }
            }
            return true;
        }

        private async Task<bool> CheckAndCreateEntriesForTransitManifest(ScanDTO scan, Manifest manifest, HashSet<string> waybillsInManifest)
        {
            var serviceCenters = await _userService.GetPriviledgeServiceCenters();
            var currentUserSercentreId = serviceCenters.Length > 0 ? serviceCenters[0] : 0;
            var currentUserId = await _userService.GetCurrentUserId();
            var groupWaybillsInManifest = new HashSet<string>();

            //1. Only scan for manifest
            if (manifest != null)
            {
                if (manifest.ManifestType == ManifestType.Transit)
                {
                    //2. Check for Scan related to Transit Manifest
                    if (scan.ShipmentScanStatus == ShipmentScanStatus.AST || scan.ShipmentScanStatus == ShipmentScanStatus.APT)
                    {
                        //3. Create new entries in TransitWaybills or update existing entries
                        foreach (var waybill in waybillsInManifest)
                        {
                            //3a. check if entry exist
                            var transitWaybillNumber = await _uow.TransitWaybillNumber.GetAsync(s => s.WaybillNumber == waybill);

                            if (transitWaybillNumber == null)
                            {
                                //3b. create new entry
                                await _transitWaybillNumberService.AddTransitWaybillNumber(
                                    new TransitWaybillNumberDTO
                                    {
                                        WaybillNumber = waybill,
                                        IsGrouped = false,
                                        ServiceCentreId = currentUserSercentreId,
                                        UserId = currentUserId
                                    }
                                );
                            }
                            else
                            {
                                //3c. update existing entry
                                transitWaybillNumber.ServiceCentreId = currentUserSercentreId;
                                transitWaybillNumber.UserId = currentUserId;
                                transitWaybillNumber.IsGrouped = false;
                                _uow.Complete();
                            }

                            //4. Update entry in GroupWaybillMapping
                            var groupWaybillNumberMapping = await _uow.GroupWaybillNumberMapping.GetAsync(s => s.WaybillNumber == waybill);
                            //groupWaybillNumberMapping.DepartureServiceCentreId = currentUserSercentreId;
                            _uow.GroupWaybillNumberMapping.Remove(groupWaybillNumberMapping); //remove waybill from mapping 12/09/2019
                            await _uow.CompleteAsync();

                            //5. Get the GroupWaybill numbers in the manifest
                            groupWaybillsInManifest.Add(groupWaybillNumberMapping.GroupWaybillNumber);
                        }

                        //6. Remove entry from ManifestGroupWaybillNumberMappingService
                        //6.1 Find the groupWaybill attached to the Manifest
                        foreach (var groupWaybill in groupWaybillsInManifest)
                        {
                            //await _groupManifest.RemoveGroupWaybillNumberFromManifest(manifest.ManifestCode, groupWaybill);

                            //Update the GroupWaybill for Transit
                            //1. Update Departure Service Centre to New Service Centre and Set HasManifested to false
                            //await _groupWaybill.ChangeDepartureServiceInGroupWaybill(currentUserSercentreId, groupWaybill);

                            //New Update on 12 Sept 2019
                            //Remove the group waybill from manifest
                            var manifestGroupWaybillNumberMapping = _uow.ManifestGroupWaybillNumberMapping.SingleOrDefault(x => x.ManifestCode == manifest.ManifestCode && x.GroupWaybillNumber == groupWaybill);
                            var groupwaybill = _uow.GroupWaybillNumber.SingleOrDefault(x => x.GroupWaybillCode == groupWaybill);

                            _uow.ManifestGroupWaybillNumberMapping.Remove(manifestGroupWaybillNumberMapping);
                            _uow.GroupWaybillNumber.Remove(groupwaybill);
                            await _uow.CompleteAsync();
                        }

                        //update the manifest to default to suolve super manifest issue
                        var updateManifest = await _uow.Manifest.GetAsync(x => x.ManifestCode == manifest.ManifestCode);
                        updateManifest.SuperManifestStatus = SuperManifestStatus.Pending;
                        await _uow.CompleteAsync();
                    }
                }
            }

            return true;
        }

        private async Task CompleteTransitWaybillProcess(ScanDTO scan, HashSet<string> waybillsInGroupWaybill, HashSet<string> waybillsInManifest)
        {
            if (scan.ShipmentScanStatus == ShipmentScanStatus.ARF)
            {
                if (waybillsInManifest.Any())
                {
                    foreach (var waybill in waybillsInManifest)
                    {
                        await CompleteWaybillInTransit(waybill);
                    }
                }
                else if (waybillsInGroupWaybill.Any())
                {
                    foreach (var waybill in waybillsInGroupWaybill)
                    {
                        await CompleteWaybillInTransit(waybill);
                    }
                }
                else
                {
                    await CompleteWaybillInTransit(scan.WaybillNumber);
                }
            }
        }

        private async Task CompleteWaybillInTransit(string waybill)
        {
            var transitWaybillNumber = await _uow.TransitWaybillNumber.GetAsync(s => s.WaybillNumber == waybill);
            if (transitWaybillNumber != null)
            {
                transitWaybillNumber.IsTransitCompleted = true;
                _uow.Complete();
            }
        }

        private async Task ProcessMissedWaybillFromTransitManifest(string waybill, ShipmentScanStatus scanStatus, int currentServiceCentre, string currentServiceCentreName)
        {
            //1. Get the GroupWaybill, Transit & Manifest
            var groupWaybillNumberMapping = _uow.GroupWaybillNumberMapping.GetAllAsQueryable().Where(x => x.WaybillNumber == waybill).ToList().LastOrDefault();

            if (groupWaybillNumberMapping != null)
            {
                //2. Remove it from the group and transit table
                _uow.GroupWaybillNumberMapping.Remove(groupWaybillNumberMapping);

                var manifestGroupwaybillMapping = await _uow.ManifestGroupWaybillNumberMapping.GetAsync(x => x.GroupWaybillNumber == groupWaybillNumberMapping.GroupWaybillNumber);
                if (manifestGroupwaybillMapping != null)
                {
                    var shipmentDTO = await _uow.Shipment.GetAsync(x => x.Waybill == waybill);

                    //4. Make it available for grouping
                    var transitWaybill = await _uow.TransitWaybillNumber.GetAsync(s => s.WaybillNumber == waybill);
                    if (transitWaybill != null)
                    {
                        transitWaybill.IsGrouped = false;
                    }
                    else
                    {
                        shipmentDTO.IsGrouped = false;
                    }

                    var newShipmentTracking = new ShipmentTrackingDTO
                    {
                        Status = scanStatus.ToString(),
                        Waybill = waybill,
                        DepartureServiceCentreId = shipmentDTO.DepartureServiceCentreId,
                        DestinationServiceCentreId = shipmentDTO.DestinationServiceCentreId,
                        Location = currentServiceCentreName,
                        Manifest = manifestGroupwaybillMapping.ManifestCode,
                        ServiceCentreId = currentServiceCentre,
                        GroupWaybill = groupWaybillNumberMapping.GroupWaybillNumber
                    };

                    await _shipmentTrackingService.AddTrackingAndSendEmailForRemovingMissingShipmentsInManifest(newShipmentTracking, scanStatus, MessageType.SMIM);
                }

                //delete the group if it the only waybill in the group and only group in the manifest
                int groupWaybill = _uow.GroupWaybillNumberMapping.GetAllAsQueryable().Where(x => x.GroupWaybillNumber == groupWaybillNumberMapping.GroupWaybillNumber).Count();

                if (groupWaybill == 0)
                {
                    var gw = await _uow.GroupWaybillNumber.GetAsync(x => x.GroupWaybillCode == groupWaybillNumberMapping.GroupWaybillNumber);
                    _uow.GroupWaybillNumber.Remove(gw);

                    //remove group from the manifest
                    _uow.ManifestGroupWaybillNumberMapping.Remove(manifestGroupwaybillMapping);

                    await _uow.CompleteAsync();
                }
            }
        }

        //think on this process
        private async Task ProcessFoundMissedWaybill(string waybill, ShipmentScanStatus scanStatus, int currentServiceCentre, string currentServiceCentreName)
        {
            var shipmentDTO = await _uow.Shipment.GetAsync(x => x.Waybill == waybill);

            var newShipmentTracking = new ShipmentTrackingDTO
            {
                DateTime = DateTime.Now,
                Status = scanStatus.ToString(),
                Waybill = waybill,
                DepartureServiceCentreId = shipmentDTO.DepartureServiceCentreId,
                DestinationServiceCentreId = shipmentDTO.DestinationServiceCentreId,
                Location = currentServiceCentreName,
                ServiceCentreId = currentServiceCentre
            };

            //add/update the transit table
            var transitWaybill = await _uow.TransitWaybillNumber.GetAsync(s => s.WaybillNumber == waybill);
            if (transitWaybill != null)
            {
                transitWaybill.ServiceCentreId = currentServiceCentre;
                transitWaybill.IsGrouped = false;
            }
            else
            {
                string user = await _userService.GetCurrentUserId();
                shipmentDTO.IsGrouped = true;

                //create new transit waybill
                var newTransit = new TransitWaybillNumber
                {
                    ServiceCentreId = currentServiceCentre,
                    WaybillNumber = waybill,
                    IsGrouped = false,
                    IsTransitCompleted = false,
                    UserId = user
                };

                _uow.TransitWaybillNumber.Add(newTransit);
            }

            //remove the waybill from the group and manifest
            //1. Get the GroupWaybill
            var groupWaybillNumberMapping = _uow.GroupWaybillNumberMapping.GetAllAsQueryable().Where(x => x.WaybillNumber == waybill).ToList().LastOrDefault();

            if (groupWaybillNumberMapping != null)
            {
                //2. Remove it from the group and transit table
                _uow.GroupWaybillNumberMapping.Remove(groupWaybillNumberMapping);

                //delete the group if it the only waybill in the group and only group in the manifest
                int groupWaybill = _uow.GroupWaybillNumberMapping.GetAllAsQueryable().Where(x => x.GroupWaybillNumber == groupWaybillNumberMapping.GroupWaybillNumber).Count();

                if (groupWaybill == 1)
                {
                    var group = await _uow.GroupWaybillNumber.GetAsync(x => x.GroupWaybillCode == groupWaybillNumberMapping.GroupWaybillNumber);
                    _uow.GroupWaybillNumber.Remove(group);

                    var manifestGroupwaybillMapping = await _uow.ManifestGroupWaybillNumberMapping.GetAsync(x => x.GroupWaybillNumber == groupWaybillNumberMapping.GroupWaybillNumber);

                    if (manifestGroupwaybillMapping != null)
                    {
                        //remove group from the manifest
                        _uow.ManifestGroupWaybillNumberMapping.Remove(manifestGroupwaybillMapping);
                    }
                }
            }

            //8.Add/Update missing(Incident), add tracking history and send message to regional manager
            await _shipmentTrackingService.AddTrackingAndSendEmailForRemovingMissingShipmentsInManifest(newShipmentTracking, scanStatus, MessageType.FMS);
        }

        private async Task ProcessPickUpShipment(Shipment shipment, ScanDTO scan, int currentUserSercentreId, string serviceCentreName)
        {
            var currentUserId = await _userService.GetCurrentUserId();

            //Only allow if the shipment has not been grouped since it was created
            if (!shipment.IsGrouped)
            {
                //1. Set the waybill as grouped in the departure service centre
                shipment.IsGrouped = true;

                //2. Add it to transit table to be group by the centre
                await _transitWaybillNumberService.AddTransitWaybillNumber(new TransitWaybillNumberDTO
                {
                    WaybillNumber = shipment.Waybill,
                    IsGrouped = false,
                    ServiceCentreId = currentUserSercentreId,
                    UserId = currentUserId
                });
            }

            string scanStatus = scan.ShipmentScanStatus.ToString();

            //3. Create a scan status for it
            await _shipmentTrackingService.AddShipmentTracking(new ShipmentTrackingDTO
            {
                DateTime = DateTime.Now,
                Status = scanStatus,
                Waybill = scan.WaybillNumber,
                User = currentUserId,
                Location = serviceCentreName,
                ServiceCentreId = currentUserSercentreId
            }, scan.ShipmentScanStatus);
        }


        private async Task ProcessDelayedPickUpShipment(Shipment shipment, ScanDTO scan, int currentUserSercentreId, string serviceCentreName)
        {
            var currentUserId = await _userService.GetCurrentUserId();

            string scanStatus = scan.ShipmentScanStatus.ToString();

            //Create a scan status for it
            await _shipmentTrackingService.AddShipmentTracking(new ShipmentTrackingDTO
            {
                DateTime = DateTime.Now,
                Status = scanStatus,
                Waybill = scan.WaybillNumber,
                User = currentUserId,
                Location = serviceCentreName,
                ServiceCentreId = currentUserSercentreId
            }, scan.ShipmentScanStatus);
        }

        /// <summary>
        /// ///////////SignOffDeliveryManifest
        /// </summary>
        /// <param name="manifest"></param>
        /// <returns></returns>
        public async Task<bool> ScanSignOffDeliveryManifest(string manifest)
        {
            //This code is not used again
            // get waybills in manifest that have not been collected by customer
            //var waybills = await _manifestWaybillService.GetWaybillsInManifest(manifest);

            // scan the Delivery Manifest with the scan code of 'ShipmentScanStatus.SRC'
            // (Mark the manifest as Received at the final service centre)  - scanning does this
            await ScanShipment(new ScanDTO()
            {
                WaybillNumber = manifest,
                ShipmentScanStatus = ShipmentScanStatus.SRC
            });

            return true;
        }


        private async Task BlockAnyScanOnCollectedShipment(string waybill, ScanDTO scan)
        {
            //check if the waybill has not been scan for (AHK) shipment collecte or Delivered status before
            var checkShipmentCollected = await _uow.Invoice.GetAsync(x => x.Waybill.Equals(waybill) && x.IsShipmentCollected == true);

            if (checkShipmentCollected != null)
            {
                var newScan = scan;
                newScan.WaybillNumber = waybill;

                //Send Email to Regional Managers whenever this occurs
                scan.CancelledOrCollected = "Collected";
                bool emailSentResult = await SendEmailOnAttemptedScanOfCancelledShipment(newScan);

                throw new GenericException($"Shipment with waybill: {newScan.WaybillNumber} already collected, no further scan is required!");
            }
        }

        private async Task<bool> CheckAndCreateManifestEntriesForSuperManifest(Manifest manifest)
        {
            var serviceCenters = await _userService.GetPriviledgeServiceCenters();
            var currentUserSercentreId = serviceCenters.Length > 0 ? serviceCenters[0] : 0;

            //1. Only scan for manifest with status "Arrived Collation Center"

            //var dispatch = await _uow.Dispatch.GetAsync(s => s.ManifestNumber == manifest.ManifestCode);
            //if (dispatch == null)
            //{
            //    throw new GenericException("Manifest has not been dispatched");
            //}

            manifest.DepartureServiceCentreId = currentUserSercentreId;
            // manifest.DestinationServiceCentreId = dispatch.DestinationServiceCenterId;
            manifest.SuperManifestStatus = SuperManifestStatus.ArrivedScan;
            manifest.DispatchedById = null;
            manifest.HasSuperManifest = false;

            await _uow.CompleteAsync();

            return true;
        }

        private async Task<bool> CheckAndCreateManifestEntriesForSuperManifest(List<Manifest> manifests)
        {
            var serviceCenters = await _userService.GetPriviledgeServiceCenters();
            var currentUserSercentreId = serviceCenters.Length > 0 ? serviceCenters[0] : 0;

            manifests.ForEach(x => x.DepartureServiceCentreId = currentUserSercentreId);
            manifests.ForEach(x => x.SuperManifestStatus = SuperManifestStatus.ArrivedScan);
            manifests.ForEach(x => x.DispatchedById = null);
            manifests.ForEach(x => x.HasSuperManifest = false);

            await _uow.CompleteAsync();
            return true;
        }

        //Update Shipment Package
        private async Task UpdateShipmentPackageForServiceCenter(Shipment newShipment)
        {
            var user = await _userService.GetCurrentUserId();
            var serviceCenterIds = await _userService.GetPriviledgeServiceCenters();
            var currentServiceCenterId = serviceCenterIds[0];

            if (newShipment.DestinationServiceCentreId != currentServiceCenterId)
            {
                throw new GenericException($"Shipment with waybill: {newShipment.Waybill} does not belong to your center!");
            }

            List<ShipmentPackagingTransactions> packageInflow = new List<ShipmentPackagingTransactions>();
            List<ServiceCenterPackage> servicePackage = new List<ServiceCenterPackage>();

            foreach (var shipmentItem in newShipment.ShipmentItems)
            {
                if (shipmentItem.ShipmentType == ShipmentType.Store)
                {
                    //use ShipmentPackagePriceId in the item table to handle this instead for description
                    //use it when creating the shipment
                    var shipmentPackage = await _uow.ShipmentPackagePrice.GetAsync(x => x.ShipmentPackagePriceId == shipmentItem.ShipmentPackagePriceId);
                    var serviceCenterPackage = await _uow.ServiceCenterPackage.GetAsync(x => x.ShipmentPackageId == shipmentPackage.ShipmentPackagePriceId && x.ServiceCenterId == currentServiceCenterId);

                    if (serviceCenterPackage == null)
                    {
                        var newshipmentPackage = new ServiceCenterPackage
                        {
                            ServiceCenterId = currentServiceCenterId,
                            ShipmentPackageId = shipmentPackage.ShipmentPackagePriceId,
                            InventoryOnHand = shipmentItem.Quantity,
                            MinimunRequired = 0,
                        };
                        servicePackage.Add(newshipmentPackage);
                    }
                    else
                    {
                        serviceCenterPackage.InventoryOnHand += shipmentItem.Quantity;
                    }

                    var newInflow = new ShipmentPackagingTransactions
                    {
                        ServiceCenterId = currentServiceCenterId,
                        ShipmentPackageId = shipmentPackage.ShipmentPackagePriceId,
                        Quantity = shipmentItem.Quantity,
                        Waybill = newShipment.Waybill,
                        UserId = user,
                        PackageTransactionType = PackageTransactionType.InflowToServiceCentre
                    };

                    packageInflow.Add(newInflow);
                }

            }

            _uow.ShipmentPackagingTransactions.AddRange(packageInflow);
            _uow.ServiceCenterPackage.AddRange(servicePackage);

            await _uow.CompleteAsync();
        }

        public async Task ItemShippedFromUKScan(string manifestCode)
        {
            var shipmentTracking = new List<ShipmentTracking>();

            var manifest = await _uow.Manifest.GetAsync(x => x.ManifestCode == manifestCode);
            if (manifest == null)
            {
                throw new GenericException($"No Manifest exists for this code: {manifest}");
            }

            var currentUser = await _userService.GetCurrentUserId();
            var userServiceCenters = await _userService.GetPriviledgeServiceCenters();

            //default sc
            if (userServiceCenters.Length <= 0)
            {
                userServiceCenters = new int[] { 0 };
                var defaultServiceCenter = await _userService.GetDefaultServiceCenter();
                userServiceCenters[0] = defaultServiceCenter.ServiceCentreId;
            }
            var serviceCenter = await _uow.ServiceCentre.GetAsync(userServiceCenters[0]);


            var groupWaybillInManifestList = await _groupManifest.GetGroupWaybillNumbersInManifest(manifest.ManifestId);

            if (groupWaybillInManifestList.Any())
            {
                foreach (var groupShipment in groupWaybillInManifestList)
                {
                    if (groupShipment.WaybillNumbers.Any())
                    {
                        foreach (var waybill in groupShipment.WaybillNumbers)
                        {
                            //Should i also add other validation for scan status like ARF?
                            var checkTrack = await _shipmentTrackingService.CheckShipmentTracking(waybill, ShipmentScanStatus.ISFUK.ToString());
                            if (!checkTrack)
                            {
                                var newTracking = new ShipmentTracking()
                                {
                                    DateTime = DateTime.Now,
                                    Status = ShipmentScanStatus.ISFUK.ToString(),
                                    Waybill = waybill,
                                    UserId = currentUser,
                                    Location = serviceCenter.Name,
                                    ServiceCentreId = serviceCenter.ServiceCentreId
                                };
                                shipmentTracking.Add(newTracking);

                                var waybillInfo = await _uow.Shipment.GetAsync(x => x.Waybill == waybill);
                                var waybillDTO = Mapper.Map<ShipmentDTO>(waybillInfo);

                                await _shipmentTrackingService.SendEmailToCustomerWhenIntlShipmentIsCargoed(waybillDTO);
                            }

                        }
                    }
                }
                manifest.CargoStatus = CargoStatus.Cargoed;
                _uow.ShipmentTracking.AddRange(shipmentTracking);
                await _uow.CompleteAsync();
            }
        }

        //Make movement manifest available to service center on the event of scan of "Arrived Collation Center"
        private async Task<bool> UpdateMovementManifests(string movementManifestCode)
        {
            try
            {

                var movementmanifestMappingList = await _uow.MovementManifestNumberMapping.FindAsync(x => x.MovementManifestCode == movementManifestCode);
                if (!movementManifestCode.Any())
                {
                    throw new GenericException($"Manifests does not exist for  Movement Manifest {movementManifestCode} ");
                }
                var movementManifestList = movementmanifestMappingList.ToList();

                var serviceCenters = await _userService.GetPriviledgeServiceCenters();
                var currentUserSercentreId = serviceCenters.Length > 0 ? serviceCenters[0] : 0;

                //Get all Manifests in Movement Manifest
                var manifestByScList = movementManifestList.Select(x => x.ManifestNumber).Distinct().ToList();
            
                //Update the status of movement status
                var listOfManifests = _uow.Manifest.GetAll().Where(s => manifestByScList.Contains(s.ManifestCode)).ToList();

                //Make manifest available to service Center
                listOfManifests.ForEach(x => x.DepartureServiceCentreId = currentUserSercentreId);
                listOfManifests.ForEach(x => x.MovementStatus = MovementStatus.NoMovement);

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