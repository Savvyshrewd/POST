﻿namespace GIGLS.Core.Domain.Wallet
{
    public class CashOnDeliveryBalance : BaseDomain, IAuditable
    {
        public int CashOnDeliveryBalanceId { get; set; }   
        public decimal Balance { get; set; }      
        public int WalletId { get; set; }
        public virtual Wallet Wallet { get; set; }
        public string UserId { get; set; }
    }
}
