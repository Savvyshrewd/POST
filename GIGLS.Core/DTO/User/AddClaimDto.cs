﻿namespace POST.CORE.DTO.User
{
    public class AddClaimDto
    {
        public string userId { get; set; }
        public string claimType { get; set; }
        public string claimValue { get; set; }
        public string SystemRole { get; set; } 
    }
}
