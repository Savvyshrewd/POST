﻿using System;
using System.Threading.Tasks;
using GIGLS.Core.DTO.Shipments;
using GIGLS.Core.IServices.Shipments;
using GIGLS.Core;
using GIGLS.Core.IServices.User;
using GIGLS.Infrastructure;
using AutoMapper;
using GIGLS.Core.Domain;
using System.Collections.Generic;
using System.Linq;
using GIGLS.Core.Enums;
using GIGLS.CORE.IServices.Shipments;
using GIGLS.Core.IServices.Business;

namespace GIGLS.Services.Implementation.Shipments
{
    public class ShipmentRerouteService : IShipmentRerouteService
    {
        private readonly IUnitOfWork _uow;
        private IUserService _userService;
        private readonly IShipmentService _shipmentService;
        private readonly IShipmentCollectionService _collectionService;
        private readonly IShipmentTrackService _shipmentTrackService;

        public ShipmentRerouteService(IUnitOfWork uow, IUserService userService,
            IShipmentService shipmentService, IShipmentCollectionService collectionService, IShipmentTrackService shipmentTrackService)
        {
            _uow = uow;
            _userService = userService;
            _shipmentService = shipmentService;
            _collectionService = collectionService;
            _shipmentTrackService = shipmentTrackService;
            MapperConfig.Initialize();
        }

        public Task<IEnumerable<ShipmentRerouteDTO>> GetRerouteShipments()
        {
            //get all shipments by servicecentre
            //var serviceCenters = await _userService.GetPriviledgeServiceCenters();
            //var shipments = await _uow.Shipment.FindAsync(s => serviceCenters.Contains(s.DepartureServiceCentreId));
            //var shipmentsWaybills = shipments.ToList().Select(a => a.Waybill).AsEnumerable();

            ////get collected shipment
            //var shipmentReturns = await _uow.ShipmentReturn.FindAsync(x => shipmentsWaybills.Contains(x.WaybillNew));
            //var shipmentReturnsDto = Mapper.Map<IEnumerable<ShipmentReturnDTO>>(shipmentReturns);
            //return await Task.FromResult(shipmentReturnsDto.OrderByDescending(x => x.DateCreated));

            //comment this below and uncomment the one above once the UI is implemented
            var shipmentReturns = _uow.ShipmentReroute.GetAll().ToList().OrderByDescending(x => x.DateCreated);
            var shipmentReturnsDto = Mapper.Map<IEnumerable<ShipmentRerouteDTO>>(shipmentReturns);
            return Task.FromResult(shipmentReturnsDto);
        }

        public async Task<ShipmentDTO> AddRerouteShipment(ShipmentDTO shipmentDTO)
        {
            try
            {
                ////1. check if shipment has already been rerouted
                if (await _uow.ShipmentReroute.ExistAsync(v => v.WaybillOld.Equals(shipmentDTO.Waybill)))
                {
                    throw new GenericException($"Shipment with waybill: {shipmentDTO.Waybill} already been rerouted.");
                }

                ////2. check if Shipment has been collected
                await _collectionService.CheckShipmentCollection(shipmentDTO.Waybill);

                ////3. Check if the user is a staff at final destination
                //Get original Shipment information
                var originalShipment = await _shipmentService.GetShipment(shipmentDTO.Waybill);
                int originalDestinationId = originalShipment.DestinationServiceCentreId;
                var serviceCenters = await _userService.GetPriviledgeServiceCenters();

                var trackInfo = await _shipmentTrackService.TrackShipment(originalShipment.Waybill);

                if (serviceCenters.Length == 1 && (serviceCenters[0] == originalDestinationId ||
                    (trackInfo.FirstOrDefault().ScanStatus.Code == ShipmentScanStatus.AST.ToString() ||
                    trackInfo.FirstOrDefault().ScanStatus.Code == ShipmentScanStatus.ARP.ToString() ||
                    trackInfo.FirstOrDefault().ScanStatus.Code == ShipmentScanStatus.APT.ToString())))
                {
                    //do nothing
                }
                else
                {
                    throw new GenericException("Error processing request. The login user is not at the final Destination nor has the right privilege");
                }

                ////5. Create new shipment
                //5.1 Get Existing Shipment information and update rerouting information
                originalShipment.DepartureServiceCentreId = shipmentDTO.DepartureServiceCentreId;
                originalShipment.DestinationServiceCentreId = shipmentDTO.DestinationServiceCentreId;

                //5.2 update Receiver information
                originalShipment.ReceiverName = shipmentDTO.ReceiverName;
                originalShipment.ReceiverPhoneNumber = shipmentDTO.ReceiverPhoneNumber;
                originalShipment.ReceiverAddress = shipmentDTO.ReceiverAddress;
                originalShipment.ReceiverEmail = shipmentDTO.ReceiverEmail;
                originalShipment.ReceiverCity = shipmentDTO.ReceiverCity;
                originalShipment.ReceiverState = shipmentDTO.ReceiverState;

                //5.3 update Shipment Items
                originalShipment.ShipmentItems = shipmentDTO.ShipmentItems;

                if (serviceCenters.Length == 1 && (serviceCenters[0] != shipmentDTO.DestinationServiceCentreId) && originalDestinationId == shipmentDTO.DestinationServiceCentreId)
                {
                    for (int i = 0; i < shipmentDTO.ShipmentItems.Count; i++)
                    {
                        originalShipment.ShipmentItems[i].Price = 0;
                    }
                    //5.4 update TotalPrice and GrandTotal
                    originalShipment.Total = 0;
                    originalShipment.GrandTotal = 0;
                    originalShipment.Vat = 0;
                    originalShipment.vatvalue_display = 0;
                    originalShipment.Insurance = 0;
                }
                else
                {
                    //5.4 update TotalPrice and GrandTotal
                    originalShipment.Total = shipmentDTO.Total;
                    originalShipment.GrandTotal = shipmentDTO.GrandTotal;
                }

                //for international shipment
                if (shipmentDTO.Waybill.Contains("AWR"))
                {
                    originalShipment.Waybill = shipmentDTO.Waybill + "R";
                }

                //5.5 Create new shipment
                var newShipment = await _shipmentService.AddShipment(originalShipment);

                ////6. create new shipment reroute
                var user = await _userService.GetCurrentUserId();
                ShipmentRerouteInitiator initiator = shipmentDTO.GrandTotal > 0 ? ShipmentRerouteInitiator.Customer : ShipmentRerouteInitiator.Staff;
                string rerouteReason = shipmentDTO.ShipmentReroute == null ? null : shipmentDTO.ShipmentReroute.RerouteReason;

                var newShipmentReroute = new ShipmentReroute
                {
                    WaybillNew = newShipment.Waybill,
                    WaybillOld = shipmentDTO.Waybill,
                    RerouteBy = user,
                    RerouteReason = rerouteReason,
                    ShipmentRerouteInitiator = initiator
                };
                _uow.ShipmentReroute.Add(newShipmentReroute);

                ////4. update shipment collection status to RerouteStatus
                var shipmentCollection = await _collectionService.GetShipmentCollectionById(shipmentDTO.Waybill);
                shipmentCollection.ShipmentScanStatus = ShipmentScanStatus.SRR;
                await _collectionService.UpdateShipmentCollectionForReturn(shipmentCollection);

                if (serviceCenters.Length == 1 && (serviceCenters[0] != shipmentDTO.DestinationServiceCentreId))
                {
                    var invoice = await _uow.Invoice.GetAsync(x => x.Waybill.Equals(shipmentDTO.Waybill));
                    invoice.Amount = 0;
                }

                //complete transaction
                await _uow.CompleteAsync();

                ////7. return new shipment 
                return newShipment;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task DeleteRerouteShipment(string waybill)
        {
            var shipmentReroute = await _uow.ShipmentReroute.GetAsync(x => x.WaybillNew.Equals(waybill) || x.WaybillOld.Equals(waybill));

            if (shipmentReroute == null)
            {
                throw new GenericException($"Waybill: {waybill} does not exist");
            }
            _uow.ShipmentReroute.Remove(shipmentReroute);
            await _uow.CompleteAsync();
        }

        public async Task<ShipmentRerouteDTO> GetRerouteShipment(string waybill)
        {
            var shipmentReroute = await _uow.ShipmentReroute.GetAsync(x => x.WaybillNew.Equals(waybill) || x.WaybillOld.Equals(waybill));

            if (shipmentReroute == null)
            {
                throw new GenericException($"Waybill: {waybill} does not exist");
            }

            var shipmentRerouteDto = Mapper.Map<ShipmentRerouteDTO>(shipmentReroute);

            var user = await _userService.GetUserById(shipmentRerouteDto.RerouteBy);

            shipmentRerouteDto.RerouteBy = user.FirstName + " " + user.LastName;

            return shipmentRerouteDto;
        }
        
        public Task UpdateRerouteShipment(ShipmentRerouteDTO rerouteDto)
        {
            throw new NotImplementedException();
        }
    }
}
