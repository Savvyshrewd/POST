﻿using GIGLS.Core.Enums;
using GIGLS.Core.DTO.Customers;
using System;
using System.Collections.Generic;
using GIGLS.CORE.DTO;
using GIGLS.Core.DTO.ServiceCentres;
using GIGLS.Core.DTO.Zone;
using GIGLS.CORE.DTO.Shipments;
using GIGLS.Core.DTO.Account;

namespace GIGLS.Core.DTO.Shipments
{
    public class PreShipmentDTO : BaseDomainDTO
    {
        //Shipment Information
        public int PreShipmentId { get; set; }
        public string TempCode { get; set; }
        public string Waybill { get; set; }

        //Senders' Information
        
        public string CompanyType { get; set; }
        public string CustomerCode { get; set; }
        public int CountryId { get; set; }
        public decimal WalletBalance { get; set; }
        public string SenderUserId { get; set; }
        public string SenderName { get; set; }
        public string SenderPhoneNumber { get; set; }
        public string SenderCity { get; set; }

        public string ReceiverName { get; set; }
        public string ReceiverPhoneNumber { get; set; }
        public string ReceiverAddress { get; set; }
        public string ReceiverCity { get; set; }
        public string ReceiverState { get; set; }
        public string ReceiverCountry { get; set; }
        public PickupOptions PickupOptions { get; set; }

        public int DepartureStationId { get; set; }
        public int DestinationStationId { get; set; }

        //PreShipment Items
        public List<PreShipmentItemDTO> PreShipmentItems { get; set; }
        public decimal GrandTotal { get; set; }
        public bool IsProcessed { get; set; }

        //Delivery Options
        //public int DeliveryOptionId { get; set; } = 1;
        //public DeliveryOptionDTO DeliveryOption { get; set; }
        //public List<int> DeliveryOptionIds { get; set; } = new List<int>();

        //General but optional
        //public bool IsDomestic { get; set; }
        //public DateTime? ExpectedDateOfArrival { get; set; }
        //public DateTime? ActualDateOfArrival { get; set; }



        //Invoice parameters: Helps generate invoice for ecomnerce customers  by customerType
        //public bool IsCashOnDelivery { get; set; }
        //public decimal? CashOnDeliveryAmount { get; set; } = 0;

        //public decimal? ExpectedAmountToCollect { get; set; } = 0;
        //public decimal? ActualAmountCollected { get; set; } = 0;

        //General Details comes with role user
        //public string UserId { get; set; }

        //
        //public List<CustomerDTO> Customer { get; set; }
        //public CustomerDTO CustomerDetails { get; set; }

        //
        //public bool IsdeclaredVal { get; set; }
        //public decimal? DeclarationOfValueCheck { get; set; } = 0;

        ////discount information
        //public decimal? AppliedDiscount { get; set; } = 0;
        //public decimal? DiscountValue { get; set; } = 0;

        //public decimal? Insurance { get; set; } = 0;
        //public decimal? Vat { get; set; } = 0;
        //public decimal? Total { get; set; } = 0;

        //public decimal ShipmentPackagePrice { get; set; }

        ////wallet information
        //public string WalletNumber { get; set; }

        //from client
        //public decimal? vatvalue_display { get; set; } = 0;
        //public decimal? InvoiceDiscountValue_display { get; set; } = 0;
        //public decimal? offInvoiceDiscountvalue_display { get; set; } = 0;

        //ShipmentCollection
        //public ShipmentCollectionDTO ShipmentCollection { get; set; }

        //Demurrage Information
        //public DemurrageDTO Demurrage { get; set; }

        //public bool IsCancelled { get; set; }
        //public bool IsInternational { get; set; }

        //Invoice Information
        //public InvoiceDTO Invoice { get; set; }

        //public string Description { get; set; }

        //public PreShipmentRequestStatus RequestStatus { get; set; }
        //public PreShipmentProcessingStatus ProcessingStatus { get; set; }

        ////Receivers Information


        //public bool IsMapped { get; set; }
        //public string DeclinedReason { get; set; }

        ////Agility Validations
        //public decimal? CalculatedTotal { get; set; } = 0;
    }
}
