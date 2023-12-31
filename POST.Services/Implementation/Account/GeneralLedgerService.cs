﻿using AutoMapper;
using POST.Core;
using POST.Core.Domain;
using POST.Core.DTO.Account;
using POST.Core.Enums;
using POST.Core.IServices.Account;
using POST.Core.IServices.User;
using POST.Infrastructure;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace POST.Services.Implementation.Account
{
    public class GeneralLedgerService : IGeneralLedgerService
    {
        private readonly IUnitOfWork _uow;
        private readonly IUserService _userService;

        public GeneralLedgerService(IUnitOfWork uow, IUserService userService)
        {
            _uow = uow;
            _userService = userService;
            MapperConfig.Initialize();
        }

        public async Task<IEnumerable<GeneralLedgerDTO>> GetGeneralLedgers()
        {
            var serviceCenterIds = await _userService.GetPriviledgeServiceCenters();
            var generalLedgers = await _uow.GeneralLedger.GetGeneralLedgersAsync(serviceCenterIds);
            return generalLedgers;
        }

        public async Task<IEnumerable<GeneralLedgerDTO>> GetGeneralLedgersAsync(CreditDebitType creditDebitType)
        {
            var serviceCenterIds = await _userService.GetPriviledgeServiceCenters();
            var generalLedgers = await _uow.GeneralLedger.GetGeneralLedgersAsync(creditDebitType, serviceCenterIds);
            return generalLedgers;
        }

        public async Task<GeneralLedgerDTO> GetGeneralLedgerById(int generalLedgerId)
        {
            var generalLedger = await _uow.GeneralLedger.GetAsync(s => s.GeneralLedgerId == generalLedgerId, "ServiceCentre");

            if (generalLedger == null)
            {
                throw new GenericException("GeneralLedger information does not exist");
            }

            var generalLedgerDto = Mapper.Map<GeneralLedgerDTO>(generalLedger);

            //get user details
            var user = await _userService.GetUserById(generalLedger.UserId);
            generalLedgerDto.User = user;

            //var user = await _uow.User.GetUserById(generalLedger.UserId);
            //generalLedgerDto.UserId = user.FirstName + " " + user.LastName;
            
            return generalLedgerDto;
        }

        public async Task<object> AddGeneralLedger(GeneralLedgerDTO generalLedgerDto)
        {
            try
            {
                var newGeneralLedger = Mapper.Map<GeneralLedger>(generalLedgerDto);
                newGeneralLedger.DateOfEntry = DateTime.Now;

                if(generalLedgerDto != null)
                {
                    if (generalLedgerDto.UserId == null)
                    {
                        newGeneralLedger.UserId = await _userService.GetCurrentUserId();
                    }

                    if (generalLedgerDto.ServiceCentreId < 1)
                    {
                        var serviceCenterIds = await _userService.GetPriviledgeServiceCenters();
                        newGeneralLedger.ServiceCentreId = serviceCenterIds[0];
                    }
                }

                ////--start--///Set the DepartureCountryId
                int countryIdFromServiceCentreId = 0;

                var departureCountry = await _uow.Country.GetCountryByServiceCentreId(newGeneralLedger.ServiceCentreId);
                countryIdFromServiceCentreId = departureCountry.CountryId;

                ////--end--///Set the DepartureCountryId
                newGeneralLedger.CountryId = countryIdFromServiceCentreId;
                _uow.GeneralLedger.Add(newGeneralLedger);
                await _uow.CompleteAsync();

                return new { id = newGeneralLedger.GeneralLedgerId };
            }
            catch (Exception) 
            {
                throw;
            }
        }

        public async Task UpdateGeneralLedger(int generalLedgerId, GeneralLedgerDTO generalLedgerDto)
        {
            var generalLedger = await _uow.GeneralLedger.GetAsync(generalLedgerId);

            if (generalLedger == null)
            {
                throw new GenericException("GeneralLedgerDTO");
            }
            generalLedger.Amount = generalLedgerDto.Amount;
            generalLedger.DateOfEntry = DateTime.Now;
            generalLedger.Description = generalLedgerDto.Description;
            generalLedger.ServiceCentreId = generalLedgerDto.ServiceCentreId;
            generalLedger.UserId = generalLedgerDto.UserId;
            generalLedger.CreditDebitType = generalLedgerDto.CreditDebitType;
            generalLedger.IsDeferred = generalLedgerDto.IsDeferred;
            generalLedger.Waybill = generalLedgerDto.Waybill;
            generalLedger.ClientNodeId = generalLedgerDto.ClientNodeId;

            await _uow.CompleteAsync();
        }

        public async Task RemoveGeneralLedger(int generalLedgerId)
        {
            var generalLedger = await _uow.GeneralLedger.GetAsync(generalLedgerId);

            if (generalLedger == null)
            {
                throw new GenericException("GeneralLedgerDTO");
            }
            _uow.GeneralLedger.Remove(generalLedger);
            await _uow.CompleteAsync();
        }
    }
}
