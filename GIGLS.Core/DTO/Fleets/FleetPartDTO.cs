﻿using POST.CORE.DTO;
using System;

namespace POST.Core.DTO.Fleets
{
    public class FleetPartDTO : BaseDomainDTO
    {
        public int PartId { get; set; }
        public string PartName { get; set; }

        public string ModelName { get; set; }
        public int ModelId { get; set; }
    }
}
