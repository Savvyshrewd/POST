﻿using GIGLS.Core.Enums;
using GIGLS.Core.Domain;
using System;

namespace GIGLS.Core.Domain.Wallet
{
    public class AccountSummary : BaseDomain, IAuditable
    {
        public int AccountSummaryId { get; set; }

        public double Balance { get; set; }
        public virtual AccountType AccountType { get; set; }
    }
}
