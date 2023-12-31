﻿using POST.Core.Enums;
using POST.CORE.DTO;

namespace POST.Core.DTO.Wallet
{
    public class WalletDTO : BaseDomainDTO
    {
        public int WalletId { get; set; }
        public string WalletNumber { get; set; }
        public decimal Balance { get; set; }
        public int CustomerId { get; set; }
        public CustomerType CustomerType { get; set; }
        public string CustomerName { get; set; }
        public bool IsSystem { get; set; }
        public string CustomerCode { get; set; }
        public string CompanyType { get; set; }
        public string CustomerPhoneNumber { get; set; }
        public string CustomerEmail { get; set; }

        public int UserActiveCountryId { get; set; }
        public CountryDTO Country { get; set; }
        public decimal AmountToCharge { get; set; }
        public string Reason { get; set; }
    }
    public class ChargeWalletDTO
    {
        public string UserId { get; set; }
        public string ReferenceNo { get; set; }
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public BillType BillType { get; set; }
        public decimal ServiceCharge { get; set; }
    }
}
