﻿using AutoMapper;
using GIGLS.Core;
using GIGLS.Core.Domain;
using GIGLS.Core.DTO.Account;
using GIGLS.Core.Enums;
using GIGLS.Core.IServices.Account;
using GIGLS.Core.IServices.User;
using GIGLS.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GIGLS.Services.Implementation.Account
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
            return Mapper.Map<GeneralLedgerDTO>(generalLedger);
        }

        public async Task<object> AddGeneralLedger(GeneralLedgerDTO generalLedgerDto)
        {
            var newGeneralLedger = Mapper.Map<GeneralLedger>(generalLedgerDto);
            newGeneralLedger.DateOfEntry = DateTime.Now;

            _uow.GeneralLedger.Add(newGeneralLedger);
            await _uow.CompleteAsync();
            return new { id = newGeneralLedger.GeneralLedgerId };
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
