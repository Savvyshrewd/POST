﻿using GIGLS.Core.Enums;

namespace GIGLS.Core.Domain.Wallet
{
    public class Wallet : BaseDomain, IAuditable
    {
        public int WalletId { get; set; }
        public string WalletNumber { get; set; }
        public decimal Balance { get; set; }
        public int CustomerId { get; set; }
        public CustomerType CustomerType { get; set; }
    }
}
