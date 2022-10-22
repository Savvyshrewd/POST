﻿using POST.CORE.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POST.Core.DTO
{
    public class DeliveryLocationDTO : BaseDomainDTO
    {         
        public int DeliveryLocationId { get; set; }
        public string Location { get; set; }
        public decimal Tariff { get; set; }
    }
}
