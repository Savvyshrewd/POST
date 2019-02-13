﻿using GIGLS.Core.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GIGLS.Core.Domain.BankSettlement
{ 

    public class BankProcessingOrderForShipmentAndCOD : BaseDomain, IAuditable  
    {
        [Key]
        public int ProcessingOrderId { get; set; } 

        [MaxLength(100), MinLength(5)] 
        [Index(IsUnique = true)]
        public string Waybill { get; set; }

        public string RefCode { get; set; }
        public int ServiceCenterId { get; set; }
        public string ServiceCenter { get; set; }
        public bool status { get; set; }
    }
    
    public class BankProcessingOrderCodes : BaseDomain, IAuditable
    {
        [Key]
        public int CodeId { get; set; }
        public string Code { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime DateAndTimeOfDeposit { get; set; } 
        public string UserId { get; set; }
        public int ServiceCenter { get; set; }
        public DepositType DepositType { get; set; }
        public DateTime StartDateTime { get; set; }
        public bool status { get; set; }

    }
}
