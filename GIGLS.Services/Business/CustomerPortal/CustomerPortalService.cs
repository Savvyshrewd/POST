﻿using GIGLS.Core.IServices.CustomerPortal;
using System.Collections.Generic;
using System.Threading.Tasks;
using GIGLS.Core.DTO.Account;
using GIGLS.Core.DTO.Shipments;
using GIGLS.Core.DTO.Wallet;
using GIGLS.CORE.DTO.Report;
using GIGLS.Core.IServices.Shipments;
using GIGLS.Core;
using System.Linq;
using AutoMapper;
using GIGLS.Core.IServices.Account;
using GIGLS.Core.IServices.Business;
using GIGLS.Core.IServices.User;
using GIGLS.Core.IServices.Wallet;
using GIGLS.Core.IServices.CashOnDeliveryAccount;
using GIGLS.Core.DTO.PaymentTransactions;
using GIGLS.Core.DTO.Dashboard;
using GIGLS.Infrastructure;
using GIGLS.Core.DTO.Haulage;
using GIGLS.Core.DTO.Zone;
using GIGLS.Core.Domain;
using GIGLS.Core.DTO.ServiceCentres;
using GIGLS.Core.DTO;
using Microsoft.AspNet.Identity;
using GIGLS.Core.IServices.Customers;
using GIGLS.Core.DTO.Customers;
using System;
using GIGLS.CORE.DTO.Shipments;
using GIGLS.Core.DTO.User;

namespace GIGLS.Services.Business.CustomerPortal
{
    public class CustomerPortalService : ICustomerPortalService
    {
        private readonly IUnitOfWork _uow;
        private readonly IShipmentService _shipmentService;
        private readonly IInvoiceService _invoiceService;
        private readonly IShipmentTrackService _iShipmentTrackService;
        private readonly IUserService _userService;
        private readonly IWalletTransactionService _iWalletTransactionService;
        private readonly ICashOnDeliveryAccountService _iCashOnDeliveryAccountService;
        private readonly IPricingService _pricing;
        private readonly ICustomerService _customerService;
        private readonly IPreShipmentService _preShipmentService;
        private readonly IWalletService _walletService;
        private readonly IWalletPaymentLogService _wallepaymenttlogService;


        public CustomerPortalService(IUnitOfWork uow, IShipmentService shipmentService, IInvoiceService invoiceService,
            IShipmentTrackService iShipmentTrackService, IUserService userService, IWalletTransactionService iWalletTransactionService,
            ICashOnDeliveryAccountService iCashOnDeliveryAccountService, IPricingService pricingService,
            ICustomerService customerService, IPreShipmentService preShipmentService, IWalletService walletService, IWalletPaymentLogService wallepaymenttlogService)
        {
            _shipmentService = shipmentService;
            _invoiceService = invoiceService;
            _iShipmentTrackService = iShipmentTrackService;
            _userService = userService;
            _iWalletTransactionService = iWalletTransactionService;
            _iCashOnDeliveryAccountService = iCashOnDeliveryAccountService;
            _pricing = pricingService;
            _customerService = customerService;
            _preShipmentService = preShipmentService;
            _uow = uow;
            _walletService = walletService;
            _wallepaymenttlogService = wallepaymenttlogService;
            MapperConfig.Initialize();
        }


        public async Task<List<InvoiceViewDTO>> GetShipmentTransactions(ShipmentFilterCriteria f_Criteria)
        {
            //get the current login user 
            var currentUserId = await _userService.GetCurrentUserId();
            var currentUser = await _userService.GetUserById(currentUserId);

            var invoices = _uow.Invoice.GetAllFromInvoiceView().Where(s => s.CustomerCode == currentUser.UserChannelCode).ToList();
            invoices = invoices.OrderByDescending(s => s.DateCreated).ToList();

            var invoicesDto = Mapper.Map<List<InvoiceViewDTO>>(invoices);
            return await Task.FromResult(invoicesDto);
        }

        public async Task UpdateWallet(int walletId, WalletTransactionDTO walletTransactionDTO)
        {

            await _walletService.UpdateWallet(walletId, walletTransactionDTO, false);

        }

        public async Task<object> AddWalletPaymentLog(WalletPaymentLogDTO walletPaymentLogDto)
        {
            var walletPaymentLog = await _wallepaymenttlogService.AddWalletPaymentLog(walletPaymentLogDto);
            return walletPaymentLog;

        }

        public async Task<object> UpdateWalletPaymentLog(WalletPaymentLogDTO walletPaymentLogDto)
        {

            //1.check to prevent multiple entries
            //var walletLogObject = _uow.WalletPaymentLog.GetAllAsQueryable().SingleOrDefault(s => 
            //    s.Reference == walletPaymentLogDto.Reference && s.IsWalletCredited == false);
            //if(walletLogObject == null)
            //{
            //    //wallet already updated
            //    return false;
            //}

            //2. update wallet for user
            if (walletPaymentLogDto.TransactionStatus.Equals("success"))
            {

                //Check if transaction exist before updating the wallet
                //to prevent duplicate entry
                var transactionExist =_uow.WalletTransaction.GetAllAsQueryable().SingleOrDefault(s => s.PaymentTypeReference == walletPaymentLogDto.Reference);

                if (transactionExist != null)
                {
                    throw new GenericException("Wallet transaction done already");
                }

                await UpdateWallet(walletPaymentLogDto.WalletId, new WalletTransactionDTO()
                {
                    WalletId = walletPaymentLogDto.WalletId,
                    Amount = walletPaymentLogDto.Amount,
                    Description = walletPaymentLogDto.Description,
                    PaymentTypeReference = walletPaymentLogDto.Reference
                }
             );
            }

            //3. update wallet log
            if (walletPaymentLogDto.TransactionStatus.Equals("success"))
            {
                walletPaymentLogDto.IsWalletCredited = true;
            }
            else
            {
                walletPaymentLogDto.IsWalletCredited = false;
            }

            await _wallepaymenttlogService.UpdateWalletPaymentLog(walletPaymentLogDto.Reference, walletPaymentLogDto);
            return walletPaymentLogDto;

        }

        public async Task<WalletTransactionSummaryDTO> GetWalletTransactions()
        {
            //get the current login user 
            var currentUserId = await _userService.GetCurrentUserId();
            var currentUser = await _userService.GetUserById(currentUserId);
            var wallet = await _uow.Wallet.GetAsync(s => s.CustomerCode == currentUser.UserChannelCode);

            if (wallet == null)
            {
                throw new GenericException("Wallet does not exist");
            }

            var walletTransactionSummary = await _iWalletTransactionService.GetWalletTransactionByWalletId(wallet.WalletId);
            return walletTransactionSummary;
        }

        public async Task<InvoiceDTO> GetInvoiceByWaybill(string waybill)
        {
            var invoice = await _invoiceService.GetInvoiceByWaybill(waybill);
            if (invoice != null)
            {
                var shipmentPreparedBy = await _userService.GetUserById(invoice.Shipment.UserId);
                invoice.Shipment.UserId = shipmentPreparedBy.LastName + " " + shipmentPreparedBy.FirstName;
            }
            return invoice;
        }

        public async Task<IEnumerable<InvoiceViewDTO>> GetInvoices()
        {
            //get the current login user 
            var currentUserId = await _userService.GetCurrentUserId();
            var currentUser = await _userService.GetUserById(currentUserId);

            var invoices = _uow.Invoice.GetAllFromInvoiceView().Where(s => s.CustomerCode == currentUser.UserChannelCode).ToList();
            invoices = invoices.OrderByDescending(s => s.DateCreated).ToList();

            var invoicesDto = Mapper.Map<List<InvoiceViewDTO>>(invoices);
            return invoicesDto;
        }

        public async Task<IEnumerable<ShipmentTrackingDTO>> TrackShipment(string waybillNumber)
        {
            //1. Verify the waybill is attached to the login user
            var currentUserId = await _userService.GetCurrentUserId();
            var currentUser = await _userService.GetUserById(currentUserId);

            var invoices =
                _uow.Invoice.GetAllFromInvoiceView().Where(s =>
                s.CustomerCode == currentUser.UserChannelCode && s.Waybill == waybillNumber).ToList();

            if (invoices.Count > 0)
            {
                var result = await _iShipmentTrackService.TrackShipment(waybillNumber);
                return result;
            }
            else
            {
                throw new GenericException("Error: You cannot track this waybill number.");
            }
        }

        public async Task<IEnumerable<ShipmentTrackingDTO>> PublicTrackShipment(string waybillNumber)
        {
            var result = await _iShipmentTrackService.TrackShipment(waybillNumber);
            return result;
        }

        public async Task<CashOnDeliveryAccountSummaryDTO> GetCashOnDeliveryAccount()
        {
            //get the current login user 
            var currentUserId = await _userService.GetCurrentUserId();
            var currentUser = await _userService.GetUserById(currentUserId);
            var wallet = await _uow.Wallet.GetAsync(s => s.CustomerCode == currentUser.UserChannelCode);

            var result = await _iCashOnDeliveryAccountService.GetCashOnDeliveryAccountByWallet(wallet.WalletNumber);
            return result;
        }

        public async Task<IEnumerable<PaymentPartialTransactionDTO>> GetPartialPaymentTransaction(string waybill)
        {
            var transaction = await _uow.PaymentPartialTransaction.FindAsync(x => x.Waybill.Equals(waybill));
            return Mapper.Map<IEnumerable<PaymentPartialTransactionDTO>>(transaction);
        }

        public async Task<DashboardDTO> GetDashboard()
        {
            var dashboardDTO = new DashboardDTO();

            //get the current login user 
            var currentUserId = await _userService.GetCurrentUserId();
            var currentUser = await _userService.GetUserById(currentUserId);
            var wallet = await _uow.Wallet.GetAsync(s => s.CustomerCode == currentUser.UserChannelCode);

            if (wallet != null)
            {
                var invoices = _uow.Invoice.GetAllFromInvoiceView().Where(s => s.CustomerCode == currentUser.UserChannelCode).ToList();
                var invoicesDto = Mapper.Map<List<InvoiceViewDTO>>(invoices);

                // 
                dashboardDTO.TotalShipmentOrdered = invoices.Count();
                dashboardDTO.WalletBalance = wallet.Balance;
            }

            return await Task.FromResult(dashboardDTO);
        }

        public async Task<IEnumerable<StateDTO>> GetStates(int pageSize, int page)
        {
            var states = await _uow.State.GetStatesAsync(pageSize, page);
            return states.OrderBy(x => x.StateName).ToList();
        }

        public int GetStatesTotal()
        {
            var states = _uow.State.GetStatesTotal();
            return states;
        }

        public async Task<List<ServiceCentreDTO>> GetLocalServiceCentres()
        {
            return await _uow.ServiceCentre.GetLocalServiceCentres();
        }

        public async Task<IEnumerable<DeliveryOptionDTO>> GetDeliveryOptions()
        {
            return await _uow.DeliveryOption.GetDeliveryOptions();
        }

        public Task<IEnumerable<SpecialDomesticPackageDTO>> GetSpecialDomesticPackages()
        {
            return Task.FromResult(Mapper.Map<IEnumerable<SpecialDomesticPackage>, IEnumerable<SpecialDomesticPackageDTO>>(_uow.SpecialDomesticPackage.GetAll()));
        }

        public async Task<IEnumerable<HaulageDTO>> GetHaulages()
        {
            var haulages = await _uow.Haulage.GetHaulagesAsync();
            return haulages;
        }

        public async Task<IEnumerable<InsuranceDTO>> GetInsurances()
        {
            var insurances = await _uow.Insurance.GetInsurancesAsync();
            return insurances;
        }

        public async Task<IEnumerable<VATDTO>> GetVATs()
        {
            var vats = await _uow.VAT.GetVATsAsync();
            return vats;
        }

        public async Task<DomesticRouteZoneMapDTO> GetZone(int departure, int destination)
        {
            // get serviceCenters
            var departureServiceCenter = _uow.ServiceCentre.Get(departure);
            var destinationServiceCenter = _uow.ServiceCentre.Get(destination);

            // use Stations
            var routeZoneMap = await _uow.DomesticRouteZoneMap.GetAsync(r =>
                r.DepartureId == departureServiceCenter.StationId &&
                r.DestinationId == destinationServiceCenter.StationId, "Zone,Destination,Departure");

            if (routeZoneMap == null)
                throw new GenericException("The Mapping of Route to Zone does not exist");

            return Mapper.Map<DomesticRouteZoneMapDTO>(routeZoneMap);
        }

        public async Task<decimal> GetPrice(PricingDTO pricingDto)
        {
            return await _pricing.GetPrice(pricingDto);
        }

        public async Task<decimal> GetHaulagePrice(HaulagePricingDTO haulagePricingDto)
        {
            return await _pricing.GetHaulagePrice(haulagePricingDto);
        }

        public async Task<CustomerDTO> GetCustomer(string userId)
        {
            var user = await _userService.GetUserById(userId);
            var customer = await _customerService.GetCustomer(user.UserChannelCode, user.UserChannelType);
            return customer;
        }

        public async Task<IdentityResult> ChangePassword(string userid, string currentPassword, string newPassword)
        {
            return await _userService.ChangePassword(userid, currentPassword, newPassword);
        }

        public async Task<List<PreShipmentDTO>> GetPreShipments(FilterOptionsDto filterOptionsDto)
        {
            try
            {
                //get the current login user 
                var currentUserId = await _userService.GetCurrentUserId();

                var preShipmentsQuery = _uow.PreShipment.PreShipmentsAsQueryable();
                preShipmentsQuery = preShipmentsQuery.Where(s => s.UserId == currentUserId);
                var preShipments = preShipmentsQuery.ToList();
                var preShipmentsDTO = Mapper.Map<List<PreShipmentDTO>>(preShipments);
                return preShipmentsDTO;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<PreShipmentDTO> GetPreShipment(string waybill)
        {
            try
            {
                var preShipmentDTO = await _preShipmentService.GetPreShipment(waybill);
                return preShipmentDTO;
            }
            catch (Exception)
            {
                throw;
            }

        }

        public Task<UserDTO> Register(UserDTO user)
        {
            throw new NotImplementedException();
        }
    }
}
