﻿using AutoMapper;
using GIGLS.Core;
using GIGLS.Core.Domain.Partnership;
using GIGLS.Core.DTO;
using GIGLS.Core.DTO.Fleets;
using GIGLS.Core.DTO.MessagingLog;
using GIGLS.Core.DTO.Partnership;
using GIGLS.Core.DTO.Report;
using GIGLS.Core.DTO.User;
using GIGLS.Core.Enums;
using GIGLS.Core.IMessageService;
using GIGLS.Core.IServices.Customers;
using GIGLS.Core.IServices.Partnership;
using GIGLS.Core.IServices.User;
using GIGLS.Core.IServices.Utility;
using GIGLS.Infrastructure;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace GIGLS.Services.Implementation.Partnership
{
    public class FleetPartnerService : IFleetPartnerService
    {
        private readonly IUnitOfWork _uow;
        private readonly INumberGeneratorMonitorService _numberGeneratorMonitorService;
        private readonly IUserService _userService;
        private readonly ICompanyService _companyService;
        private readonly IPasswordGenerator _passwordGenerator;
        private readonly IMessageSenderService _messageSenderService;
        private readonly IGlobalPropertyService _globalPropertyService;

        public FleetPartnerService(IUnitOfWork uow, INumberGeneratorMonitorService numberGeneratorMonitorService,
            IUserService userService, ICompanyService companyService, IPasswordGenerator passwordGenerator,
            IMessageSenderService messageSenderService, IGlobalPropertyService globalPropertyService)
        {
            _uow = uow;
            _numberGeneratorMonitorService = numberGeneratorMonitorService;
            _userService = userService;
            _companyService = companyService;
            _passwordGenerator = passwordGenerator;
            _messageSenderService = messageSenderService;
            _globalPropertyService = globalPropertyService;
            MapperConfig.Initialize();
        }


        public async Task<object> AddFleetPartner(FleetPartnerDTO fleetPartnerDTO)
        {
            if (fleetPartnerDTO.LastName != null)
            {
                fleetPartnerDTO.LastName.Trim();
            }
            if (fleetPartnerDTO.FirstName != null)
            {
                fleetPartnerDTO.FirstName.Trim();
            }

            if (fleetPartnerDTO.Email != null)
            {
                fleetPartnerDTO.Email = fleetPartnerDTO.Email.Trim().ToLower();
            }


            //Pending
            ////update the customer update to have country code added to it
            //if (fleetPartnerDTO.PhoneNumber.StartsWith("0"))
            //{
            //    fleetPartnerDTO.PhoneNumber = await _companyService.AddCountryCodeToPhoneNumber(fleetPartnerDTO.PhoneNumber, fleetPartnerDTO.UserActiveCountryId);
            //}

            if (await _uow.FleetPartner.ExistAsync(v => v.PhoneNumber == fleetPartnerDTO.PhoneNumber || v.Email == fleetPartnerDTO.Email))
            {
                throw new GenericException("Fleet Partner Already Exists");
            }

            var EmailUser = await _uow.User.GetUserByEmailorPhoneNumber(fleetPartnerDTO.Email, fleetPartnerDTO.PhoneNumber);

            if (EmailUser != null)
            {
                throw new GenericException("Customer already exists");
            }

            var fleetPartnerCode = await _numberGeneratorMonitorService.GenerateNextNumber(NumberGeneratorType.FleetPartner);
            var fleetPartner = Mapper.Map<FleetPartner>(fleetPartnerDTO);
            fleetPartner.FleetPartnerCode = fleetPartnerCode;
            
            string password = await _passwordGenerator.Generate();

            var user = new GIGL.GIGLS.Core.Domain.User
            {
                Email = fleetPartner.Email,
                FirstName = fleetPartner.FirstName,
                LastName = fleetPartner.LastName,
                PhoneNumber = fleetPartner.PhoneNumber,
                UserType = UserType.Regular,
                UserName = fleetPartner.Email,
                UserChannelCode = fleetPartner.FleetPartnerCode,
                UserChannelPassword = password,
                UserChannelType = UserChannelType.FleetPartner,
                UserActiveCountryId = fleetPartner.UserActiveCountryId,
                IsActive = true,
                DateCreated = DateTime.Now.Date,
                DateModified = DateTime.Now.Date,
                PasswordExpireDate = DateTime.Now                
            };

            user.Id = Guid.NewGuid().ToString();
            
            var u = await _uow.User.RegisterUser(user, password);

            if (u.Succeeded)
            {
                fleetPartner.UserId = user.Id;
                _uow.FleetPartner.Add(fleetPartner);
                await AssignPartnersToFleetPartner(fleetPartnerCode, fleetPartnerDTO.PartnerCodes);
                await _uow.CompleteAsync();
            }
            
            var passwordMessage = new PasswordMessageDTO()
            {
                Password = password,
                UserEmail = fleetPartner.Email
            };

            await _messageSenderService.SendGenericEmailMessage(MessageType.FPEmail, passwordMessage);
            return new { id = fleetPartner.FleetPartnerId };
        }

        public async Task UpdateFleetPartner(int partnerId, FleetPartnerDTO fleetPartnerDTO)
        {
            var existingParntner = await _uow.FleetPartner.GetAsync(partnerId);

            if (existingParntner == null)
            {
                throw new GenericException("Fleet Partner Does not Exist");
            }

            existingParntner.FirstName = fleetPartnerDTO.FirstName.Trim();
            existingParntner.LastName = fleetPartnerDTO.LastName.Trim();
            existingParntner.Email = fleetPartnerDTO.Email.Trim();
            existingParntner.Address = fleetPartnerDTO.Address;
            existingParntner.PhoneNumber = fleetPartnerDTO.PhoneNumber;
            await _uow.CompleteAsync();
        }

        public async Task RemoveFleetPartner(int partnerId)
        {
            var existingPartner = await _uow.FleetPartner.GetAsync(partnerId);

            if (existingPartner == null)
            {
                throw new GenericException("Fleet Partner does not exist");
            }
            _uow.FleetPartner.Remove(existingPartner);
            await _uow.CompleteAsync();
        }

        public async Task<IEnumerable<FleetPartnerDTO>> GetFleetPartners()
        {
            var partners = await _uow.FleetPartner.GetFleetPartnersAsync();
            return partners;
        }

        public async Task<FleetPartnerDTO> GetFleetPartnerById(int partnerId)
        {
            var partnerDto = new FleetPartnerDTO();
            var partner = await _uow.FleetPartner.GetAsync(partnerId);

            if (partner == null)
            {
                throw new GenericException("Fleet Partner does not exist");
            }
            else
            {
                partnerDto = Mapper.Map<FleetPartnerDTO>(partner);
                var Country = await _uow.Country.GetAsync(s => s.CountryId == partner.UserActiveCountryId);

            }
            return partnerDto;
        }

        public async Task<int> CountOfPartnersUnderFleet()
        {
            //get the current login user 
            var currentUserId = await _userService.GetCurrentUserId();
            var currentUser = await _userService.GetUserById(currentUserId);

            var partners = await _uow.FleetPartner.FleetCount(currentUser.UserChannelCode);
            return partners;
        }
        public async Task<List<VehicleTypeDTO>> GetVehiclesAttachedToFleetPartner()
        {
            //get the current login user 
            var currentUserId = await _userService.GetCurrentUserId();
            var currentUser = await _userService.GetUserById(currentUserId);

            var vehicles = await _uow.FleetPartner.GetVehiclesAttachedToFleetPartner(currentUser.UserChannelCode);
            return vehicles;
        }

        public async Task<List<VehicleTypeDTO>> GetVehiclesAttachedToFleetPartner(string fleetCode)
        {
            var vehicles = await _uow.FleetPartner.GetVehiclesAttachedToFleetPartner(fleetCode);
            return vehicles;
        }

        public async Task<List<FleetPartnerTransactionsDTO>> GetFleetTransaction(ShipmentCollectionFilterCriteria filterCriteria)
        {
            //get the current login user 
            var currentUserId = await _userService.GetCurrentUserId();
            var currentUser = await _userService.GetUserById(currentUserId);
            var partners = new List<FleetPartnerTransactionsDTO>();

            if (filterCriteria.IsDashboard)
            {
                partners = await _uow.PartnerTransactions.GetRecentFivePartnerTransactionsForFleet(currentUser.UserChannelCode);
            }
            else
            {
                partners = await _uow.PartnerTransactions.GetPartnerTransactionsForFleet(filterCriteria, currentUser.UserChannelCode);
            }

            return await Task.FromResult(partners);
        }

        public async Task<List<object>> GetEarningsOfPartnersAttachedToFleet(ShipmentCollectionFilterCriteria filterCriteria)
        {
            //get the current login user 
            var currentUserId = await _userService.GetCurrentUserId();
            var currentUser = await _userService.GetUserById(currentUserId);

            var earnings = await _uow.PartnerTransactions.GetPartnerEarningsForFleet(filterCriteria, currentUser.UserChannelCode);
            return await Task.FromResult(earnings);
        }

        public async Task<List<FleetMobilePickUpRequestsDTO>> GetPartnerResponseAttachedToFleet(ShipmentCollectionFilterCriteria filterCriteria)
        {
            //get the current login user 
            var currentUserId = await _userService.GetCurrentUserId();
            var currentUser = await _userService.GetUserById(currentUserId);

            var response = await _uow.MobilePickUpRequests.GetPartnerMobilePickUpRequestsForFleetPartner(filterCriteria, currentUser.UserChannelCode);
            return await Task.FromResult(response);
        }

        public async Task<List<PartnerDTO>> GetExternalPartnersNotAttachedToAnyFleetPartner()
        {

            var partners = await _uow.FleetPartner.GetExternalPartnersNotAttachedToAnyFleetPartner();
            return partners;
        }

        private async Task AssignPartnersToFleetPartner(string fleetCode,List<string> partnerCodes)
        {
            foreach (var partnerCode in partnerCodes)
            {
                var partner = await _uow.Partner.GetAsync(x => x.PartnerCode == partnerCode);
                if (partner != null)
                {
                    partner.FleetPartnerCode = fleetCode;
                }
            }
            await _uow.CompleteAsync();
            
        }

        public async Task RemovePartnerFromFleetPartner(string partnerCode)
        {
                var partner = await _uow.Partner.GetAsync(x => x.PartnerCode == partnerCode);
                if (partner != null)
                {
                    partner.FleetPartnerCode = null;
                }
            
            await _uow.CompleteAsync();

        }

        public async Task<List<VehicleTypeDTO>> GetVerifiedPartners()
        {
            //get the current login user 
            var currentUserId = await _userService.GetCurrentUserId();
            var currentUser = await _userService.GetUserById(currentUserId);

            var partners = await _uow.Partner.GetPartnersAsync(currentUser.UserChannelCode, true);

            return partners;
        }

        public async Task<List<AssetDTO>> GetFleetAttachedToEnterprisePartner(string fleetPartnerCode)
        {
            //get the current login user 
            var currentUserId = await _userService.GetCurrentUserId();
            var currentUser = await _userService.GetUserById(currentUserId);

            var assets = await _uow.FleetPartner.GetFleetAttachedToEnterprisePartner(fleetPartnerCode);
            return assets;
        }

        public async Task<AssetDetailsDTO> GetFleetAttachedToEnterprisePartnerById(int fleetId)
        {
            var asset = await _uow.FleetPartner.GetFleetAttachedToEnterprisePartnerById(fleetId);
            return asset;
        }

        public async Task<List<FleetTripDTO>> GetFleetTrips(int fleetId)
        {
            var fleetTrips = await _uow.FleetPartner.GetFleetTrips(fleetId);
            return fleetTrips;
        }

        public async Task<FleetPartnerWalletDTO> GetPartnerWalletBalance()
        {
            //get the current login user 
            var currentUserId = await _userService.GetCurrentUserId();
            var currentUser = await _userService.GetUserById(currentUserId);

            var result = new FleetPartnerWalletDTO
            {
                AvailableBalance = 20000.00m,
                LedgerBalance = 40000.00m,
                CurrentDayIncome = 12000m
            };

            return result;
        }

        private async Task<decimal> CalculateFleetPricing(FleetDTO fleet)
        {
            if(fleet == null)
                throw new GenericException("Invalid fleet details");

            decimal price = 0.00m;

            if (fleet.IsFixed == VehicleFixedStatus.Fixed)
            {
                //Get pricing for fixed fleet
                var fixPrice = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.EnterpriseFleetFixPrice, 1);
                decimal fixPriceValue = Convert.ToDecimal( fixPrice?.Value);

                //Get minimum trip for fixed fleet
                var minimumTrip = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.EnterpriseFleetMinimumTrip, 1);
                int minimumTripValue = Convert.ToInt32(minimumTrip?.Value);

                //Set start date and end date
                var startDate = DateTime.Now;
                var endDate = DateTime.Now;
                startDate = new DateTime(startDate.Year, startDate.Month, 1);
                endDate = endDate.AddDays(1);

                //Get total count of fleet trips for the current month
                var fleetTripCount = _uow.FleetTrip.GetAllAsQueryable().Where(x => x.FleetRegistrationNumber == fleet.RegistrationNumber 
                                                                                && x.DateCreated >= startDate && x.DateCreated <= endDate).Count();

                if(fleetTripCount >= minimumTripValue)
                {
                    price = fixPriceValue;
                }
            }
            else
            {
                //Get pricing for Variable fleet


            }

            return price;
        }
    }
}
