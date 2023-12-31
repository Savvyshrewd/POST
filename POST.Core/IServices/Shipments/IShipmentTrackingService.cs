﻿using GIGL.POST.Core.Domain;
using POST.Core.DTO;
using POST.Core.DTO.Shipments;
using POST.Core.DTO.User;
using POST.Core.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace POST.Core.IServices.Shipments
{
    public interface IShipmentTrackingService : IServiceDependencyMarker
    {
        Task<List<ShipmentTrackingDTO>> GetShipmentTrackings();
        //Task<IEnumerable<ShipmentTrackingDTO>> GetShipmentWaitingForCollection();
        Task<IEnumerable<ShipmentTrackingDTO>> GetShipmentTrackings(string waybill);
        Task<IEnumerable<ShipmentTrackingDTO>> GetShipmentTrackingsForMobile(string waybill);
        Task<ShipmentTrackingDTO> GetShipmentTrackingById(int trackingId);
        Task<object> AddShipmentTracking(ShipmentTrackingDTO tracking, ShipmentScanStatus scanStatus);
        Task UpdateShipmentTracking(int trackingId, ShipmentTrackingDTO tracking);
        Task DeleteShipmentTracking(int trackingId);
        Task<bool> CheckShipmentTracking(string waybill, string status);
        Task<bool> SendEmailForAttemptedScanOfCancelledShipments(ScanDTO scan);
        Task<bool> AddTrackingAndSendEmailForRemovingMissingShipmentsInManifest(ShipmentTrackingDTO tracking, ShipmentScanStatus scanStatus, MessageType messageType);
        Task<List<UserDTO>> GetAllRegionalManagersForServiceCentre(int currentServiceCenterId);
        Task<bool> SendEmailToCustomerForIntlShipment(Shipment shipment);
        Task<bool> SendEmailToCustomerWhenIntlShipmentIsCargoed(ShipmentDTO shipmentDTO);
        Task<bool> SendEmailToCustomerForIntlShipmentArriveNigeria(ShipmentDTO shipmentDTO, List<string> paymentLinks);
        Task<bool> AddShipmentTrackingForReceivedItems(ShipmentTrackingDTO tracking, ShipmentScanStatus scanStatus, string reqNo);
        Task<bool> SendEmailShipmentArriveFinalDestination(ShipmentDTO shipmentDTO);
        Task<bool> SendEmailShipmentARFHomeDelivery(ShipmentDTO shipmentDTO);
        Task<bool> SendEmailShipmentARFTerminalPickup(ShipmentDTO shipmentDTO);
        Task<bool> SendWhatsappMessage(ShipmentDTO shipmentDTO);
        Task<bool> SendWhatsappMessageTemporal( MessageType messageType, object tracking);
    }
}
