﻿using AutoMapper;
using POST.Core;
using POST.Core.Domain;
using POST.Core.DTO.BankSettlement;
using POST.Core.IServices.BankSettlement;
using POST.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace POST.Services.Implementation.BankSettlement
{
    public class BankService : IBankService
    {
        private readonly IUnitOfWork _uow;

        public BankService(IUnitOfWork uow)
        {
            _uow = uow;
            MapperConfig.Initialize();
        }

        public Task<IEnumerable<BankDTO>> GetBanks()
        {
            var banks = _uow.Bank.GetAll().OrderBy(x => x.BankName);
            return Task.FromResult(Mapper.Map<IEnumerable<BankDTO>>(banks));
        }

        public async Task<BankDTO> GetBankById(int bankId)
        {
            try
            {
                var bank = await _uow.Bank.GetAsync(bankId);
                if (bank == null)
                {
                    throw new GenericException("Bank information does not exist");
                }
                var bankDto = Mapper.Map<BankDTO>(bank);
                return bankDto;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<object> AddBank(BankDTO bankDTO)
        {
            try
            {
                var bank = await _uow.Bank.GetAsync(x => x.BankName.ToLower() == bankDTO.BankName.ToLower());
                if (bank != null)
                {
                    throw new GenericException("Bank information already exist");
                }

                var newBank = Mapper.Map<Bank>(bankDTO);
                _uow.Bank.Add(newBank);
                await _uow.CompleteAsync();
                return new { Id = newBank.BankId };
            }
            catch (Exception)
            {
                throw;
            }
        }
        
        public async Task UpdateBank (int bankId, BankDTO bankDTO)
        {
            try
            {
                //To check if the update already esists
                var banks = await _uow.Bank.ExistAsync(c => c.BankName.ToLower() == bankDTO.BankName.ToLower());

                if (banks)
                {
                    throw new GenericException("Bank information already exists");
                }
                else
                {
                    var bank = await _uow.Bank.GetAsync(bankId);

                    if (bank == null || bankDTO.BankId != bankId)
                    {
                        throw new GenericException("Bank information does not exist");
                    }

                    bank.BankName = bankDTO.BankName;
                    _uow.Complete();
                }                
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task DeleteBank (int bankId)
        {
            try
            {
                var bank = await _uow.Bank.GetAsync(bankId);
                if (bank == null)
                {
                    throw new GenericException("Bank information does not exist");
                }
                _uow.Bank.Remove(bank);
                _uow.Complete();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
