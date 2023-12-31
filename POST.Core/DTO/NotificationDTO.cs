﻿using POST.Core.Enums;
using POST.CORE.DTO;

namespace POST.Core.DTO
{
    public class NotificationDTO : BaseDomainDTO
    {
        public int NotificationId { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public string UserId { get; set; }
        public bool IsRead { get; set; }
        public MessageAction MesageActions { get; set; }
    }

    public class ServiceSMS
    {
        public string ReferenceNo { get; set; }
        public BillType BillType { get; set; }
    }


}
