﻿using POST.CORE.DTO;
using System;

namespace POST.Core.DTO.Partnership
{
    public class PartnerPayoutDTO : BaseDomainDTO
    {
        public int PartnerPayoutId { get; set; }

        public decimal Amount { get; set; }

        public string ProcessedBy { get; set; }

        public DateTime DateProcessed { get; set; }

        public string PartnerName { get; set; }
        public string PartnerType { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }
    }
}
