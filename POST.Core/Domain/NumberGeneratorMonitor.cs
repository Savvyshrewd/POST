﻿using POST.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace POST.Core.Domain
{
    public class NumberGeneratorMonitor : BaseDomain
    {
        public int NumberGeneratorMonitorId { get; set; }

        [MaxLength(100)]
        public string ServiceCentreCode { get; set; }
        public NumberGeneratorType NumberGeneratorType { get; set; }

        [MaxLength(100)]
        public string Number { get; set; }
    }
}
