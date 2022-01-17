﻿using AutoMapper;
using GIGLS.Core;
using GIGLS.Core.Domain.Wallet;
using GIGLS.Core.DTO;
using GIGLS.Core.DTO.Report;
using GIGLS.Core.DTO.User;
using GIGLS.Core.DTO.Wallet;
using GIGLS.Core.Enums;
using GIGLS.Core.IServices;
using GIGLS.Core.IServices.Customers;
using GIGLS.Core.IServices.ServiceCentres;
using GIGLS.Core.IServices.User;
using GIGLS.Core.IServices.Wallet;
using GIGLS.CORE.DTO.Report;
using GIGLS.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
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
        private readonly ICountryService _countryservice;

        public WalletTransactionService(IUnitOfWork uow, IUserService userService,
            IWalletService walletService, ICustomerService customerService,
            IServiceCentreService centreService, ICountryService countryservice)
        {
            _uow = uow;
            _userService = userService;
            _walletService = walletService;
            _customerService = customerService;
            _centreService = centreService;
            _countryservice = countryservice;
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
        public async Task<IEnumerable<WalletTransactionDTO>> GetWalletTransactionsByDate(ShipmentCollectionFilterCriteria dateFilter)
        {
            var serviceCenterIds = await _userService.GetPriviledgeServiceCenters();
            var walletTransactions = await _uow.WalletTransaction.GetWalletTransactionDateAsync(serviceCenterIds, dateFilter);

            //get customer details
            ////set the customer name
            foreach (var item in walletTransactions)
            {
                // handle Company customers
                if (CustomerType.Company == item.Wallet.CustomerType)
                {
                    var companyDTO = await _uow.Company.GetCompanyByIdWithCountry(item.Wallet.CustomerId);

                    if (companyDTO != null)
                    {
                        item.Wallet.CustomerName = companyDTO.Name;
                        item.Wallet.Country = companyDTO.Country;
                        item.Wallet.UserActiveCountryId = companyDTO.UserActiveCountryId;
                    }
                }
                else if (CustomerType.Partner == item.Wallet.CustomerType)
                {
                    var partnerDTO = await _uow.Partner.GetPartnerByIdWithCountry(item.Wallet.CustomerId);
                   
                    if (partnerDTO != null)
                    {
                        item.Wallet.CustomerName = partnerDTO.PartnerName;
                        item.Wallet.UserActiveCountryId = partnerDTO.UserActiveCountryId;
                        item.Wallet.Country = partnerDTO.Country;
                    }
                }
                else
                {
                    // handle IndividualCustomers
                    var individualCustomerDTO = await _uow.IndividualCustomer.GetIndividualCustomerByIdWithCountry(item.Wallet.CustomerId);

                    if (individualCustomerDTO != null)
                    {
                        item.Wallet.CustomerName = string.Format($"{individualCustomerDTO.FirstName} " + $"{individualCustomerDTO.LastName}");
                        item.Wallet.UserActiveCountryId = individualCustomerDTO.UserActiveCountryId;
                        item.Wallet.Country = individualCustomerDTO.Country;
                    }
                }
            }

            return walletTransactions;
        }

        public async Task<List<WalletTransactionDTO>> GetWalletTransactionsCredit(AccountFilterCriteria accountFilterCriteria)
        {
            var serviceCenterIds = await _userService.GetPriviledgeServiceCenters();
            var walletTransactions = await _uow.WalletTransaction.GetWalletTransactionCreditAsync(serviceCenterIds, accountFilterCriteria);

            //get customer details
            ////set the customer name
            foreach (var item in walletTransactions)
            {
                // handle Company customers
                if (CustomerType.Company == item.Wallet.CustomerType)
                {
                    var companyDTO = await _uow.Company.GetCompanyByIdWithCountry(item.Wallet.CustomerId);

                    if (companyDTO != null)
                    {
                        item.Wallet.CustomerName = companyDTO.Name;
                        item.Wallet.Country = companyDTO.Country;
                        item.Wallet.UserActiveCountryId = companyDTO.UserActiveCountryId;
                    }
                }
                else if (CustomerType.Partner == item.Wallet.CustomerType)
                {
                    var partnerDTO = await _uow.Partner.GetPartnerByIdWithCountry(item.Wallet.CustomerId);

                    if (partnerDTO != null)
                    {
                        item.Wallet.CustomerName = partnerDTO.PartnerName;
                        item.Wallet.UserActiveCountryId = partnerDTO.UserActiveCountryId;
                        item.Wallet.Country = partnerDTO.Country;
                    }
                }
                else
                {
                    // handle IndividualCustomers
                    var individualCustomerDTO = await _uow.IndividualCustomer.GetIndividualCustomerByIdWithCountry(item.Wallet.CustomerId);

                    if (individualCustomerDTO != null)
                    {
                        item.Wallet.CustomerName = string.Format($"{individualCustomerDTO.FirstName} " + $"{individualCustomerDTO.LastName}");
                        item.Wallet.UserActiveCountryId = individualCustomerDTO.UserActiveCountryId;
                        item.Wallet.Country = individualCustomerDTO.Country;
                    }
                }
            }
            return walletTransactions;
        }

        public async Task<List<WalletTransactionDTO>> GetWalletTransactionCreditOrDebit(AccountFilterCriteria accountFilterCriteria)
        {
            var serviceCenterIds = await _userService.GetPriviledgeServiceCenters();
            var walletTransactions = await _uow.WalletTransaction.GetWalletTransactionCreditOrDebitAsync(serviceCenterIds, accountFilterCriteria);

            //get customer details
            ////set the customer name
            foreach (var item in walletTransactions)
            {
                // handle Company customers
                if (CustomerType.Company == item.Wallet.CustomerType)
                {
                    var companyDTO = await _uow.Company.GetCompanyByIdWithCountry(item.Wallet.CustomerId);

                    if (companyDTO != null)
                    {
                        item.Wallet.CustomerName = companyDTO.Name;
                        item.Wallet.Country = companyDTO.Country;
                        item.Wallet.UserActiveCountryId = companyDTO.UserActiveCountryId;
                    }
                }
                else if (CustomerType.Partner == item.Wallet.CustomerType)
                {
                    var partnerDTO = await _uow.Partner.GetPartnerByIdWithCountry(item.Wallet.CustomerId);

                    if (partnerDTO != null)
                    {
                        item.Wallet.CustomerName = partnerDTO.PartnerName;
                        item.Wallet.UserActiveCountryId = partnerDTO.UserActiveCountryId;
                        item.Wallet.Country = partnerDTO.Country;
                    }
                }
                else
                {
                    // handle IndividualCustomers
                    var individualCustomerDTO = await _uow.IndividualCustomer.GetIndividualCustomerByIdWithCountry(item.Wallet.CustomerId);

                    if (individualCustomerDTO != null)
                    {
                        item.Wallet.CustomerName = string.Format($"{individualCustomerDTO.FirstName} " + $"{individualCustomerDTO.LastName}");
                        item.Wallet.UserActiveCountryId = individualCustomerDTO.UserActiveCountryId;
                        item.Wallet.Country = individualCustomerDTO.Country;
                    }
                }
            }
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
            // get the wallet owner information
            var wallet = await _walletService.GetWalletById(walletId);

            //get the customer info
            //var customerDTO = await _customerService.GetCustomer(wallet.CustomerId, wallet.CustomerType);

            var walletTransactions = await _uow.WalletTransaction.FindAsync(s => s.WalletId == walletId);
            if (!walletTransactions.Any())
            {
                return new WalletTransactionSummaryDTO
                {
                    WalletTransactions = new List<WalletTransactionDTO>(),
                    WalletNumber = wallet.WalletNumber,
                    WalletBalance = wallet.Balance,
                    WalletOwnerName = wallet.CustomerName,
                    WalletId = walletId,
                    CurrencyCode = wallet.Country.CurrencyCode,
                    CurrencySymbol = wallet.Country.CurrencySymbol
                };
            }
            var walletTransactionDTOList = Mapper.Map<List<WalletTransactionDTO>>(walletTransactions.OrderByDescending(s => s.DateCreated));

            // get the service centre
            foreach (var item in walletTransactionDTOList)
            {
                var serviceCentre = await _centreService.GetServiceCentreById(item.ServiceCentreId);
                item.ServiceCentre = serviceCentre;
            }

            return new WalletTransactionSummaryDTO
            {
                WalletTransactions = walletTransactionDTOList,
                WalletNumber = wallet.WalletNumber,
                WalletBalance = wallet.Balance,
                WalletOwnerName = wallet.CustomerName,
                WalletId = walletId,
                CurrencyCode = wallet.Country.CurrencyCode,
                CurrencySymbol = wallet.Country.CurrencySymbol
            };
        }

        public async Task<WalletTransactionSummaryDTO> GetWalletTransactionByWalletId(int walletId, PaginationDTO pagination)
        {
            // get the wallet owner information
            var wallet = await _walletService.GetWalletById(walletId);
            var walletTransactions = new List<WalletTransaction>();
            //get the customer info
            //var customerDTO = await _customerService.GetCustomer(wallet.CustomerId, wallet.CustomerType);
            int totalCount;
            if (pagination == null)
            {
                pagination = new PaginationDTO
                {
                    Page = 1,
                    PageSize = 20
                };
            }
            if (pagination.StartDate != null && pagination.EndDate != null)
            {
                walletTransactions = _uow.WalletTransaction.Query(s => s.WalletId == walletId && s.DateCreated >= pagination.StartDate && s.DateCreated <= pagination.EndDate).SelectPage(pagination.Page, pagination.PageSize, out totalCount).OrderByDescending(s => s.DateCreated).ToList();

            }
            else
            {
                walletTransactions = _uow.WalletTransaction.Query(s => s.WalletId == walletId).SelectPage(pagination.Page, pagination.PageSize, out totalCount).OrderByDescending(s => s.DateCreated).ToList();
            }
            if (!walletTransactions.Any())
            {
                return new WalletTransactionSummaryDTO
                {
                    WalletTransactions = new List<WalletTransactionDTO>(),
                    WalletNumber = wallet.WalletNumber,
                    WalletBalance = wallet.Balance,
                    WalletOwnerName = wallet.CustomerName,
                    WalletId = walletId,
                    CurrencyCode = wallet.Country.CurrencyCode,
                    CurrencySymbol = wallet.Country.CurrencySymbol
                };
            }
            var walletTransactionDTOList = Mapper.Map<List<WalletTransactionDTO>>(walletTransactions.OrderByDescending(s => s.DateCreated));

            var countryIds = walletTransactionDTOList.Select(x => x.TransactionCountryId).ToList();
            var countries = _uow.Country.GetAllAsQueryable().Where(x => countryIds.Contains(x.CountryId)).ToList();
            // get the service centre
            foreach (var item in walletTransactionDTOList)
            {
                var serviceCentre = await _centreService.GetServiceCentreById(item.ServiceCentreId);
                item.ServiceCentre = serviceCentre;
                var transactionCountry = countries.FirstOrDefault(x => x.CountryId == item.TransactionCountryId);
                if (transactionCountry != null)
                {
                    item.CurrencyCode = transactionCountry.CurrencyCode;
                    item.CurrencySymbol = transactionCountry.CurrencySymbol;
                }
                else
                {
                    item.CurrencyCode = wallet.Country.CurrencyCode;
                    item.CurrencySymbol = wallet.Country.CurrencySymbol;
                }
            }

            return new WalletTransactionSummaryDTO
            {
                WalletTransactions = walletTransactionDTOList,
                WalletNumber = wallet.WalletNumber,
                WalletBalance = wallet.Balance,
                WalletOwnerName = wallet.CustomerName,
                WalletId = walletId,
                CurrencyCode = wallet.Country.CurrencyCode,
                CurrencySymbol = wallet.Country.CurrencySymbol
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

        private async Task<WalletTransactionSummaryDTO> GetWalletTransactionByWalletIdForMobile(WalletDTO wallet)
        {
            //get the customer info
            var country = new CountryDTO();
            var userid = await _userService.GetUserByChannelCode(wallet.CustomerCode);
            if (userid != null)
            {
                if (userid.UserActiveCountryId != 0)
                {
                    country = await _countryservice.GetCountryById(userid.UserActiveCountryId);
                }
                else
                {
                    country = await _countryservice.GetCountryById(1);
                }
            }
            var walletTransactions = await _uow.WalletTransaction.FindAsync(s => s.WalletId == wallet.WalletId);
            if (!walletTransactions.Any())
            {
                return new WalletTransactionSummaryDTO
                {
                    WalletTransactions = new List<WalletTransactionDTO>(),
                    CurrencyCode = country.CurrencyCode,
                    CurrencySymbol = country.CurrencySymbol,
                    WalletNumber = wallet.WalletNumber,
                    WalletBalance = wallet.Balance,
                    WalletOwnerName = userid.FirstName + " " + userid.LastName,
                    WalletId = wallet.WalletId

                };
            }
            var walletTransactionDTOList = Mapper.Map<List<WalletTransactionDTO>>(walletTransactions.OrderByDescending(s => s.DateCreated));

            return new WalletTransactionSummaryDTO
            {
                WalletTransactions = walletTransactionDTOList,
                CurrencyCode = country.CurrencyCode,
                CurrencySymbol = country.CurrencySymbol,
                WalletNumber = wallet.WalletNumber,
                WalletBalance = wallet.Balance,
                WalletOwnerName = userid.FirstName + " " + userid.LastName,
                WalletId = wallet.WalletId

            };
        }

        private async Task<WalletTransactionSummaryDTO> GetWalletTransactionByWalletIdForMobile()
        {
            //get the customer info
            var currentUser = await _userService.GetCurrentUserId();
            var customer = await _uow.User.GetUserById(currentUser);

            //get customer country info
            var country = new CountryDTO();
            if (customer != null)
            {
                if (customer.UserActiveCountryId != 0)
                {
                    country = await _countryservice.GetCountryById(customer.UserActiveCountryId);
                }
                else
                {
                    country = await _countryservice.GetCountryById(1);
                }
            }

            //Get Wallet
            var wallet = await _uow.Wallet.GetAsync(x => x.CustomerCode == customer.UserChannelCode);
            if (wallet == null)
            {
                throw new GenericException("Wallet does not exist", $"{(int)System.Net.HttpStatusCode.NotFound}");
            }

            var walletTransactions = await _uow.WalletTransaction.FindAsync(s => s.WalletId == wallet.WalletId);
            if (!walletTransactions.Any())
            {
                return new WalletTransactionSummaryDTO
                {
                    WalletTransactions = new List<WalletTransactionDTO>(),
                    CurrencyCode = country.CurrencyCode,
                    CurrencySymbol = country.CurrencySymbol,
                    WalletNumber = wallet.WalletNumber,
                    WalletBalance = wallet.Balance,
                    WalletOwnerName = customer.FirstName + " " + customer.LastName,
                    WalletId = wallet.WalletId
                };
            }

            var walletTransactionDTOList = Mapper.Map<List<WalletTransactionDTO>>(walletTransactions.OrderByDescending(s => s.DateCreated));
            var countryIds = walletTransactionDTOList.Select(x => x.TransactionCountryId).ToList();
            var countries = _uow.Country.GetAllAsQueryable().Where(x => countryIds.Contains(x.CountryId)).ToList();
            // get the service centre
            foreach (var item in walletTransactionDTOList)
            {
                var transactionCountry = countries.FirstOrDefault(x => x.CountryId == item.TransactionCountryId);
                if (transactionCountry != null)
                {
                    item.CurrencyCode = transactionCountry.CurrencyCode;
                    item.CurrencySymbol = transactionCountry.CurrencySymbol;
                }
                else
                {
                    item.CurrencyCode = country.CurrencyCode;
                    item.CurrencySymbol = country.CurrencySymbol;
                }
            }

            return new WalletTransactionSummaryDTO
            {
                WalletTransactions = walletTransactionDTOList,
                CurrencyCode = country.CurrencyCode,
                CurrencySymbol = country.CurrencySymbol,
                WalletNumber = wallet.WalletNumber,
                WalletBalance = wallet.Balance,
                WalletOwnerName = customer.FirstName + " " + customer.LastName,
                WalletId = wallet.WalletId
            };
        }

        private async Task<ModifiedWalletTransactionSummaryDTO> GetWalletTransactionByWalletIdForMobile(UserDTO customer, ShipmentCollectionFilterCriteria filterCriteria)
        {
            //get customer country info
            var country = new CountryDTO();
            if (customer != null)
            {
                if (customer.UserActiveCountryId != 0)
                {
                    country = await _countryservice.GetCountryById(customer.UserActiveCountryId);
                }
                else
                {
                    country = await _countryservice.GetCountryById(1);
                }
            }

            //Get Wallet
            var wallet = await _uow.Wallet.GetAsync(x => x.CustomerCode == customer.UserChannelCode);
            if (wallet == null)
            {
                throw new GenericException("Wallet does not exist", $"{(int)System.Net.HttpStatusCode.NotFound}");
            }

            var walletTransactionDTOList = await _uow.WalletTransaction.GetWalletTransactionMobile(wallet.WalletId, filterCriteria);
            if (walletTransactionDTOList.Any())
            {
                var countryIds = walletTransactionDTOList.Select(x => x.TransactionCountryId).ToList();
                var countries = _uow.Country.GetAllAsQueryable().Where(x => countryIds.Contains(x.CountryId)).ToList();
                // get the service centre
                foreach (var item in walletTransactionDTOList)
                {
                    var transactionCountry = countries.FirstOrDefault(x => x.CountryId == item.TransactionCountryId);
                    if (transactionCountry != null)
                    {
                        item.CurrencyCode = transactionCountry.CurrencyCode;
                        item.CurrencySymbol = transactionCountry.CurrencySymbol;
                    }
                    else
                    {
                        item.CurrencyCode = country.CurrencyCode;
                        item.CurrencySymbol = country.CurrencySymbol;
                    }
                }
            }
            return new ModifiedWalletTransactionSummaryDTO
            {
                WalletTransactions = walletTransactionDTOList,
                CurrencyCode = country.CurrencyCode,
                CurrencySymbol = country.CurrencySymbol,
                WalletNumber = wallet.WalletNumber,
                WalletBalance = wallet.Balance,
                WalletOwnerName = customer.FirstName + " " + customer.LastName,
                WalletId = wallet.WalletId
            };
        }

        public async Task<WalletTransactionSummaryDTO> GetWalletTransactionsForMobile()
        {
            //var wallet = await _walletService.GetWalletBalance();
            //return await GetWalletTransactionByWalletIdForMobile(wallet);
            return await GetWalletTransactionByWalletIdForMobile();
        }

        public async Task<ModifiedWalletTransactionSummaryDTO> GetWalletTransactionsForMobile(UserDTO customer,ShipmentCollectionFilterCriteria filterCriteria)
        {
            return await GetWalletTransactionByWalletIdForMobile(customer, filterCriteria);
        }

        private async Task<List<WalletTransactionDTO>> GetWalletTransactionByWalletIdForMobilePaginated(ShipmentAndPreShipmentParamDTO shipmentAndPreShipmentParamDTO)
        {
            //set default values if payload is null
            if (shipmentAndPreShipmentParamDTO == null)
            {
                shipmentAndPreShipmentParamDTO = new ShipmentAndPreShipmentParamDTO
                {
                 Page = 1,
                 PageSize = 20,
                 StartDate = null,
                 EndDate = null
                };
            }
            //get the customer info
            var currentUser = await _userService.GetCurrentUserId();
            var customer = await _uow.User.GetUserById(currentUser);
            int totalCount;
            var walletTransactionDTOList = new List<WalletTransactionDTO>();
            var walletTransactions = new List<WalletTransaction>();

            //get customer country info
            var country = new CountryDTO();
            if (customer != null)
            {
                if (customer.UserActiveCountryId != 0)
                {
                    country = await _countryservice.GetCountryById(customer.UserActiveCountryId);
                }
                else
                {
                    country = await _countryservice.GetCountryById(1);
                }
            }

            //Get Wallet
            var wallet = await _uow.Wallet.GetAsync(x => x.CustomerCode == customer.UserChannelCode);
            if (wallet == null)
            {
                throw new GenericException("Wallet does not exist", $"{(int)HttpStatusCode.NotFound}");
            }
            if (shipmentAndPreShipmentParamDTO.PageSize < 1)
            {
                shipmentAndPreShipmentParamDTO.PageSize = 20;
            }
            if (shipmentAndPreShipmentParamDTO.Page < 1)
            {
                shipmentAndPreShipmentParamDTO.Page = 1;
            }    

            if (shipmentAndPreShipmentParamDTO.StartDate != null && shipmentAndPreShipmentParamDTO.EndDate != null)
            {
               
                walletTransactions = _uow.WalletTransaction.Query(s => s.WalletId == wallet.WalletId && s.DateCreated >= shipmentAndPreShipmentParamDTO.StartDate && s.DateCreated <= shipmentAndPreShipmentParamDTO.EndDate).SelectPage(shipmentAndPreShipmentParamDTO.Page, shipmentAndPreShipmentParamDTO.PageSize, out totalCount).OrderBy(s => s.DateCreated).ToList();
            }
            else
            {
                walletTransactions = _uow.WalletTransaction.Query(s => s.WalletId == wallet.WalletId).SelectPage(shipmentAndPreShipmentParamDTO.Page, shipmentAndPreShipmentParamDTO.PageSize, out totalCount).OrderBy(s => s.DateCreated).ToList();
            }

            if (walletTransactions.Any())
            {
                 walletTransactionDTOList = Mapper.Map<List<WalletTransactionDTO>>(walletTransactions.OrderByDescending(x => x.DateCreated));
            }
            return walletTransactionDTOList;
        }

        public async Task<List<WalletTransactionDTO>> GetWalletTransactionsForMobilePaginated(ShipmentAndPreShipmentParamDTO shipmentAndPreShipmentParamDTO)
        {
            return await GetWalletTransactionByWalletIdForMobilePaginated(shipmentAndPreShipmentParamDTO);
        }

        public async Task<IEnumerable<WalletTransactionDTO>> GetWalletTransactionHistoryByDate(ShipmentCollectionFilterCriteria dateFilter)
        {
            var walletTransactions = await _uow.WalletTransaction.GetWalletTransactionHistoryAsync(dateFilter);

            return walletTransactions;
        }

        public async Task<WalletCreditTransactionSummaryDTO> GetWalletCreditTransactionHistoryByDate(ShipmentCollectionFilterCriteria dateFilter)
        {
            var walletTransactions = await _uow.WalletTransaction.GetWalletCreditTransactionHistoryAsync(dateFilter);
            return walletTransactions;
        }

        public async Task<IEnumerable<WalletCreditTransactionConvertedDTO>> GetWalletConversionTransactionHistory(ShipmentCollectionFilterCriteria dateFilter)
        {
            return await _uow.WalletTransaction.GetWalletConversionTransactionHistoryAsync(dateFilter);
        }
    }
}