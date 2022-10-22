﻿using POST.CORE.DTO;

namespace POST.Core.DTO.Fleets
{
    public class DispatchActivityDTO : BaseDomainDTO
    {
        public int DispatchActivityId { get; set; }
        public int DispatchId { get; set; }
        public DispatchDTO Dispatch { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
    }
}
