﻿using POST.Core;
using POST.Core.Domain;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace GIGL.POST.Core.Domain
{
    public class FleetModel : BaseDomain, IAuditable
    {      
        [Key]
        public int ModelId { get; set; }
        public string ModelName { get; set; }

        public int MakeId { get; set; }
        public virtual FleetMake FleetMake { get; set; }

        public virtual ICollection<Fleet> Fleets { get; set; }
    }
}