﻿using GIGLS.Core.Enums;
using GIGLS.CORE.Domain;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using System.Threading.Tasks;

namespace GIGL.GIGLS.Core.Domain
{
    public class ApplicationUserRole : IdentityUserRole<string> { }
    public class User : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Gender Gender { get; set; }
        public string Designation { get; set; }
        public string Department { get; set; }
        public string PictureUrl { get; set; }
        public bool IsActive { get; set; }
        public string Organisation { get; set; }
        public int Status { get; set; }
        public UserType UserType { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime DateModified { get; set; }
        public bool IsDeleted { get; set; }

        //public async Task GenerateUserIdentityAsync(Microsoft.AspNet.Identity.UserManager manager)
        //{
        //    return await GenerateUserIdentityAsync(manager, DefaultAuthenticationTypes.ApplicationCookie);
        //}

        //Rest of code is removed for brevity
        public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<User> manager, string authenticationType)
        {
            var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
            // Add custom user claims here
            return userIdentity;
        }
    }


}
