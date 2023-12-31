﻿using POST.Core.Enums;
using POST.CORE.DTO;

namespace POST.Core.DTO.Wallet
{
    public class WalletPaymentLogDTO : BaseDomainDTO
    {
        public int WalletPaymentLogId { get; set; }

        public int WalletId { get; set; }
        public  WalletDTO Wallet { get; set; }

        public decimal Amount { get; set; }
        public int PaystackAmount { get; set; } 
        public string TransactionStatus { get; set; }
        public string TransactionResponse { get; set; }
        public string Description { get; set; }
        public string Email { get; set; } 
        public string UserId { get; set; }

        public bool IsWalletCredited { get; set; }
        public string Reference { get; set; }

        public OnlinePaymentType OnlinePaymentType { get; set; }
        public int PaymentCountryId { get; set; }
        public string PhoneNumber { get; set; }
        public string ExternalReference { get; set; }
        public string GatewayCode { get; set; }
        public string CurrencySymbol { get; set; }
        public string CurrencyCode { get; set; }
        public WalletTransactionType TransactionType { get; set; }
        public bool isConverted { get; set; }
        public CardType CardType { get; set; }
    }

}
