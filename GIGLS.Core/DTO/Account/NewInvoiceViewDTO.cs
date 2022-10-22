﻿using POST.Core.DTO.Customers;
using POST.Core.DTO.ServiceCentres;
using POST.Core.DTO.Zone;
using POST.Core.Enums;
using POST.CORE.DTO;
using System;

namespace POST.Core.DTO.Account
{
    public class NewInvoiceViewDTO : BaseDomainDTO 
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

        public string Name { get; set; }


        //Receivers Information
        public int DepartureServiceCentreId { get; set; }
        public string DepartureServiceCentreName { get; set; }

        public int ServiceCenter { get; set; } 
        public string ScName { get; set; }

        //public ServiceCentreDTO DepartureServiceCentre { get; set; }

        //public ServiceCentreDTO DestinationServiceCentre { get; set; }

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

        //PickUp Options
        public PickupOptions PickupOptions { get; set; }

        //General but optional
        //public bool IsDomestic { get; set; }
        public DateTime? ExpectedDateOfArrival { get; set; }
        public DateTime? ActualDateOfArrival { get; set; }

        //Shipment Items
        public double ApproximateItemsWeight { get; set; }

        public decimal GrandTotal { get; set; }
        public decimal TotalAmount { get; set; }

        //Invoice parameters: Helps generate invoice for ecomnerce customers  by customerType
        public bool IsCashOnDelivery { get; set; }
        public decimal? CashOnDeliveryAmount { get; set; } = 0;

        public decimal? ExpectedAmountToCollect { get; set; } = 0;
        public decimal? ActualAmountCollected { get; set; } = 0;

        //General Details comes with role user
        public string UserId { get; set; }
        public string UserName { get; set; } 


        //
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

        //wallet information
        public string WalletNumber { get; set; }

        //from client
        public decimal? vatvalue_display { get; set; } = 0;
        public decimal? InvoiceDiscountValue_display { get; set; } = 0;
        public decimal? offInvoiceDiscountvalue_display { get; set; } = 0;

        //payment method
        public string PaymentMethod { get; set; }

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
        public bool IsCODPaidOut { get; set; }

    }

}
