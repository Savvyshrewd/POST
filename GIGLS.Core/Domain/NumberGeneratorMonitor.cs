﻿using GIGLS.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GIGLS.Core.Domain
{
    public class NumberGeneratorMonitor : BaseDomain
    {
        public int NumberGeneratorMonitorId { get; set; }
        public string ServiceCentreCode { get; set; }
        public NumberGeneratorType NumberGeneratorType { get; set; }
        public string Number { get; set; }
    }
}
