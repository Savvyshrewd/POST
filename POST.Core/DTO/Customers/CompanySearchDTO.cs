﻿using POST.Core.Enums;
using POST.CORE.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POST.Core.DTO.Customers
{
    public class CompanySearchDTO : BaseDomainDTO
    {
        public string searchItem { get; set; }
        public Rank? rank { get; set; } = null;
        public string email { get; set; }
    }
}
