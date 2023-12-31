﻿using POST.Core;
using POST.Core.DTO;
using POST.Core.DTO.Account;
using POST.Core.DTO.Customers;
using POST.Core.DTO.Dashboard;
using POST.Core.DTO.Report;
using POST.Core.Enums;
using POST.Core.IServices;
using POST.Core.IServices.Shipments;
using POST.Core.IServices.User;
using POST.Core.View;
using POST.CORE.DTO.Report;
using POST.CORE.IServices.Report;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace POST.Services.Implementation.Report
{
    public class AccountReportService : IAccountReportService
    {
        private readonly IUnitOfWork _uow;
        private readonly IUserService _userService;
        private readonly IShipmentService _shipmentService;
        private readonly ICountryService _countryService;

        public AccountReportService(IUnitOfWork uow, IUserService userService, IShipmentService shipmentService, ICountryService countryService)
        {
            _uow = uow;
            _userService = userService;
            _shipmentService = shipmentService;
            _countryService = countryService;
        }

        public async Task<List<GeneralLedgerDTO>> GetExpenditureReports(AccountFilterCriteria accountFilterCriteria)
        {
            //filter by User Active Country
            var userActiveCountry = await _userService.GetUserActiveCountry();
            accountFilterCriteria.CountryId = userActiveCountry.CountryId;

            var serviceCenterIds = await _userService.GetPriviledgeServiceCenters();
            accountFilterCriteria.creditDebitType = CreditDebitType.Debit;
            var generalLedgerDTO = await _uow.GeneralLedger.GetGeneralLedgersAsync(accountFilterCriteria, serviceCenterIds);

            foreach (var item in generalLedgerDTO)
            {
                var user = await _uow.User.GetUserById(item.UserId);
                item.UserId = user.FirstName + " " + user.LastName;
            }

            return generalLedgerDTO;
        }

        public async Task<List<GeneralLedgerDTO>> GetIncomeReports(AccountFilterCriteria accountFilterCriteria)
        {
            //filter by User Active Country
            var userActiveCountry = await _userService.GetUserActiveCountry();
            accountFilterCriteria.CountryId = userActiveCountry.CountryId;

            var serviceCenterIds = await _userService.GetPriviledgeServiceCenters();
            accountFilterCriteria.creditDebitType = CreditDebitType.Credit;
            var generalLedgerDTO = await _uow.GeneralLedger.GetGeneralLedgersAsync(accountFilterCriteria, serviceCenterIds);

            foreach (var item in generalLedgerDTO)
            {
                var user = await _uow.User.GetUserById(item.UserId);
                item.UserId = user.FirstName + " " + user.LastName;
            }
            return generalLedgerDTO;
        }

        public async Task<List<GeneralLedgerDTO>> GetDemurageReports(AccountFilterCriteria accountFilterCriteria)
        {
            //filter by User Active Country
            var userActiveCountry = await _userService.GetUserActiveCountry();
            accountFilterCriteria.CountryId = userActiveCountry.CountryId;

            int[] serviceCenterIds = null;

            if (accountFilterCriteria.ServiceCentreId == 0)
            {
                serviceCenterIds = await _userService.GetPriviledgeServiceCenters();

            }
            else
            {
                int[] serviceCenterId = new int[] {
                    accountFilterCriteria.ServiceCentreId
                };
                serviceCenterIds = serviceCenterId;

            }

            accountFilterCriteria.PaymentServiceType = PaymentServiceType.Demurage;
            var generalLedgerDTO = await _uow.GeneralLedger.GetGeneralLedgersAsync(accountFilterCriteria, serviceCenterIds);

            foreach (var item in generalLedgerDTO)
            {
                var user = await _uow.User.GetUserById(item.UserId);
                item.UserId = user.FirstName + " " + user.LastName;
            }
            return generalLedgerDTO;
        }

        public async Task<List<InvoiceDTO>> GetInvoiceReports(AccountFilterCriteria accountFilterCriteria)
        {
            var serviceCenterIds = await _userService.GetPriviledgeServiceCenters();
            var invoices = await _uow.Invoice.GetInvoicesAsync(accountFilterCriteria, serviceCenterIds);

            //get shipment info
            foreach (var item in invoices)
            {
                var shipmentDTO = await _shipmentService.GetShipment(item.Waybill);
                var user = await _uow.User.GetUserById(shipmentDTO.UserId);
                item.Shipment = shipmentDTO;
                item.Shipment.UserId = user.FirstName + " " + user.LastName;
            }
            return invoices;
        }

        public async Task<List<InvoiceViewDTO>> GetInvoiceReportsFromView(AccountFilterCriteria accountFilterCriteria)
        {
            //filter by User Active Country
            var userActiveCountry = await _userService.GetUserActiveCountry();
            accountFilterCriteria.CountryId = userActiveCountry.CountryId;


            var serviceCenterIds = await _userService.GetPriviledgeServiceCenters();
            var invoices = await _uow.Invoice.GetInvoicesFromViewAsyncFromSP(accountFilterCriteria, serviceCenterIds);

            foreach (var item in invoices)
            {
                //get CustomerDetails
                if (item.CustomerType.Contains("Individual"))
                {
                    item.CustomerType = CustomerType.IndividualCustomer.ToString();
                }

                CustomerType customerType = (CustomerType)Enum.Parse(typeof(CustomerType), item.CustomerType);
                //var customerDetails = await _shipmentService.GetCustomer(item.CustomerId, customerType);
                var customerDetails = new CustomerDTO()
                {
                    CustomerType = customerType,
                    CustomerCode = item.CustomerCode,
                    Email = item.Email,
                    PhoneNumber = item.PhoneNumber,
                    CompanyId = item.CompanyId.GetValueOrDefault(),
                    Name = item.Name,
                    IndividualCustomerId = item.IndividualCustomerId.GetValueOrDefault(),
                    FirstName = item.FirstName,
                    LastName = item.LastName
                };

                item.CustomerDetails = customerDetails;

                //Update to change the Corporate Paid status from 'Paid' to 'Credit'
                item.PaymentStatusDisplay = item.PaymentStatus.ToString();
                if ((CompanyType.Corporate.ToString() == item.CompanyType)
                    && (PaymentStatus.Paid == item.PaymentStatus))
                {
                    item.PaymentStatusDisplay = "Credit";
                }
            }

            return invoices;
        }

        public async Task<List<InvoiceViewDTO>> GetInvoiceReportsFromViewPlusDeliveryTime(AccountFilterCriteria accountFilterCriteria)
        {
            var serviceCenterIds = await _userService.GetPriviledgeServiceCenters();
            var invoices = await _uow.Invoice.GetInvoicesFromViewWithDeliveryTimeAsyncFromSP(accountFilterCriteria, serviceCenterIds);

            foreach (var item in invoices)
            {
                //get CustomerDetails
                if (item.CustomerType.Contains("Individual"))
                {
                    item.CustomerType = CustomerType.IndividualCustomer.ToString();
                }

                CustomerType customerType = (CustomerType)Enum.Parse(typeof(CustomerType), item.CustomerType);
                //var customerDetails = await _shipmentService.GetCustomer(item.CustomerId, customerType);
                var customerDetails = new CustomerDTO()
                {
                    CustomerType = customerType,
                    CustomerCode = item.CustomerCode,
                    Email = item.Email,
                    PhoneNumber = item.PhoneNumber,
                    CompanyId = item.CompanyId.GetValueOrDefault(),
                    Name = item.Name,
                    IndividualCustomerId = item.IndividualCustomerId.GetValueOrDefault(),
                    FirstName = item.FirstName,
                    LastName = item.LastName
                };

                item.CustomerDetails = customerDetails;

                //Update to change the Corporate Paid status from 'Paid' to 'Credit'
                item.PaymentStatusDisplay = item.PaymentStatus.ToString();
                if ((CompanyType.Corporate.ToString() == item.CompanyType)
                    && (PaymentStatus.Paid == item.PaymentStatus))
                {
                    item.PaymentStatusDisplay = "Credit";
                }
            }

            return invoices;
        }

        //Get Earnings Breakdown
        public async Task<EarningsBreakdownDTO> GetEarningsBreakdown(DashboardFilterCriteria dashboardFilter)
        {
            string currencySymbol = null;

            if (dashboardFilter.ActiveCountryId == null)
            {
                CountryDTO userActiveCountry = null;

                //set user active country from PriviledgeCountrys
                var countries = await _userService.GetPriviledgeCountrys();
                if (countries.Count == 1)
                {
                    userActiveCountry = countries[0];
                }
                else
                {
                    //If UserActive Country is already set in the UserEntity, use that value
                    string currentUserId = await _userService.GetCurrentUserId();
                    var currentUser = await _userService.GetUserById(currentUserId);

                    if (currentUser.UserActiveCountryId > 0)
                    {
                        var userActiveCountryFromEntity = await _countryService.GetCountryById(currentUser.UserActiveCountryId);
                        if (userActiveCountryFromEntity.CurrencySymbol != null)
                        {
                            userActiveCountry = userActiveCountryFromEntity;
                        }
                    }
                }

                dashboardFilter.ActiveCountryId = userActiveCountry.CountryId;
                currencySymbol = userActiveCountry.CurrencySymbol;
            }
            else
            {
                var userActiveCountryFromEntity = await _countryService.GetCountryById((int)dashboardFilter.ActiveCountryId);
                if (userActiveCountryFromEntity.CurrencySymbol != null)
                {
                    currencySymbol = userActiveCountryFromEntity.CurrencySymbol;
                }
            }

            var earnings = await _uow.FinancialReport.GetEarningsBreakdown(dashboardFilter);
            earnings.CurrencySymbol = currencySymbol;

            return earnings;

        }

        //Get Each Financial Breakdown
        public async Task<List<FinancialReportDTO>> GetFinancialBreakdownByType(AccountFilterCriteria accountFilter)
        {
            if (accountFilter.CountryId == 0)
            {
                CountryDTO userActiveCountry = null;

                //set user active country from PriviledgeCountrys
                var countries = await _userService.GetPriviledgeCountrys();
                if (countries.Count == 1)
                {
                    userActiveCountry = countries[0];
                }
                else
                {
                    //If UserActive Country is already set in the UserEntity, use that value
                    string currentUserId = await _userService.GetCurrentUserId();
                    var currentUser = await _userService.GetUserById(currentUserId);

                    if (currentUser.UserActiveCountryId > 0)
                    {
                        var userActiveCountryFromEntity = await _countryService.GetCountryById(currentUser.UserActiveCountryId);
                        if (userActiveCountryFromEntity.CurrencySymbol != null)
                        {
                            userActiveCountry = userActiveCountryFromEntity;
                        }
                    }
                }
                accountFilter.CountryId = userActiveCountry.CountryId;
            }
            var earnings = new List<FinancialReportDTO>();

            if (accountFilter.ServiceCenterId == 3)
            {
                earnings = await _uow.FinancialReport.GetFinancialReportBreakdownForDemurrage(accountFilter);
            }
            else
            {
                earnings = await _uow.FinancialReport.GetFinancialReportBreakdown(accountFilter);
            }

            return earnings;

        }

        //Get Each Wallet Payment Breakdown
        public async Task<List<WalletPaymentLogView>> GetWalletPaymentLogBreakdown(DashboardFilterCriteria dashboardFilter)
        {
            var result = await _uow.WalletPaymentLog.GetWalletPaymentLogBreakdown(dashboardFilter);

            return result;

        }

        //Get  Financial Breakdown of Outbound Or Inbound Shipments
        //Outbound is 0 and Inbound is 1
        public async Task<List<OutboundFinancialReportDTO>> GetFinancialBreakdownOfOutboundShipments(AccountFilterCriteria accountFilter, int queryType)
        {
            if (accountFilter.CountryId == 0)
            {
                CountryDTO userActiveCountry = null;

                //set user active country from PriviledgeCountrys
                var countries = await _userService.GetPriviledgeCountrys();
                if (countries.Count == 1)
                {
                    userActiveCountry = countries[0];
                }
                else
                {
                    //If UserActive Country is already set in the UserEntity, use that value
                    string currentUserId = await _userService.GetCurrentUserId();
                    var currentUser = await _userService.GetUserById(currentUserId);

                    if (currentUser.UserActiveCountryId > 0)
                    {
                        var userActiveCountryFromEntity = await _countryService.GetCountryById(currentUser.UserActiveCountryId);
                        if (userActiveCountryFromEntity.CurrencySymbol != null)
                        {
                            userActiveCountry = userActiveCountryFromEntity;
                        }
                    }
                }
                accountFilter.CountryId = userActiveCountry.CountryId;
            }

            var earnings = new List<OutboundFinancialReportDTO>();

            earnings = await _uow.FinancialReport.GetFinancialReportOfOutboundShipmentsBreakdown(accountFilter, queryType);

            return earnings;

        }

    }
}
