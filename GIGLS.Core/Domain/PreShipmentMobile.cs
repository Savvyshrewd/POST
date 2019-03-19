﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GIGLS.Core.Domain
{
    public class PreShipmentMobile : BaseDomain, IAuditable
    {
         [Key]
        public int PreShipmentMobileId { get; set; }
        public string SealNumber { get; set; }

        [MaxLength(100), MinLength(5)]
        [Index(IsUnique = true)]
        public string Waybill { get; set; }

        //Senders' Information
        public string SenderName { get; set; }

        public string SenderPhoneNumber { get; set; }
        public decimal Value { get; set; }
        public DateTime? DeliveryTime { get; set; }
       

        public string CustomerType { get; set; }
        public string CompanyType { get; set; }
        public string CustomerCode { get; set; }

        //Receivers Information
         public string ReceiverName { get; set; }
        public string ReceiverPhoneNumber { get; set; }
        public string ReceiverEmail { get; set; }
        public string ReceiverAddress { get; set; }
        public string ReceiverCity { get; set; }
        public string ReceiverState { get; set; }
        public string ReceiverCountry { get; set; }
        
        

        public int SenderLocationId { get; set; }
        public virtual Location SenderLocation { get; set; }

        public int ReceiverLocationId { get; set; }

        public virtual Location ReceiverLocation { get; set; }
        //Delivery Options
        public bool IsHomeDelivery { get; set; }

        //General but optional
      
        public DateTime? ExpectedDateOfArrival { get; set; }
        public DateTime? ActualDateOfArrival { get; set; }

        //Shipment Items
        public virtual List<PreShipmentItemMobile> PreShipmentItems { get; set; }

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
        public decimal? AppliedDiscount { get; set; }
        public decimal? DiscountValue { get; set; }

        public decimal? Insurance { get; set; }
        public decimal? Vat { get; set; }
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
        public decimal? CalculatedTotal { get; set; } = 0;
    }
}
