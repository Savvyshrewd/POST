﻿using POST.CORE.DTO;

namespace POST.Core.DTO.Wallet
{
    public class CashOnDeliveryBalanceDTO : BaseDomainDTO
    {
        public int CashOnDeliveryBalanceId { get; set; }
        public decimal Balance { get; set; }
        public int WalletId { get; set; }
        public WalletDTO Wallet { get; set; }
        public string UserId { get; set; }
    }
}
