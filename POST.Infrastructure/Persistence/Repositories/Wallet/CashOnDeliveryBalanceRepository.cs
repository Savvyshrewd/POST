﻿using POST.Core.Domain.Wallet;
using POST.Core.IRepositories.Wallet;
using POST.Infrastructure.Persistence.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using POST.Core.DTO.Wallet;
using AutoMapper;

namespace POST.Infrastructure.Persistence.Repositories.Wallet
{
    public class CashOnDeliveryBalanceRepository : Repository<CashOnDeliveryBalance, GIGLSContext>, ICashOnDeliveryBalanceRepository
    {
        private GIGLSContext _context;

        public CashOnDeliveryBalanceRepository(GIGLSContext context) : base(context)
        {
            _context = context;
        }

        public Task<IEnumerable<CashOnDeliveryBalanceDTO>> GetCashOnDeliveryBalanceAsync()
        {
            try
            {
                var codBalance = _context.CashOnDeliveryBalance.Include("Wallet").ToList();
                var codAccountsDto = Mapper.Map<IEnumerable<CashOnDeliveryBalanceDTO>>(codBalance);
                return Task.FromResult(codAccountsDto);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task<IEnumerable<CashOnDeliveryBalanceDTO>> GetCashOnDeliveryPaymentSheetAsync()
        {
            try
            {
                var codBalance = _context.CashOnDeliveryBalance.Include("Wallet").Where(x => x.Balance > 0).ToList();
                var codAccountsDto = Mapper.Map<IEnumerable<CashOnDeliveryBalanceDTO>>(codBalance);
                return Task.FromResult(codAccountsDto);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
