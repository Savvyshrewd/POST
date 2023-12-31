﻿using POST.Core.Enums;
using POST.CORE.DTO;
using System;
using System.ComponentModel.DataAnnotations;

namespace POST.Core.View.Archived
{
    public class InvoiceArchiveView : BaseDomainDTO
    {
        [Key]
        public int InvoiceId { get; set; }
        public string InvoiceNo { get; set; }
        public decimal Amount { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public string PaymentMethod { get; set; }
        public DateTime PaymentDate { get; set; }
        public string Waybill { get; set; }
        public DateTime DueDate { get; set; }
        public bool IsShipmentCollected { get; set; }
        public string PaymentTypeReference { get; set; }

        //Shipment Information
        public int ShipmentId { get; set; }
        public string SealNumber { get; set; }
        public decimal Value { get; set; }
        public decimal GrandTotal { get; set; }
        public decimal? DiscountValue { get; set; }

        public decimal? Insurance { get; set; }
        public decimal? Vat { get; set; }
        public decimal? Total { get; set; }
        public decimal? CashOnDeliveryAmount { get; set; }
        public bool IsCancelled { get; set; }
        public decimal ShipmentPackagePrice { get; set; }
        public bool IsInternational { get; set; }
        public double ApproximateItemsWeight { get; set; }

        //Customer Information
        public int CustomerId { get; set; }
        public string CustomerType { get; set; }
        public string CompanyType { get; set; }
        public string CustomerCode { get; set; }

        //Receiver Information
        public string ReceiverName { get; set; }
        public string ReceiverPhoneNumber { get; set; }
        public string ReceiverEmail { get; set; }
        public string ReceiverAddress { get; set; }
        public string ReceiverCity { get; set; }
        public string ReceiverState { get; set; }
        public string ReceiverCountry { get; set; }

        //DeliveryOption
        public int DeliveryOptionId { get; set; }
        public string DeliveryOptionCode { get; set; }
        public string DeliveryOptionDescription { get; set; }

        public int DepartureStationId { get; set; }
        public string DepartureStationName { get; set; }

        public int DestinationStationId { get; set; }
        public string DestinationStationName { get; set; }

        //service centre
        public int DepartureServiceCentreId { get; set; }
        public string DepartureServiceCentreCode { get; set; }
        public string DepartureServiceCentreName { get; set; }

        public int DestinationServiceCentreId { get; set; }
        public string DestinationServiceCentreCode { get; set; }
        public string DestinationServiceCentreName { get; set; }

        //UserId
        public string UserId { get; set; }
        public string UserName { get; set; }

        public string Description { get; set; }

        //PickUp Options
        public PickupOptions PickupOptions { get; set; }

        //Customer
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public int? CompanyId { get; set; }
        public string Name { get; set; }
        public int? IndividualCustomerId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }

        public DepositStatus DepositStatus { get; set; }

        public bool ReprintCounterStatus { get; set; }
        public bool isInternalShipment { get; set; }

        //use to optimise shipment progress for shipment that has depart service centre
        public ShipmentScanStatus ShipmentScanStatus { get; set; }
        public bool IsGrouped { get; set; }

        //Country info
        public int DepartureCountryId { get; set; }
        public int DestinationCountryId { get; set; }
        public bool IsCashOnDelivery { get; set; }
        public decimal? CODAmount { get; set; }
        public bool IsFromMobile { get; set; }
    }
}
