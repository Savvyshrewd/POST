﻿using GIGLS.Core.IRepositories.User;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GIGLS.Infrastructure.Persistence.Repository;
using GIGLS.Infrastructure.Persistence;
using GIGLS.Core.DTO.User;
using Microsoft.AspNet.Identity;
using GIGLS.CORE.Domain;
using System.Security.Claims;
using GIGLS.Core.Enums;

namespace GIGLS.INFRASTRUCTURE.Persistence.Repositories.User
{
    public class UserRepository : AuthRepository<GIGL.GIGLS.Core.Domain.User, GIGLSContext>, IUserRepository
    {
        public UserRepository(GIGLSContext context) : base(context)
        {

        }

        public Task<GIGL.GIGLS.Core.Domain.User> GetUserByEmail(string email)
        {            
            var user = _userManager.Users.Where(x => x.Email.Equals(email)).FirstOrDefault();
            return Task.FromResult(user);
        }

        public Task<GIGL.GIGLS.Core.Domain.User> GetUserById(string id)
        {
            var user = _userManager.FindByIdAsync(id);
            return user;
        }

        public Task<GIGL.GIGLS.Core.Domain.User> GetUserByChannelCode(string channelCode)
        {
            var user = _userManager.Users.Where(x => x.UserChannelCode.Equals(channelCode)).FirstOrDefault();          
            return Task.FromResult(user);
        }

        public Task<GIGL.GIGLS.Core.Domain.User> GetUserById(int id)
        {
            var user = _repo.Get(id);
            return Task.FromResult(user);
        }

        public Task<GIGL.GIGLS.Core.Domain.User> GetUserByName(string userName)
        {
            var user = _userManager.FindByNameAsync(userName);
            return user;
        }

        public Task<GIGL.GIGLS.Core.Domain.User> GetUserByCompanyName(string name)
        {
            var user = _userManager.Users.Where(x => x.Organisation.ToLower() == name.ToLower()).FirstOrDefault();
            return Task.FromResult(user);
        }

        public Task<IEnumerable<GIGL.GIGLS.Core.Domain.User>> GetUsers()
        {
            var user = _userManager.Users.Where(x => x.IsDeleted == false && x.UserType != UserType.System 
                        && x.UserChannelType == UserChannelType.Employee).AsEnumerable();
            return Task.FromResult(user.OrderBy(x => x.FirstName).AsEnumerable());
        }

        public Task<IEnumerable<GIGL.GIGLS.Core.Domain.User>> GetCustomerUsers(string email)
        {
            var user = _userManager.Users.Where(x => x.IsDeleted == false && x.Email == email && x.UserType != UserType.System && x.UserChannelType != UserChannelType.Employee).AsEnumerable();
            return Task.FromResult(user.OrderBy(x => x.FirstName).AsEnumerable());
        }

        public Task<IEnumerable<GIGL.GIGLS.Core.Domain.User>> GetCustomerUsers()
        {
            var user = _userManager.Users.Where(x => x.IsDeleted == false && (x.UserChannelType == UserChannelType.Corporate 
                        || x.UserChannelType == UserChannelType.Ecommerce || x.UserChannelType == UserChannelType.IndividualCustomer)).AsEnumerable();
            return Task.FromResult(user.OrderBy(x => x.FirstName).AsEnumerable());
        }

        public Task<IEnumerable<GIGL.GIGLS.Core.Domain.User>> GetCorporateCustomerUsers()
        {
            var user = _userManager.Users.Where(x => x.IsDeleted == false && (x.UserChannelType == UserChannelType.Corporate
                        || x.UserChannelType == UserChannelType.Ecommerce)).AsEnumerable();
            return Task.FromResult(user.OrderBy(x => x.FirstName).AsEnumerable());
        }

        public IQueryable<GIGL.GIGLS.Core.Domain.User> GetCorporateCustomerUsersAsQueryable()
        {
            var users = _userManager.Users.AsQueryable();
            return users; 
        }


        public Task<IEnumerable<GIGL.GIGLS.Core.Domain.User>> GetPartnerUsers()
        {
            var user = _userManager.Users.Where(x => x.IsDeleted == false && x.UserChannelType == UserChannelType.Partner).AsEnumerable();
            return Task.FromResult(user.OrderBy(x => x.FirstName).AsEnumerable());
        }

        public Task<IEnumerable<GIGL.GIGLS.Core.Domain.User>> GetSystemUsers()
        {
            var user = _userManager.Users.Where(x => x.IsDeleted == false && x.UserType == UserType.System).AsEnumerable();
            return Task.FromResult(user.OrderBy(x => x.FirstName).AsEnumerable());
        }

        public Task<IEnumerable<GIGL.GIGLS.Core.Domain.User>> GetDispatchCaptains()
        {
            var user = _userManager.Users.Where(x => x.IsDeleted == false && x.UserType != UserType.System
                        && x.UserChannelType == UserChannelType.Employee
                        && (x.SystemUserRole == "Dispatch Rider" || x.SystemUserRole == "Captain")).AsEnumerable();
            return Task.FromResult(user.OrderBy(x => x.FirstName).AsEnumerable());
        }

        public Task<IEnumerable<GIGL.GIGLS.Core.Domain.User>> GetCaptains()
        {
            var user = _userManager.Users.Where(x => x.IsDeleted == false && x.UserType != UserType.System
                        && x.UserChannelType == UserChannelType.Employee
                        && (x.SystemUserRole == "Captain" && x.Designation == "Captain")).AsEnumerable();
            return Task.FromResult(user.OrderBy(x => x.FirstName).AsEnumerable());
        }

        public Task<IEnumerable<GIGL.GIGLS.Core.Domain.User>> GetDispatchRiders()
        {
            var user = _userManager.Users.Where(x => x.IsDeleted == false && x.UserType != UserType.System
                        && x.UserChannelType == UserChannelType.Employee
                        && (x.SystemUserRole == "Dispatch Rider")).AsEnumerable();
            return Task.FromResult(user.OrderBy(x => x.FirstName).AsEnumerable());
        }

        public async Task<IdentityResult> UpdateUser(string userId, GIGL.GIGLS.Core.Domain.User user)
        {
            return await _userManager.UpdateAsync(user);
        }

        public async Task<IdentityResult> RegisterUser(GIGL.GIGLS.Core.Domain.User user, string password)
        {
            try
            {
                return await _userManager.CreateAsync(user, password);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IdentityResult> AddRole(string name)
        {
            var role = new AppRole(name);
            role.DateCreated = DateTime.Now.Date;
            role.DateModified = DateTime.Now.Date;

            var result = await _roleManager.CreateAsync(role);
            return result;
        }

        public Task<AppRole> GetRoleById(string roleId)
        {
            var role = _roleManager.FindByIdAsync(roleId);
            return role;
        }

        public Task<AppRole> GetRoleByName(string roleName)
        {
            var role = _roleManager.FindByNameAsync(roleName);
            return role;
        }

        public Task<IEnumerable<AppRole>> GetRoles()
        {
            var role = _roleManager.Roles.Where(x => x.IsDeleted == false).AsEnumerable();
            return Task.FromResult(role.OrderBy(x => x.Name).AsEnumerable());
        }

        public async Task<IdentityResult> Remove(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            user.IsDeleted = true;
            return await _userManager.UpdateAsync(user);
        }

        public async Task<IdentityResult> RemoveRole(string roleId)
        {
            var rolResult = await _roleManager.FindByIdAsync(roleId);
            rolResult.IsDeleted = true;
            rolResult.DateModified = DateTime.Now.Date;
            return await _roleManager.UpdateAsync(rolResult);
        }

        public async Task<IdentityResult> UpdateRole(string roleId, RoleDTO roleDTO)
        {
            var rolResult = await _roleManager.FindByIdAsync(roleId);
            rolResult.Name = roleDTO.Name;
            rolResult.DateModified = DateTime.Now.Date;
            return await _roleManager.UpdateAsync(rolResult);
        }

        //Add a user to a role
        public async Task<IdentityResult> AddToRoleAsync(string userid, string name)
        {
            var Result = await _userManager.AddToRoleAsync(userid, name);
            return Result;
        }

        //Returns a list of roles a user has
        public async Task<IList<string>> GetUserRoles(string userid)
        {
            var Result = await _userManager.GetRolesAsync(userid);
            return Result;
        }

        //Returns true if the user with the specified ID is a member of the role
        public async Task<bool> IsInRoleAsync(string roleId, string name)
        {
            var Result = await _userManager.IsInRoleAsync(roleId, name);
            return Result;
        }

        //Remove a user with specified id from the specified role name
        public async Task<IdentityResult> RemoveFromRoleAsync(string roleId, string name)
        {
            var Result = await _userManager.RemoveFromRoleAsync(roleId, name);
            return Result;
        }

        public async Task<IdentityResult> AddClaimAsync(string userid, Claim claim)
        {
            var Result = await _userManager.AddClaimAsync(userid, claim);
            return Result;
        }

        public async Task<IdentityResult> RemoveClaimAsync(string userid, Claim claim)
        {
            var Result = await _userManager.RemoveClaimAsync(userid, claim);
            return Result;
        }

        public async Task<IList<Claim>> GetClaimsAsync(string userid)
        {
            var Result = await _userManager.GetClaimsAsync(userid);
            return Result;
        }

        public async Task<IdentityResult> ResetPassword(string userid, string password)
        {
            await _userManager.RemovePasswordAsync(userid);
            var Result = await _userManager.AddPasswordAsync(userid, password);
            return Result;
        }

        public async Task<IdentityResult> ChangePassword(string userid, string currentPassword, string newPassword)
        {
            var Result = await _userManager.ChangePasswordAsync(userid, currentPassword, newPassword);
            return Result;
        }

        public async Task<bool> CheckPasswordAsync(GIGL.GIGLS.Core.Domain.User user, string password)
        {
            if (await _userManager.CheckPasswordAsync(user, password))
                return true;

            return false;
        }

        public Task<GIGL.GIGLS.Core.Domain.User> GetUserByPhoneNumber(string PhoneNumber)
        {
            var user = _userManager.Users.Where(x => x.PhoneNumber.Contains(PhoneNumber)).FirstOrDefault();
            return Task.FromResult(user);
        }

        public Task<bool> IsUserHasAdminRole(string userId)
        {
            bool hasAdminRole = false;

            var user = _userManager.Users
                .Where(x => x.IsDeleted == false && x.Id == userId 
                && (x.SystemUserRole == "Chairman" || x.SystemUserRole == "Administrator" || x.SystemUserRole == "Director")).FirstOrDefault();

            if(user != null)
            {
                hasAdminRole = true;
            }

            return Task.FromResult(hasAdminRole);
        }

        public Task<GIGL.GIGLS.Core.Domain.User> GetUserByEmailorPhoneNumber(string email, string PhoneNumber)
        {
            var user = _userManager.Users.Where(x => x.Email.Equals(email) || x.PhoneNumber.Contains(PhoneNumber)).ToList();
            var lastUser = user.LastOrDefault();
            return Task.FromResult(lastUser);
        }

        public Task<List<GIGL.GIGLS.Core.Domain.User>> GetUserListByEmailorPhoneNumber(string email, string PhoneNumber)
        {
            var user = _userManager.Users.Where(x => x.Email.Equals(email) || x.PhoneNumber.Contains(PhoneNumber)).ToList();
            return Task.FromResult(user);
        }

        public Task<GIGL.GIGLS.Core.Domain.User> GetUserUsingCustomer(string emailPhoneCode)
        {
            var user = _userManager.Users.Where(x => (x.Email.Equals(emailPhoneCode) 
            || x.UserChannelCode.Equals(emailPhoneCode) || x.PhoneNumber.Contains(emailPhoneCode)) 
            && (x.IsRegisteredFromMobile == true || x.SystemUserRole == "Dispatch Rider" || x.SystemUserRole == "Captain" 
            || x.UserChannelType==UserChannelType.Ecommerce || x.UserChannelType == UserChannelType.Corporate)).FirstOrDefault();
            return Task.FromResult(user);
        }

        //get User using Customer - For Customer Portal
        public Task<GIGL.GIGLS.Core.Domain.User> GetUserUsingCustomerForCustomerPortal(string emailPhoneCode)
        {
            var user = _userManager.Users.Where(x => (x.Email.Equals(emailPhoneCode) ||
            x.UserChannelCode.Equals(emailPhoneCode) || x.PhoneNumber.Contains(emailPhoneCode)) && (x.UserChannelType != UserChannelType.Employee )).FirstOrDefault();
            return Task.FromResult(user);
        }

        //get User using Customer - For Mobile Scanner App
        public Task<GIGL.GIGLS.Core.Domain.User> GetUserUsingCustomerForMobileScanner(string emailPhoneCode)
        {
            var user = _userManager.Users.Where(x => (x.Email.Equals(emailPhoneCode) ||
            x.UserChannelCode.Equals(emailPhoneCode) || x.PhoneNumber.Contains(emailPhoneCode)) && (x.UserChannelType == UserChannelType.Employee)).FirstOrDefault();
            return Task.FromResult(user);
        }

        //get User using Customer - For Fast Track Agent App Only
        public Task<GIGL.GIGLS.Core.Domain.User> GetUserUsingCustomerForAgentApp(string emailPhoneCode)
        {
            //var user = _userManager.Users.Where(x => (x.Email.Equals(emailPhoneCode) ||
            //x.UserChannelCode.Equals(emailPhoneCode) || x.PhoneNumber.Contains(emailPhoneCode)) && (x.UserChannelType == UserChannelType.Employee && x.SystemUserRole == "FastTrack Agent")).FirstOrDefault();

            var user = _userManager.Users.Where(x => x.Email == emailPhoneCode || x.UserChannelCode == emailPhoneCode || x.PhoneNumber.Contains(emailPhoneCode)).FirstOrDefault();
            return Task.FromResult(user);
        }

        public async Task<GIGL.GIGLS.Core.Domain.User> ActivateUserByEmail(string email, bool isActive)
        {
            var user = _userManager.Users.Where(x => x.Email.Equals(email)).FirstOrDefault();
            if(user != null)
            {
                user.IsActive = isActive;
                await _userManager.UpdateAsync(user);
            }
            return await Task.FromResult(user);
        }

        public Task<bool> IsCustomerHasAgentRole(string userId)
        {
            bool hasAgentRole = false;

            //FastTrack Agent
            var user = _userManager.Users.Where(x => x.IsDeleted == false && x.Id == userId && (x.SystemUserRole == "FastTrack Agent")).FirstOrDefault();
                   
            if (user != null)
            {
                hasAgentRole = true;
            }

            return Task.FromResult(hasAgentRole);
        }

        public Task<List<GIGL.GIGLS.Core.Domain.User>> GetUsers(string[] ids)
        {
            var user = _userManager.Users.Where(x => x.IsDeleted == false && ids.Contains(x.Id)).ToList();
            return Task.FromResult(user);
        }

        public Task<GIGL.GIGLS.Core.Domain.User> GetUserByEmailorChannelCode(string searchParam)
        {
            var user = _userManager.Users.Where(x => x.Email.Equals(searchParam) || x.UserChannelCode.Contains(searchParam)).FirstOrDefault();
            return Task.FromResult(user);
        }

        public Task<GIGL.GIGLS.Core.Domain.User> GetEmployeeUserByEmail(string email)
        {
            var user = _userManager.Users.Where(x => x.Email.Equals(email) && x.IsDeleted == false && x.UserType != UserType.System
                        && x.UserChannelType == UserChannelType.Employee).FirstOrDefault();
            return Task.FromResult(user);
        }

        public Task<List<GIGL.GIGLS.Core.Domain.User>> GetPartnerUsersByEmail(string email)
        {
            var user = _userManager.Users.Where(x =>email.Contains(x.Email) &&x.IsActive == true && x.IsDeleted == false && x.UserChannelType == UserChannelType.Partner || x.UserChannelType == UserChannelType.Employee);
            return Task.FromResult(user.ToList());
        }

        public Task<IEnumerable<GIGL.GIGLS.Core.Domain.User>> GetPartnerUsersByEmail2(string email)
        {
            var user = _userManager.Users.Where(x => x.Email.Equals(email) && x.IsDeleted == false && x.UserChannelType == UserChannelType.Partner).AsEnumerable();
            return Task.FromResult(user.OrderBy(x=>x.FirstName).AsEnumerable());
        }
    }
}
