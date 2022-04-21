﻿using GIGLS.Core;
using GIGLS.Core.DTO.Captains;
using GIGLS.Core.IServices.User;
using System.Threading.Tasks;
using GIGL.GIGLS.Core.Domain;
using System;
using GIGLS.Core.IServices.Utility;
using GIGLS.Services.Implementation.Messaging;
using GIGLS.Core.Enums;
using GIGLS.Core.IServices;
using GIGLS.Core.Domain.Partnership;
using GIGLS.Core.IServices.Partnership;
using GIGLS.Infrastructure;
using System.Collections.Generic;
using GIGLS.Core.DTO;

namespace GIGLS.Services.Implementation
{
    public class CaptainService : ICaptainService
    {
        private readonly IUserService _userService;
        private readonly IUnitOfWork _uow;
        private readonly IPasswordGenerator _passwordGenerator;
        private readonly MessageSenderService _messageSenderService;
        private readonly INumberGeneratorMonitorService _numberGeneratorMonitorService;
        private readonly IPartnerService _partnerService;

        public CaptainService(IUserService userService, IUnitOfWork uow, IPasswordGenerator passwordGenerator, MessageSenderService messageSenderService, INumberGeneratorMonitorService numberGeneratorMonitorService, IPartnerService partnerService)
        {
            _userService = userService;
            _uow = uow;
            _passwordGenerator = passwordGenerator;
            _messageSenderService = messageSenderService;
            _numberGeneratorMonitorService = numberGeneratorMonitorService;
            _partnerService = partnerService;
        }


        public async Task<object> RegisterCaptainAsync(CaptainDTO captainDTO)
        {
            var currentUserId = await _userService.GetCurrentUserId();
            var currentUser = await _userService.GetUserById(currentUserId);

            if(currentUser.SystemUserRole == "Administrator" || currentUser.SystemUserRole == "CaptainManagement")
            {
                var confirmUser = await _uow.User.GetUserByEmail(captainDTO.Email);

                string password = await _passwordGenerator.Generate();
                var user = new GIGL.GIGLS.Core.Domain.User
                {
                    Organisation = captainDTO.Organisation,
                    Status = (int)UserStatus.Active,
                    DateCreated = DateTime.Now.Date,
                    DateModified = DateTime.Now.Date,
                    Department = captainDTO.Department,
                    Designation = captainDTO.Designation,
                    Email = captainDTO.Email,
                    FirstName = captainDTO.FirstName,
                    LastName = captainDTO.LastName,
                    Gender = captainDTO.Gender,
                    UserName = captainDTO.Email,
                    PhoneNumber = captainDTO.PhoneNumber,
                    UserType = captainDTO.UserType,
                    IsActive = true,
                    PictureUrl = captainDTO.PictureUrl,
                    SystemUserRole = "Captain",
                    PasswordExpireDate = DateTime.Now
                };
                user.Id = Guid.NewGuid().ToString();

                // partner
                var partnerCode = await _numberGeneratorMonitorService.GenerateNextNumber(NumberGeneratorType.Partner);
                var partner = new Partner()
                {
                    Address = captainDTO.Address,
                    CaptainAccountName = captainDTO.AccountName,
                    CaptainAccountNumber = captainDTO.AccountNumber,
                    CaptainBankName = captainDTO.BankName,
                    DateCreated = DateTime.Now.Date,
                    Email = captainDTO.Email,
                    PictureUrl = captainDTO?.PictureUrl,
                    DateModified = DateTime.Now.Date,
                    FirstName = captainDTO.FirstName,
                    LastName = captainDTO.LastName,
                    PhoneNumber = captainDTO.PhoneNumber,
                    PartnerType = PartnerType.Captain,
                    UserId = user.Id,
                    IsDeleted = false,
                    IsActivated = true,
                    PartnerCode = partnerCode,
                    ActivityDate = DateTime.Now.Date,
                    Age = captainDTO.Age,
                    PartnerName = captainDTO.FirstName + " " + captainDTO.LastName,
                };

                if (confirmUser == null)
                {
                    var result = await _uow.User.RegisterUser(user, password);
                    if (!result.Succeeded)
                    {
                        var errors = "";
                        foreach (var error in result.Errors)
                        {
                            errors += error + "\n";
                        }
                        throw new GenericException($"{errors}");
                    }
                }
                _uow.Partner.Add(partner);
                await _uow.CompleteAsync();

                var newMsg = new NewMessageDTO();
                newMsg.Subject = "Captain Registration Successful";
                newMsg.EmailSmsType = EmailSmsType.Email;
                newMsg.ReceiverDetail = captainDTO.Email;
                newMsg.Body = $"Captain registration successful on Agility. Login details below. \nRegistered email: {captainDTO.Email} \nPassword: {password}";

                await _messageSenderService.SendGenericEmailMessage(MessageType.CAPEMAIL, newMsg);

                return new { id = user.Id, password = password, email = user.Email };
            } 
            
            throw new GenericException("You are not authorized to use this feature");
        }

        public async Task<IReadOnlyList<ViewCaptainsDTO>> GetCaptainsByDateAsync(DateTime? date)
        {
            try
            {
                var currentUserId = await _userService.GetCurrentUserId();
                var currentUser = await _userService.GetUserById(currentUserId);

                if (currentUser.SystemUserRole == "CaptainManagement" || currentUser.SystemUserRole == "Admin" || currentUser.SystemUserRole == "Administrator")
                {
                    var captains = await _uow.CaptainRepository.GetAllCaptainsByDateAsync(date);
                    return captains;
                }

                throw new GenericException("You are not authorized to use this feature");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<object> GetCaptainByIdAsync(int partnerId)
        {
            try
            {
                var currentUserId = await _userService.GetCurrentUserId();
                var currentUser = await _userService.GetUserById(currentUserId);

                if (currentUser.SystemUserRole == "CaptainManagement" || currentUser.SystemUserRole == "Admin" || currentUser.SystemUserRole == "Administrator")
                {
                    var captain = await _uow.CaptainRepository.GetCaptainByIdAsync(partnerId);
                    var user = await _userService.GetUserById(captain.UserId);

                    var captainDetails = new
                    {
                        PartnerId = partnerId,
                        Status = user.Status == 1 ? "Active" : "Inactive",
                        EmploymentDate = captain.DateCreated,
                        AssignedVehicleType = captain.VehicleType,
                        AssignedVehicleNumber = captain.VehicleLicenseNumber,
                        CaptainAge = captain.Age,
                        CaptainCode = captain.PartnerCode,
                        CaptainName = captain.FirstName + " " + captain.LastName,
                        CaptainFirstName = captain.FirstName,
                        CaptainLastName = captain.LastName,
                        CaptainPhoneNumber = captain.PhoneNumber,
                        Email = captain.Email,
                        PictureUrl = captain.PictureUrl,
                        Organization = user.Organisation,
                        Department = user.Department,
                        Designation = user.Designation,
                        Address = captain.Address,
                        CaptainBankName = captain.CaptainBankName,
                        CaptainAccountNumber = captain.CaptainAccountNumber,
                        CaptainAccountName = captain.CaptainAccountName,
                    };
                    return captainDetails;
                }
                throw new GenericException("You are not authorized to use this feature");
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task DeleteCaptainByIdAsync(int captainId)
        {
            try
            {
                var currentUserId = await _userService.GetCurrentUserId();
                var currentUser = await _userService.GetUserById(currentUserId);

                if (currentUser.SystemUserRole == "CaptainManagement" || currentUser.SystemUserRole == "Admin" || currentUser.SystemUserRole == "Administrator")
                {
                    var captain = await _uow.CaptainRepository.GetCaptainByIdAsync(captainId);
                    if (captain == null)
                    {
                        throw new GenericException($"Captain with Id {captain.PartnerId} does not exist");
                    }
                    captain.IsDeleted = true;
                    //_uow.CaptainRepository.Remove(captain);
                    await _uow.CompleteAsync();
                }
                else
                {
                    throw new GenericException("You are not authorized to use this feature");
                }
                
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task EditCaptainAsync(UpdateCaptainDTO partner)
        {
            try
            {
                var currentUserId = await _userService.GetCurrentUserId();
                var currentUser = await _userService.GetUserById(currentUserId);

                if (currentUser.SystemUserRole == "CaptainManagement" || currentUser.SystemUserRole == "Admin" || currentUser.SystemUserRole == "Administrator")
                {
                    var captain = await _uow.CaptainRepository.GetCaptainByIdAsync(partner.PartnerId);
                    if (captain == null)
                    {
                        throw new GenericException($"Captain with Id {captain.PartnerId} does not exist");
                    }

                    var user = await _userService.GetUserById(captain.UserId);
                    if (user == null) 
                    { 
                        throw new GenericException($"Captain's user info not exist"); 
                    }
                    // update user
                    user.LastName = partner.LastName;
                    user.FirstName = partner.FirstName;
                    user.Email = partner.Email;
                    user.PhoneNumber = partner.CaptainPhoneNumber;
                    user.Username = partner.Email;
                    user.Status = partner.Status.ToLower() == "active" ? 1 : 0;

                    await _userService.UpdateUser(user.Id, user);

                    // update captain
                    captain.PartnerName = partner.FirstName + " " + partner.LastName;
                    captain.FirstName = partner.FirstName;
                    captain.LastName = partner.LastName;
                    captain.Email = partner.Email;
                    captain.PhoneNumber = partner.CaptainPhoneNumber;
                    captain.DateModified = DateTime.Now;
                    captain.Age = partner.CaptainAge;
                    captain.VehicleType = partner.AssignedVehicleType;
                    captain.VehicleLicenseNumber = partner.AssignedVehicleNumber;
                    captain.PictureUrl = partner.PictureUrl;

                    await _uow.CompleteAsync();
                }
                else
                {
                    throw new GenericException("You are not authorized to use this feature");
                }

            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
