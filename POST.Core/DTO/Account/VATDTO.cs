﻿using POST.Core.Enums;
using POST.CORE.DTO;

namespace POST.Core.DTO.Account
{
    public class VATDTO : BaseDomainDTO
    {
        public int VATId { get; set; }
        public string Name { get; set; }
        public VATType Type { get; set; }
        public decimal Value { get; set; }
        public int CountryId { get; set; }
        public CountryDTO Country { get; set; }
    }
}
