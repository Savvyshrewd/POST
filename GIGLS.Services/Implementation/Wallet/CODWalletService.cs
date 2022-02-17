﻿using AutoMapper;
using GIGLS.Core;
using GIGLS.Core.Domain;
using GIGLS.Core.Domain.Utility;
using GIGLS.Core.Domain.Wallet;
using GIGLS.Core.DTO;
using GIGLS.Core.DTO.Customers;
using GIGLS.Core.DTO.Partnership;
using GIGLS.Core.DTO.Wallet;
using GIGLS.Core.Enums;
using GIGLS.Core.IServices.User;
using GIGLS.Core.IServices.Utility;
using GIGLS.Core.IServices.Wallet;
using GIGLS.CORE.Enums;
using GIGLS.Infrastructure;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace GIGLS.Services.Implementation.Wallet
{
    public class CODWalletService : ICODWalletService
    {
        private readonly IUserService _userService;
        private readonly IUnitOfWork _uow;
        private readonly IGlobalPropertyService _globalPropertyService;
        private readonly IStellasService _stellasService;

        public CODWalletService(IUserService userService, IUnitOfWork uow, IGlobalPropertyService globalPropertyService, IStellasService stellasService)
        {
            _userService = userService;
            _uow = uow;
            _globalPropertyService = globalPropertyService;
            _stellasService = stellasService;
            MapperConfig.Initialize();
        }


        public async Task<CODWalletDTO> CreateStellasAccount(CreateStellaAccountDTO createStellaAccountDTO)
        {
            var user = await _uow.Company.GetAsync(x => x.CustomerCode == createStellaAccountDTO.CustomerCode);
            if (user is null)
            {
                throw new GenericException("User does not exist as an Ecommerce user!", $"{(int)HttpStatusCode.NotFound}");
            }
            CODWalletDTO res = new CODWalletDTO();
            var result = await _stellasService.CreateStellasAccount(createStellaAccountDTO);
            if (result.status)
            {
                //now create this user CODWALLET on agility  
                CODWalletDTO codWalletDTO = new CODWalletDTO
                {
                    AccountNo = result.account_details.accountNumber,
                    AvailableBalance = result.account_details.availableBalance,
                    CustomerId = result.account_details.customerId,
                    CustomerCode = user.CustomerCode,
                    CustomerType = CustomerType.Company,
                    CompanyType = CompanyType.Ecommerce.ToString(),
                    AccountType = result.account_details.accountType,
                    WithdrawableBalance = result.account_details.withdrawableBalance,
                    CustomerAccountId = result.account_details.customerId
                };
                await AddCODWallet(codWalletDTO);
                res = codWalletDTO;
            }
            else
            {
                res = null;
                throw new GenericException(result.message, $"{(int)HttpStatusCode.BadRequest}");
            }
            return res;
        }



        public async Task AddCODWallet(CODWalletDTO codWalletDTO)
        {
            try
            {
                var codWallet = Mapper.Map<CODWallet>(codWalletDTO);
                _uow.CODWallet.Add(codWallet);
                _uow.CompleteAsync();
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Task<GetCustomerBalanceDTO> GetStellasAccountBal(string customerCode)
        {
            var bal = _stellasService.GetCustomerStellasAccount(customerCode);
            return bal;
        }

       
    }
}