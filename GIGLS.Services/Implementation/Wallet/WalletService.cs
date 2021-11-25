﻿using AutoMapper;
using GIGLS.Core;
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
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace GIGLS.Services.Implementation.Wallet
{
    public class WalletService : IWalletService
    {
        private readonly INumberGeneratorMonitorService _numberGeneratorMonitorService;
        private readonly IUserService _userService;
        private readonly IUnitOfWork _uow;
        private readonly IGlobalPropertyService _globalPropertyService;

        public WalletService(IUserService userService, INumberGeneratorMonitorService numberGeneratorMonitorService, IUnitOfWork uow, IGlobalPropertyService globalPropertyService)
        {
            _numberGeneratorMonitorService = numberGeneratorMonitorService;
            _userService = userService;
            _uow = uow;
            _globalPropertyService = globalPropertyService;
            MapperConfig.Initialize();
        }

        public async Task<IEnumerable<WalletDTO>> GetWallets()
        {
            var wallets = await _uow.Wallet.GetWalletsAsync();

            //set the customer name
            foreach (var item in wallets)
            {
                // handle Company customers
                if (CustomerType.Company.Equals(item.CustomerType))
                {
                    var companyDTO = await _uow.Company.GetCompanyByIdWithCountry(item.CustomerId);

                    if (companyDTO != null)
                    {
                        item.CustomerName = companyDTO.Name;
                        item.Country = companyDTO.Country;
                        item.UserActiveCountryId = companyDTO.UserActiveCountryId;
                    }
                }
                if (CustomerType.Partner.Equals(item.CustomerType))
                {
                    var partnerDTO = await _uow.Partner.GetPartnerByIdWithCountry(item.CustomerId);
                    item.CustomerName = partnerDTO.PartnerName;
                    item.UserActiveCountryId = partnerDTO.UserActiveCountryId;
                    item.Country = partnerDTO.Country;
                }
                else
                {
                    // handle IndividualCustomers
                    var individualCustomerDTO = await _uow.IndividualCustomer.GetIndividualCustomerByIdWithCountry(item.CustomerId);
                    item.CustomerName = string.Format($"{individualCustomerDTO.FirstName} " + $"{individualCustomerDTO.LastName}");
                    item.UserActiveCountryId = individualCustomerDTO.UserActiveCountryId;
                    item.Country = individualCustomerDTO.Country;
                }
            }

            return wallets.ToList().OrderBy(x => x.CustomerName);
        }


        public async Task<WalletDTO> GetWalletById(int walletid)
        {
            var wallet = await _uow.Wallet.GetAsync(walletid);

            if (wallet == null)
            {
                throw new GenericException("Wallet does not exist");
            }
            var walletDTO = Mapper.Map<WalletDTO>(wallet);

            //set the customer name
            // handle Company customers
            if (CustomerType.Company.Equals(wallet.CustomerType))
            {
                var companyDTO = await _uow.Company.GetCompanyByIdWithCountry(walletDTO.CustomerId);

                if (companyDTO != null)
                {
                    walletDTO.CustomerName = companyDTO.Name;
                    walletDTO.CustomerEmail = companyDTO.Email;
                    walletDTO.Country = companyDTO.Country;
                    walletDTO.UserActiveCountryId = companyDTO.UserActiveCountryId;
                }
            }
            else if (CustomerType.Partner.Equals(wallet.CustomerType))
            {
                var partnerDTO = await _uow.Partner.GetPartnerByIdWithCountry(walletDTO.CustomerId);
                if (partnerDTO != null)
                {
                    walletDTO.CustomerName = partnerDTO.PartnerName;
                    walletDTO.CustomerEmail = partnerDTO.Email;
                    walletDTO.UserActiveCountryId = partnerDTO.UserActiveCountryId;
                    walletDTO.Country = partnerDTO.Country;
                }
            }
            else
            {
                // handle IndividualCustomers
                var individualCustomerDTO = await _uow.IndividualCustomer.GetIndividualCustomerByIdWithCountry(walletDTO.CustomerId);
                if (individualCustomerDTO != null)
                {
                    walletDTO.CustomerName = string.Format($"{individualCustomerDTO.FirstName} " + $"{individualCustomerDTO.LastName}");
                    walletDTO.UserActiveCountryId = individualCustomerDTO.UserActiveCountryId;
                    walletDTO.Country = individualCustomerDTO.Country;
                    walletDTO.CustomerEmail = individualCustomerDTO.Email;
                }
            }

            return walletDTO;
        }

        public async Task<Core.Domain.Wallet.Wallet> GetWalletById(string walletNumber)
        {
            var wallet = await _uow.Wallet.GetAsync(x => x.WalletNumber == walletNumber || x.CustomerCode == walletNumber);

            if (wallet == null)
            {
                throw new GenericException("Wallet does not exist", $"{(int)HttpStatusCode.NotFound}");
            }

            return wallet;
        }

        public async Task<WalletDTO> GetSystemWallet()
        {
            var wallet = await _uow.Wallet.GetAsync(x => x.IsSystem == true);

            if (wallet == null)
            {
                throw new GenericException("System Wallet does not exist");
            }

            return Mapper.Map<WalletDTO>(wallet);
        }

        public async Task AddWallet(WalletDTO wallet)
        {
            var walletNumber = await _numberGeneratorMonitorService.GenerateNextNumber(NumberGeneratorType.Wallet);
            wallet.WalletNumber = walletNumber;

            _uow.Wallet.Add(new Core.Domain.Wallet.Wallet
            {
                WalletId = wallet.WalletId,
                WalletNumber = wallet.WalletNumber,
                Balance = wallet.Balance,
                CustomerId = wallet.CustomerId,
                CustomerType = wallet.CustomerType,
                CustomerCode = wallet.CustomerCode,
                CompanyType = wallet.CompanyType
            });
            await _uow.CompleteAsync();
        }

        public async Task UpdateWallet(int walletId, WalletTransactionDTO walletTransactionDTO, bool hasServiceCentre = true)
        {
            var wallet = await _uow.Wallet.GetAsync(walletId);
            if (wallet == null)
            {
                throw new GenericException("Wallet does not exists", $"{(int)HttpStatusCode.NotFound}");
            }

            //verify second time to reduce multiple credit of account
            if (!string.IsNullOrWhiteSpace(walletTransactionDTO.PaymentTypeReference))
            {
                var walletTrans = await _uow.WalletTransaction.GetAsync(x => x.PaymentTypeReference == walletTransactionDTO.PaymentTypeReference);

                if (walletTrans != null)
                {
                    //update wallet payment log
                    var paymentLog = await _uow.WalletPaymentLog.GetAsync(x => x.Reference == walletTransactionDTO.PaymentTypeReference);
                    if (paymentLog != null)
                    {
                        paymentLog.IsWalletCredited = true;
                        await _uow.CompleteAsync();
                    }

                    throw new GenericException("Account Already Credited, Kindly check your wallet", $"{(int)HttpStatusCode.Forbidden}");
                }

            }

            //Manage want every customer to be eligible
            //await CheckIfEcommerceIsEligible(wallet, walletTransactionDTO.Amount);

            if (walletTransactionDTO.UserId == null)
            {
                walletTransactionDTO.UserId = await _userService.GetCurrentUserId();
            }
            var user = await _userService.GetUserById(walletTransactionDTO.UserId);

            var serviceCenterIds = new int[] { };
            if (hasServiceCentre == true)
            {
                serviceCenterIds = await _userService.GetPriviledgeServiceCenters();
            }

            if (serviceCenterIds.Length < 1)
            {
                serviceCenterIds = new int[] { 0 };
                var defaultServiceCenter = await _userService.GetDefaultServiceCenter();
                serviceCenterIds[0] = defaultServiceCenter.ServiceCentreId;
            }

            var newWalletTransaction = Mapper.Map<WalletTransaction>(walletTransactionDTO);
            newWalletTransaction.WalletId = walletId;
            newWalletTransaction.DateOfEntry = DateTime.Now;
            newWalletTransaction.ServiceCentreId = serviceCenterIds[0];
            newWalletTransaction.UserId = walletTransactionDTO.UserId;
            newWalletTransaction.TransactionCountryId = user.UserActiveCountryId;
            if (newWalletTransaction.CreditDebitType == CreditDebitType.Credit)
            {
                newWalletTransaction.BalanceAfterTransaction = wallet.Balance + newWalletTransaction.Amount;
            }
            else
            {
                newWalletTransaction.BalanceAfterTransaction = wallet.Balance - newWalletTransaction.Amount;
            }
            _uow.WalletTransaction.Add(newWalletTransaction);
            await _uow.CompleteAsync();

            //get balance
            var walletTransactions = await _uow.WalletTransaction.FindAsync(s => s.WalletId == walletId);
            decimal balance = 0;
            foreach (var item in walletTransactions)
            {
                if (item.CreditDebitType == CreditDebitType.Credit)
                {
                    balance += item.Amount;
                }
                else
                {
                    balance -= item.Amount;
                }
            }

            //wallet = await _uow.Wallet.GetAsync(walletId);
            wallet.Balance = balance;
            await _uow.CompleteAsync();
        }

        public async Task RemoveWallet(int walletId)
        {
            var wall = await _uow.Wallet.GetAsync(walletId);

            if (wall == null)
            {
                throw new GenericException("Wallet does not exists", $"{(int)HttpStatusCode.NotFound}");
            }

            _uow.Wallet.Remove(wall);
            await _uow.CompleteAsync();
        }

        public async Task<WalletNumber> GenerateNextValidWalletNumber()
        {
            //1. Get the last wallet number
            var walletNumber = await _uow.WalletNumber.GetLastValidWalletNumber();

            // At this point, walletNumber can only be null if it's the first time we're
            // creating a wallet. If that's the case, we assume our wallet PAN to be "0".
            var walletPan = walletNumber?.WalletPan ?? "0";

            //2. Increment and pad walletPan to get the next available wallet number
            var number = long.Parse(walletPan) + 1;
            var numberStr = number.ToString("0000000000");

            //3. Return New Wallet Number
            return new WalletNumber
            {
                WalletPan = numberStr,
                IsActive = true
            };
        }

        public async Task<List<WalletDTO>> SearchForWallets(WalletSearchOption searchOption)
        {
            try
            {
                List<WalletDTO> walletsDto = new List<WalletDTO>();
                List<PartnerDTO> partners = new List<PartnerDTO>();
                List<CompanyDTO> companies = new List<CompanyDTO>();
                List<IndividualCustomerDTO> individualCustomer = new List<IndividualCustomerDTO>();

                var walletsQueryable = _uow.Wallet.GetWalletsAsQueryable();

                if (!string.IsNullOrWhiteSpace(searchOption.SearchData))
                {
                    List<string> customerCodes = new List<string>();

                    if (searchOption.CustomerType == FilterCustomerType.Ecommerce || searchOption.CustomerType == FilterCustomerType.Corporate)
                    {
                        companies = await _uow.Company.GetCompanyByEmail(searchOption.SearchData);
                        customerCodes = companies.Select(x => x.CustomerCode).ToList();
                    }
                    else if (searchOption.CustomerType == FilterCustomerType.Partner)
                    {
                        partners = await _uow.Partner.GetPartnerBySearchParameters(searchOption.SearchData);
                        customerCodes = partners.Select(x => x.PartnerCode).ToList();
                    }
                    else if (searchOption.CustomerType == FilterCustomerType.IndividualCustomer)
                    {
                        individualCustomer = await _uow.IndividualCustomer.GetIndividualCustomers(searchOption.SearchData);
                        customerCodes = individualCustomer.Select(x => x.CustomerCode).ToList();
                    }

                    walletsQueryable = walletsQueryable.Where(x => customerCodes.Contains(x.CustomerCode));
                    walletsDto = Mapper.Map<List<WalletDTO>>(walletsQueryable.ToList());
                }

                ////set the customer name
                foreach (var item in walletsDto)
                {
                    // handle Company customers
                    if (CustomerType.Company == item.CustomerType)
                    {
                        if (companies.Any())
                        {
                            var company = companies.Where(x => x.CustomerCode == item.CustomerCode).FirstOrDefault();

                            if (company != null)
                            {
                                item.CustomerName = company.Name;
                                item.Country = company.Country;
                                item.CustomerPhoneNumber = company.PhoneNumber;
                                item.CustomerEmail = company.Email;
                                item.UserActiveCountryId = company.UserActiveCountryId;
                            }
                        }
                        else
                        {
                            var walletDTO = await SearchWalletDetails(item.CustomerType, item.CustomerCode);

                            if (walletDTO != null)
                            {
                                item.CustomerName = walletDTO.CustomerName;
                                item.Country = walletDTO.Country;
                                item.CustomerPhoneNumber = walletDTO.CustomerPhoneNumber;
                                item.CustomerEmail = walletDTO.CustomerEmail;
                                item.UserActiveCountryId = walletDTO.UserActiveCountryId;
                            }

                        }
                    }
                    else if (CustomerType.Partner == item.CustomerType)
                    {
                        if (partners.Any())
                        {
                            var partner = partners.Where(x => x.PartnerCode == item.CustomerCode).FirstOrDefault();

                            if (partner != null)
                            {
                                item.CustomerName = partner.PartnerName;
                                item.CustomerPhoneNumber = partner.PhoneNumber;
                                item.CustomerEmail = partner.Email;
                                item.Country = partner.Country;
                                item.UserActiveCountryId = partner.UserActiveCountryId;
                            }
                        }
                        else
                        {
                            var walletDTO = await SearchWalletDetails(item.CustomerType, item.CustomerCode);

                            if (walletDTO != null)
                            {
                                item.CustomerName = walletDTO.CustomerName;
                                item.CustomerPhoneNumber = walletDTO.CustomerPhoneNumber;
                                item.CustomerEmail = walletDTO.CustomerEmail;
                                item.Country = walletDTO.Country;
                                item.UserActiveCountryId = walletDTO.UserActiveCountryId;
                            }

                        }
                    }
                    else
                    {
                        // handle IndividualCustomers
                        if (individualCustomer.Any())
                        {
                            var individual = individualCustomer.Where(x => x.CustomerCode == item.CustomerCode).FirstOrDefault();

                            if (individual != null)
                            {
                                item.CustomerName = string.Format($"{individual.FirstName} " + $"{individual.LastName}");
                                item.CustomerPhoneNumber = individual.PhoneNumber;
                                item.CustomerEmail = individual.Email;
                                item.UserActiveCountryId = individual.UserActiveCountryId;
                                item.Country = individual.Country;
                            }
                        }
                        else
                        {
                            var walletDTO = await SearchWalletDetails(item.CustomerType, item.CustomerCode);

                            if (walletDTO != null)
                            {
                                item.CustomerName = walletDTO.CustomerName;
                                item.CustomerPhoneNumber = walletDTO.CustomerPhoneNumber;
                                item.CustomerEmail = walletDTO.CustomerEmail;
                                item.UserActiveCountryId = walletDTO.UserActiveCountryId;
                                item.Country = walletDTO.Country;
                            }
                        }
                    }
                }

                return walletsDto.OrderBy(x => x.CustomerName).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<WalletDTO> SearchWalletDetails(CustomerType customerType, string customerCode)
        {
            var item = new WalletDTO();

            if (CustomerType.Company == customerType)
            {
                var companyData = await _uow.Company.GetCompanyByEmail(customerCode);
                var company = companyData.FirstOrDefault();

                if (company != null)
                {
                    item.CustomerName = company.Name;
                    item.Country = company.Country;
                    item.CustomerPhoneNumber = company.PhoneNumber;
                    item.CustomerEmail = company.Email;
                    item.UserActiveCountryId = company.UserActiveCountryId;
                }
            }
            else if (CustomerType.Partner == customerType)
            {

                var partnerData = await _uow.Partner.GetPartnerBySearchParameters(customerCode);
                var partner = partnerData.FirstOrDefault();

                if (partner != null)
                {
                    item.CustomerName = partner.PartnerName;
                    item.CustomerPhoneNumber = partner.PhoneNumber;
                    item.CustomerEmail = partner.Email;
                    item.Country = partner.Country;
                    item.UserActiveCountryId = partner.UserActiveCountryId;
                }
            }
            else
            {

                var individualData = await _uow.IndividualCustomer.GetIndividualCustomers(customerCode);
                var individual = individualData.FirstOrDefault();

                if (individual != null)
                {
                    item.CustomerName = string.Format($"{individual.FirstName} " + $"{individual.LastName}");
                    item.CustomerPhoneNumber = individual.PhoneNumber;
                    item.CustomerEmail = individual.Email;
                    item.UserActiveCountryId = individual.UserActiveCountryId;
                    item.Country = individual.Country;
                }
            }

            return item;

        }

        public async Task<WalletDTO> GetWalletBalance()
        {
            var currentUser = await _userService.GetCurrentUserId();
            var user = await _uow.User.GetUserById(currentUser);
            var wallet = await _uow.Wallet.GetAsync(x => x.CustomerCode == user.UserChannelCode);

            if (wallet == null)
            {
                throw new GenericException("Wallet does not exist", $"{(int)HttpStatusCode.NotFound}");
            }

            var walletDTO = Mapper.Map<WalletDTO>(wallet);
            walletDTO.UserActiveCountryId = user.UserActiveCountryId;
            return walletDTO;
        }

        public async Task<WalletDTO> GetWalletBalance(string userChannelCode)
        {
            var wallet = await _uow.Wallet.GetAsync(x => x.CustomerCode.Equals(userChannelCode));
            if (wallet == null)
            {
                throw new GenericException("Wallet does not exist", $"{(int)HttpStatusCode.NotFound}");
            }

            var walletDTO = Mapper.Map<WalletDTO>(wallet);
            return walletDTO;
        }

        public async Task<WalletDTO> GetWalletBalanceWithName()
        {
            var currentUser = await _userService.GetCurrentUserId();
            var user = await _uow.User.GetUserById(currentUser);
            var wallet = await _uow.Wallet.GetAsync(x => x.CustomerCode.Equals(user.UserChannelCode));
            if (wallet == null)
            {
                throw new GenericException("Wallet does not exist", $"{(int)HttpStatusCode.NotFound}");
            }

            var walletDTO = Mapper.Map<WalletDTO>(wallet);
            var country = await _uow.Country.GetAsync(x => x.CountryId == user.UserActiveCountryId);
            walletDTO.Country = Mapper.Map<CountryDTO>(country);

            if (wallet.CompanyType == CustomerType.IndividualCustomer.ToString())
            {
                var customer = await _uow.IndividualCustomer.GetAsync(x => x.CustomerCode == wallet.CustomerCode);
                walletDTO.CustomerName = customer.FirstName + " " + customer.LastName;
            }
            else
            {
                var customer = await _uow.Company.GetAsync(x => x.CustomerCode == wallet.CustomerCode);
                walletDTO.CustomerName = customer.Name;
            }
            return walletDTO;
        }

        public IQueryable<Core.Domain.Wallet.Wallet> GetWalletAsQueryableService()
        {
            var wallet = _uow.Wallet.GetAllAsQueryable();
            return wallet;
        }

        private async Task CheckIfEcommerceIsEligible(Core.Domain.Wallet.Wallet wallet, decimal amount)
        {
            var company = await _uow.Company.GetAsync(s => s.CustomerCode == wallet.CustomerCode);

            if (company != null)
            {
                if (company.IsEligible == true)
                    return;

                decimal codAmountValue;

                if (company.isCodNeeded)
                {
                    var codAmount = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.EcommerceCodAmount, company.UserActiveCountryId);
                    codAmountValue = Convert.ToDecimal(codAmount.Value);
                }
                else
                {
                    var noCoDAmount = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.EcommerceNoCodAmount, company.UserActiveCountryId);
                    codAmountValue = Convert.ToDecimal(noCoDAmount.Value);
                }

                company.WalletAmount = Convert.ToDecimal(company.WalletAmount) + amount;

                if (company.WalletAmount >= codAmountValue)
                {
                    company.IsEligible = true;
                }
                else
                {
                    company.IsEligible = false;
                }
            }
        }

        public async Task<List<WalletDTO>> GetOutstaningCorporatePayments()
        {

            var walletDTO = await _uow.Wallet.GetOutstaningCorporatePayments();

            return walletDTO;
        }

        public async Task<ResponseDTO> ChargeWallet(ChargeWalletDTO chargeWalletDTO)
        {
            try
            {
                var result = new ResponseDTO();
                if (chargeWalletDTO == null)
                {
                    result.Succeeded = false;
                    result.Message = $"Invalid payload";
                    return result;
                }
                

                if (chargeWalletDTO.Amount <= 0)
                {
                    result.Succeeded = false;
                    result.Message = $"Invalid amount";
                    return result;
                }

                var limit = await _uow.GlobalProperty.GetAsync(x => x.Key == GlobalPropertyType.AirtimeAmountLimit.ToString());
                if (limit == null)
                {
                    result.Succeeded = false;
                    result.Message = $"Airtime limit does not exist";
                    return result;
                }

                int limitAmount = Convert.ToInt32(limit.Value);
                if (chargeWalletDTO.Amount > limitAmount)
                {
                    result.Succeeded = false;
                    result.Message = $"We are sorry you have exceeded the maximum limit for airtime recharge.";
                    return result;
                }

                if (String.IsNullOrEmpty(chargeWalletDTO.UserId) || chargeWalletDTO.Amount <= 0)
                {
                    result.Succeeded = false;
                    result.Message = $"User or amount not provided";
                    return result;
                }

                var user = await _uow.User.GetUserById(chargeWalletDTO.UserId);
                if (user == null)
                {
                    result.Succeeded = false;
                    result.Message = $"user does not exist";
                    return result;
                }
                var wallet = await _uow.Wallet.GetAsync(x => x.CustomerCode.Equals(user.UserChannelCode));
                if (wallet == null)
                {
                    result.Succeeded = false;
                    result.Message = $"Wallet does not exist";
                    return result;
                }

                //add xtra charge for tv or electricity
                if (chargeWalletDTO.BillType == BillType.TVSUB || chargeWalletDTO.BillType == BillType.ELECTRICITY)
                {
                    var subCharge = await _uow.GlobalProperty.GetAsync(x => x.Key == GlobalPropertyType.SubscriptionCharge.ToString());
                    if (subCharge != null)
                    {
                        var chargeAmount = Convert.ToDecimal(subCharge.Value);
                        chargeWalletDTO.Amount = chargeWalletDTO.Amount + chargeAmount;
                    }
                }

                //charge wallet
                if ((wallet.Balance - chargeWalletDTO.Amount) >= 0)
                {
                    wallet.Balance -= chargeWalletDTO.Amount;
                }
                else
                {
                    result.Succeeded = false;
                    result.Message = $"Insufficient balance on customer wallet";
                    return result;
                }
                await _uow.CompleteAsync();

                //update wallet transaction
                //generate paymentref
                var today = DateTime.Now;
                var referenceNo = $"{user.UserChannelCode}{DateTime.Now.ToString("ddMMyyyss")}";
                var desc = (chargeWalletDTO.BillType == BillType.ClassSubscription) ? "Customer subscription" 
                    : chargeWalletDTO.Description;
                if (chargeWalletDTO.BillType != BillType.ClassSubscription)
                {
                    referenceNo = chargeWalletDTO.ReferenceNo;
                }
                await UpdateWallet(wallet.WalletId, new WalletTransactionDTO()
                {
                    WalletId = wallet.WalletId,
                    Amount = chargeWalletDTO.Amount,
                    CreditDebitType = CreditDebitType.Debit,
                    Description = desc,
                    PaymentType = PaymentType.Wallet,
                    PaymentTypeReference = referenceNo,
                    UserId = chargeWalletDTO.UserId
                }, false);
                result.Succeeded = true;
                result.Message = $"Wallet successfully charged";
                result.Entity = new { transactionId = referenceNo };
                return result;
            }
            catch (Exception ex)
            {

                throw;
            }
        }


        public async Task<List<WalletDTO>> GetUserWallets(WalletSearchOption searchOption)
        {
            try
            {
                List<WalletDTO> walletsDto = new List<WalletDTO>();
                List<int> customerIds = new List<int>();


                var companyIds = _uow.Company.GetAllAsQueryable().Where(x => x.Email == searchOption.SearchData).ToList();
                var individualIds = _uow.IndividualCustomer.GetAllAsQueryable().Where(x => x.Email == searchOption.SearchData).ToList();
                var partnerIds = _uow.Partner.GetAllAsQueryable().Where(x => x.Email == searchOption.SearchData).ToList();
                customerIds.AddRange(companyIds.Select(x => x.CompanyId));
                customerIds.AddRange(individualIds.Select(x => x.IndividualCustomerId));
                customerIds.AddRange(partnerIds.Select(x => x.PartnerId));

                var wallets = _uow.Wallet.GetWalletsAsQueryable().Where(x => customerIds.Contains(x.CustomerId)).ToList();
                var walletDTO = Mapper.Map<List<WalletDTO>>(wallets.ToList());
                ////set the customer name
                foreach (var item in walletDTO)
                {
                    // handle Company customers
                    if (CustomerType.Company == item.CustomerType)
                    {
                        if (companyIds.Any())
                        {
                            var company = companyIds.Where(x => x.CustomerCode == item.CustomerCode).FirstOrDefault();

                            if (company != null)
                            {
                                item.CustomerName = company.Name;
                                item.CustomerPhoneNumber = company.PhoneNumber;
                                item.CustomerEmail = company.Email;
                                item.UserActiveCountryId = company.UserActiveCountryId;
                                walletsDto.Add(item);
                            }
                        }

                    }
                    if (CustomerType.IndividualCustomer == item.CustomerType)
                    {
                        // handle IndividualCustomers
                        if (individualIds.Any())
                        {
                            var individual = individualIds.Where(x => x.CustomerCode == item.CustomerCode).FirstOrDefault();

                            if (individual != null)
                            {
                                item.CustomerName = string.Format($"{individual.FirstName} " + $"{individual.LastName}");
                                item.CustomerPhoneNumber = individual.PhoneNumber;
                                item.CustomerEmail = individual.Email;
                                item.UserActiveCountryId = individual.UserActiveCountryId;
                                walletsDto.Add(item);
                            }
                        }

                    }
                    if (CustomerType.Partner == item.CustomerType)
                    {
                        // handle Partner
                        if (partnerIds.Any())
                        {
                            var partner = partnerIds.Where(x => x.PartnerCode == item.CustomerCode).FirstOrDefault();
                            if (partner != null)
                            {
                                item.CustomerName = string.Format($"{partner.FirstName} " + $"{partner.LastName}");
                                item.CustomerPhoneNumber = partner.PhoneNumber;
                                item.CustomerEmail = partner.Email;
                                item.UserActiveCountryId = partner.UserActiveCountryId;
                                walletsDto.Add(item);
                            }
                        }
                    }
                }

                return walletsDto.OrderBy(x => x.CustomerName).ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> ChargeUserWallet(WalletDTO walletDTO)
        {
            try
            {
                if (walletDTO == null)
                {
                    throw new GenericException("Invalid payload", $"{(int)HttpStatusCode.BadRequest}");
                }
                var wallet = await _uow.Wallet.GetAsync(walletDTO.WalletId);
                if (wallet == null)
                {
                    throw new GenericException("Wallet does not exists", $"{(int)HttpStatusCode.NotFound}");
                }
                if (wallet.Balance < walletDTO.AmountToCharge)
                {
                    throw new GenericException("Insufficient fund", $"{(int)HttpStatusCode.BadRequest}");
                }
                var walletTransaction = new WalletTransactionDTO();
                walletTransaction.Amount = walletDTO.AmountToCharge;
                walletTransaction.Waybill = String.Empty;
                walletTransaction.UserId = await _userService.GetCurrentUserId();
                walletTransaction.Description = walletDTO.Reason;
                walletTransaction.PaymentType = PaymentType.Cash;
                walletTransaction.PaymentTypeReference = String.Empty;
                walletTransaction.DateOfEntry = DateTime.Now;
                walletTransaction.CreditDebitType = CreditDebitType.Debit;
                await UpdateWallet(walletDTO.WalletId, walletTransaction, false);
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task TopUpWallet(int walletId, WalletTransactionDTO walletTransactionDTO, bool hasServiceCentre = true)
        {
            try
            {
                if (walletTransactionDTO.CreditDebitType == CreditDebitType.Credit)
                {
                    //check if user is authorized
                    var passKey = ConfigurationManager.AppSettings["Pass4sureGIG"];
                    if (passKey != walletTransactionDTO.PassKey)
                    {
                        throw new GenericException("You are not authorized to perform this action", $"{(int)HttpStatusCode.Forbidden}");
                    }
                }
                await UpdateWallet(walletId, walletTransactionDTO, hasServiceCentre);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ResponseDTO> ReverseWallet(string reference)
        {
            try
            {
                var result = new ResponseDTO();
                var walletTrans = await _uow.WalletTransaction.GetAsync(x => x.PaymentTypeReference == reference);
                if (walletTrans == null)
                {
                    result.Succeeded = false;
                    result.Message = $"Wallet transaction does not exist";
                    return result;
                }

                //check if wallet already credited
                var checks = _uow.WalletTransaction.Find(x => x.PaymentTypeReference == reference);
                foreach (var check in checks)
                {
                    if (check.CreditDebitType == CreditDebitType.Credit)
                    {
                        result.Succeeded = false;
                        result.Message = $"Wallet already credited";
                        return result;
                    }
                }

                var user = await _uow.User.GetUserById(walletTrans.UserId);
                if (user == null)
                {
                    result.Succeeded = false;
                    result.Message = $"User does not exist";
                    return result;
                }
                var wallet = await _uow.Wallet.GetAsync(x => x.CustomerCode.Equals(user.UserChannelCode));
                if (wallet == null)
                {
                    result.Succeeded = false;
                    result.Message = $"Wallet does not exist";
                    return result;
                }

                //charge wallet
                if (walletTrans.CreditDebitType == CreditDebitType.Debit)
                {
                    if ((wallet.Balance + walletTrans.Amount) >= 0)
                    {
                        wallet.Balance += walletTrans.Amount;
                    }
                }
                else
                {
                    result.Succeeded = false;
                    result.Message = $"Wallet is not yet charged";
                    return result;
                }


                await _uow.CompleteAsync();

                //update wallet transaction
                //generate paymentref
                var desc = $"{walletTrans.Description} refund";

                await UpdateWalletForReverse(wallet.WalletId, new WalletTransactionDTO()
                {
                    WalletId = wallet.WalletId,
                    Amount = walletTrans.Amount,
                    CreditDebitType = CreditDebitType.Credit,
                    Description = desc,
                    PaymentType = PaymentType.Wallet,
                    PaymentTypeReference = walletTrans.PaymentTypeReference,
                    UserId = walletTrans.UserId
                }, false);
                result.Succeeded = true;
                result.Message = $"Wallet payment charge successfully refunded";
                result.Entity = new { transactionId = reference };
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task UpdateWalletForReverse(int walletId, WalletTransactionDTO walletTransactionDTO, bool hasServiceCentre = true)
        {
            var wallet = await _uow.Wallet.GetAsync(walletId);
            if (wallet == null)
            {
                throw new GenericException("Wallet does not exists", $"{(int)HttpStatusCode.NotFound}");
            }

            //verify second time to reduce multiple credit of account
            if (!string.IsNullOrWhiteSpace(walletTransactionDTO.PaymentTypeReference))
            {
                var walletTrans = await _uow.WalletTransaction.GetAsync(x => x.PaymentTypeReference == walletTransactionDTO.PaymentTypeReference);

                if (walletTrans != null && walletTrans.CreditDebitType == CreditDebitType.Credit)
                {
                    throw new GenericException("Account Already Credited, Kindly check your wallet", $"{(int)HttpStatusCode.Forbidden}");
                }

            }

            //Manage want every customer to be eligible
            //await CheckIfEcommerceIsEligible(wallet, walletTransactionDTO.Amount);

            if (walletTransactionDTO.UserId == null)
            {
                walletTransactionDTO.UserId = await _userService.GetCurrentUserId();
            }
            var user = await _userService.GetUserById(walletTransactionDTO.UserId);

            var serviceCenterIds = new int[] { };
            if (hasServiceCentre == true)
            {
                serviceCenterIds = await _userService.GetPriviledgeServiceCenters();
            }

            if (serviceCenterIds.Length < 1)
            {
                serviceCenterIds = new int[] { 0 };
                var defaultServiceCenter = await _userService.GetDefaultServiceCenter();
                serviceCenterIds[0] = defaultServiceCenter.ServiceCentreId;
            }

            var newWalletTransaction = Mapper.Map<WalletTransaction>(walletTransactionDTO);
            newWalletTransaction.WalletId = walletId;
            newWalletTransaction.DateOfEntry = DateTime.Now;
            newWalletTransaction.ServiceCentreId = serviceCenterIds[0];
            newWalletTransaction.UserId = walletTransactionDTO.UserId;
            newWalletTransaction.TransactionCountryId = user.UserActiveCountryId;

            newWalletTransaction.BalanceAfterTransaction = wallet.Balance;

            _uow.WalletTransaction.Add(newWalletTransaction);
            await _uow.CompleteAsync();

        }
    }
}