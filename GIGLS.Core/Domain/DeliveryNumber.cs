﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GIGLS.Core.Domain
{
    public class DeliveryNumber : BaseDomain, IAuditable
    {
        public int DeliveryNumberId { get; set; }
        public string Number { get; set; }

        public string UserId { get; set; }

        public bool IsUsed { get; set; }
    }
}