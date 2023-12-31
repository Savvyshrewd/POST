﻿using AutoMapper;
using POST.Core;
using POST.Core.Domain;
using POST.Core.DTO.BankSettlement;
using POST.Core.DTO.User;
using POST.Core.IServices;
using POST.Core.IServices.BankSettlement;
using POST.Core.IServices.User;
using POST.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace POST.Services.Implementation.BankSettlement
{
    public class GIGXUserDetailService : IGIGXUserDetailService
    {
        private readonly IUnitOfWork _uow;
        private readonly IUserService _userService;

        public GIGXUserDetailService(IUnitOfWork uow, IUserService userService)
        {
            _uow = uow;
            _userService = userService;
            MapperConfig.Initialize();
        }

        public async Task<bool> AddGIGXUserDetail(GIGXUserDetailDTO gIGXUserDetailDTO)
        {

            try
            {
                // get the current user info
                var result = false;
                var currentUserId = await _userService.GetCurrentUserId();
                var user = await _userService.GetUserById(currentUserId);

                var gigx =  _uow.GIGXUserDetail.GetAllAsQueryable().Where(x => x.CustomerCode == user.UserChannelCode).FirstOrDefault();
                if (gigx != null)
                {
                    gigx.CustomerCode = user.UserChannelCode;
                    gigx.PrivateKey = gIGXUserDetailDTO.PrivateKey;
                    gigx.PublicKey = gIGXUserDetailDTO.PublicKey;
                    gigx.WalletAddress = gIGXUserDetailDTO.WalletAddress;
                    gigx.GIGXEmail = gIGXUserDetailDTO.GIGXEmail;
                }
                else
                {
                    var gigxUser = new GIGXUserDetail
                    {
                        CustomerCode = user.UserChannelCode,
                        PrivateKey = gIGXUserDetailDTO.PrivateKey,
                        PublicKey = gIGXUserDetailDTO.PublicKey,
                        WalletAddress = gIGXUserDetailDTO.WalletAddress,
                        GIGXEmail = gIGXUserDetailDTO.GIGXEmail
                    };
                    _uow.GIGXUserDetail.Add(gigxUser);
                }
                await _uow.CompleteAsync();
                result = true;
                return result;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task<bool> CheckIfUserHasPin()
        {
            // get the current user info
            GIGXUserPinDTO gigxusersDTO = new GIGXUserPinDTO();
            gigxusersDTO.HasPin = false;
            var currentUserId = await _userService.GetCurrentUserId();
            var user = await _userService.GetUserById(currentUserId);
            var userPin = await _uow.GIGXUserDetail.GetGIGXUserDetailByCode(user.UserChannelCode);
            if (userPin != null && !String.IsNullOrEmpty(userPin.CustomerPin))
            {
                gigxusersDTO.HasPin = true;
            }
            return gigxusersDTO.HasPin;
        }

        public async Task<bool> VerifyUserPin(GIGXUserDetailDTO gIGXUserDetailDTO)
        {
            // get the current user info
            GIGXUserPinDTO gigxusersDTO = new GIGXUserPinDTO();
            var currentUserId = await _userService.GetCurrentUserId();
            var user = await _userService.GetUserById(currentUserId);
            var pin = gIGXUserDetailDTO.CustomerPin.ToString();
            var userPin = await _uow.GIGXUserDetail.GetGIGXUserDetailByCode(user.UserChannelCode);
            if (userPin != null && !String.IsNullOrEmpty(pin) && userPin.CustomerPin == gIGXUserDetailDTO.CustomerPin)
            {
                gigxusersDTO.GIGXUserDetailDTO = userPin;
                gigxusersDTO.GIGXUserDetailDTO.CustomerPin = null;
                gigxusersDTO.HasPin = true;
            }
            return gigxusersDTO.HasPin;
        }

        public async Task<bool> ChangeUserPin(GIGXUserDetailDTO gIGXUserDetailDTO)
        {
            try
            {
                bool result = false;
                // get the current user info
                var currentUserId = await _userService.GetCurrentUserId();
                var user = await _userService.GetUserById(currentUserId);
                var oldPin = gIGXUserDetailDTO.CustomerPin.ToString();
                var newPin = gIGXUserDetailDTO.CustomerNewPin.ToString();
                var userPin = await _uow.GIGXUserDetail.GetAsync(x => x.CustomerCode == user.UserChannelCode);

                if (userPin != null && userPin.CustomerPin != gIGXUserDetailDTO.CustomerPin)
                {
                    throw new GenericException($"Old pin is incorrect. Please provide correct pin.");
                }
                if (userPin != null && !String.IsNullOrEmpty(oldPin) && !String.IsNullOrEmpty(newPin) && userPin.CustomerPin == gIGXUserDetailDTO.CustomerPin)
                {
                    userPin.CustomerPin = newPin;
                    result = true;
                }
                await _uow.CompleteAsync();
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> ResetUserPin(GIGXUserDetailDTO gIGXUserDetailDTO)
        {
            try
            {
                bool result = false;

                var userPin = await _uow.GIGXUserDetail.GetAsync(x => x.CustomerCode == gIGXUserDetailDTO.CustomerCode);
                
                if (userPin != null)
                {
                    userPin.CustomerPin = gIGXUserDetailDTO.CustomerNewPin;
                    result = true;
                }
                await _uow.CompleteAsync();
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> AddGIGXUserDetailPin(GIGXUserDetailDTO gIGXUserDetailDTO)
        {

            try
            {
                // get the current user info
                var result = false;
                var currentUserId = await _userService.GetCurrentUserId();
                var user = await _userService.GetUserById(currentUserId);

                var gigx = _uow.GIGXUserDetail.GetAllAsQueryable().Where(x => x.CustomerCode == user.UserChannelCode).FirstOrDefault();
                bool isNumeric = int.TryParse(gIGXUserDetailDTO.CustomerPin, out int n);
                if (!isNumeric)
                {
                    throw new GenericException("invalid wallet pin");
                }
                if (isNumeric && n > 0 && !String.IsNullOrEmpty(gIGXUserDetailDTO.CustomerPin))
                {
                    gIGXUserDetailDTO.CustomerPin = gIGXUserDetailDTO.CustomerPin.ToString().Trim();
                }
                if (gigx != null)
                {
                    gigx.CustomerCode = user.UserChannelCode;
                    gigx.CustomerPin = gIGXUserDetailDTO.CustomerPin;
                }
                else
                {
                    var gigxUser = new GIGXUserDetail
                    {
                        CustomerCode = user.UserChannelCode,
                        CustomerPin = gIGXUserDetailDTO.CustomerPin
                    };
                    _uow.GIGXUserDetail.Add(gigxUser);
                }
                await _uow.CompleteAsync();
                result = true;
                return result;
            }
            catch (Exception ex)
            {

                throw;
            }
        }

    }
}
