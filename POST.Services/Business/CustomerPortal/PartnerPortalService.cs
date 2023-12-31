﻿using DocumentFormat.OpenXml.Office2013.Drawing.ChartStyle;
using POST.Core.DTO;
using POST.Core.DTO.Fleets;
using POST.Core.DTO.Partnership;
using POST.Core.DTO.Shipments;
using POST.Core.DTO.ShipmentScan;
using POST.Core.DTO.User;
using POST.Core.DTO.Wallet;
using POST.Core.Enums;
using POST.Core.IServices.CustomerPortal;
using POST.Core.IServices.Partnership;
using POST.Core.IServices.Shipments;
using POST.CORE.DTO.Report;
using POST.CORE.DTO.Shipments;
using POST.Infrastructure;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace POST.Services.Business.CustomerPortal
{
    public class PartnerPortalService : IPartnerPortalService
    {
        private readonly ICustomerPortalService _portalService;
        private readonly IShipmentService _shipmentService;
        private readonly IPartnerTransactionsService _partnerTransactionsService;
        private readonly IPreShipmentMobileService _preShipmentMobileService;

        public PartnerPortalService(ICustomerPortalService portalService, IShipmentService shipmentService, IPartnerTransactionsService partnerTransactionsService, IPreShipmentMobileService preShipmentMobileService)
        {
            _portalService = portalService;
            _shipmentService = shipmentService;
            _partnerTransactionsService = partnerTransactionsService;
            _preShipmentMobileService = preShipmentMobileService;
        }

        public async Task<object> AddManifestVisitMonitoring(ManifestVisitMonitoringDTO manifestVisitMonitoringDTO)
        {
            return await _portalService.AddManifestVisitMonitoring(manifestVisitMonitoringDTO);
        }

        public async Task<PreShipmentMobileDTO> AddMobilePickupRequest(MobilePickUpRequestsDTO pickuprequest)
        {
            return await _portalService.AddMobilePickupRequest(pickuprequest);
        }
        
        public async Task<object> AddRatings(MobileRatingDTO rating)
        {
            return await _portalService.AddRatings(rating);
        }

        public async Task AddWallet(WalletDTO wallet)
        {
            await _portalService.AddWallet(wallet);
        }

        public async Task<object> CancelShipmentWithNoCharge(CancelShipmentDTO shipment)
        {
            return await _portalService.CancelShipmentWithNoCharge(shipment);
        }

        public async Task<IdentityResult> ChangePassword(ChangePasswordDTO passwordDTO)
        {
            return await _portalService.ChangePassword(passwordDTO);
        }

        public async Task<UserDTO> CheckDetails(string user, string userchanneltype)
        {
            return await _portalService.CheckDetails(user, userchanneltype);
        }

        public async Task<bool> CreateCompany(string CustomerCode)
        {
            return await _portalService.CreateCompany(CustomerCode);
        }

        public async Task<bool> CreateCustomer(string CustomerCode)
        {
            return await _portalService.CreateCustomer(CustomerCode);
        }

        public async Task<PartnerDTO> CreatePartner(string CustomerCode)
        {
            return await _portalService.CreatePartner(CustomerCode);
        }

        public async Task<bool> EditProfile(UserDTO user)
        {
            return await _portalService.EditProfile(user);
        }

        public async Task<IdentityResult> ForgotPassword(string email, string password)
        {
            return await _portalService.ForgotPassword(email, password);
        }

        public async Task<string> Generate(int length)
        {
            return await _portalService.Generate(length);
        }

        public async Task<UserDTO> GenerateReferrerCode(UserDTO user)
        {
            return await _portalService.GenerateReferrerCode(user);
        }

        public async Task<List<string>> GetItemTypes()
        {
            return await _portalService.GetItemTypes();
        }

        public async Task<List<LogVisitReasonDTO>> GetLogVisitReasons()
        {
            return await _portalService.GetLogVisitReasons();
        }

        public async Task<List<MobilePickUpRequestsDTO>> GetMobilePickupRequest()
        {
            return await _portalService.GetMobilePickupRequest();
        }

        public async Task<Partnerdto> GetMonthlyPartnerTransactions()
        {
            return await _portalService.GetMonthlyPartnerTransactions();
        }

        public async Task<SummaryTransactionsDTO> GetPartnerWalletTransactions()
        {
            return await _portalService.GetPartnerWalletTransactions();
        }

        public async Task<PreShipmentMobileDTO> GetPreShipmentDetail(string waybill)
        {
            return await _portalService.GetPreShipmentDetail(waybill);
        }

        public async Task<MobilePriceDTO> GetPrice(PreShipmentMobileDTO preShipment)
        {
            return await _portalService.GetPrice(preShipment);
        }

        public async Task<IEnumerable<ScanStatusDTO>> GetScanStatus()
        {
            return await _portalService.GetScanStatus();
        }

        public async Task<List<MovementDispatchDTO>> GetManifestsInMovementManifestForMovementDispatch()  
        {
            return await _portalService.GetManifestsInMovementManifestForMovementDispatch();
        }

        public async Task<List<MovementDispatchDTO>> getManifestsinmovementmanifestDispatchCompleted(DateFilterCriteria dateFilterCriteria) 
        {
            return await _portalService.getManifestsinmovementmanifestDispatchCompleted(dateFilterCriteria);
        }

        public async Task<List<ManifestWaybillMappingDTO>> GetWaybillsInManifestForDispatch() 
        {
            return await _portalService.GetWaybillsInManifestForDispatch();
        }

        public async Task ReleaseShipmentForCollectionOnScanner(ShipmentCollectionDTO shipmentCollection)
        {
            await _portalService.ReleaseShipmentForCollectionOnScanner(shipmentCollection);
        }

        public async Task<SignResponseDTO> ResendOTP(UserDTO user)
        {
            return await _portalService.ResendOTP(user);
        }

        public async Task<bool> ScanMultipleShipment(List<ScanDTO> scanList)
        {
            return await _portalService.ScanMultipleShipment(scanList);
        }

        public async Task SendGenericEmailMessage(MessageType messageType, object obj)
        {
            await _portalService.SendGenericEmailMessage(messageType, obj);
        }

        public async Task<SignResponseDTO> SignUp(UserDTO user)
        {
            return await _portalService.SignUp(user);
        }

        public async Task<bool> UpdateDeliveryNumber(MobileShipmentNumberDTO detail)
        {
            throw new GenericException($"Your App version is Old, Kindly update to the latest version.", $"{(int)HttpStatusCode.Forbidden}");
            //return await _portalService.UpdateDeliveryNumber(detail);
        }

        public async Task<bool> UpdateDeliveryNumberV2(MobileShipmentNumberDTO detail)
        {
            return await _portalService.UpdateDeliveryNumberV2(detail);
        }

        //Verify Shipment's Delivery Code
        public async Task<bool> VerifyDeliveryCode(MobileShipmentNumberDTO detail)
        {
            return await _portalService.VerifyDeliveryCode(detail);
        }

        public async Task<bool> UpdateMobilePickupRequest(MobilePickUpRequestsDTO pickuprequest)
        {
            return await _portalService.UpdateMobilePickupRequest(pickuprequest);
        }

        public async Task<bool> UpdatePreShipmentMobileDetails(List<PreShipmentItemMobileDTO> preshipmentmobile)
        {
            return await _portalService.UpdatePreShipmentMobileDetails(preshipmentmobile);
        }

        public async Task<bool> UpdateReceiverDetails(PreShipmentMobileDTO receiver)
        {
            return await _portalService.UpdateReceiverDetails(receiver);
        }

        public async Task<bool> UpdateVehicleProfile(UserDTO user)
        {
            return await _portalService.UpdateVehicleProfile(user);
        }

        public async Task<UserDTO> ValidateOTP(OTPDTO otp)
        {
            return await _portalService.ValidateOTP(otp);
        }

        public async Task<List<MobilePickUpRequestsDTO>> GetAllMobilePickUpRequestsPaginated(ShipmentAndPreShipmentParamDTO shipmentAndPreShipmentParamDTO)
        {
            return await _portalService.GetAllMobilePickUpRequestsPaginated(shipmentAndPreShipmentParamDTO);
        }

        public async Task<List<PreshipmentManifestDTO>> GetAllManifestForPreShipmentMobile()
        {
            return await _portalService.GetAllManifestForPreShipmentMobile();
        }

        public async Task<bool> UpdatePreshipmentMobileStatusToPickedup(string manifestNumber, List<string> waybills)
        {
            return await _portalService.UpdatePreshipmentMobileStatusToPickedup(manifestNumber, waybills);
        }

        public async Task<bool> ReleaseMovementManifest(ReleaseMovementManifestDto valMovementManifest)
        {
            return await _shipmentService.ReleaseMovementManifest(valMovementManifest);
        }

        public async Task CreditCaptainForMovementManifestTransaction(CreditPartnerTransactionsDTO creditPartnerTransactionsDTO)
        {
             await _partnerTransactionsService.CreditCaptainForMovementManifestTransaction(creditPartnerTransactionsDTO);
        }

        public async Task RemoveShipmentFromQueue(string waybill)
        {
             await _preShipmentMobileService.RemoveShipmentFromQueue(waybill);
        }

        public async Task<CODPaymentResponse> GetTransferStatus(string craccount)
        {
           return await _portalService.GetTransferStatus(craccount);
        }
    }
}
