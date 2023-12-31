﻿using AutoMapper;
using POST.Core;
using POST.Core.DTO;
using POST.Core.DTO.ServiceCentres;
using POST.Core.DTO.User;
using POST.Core.Enums;
using POST.Core.IServices;
using POST.Core.IServices.ServiceCentres;
using POST.Core.IServices.User;
using POST.Core.IServices.Utility;
using POST.CORE.Domain;
using POST.Infrastructure;
using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace POST.Services.Implementation.User
{
    public class UserService : IUserService
    {
        private readonly IUnitOfWork _unitOfWork;
        private IServiceCentreService _serviceCentreService;
        private IRegionServiceCentreMappingService _regionServiceCentreMappingService;
        private readonly INumberGeneratorMonitorService _numberGeneratorMonitorService;
        private ICountryService _countryService;

        public UserService(IUnitOfWork uow, IServiceCentreService serviceCentreService,
            IRegionServiceCentreMappingService regionServiceCentreMappingService,
            INumberGeneratorMonitorService numberGeneratorMonitorService,ICountryService countryService)
        {
            _unitOfWork = uow;
            _serviceCentreService = serviceCentreService;
            _regionServiceCentreMappingService = regionServiceCentreMappingService;
            _numberGeneratorMonitorService = numberGeneratorMonitorService;
            _countryService = countryService;
            MapperConfig.Initialize();
        }

        //Register a new user
        public async Task<IdentityResult> AddUser(UserDTO userDto)
        {
            var userExist = await _unitOfWork.User.GetUserByEmail(userDto.Email);
            if (userExist != null)
            {
                throw new GenericException($"User with email: {userDto.Email} already exist", $"{(int)HttpStatusCode.Forbidden}");
            }

            var user = Mapper.Map<GIGL.POST.Core.Domain.User>(userDto);

            user.Id = Guid.NewGuid().ToString();
            user.DateCreated = DateTime.Now.Date;
            user.DateModified = DateTime.Now.Date;
            user.PasswordExpireDate = DateTime.Now;
            user.UserName = (user.UserChannelType == UserChannelType.Employee) ? user.Email : user.UserChannelCode;            

            //UserChannelCode for employee
            if (user.UserChannelType == UserChannelType.Employee)
            {
                var employeeCode = await _numberGeneratorMonitorService.GenerateNextNumber(NumberGeneratorType.Employee);
                user.UserChannelCode = employeeCode;
                user.UserChannelPassword = GeneratePassword();
                user.IsActive = true;
            }

            var u = await _unitOfWork.User.RegisterUser(user, userDto.Password);
            return u;
        }

        //Get all users
        public Task<IEnumerable<GIGL.POST.Core.Domain.User>> GetUsers()
        {
            return _unitOfWork.User.GetUsers();
        }

        public Task<IEnumerable<GIGL.POST.Core.Domain.User>> GetCustomerUsers(string email)
        {
            return _unitOfWork.User.GetCustomerUsers(email);
        }

        public Task<IEnumerable<GIGL.POST.Core.Domain.User>> GetCustomerUsers()
        {
            return _unitOfWork.User.GetCustomerUsers();
        }

        public Task<IEnumerable<GIGL.POST.Core.Domain.User>> GetCorporateCustomerUsers()
        {
            return _unitOfWork.User.GetCorporateCustomerUsers();
        }

        public async Task<ServiceCentreDTO> getServiceCenterById(int ServiceCenterId)
        {
            return await _serviceCentreService.GetServiceCentreById(ServiceCenterId);
        }

        public IQueryable<GIGL.POST.Core.Domain.User> GetCorporateCustomerUsersAsQueryable()
        {
            return _unitOfWork.User.GetCorporateCustomerUsersAsQueryable();
        }

        public Task<IEnumerable<GIGL.POST.Core.Domain.User>> GetPartnerUsers()
        {
            return _unitOfWork.User.GetPartnerUsers();
        }

        //Get all system users
        public Task<IEnumerable<GIGL.POST.Core.Domain.User>> GetSystemUsers()
        {
            return _unitOfWork.User.GetSystemUsers();
        }

        public Task<IEnumerable<GIGL.POST.Core.Domain.User>> GetDispatchCaptains()
        {
            return _unitOfWork.User.GetDispatchCaptains();
        }

        public Task<IEnumerable<GIGL.POST.Core.Domain.User>> GetDispatchRiders()
        {
            return _unitOfWork.User.GetDispatchRiders();
        }

        //Get a user by Id using Guid from identity implement of EF
        public async Task<UserDTO> GetUserById(string Id)
        {
                var user = await _unitOfWork.User.GetUserById(Id);

            if (user == null)
            {
                throw new GenericException("User Information Not Found!", $"{(int)HttpStatusCode.NotFound}");
            }

            var userDto = Mapper.Map<UserDTO>(user);

            return await Task.FromResult(userDto);
        }

        public async Task<UserDTO> GetUserByEmail(string email)
        {
            var user = await _unitOfWork.User.GetUserByEmail(email);

            if (user == null)
            {
                throw new GenericException("User Information Not Found!", $"{(int)HttpStatusCode.NotFound}");
            }

            return Mapper.Map<UserDTO>(user);
        }

        //Get a user by Id using int custom UserId added to users table
        public async Task<UserDTO> GetUserById(int userId)
        {
            var user = await _unitOfWork.User.GetUserById(userId);

            if (user == null)
            {
                throw new GenericException("User Information Not Found!", $"{(int)HttpStatusCode.NotFound}");
            }

            return Mapper.Map<UserDTO>(user);
        }

        public async Task<string> GetUserChannelCode()
        {
            var currentUserId = await GetCurrentUserId();
            var user = await GetUserById(currentUserId);

            if (user == null)
            {
                throw new GenericException("User Information Not Found!", $"{(int)HttpStatusCode.NotFound}");
            }

            return user.UserChannelCode;
        }

        public async Task<UserDTO> GetUserByChannelCode(string channelCode)
        {
            var user = await _unitOfWork.User.GetUserByChannelCode(channelCode);

            if (user == null)
            {
                throw new GenericException("User Information Not Found!", $"{(int)HttpStatusCode.NotFound}");
            }

            return Mapper.Map<UserDTO>(user);
        }


        public async Task<IdentityResult> RemoveUser(string userId)
        {
            //Remove user, wallet and customer (Inidvidual or company)

            var user = await _unitOfWork.User.GetUserById(userId);

            if (user == null)
            {
                throw new GenericException("User Information Not Found!", $"{(int)HttpStatusCode.NotFound}");
            }

            if (user.UserChannelType.Equals(UserChannelType.Corporate) || user.UserChannelType.Equals(UserChannelType.Ecommerce))
            {
                await RemoveCompanyCustomer(user.UserChannelCode);
            }

            if (user.UserChannelType.Equals(UserChannelType.IndividualCustomer))
            {
                await RemoveIndividualCustomer(user.UserChannelCode);
            }

            await RemoveWallet(user.UserChannelCode);
            await _unitOfWork.CompleteAsync();
            return await _unitOfWork.User.Remove(userId);
        }

        private async Task RemoveCompanyCustomer(string customerCode)
        {
            var company = await _unitOfWork.Company.GetAsync(x => x.CustomerCode == customerCode);
            if (company != null)
            {
                _unitOfWork.Company.Remove(company);
            }
        }

        private async Task RemoveIndividualCustomer(string customerCode)
        {
            var individual = await _unitOfWork.IndividualCustomer.GetAsync(x => x.CustomerCode == customerCode);
            if (individual != null)
            {
                _unitOfWork.IndividualCustomer.Remove(individual);
            }
        }

        private async Task RemoveWallet(string customerCode)
        {
            var wallet = await _unitOfWork.Wallet.GetAsync(x => x.CustomerCode == customerCode);
            if (wallet != null)
            {
                _unitOfWork.Wallet.Remove(wallet);
            }
        }

        public async Task<IdentityResult> UpdateUser(string userid, UserDTO userDto)
        {
            var user = await _unitOfWork.User.GetUserById(userid);

            if (user == null)
            {
                throw new GenericException("User does not exist!", $"{(int)HttpStatusCode.NotFound}");
            }

            user.Department = userDto.Department;
            user.Designation = user.Designation;
            user.Email = userDto.Email;
            user.FirstName = userDto.FirstName;
            user.LastName = userDto.LastName;
            user.Gender = userDto.Gender;
            user.Organisation = userDto.Organisation;
            user.IsActive = userDto.IsActive;
            user.PhoneNumber = userDto.PhoneNumber;
            user.SystemUserId = userDto.SystemUserId;
            user.SystemUserRole = userDto.SystemUserRole;
            user.UserName = userDto.Username;
            user.RegistrationReferrercode = userDto.RegistrationReferrercode;

            user.UserChannelCode = userDto.UserChannelCode;
            user.UserChannelPassword = userDto.UserChannelPassword;
            user.UserChannelType = userDto.UserChannelType;
            user.PictureUrl = userDto.PictureUrl;
            user.UserActiveCountryId = userDto.UserActiveCountryId;
            user.AssignedEcommerceCustomer = userDto.AssignedEcommerceCustomer;

            return await _unitOfWork.User.UpdateUser(userid, user);

        }

        public async Task<IdentityResult> UpdateMagayaUser(string userid, UserDTO userDto)
        {
            var user = await _unitOfWork.User.GetUserById(userid);

            if (user == null)
            {
                throw new GenericException("User does not exist!", $"{(int)HttpStatusCode.NotFound}");
            }

            user.IsMagaya = userDto.IsMagaya;
            return await _unitOfWork.User.UpdateUser(userid, user);

        }

        public async Task<IdentityResult> ActivateUser(string userid, bool val)
        {
            var user = await _unitOfWork.User.GetUserById(userid);

            if (user == null)
            {
                throw new GenericException("User does not exist!", $"{(int)HttpStatusCode.NotFound}");
            }

            user.IsActive = val;

            return await _unitOfWork.User.UpdateUser(userid, user);
        }

        public Task<AppRole> GetRoleById(string roleId)
        {
            var role = _unitOfWork.User.GetRoleById(roleId);
            return role;
        }

        public Task<AppRole> GetRoleByName(string roleName)
        {
            var role = _unitOfWork.User.GetRoleById(roleName);
            return role;
        }

        public Task<IEnumerable<AppRole>> GetRoles()
        {
            var role = _unitOfWork.User.GetRoles();
            return role;
        }

        public async Task<IdentityResult> AddRole(RoleDTO roleDTO)
        {
            if (await GetRoleByName(roleDTO.Name) != null)
            {
                throw new GenericException("Role exist already!", $"{(int)HttpStatusCode.Forbidden}");
            }

            var result = await _unitOfWork.User.AddRole(roleDTO.Name);
            return result;
        }

        public Task<IdentityResult> RemoveRole(string roleId)
        {
            var result = _unitOfWork.User.RemoveRole(roleId);
            return result;
        }

        public Task<IdentityResult> UpdateRole(string roleId, RoleDTO roleDTO)
        {
            var result = _unitOfWork.User.UpdateRole(roleId, roleDTO);
            return result;
        }

        public async Task<IdentityResult> AddToRoleAsync(string userId, string name)
        {
            var result = await _unitOfWork.User.AddToRoleAsync(userId, name);
            await _unitOfWork.CompleteAsync();

            //update all users that belong to this system user
            var usersList = await GetUsers();
            var usersInSystemUsers = usersList.ToList().Where(s => s.SystemUserId == userId);
            foreach (var userInList in usersInSystemUsers)
            {
                await RoleSettings(userId, userInList.Id);
            }

            return result;
        }

        public Task<IList<string>> GetUserRoles(string userid)
        {
            var result = _unitOfWork.User.GetUserRoles(userid);
            return result;
        }

        public Task<bool> IsInRoleAsync(string userid, string name)
        {
            var result = _unitOfWork.User.IsInRoleAsync(userid, name);
            return result;
        }

        public async Task<IdentityResult> RemoveFromRoleAsync(string userId, string name)
        {
            var result = await _unitOfWork.User.RemoveFromRoleAsync(userId, name);
            await _unitOfWork.CompleteAsync();

            //update all users that belong to this system user
            var usersList = await GetUsers();
            var usersInSystemUsers = usersList.ToList().Where(s => s.SystemUserId == userId);
            foreach (var userInList in usersInSystemUsers)
            {
                await RoleSettings(userId, userInList.Id);
            }

            return result;
        }

        public async Task<IdentityResult> AddClaimAsync(string userid, Claim claim)
        {
            var user = await _unitOfWork.User.GetUserById(userid);

            if (user == null)
            {
                throw new GenericException("User does not exist!", $"{(int)HttpStatusCode.NotFound}");
            }

            var userClaims = await GetClaimsAsync(userid);

            foreach (var cl in userClaims)
            {
                if (cl.Type == "Privilege")
                {
                    await RemoveClaimAsync(userid, cl);
                }
            }

            var result = await _unitOfWork.User.AddClaimAsync(userid, claim);
            await _unitOfWork.CompleteAsync();

            //update all users that belong to this system user
            var usersList = await GetUsers();
            var usersInSystemUsers = usersList.ToList().Where(s => s.SystemUserId == userid);
            foreach (var userInList in usersInSystemUsers)
            {
                await RoleSettings(userid, userInList.Id);
            }

            return result;
        }

        public async Task<IdentityResult> RemoveClaimAsync(string userid, Claim claim)
        {
            var user = await _unitOfWork.User.GetUserById(userid);

            if (user == null)
            {
                throw new GenericException("User does not exist!", $"{(int)HttpStatusCode.NotFound}");
            }
            var result = await _unitOfWork.User.RemoveClaimAsync(userid, claim);
            await _unitOfWork.CompleteAsync();

            //update all users that belong to this system user
            var usersList = await GetUsers();
            var usersInSystemUsers = usersList.ToList().Where(s => s.SystemUserId == userid);
            foreach (var userInList in usersInSystemUsers)
            {
                await RoleSettings(userid, userInList.Id);
            }

            return result;
        }

        public async Task<IList<Claim>> GetClaimsAsync(string userid)
        {
            var result = await _unitOfWork.User.GetClaimsAsync(userid);
            return result.OrderBy(x => x.Value).ToList();
        }

        public async Task<string> GetCurrentUserId()
        {
            var userId = HttpContext.Current?.User?.Identity?.GetUserId();
            return await Task.FromResult(!string.IsNullOrEmpty(userId) ? userId : "Anonymous");
        }

        public async Task<bool> RoleSettings(string systemuserid, string userid)
        {
            var result = false;

            try
            {
                List<Claim> nonActivityClaim = new List<Claim>();

                // get the users
                var systemUser = await GetUserById(systemuserid);
                if (systemUser.UserType != UserType.System)
                {
                    throw new GenericException("User is not a System Type!", $"{(int)HttpStatusCode.Forbidden}");
                }

                // get the roles and claims
                var systemUserRoles = await GetUserRoles(systemuserid);
                var userRoles = await GetUserRoles(userid);

                var systemUserClaims = await GetClaimsAsync(systemuserid);
                var userClaims = await GetClaimsAsync(userid);

                // remove all roles and activity claims from the user
                foreach (var role in userRoles)
                {
                    await RemoveFromRoleAsync(userid, role);
                }

                foreach (var claim in userClaims)
                {
                    if (claim.Type == "Activity")
                    {
                        await RemoveClaimAsync(userid, claim);
                    }
                    else
                    {
                        nonActivityClaim.Add(claim);
                    }
                }

                // assign roles and claim from systemUser
                foreach (var role in systemUserRoles)
                {
                    await AddToRoleAsync(userid, role);
                }

                foreach (var claim in systemUserClaims)
                {
                    if (claim.Type == "Activity")
                    {
                        await AddClaimAsync(userid, claim);
                    }
                }

                foreach (var claim in nonActivityClaim)
                {
                    await AddClaimAsync(userid, claim);
                }

                //update the user with the system user role
                var userDTO = await GetUserById(userid);

                if (systemUserRoles.Any())
                {
                    userDTO.SystemUserId = systemuserid;
                    userDTO.SystemUserRole = systemUser.FirstName;
                }
                else
                {
                    userDTO.SystemUserId = "";
                    userDTO.SystemUserRole = "";
                }

                await UpdateUser(userid, userDTO);

                // complete transaction if all actions are successful
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw ex;
            }

            return result;
        }

        public async Task<bool> CheckSCA()
        {

            var result = false;

            var currentUserId = await GetCurrentUserId();
            var currentUser = await GetUserById(currentUserId);
            var userClaims = await GetClaimsAsync(currentUserId);

            string[] claimValue = null;
            foreach (var claim in userClaims)
            {
                if (claim.Type == "Privilege")
                {
                    claimValue = claim.Value.Split(':');   // format stringName:stringValue
                }
            }
            if (claimValue == null)
            {
                return result;
            }

            if (
                claimValue[0] == "ServiceCentre")
            {
                result = true;
            }

            return result;
        }

        public async Task<UserDTO> retUser()
        {
            var currentUserId = await GetCurrentUserId();
            var currentUser = await GetUserById(currentUserId);

            return currentUser;
        }

        public async Task<UserDTO> retServiceCenter()
        {
            var currentUserId = await GetCurrentUserId();
            var currentUser = await GetUserById(currentUserId);

            return currentUser;
        }

        public async Task<bool> CheckPriviledge()
        {
            var result = false;

            try
            {
                var currentUserId = await GetCurrentUserId();
                var currentUser = await GetUserById(currentUserId);
                var userClaims = await GetClaimsAsync(currentUserId);

                string[] claimValue = null;
                foreach (var claim in userClaims)
                {
                    if (claim.Type == "Privilege")
                    {
                        claimValue = claim.Value.Split(':');   // format stringName:stringValue
                    }
                }
                if (claimValue == null)
                {
                    return result;
                }

                if (claimValue[0] == "Global" ||
                    claimValue[0] == "Region" ||
                    claimValue[0] == "Station" ||
                    claimValue[0] == "ServiceCentre")
                {
                    result = true;
                }
            }
            catch (Exception ex) { throw ex; }

            return result;
        }

        public async Task<ServiceCentreDTO[]> GetCurrentServiceCenter()
        {
            var sc = new ServiceCentreDTO[] { };

            // get current user
            try
            {
                var currentUserId = await GetCurrentUserId();
                var currentUser = await GetUserById(currentUserId);
                var userClaims = await GetClaimsAsync(currentUserId);

                //currentUser.ServiceCentres

                string[] claimValue = null;
                foreach (var claim in userClaims)
                {
                    if (claim.Type == "Privilege")
                    {
                        claimValue = claim.Value.Split(':');   // format stringName:stringValue
                    }
                }

                if (claimValue == null)
                {
                    throw new GenericException($"User {currentUser.Username} does not have a priviledge claim.", $"{(int)HttpStatusCode.NotFound}");
                }

                if (claimValue[0] == "Global")
                {
                    sc = new ServiceCentreDTO[] { };
                }
                else if (claimValue[0] == "Region")
                {
                    var regionId = int.Parse(claimValue[1]);
                    var regionServiceCentreMappingDTOList = await _regionServiceCentreMappingService.GetServiceCentresInRegion(regionId);
                    sc = regionServiceCentreMappingDTOList.Select(s => s.ServiceCentre).ToArray();
                }
                else if (claimValue[0] == "Station")
                {
                    var stationId = int.Parse(claimValue[1]);
                    var serviceCentres = await _serviceCentreService.GetServiceCentres();
                    sc = serviceCentres.Where(s => s.StationId == stationId).ToArray();
                }
                else if (claimValue[0] == "ServiceCentre")
                {
                    int serviceCenterId = int.Parse(claimValue[1]);
                    var serviceCentres = await _serviceCentreService.GetServiceCentres();
                    sc = serviceCentres.Where(s => s.ServiceCentreId == serviceCenterId).ToArray();
                }
                else if (claimValue[0] == "Public")
                {
                    sc = new ServiceCentreDTO[] { };
                }
                else
                {
                    throw new GenericException($"User {currentUser.Username} does not have a priviledge claim.", $"{(int)HttpStatusCode.NotFound}");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return sc;
        }

        public async Task<int[]> GetPriviledgeServiceCenters(string currentUserId)
        {
            int[] serviceCenterIds = { };   //empty array
            // get current user
            try
            {
                var currentUser = await GetUserById(currentUserId);
                var userClaims = await GetClaimsAsync(currentUserId);

                string[] claimValue = null;
                foreach (var claim in userClaims)
                {
                    if (claim.Type == "Privilege")
                    {
                        claimValue = claim.Value.Split(':');   // format stringName:stringValue
                    }
                }
                if (claimValue == null)
                {
                    throw new GenericException($"User {currentUser.Username} does not have a priviledge claim.", $"{(int)HttpStatusCode.NotFound}");
                }

                if (claimValue[0] == "Global")
                {
                    serviceCenterIds = new int[] { };
                }
                else if (claimValue[0] == "Region")
                {
                    int regionId = int.Parse(claimValue[1]);
                    var regionServiceCentreMappingDTOList = await _regionServiceCentreMappingService.GetServiceCentresInRegion(regionId);
                    regionServiceCentreMappingDTOList = regionServiceCentreMappingDTOList.Where(x => x.ServiceCentre != null).ToList();
                    serviceCenterIds = regionServiceCentreMappingDTOList.Select(s => s.ServiceCentre.ServiceCentreId).ToArray();
                }
                else if (claimValue[0] == "Station")
                {
                    int stationId = int.Parse(claimValue[1]);
                    var serviceCentres = await _serviceCentreService.GetServiceCentres();
                    serviceCenterIds = serviceCentres.Where(s => s.StationId == stationId).Select(s => s.ServiceCentreId).ToArray();
                }
                else if (claimValue[0] == "ServiceCentre")
                {
                    int serviceCenterId = int.Parse(claimValue[1]);
                    serviceCenterIds = new int[] { serviceCenterId };
                }
                else if (claimValue[0] == "Public")
                {
                    serviceCenterIds = new int[] { };
                }
                else
                {
                    throw new GenericException($"User {currentUser.Username} does not have a priviledge claim.", $"{(int)HttpStatusCode.NotFound}");
                }
            }
            catch (Exception)
            {
                throw;
            }

            return serviceCenterIds;
        }


        public async Task<int[]> GetPriviledgeServiceCenters()
        {
            int[] serviceCenterIds = { };   //empty array
            // get current user
            try
            {
                var currentUserId = await GetCurrentUserId();
                var currentUser = await GetUserById(currentUserId);
                var userClaims = await GetClaimsAsync(currentUserId);

                //currentUser.ServiceCentres

                string[] claimValue = null;
                foreach (var claim in userClaims)
                {
                    if (claim.Type == "Privilege")
                    {
                        claimValue = claim.Value.Split(':');   // format stringName:stringValue
                    }
                }
                if (claimValue == null)
                {
                    throw new GenericException($"User {currentUser.Username} does not have a priviledge claim.", $"{(int)HttpStatusCode.NotFound}");
                }

                if (claimValue[0] == "Global")
                {
                    serviceCenterIds = new int[] { };
                }
                else if (claimValue[0] == "Region")
                {
                    var regionId = int.Parse(claimValue[1]);
                    var regionServiceCentreMappingDTOList = await _regionServiceCentreMappingService.GetServiceCentresInRegion(regionId);
                    serviceCenterIds = regionServiceCentreMappingDTOList.Where(s => s.ServiceCentre != null).Select(s => s.ServiceCentre.ServiceCentreId).ToArray();
                }
                else if (claimValue[0] == "Station")
                {
                    var stationId = int.Parse(claimValue[1]);
                    var serviceCentres = await _serviceCentreService.GetServiceCentres();
                    serviceCenterIds = serviceCentres.Where(s => s.StationId == stationId).Select(s => s.ServiceCentreId).ToArray();
                }
                else if (claimValue[0] == "ServiceCentre")
                {
                    int serviceCenterId = int.Parse(claimValue[1]);
                    serviceCenterIds = new int[] { serviceCenterId };
                }
                else if (claimValue[0] == "Public")
                {
                    serviceCenterIds = new int[] { };
                }
                else
                {
                    throw new GenericException($"User {currentUser.Username} does not have a priviledge claim.", $"{(int)HttpStatusCode.NotFound}");
                }
            }
            catch (Exception)
            {
                throw;
            }

            return serviceCenterIds;
        }

        public async Task<ServiceCentreDTO> GetDefaultServiceCenter()
        {
            var defaultServiceCenter = await _serviceCentreService.GetDefaultServiceCentre();
            return defaultServiceCenter;
        }

        public async Task<ServiceCentreDTO> GetGIGGOServiceCentre()
        {
            var gigGOServiceCenter = await _serviceCentreService.GetGIGGOServiceCentre();
            return gigGOServiceCenter;
        }

        public async Task<ServiceCentreDTO> GetInternationalOutBoundServiceCentre()
        {
            var gigGOServiceCenter = await _serviceCentreService.GetInternationalOutBoundServiceCentre();
            return gigGOServiceCenter;
        }


        //change user password by Admin
        public async Task<IdentityResult> ResetPassword(string userid, string password)
        {
            var user = await _unitOfWork.User.GetUserById(userid);

            if (user == null || string.IsNullOrEmpty(password))
            {
                throw new GenericException("User does not exist!", $"{(int)HttpStatusCode.NotFound}");
            }

            user.PasswordExpireDate = DateTime.Now;
            user.IsRequestNewPassword = true;
            var result = await _unitOfWork.User.ResetPassword(userid, password);
            await _unitOfWork.CompleteAsync();
            return result;
        }

        public async Task<IdentityResult> ChangePassword(string userid, string currentPassword, string newPassword)
        {
            var user = await _unitOfWork.User.GetUserById(userid);
            if (user == null || string.IsNullOrEmpty(newPassword))
            {
                throw new GenericException("Operation could not complete, kindly supply valid credential", $"{(int)HttpStatusCode.Forbidden}");
            }

            if (!await _unitOfWork.User.CheckPasswordAsync(user, currentPassword))
            {
                throw new GenericException("Operation could not complete, kindly supply valid credential", $"{(int)HttpStatusCode.Forbidden}");
            }

            user.PasswordExpireDate = DateTime.Now;
            user.IsRequestNewPassword = false;
            var result = await _unitOfWork.User.ChangePassword(userid, currentPassword, newPassword);
            await _unitOfWork.CompleteAsync();
            return result;
        }

        public async Task<IdentityResult> ResetExpiredPassword(string email, string currentPassword, string newPassword)
        {
            var user = await _unitOfWork.User.GetUserByEmail(email);
            if (user == null || string.IsNullOrEmpty(newPassword))
            {
                throw new GenericException("Operation could not complete, kindly supply valid credential", $"{(int)HttpStatusCode.Forbidden}");
            }

            if (!await _unitOfWork.User.CheckPasswordAsync(user, currentPassword))
            {
                throw new GenericException("Operation could not complete, kindly supply valid credential", $"{(int)HttpStatusCode.Forbidden}");
            }

            user.PasswordExpireDate = DateTime.Now;
            var result = await _unitOfWork.User.ChangePassword(user.Id, currentPassword, newPassword);
            await _unitOfWork.CompleteAsync();
            return result;
        }
        public async Task<IdentityResult> ForgotPassword(string email, string password)
        {
            var result = new IdentityResult();

            var user = await _unitOfWork.User.GetUserByEmail(email);            

            if (user != null)
            {
                result = await ResetPassword(user.Id, password);
                await _unitOfWork.CompleteAsync();                
            }
            return result;
        }
                
        /// <summary>
        /// Code for first migration
        /// 1. Reset the users password
        /// 2. Add users to roles
        /// </summary>
        /// <returns></returns>
        private async Task<int> CodeToAddUsersToAspNetUserRolesTable()
        {
            try
            {
                //1. reset the users password
                var users = await _unitOfWork.User.GetUsers();
                var allEmployees = users.Where(s => s.UserType == UserType.Regular && s.UserChannelType == UserChannelType.Employee);

                foreach (var employee in allEmployees)
                {
                    if (employee.UserChannelCode == "EMP000900")
                    {
                        continue;
                    }

                    //reset the password
                    var password = GeneratePassword();
                    var result = await _unitOfWork.User.ResetPassword(employee.Id, password);

                    //update UserChannelPassword
                    var user = await _unitOfWork.User.GetUserById(employee.Id);
                    user.UserChannelPassword = password;
                    await _unitOfWork.CompleteAsync();
                }

                //2. Add users to roles
                var systemUsers = await _unitOfWork.User.GetSystemUsers();
                foreach (var employee in allEmployees)
                {
                    //update SystemUserId and SystemUserRole
                    if (employee.Designation.Trim() == "Accountant".Trim())
                    {
                        var systemUser = systemUsers.FirstOrDefault(s => s.FirstName.Trim() == "Accountant".Trim());
                        var user = await _unitOfWork.User.GetUserById(employee.Id);
                        user.SystemUserId = systemUser.Id;
                        user.SystemUserRole = systemUser.FirstName;
                    }
                    else if (employee.Designation.Trim() == "Asst. Dispatch Supervisor".Trim())
                    {
                        var systemUser = systemUsers.FirstOrDefault(s => s.FirstName.Trim() == "Dispatch Coordinator".Trim());
                        var user = await _unitOfWork.User.GetUserById(employee.Id);
                        user.SystemUserId = systemUser.Id;
                        user.SystemUserRole = systemUser.FirstName;
                    }
                    else if (employee.Designation.Trim() == "Business Development Exec".Trim())
                    {
                        var systemUser = systemUsers.FirstOrDefault(s => s.FirstName.Trim() == "Business Development Executive".Trim());
                        var user = await _unitOfWork.User.GetUserById(employee.Id);
                        user.SystemUserId = systemUser.Id;
                        user.SystemUserRole = systemUser.FirstName;
                    }
                    else if (employee.Designation.Trim() == "Call Center Agent".Trim())
                    {
                        var systemUser = systemUsers.FirstOrDefault(s => s.FirstName.Trim() == "Customer Care".Trim());
                        var user = await _unitOfWork.User.GetUserById(employee.Id);
                        user.SystemUserId = systemUser.Id;
                        user.SystemUserRole = systemUser.FirstName;
                    }
                    else if (employee.Designation.Trim() == "CEO, GIGL HOUSTON".Trim())
                    {
                        var systemUser = systemUsers.FirstOrDefault(s => s.FirstName.Trim() == "MD".Trim());
                        var user = await _unitOfWork.User.GetUserById(employee.Id);
                        user.SystemUserId = systemUser.Id;
                        user.SystemUserRole = systemUser.FirstName;
                    }
                    else if (employee.Designation.Trim() == "Coordinator".Trim())
                    {
                        var systemUser = systemUsers.FirstOrDefault(s => s.FirstName.Trim() == "Coordinator".Trim());
                        var user = await _unitOfWork.User.GetUserById(employee.Id);
                        user.SystemUserId = systemUser.Id;
                        user.SystemUserRole = systemUser.FirstName;
                    }
                    else if (employee.Designation.Trim() == "Customer Care Officer".Trim())
                    {
                        var systemUser = systemUsers.FirstOrDefault(s => s.FirstName.Trim() == "Customer Care".Trim());
                        var user = await _unitOfWork.User.GetUserById(employee.Id);
                        user.SystemUserId = systemUser.Id;
                        user.SystemUserRole = systemUser.FirstName;
                    }
                    else if (employee.Designation.Trim() == "Customer Experience Manger".Trim())
                    {
                        var systemUser = systemUsers.FirstOrDefault(s => s.FirstName.Trim() == "Customer Experience Manager".Trim());
                        var user = await _unitOfWork.User.GetUserById(employee.Id);
                        user.SystemUserId = systemUser.Id;
                        user.SystemUserRole = systemUser.FirstName;
                    }
                    else if (employee.Designation.Trim() == "Director".Trim())
                    {
                        var systemUser = systemUsers.FirstOrDefault(s => s.FirstName.Trim() == "Director".Trim());
                        var user = await _unitOfWork.User.GetUserById(employee.Id);
                        user.SystemUserId = systemUser.Id;
                        user.SystemUserRole = systemUser.FirstName;
                    }
                    else if (employee.Designation.Trim() == "Dispatch Coordinator".Trim())
                    {
                        var systemUser = systemUsers.FirstOrDefault(s => s.FirstName.Trim() == "Dispatch Coordinator".Trim());
                        var user = await _unitOfWork.User.GetUserById(employee.Id);
                        user.SystemUserId = systemUser.Id;
                        user.SystemUserRole = systemUser.FirstName;
                    }
                    else if (employee.Designation.Trim() == "Dispatch Supervisor".Trim())
                    {
                        var systemUser = systemUsers.FirstOrDefault(s => s.FirstName.Trim() == "Dispatch Coordinator".Trim());
                        var user = await _unitOfWork.User.GetUserById(employee.Id);
                        user.SystemUserId = systemUser.Id;
                        user.SystemUserRole = systemUser.FirstName;
                    }
                    else if (employee.Designation.Trim() == "Ecommerce Supervisor".Trim())
                    {
                        var systemUser = systemUsers.FirstOrDefault(s => s.FirstName.Trim() == "Supervisors".Trim());
                        var user = await _unitOfWork.User.GetUserById(employee.Id);
                        user.SystemUserId = systemUser.Id;
                        user.SystemUserRole = systemUser.FirstName;
                    }
                    else if (employee.Designation.Trim() == "Gateway Officer".Trim())
                    {
                        var systemUser = systemUsers.FirstOrDefault(s => s.FirstName.Trim() == "Gateway Officer".Trim());
                        var user = await _unitOfWork.User.GetUserById(employee.Id);
                        user.SystemUserId = systemUser.Id;
                        user.SystemUserRole = systemUser.FirstName;
                    }
                    else if (employee.Designation.Trim() == "Gateway Supervisor".Trim())
                    {
                        var systemUser = systemUsers.FirstOrDefault(s => s.FirstName.Trim() == "Gateway Supervisor".Trim());
                        var user = await _unitOfWork.User.GetUserById(employee.Id);
                        user.SystemUserId = systemUser.Id;
                        user.SystemUserRole = systemUser.FirstName;
                    }
                    else if (employee.Designation.Trim() == "HR Business Partner".Trim())
                    {
                        var systemUser = systemUsers.FirstOrDefault(s => s.FirstName.Trim() == "HR Business Partner".Trim());
                        var user = await _unitOfWork.User.GetUserById(employee.Id);
                        user.SystemUserId = systemUser.Id;
                        user.SystemUserRole = systemUser.FirstName;
                    }
                    else if (employee.Designation.Trim() == "Maintenance Supervisor".Trim())
                    {
                        var systemUser = systemUsers.FirstOrDefault(s => s.FirstName.Trim() == "Supervisors".Trim());
                        var user = await _unitOfWork.User.GetUserById(employee.Id);
                        user.SystemUserId = systemUser.Id;
                        user.SystemUserRole = systemUser.FirstName;
                    }
                    else if (employee.Designation.Trim() == "Pick-up Agent".Trim())
                    {
                        var systemUser = systemUsers.FirstOrDefault(s => s.FirstName.Trim() == "Dispatch Rider".Trim());
                        var user = await _unitOfWork.User.GetUserById(employee.Id);
                        user.SystemUserId = systemUser.Id;
                        user.SystemUserRole = systemUser.FirstName;
                    }
                    else if (employee.Designation.Trim() == "Processing Officer/Ecommerce".Trim())
                    {
                        var systemUser = systemUsers.FirstOrDefault(s => s.FirstName.Trim() == "Service Center Agent".Trim());
                        var user = await _unitOfWork.User.GetUserById(employee.Id);
                        user.SystemUserId = systemUser.Id;
                        user.SystemUserRole = systemUser.FirstName;
                    }
                    else if (employee.Designation.Trim() == "Regional Manager".Trim())
                    {
                        var systemUser = systemUsers.FirstOrDefault(s => s.FirstName.Trim() == "Regional Manager".Trim());
                        var user = await _unitOfWork.User.GetUserById(employee.Id);
                        user.SystemUserId = systemUser.Id;
                        user.SystemUserRole = systemUser.FirstName;
                    }
                    else if (employee.Designation.Trim() == "Service Center Agent".Trim())
                    {
                        var systemUser = systemUsers.FirstOrDefault(s => s.FirstName.Trim() == "Service Center Agent".Trim());
                        var user = await _unitOfWork.User.GetUserById(employee.Id);
                        user.SystemUserId = systemUser.Id;
                        user.SystemUserRole = systemUser.FirstName;
                    }
                    else if (employee.Designation.Trim() == "Service Center Agent/ E-commerce".Trim())
                    {
                        var systemUser = systemUsers.FirstOrDefault(s => s.FirstName.Trim() == "Service Center Agent".Trim());
                        var user = await _unitOfWork.User.GetUserById(employee.Id);
                        user.SystemUserId = systemUser.Id;
                        user.SystemUserRole = systemUser.FirstName;
                    }
                    else if (employee.Designation.Trim() == "Service Center Agent/Ecommerce".Trim())
                    {
                        var systemUser = systemUsers.FirstOrDefault(s => s.FirstName.Trim() == "Service Center Agent".Trim());
                        var user = await _unitOfWork.User.GetUserById(employee.Id);
                        user.SystemUserId = systemUser.Id;
                        user.SystemUserRole = systemUser.FirstName;
                    }
                    else if (employee.Designation.Trim() == "Service Center Assistant".Trim())
                    {
                        var systemUser = systemUsers.FirstOrDefault(s => s.FirstName.Trim() == "Service Center Agent".Trim());
                        var user = await _unitOfWork.User.GetUserById(employee.Id);
                        user.SystemUserId = systemUser.Id;
                        user.SystemUserRole = systemUser.FirstName;
                    }
                    else if (employee.Designation.Trim() == "Service Center Assistant/ Ecommerce".Trim())
                    {
                        var systemUser = systemUsers.FirstOrDefault(s => s.FirstName.Trim() == "Service Center Agent".Trim());
                        var user = await _unitOfWork.User.GetUserById(employee.Id);
                        user.SystemUserId = systemUser.Id;
                        user.SystemUserRole = systemUser.FirstName;
                    }
                    else if (employee.Designation.Trim() == "Service Center Supervisor".Trim())
                    {
                        var systemUser = systemUsers.FirstOrDefault(s => s.FirstName.Trim() == "Supervisors ".Trim());
                        var user = await _unitOfWork.User.GetUserById(employee.Id);
                        user.SystemUserId = systemUser.Id;
                        user.SystemUserRole = systemUser.FirstName;
                    }
                    else if (employee.Designation.Trim() == "Snr. Accountant".Trim())
                    {
                        var systemUser = systemUsers.FirstOrDefault(s => s.FirstName.Trim() == "Accountant".Trim());
                        var user = await _unitOfWork.User.GetUserById(employee.Id);
                        user.SystemUserId = systemUser.Id;
                        user.SystemUserRole = systemUser.FirstName;
                    }
                    else if (employee.Designation.Trim() == "Software Developer".Trim())
                    {
                        var systemUser = systemUsers.FirstOrDefault(s => s.FirstName.Trim() == "Administrator".Trim());
                        var user = await _unitOfWork.User.GetUserById(employee.Id);
                        user.SystemUserId = systemUser.Id;
                        user.SystemUserRole = systemUser.FirstName;
                    }
                    else if (employee.Designation.Trim() == "Tracking Officer".Trim())
                    {
                        var systemUser = systemUsers.FirstOrDefault(s => s.FirstName.Trim() == "Service Center Agent".Trim());
                        var user = await _unitOfWork.User.GetUserById(employee.Id);
                        user.SystemUserId = systemUser.Id;
                        user.SystemUserRole = systemUser.FirstName;
                    }
                }
                await _unitOfWork.CompleteAsync();

            }
            catch (Exception ex)
            {
                throw ex;
            }


            return await Task.FromResult(0);
        }

        private async Task<int> CodeToAddUsersToAspNetUserRolesTable_LOCATION()
        {
            try
            {
                var users = await _unitOfWork.User.GetUsers();
                var allEmployees = users.Where(s => s.UserType == UserType.Regular && s.UserChannelType == UserChannelType.Employee);


                //1.Add to user to systemRole
                foreach (var employee in allEmployees)
                {
                    if (employee.SystemUserId != null)
                    {
                        await RoleSettings(employee.SystemUserId, employee.Id);
                    }
                }


                //2. Add priviledge for ServiceCentre
                await _unitOfWork.CompleteAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return await Task.FromResult(0);
        }

        private static string GeneratePassword(int length = 6)
        {
            var strippedText = Guid.NewGuid().ToString().Replace("-", string.Empty);
            return strippedText.Substring(0, length);
        }

        public async Task<UserDTO> GetUserByPhone(string PhoneNumber)
        {
            var user = await _unitOfWork.User.GetUserByPhoneNumber(PhoneNumber);

            if (user == null)
            {
                throw new GenericException("Phone number does not exist!", $"{(int)HttpStatusCode.NotFound}");
            }
            return Mapper.Map<UserDTO>(user);
        }

        public async Task<int[]> GetPriviledgeCountryIds(string currentUserId)
        {
            //1. get service centres
            int[] serviceCenterIds = await GetPriviledgeServiceCenters(currentUserId);

            //2. get the countries based on the service centre
            var serviceCentres =
                _unitOfWork.ServiceCentre.GetAll("Station, Station.State").Where(s => serviceCenterIds.Contains(s.ServiceCentreId)).ToList();

            //3.
            var countryIds = new HashSet<int>();
            foreach (var serviceCentre in serviceCentres)
            {
                var countryId = serviceCentre.Station.State.CountryId;
                countryIds.Add(countryId);
            }

            return countryIds.ToArray();
        }

        public async Task<int[]> GetPriviledgeCountryIds()
        {
            //1. get service centres
            int[] serviceCenterIds = await GetPriviledgeServiceCenters();

            //2. get the countries based on the service centre
            var serviceCentres =
                _unitOfWork.ServiceCentre.GetAll("Station, Station.State").Where(s => serviceCenterIds.Contains(s.ServiceCentreId)).ToList();

            //3.
            var countryIds = new HashSet<int>();
            foreach (var serviceCentre in serviceCentres)
            {
                var countryId = serviceCentre.Station.State.CountryId;
                countryIds.Add(countryId);
            }

            return countryIds.ToArray();
        }

        public async Task<List<CountryDTO>> GetPriviledgeCountrys(string currentUserId)
        {
            //1. get countries
            int[] countryIds = await GetPriviledgeCountryIds(currentUserId);

            //2. get the countries based on the service centre
            var countries = _unitOfWork.Country.GetAllAsQueryable().Where(s => countryIds.Contains(s.CountryId)).ToList();

            //3.
            var uniqueCountryIds = new HashSet<int>();
            var priviledgeCountrys = new List<CountryDTO>();
            foreach (var country in countries)
            {
                if (uniqueCountryIds.Add(country.CountryId))
                {
                    var countryDTO = new CountryDTO()
                    {
                        CountryId = country.CountryId,
                        CountryCode = country.CountryCode,
                        CountryName = country.CountryName,
                        CurrencyRatio = country.CurrencyRatio,
                        CurrencyCode = country.CurrencyCode,
                        CurrencySymbol = country.CurrencySymbol
                    };
                    priviledgeCountrys.Add(countryDTO);
                }
            }

            return priviledgeCountrys;
        }

        public async Task<List<CountryDTO>> GetPriviledgeCountrys()
        {
            //1. get countries
            int[] countryIds = await GetPriviledgeCountryIds();

            //2. get the countries based on the service centre
            var countries = _unitOfWork.Country.GetAllAsQueryable().Where(s => countryIds.Contains(s.CountryId)).ToList();

            //3.
            var uniqueCountryIds = new HashSet<int>();
            var priviledgeCountrys = new List<CountryDTO>();
            foreach (var country in countries)
            {
                if (uniqueCountryIds.Add(country.CountryId))
                {
                    var countryDTO = new CountryDTO()
                    {
                        CountryId = country.CountryId,
                        CountryCode = country.CountryCode,
                        CountryName = country.CountryName,
                        CurrencyRatio = country.CurrencyRatio,
                        CurrencyCode = country.CurrencyCode,
                        CurrencySymbol = country.CurrencySymbol,
                        CourierEnable = country.CourierEnable
                    };
                    priviledgeCountrys.Add(countryDTO);
                }
            }

            return priviledgeCountrys;
        }

        public async Task<bool> UserActiveCountrySettings(string userid, int countryId)
        {
            var result = false;
            try
            {
                //validate userId
                var user = await _unitOfWork.User.GetUserById(userid);
                if (user == null)
                {
                    throw new GenericException("User does not exist!", $"{(int)HttpStatusCode.NotFound}");
                }

                //validate countryId
                var country = await _unitOfWork.Country.GetAsync(countryId);
                if (country == null)
                {
                    throw new GenericException("Country does not exist!", $"{(int)HttpStatusCode.NotFound}");
                }

                //set countryId
                user.UserActiveCountryId = country.CountryId;
                user.CountryType = country.CountryCode;
                await _unitOfWork.CompleteAsync();
                result = true;
            }
            catch (Exception ex)
            {
                result = false;
                throw ex;
            }
            return result;
        }

        public async Task<CountryDTO> GetUserActiveCountry()
        {
            CountryDTO userActiveCountry = null;
            //set user active country from PriviledgeCountrys
            var countries = await GetPriviledgeCountrys();
            if (countries.Count == 1)
            {
                userActiveCountry = countries[0];
            }
            else
            {
                //If UserActive Country is already set in the UserEntity, use that value
                string currentUserId = await GetCurrentUserId();
                var currentUser = await GetUserById(currentUserId);
                if (currentUser.UserActiveCountryId > 0)
                {
                    var userActiveCountryFromEntity = await _countryService.GetCountryById(currentUser.UserActiveCountryId);
                    if (userActiveCountryFromEntity.CurrencySymbol != null)
                    {
                        userActiveCountry = userActiveCountryFromEntity;
                    }
                }
            }
            return userActiveCountry;
        }

        public async Task<int> GetUserActiveCountryId()
        {

            //1. Get the user active country
            var countries = await GetPriviledgeCountrys();
            int UserCountryId;
            if (countries.Count == 1)
            {
                var userActiveCountry = countries[0];
                UserCountryId = userActiveCountry.CountryId;
            }
            else
            {
                //Use default country
                var currentUserId = await GetCurrentUserId();
                var currentUser = await GetUserById(currentUserId);
                var countryId = currentUser.UserActiveCountryId;

                if (countryId > 0)
                {
                    UserCountryId = countryId;
                }
                else
                {
                    //throw exception
                    throw new GenericException($"You have not been assigned an Active Country. Kindly inform the administrator.", $"{(int)HttpStatusCode.Forbidden}");
                }
            }

            return UserCountryId;
        }

        public async Task<int[]> GetRegionServiceCenters (string currentUserId)
        {
            int[] serviceCenterIds = { };
            try
            {
                var currentUser = await GetUserById(currentUserId);
                IList<Claim> userClaims = await GetClaimsAsync(currentUserId);
                
                string[] claimValue = null;
                foreach (var claim in userClaims)
                {
                    if (claim.Type == "Privilege")
                    {
                        claimValue = claim.Value.Split(':');
                    }
                }

                if (claimValue == null)
                {
                    throw new GenericException($"You have not been assigned a Regional access. Kindly inform the administrator.", $"{(int)HttpStatusCode.Forbidden}");
                }

                if (claimValue[0] == "Region")
                {
                    var regionId = int.Parse(claimValue[1]);
                    var regionServiceCentreMappingDTOList = await _regionServiceCentreMappingService.GetServiceCentresInRegion(regionId);
                    serviceCenterIds = regionServiceCentreMappingDTOList.Select(s => s.ServiceCentre.ServiceCentreId).ToArray();
                }
                else if (claimValue[0] == "Global")
                {
                    serviceCenterIds = _unitOfWork.ServiceCentre.GetAllAsQueryable().Select(x => x.ServiceCentreId).ToArray();
                }
                else
                {
                    throw new GenericException($"You have not been assigned a Regional access. Kindly inform the administrator.", $"{(int)HttpStatusCode.Forbidden}");
                }
            }
            catch (Exception)
            {
                throw;
            }

            return serviceCenterIds;
        }

        public async Task<bool> IsUserHasAdminRole(string userId)
        {
            return await _unitOfWork.User.IsUserHasAdminRole(userId);
        }

        //use for mobile request
        public async Task<UserDTO> GetUserUsingCustomer(string emailPhoneCode)
        {
            var user = await _unitOfWork.User.GetUserUsingCustomer(emailPhoneCode);

            if (user == null)
            {
                throw new GenericException("User does not exist!", $"{(int)HttpStatusCode.NotFound}");
            }

            return Mapper.Map<UserDTO>(user);
        }

        //used for Customer Portal
        public async Task<UserDTO> GetUserUsingCustomerForCustomerPortal(string emailPhoneCode)
        {
            var user = await _unitOfWork.User.GetUserUsingCustomerForCustomerPortal(emailPhoneCode);

            if (user == null)
            {
                throw new GenericException("User does not exist", $"{(int)HttpStatusCode.NotFound}");
            }
            return Mapper.Map<UserDTO>(user);
        }

        //used for Mobile Scanner login
        public async Task<UserDTO> GetUserUsingCustomerForMobileScanner(string emailPhoneCode)
        {
            var user = await _unitOfWork.User.GetUserUsingCustomerForMobileScanner(emailPhoneCode);

            if (user == null)
            {
                throw new GenericException("User does not exist", $"{(int)HttpStatusCode.NotFound}");
            }
            return Mapper.Map<UserDTO>(user);
        }

        //used for Fast Track Agent App login
        public async Task<UserDTO> GetUserUsingCustomerForAgentApp(string emailPhoneCode)
        {
            var user = await _unitOfWork.User.GetUserUsingCustomerForAgentApp(emailPhoneCode);

            if (user == null)
            {
                throw new GenericException("User does not exist", $"{(int)HttpStatusCode.NotFound}");
            }
            return Mapper.Map<UserDTO>(user);
        }

        public async Task<UserDTO> GetActivatedUserByEmail(string email, bool isActive)
        {
            var user = await _unitOfWork.User.ActivateUserByEmail(email, isActive);

            if (user == null)
            {
                throw new GenericException("User does not exist!", $"{(int)HttpStatusCode.NotFound}");
            }

            return Mapper.Map<UserDTO>(user);
        }

        public async Task<bool> IsCustomerHasAgentRole(string userId)
        {
            return await _unitOfWork.User.IsCustomerHasAgentRole(userId);
        }

        public async Task<IdentityResult> SetDashboardAccess(string userid, bool val)
        {
            var user = await _unitOfWork.User.GetUserById(userid);

            if (user == null)
            {
                throw new GenericException("User does not exist!", $"{(int)HttpStatusCode.NotFound}");
            }

            user.DashboardAccess = val;

            return await _unitOfWork.User.UpdateUser(userid, user);
        }

        public async Task<UserDTO> GetEmployeeUserByEmail(string email)
        {
            //if (string.IsNullOrEmpty(email))
            //{
            //    throw new GenericException("Email is empty or Not Valid!");
            //}
            email = string.IsNullOrEmpty(email) ? throw new GenericException("Email is empty or Not Valid!"): email.Trim();
            var user = await _unitOfWork.User.GetEmployeeUserByEmail(email);

            if (user == null)
            {
                throw new GenericException("User Information Not Found!", $"{(int)HttpStatusCode.NotFound}");
            }

            return Mapper.Map<UserDTO>(user);
        }

        public Task<IEnumerable<GIGL.POST.Core.Domain.User>> GetPartnerUsersByEmail(string email)
        {
            email = string.IsNullOrEmpty(email) ? throw new GenericException("Email is empty or Not Valid!") : email.Trim();
            return  _unitOfWork.User.GetPartnerUsersByEmail2(email);
        }

        //Check user role
        public async Task<bool> CheckCurrentUserSystemRole(string currentUserId)
        {
            var currentUser = await GetUserById(currentUserId);

            if (currentUser.SystemUserRole == "Gateway Supervisor")
                return true;
            return false;
        }

        public async Task<int> GetPriviledgeServiceCenter()
        {
            int serviceCenterId = 0;   //empty array
            // get current user
            try
            {

                var currentUserId = await GetCurrentUserId();
                var currentUser = await GetUserById(currentUserId);
                var userClaims = await GetClaimsAsync(currentUserId);

                string[] claimValue = null;
                foreach (var claim in userClaims)
                {
                    if (claim.Type == "Privilege")
                    {
                        claimValue = claim.Value.Split(':');   // format stringName:stringValue
                    }
                }
                if (claimValue == null)
                {
                    return 0;
                }

                if (claimValue[0] == "Global")
                {
                    serviceCenterId = 0;
                }
                else if (claimValue[0] == "Region")
                {
                    int regionId = int.Parse(claimValue[1]);
                    var regionServiceCentreMappingDTOList = await _regionServiceCentreMappingService.GetServiceCentresInRegion(regionId);
                    serviceCenterId = regionServiceCentreMappingDTOList.Select(s => s.ServiceCentre.ServiceCentreId).FirstOrDefault();
                }
                else if (claimValue[0] == "Station")
                {
                    int stationId = int.Parse(claimValue[1]);
                    var serviceCentres = await _serviceCentreService.GetServiceCentres();
                    serviceCenterId = serviceCentres.Where(s => s.StationId == stationId).Select(s => s.ServiceCentreId).FirstOrDefault();
                }
                else if (claimValue[0] == "ServiceCentre")
                {
                    serviceCenterId = int.Parse(claimValue[1]);
                }
                else if (claimValue[0] == "Public")
                {
                    serviceCenterId = 0;
                }
            }
            catch (Exception)
            {
                throw;
            }

            return serviceCenterId;
        }

    }
}
