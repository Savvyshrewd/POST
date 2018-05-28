﻿using AutoMapper;
using GIGLS.Core;
using GIGLS.Core.Domain.Wallet;
using GIGLS.Core.DTO.Wallet;
using GIGLS.Core.IServices.Customers;
using GIGLS.Core.IServices.ServiceCentres;
using GIGLS.Core.IServices.User;
using GIGLS.Core.IServices.Wallet;
using GIGLS.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GIGLS.Services.Implementation.Wallet
{
    public class WalletTransactionService : IWalletTransactionService
    {
        private readonly IUnitOfWork _uow;
        private readonly IUserService _userService;
        private readonly IWalletService _walletService;
        private readonly ICustomerService _customerService;
        private readonly IServiceCentreService _centreService;

        public WalletTransactionService(IUnitOfWork uow, IUserService userService, 
            IWalletService walletService, ICustomerService customerService,
            IServiceCentreService centreService)
        {
            _uow = uow;
            _userService = userService;
            _walletService = walletService;
            _customerService = customerService;
            _centreService = centreService;
            MapperConfig.Initialize();
        }


        public async Task<object> AddWalletTransaction(WalletTransactionDTO walletTransactionDTO)
        {
            var newWalletTransaction = Mapper.Map<WalletTransaction>(walletTransactionDTO);
            newWalletTransaction.DateOfEntry = DateTime.Now;

            _uow.WalletTransaction.Add(newWalletTransaction);
            await _uow.CompleteAsync();
            return new { id = newWalletTransaction.WalletTransactionId };
        }

        public async Task<IEnumerable<WalletTransactionDTO>> GetWalletTransactions()
        {
            var serviceCenterIds = await _userService.GetPriviledgeServiceCenters();
            var walletTransactions = await _uow.WalletTransaction.GetWalletTransactionAsync(serviceCenterIds);
            return walletTransactions;
        }

        public async Task<WalletTransactionDTO> GetWalletTransactionById(int walletTransactionId)
        {
            var walletTransaction = await _uow.WalletTransaction.GetAsync(s => s.WalletTransactionId == walletTransactionId, "ServiceCentre");

            if (walletTransaction == null)
            {
                throw new GenericException("Wallet Transaction information does not exist");
            }
            return Mapper.Map<WalletTransactionDTO>(walletTransaction);
        }

        public async Task<WalletTransactionSummaryDTO> GetWalletTransactionByWalletId(int walletId)
        {
            var walletTransactions = await _uow.WalletTransaction.FindAsync(s => s.WalletId == walletId);
            if (walletTransactions.Count() < 1)
            {
                throw new GenericException("Wallet Transaction information does not exist");
            }
            var walletTransactionDTOList = Mapper.Map<List<WalletTransactionDTO>>(walletTransactions.OrderByDescending(s => s.DateCreated));

            // get the service centre
            foreach(var item in walletTransactionDTOList)
            {
                var serviceCentre = await _centreService.GetServiceCentreById(item.ServiceCentreId);
                item.ServiceCentre = serviceCentre;
            }
            

            // get the wallet owner information
            var wallet = await _walletService.GetWalletById(walletId);

            //get the customer info
            var customerDTO = await _customerService.GetCustomer(wallet.CustomerId, wallet.CustomerType);

            return new WalletTransactionSummaryDTO
            {
                WalletTransactions = walletTransactionDTOList,
                WalletNumber = wallet.WalletNumber,
                WalletBalance = wallet.Balance,
                WalletOwnerName = customerDTO.CustomerName
            };
        }

        public async Task RemoveWalletTransaction(int walletTransactionId)
        {
            var walletTransaction = await _uow.WalletTransaction.GetAsync(walletTransactionId);

            if (walletTransaction == null)
            {
                throw new GenericException("Wallet Transaction does not exist");
            }
            _uow.WalletTransaction.Remove(walletTransaction);
            await _uow.CompleteAsync();
        }

        public async Task UpdateWalletTransaction(int walletTransactionId, WalletTransactionDTO walletTransactionDTO)
        {
            var walletTransaction = await _uow.WalletTransaction.GetAsync(walletTransactionId);

            if (walletTransaction == null)
            {
                throw new GenericException("Wallet Transaction does not exist");
            }
            walletTransaction.Amount = walletTransactionDTO.Amount;
            walletTransaction.DateOfEntry = DateTime.Now;
            walletTransaction.Description = walletTransactionDTO.Description;
            walletTransaction.ServiceCentreId = walletTransactionDTO.ServiceCentreId;
            walletTransaction.UserId = walletTransactionDTO.UserId;
            walletTransaction.CreditDebitType = walletTransactionDTO.CreditDebitType;
            walletTransaction.IsDeferred = walletTransactionDTO.IsDeferred;
            walletTransaction.Waybill = walletTransactionDTO.Waybill;
            walletTransaction.ClientNodeId = walletTransactionDTO.ClientNodeId;
            walletTransaction.PaymentType = walletTransactionDTO.PaymentType;
            walletTransaction.PaymentTypeReference = walletTransactionDTO.PaymentTypeReference;

            await _uow.CompleteAsync();
        }
    }
}
