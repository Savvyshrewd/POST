﻿using GIGLS.Core.Enums;
using System.ComponentModel.DataAnnotations;

namespace GIGLS.Core.Domain.MessagingLog
{
    public class EmailSendLog : BaseDomain, IAuditable
    {
        public int EmailSendLogId { get; set; }

        [MaxLength(128)]
        public string To { get; set; }
        public string From { get; set; }
        public string Message { get; set; }
        public MessagingLogStatus Status { get; set; }
        public string User { get; set; }
        public string ResultStatus { get; set; }
        public string ResultDescription { get; set; }
    }
}
