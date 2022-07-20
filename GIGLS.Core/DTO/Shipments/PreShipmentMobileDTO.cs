﻿using GIGLS.Core.DTO;
using GIGLS.Core.DTO.Customers;
using GIGLS.Core.DTO.Partnership;
using GIGLS.Core.DTO.Shipments;
using GIGLS.Core.Enums;
using GIGLS.CORE.DTO;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace GIGLS.Core.DTO.Shipments
{
    public class PreShipmentMobileDTO : BaseDomainDTO
    {
        public PreShipmentMobileDTO()
        {
            SenderLocation = new LocationDTO();
            ReceiverLocation = new LocationDTO();
            PreShipmentItems = new List<PreShipmentItemMobileDTO>();
            serviceCentreLocation = new LocationDTO();
            partnerDTO = new PartnerDTO();
        }

        public int PreShipmentMobileId { get; set; }
        public new DateTime? DateCreated { get; set; }
        public string Waybill { get; set; }
        public string GroupCodeNumber { get; set; }

        //Senders' Information
        public string SenderName { get; set; }
        public string SenderStationName { get; set; }
        public string SenderPhoneNumber { get; set; }
        public decimal Value { get; set; }
        public DateTime? DeliveryTime { get; set; }
        public int SenderStationId { get; set; }
        public string InputtedSenderAddress { get; set; }
        public string SenderLocality { get; set; }

        public int ReceiverStationId { get; set; }

        public string CustomerType { get; set; }
        public string CompanyType { get; set; }
        public string CustomerCode { get; set; }
        public string SenderAddress { get; set; }

        //Receivers Information
        public string ReceiverName { get; set; }
        public string ReceiverPhoneNumber { get; set; }
        public string ReceiverEmail { get; set; }
        public string ReceiverAddress { get; set; }
        public string ReceiverCity { get; set; }
        public string ReceiverState { get; set; }
        public string ReceiverCountry { get; set; }
        public string ReceiverStationName { get; set; }
        public string ReceiverCompanyName { get; set; }
        public string ReceiverPostalCode { get; set; }
        public string ReceiverStateOrProvinceCode { get; set; }
        public string ReceiverCountryCode { get; set; }
        public decimal InternationalShippingCost { get; set; }
        public string ManufacturerCountry { get; set; }
        public string ItemDetails { get; set; }
        public CompanyMap CompanyMap { get; set; }
        public bool IsInternationalShipment { get; set; }
        public decimal DeclarationOfValueCheck { get; set; }
        public int DepartureCountryId { get; set; }
        public int DestinationCountryId { get; set; }

        public string InputtedReceiverAddress { get; set; }

        public LocationDTO SenderLocation { get; set; }      

        public  LocationDTO ReceiverLocation { get; set; }
        //Delivery Options
        public bool IsHomeDelivery { get; set; } = true;

        //General but optional

        public DateTime? ExpectedDateOfArrival { get; set; }
        public DateTime? ActualDateOfArrival { get; set; }

        //Shipment Items
        public List<PreShipmentItemMobileDTO> PreShipmentItems { get; set; } = null;

        public decimal GrandTotal { get; set; }

        //Invoice parameters: Helps generate invoice for ecomnerce customers  by customerType
        public bool IsCashOnDelivery { get; set; }
        public decimal? CashOnDeliveryAmount { get; set; }
        public decimal? ExpectedAmountToCollect { get; set; }
        public decimal? ActualAmountCollected { get; set; }


        //General Details comes with role user
        public string UserId { get; set; }

        public bool IsdeclaredVal { get; set; }


        //discount information
      

        public decimal? DiscountValue { get; set; }

        public decimal? Vat { get; set; }

        public decimal? InsuranceValue { get; set; }
        public decimal? DeliveryPrice { get; set; }
        public decimal? Total { get; set; }

        public decimal? ShipmentPackagePrice { get; set; }

        //from client
        public decimal? vatvalue_display { get; set; }
        public decimal? InvoiceDiscountValue_display { get; set; }
        public decimal? offInvoiceDiscountvalue_display { get; set; }

        //Cancelled shipment
        public bool IsCancelled { get; set; }
        public bool IsConfirmed { get; set; }
        public string DeclinedReason { get; set; }

        //Agility Validations
        public double? CalculatedTotal { get; set; } = 0;
        public bool? IsBalanceSufficient { get; set; }

        public string shipmentstatus { get; set; }
        public bool IsDelivered { get; set; }
        public int TrackingId { get; set; }

        public string VehicleType { get; set; }

        public int? ZoneMapping { get; set; }
        public List<int> DeletedItems { get; set; }

        public bool IsRated { get; set; }

        public string PartnerFirstName { get; set; }

        public string PartnerLastName { get; set; }

        public string PartnerImageUrl { get; set; }
        public string ActualReceiverFirstName { get; set; }
        public string ActualReceiverLastName { get; set; }
        public string ActualReceiverPhoneNumber { get; set; }

        public string CountryName { get; set; }
        public int CountryId { get; set; }

        public string CurrencySymbol { get; set; }

        public string CurrencyCode { get; set; }

        public int? Haulageid { get; set; }
        public ShipmentType Shipmentype { get; set; }
        public string ServiceCentreAddress { get; set; }
        public LocationDTO serviceCentreLocation { get; set; }
        public bool? IsApproved { get; set; }

        public bool? IsFromShipment { get; set; }
        public int DepartureServiceCentreId { get; set; }
        public int CustomerId { get; set; }
        
        public bool? IsEligible { get; set; }
        public bool IsCodNeeded { get; set; }
        public decimal CurrentWalletAmount { get; set; }
        public decimal ShipmentPickupPrice { get; set; }
        public PartnerDTO partnerDTO { get; set; }

        public DateTime? TimeAssigned { get; set; }
        public DateTime? TimePickedUp { get; set; }
        public DateTime? TimeDelivered { get; set; }

        public string IndentificationUrl { get; set; }
        public string DeliveryAddressImageUrl { get; set; }
        public string QRCode { get; set; }
        public bool IsScheduled { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public string SenderCode { get; set; }
        public string ReceiverCode { get; set; }
        public int DestinationServiceCenterId { get; set; }
        public bool IsBatchPickUp { get; set; }
        public string WaybillImageUrl { get; set; }
        public bool IsFromAgility { get; set; }
        public List<CustomerDTO> Customer { get; set; }
        //PickUp Options
        public PickupOptions PickupOptions { get; set; }
        public int DestinationServiceCentreId { get; set; }
        public string PaymentUrl { get; set; }
        public double? ReceiverLat { get; set; }
        public double? ReceiverLng { get; set; }
        public double? SenderLat { get; set; }
        public double? SenderLng { get; set; }
        public bool IsCoupon { get; set; }
        public string CouponCode { get; set; }
        public DeliveryType DeliveryType { get; set; }
        public PaymentType PaymentType { get; set; }
        public string CustomerName { get; set; }
        public bool IsAlpha { get; set; }
    }
    public class NewPreShipmentMobileDTO : BaseDomainDTO
    {
        public int PreShipmentMobileId { get; set; }

        //Senders' Information
        public string SenderName { get; set; }
        public string SenderPhoneNumber { get; set; }
        public int SenderStationId { get; set; }
        public string CustomerType { get; set; }
        public string CompanyType { get; set; }
        public string CustomerCode { get; set; }
        public string SenderAddress { get; set; }
        public LocationDTO SenderLocation { get; set; }

        //General Details comes with role user
        public string UserId { get; set; }

        //public string PartnerFirstName { get; set; }
        //public string PartnerLastName { get; set; }
        //public string PartnerImageUrl { get; set; }

        public decimal CurrentWalletAmount { get; set; } 

        public string CountryName { get; set; }  
        public int CountryId { get; set; } 

        public string CurrencySymbol { get; set; } 

        public string CurrencyCode { get; set; } 
        public bool? IsEligible { get; set; } 
        public bool IsCodNeeded { get; set; } 
        public ShipmentType Shipmentype { get; set; } 
        public bool? IsFromShipment { get; set; } 
        public string VehicleType { get; set; }
        public bool? IsBalanceSufficient { get; set; }  
        public int? Haulageid { get; set; }
        public decimal PickupPrice { get; set; } 

        //List of Receivers
        public List<ReceiverPreShipmentMobileDTO> Receivers { get; set; }
     
        public PartnerDTO partnerDTO { get; set; }
    }

    public class ReceiverPreShipmentMobileDTO : BaseDomainDTO
    {
        //Receivers Information
        public string ReceiverName { get; set; }
        public string ReceiverPhoneNumber { get; set; }
        public string ReceiverEmail { get; set; }
        public string ReceiverAddress { get; set; }
        public string ReceiverCity { get; set; }
        public string ReceiverState { get; set; }
        public string ReceiverCountry { get; set; }
        public int ReceiverStationId { get; set; }
        public LocationDTO ReceiverLocation { get; set; }

        public string Waybill { get; set; } 
        //Delivery Options
        public bool IsHomeDelivery { get; set; }
        public int? ZoneMapping { get; set; } 
        public decimal GrandTotal { get; set; } 
        public bool IsdeclaredVal { get; set; }  
        public decimal? DeliveryPrice { get; set; }  
        public decimal? InsuranceValue { get; set; } 
        public double? CalculatedTotal { get; set; } = 0; 
        public decimal Value { get; set; } 
        public bool IsConfirmed { get; set; } 
        public bool IsDelivered { get; set; }  
        public string shipmentstatus { get; set; } 
        public decimal ReceiverPickupPrice { get; set; }

        //discount information
        public decimal? DiscountValue { get; set; }
        //Shipment Items
        //public List<NewPreShipmentItemMobileDTO> PreShipmentItems { get; set; }
        public List<PreShipmentItemMobileDTO> preShipmentItems { get; set; }

        //Invoice parameters: Helps generate invoice for ecomnerce customers  by customerType
        //public bool IsCashOnDelivery { get; set; }
        //public decimal? CashOnDeliveryAmount { get; set; }
        //public decimal? ExpectedAmountToCollect { get; set; }
        //public decimal? ActualAmountCollected { get; set; }

    }

    public class PreShipmentMobileReportDTO
    {
        public string Waybill { get; set; }
        public string SenderName { get; set; }
        public string SenderPhoneNumber { get; set; }
        public string CompanyType { get; set; }
        public string SenderAddress { get; set; }
        public string ReceiverName { get; set; }
        public string ReceiverPhoneNumber { get; set; }
        public string ReceiverAddress { get; set; }
        public decimal GrandTotal { get; set; }
        public double CalculatedTotal { get; set; }
        public decimal? DiscountValue { get; set; }
        public decimal? Vat { get; set; }
        public decimal? DeliveryPrice { get; set; }
        public decimal? InsuranceValue { get; set; }
        public decimal? Value { get; set; }
        public string shipmentstatus { get; set; }
        public string SenderStationName { get; set; }
        public string ReceiverStationName { get; set; }
        public DateTime DateCreated { get; set; }
        public string VehicleType { get; set; }
    }

    public class PreShipmentMobileThirdPartyDTO
    {
        public string waybill { get; set; }
        public string message { get; set; }
        public bool IsBalanceSufficient { get; set; }
        public int Zone { get; set; }
        public string WaybillImage { get; set; }
        public string WaybillImageFormat { get; set; }
        public string ImagePath { get; set; }
        public string PaymentUrl { get; set; }
    }

    public class PreShipmentMobileFromAgilityDTO : BaseDomainDTO
    {
        public PreShipmentMobileFromAgilityDTO()
        {
            SenderLocation = new LocationDTO();
            ReceiverLocation = new LocationDTO();
        }

        public int PreShipmentMobileId { get; set; }
        public string Waybill { get; set; }

        //Senders' Information
        public string SenderName { get; set; }
        public string SenderStationName { get; set; }
        public string SenderPhoneNumber { get; set; }
        public decimal Value { get; set; }
        public int SenderStationId { get; set; }
        public int ReceiverStationId { get; set; }
        public string CustomerType { get; set; }
        public string CompanyType { get; set; }
        public string CustomerCode { get; set; }
        public string SenderAddress { get; set; }
        public string SenderLocality { get; set; }

        //Receivers Information
        public string ReceiverName { get; set; }
        public string ReceiverPhoneNumber { get; set; }
        public string ReceiverEmail { get; set; }
        public string ReceiverAddress { get; set; }
       
        //Delivery Options
        public bool IsHomeDelivery { get; set; }
        //Shipment Items
        public List<PreShipmentItemMobileDTO> PreShipmentItems { get; set; } = null;
        public List<ShipmentItemDTO> ShipmentItems { get; set; }
        public decimal GrandTotal { get; set; }
        //General Details comes with role user
        public string UserId { get; set; }
        public bool IsdeclaredVal { get; set; }
        //discount information
        public decimal? Discount { get; set; }
        public decimal? Vat { get; set; }
        public decimal? InsuranceValue { get; set; }
        public decimal? DeliveryPrice { get; set; }
        public decimal? Total { get; set; }
        //Agility Validations
        public double? CalculatedTotal { get; set; } = 0;
        public string VehicleType { get; set; }
        public int? ZoneMapping { get; set; }
        public string CountryName { get; set; }
        public int CountryId { get; set; }
       
        public ShipmentType Shipmentype { get; set; }
        public int DepartureServiceCentreId { get; set; }
        public LocationDTO SenderLocation { get; set; }
        public LocationDTO ReceiverLocation { get; set; }
        public List<CustomerDTO> Customer { get; set; }
        public CustomerDTO CustomerDetails { get; set; }

        public int DestinationServiceCentreId { get; set; }
        public string TransactionCode { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public PaymentType PaymentType { get; set; }
        public List<int> DeliveryOptionIds { get; set; } = new List<int>();
        public decimal? DeclarationOfValueCheck { get; set; } = 0;
        public string Description { get; set; }

        //PickUp Options
        public PickupOptions PickupOptions { get; set; }
        public List<int> PackageOptionIds { get; set; } = new List<int>();
        //public bool FromApp { get; set; }
        public decimal? PickUpCharge { get; set; }
        
    }

    public class GIGGOProgressReport
    {
        public int ShipmentCreated { get; set; }
        public int ShipmentAssignedForPickedUp { get; set; }
        public int ShipmentPickedUp { get; set; }
    }


    public class AddressDTO
    {
        public string ReceiverAddress { get; set; }
        public double? ReceiverLat { get; set; }
        public double? ReceiverLng { get; set; }
        public string ReceiverStationName { get; set; }
        public string ReceiverName { get; set; }
        public string ReceiverLGA { get; set; }
        public string ReceiverCity { get; set; }
        public string ReceiverPostalCode { get; set; }
        public string ReceiverStateOrProvinceCode { get; set; }
        public string ReceiverEmail { get; set; }
        public string ReceiverPhoneNumber { get; set; }
        public string ReceiverCountryCode { get; set; }
        public string ReceiverCountry { get; set; }
        public int ReceiverStationId { get; set; }
        public string SenderAddress { get; set; }
        public double? SenderLat { get; set; }
        public double? SenderLng { get; set; }
        public string SenderStationName { get; set; }
        public string SenderPhoneNumber { get; set; }
        public string SenderName { get; set; }
        public string SenderLGA { get; set; }
        public int DestinationCountryId { get; set; }
        public string ReceiverState { get; set; }
        public DateTime DateCreated { get; set; }
    }


    public class PreShipmentMobileTATDTO
    {
        public string Waybill { get; set; }
        public string SenderName { get; set; }
        public string SenderPhoneNumber { get; set; }
        public string SenderAddress { get; set; }
        public string ReceiverName { get; set; }
        public string ReceiverPhoneNumber { get; set; }
        public string ReceiverAddress { get; set; }
        public double CalculatedTotal { get; set; }
        public string VehicleType { get; set; }
        public DateTime DateCreated { get; set; }
        public bool IsHomeDelivery { get; set; }
        public bool IsScheduled { get; set; }
        public string SenderLocality { get; set; }
        public string shipmentstatus { get; set; }
        public string ShipmentScanStatus { get; set; }
        public DateTime LastScanDate { get; set; }
        public string OATAT { get; set; }
        public string AssignTAT { get; set; }
        public string PickupTAT { get; set; }
        public string DeliveryTAT { get; set; }
        public string AppType { get; set; }
        public string PartnerName { get; set; }
        public string PartnerType { get; set; }
        public DateTime DeliveredTime { get; set; }
    }



    public class RiderRateDTO
    {
        public string Waybill { get; set; }
        public string ShipmentScanStatus { get; set; }
        public DateTime LastScanDate { get; set; }
        public int DeliveryTAT { get; set; }
        public int AssignTAT { get; set; }
        public int PickupTAT { get; set; }
        public int AverageDeliveryTAT { get; set; }
        public int AverageAssignTAT { get; set; }
        public int AveragePickupTAT { get; set; }
        public int AverageOATAT { get; set; }
        public int Trip { get; set; }
        public DateTime LastSeen { get; set; }
        public ActivityStatus Status { get; set; }
        public double Rate { get; set; }
        public string PartnerName { get; set; }
        public string PartnerID { get; set; }
        public string PartnerEmail { get; set; }
        public string PartnerType { get; set; }
        public DateTime DeliveredTime { get; set; }
    }

    public class PendingNodeShipmentDTO
    {
        public string PartnerId { get; set; } // UserId
        public string WaybillNumber { get; set; }
    }

    public class CaptainTransactionDTO
    {
        public string PartnerCode { get; set; }
        public string PartnerName { get; set; }
        public string PartnerEmail { get; set; }
        public decimal Amount { get; set; }
        public string PartnerType { get; set; }
    }

    public class UpdateNodeMercantSubscriptionDTO
    {
        public string MerchantCode { get; set; }
        public string UserId { get; set; }
    }
}

public class UpdateNodeMercantSubscriptionDTO
{
    public string MerchantCode { get; set; }
    public string UserId { get; set; }
    public string Email { get; set; }
}

public class PreShipmentMobileMultiMerchantDTO : BaseDomainDTO
{
    public PreShipmentMobileMultiMerchantDTO()
    {
        ReceiverLocation = new LocationDTO();
        Merchants = new List<MultiMerchantDTO>();
    }
    //Receivers Information
    public string ReceiverName { get; set; }
    public string ReceiverPhoneNumber { get; set; }
    public string ReceiverEmail { get; set; }
    public string ReceiverCode { get; set; }
    public string ReceiverAddress { get; set; }
    public string ReceiverCity { get; set; }
    public string ReceiverState { get; set; }
    public string ReceiverCountry { get; set; }
    public int ReceiverStationId { get; set; }
    public string ReceiverStationName { get; set; }
    public string ReceiverCompanyName { get; set; }
    public decimal DeclarationOfValueCheck { get; set; }
    public int DepartureCountryId { get; set; }
    public int DestinationCountryId { get; set; }
    public string InputtedReceiverAddress { get; set; }
    public LocationDTO ReceiverLocation { get; set; }
    public List<MultiMerchantDTO> Merchants { get; set; }
    //PickUp Options
    public PickupOptions PickupOptions { get; set; }
    public decimal Value { get; set; }
    public DateTime? DeliveryTime { get; set; }
    public bool IsHomeDelivery { get; set; } = true;
    public string UserId { get; set; }
    public string VehicleType { get; set; }
    public int? ZoneMapping { get; set; }
    public PaymentType PaymentType { get; set; }
    public bool IsConsolidated { get; set; }
    public bool IsCashOnDelivery { get; set; }
    public decimal? CashOnDeliveryAmount { get; set; }
}


public class MultiMerchantDTO
{
    public MultiMerchantDTO()
    {

        PreShipmentItems = new List<PreShipmentItemMobileMultiMerchantDTO>();
        SenderLocation = new LocationDTO();
    }
    public string CustomerType { get; set; }
    public string CompanyType { get; set; }
    public string CustomerCode { get; set; }
    public string SenderAddress { get; set; }
    public string SenderName { get; set; }
    public string SenderStationName { get; set; }
    public string SenderPhoneNumber { get; set; }
    public int SenderStationId { get; set; }
    public string InputtedSenderAddress { get; set; }
    public string SenderLocality { get; set; }
    public LocationDTO SenderLocation { get; set; }
    public int? ZoneMapping { get; set; }
    public PaymentType PaymentType { get; set; }
    public List<PreShipmentItemMobileMultiMerchantDTO> PreShipmentItems { get; set; }

}

public class UpdateNodeMercantDetailsDTO
{
    public string MerchantCode { get; set; }
    public string Email { get; set; }
}

public class PushNotificationMessageDTO
{
    public string CustomerId { get; set; }
    public string Title { get; set; }
    public string Message { get; set; }
}
