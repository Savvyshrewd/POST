﻿using POST.Core.Domain;
using POST.Core.DTO.Fleets;
using POST.Core.Enums;
using POST.Core.IRepositories;
using POST.Infrastructure.Persistence.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POST.Infrastructure.Persistence.Repositories
{
    public class FleetPartnerTransactionRepository : Repository<FleetPartnerTransaction, GIGLSContext>, IFleetPartnerTransactionRepository
    {
        private GIGLSContext _context;
        public FleetPartnerTransactionRepository(GIGLSContext context) : base(context)
        {
            _context = context;
        }

        public Task<List<FleetPartnerTransactionDTO>> GetFleetPartnerTransaction(string partnercode)
        {
            //To be completed
            var users = _context.Users.Where(x => x.UserChannelCode == partnercode);

            var transactionsDto = from user in users
                                join fleet in _context.Fleet on user.Id equals fleet.EnterprisePartnerId
                                join trans in _context.FleetPartnerTransaction on fleet.RegistrationNumber.ToLower() equals trans.FleetRegistrationNumber.ToLower()
                                select new FleetPartnerTransactionDTO
                                {
                                    CreditDebitType = trans.CreditDebitType,
                                    Amount = trans.Amount,
                                    Description = trans.Description,
                                    DateOfEntry = trans.DateOfEntry
                                };

            return Task.FromResult(transactionsDto.OrderByDescending(x => x.DateOfEntry).ToList());
        }

        public Task<List<FleetPartnerTransactionDTO>> GetFleetPartnerTransactionByDateRange(string partnercode, FleetFilterCriteria filterCriteria)
        {
            var queryDate = filterCriteria.getStartDateAndEndDate();
            var startDate = queryDate.Item1;
            var endDate = queryDate.Item2;

            //To be completed
            var users = _context.Users.Where(x => x.UserChannelCode == partnercode);

            var transactionsDto = from user in users
                                  join fleet in _context.Fleet on user.Id equals fleet.EnterprisePartnerId
                                  join trans in _context.FleetPartnerTransaction on fleet.RegistrationNumber.ToLower() equals trans.FleetRegistrationNumber.ToLower()
                                  where trans.DateOfEntry >= startDate && trans.DateOfEntry < endDate
                                  select new FleetPartnerTransactionDTO
                                  {
                                      CreditDebitType = trans.CreditDebitType,
                                      Amount = trans.Amount,
                                      Description = trans.Description,
                                      DateOfEntry = trans.DateOfEntry
                                  };

            return Task.FromResult(transactionsDto.OrderByDescending(x => x.DateOfEntry).ToList());
        }

        public Task<List<FleetPartnerTransactionDTO>> GetFleetPartnerCreditTransaction(string partnercode)
        {
            //To be completed
            var users = _context.Users.Where(x => x.UserChannelCode == partnercode);

            var transactionsDto = from user in users
                                  join fleet in _context.Fleet on user.Id equals fleet.EnterprisePartnerId
                                  join trans in _context.FleetPartnerTransaction on fleet.RegistrationNumber.ToLower() equals trans.FleetRegistrationNumber.ToLower()
                                  where trans.CreditDebitType == CreditDebitType.Credit
                                  select new FleetPartnerTransactionDTO
                                  {
                                      CreditDebitType = trans.CreditDebitType,
                                      Amount = trans.Amount,
                                      Description = trans.Description,
                                      DateOfEntry = trans.DateOfEntry
                                  };

            return Task.FromResult(transactionsDto.OrderByDescending(x => x.DateOfEntry).ToList());
        }

        public Task<List<FleetPartnerTransactionDTO>> GetFleetPartnerDebitTransaction(string partnercode)
        {
            //To be completed
            var users = _context.Users.Where(x => x.UserChannelCode == partnercode);

            var transactionsDto = from user in users
                                  join fleet in _context.Fleet on user.Id equals fleet.EnterprisePartnerId
                                  join trans in _context.FleetPartnerTransaction on fleet.RegistrationNumber.ToLower() equals trans.FleetRegistrationNumber.ToLower()
                                  where trans.CreditDebitType == CreditDebitType.Debit
                                  select new FleetPartnerTransactionDTO
                                  {
                                      CreditDebitType = trans.CreditDebitType,
                                      Amount = trans.Amount,
                                      Description = trans.Description,
                                      DateOfEntry = trans.DateOfEntry
                                  };

            return Task.FromResult(transactionsDto.OrderByDescending(x => x.DateOfEntry).ToList());
        }
    }
}
