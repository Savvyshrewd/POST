﻿using POST.Core;
using POST.Core.DTO.Account;
using POST.Core.Enums;
using POST.Core.IServices.Account;
using System.Collections.Generic;
using System.Threading.Tasks;
using POST.Core.DTO.Expenses;
using POST.Core.DTO.Report;
using System;
using POST.Core.IServices.User;
using AutoMapper;
using POST.Core.Domain.Expenses;
using POST.Core.Domain;

namespace POST.Services.Implementation.Account
{
    public class ExpenditureService : IExpenditureService
    {
        private readonly IUnitOfWork _uow;
        private readonly IUserService _userService;
        private readonly IGeneralLedgerService _generalLedgerService;

        public ExpenditureService(IUnitOfWork uow,IUserService userService, IGeneralLedgerService generalLedgerService)
        {
            _uow = uow;
            _userService = userService;
            _generalLedgerService = generalLedgerService;
            MapperConfig.Initialize();
        }

        public async Task<IEnumerable<GeneralLedgerDTO>> GetExpenditures()
        {
            var expenditures = await _generalLedgerService.GetGeneralLedgersAsync(CreditDebitType.Debit);
            return expenditures;
        }

        public Task<object> AddExpenditure(GeneralLedgerDTO generalLedger)
        {
            if(generalLedger != null)
            {
                generalLedger.CreditDebitType = CreditDebitType.Debit;
                generalLedger.PaymentServiceType = PaymentServiceType.Miscellaneous;
            }
            return _generalLedgerService.AddGeneralLedger(generalLedger);
        }

        public async Task<object> AddExpenditure(ExpenditureDTO expenditureDto)
        {
            var expenditure = Mapper.Map<Expenditure>(expenditureDto);

            if(expenditureDto != null)
            {
                if (expenditureDto.UserId == null)
                {
                    expenditure.UserId = await _userService.GetCurrentUserId();
                }

                if (expenditure.ServiceCentreId < 1)
                {
                    var serviceCenterIds = await _userService.GetPriviledgeServiceCenters();
                    expenditure.ServiceCentreId = serviceCenterIds[0];
                }
            }

            _uow.Expenditure.Add(expenditure);

            ////--start--///Set the DepartureCountryId
            int countryIdFromServiceCentreId = 0;
            try
            {
                var departureCountry = await _uow.Country.GetCountryByServiceCentreId(expenditure.ServiceCentreId);
                countryIdFromServiceCentreId = departureCountry.CountryId;
            }
            catch (Exception) { throw; }
            ////--end--///Set the DepartureCountryId

            //Add record to general ledger
            var generalLedger = new GeneralLedger()
            {
                DateOfEntry = DateTime.Now,
                ServiceCentreId = expenditure.ServiceCentreId,
                CountryId = countryIdFromServiceCentreId,
                UserId = expenditure.UserId,
                Amount = expenditure.Amount,
                CreditDebitType = CreditDebitType.Debit,
                Description = expenditure.Description,
                PaymentServiceType = PaymentServiceType.Miscellaneous                
            };
            _uow.GeneralLedger.Add(generalLedger);

            await _uow.CompleteAsync();
            return new { id = expenditure.ExpenditureId };
        }

        public async Task<IEnumerable<ExpenditureDTO>> GetExpenditures(ExpenditureFilterCriteria expenditureFilterCriteria)
        {
            if (!expenditureFilterCriteria.StartDate.HasValue)
            {
                var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                expenditureFilterCriteria.StartDate = startDate;
            }

            var serviceCenterIds = await _userService.GetPriviledgeServiceCenters();
            var expenditures = await _uow.Expenditure.GetExpenditures(expenditureFilterCriteria, serviceCenterIds);
            return expenditures;
        }
    }
}
