﻿using GIGLS.Core.Enums;
using GIGLS.Core.DTO.Customers;
using System;
using System.Collections.Generic;
using GIGLS.CORE.DTO;
using GIGLS.Core.DTO.ServiceCentres;
using GIGLS.Core.DTO.Zone;
using GIGLS.CORE.DTO.Shipments;
using GIGLS.Core.DTO.Account;
using GIGLS.Core.DTO.DHL.Enum;

namespace GIGLS.Core.DTO.Shipments
{
    public class ShipmentDTO : BaseDomainDTO
    {
        //Shipment Information
        public int ShipmentId { get; set; }
        public string SealNumber { get; set; }

        public string Waybill { get; set; }

        //Senders' Information
        public decimal Value { get; set; }
        public DateTime? DeliveryTime { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public string CustomerType { get; set; }
        public int CustomerId { get; set; }
        public string CompanyType { get; set; }
        public string CustomerCode { get; set; }

        //Receivers Information
        public int DepartureServiceCentreId { get; set; }
        public ServiceCentreDTO DepartureServiceCentre { get; set; }

        public int DestinationServiceCentreId { get; set; }
        public ServiceCentreDTO DestinationServiceCentre { get; set; }

        public string ReceiverName { get; set; }
        public string ReceiverPhoneNumber { get; set; }
        public string ReceiverEmail { get; set; }
        public string ReceiverAddress { get; set; }
        public string ReceiverCity { get; set; }
        public string ReceiverState { get; set; }
        public string ReceiverCountry { get; set; }

        //Delivery Options
        public int DeliveryOptionId { get; set; } = 1;
        public DeliveryOptionDTO DeliveryOption { get; set; }
        public List<int> DeliveryOptionIds { get; set; } = new List<int>();

        //PickUp Options
        public PickupOptions PickupOptions { get; set; }

        //General but optional
        //public bool IsDomestic { get; set; }
        public DateTime? ExpectedDateOfArrival { get; set; }
        public DateTime? ActualDateOfArrival { get; set; }

        //Shipment Items
        public List<ShipmentItemDTO> ShipmentItems { get; set; }
        public double ApproximateItemsWeight { get; set; }

        public decimal GrandTotal { get; set; }

        //Invoice parameters: Helps generate invoice for ecomnerce customers  by customerType
        public bool IsCashOnDelivery { get; set; }
        public decimal? CashOnDeliveryAmount { get; set; } = 0;

        public decimal? ExpectedAmountToCollect { get; set; } = 0;
        public decimal? ActualAmountCollected { get; set; } = 0;

        //General Details comes with role user
        public string UserId { get; set; }

        public List<CustomerDTO> Customer { get; set; }
        public CustomerDTO CustomerDetails { get; set; }

        //
        public bool IsdeclaredVal { get; set; }
        public decimal? DeclarationOfValueCheck { get; set; } = 0;

        //discount information
        public decimal? AppliedDiscount { get; set; } = 0;
        public decimal? DiscountValue { get; set; } = 0;

        public decimal? Insurance { get; set; } = 0;
        public decimal? Vat { get; set; } = 0;
        public decimal? Total { get; set; } = 0;

        public decimal ShipmentPackagePrice { get; set; }

        public decimal ShipmentPickupPrice { get; set; }
        //wallet information
        public string WalletNumber { get; set; }

        //from client
        public decimal? vatvalue_display { get; set; } = 0;
        public decimal? InvoiceDiscountValue_display { get; set; } = 0;
        public decimal? offInvoiceDiscountvalue_display { get; set; } = 0;

        //payment method
        public string PaymentMethod { get; set; }

        //ShipmentCollection
        public ShipmentCollectionDTO ShipmentCollection { get; set; }

        //Demurrage Information
        public DemurrageDTO Demurrage { get; set; }

        public bool IsCancelled { get; set; }
        public bool IsInternational { get; set; }

        //Invoice Information
        public InvoiceDTO Invoice { get; set; }

        public string Description { get; set; }

        public int DepositStatus { get; set; }

        public bool ReprintCounterStatus { get; set; }

        //Sender's Address - added for the special case of corporate customers
        public string SenderAddress { get; set; }
        public string SenderState { get; set; }

        //Cancelled Reason
        public ShipmentCancelDTO ShipmentCancel { get; set; }

        //Reroute Reason
        public ShipmentRerouteDTO ShipmentReroute { get; set; }

        public bool IsFromMobile { get; set; }
        public bool isInternalShipment { get; set; }

        //Country info
        public int DepartureCountryId { get; set; }
        public int DestinationCountryId { get; set; }
        public decimal CurrencyRatio { get; set; }

        public string ShipmentHash { get; set; }

        //Drop Off
        public string TempCode { get; set; }

        public List<int> PackageOptionIds { get; set; } = new List<int>();
        public string VehicleType { get; set; }
        public string SenderCode { get; set; }
        public string ReceiverCode { get; set; }
        public bool IsGIGGOExtension { get; set; }

        public string RequestNumber { get; set; }
        public string URL { get; set; }
        public string ItemDetails { get; set; }
        public ShipmentScanStatus ShipmentScanStatus { get; set; }
        public int TimeInSeconds { get; set; }

        public InternationalShipmentType InternationalShipmentType { get; set; }
        public bool IsClassShipment { get; set; }
        public string LGA { get; set; }
        public int ETA { get; set; }
        public string DestinationServiceCentreName { get; set; }
        public string FileNameUrl { get; set; }
        public string InternationalWayBill { get; set; }
        public int SenderStationId { get; set; }
        public int ReceiverStationId { get; set; }
        public List<WaybillChargeDTO> WaybillCharges { get; set; }

        public decimal? InternationalShippingCost { get; set; } = 0;
        public string Courier { get; set; }
    }

    public class IntlShipmentRequestDTO : BaseDomainDTO
    {
        //Shipment Information 
        public int IntlShipmentRequestId { get; set; }
        //IntlShipmentRequest

        public string RequestNumber { get; set; }

        //General Details comes with role user 
        public string UserId { get; set; }
        public string CustomerFirstName { get; set; }
        public string CustomerLastName { get; set; }
        public string CustomerType { get; set; }
        public int CustomerId { get; set; }
        public int CustomerCountryId { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhoneNumber { get; set; }
        public string CustomerCity { get; set; }
        public string CustomerState { get; set; }
        public string ItemSenderfullName { get; set; }
        public string CompanyType { get; set; }
        public int UserActiveCountryId { get; set; }
        public string CustomerCode { get; set; }
        public string ManufacturerCountry { get; set; }
        //Senders' Information
        public decimal Value { get; set; }

        //public PaymentStatus PaymentStatus { get; set; }

        //Receivers Information
        public int DestinationServiceCentreId { get; set; }
        public int DepartureServiceCentreId { get; set; }
        public virtual ServiceCentreDTO DestinationServiceCentre { get; set; }
        public virtual ServiceCentreDTO DepartureServiceCentre { get; set; }
        public virtual NewCountryDTO DepartureCountry { get; set; }
        public int DestinationCountryId { get; set; }

        public string ReceiverName { get; set; }

        public string ReceiverPhoneNumber { get; set; }

        public string ReceiverEmail { get; set; }

        public string ReceiverAddress { get; set; }

        public string ReceiverCity { get; set; }

        public string ReceiverState { get; set; }

        public string ReceiverCountry { get; set; }

        //Delivery Options 
        //public int DeliveryOptionId { get; set; }

        //public DeliveryOption DeliveryOption { get; set; }

        //PickUp Options
        public PickupOptions PickupOptions { get; set; }

        //Shipment Items
        public virtual List<IntlShipmentRequestItemDTO> ShipmentRequestItems { get; set; }
        public double ApproximateItemsWeight { get; set; }

        public decimal GrandTotal { get; set; }

        //discount information
        public decimal? Total { get; set; }

        //payment method 
        public string PaymentMethod { get; set; }

        //Sender's Address - added for the special case of corporate customers
        public string SenderAddress { get; set; }

        public string SenderState { get; set; }

        public int StationId { get; set; }

        public bool IsProcessed { get; set; }

        public string URL { get; set; }
        public string ItemDetails { get; set; }

        public bool IsMobile { get; set; }
        public bool Consolidated { get; set; }
        public bool FullyReceived { get; set; }
        public string ConsolidationId { get; set; }
        public string ItemCount { get; set; }
        public int RequestProcessingCountryId { get; set; }

    }

    public class IntlShipmentDTO : BaseDomainDTO
    {
        public int IntlShipmentRequestId { get; set; }

        public string RequestNumber { get; set; }
        public bool Consolidated { get; set; }
        public string ConsolidationId { get; set; }
        public string ItemSenderfullName { get; set; }

        //General Details comes with role user 
        public string UserId { get; set; }
        public string CustomerFirstName { get; set; }
        public string CustomerLastName { get; set; }
        public string CustomerType { get; set; }
        public int CustomerId { get; set; }
        public int CustomerCountryId { get; set; }
        public string CustomerAddress { get; set; }
        public string CustomerEmail { get; set; }
        public string CustomerPhoneNumber { get; set; }
        public string CustomerCity { get; set; }
        public string CustomerState { get; set; }
        public decimal Value { get; set; }
        //public PaymentStatus PaymentStatus { get; set; }
        //Receivers Information
        public int DestinationServiceCentreId { get; set; }
        public virtual ServiceCentreDTO DestinationServiceCentre { get; set; }
        public int DestinationCountryId { get; set; }
        public string ReceiverName { get; set; }
        public string ReceiverPhoneNumber { get; set; }
        public string ReceiverEmail { get; set; }
        public string ReceiverAddress { get; set; }
        public string ReceiverCity { get; set; }
        public string ReceiverState { get; set; }
        public string ReceiverCountry { get; set; }

        //Delivery Options 
        //public int DeliveryOptionId { get; set; }

        //public DeliveryOption DeliveryOption { get; set; }

        //PickUp Options
        public PickupOptions PickupOptions { get; set; }

        //Shipment Items
        public double ApproximateItemsWeight { get; set; }

        public decimal GrandTotal { get; set; }

        //discount information
        public decimal? Total { get; set; }

        //payment method 
        public string PaymentMethod { get; set; }

        //Sender's Address - added for the special case of corporate customers
        public string SenderAddress { get; set; }

        public string SenderState { get; set; }

        public int StationId { get; set; }

        public bool IsProcessed { get; set; }
        public int IntlShipmentRequestItemId { get; set; }
        public string Description { get; set; }
        public string ItemName { get; set; }
        public string TrackingId { get; set; }
        public string storeName { get; set; }
        public ShipmentType ShipmentType { get; set; }
        public double Weight { get; set; }
        public string Nature { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public int SerialNumber { get; set; }

        //To handle volumetric weight
        public bool IsVolumetric { get; set; }
        public double Length { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }

        public string URL { get; set; }
        public string ItemDetails { get; set; }
        public decimal ItemValue { get; set; }
        public string ItemCount { get; set; }
        public bool Received { get; set; }
        public string ReceivedBy { get; set; }
        public int RequestProcessingCountryId { get; set; }
    }

    public class InternationalShipmentDTO : BaseDomainDTO
    {
        public string ReceiverName { get; set; }
        public string ReceiverCompanyName { get; set; }
        public string ReceiverPhoneNumber { get; set; }
        public string ReceiverEmail { get; set; }
        public string ReceiverAddress { get; set; }
        public string ReceiverCity { get; set; }
        public string ReceiverState { get; set; }
        public string ReceiverCountry { get; set; }
        public string ReceiverPostalCode { get; set; }
        public string ReceiverStreetLines { get; set; }
        public string ReceiverStateOrProvinceCode { get; set; }
        public string ReceiverCountryCode { get; set; }

        //Shipment Items
        public List<ShipmentItemDTO> ShipmentItems { get; set; }
        public double ApproximateItemsWeight { get; set; }
        public decimal GrandTotal { get; set; }

        //General Details comes with role user
        //  public string UserId { get; set; }
        public CustomerDTO CustomerDetails { get; set; }
        public bool IsdeclaredVal { get; set; } = true;
        public decimal? DeclarationOfValueCheck { get; set; } = 0;

        public decimal ShipmentPackagePrice { get; set; }

        public decimal ShipmentPickupPrice { get; set; }

        //payment method
        public string PaymentMethod { get; set; }
        public bool IsInternational { get; set; } = true;

        public string Description { get; set; }

        //public int DepositStatus { get; set; }

        public bool ReprintCounterStatus { get; set; }

        //Sender's Address - added for the special case of corporate customers
        public string SenderAddress { get; set; }
        public string SenderState { get; set; }
        public bool IsFromMobile { get; set; }

        //Country info
        public int DepartureCountryId { get; set; }
        public int DestinationCountryId { get; set; }
        //  public decimal CurrencyRatio { get; set; }
        //  public string ShipmentHash { get; set; }

        public List<int> PackageOptionIds { get; set; } = new List<int>();
        public string VehicleType { get; set; }
        //public string SenderCode { get; set; }
        //   public bool IsGIGGOExtension { get; set; }

        //public string RequestNumber { get; set; }
        //public string URL { get; set; }
        public string ItemDetails { get; set; }
        public Content Content { get; set; }
        public PaymentType PaymentType { get; set; }
        public int RequestProcessingCountryId { get; set; }
        public CompanyMap CompanyMap { get; set; }
        public string ManufacturerCountry { get; set; }
        public decimal InternationalShippingCost { get; set; }
        public string Waybill { get; set; }
        public InternationalRequestType RequestType { get; set; }
    }

    public class CODShipmentDTO : BaseDomainDTO
    {
        public string Waybill { get; set; }
        public string DepartureServiceCentre { get; set; }
        public string DestinationServiceCentre { get; set; }
        public decimal CODAmount { get; set; }
        public bool IsCOD { get; set; }
        public string ShipmentStatus { get; set; }
        public string ShipmentScanStatus { get; set; }
        public string CompanyName { get; set; }
    }

    public class NewShipmentDTO
    {
        //Shipment Information
        public int ShipmentId { get; set; }
        public string Waybill { get; set; }
        //Senders' Information
        public decimal Value { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public string CustomerType { get; set; }
        public int CustomerId { get; set; }
        public string CustomerCode { get; set; }

        //Receivers Information
        public int DepartureServiceCentreId { get; set; }
        public int DestinationServiceCentreId { get; set; }
        public string ReceiverName { get; set; }
        public string ReceiverPhoneNumber { get; set; }
        public string ReceiverEmail { get; set; }
        public string ReceiverAddress { get; set; }
        public string ReceiverState { get; set; }
        public string ReceiverCity { get; set; }
        public string CompanyType { get; set; }

        //Delivery Options
        public int DeliveryOptionId { get; set; } = 1;
        public List<int> DeliveryOptionIds { get; set; } = new List<int>();
        //PickUp Options
        public PickupOptions PickupOptions { get; set; }
        //Shipment Items
        public List<NewShipmentItemDTO> ShipmentItems { get; set; }
        public decimal GrandTotal { get; set; }

        //Invoice parameters: Helps generate invoice for ecomnerce customers  by customerType
        public bool IsCashOnDelivery { get; set; }
        public decimal? CashOnDeliveryAmount { get; set; } = 0;
        public List<NewCustomerDTO> Customer { get; set; }
        public bool IsdeclaredVal { get; set; }
        public decimal? DeclarationOfValueCheck { get; set; } = 0;
        public decimal? DiscountValue { get; set; } = 0;
        public decimal? Insurance { get; set; } = 0;
        public decimal? Vat { get; set; } = 0;
        public decimal? Total { get; set; } = 0;
        //from client
        public decimal? vatvalue_display { get; set; } = 0;
        public decimal? InvoiceDiscountValue_display { get; set; } = 0;
        public decimal? offInvoiceDiscountvalue_display { get; set; } = 0;
        public string Description { get; set; }
        public bool IsFromMobile { get; set; }
        //Country info
        public int DepartureCountryId { get; set; }
        public int DestinationCountryId { get; set; }
        public List<int> PackageOptionIds { get; set; } = new List<int>();
        public int TimeInSeconds { get; set; }
        public List<WaybillChargeDTO> WaybillCharges { get; set; }
    }
    public class CargoMagayaShipmentDTO : BaseDomainDTO
    {
        public string Waybill { get; set; }
        public string DepartureServiceCentre { get; set; }
        public string DestinationServiceCentre { get; set; }
        public decimal Total { get; set; }
        public decimal GrandTotal { get; set; }
        public double ApproximateItemsWeight { get; set; }
        public string CustomerType { get; set; }
        public string UserName { get; set; }
        public bool IsCargoed { get; set; }
        public string ReceiverName { get; set; }
        public string ReceiverPhoneNumber { get; set; }
        public string ReceiverCity { get; set; }

    }

    public class ShipmentPaymentDTO
    {
        public string Email { get; set; }
        public string CustomerCode { get; set; }
        public string Waybill { get; set; }
    }

    public class CorporateShipmentDTO : BaseDomainDTO
    {
        //Shipment Information
        //Senders' Information
        public string CustomerCode { get; set; }
        //Receivers Information
        public int DepartureServiceCentreId { get; set; }
        public int DestinationServiceCentreId { get; set; }
        public string ReceiverName { get; set; }
        public string ReceiverPhoneNumber { get; set; }
        public string ReceiverEmail { get; set; }
        public string ReceiverAddress { get; set; }
        public string ReceiverCity { get; set; }
        //Delivery Options
        public int DeliveryOptionId { get; set; } = 1;
        public List<int> DeliveryOptionIds { get; set; } = new List<int>();
        //PickUp Options
        public PickupOptions PickupOptions { get; set; }
        //Shipment Items
        public List<ShipmentItemDTO> ShipmentItems { get; set; }
        public decimal GrandTotal { get; set; }
        //Invoice parameters: Helps generate invoice for ecomnerce customers  by customerType
        //General Details comes with role user
        public string UserId { get; set; }        //
        public bool IsdeclaredVal { get; set; }
        public decimal? DeclarationOfValueCheck { get; set; } = 0;
        //payment method
        public bool IsInternational { get; set; }
        public string Description { get; set; }
        //Sender's Address - added for the special case of corporate customers
        public string SenderAddress { get; set; }
        public string SenderState { get; set; }
        public bool IsFromMobile { get; set; }
        public bool isInternalShipment { get; set; }
        //Country info
        public int DepartureCountryId { get; set; }
        public int DestinationCountryId { get; set; }
        public string SenderCode { get; set; }
        public string ReceiverCode { get; set; }
        public ShipmentScanStatus ShipmentScanStatus { get; set; }
        public int TimeInSeconds { get; set; }
        public string DestinationServiceCentreName { get; set; }
        public string DepartureServiceCentreName { get; set; }
        public List<WaybillChargeDTO> WaybillCharges { get; set; }
        public string VehicleType { get; set; }
    }
}
