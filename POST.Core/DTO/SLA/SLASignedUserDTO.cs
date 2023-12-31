﻿using POST.Core.DTO.User;
using POST.CORE.DTO;

namespace POST.Core.DTO.SLA
{
    public class SLASignedUserDTO : BaseDomainDTO
    {
        public int SLASignedUserId { get; set; }
        public int SLAId { get; set; }
        public SLADTO SLA { get; set; }
        public string UserId { get; set; }
        public UserDTO User { get; set; }
    }
}
