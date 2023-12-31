﻿using System.ComponentModel.DataAnnotations;

namespace POST.Core.Domain
{
    public class PackingList : BaseDomain, IAuditable
    {
        public int PackingListId  { get; set; }

        [MaxLength(100)]
        public string Waybill { get; set; }
        public string Items { get; set; }
    }
}
