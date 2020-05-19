﻿using GIGLS.Core.IServices.Shipments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GIGLS.Core.DTO.Shipments;
using GIGLS.Core;
using GIGLS.Core.IServices.Zone;
using GIGLS.Core.IServices.Utility;
using GIGLS.Core.Domain;
using AutoMapper;
using GIGLS.Core.Enums;
using GIGLS.Core.IServices.Business;
using GIGLS.Core.DTO.PaymentTransactions;
using GIGLS.Core.IServices.Wallet;
using GIGLS.Infrastructure;
using GIGLS.Core.DTO.Wallet;
using GIGLS.CORE.DTO.Report;
using GIGLS.Core.IServices.User;
using GIGLS.Core.DTO.Zone;
using GIGLS.Core.DTO;
using GIGLS.Core.IServices;
using GIGLS.Core.IServices.Partnership;
using GIGLS.Core.DTO.Partnership;
using GIGLS.Core.IMessageService;
using GIGLS.Core.DTO.Customers;
using GIGL.GIGLS.Core.Domain;
using GIGLS.Core.Domain.Partnership;
using GIGLS.Core.DTO.User;
using VehicleType = GIGLS.Core.Domain.VehicleType;
using Hangfire;
using GIGLS.Core.IServices.Customers;
using GIGLS.Core.DTO.Utility;
using Audit.Core;
using System.Configuration;

namespace GIGLS.Services.Implementation.Shipments
{
    public class PreShipmentMobileService : IPreShipmentMobileService
    {
        private readonly IUnitOfWork _uow;
        private readonly IShipmentService _shipmentService;
        private readonly INumberGeneratorMonitorService _numberGeneratorMonitorService;
        private readonly IPricingService _pricingService;
        private readonly IWalletService _walletService;
        private readonly IWalletTransactionService _walletTransactionService;
        private readonly IUserService _userService;
        private readonly IMobileShipmentTrackingService _mobiletrackingservice;
        private readonly IMobilePickUpRequestsService _mobilepickuprequestservice;
        private readonly IDomesticRouteZoneMapService _domesticroutezonemapservice;
        private readonly ICategoryService _categoryservice;
        private readonly ISubCategoryService _subcategoryservice;
        private readonly IPartnerTransactionsService _partnertransactionservice;
        private readonly IGlobalPropertyService _globalPropertyService;
        private readonly IMessageSenderService _messageSenderService;
        private readonly IHaulageService _haulageService;
        private readonly IHaulageDistanceMappingService _haulageDistanceMappingService;
        private readonly IPartnerService _partnerService;
        private readonly ICustomerService _customerService;
        private readonly IGiglgoStationService _giglgoStationService;
        private readonly IGroupWaybillNumberService _groupWaybillNumberService;

        public PreShipmentMobileService(IUnitOfWork uow, IShipmentService shipmentService, INumberGeneratorMonitorService numberGeneratorMonitorService,
            IPricingService pricingService, IWalletService walletService, IWalletTransactionService walletTransactionService,
            IUserService userService, IMobileShipmentTrackingService mobiletrackingservice,
            IMobilePickUpRequestsService mobilepickuprequestservice, IDomesticRouteZoneMapService domesticroutezonemapservice, ICategoryService categoryservice, ISubCategoryService subcategoryservice,
            IPartnerTransactionsService partnertransactionservice, IGlobalPropertyService globalPropertyService, IMessageSenderService messageSenderService,
            IHaulageService haulageService, IHaulageDistanceMappingService haulageDistanceMappingService, IPartnerService partnerService, ICustomerService customerService,
            IGiglgoStationService giglgoStationService, IGroupWaybillNumberService groupWaybillNumberService)
        {
            _uow = uow;
            _shipmentService = shipmentService;
            _numberGeneratorMonitorService = numberGeneratorMonitorService;
            _pricingService = pricingService;
            _walletService = walletService;
            _walletTransactionService = walletTransactionService;
            _userService = userService;
            _mobiletrackingservice = mobiletrackingservice;
            _mobilepickuprequestservice = mobilepickuprequestservice;
            _domesticroutezonemapservice = domesticroutezonemapservice;
            _subcategoryservice = subcategoryservice;
            _categoryservice = categoryservice;
            _partnertransactionservice = partnertransactionservice;
            _globalPropertyService = globalPropertyService;
            _messageSenderService = messageSenderService;
            _haulageService = haulageService;
            _haulageDistanceMappingService = haulageDistanceMappingService;
            _partnerService = partnerService;
            _customerService = customerService;
            _giglgoStationService = giglgoStationService;
            _groupWaybillNumberService = groupWaybillNumberService;
            MapperConfig.Initialize();
        }

        public async Task<object> AddPreShipmentMobile(PreShipmentMobileDTO preShipment)
        {
            try
            {
                //null DateCreated
                preShipment.DateCreated = DateTime.Now;
                var zoneid = await _domesticroutezonemapservice.GetZoneMobile(preShipment.SenderStationId, preShipment.ReceiverStationId);
                preShipment.ZoneMapping = zoneid.ZoneId;
                var newPreShipment = await CreatePreShipment(preShipment);
                //await _uow.CompleteAsync();
                bool IsBalanceSufficient = true;
                string message = "Shipment created successfully";
                if (newPreShipment.IsBalanceSufficient == false)
                {
                    message = "Insufficient Wallet Balance";
                    IsBalanceSufficient = false;
                }
                else if (newPreShipment.IsEligible == false)
                {
                    var carPickUprice = new GlobalPropertyDTO();
                    if (newPreShipment.IsCodNeeded == false)
                    {
                        carPickUprice = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.EcommerceNoCodAmount, newPreShipment.CountryId);
                    }
                    else
                    {
                        carPickUprice = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.EcommerceCodAmount, newPreShipment.CountryId);
                    }
                    var remainingamount = (Convert.ToDecimal(carPickUprice.Value) - newPreShipment.CurrentWalletAmount);
                    message = $"You are not yet eligible to create shipment on the Platform. You need a minimum balance of {preShipment.CurrencySymbol}{carPickUprice.Value}, please fund your wallet with additional {preShipment.CurrencySymbol}{remainingamount} to complete the process. Thank you";
                    newPreShipment.Waybill = "";

                    throw new GenericException(message);
                }
                return new { waybill = newPreShipment.Waybill, message = message, IsBalanceSufficient, Zone = zoneid.ZoneId };
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task SendSMSForMobileShipmentCreation(PreShipmentMobileDTO preShipmentMobile)
        {
            var smsMessageExtensionDTO = new MobileShipmentCreationMessageDTO()
            {
                SenderName = preShipmentMobile.SenderName,
                WaybillNumber = preShipmentMobile.Waybill,
                SenderPhoneNumber = preShipmentMobile.SenderPhoneNumber
            };

            await _messageSenderService.SendMessage(MessageType.MCS, EmailSmsType.SMS, smsMessageExtensionDTO);
            //await SendSMSForShipmentDeliveryNotification(preShipmentMobile);
        }

        private async Task SendSMSForShipmentDeliveryNotification(PreShipmentMobileDTO preShipmentMobile)
        {
            string stationList = ConfigurationManager.AppSettings["stationDelayList"];

            //get station list from web config
            int[] delayDeliveryStation = stationList.Split(',').Select(int.Parse).ToArray();

            //send notification message for delay shipment
            if (delayDeliveryStation.Contains(preShipmentMobile.ReceiverStationId))
            {
                var station = await _uow.Station.GetAsync(x => x.StationId == preShipmentMobile.ReceiverStationId);

                var smsMessageExtensionDTO = new ShipmentDeliveryDelayMessageDTO()
                {
                    SenderName = preShipmentMobile.SenderName,
                    WaybillNumber = preShipmentMobile.Waybill,
                    SenderPhoneNumber = preShipmentMobile.SenderPhoneNumber,
                    StationName = station.StationName
                };

                await _messageSenderService.SendMessage(MessageType.DLD, EmailSmsType.SMS, smsMessageExtensionDTO);
            }
        }

        private async Task SendSMSForMultipleMobileShipmentCreation(PreShipmentMobileDTO preShipmentMobile, MultipleShipmentOutput shipmentOutput)
        {
            var waybills = shipmentOutput.Waybills.Select(a => a.Waybill).ToList();

            var smsMessageExtensionDTO = new MobileShipmentCreationMessageDTO()
            {
                SenderName = preShipmentMobile.SenderName,
                WaybillNumber = string.Join(",", waybills),
                SenderPhoneNumber = preShipmentMobile.SenderPhoneNumber,
                GroupCode = shipmentOutput.groupCodeNumber
            };

            await _messageSenderService.SendMessage(MessageType.MMCS, EmailSmsType.SMS, smsMessageExtensionDTO);
        }

        //Multiple Shipment New Flow NEW
        public async Task<MultipleShipmentOutput> CreateMobileShipment(NewPreShipmentMobileDTO newPreShipment)
        {
            var listOfPreShipment = await GroupMobileShipmentByReceiver(newPreShipment);

            MultipleShipmentOutput waybillList = new MultipleShipmentOutput();

            if (listOfPreShipment[0].IsEligible == true)
            {
                //to check if the customer have enough money in his/her wallet
                decimal shipmentTotal = 0;
                foreach (var item in listOfPreShipment)
                {
                    shipmentTotal = shipmentTotal + (decimal)item.CalculatedTotal;
                }

                //Get Pick UP price
                var Pickuprice = await GetPickUpPriceForMultipleShipment(listOfPreShipment[0].CustomerType, listOfPreShipment[0].VehicleType, listOfPreShipment[0].CountryId);
                var PickupValue = Convert.ToDecimal(Pickuprice);

                shipmentTotal = shipmentTotal + PickupValue;

                //Get the customer wallet balance
                var wallet = await _walletService.GetWalletBalance(listOfPreShipment[0].CustomerCode);

                if (wallet.Balance > shipmentTotal)
                {
                    waybillList = await GenerateWaybill(listOfPreShipment, PickupValue);
                    //var waybills = waybillList.Waybills.Select(a => a.Waybill).ToList();
                    //await SendSMSForMultipleMobileShipmentCreation(listOfPreShipment[0], waybills);
                    await SendSMSForMultipleMobileShipmentCreation(listOfPreShipment[0], waybillList);
                }
                else
                {
                    throw new GenericException("Insufficient Balance");
                }
            }
            else
            {
                throw new GenericException("You are not eligible to create shipment.");
            }

            return waybillList;
        }

        //Also for Get Price
        public async Task<List<PreShipmentMobileDTO>> GroupMobileShipmentByReceiver(NewPreShipmentMobileDTO newPreShipment)
        {
            var listOfPreShipment = new List<PreShipmentMobileDTO>();

            //check for sender information for validation
            if (string.IsNullOrEmpty(newPreShipment.VehicleType))
            {
                throw new GenericException("Please select a vehicle type");
            }

            if (!newPreShipment.Receivers.Any())
            {
                throw new GenericException("No Receiver was added");
            }

            newPreShipment = await ExtractSenderInfo(newPreShipment);

            int numOfItems = 0;
            var maxNumOfShipment = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.GiglgoMaxNumShipment, newPreShipment.CountryId);
            if (maxNumOfShipment == null)
            {
                throw new GenericException("Maximum Number of Shipment on Global Property is not set");
            }

            int maximumShipmentItemsAllow = Convert.ToInt32(maxNumOfShipment.Value);

            foreach (var item in newPreShipment.Receivers)
            {
                if (!item.preShipmentItems.Any())
                {
                    throw new GenericException("No shipment was added");
                }

                var newShipment = new PreShipmentMobileDTO
                {
                    PreShipmentItems = new List<PreShipmentItemMobileDTO>()
                };

                foreach (var i in item.preShipmentItems)
                {
                    if (i.Quantity == 0)
                    {
                        throw new GenericException($"Quantity cannot be zero for {i.ItemName}");
                    }
                    newShipment.PreShipmentItems.Add(i);

                    numOfItems++;
                }

                if (numOfItems > maximumShipmentItemsAllow)
                {
                    throw new GenericException($"Total number of Shipment items can not exceed {maxNumOfShipment.Value}");
                }

                newShipment.ReceiverAddress = item.ReceiverAddress;
                newShipment.ReceiverStationId = item.ReceiverStationId;
                newShipment.ReceiverLocation = item.ReceiverLocation;
                newShipment.ReceiverName = item.ReceiverName;
                newShipment.ReceiverPhoneNumber = item.ReceiverPhoneNumber;

                newShipment.SenderName = newPreShipment.SenderName;
                newShipment.SenderPhoneNumber = newPreShipment.SenderPhoneNumber;
                newShipment.SenderLocation = newPreShipment.SenderLocation;
                newShipment.SenderAddress = newPreShipment.SenderAddress;
                newShipment.SenderStationId = newPreShipment.SenderStationId;
                newShipment.CustomerCode = newPreShipment.CustomerCode;
                newShipment.UserId = newPreShipment.UserId;
                newShipment.VehicleType = newPreShipment.VehicleType;
                newShipment.CountryName = newPreShipment.CountryName;
                newShipment.Haulageid = newPreShipment.Haulageid;
                newShipment.CustomerType = newPreShipment.CustomerType;
                newShipment.CountryId = newPreShipment.CountryId;
                newShipment.IsEligible = newPreShipment.IsEligible;
                newShipment.CurrencyCode = newPreShipment.CurrencyCode;
                newShipment.CurrencySymbol = newPreShipment.CurrencySymbol;

                //Get Prices
                var getPriceAndAll = await GetPriceFromList(newShipment);
                listOfPreShipment.Add(getPriceAndAll);
            }

            return listOfPreShipment;
        }

        //Get the prices and others for each of them
        private async Task<PreShipmentMobileDTO> GetPriceFromList(PreShipmentMobileDTO preShipmentMobile)
        {
            try
            {
                preShipmentMobile.DateCreated = DateTime.Now;
                var zoneId = await _domesticroutezonemapservice.GetZoneMobile(preShipmentMobile.SenderStationId, preShipmentMobile.ReceiverStationId);
                preShipmentMobile.ZoneMapping = zoneId.ZoneId;
                var price = await GetPriceForPreShipment(preShipmentMobile);
                return price;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<PreShipmentMobileDTO> CreatePreShipment(PreShipmentMobileDTO preShipmentDTO)
        {
            try
            {
                if (string.IsNullOrEmpty(preShipmentDTO.VehicleType))
                {
                    throw new GenericException("Please select a vehicle type");
                }

                if (!preShipmentDTO.PreShipmentItems.Any())
                {
                    throw new GenericException("Shipment Items cannot be empty");
                }

                var PreshipmentPriceDTO = new MobilePriceDTO();

                // get the current user info
                var currentUserId = await _userService.GetCurrentUserId();
                preShipmentDTO.UserId = currentUserId;
                var user = await _userService.GetUserById(currentUserId);

                var Country = await _uow.Country.GetCountryByStationId(preShipmentDTO.SenderStationId);
                preShipmentDTO.CountryId = Country.CountryId;

                var customer = await _uow.Company.GetAsync(s => s.CustomerCode == user.UserChannelCode);
                if (customer != null)
                {
                    if (customer.IsEligible != true)
                    {
                        preShipmentDTO.IsEligible = false;
                        preShipmentDTO.IsCodNeeded = customer.isCodNeeded;
                        preShipmentDTO.CurrencySymbol = Country.CurrencySymbol;
                        preShipmentDTO.CurrentWalletAmount = (decimal)customer.WalletAmount;
                        return preShipmentDTO;
                    }
                }

                if (preShipmentDTO.VehicleType.ToLower() == Vehicletype.Truck.ToString().ToLower())
                {
                    PreshipmentPriceDTO = await GetHaulagePrice(new HaulagePriceDTO
                    {
                        Haulageid = (int)preShipmentDTO.Haulageid,
                        DepartureStationId = preShipmentDTO.SenderStationId,
                        DestinationStationId = preShipmentDTO.ReceiverStationId
                    });
                    preShipmentDTO.GrandTotal = (decimal)PreshipmentPriceDTO.GrandTotal;
                    if (preShipmentDTO.PreShipmentItems.Any())
                    {
                        foreach (var shipment in preShipmentDTO.PreShipmentItems)
                        {
                            shipment.CalculatedPrice = PreshipmentPriceDTO.GrandTotal;
                        }
                    }
                }
                else
                {
                    //var exists = await _uow.Company.ExistAsync(s => s.CustomerCode == user.UserChannelCode);
                    if (user.UserChannelType == UserChannelType.Ecommerce || customer != null)
                    {
                        preShipmentDTO.Shipmentype = ShipmentType.Ecommerce;
                    }
                    preShipmentDTO.IsFromShipment = true;
                    PreshipmentPriceDTO = await GetPrice(preShipmentDTO);
                }

                decimal shipmentGrandTotal = (decimal)PreshipmentPriceDTO.GrandTotal;
                var wallet = await _walletService.GetWalletBalance();

                if (wallet.Balance >= shipmentGrandTotal)
                {
                    var gigGOServiceCenter = await _userService.GetGIGGOServiceCentre();

                    //generate waybill
                    var waybill = await _numberGeneratorMonitorService.GenerateNextNumber(NumberGeneratorType.WaybillNumber, gigGOServiceCenter.Code);
                    preShipmentDTO.Waybill = waybill;

                    var newPreShipment = Mapper.Map<PreShipmentMobile>(preShipmentDTO);

                    if (user.UserChannelType == UserChannelType.Ecommerce)
                    {
                        newPreShipment.CustomerType = CustomerType.Company.ToString();
                        newPreShipment.CompanyType = CompanyType.Ecommerce.ToString();
                    }
                    else
                    {
                        newPreShipment.CustomerType = "Individual";
                        newPreShipment.CompanyType = CustomerType.IndividualCustomer.ToString();
                    }
                    newPreShipment.IsConfirmed = false;
                    newPreShipment.IsDelivered = false;
                    newPreShipment.shipmentstatus = "Shipment created";
                    newPreShipment.DateCreated = DateTime.Now;
                    newPreShipment.GrandTotal = shipmentGrandTotal;
                    preShipmentDTO.IsBalanceSufficient = true;
                    preShipmentDTO.DiscountValue = PreshipmentPriceDTO.Discount;
                    newPreShipment.ShipmentPickupPrice = (decimal)(PreshipmentPriceDTO.PickUpCharge == null ? 0.0M : PreshipmentPriceDTO.PickUpCharge);
                    _uow.PreShipmentMobile.Add(newPreShipment);

                    //process payment
                    var transaction = new WalletTransactionDTO
                    {
                        WalletId = wallet.WalletId,
                        CreditDebitType = CreditDebitType.Debit,
                        Amount = newPreShipment.GrandTotal,
                        ServiceCentreId = gigGOServiceCenter.ServiceCentreId,
                        Waybill = waybill,
                        Description = "Payment for Shipment",
                        PaymentType = PaymentType.Online,
                        UserId = newPreShipment.UserId
                    };

                    //update wallet
                    var updatedwallet = await _uow.Wallet.GetAsync(wallet.WalletId);

                    //double check in case something is wrong with the server before complete the transaction
                    if (updatedwallet.Balance < shipmentGrandTotal)
                    {
                        preShipmentDTO.IsBalanceSufficient = false;
                        return preShipmentDTO;
                    }

                    decimal price = updatedwallet.Balance - shipmentGrandTotal;
                    updatedwallet.Balance = price;
                    var walletTransaction = await _walletTransactionService.AddWalletTransaction(transaction);

                    await _uow.CompleteAsync();

                    await ScanMobileShipment(new ScanDTO
                    {
                        WaybillNumber = newPreShipment.Waybill,
                        ShipmentScanStatus = ShipmentScanStatus.MCRT
                    });

                    await SendSMSForMobileShipmentCreation(preShipmentDTO);
                    return preShipmentDTO;
                }
                else
                {
                    preShipmentDTO.IsBalanceSufficient = false;
                    return preShipmentDTO;
                }
            }
            catch
            {
                throw;
            }
        }

        //Extract Sender's Information
        private async Task<NewPreShipmentMobileDTO> ExtractSenderInfo(NewPreShipmentMobileDTO preShipmentMobileDTO)
        {
            // get the current user info
            var currentUserId = await _userService.GetCurrentUserId();
            preShipmentMobileDTO.UserId = currentUserId;
            var user = await _userService.GetUserById(currentUserId);

            var Country = await _uow.Country.GetCountryByStationId(preShipmentMobileDTO.SenderStationId);
            preShipmentMobileDTO.CountryId = Country.CountryId;
            preShipmentMobileDTO.CurrencyCode = Country.CurrencyCode;
            preShipmentMobileDTO.CurrencySymbol = Country.CurrencySymbol;
            preShipmentMobileDTO.CustomerType = user.UserChannelType.ToString();
            preShipmentMobileDTO.CustomerCode = user.UserChannelCode;
            preShipmentMobileDTO.IsEligible = true;

            var customer = await _uow.Company.GetAsync(s => s.CustomerCode == user.UserChannelCode);
            if (customer != null)
            {
                if (customer.IsEligible != true)
                {
                    preShipmentMobileDTO.IsEligible = false;
                    preShipmentMobileDTO.IsCodNeeded = customer.isCodNeeded;
                    preShipmentMobileDTO.CurrencySymbol = Country.CurrencySymbol;
                    preShipmentMobileDTO.CurrentWalletAmount = (decimal)customer.WalletAmount;
                }
            }

            return preShipmentMobileDTO;
        }

        //Multiple Shipments NEW
        private async Task<PreShipmentMobileDTO> GetPriceForPreShipment(PreShipmentMobileDTO preShipmentDTO)
        {
            try
            {
                var PreshipmentPriceDTO = new MobilePriceDTO();

                if (preShipmentDTO.VehicleType.ToLower() == Vehicletype.Truck.ToString().ToLower())
                {
                    preShipmentDTO = await GetHaulagePrice(preShipmentDTO);
                }
                else
                {
                    if (preShipmentDTO.CustomerType == UserChannelType.Ecommerce.ToString())
                    {
                        preShipmentDTO.Shipmentype = ShipmentType.Ecommerce;
                    }
                    preShipmentDTO.IsFromShipment = true;
                    PreshipmentPriceDTO = await GetPriceForNonHaulage(preShipmentDTO);

                    preShipmentDTO.CalculatedTotal = (double)PreshipmentPriceDTO.MainCharge;
                    preShipmentDTO.GrandTotal = (decimal)PreshipmentPriceDTO.GrandTotal;
                    preShipmentDTO.DiscountValue = PreshipmentPriceDTO.Discount;
                    preShipmentDTO.InsuranceValue = PreshipmentPriceDTO.InsuranceValue;
                }
                return preShipmentDTO;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //Get Haulage Price new
        private async Task<PreShipmentMobileDTO> GetHaulagePrice(PreShipmentMobileDTO preShipmentDTO)
        {
            var PreshipmentPriceDTO = new MobilePriceDTO();

            PreshipmentPriceDTO = await GetHaulagePrice(new HaulagePriceDTO
            {
                Haulageid = (int)preShipmentDTO.Haulageid,
                DepartureStationId = preShipmentDTO.SenderStationId,
                DestinationStationId = preShipmentDTO.ReceiverStationId
            });

            preShipmentDTO.GrandTotal = (decimal)PreshipmentPriceDTO.GrandTotal;

            if (preShipmentDTO.PreShipmentItems.Count() > 0)
            {
                foreach (var shipment in preShipmentDTO.PreShipmentItems)
                {
                    shipment.CalculatedPrice = PreshipmentPriceDTO.GrandTotal;
                }
            }
            return preShipmentDTO;
        }

        //Generate Waybill for Multiple Shipments Flow NEW
        private async Task<MultipleShipmentOutput> GenerateWaybill(List<PreShipmentMobileDTO> preShipmentDTO, decimal pickupValue)
        {
            var gigGOServiceCenter = await _userService.GetGIGGOServiceCentre();
            var numberOfReceiver = preShipmentDTO.Count;
            var individualPickupPrice = pickupValue / numberOfReceiver;

            HashSet<MultipleShipmentResult> waybillList = new HashSet<MultipleShipmentResult>(new MultipleShipmentResultComparer());

            foreach (var receiver in preShipmentDTO)
            {
                var wallet = await _walletService.GetWalletBalance(receiver.CustomerCode);

                receiver.GrandTotal = receiver.GrandTotal + individualPickupPrice;

                var price = (wallet.Balance - Convert.ToDecimal(receiver.GrandTotal));

                //generate waybill
                var waybill = await _numberGeneratorMonitorService.GenerateNextNumber(NumberGeneratorType.WaybillNumber, gigGOServiceCenter.Code);
                receiver.Waybill = waybill;
                var newPreShipment = Mapper.Map<PreShipmentMobile>(receiver);

                if (receiver.CustomerType == UserChannelType.Ecommerce.ToString())
                {
                    newPreShipment.CustomerType = CustomerType.Company.ToString();
                    newPreShipment.CompanyType = CompanyType.Ecommerce.ToString();
                }
                else
                {
                    newPreShipment.CustomerType = "Individual";
                    newPreShipment.CompanyType = CustomerType.IndividualCustomer.ToString();
                }

                newPreShipment.IsConfirmed = false;
                newPreShipment.IsDelivered = false;
                newPreShipment.shipmentstatus = "Shipment created";
                newPreShipment.DateCreated = DateTime.Now;
                newPreShipment.GrandTotal = receiver.GrandTotal;
                newPreShipment.CalculatedTotal = receiver.CalculatedTotal;
                receiver.IsBalanceSufficient = true;
                newPreShipment.DiscountValue = receiver.DiscountValue;
                newPreShipment.ShipmentPickupPrice = individualPickupPrice;
                _uow.PreShipmentMobile.Add(newPreShipment);
                var transaction = new WalletTransactionDTO
                {
                    WalletId = wallet.WalletId,
                    CreditDebitType = CreditDebitType.Debit,
                    Amount = newPreShipment.GrandTotal,
                    ServiceCentreId = gigGOServiceCenter.ServiceCentreId,
                    Waybill = waybill,
                    Description = "Payment for Shipment",
                    PaymentType = PaymentType.Online,
                    UserId = newPreShipment.UserId
                };
                var walletTransaction = await _walletTransactionService.AddWalletTransaction(transaction);
                var updatedwallet = await _uow.Wallet.GetAsync(wallet.WalletId);
                updatedwallet.Balance = price;
                await _uow.CompleteAsync();

                await ScanMobileShipment(new ScanDTO
                {
                    WaybillNumber = newPreShipment.Waybill,
                    ShipmentScanStatus = ShipmentScanStatus.MCRT
                });

                waybillList.Add(new MultipleShipmentResult() { Waybill = newPreShipment.Waybill, ZoneMapping = (int)newPreShipment.ZoneMapping });
            }

            var groupCode = await MappingWaybillNumbersToGroupCode(gigGOServiceCenter.Code, waybillList);
            return groupCode;
        }

        //map waybillNumber to groupCode
        private async Task<MultipleShipmentOutput> MappingWaybillNumbersToGroupCode(string serviceCenterCode, HashSet<MultipleShipmentResult> waybillNumberList)
        {
            try
            {
                var groupCode = await _groupWaybillNumberService.GenerateGroupWaybillNumber(serviceCenterCode);

                var groupCodeNumberMapping = new List<MobileGroupCodeWaybillMapping>();

                //Get All Waybills that need to be grouped
                foreach (var waybill in waybillNumberList)
                {
                    //Add new Mapping
                    var newMapping = new MobileGroupCodeWaybillMapping
                    {
                        GroupCodeNumber = groupCode,
                        WaybillNumber = waybill.Waybill,
                        DateMapped = DateTime.Now,
                    };
                    groupCodeNumberMapping.Add(newMapping);
                }
                _uow.MobileGroupCodeWaybillMapping.AddRange(groupCodeNumberMapping);
                _uow.Complete();

                var result = new MultipleShipmentOutput
                {
                    groupCodeNumber = groupCode,
                    Waybills = waybillNumberList
                };
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<MobilePriceDTO> GetPrice(PreShipmentMobileDTO preShipment)
        {
            try
            {
                if (!preShipment.PreShipmentItems.Any())
                {
                    throw new GenericException("No Preshipitem was added");
                }

                var zoneid = await _domesticroutezonemapservice.GetZoneMobile(preShipment.SenderStationId, preShipment.ReceiverStationId);

                var Country = await _uow.Country.GetCountryByStationId(preShipment.SenderStationId);
                preShipment.CountryId = Country.CountryId;

                //change the quantity of the preshipmentItem if it fall into promo category
                preShipment = await ChangePreshipmentItemQuantity(preShipment, zoneid.ZoneId);

                var Price = 0.0M;
                var amount = 0.0M;
                var IndividualPrice = 0.0M;
                decimal DeclaredValue = 0.0M;

                //undo comment when App is updated
                if (zoneid.ZoneId == 1 && preShipment.ReceiverLocation != null && preShipment.SenderLocation != null)
                {
                    if (preShipment.ReceiverLocation.Latitude != null && preShipment.SenderLocation.Latitude != null)
                    {
                        int ShipmentCount = preShipment.PreShipmentItems.Count;

                        amount = await CalculateGeoDetailsBasedonLocation(preShipment);
                        IndividualPrice = (amount / ShipmentCount);
                    }
                }

                //Get the customer Type
                var userChannelCode = await _userService.GetUserChannelCode();
                var userChannel = await _uow.Company.GetAsync(x => x.CustomerCode == userChannelCode);

                if (userChannel != null)
                {
                    preShipment.Shipmentype = ShipmentType.Ecommerce;
                }

                foreach (var preShipmentItem in preShipment.PreShipmentItems)
                {
                    if (preShipmentItem.Quantity == 0)
                    {
                        throw new GenericException("Quantity cannot be zero");
                    }

                    var PriceDTO = new PricingDTO
                    {
                        DepartureStationId = preShipment.SenderStationId,
                        DestinationStationId = preShipment.ReceiverStationId,
                        Weight = preShipmentItem.Weight,
                        SpecialPackageId = (int)preShipmentItem.SpecialPackageId,
                        ShipmentType = preShipmentItem.ShipmentType,
                        CountryId = preShipment.CountryId  //Nigeria
                    };

                    if (preShipmentItem.ShipmentType == ShipmentType.Special)
                    {
                        if (preShipment.Shipmentype == ShipmentType.Ecommerce)
                        {
                            PriceDTO.DeliveryOptionId = 4;
                        }

                        preShipmentItem.CalculatedPrice = await _pricingService.GetMobileSpecialPrice(PriceDTO);
                        preShipmentItem.CalculatedPrice = preShipmentItem.CalculatedPrice * preShipmentItem.Quantity;
                        preShipmentItem.CalculatedPrice = preShipmentItem.CalculatedPrice + IndividualPrice;
                    }
                    else if (preShipmentItem.ShipmentType == ShipmentType.Regular)
                    {
                        if (preShipmentItem.Weight == 0)
                        {
                            throw new GenericException("Item weight cannot be zero");
                        }

                        if (preShipment.Shipmentype == ShipmentType.Ecommerce)
                        {
                            preShipmentItem.CalculatedPrice = await _pricingService.GetMobileEcommercePrice(PriceDTO);
                            preShipmentItem.CalculatedPrice = preShipmentItem.CalculatedPrice * preShipmentItem.Quantity;
                            preShipmentItem.CalculatedPrice = preShipmentItem.CalculatedPrice + IndividualPrice;
                        }
                        else
                        {
                            preShipmentItem.CalculatedPrice = await _pricingService.GetMobileRegularPrice(PriceDTO);
                            preShipmentItem.CalculatedPrice = preShipmentItem.CalculatedPrice * preShipmentItem.Quantity;
                            preShipmentItem.CalculatedPrice = preShipmentItem.CalculatedPrice + IndividualPrice;
                        }
                        //preShipmentItem.CalculatedPrice = preShipmentItem.CalculatedPrice + IndividualPrice;
                    }

                    var vatForPreshipment = (preShipmentItem.CalculatedPrice * 0.05M);

                    if (!string.IsNullOrEmpty(preShipmentItem.Value))
                    {
                        DeclaredValue += Convert.ToDecimal(preShipmentItem.Value);
                        var DeclaredValueForPreShipment = Convert.ToDecimal(preShipmentItem.Value);
                        if (preShipment.Shipmentype == ShipmentType.Ecommerce)
                        {
                            preShipmentItem.CalculatedPrice = preShipmentItem.CalculatedPrice + (DeclaredValueForPreShipment * 0.01M);
                        }
                        else
                        {
                            preShipmentItem.CalculatedPrice = preShipmentItem.CalculatedPrice + (DeclaredValueForPreShipment * 0.01M) + vatForPreshipment;
                        }
                        preShipment.IsdeclaredVal = true;
                    }
                    else
                    {
                        if (preShipment.Shipmentype != ShipmentType.Ecommerce)
                        {
                            preShipmentItem.CalculatedPrice = preShipmentItem.CalculatedPrice + vatForPreshipment;
                        }
                    }

                    preShipmentItem.CalculatedPrice = (decimal)Math.Round((double)preShipmentItem.CalculatedPrice);
                    Price += (decimal)preShipmentItem.CalculatedPrice;
                };

                var DiscountPercent = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.DiscountPercentage, preShipment.CountryId);
                var Percentage = (Convert.ToDecimal(DiscountPercent.Value));
                var PercentageTobeUsed = ((100M - Percentage) / 100M);

                decimal EstimatedDeclaredPrice = Convert.ToDecimal(DeclaredValue);
                preShipment.DeliveryPrice = Price * PercentageTobeUsed;
                preShipment.InsuranceValue = (EstimatedDeclaredPrice * 0.01M);
                preShipment.CalculatedTotal = (double)(preShipment.DeliveryPrice);
                preShipment.CalculatedTotal = Math.Round((double)preShipment.CalculatedTotal);
                preShipment.Value = DeclaredValue;
                var discount = Math.Round(Price - (decimal)preShipment.CalculatedTotal);
                preShipment.DiscountValue = discount;

                var Pickuprice = await GetPickUpPrice(preShipment.VehicleType, preShipment.CountryId, preShipment.UserId);
                var PickupValue = Convert.ToDecimal(Pickuprice);

                var IsWithinProcessingTime = await WithinProcessingTime(preShipment.CountryId);

                decimal grandTotal = (decimal)preShipment.CalculatedTotal + PickupValue;

                //GIG Go Promo Price
                var gigGoPromo = await CalculatePromoPrice(preShipment, zoneid.ZoneId, PickupValue);
                if (gigGoPromo.GrandTotal > 0)
                {
                    grandTotal = (decimal)gigGoPromo.GrandTotal;
                    discount = (decimal)gigGoPromo.Discount;
                    preShipment.DiscountValue = discount;
                    preShipment.GrandTotal = grandTotal;
                }

                var returnprice = new MobilePriceDTO()
                {
                    MainCharge = (decimal)preShipment.CalculatedTotal,
                    PickUpCharge = PickupValue,
                    InsuranceValue = preShipment.InsuranceValue,
                    GrandTotal = grandTotal,
                    PreshipmentMobile = preShipment,
                    CurrencySymbol = Country.CurrencySymbol,
                    CurrencyCode = Country.CurrencyCode,
                    IsWithinProcessingTime = IsWithinProcessingTime,
                    Discount = discount
                };
                return returnprice;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task<MobilePriceDTO> GetPriceForResolveDispute(PreShipmentMobileDTO preShipment, decimal pickupPrice)
        {
            try
            {
                if (preShipment.PreShipmentItems.Count() == 0)
                {
                    throw new GenericException("No Preshipitem was added");
                }

                var zoneid = await _domesticroutezonemapservice.GetZoneMobile(preShipment.SenderStationId, preShipment.ReceiverStationId);

                //change the quantity of the preshipmentItem if it fall into promo category
                preShipment = await ChangePreshipmentItemQuantity(preShipment, zoneid.ZoneId);

                var Price = 0.0M;
                var amount = 0.0M;
                var IndividualPrice = 0.0M;
                decimal DeclaredValue = 0.0M;

                var Country = await _uow.Country.GetCountryByStationId(preShipment.SenderStationId);
                preShipment.CountryId = Country.CountryId;

                //undo comment when App is updated
                if (zoneid.ZoneId == 1 && preShipment.ReceiverLocation != null && preShipment.SenderLocation != null)
                {
                    var ShipmentCount = preShipment.PreShipmentItems.Count();

                    amount = await CalculateGeoDetailsBasedonLocation(preShipment);
                    IndividualPrice = (amount / ShipmentCount);
                }

                //Get the customer Type
                var userChannelCode = await _userService.GetUserChannelCode();
                var userChannel = await _uow.Company.GetAsync(x => x.CustomerCode == userChannelCode);

                if (userChannel != null)
                {
                    preShipment.Shipmentype = ShipmentType.Ecommerce;
                }

                foreach (var preShipmentItem in preShipment.PreShipmentItems)
                {
                    if (preShipmentItem.Quantity == 0)
                    {
                        throw new GenericException("Quantity cannot be zero");
                    }

                    var PriceDTO = new PricingDTO
                    {
                        DepartureStationId = preShipment.SenderStationId,
                        DestinationStationId = preShipment.ReceiverStationId,
                        Weight = preShipmentItem.Weight,
                        SpecialPackageId = (int)preShipmentItem.SpecialPackageId,
                        ShipmentType = preShipmentItem.ShipmentType,
                        CountryId = preShipment.CountryId  //Nigeria
                    };

                    if (preShipmentItem.ShipmentType == ShipmentType.Special)
                    {
                        if (preShipment.Shipmentype == ShipmentType.Ecommerce)
                        {
                            PriceDTO.DeliveryOptionId = 4;
                        }

                        preShipmentItem.CalculatedPrice = await _pricingService.GetMobileSpecialPrice(PriceDTO);
                        preShipmentItem.CalculatedPrice = preShipmentItem.CalculatedPrice * preShipmentItem.Quantity;
                        preShipmentItem.CalculatedPrice = preShipmentItem.CalculatedPrice + IndividualPrice;
                    }
                    else if (preShipmentItem.ShipmentType == ShipmentType.Regular)
                    {
                        if (preShipmentItem.Weight == 0)
                        {
                            throw new GenericException("Item weight cannot be zero");
                        }

                        if (preShipment.Shipmentype == ShipmentType.Ecommerce)
                        {
                            preShipmentItem.CalculatedPrice = await _pricingService.GetMobileEcommercePrice(PriceDTO);
                            preShipmentItem.CalculatedPrice = preShipmentItem.CalculatedPrice * preShipmentItem.Quantity;
                            preShipmentItem.CalculatedPrice = preShipmentItem.CalculatedPrice + IndividualPrice;
                        }
                        else
                        {
                            preShipmentItem.CalculatedPrice = await _pricingService.GetMobileRegularPrice(PriceDTO);
                            preShipmentItem.CalculatedPrice = preShipmentItem.CalculatedPrice * preShipmentItem.Quantity;
                            preShipmentItem.CalculatedPrice = preShipmentItem.CalculatedPrice + IndividualPrice;
                        }
                        //preShipmentItem.CalculatedPrice = preShipmentItem.CalculatedPrice + IndividualPrice;
                    }

                    var vatForPreshipment = (preShipmentItem.CalculatedPrice * 0.05M);

                    if (!string.IsNullOrEmpty(preShipmentItem.Value))
                    {
                        DeclaredValue += Convert.ToDecimal(preShipmentItem.Value);
                        var DeclaredValueForPreShipment = Convert.ToDecimal(preShipmentItem.Value);
                        if (preShipment.Shipmentype == ShipmentType.Ecommerce)
                        {
                            preShipmentItem.CalculatedPrice = preShipmentItem.CalculatedPrice + (DeclaredValueForPreShipment * 0.01M);
                        }
                        else
                        {
                            preShipmentItem.CalculatedPrice = preShipmentItem.CalculatedPrice + (DeclaredValueForPreShipment * 0.01M) + vatForPreshipment;
                        }
                        preShipment.IsdeclaredVal = true;
                    }
                    else
                    {
                        if (preShipment.Shipmentype != ShipmentType.Ecommerce)
                        {
                            preShipmentItem.CalculatedPrice = preShipmentItem.CalculatedPrice + vatForPreshipment;
                        }
                    }

                    preShipmentItem.CalculatedPrice = (decimal)Math.Round((double)preShipmentItem.CalculatedPrice);
                    Price += (decimal)preShipmentItem.CalculatedPrice;
                };

                var DiscountPercent = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.DiscountPercentage, preShipment.CountryId);
                var Percentage = (Convert.ToDecimal(DiscountPercent.Value));
                var PercentageTobeUsed = ((100M - Percentage) / 100M);

                decimal EstimatedDeclaredPrice = Convert.ToDecimal(DeclaredValue);
                preShipment.DeliveryPrice = Price * PercentageTobeUsed;
                preShipment.InsuranceValue = (EstimatedDeclaredPrice * 0.01M);
                preShipment.CalculatedTotal = (double)(preShipment.DeliveryPrice);
                preShipment.CalculatedTotal = Math.Round((double)preShipment.CalculatedTotal);
                preShipment.Value = DeclaredValue;
                var discount = Math.Round(Price - (decimal)preShipment.CalculatedTotal);
                preShipment.DiscountValue = discount;

                //var Pickuprice = await GetPickUpPrice(preShipment.VehicleType, preShipment.CountryId, preShipment.UserId);
                //var PickupValue = Convert.ToDecimal(Pickuprice);

                var IsWithinProcessingTime = await WithinProcessingTime(preShipment.CountryId);

                //decimal grandTotal = (decimal)preShipment.CalculatedTotal + PickupValue;
                decimal grandTotal = (decimal)preShipment.CalculatedTotal + pickupPrice;

                //GIG Go Promo Price
                var gigGoPromo = await CalculatePromoPrice(preShipment, zoneid.ZoneId, pickupPrice);
                if (gigGoPromo.GrandTotal > 0)
                {
                    grandTotal = (decimal)gigGoPromo.GrandTotal;
                    discount = (decimal)gigGoPromo.Discount;
                    preShipment.DiscountValue = discount;
                    preShipment.GrandTotal = grandTotal;
                }

                var returnprice = new MobilePriceDTO()
                {
                    MainCharge = (decimal)preShipment.CalculatedTotal,
                    PickUpCharge = pickupPrice,
                    InsuranceValue = preShipment.InsuranceValue,
                    GrandTotal = grandTotal,
                    PreshipmentMobile = preShipment,
                    CurrencySymbol = Country.CurrencySymbol,
                    CurrencyCode = Country.CurrencyCode,
                    IsWithinProcessingTime = IsWithinProcessingTime,
                    Discount = discount
                };
                return returnprice;
            }
            catch (Exception)
            {
                throw;
            }
        }

        //Get Price API that is called just before create shipment
        public async Task<MultipleMobilePriceDTO> GetPriceForMultipleShipments(NewPreShipmentMobileDTO preShipmentItemMobileDTO)
        {
            try
            {
                decimal shipmentTotal = 0;
                decimal totalInsurance = 0;
                decimal totalDiscount = 0;

                var listOfPreShipment = await GroupMobileShipmentByReceiver(preShipmentItemMobileDTO);

                var IsWithinProcessingTime = await WithinProcessingTime(listOfPreShipment[0].CountryId);

                var price = new MultipleMobilePriceDTO
                {
                    itemPriceDetails = new List<MobilePricePerItemDTO>()
                };


                foreach (var item in listOfPreShipment)
                {
                    foreach (var pricePerItem in item.PreShipmentItems)
                    {
                        var newPrice = new MobilePricePerItemDTO();
                        newPrice.ItemDescription = pricePerItem.Description;
                        newPrice.ItemShipmentType = pricePerItem.ShipmentType;
                        newPrice.ItemWeight = pricePerItem.Weight;
                        newPrice.ItemQuantity = pricePerItem.Quantity;
                        newPrice.ItemName = pricePerItem.ItemName;
                        newPrice.ItemCalculatedPrice = pricePerItem.CalculatedPrice;
                        newPrice.ItemRecever = item.ReceiverName;

                        price.itemPriceDetails.Add(newPrice);
                    }

                    shipmentTotal = shipmentTotal + (decimal)item.CalculatedTotal;
                    totalInsurance = totalInsurance + (decimal)item.InsuranceValue;
                    totalDiscount = totalDiscount + (decimal)item.DiscountValue;
                }

                //Get Pick UP price
                var Pickuprice = await GetPickUpPriceForMultipleShipment(listOfPreShipment[0].CustomerType, listOfPreShipment[0].VehicleType, listOfPreShipment[0].CountryId);
                var PickupValue = Convert.ToDecimal(Pickuprice);
                var grandTotal = shipmentTotal + PickupValue;

                price.MainCharge = shipmentTotal;
                price.PickUpCharge = PickupValue;
                price.InsuranceValue = totalInsurance;
                price.GrandTotal = grandTotal;
                price.CurrencySymbol = listOfPreShipment[0].CurrencySymbol;
                price.CurrencyCode = listOfPreShipment[0].CurrencyCode;
                price.Discount = totalDiscount;
                price.IsWithinProcessingTime = IsWithinProcessingTime;

                return price;
            }
            catch (Exception)
            {
                throw;
            }
        }

        //Multiple Shipments NEW
        public async Task<MobilePriceDTO> GetPriceForNonHaulage(PreShipmentMobileDTO preShipment)
        {
            try
            {
                var Price = 0.0M;
                var amount = 0.0M;
                var IndividualPrice = 0.0M;
                decimal DeclaredValue = 0.0M;

                //undo comment when App is updated
                if (preShipment.ZoneMapping == 1 && preShipment.ReceiverLocation != null && preShipment.SenderLocation != null)
                {
                    var ShipmentCount = preShipment.PreShipmentItems.Count();

                    amount = await CalculateGeoDetailsBasedonLocation(preShipment);
                    IndividualPrice = (amount / ShipmentCount);
                }

                foreach (var preShipmentItem in preShipment.PreShipmentItems)
                {
                    if (preShipmentItem.Quantity == 0)
                    {
                        throw new GenericException("Quantity cannot be zero");
                    }

                    var PriceDTO = new PricingDTO
                    {
                        DepartureStationId = preShipment.SenderStationId,
                        DestinationStationId = preShipment.ReceiverStationId,
                        Weight = preShipmentItem.Weight,
                        SpecialPackageId = (int)preShipmentItem.SpecialPackageId,
                        ShipmentType = preShipmentItem.ShipmentType,
                        CountryId = preShipment.CountryId  //Nigeria
                    };

                    if (preShipmentItem.ShipmentType == ShipmentType.Special)
                    {
                        if (preShipment.Shipmentype == ShipmentType.Ecommerce)
                        {
                            PriceDTO.DeliveryOptionId = 4;
                        }

                        preShipmentItem.CalculatedPrice = await _pricingService.GetMobileSpecialPrice(PriceDTO);
                        preShipmentItem.CalculatedPrice = preShipmentItem.CalculatedPrice * preShipmentItem.Quantity;
                        preShipmentItem.CalculatedPrice = preShipmentItem.CalculatedPrice + IndividualPrice;
                    }
                    else if (preShipmentItem.ShipmentType == ShipmentType.Regular)
                    {
                        if (preShipmentItem.Weight == 0)
                        {
                            throw new GenericException("Item weight cannot be zero");
                        }

                        if (preShipment.Shipmentype == ShipmentType.Ecommerce)
                        {
                            preShipmentItem.CalculatedPrice = await _pricingService.GetMobileEcommercePrice(PriceDTO);
                            preShipmentItem.CalculatedPrice = preShipmentItem.CalculatedPrice * preShipmentItem.Quantity;
                            preShipmentItem.CalculatedPrice = preShipmentItem.CalculatedPrice + IndividualPrice;
                        }
                        else
                        {
                            preShipmentItem.CalculatedPrice = await _pricingService.GetMobileRegularPrice(PriceDTO);
                            preShipmentItem.CalculatedPrice = preShipmentItem.CalculatedPrice * preShipmentItem.Quantity;
                            preShipmentItem.CalculatedPrice = preShipmentItem.CalculatedPrice + IndividualPrice;
                        }
                    }

                    var vatForPreshipment = (preShipmentItem.CalculatedPrice * 0.05M);

                    if (!string.IsNullOrEmpty(preShipmentItem.Value))
                    {
                        DeclaredValue += Convert.ToDecimal(preShipmentItem.Value);
                        var DeclaredValueForPreShipment = Convert.ToDecimal(preShipmentItem.Value);
                        if (preShipment.Shipmentype == ShipmentType.Ecommerce)
                        {
                            preShipmentItem.CalculatedPrice = preShipmentItem.CalculatedPrice + (DeclaredValueForPreShipment * 0.01M);
                        }
                        else
                        {
                            preShipmentItem.CalculatedPrice = preShipmentItem.CalculatedPrice + (DeclaredValueForPreShipment * 0.01M) + vatForPreshipment;
                        }
                        preShipment.IsdeclaredVal = true;
                    }
                    else
                    {
                        if (preShipment.Shipmentype != ShipmentType.Ecommerce)
                        {
                            preShipmentItem.CalculatedPrice = preShipmentItem.CalculatedPrice + vatForPreshipment;
                        }
                    }

                    preShipmentItem.CalculatedPrice = (decimal)Math.Round((double)preShipmentItem.CalculatedPrice);
                    Price += (decimal)preShipmentItem.CalculatedPrice;
                };

                var DiscountPercent = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.DiscountPercentage, preShipment.CountryId);
                var Percentage = (Convert.ToDecimal(DiscountPercent.Value));
                var PercentageTobeUsed = ((100M - Percentage) / 100M);

                decimal EstimatedDeclaredPrice = Convert.ToDecimal(DeclaredValue);
                preShipment.DeliveryPrice = Price * PercentageTobeUsed;
                preShipment.InsuranceValue = (EstimatedDeclaredPrice * 0.01M);
                preShipment.CalculatedTotal = (double)(preShipment.DeliveryPrice);
                preShipment.CalculatedTotal = Math.Round((double)preShipment.CalculatedTotal);
                preShipment.Value = DeclaredValue;
                var discount = Math.Round(Price - (decimal)preShipment.CalculatedTotal);
                preShipment.DiscountValue = discount;

                //var IsWithinProcessingTime = await WithinProcessingTime(preShipment.CountryId);

                var returnprice = new MobilePriceDTO()
                {
                    MainCharge = (decimal)preShipment.CalculatedTotal,
                    InsuranceValue = preShipment.InsuranceValue,
                    GrandTotal = ((decimal)preShipment.CalculatedTotal),
                    PreshipmentMobile = preShipment,
                    CurrencySymbol = preShipment.CurrencySymbol,
                    CurrencyCode = preShipment.CurrencyCode,
                    //IsWithinProcessingTime = IsWithinProcessingTime,
                    Discount = discount
                };
                return returnprice;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<List<PreShipmentMobileDTO>> GetShipments(BaseFilterCriteria filterOptionsDto)
        {
            try
            {
                //get startDate and endDate
                var queryDate = filterOptionsDto.getStartDateAndEndDate();
                var startDate = queryDate.Item1;
                var endDate = queryDate.Item2;
                var allShipmentsResult = _uow.PreShipmentMobile.GetAllAsQueryable();

                if (filterOptionsDto.StartDate == null & filterOptionsDto.EndDate == null && filterOptionsDto.fromGigGoDashboard == false)
                {
                    startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                    endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day).AddDays(1);
                }

                else if (filterOptionsDto.StartDate == null && filterOptionsDto.EndDate == null && filterOptionsDto.fromGigGoDashboard == true)
                {
                    startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day).AddDays(-7);
                    endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day).AddDays(1);
                }

                //Excluding It Test
                string[] testUserId = { "2932eb15-aa30-462c-89f0-7247670f504b", "ab3722d7-57f3-4e6e-a32d-1580315b7da6", "e67d50c2-953a-44b2-bbcd-c38fadef237f", "b476fea8-84e4-4c5b-ac51-2efd68526fdc" };

                if (filterOptionsDto.StationId > 0)
                {
                    allShipmentsResult = allShipmentsResult.Where(s => s.DateCreated >= startDate && s.DateCreated < endDate &&
                                            !testUserId.Contains(s.UserId) && s.SenderName != "it_test test"
                                            && s.SenderStationId == filterOptionsDto.StationId);

                }
                else
                {
                    allShipmentsResult = allShipmentsResult.Where(s => s.DateCreated >= startDate && s.DateCreated < endDate &&
                                            !testUserId.Contains(s.UserId) && s.SenderName != "it_test test");
                }

                List<PreShipmentMobileDTO> shipmentDto = (from r in allShipmentsResult
                                                          select new PreShipmentMobileDTO()
                                                          {
                                                              PreShipmentMobileId = r.PreShipmentMobileId,
                                                              Waybill = r.Waybill,
                                                              CustomerType = r.CustomerType,
                                                              ActualDateOfArrival = r.ActualDateOfArrival,
                                                              DateCreated = r.DateCreated,
                                                              DateModified = r.DateModified,
                                                              ExpectedDateOfArrival = r.ExpectedDateOfArrival,
                                                              ReceiverAddress = r.ReceiverAddress,
                                                              SenderAddress = r.SenderAddress,
                                                              SenderPhoneNumber = r.SenderPhoneNumber,
                                                              ReceiverCity = r.ReceiverCity,
                                                              ReceiverCountry = r.ReceiverCountry,
                                                              ReceiverEmail = r.ReceiverEmail,
                                                              ReceiverName = r.ReceiverName,
                                                              ReceiverPhoneNumber = r.ReceiverPhoneNumber,
                                                              ReceiverState = r.ReceiverState,
                                                              SenderName = r.SenderName,
                                                              UserId = r.UserId,
                                                              Value = r.Value,
                                                              shipmentstatus = r.shipmentstatus,
                                                              GrandTotal = r.GrandTotal,
                                                              DeliveryPrice = r.DeliveryPrice,
                                                              CalculatedTotal = r.CalculatedTotal,
                                                              InsuranceValue = r.InsuranceValue,
                                                              DiscountValue = r.DiscountValue,
                                                              CompanyType = r.CompanyType,
                                                              CustomerCode = r.CustomerCode,
                                                              VehicleType = r.VehicleType
                                                          }).ToList();

                return await Task.FromResult(shipmentDto.OrderByDescending(x => x.DateCreated).ToList());

            }
            catch (Exception)
            {
                throw;
            }

        }

        public async Task<PreShipmentMobileDTO> GetShipmentByWaybill(string waybill)
        {
            try
            {
                var shipmentsResult = await _uow.PreShipmentMobile.GetAsync(x => x.Waybill == waybill);
                if (shipmentsResult == null)
                {
                    throw new GenericException($"No Waybill exists for this code: {waybill}");
                }
                var shipmentDTO = Mapper.Map<PreShipmentMobileDTO>(shipmentsResult);

                return shipmentDTO;
            }
            catch (Exception)
            {
                throw;
            }

        }

        //GIG Go Dashboard
        public async Task<GIGGoDashboardDTO> GetDashboardInfo(BaseFilterCriteria filterCriteria)
        {
            try
            {
                var shipment = await GetShipments(filterCriteria);

                //To get Partners details
                var partners = await _partnerService.GetPartners();
                var internals = partners.Where(s => s.PartnerType == PartnerType.InternalDeliveryPartner).Count();
                var externals = partners.Where(s => s.PartnerType == PartnerType.DeliveryPartner && s.IsActivated == true).Count();

                //To get shipment details
                var all = shipment.Count();
                var accepted = shipment.Where(s => s.shipmentstatus == "Assigned for Pickup").Count();
                var cancelled = shipment.Where(s => s.shipmentstatus == "Cancelled").Count();
                var pickedUp = shipment.Where(s => s.shipmentstatus == "PickedUp").Count();
                var delivered = shipment.Where(s => s.shipmentstatus == "Delivered").Count();
                var sum = shipment.Sum(s => s.GrandTotal);

                //To get partner earnings
                var partnerEarnings = await _uow.PartnerTransactions.GetPartnerTransactionByDate(filterCriteria);
                var sumPartnerEarnings = partnerEarnings.Sum(s => s.AmountReceived);


                var result = new GIGGoDashboardDTO
                {
                    ExternalPartners = externals,
                    InternalPartners = internals,

                    AcceptedShipments = accepted,
                    CancelledShipments = cancelled,
                    PickedupShipments = pickedUp,
                    DeliveredShipments = delivered,
                    ShipmentRequests = all,
                    TotalRevenue = sum,
                    PartnerEarnings = sumPartnerEarnings

                };

                return result;

            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<PreShipmentMobileDTO> GetPreShipmentDetail(string waybill)
        {
            try
            {
                var Shipmentdto = new PreShipmentMobileDTO();
                var shipment = await _uow.PreShipmentMobile.GetAsync(x => x.Waybill == waybill, "PreShipmentItems,SenderLocation,ReceiverLocation,serviceCentreLocation");
                if (shipment != null)
                {
                    var currentUser = await _userService.GetCurrentUserId();
                    var user = await _uow.User.GetUserById(currentUser);
                    Shipmentdto = Mapper.Map<PreShipmentMobileDTO>(shipment);
                    if (user.UserChannelType.ToString() == UserChannelType.Partner.ToString() || user.SystemUserRole == "Dispatch Rider")
                    {
                        if (shipment.ServiceCentreAddress != null)
                        {
                            Shipmentdto.ReceiverLocation = new LocationDTO
                            {
                                Latitude = shipment.serviceCentreLocation.Latitude,
                                Longitude = shipment.serviceCentreLocation.Longitude
                            };
                            Shipmentdto.ReceiverAddress = shipment.ServiceCentreAddress;
                        }
                    }

                    var country = await _uow.Country.GetCountryByStationId(shipment.SenderStationId);
                    if (country != null)
                    {
                        Shipmentdto.CurrencyCode = country.CurrencyCode;
                        Shipmentdto.CurrencySymbol = country.CurrencySymbol;
                    }
                    var partner = await _uow.MobilePickUpRequests.GetPartnerDetailsForAWaybill(waybill);
                    Shipmentdto.partnerDTO = partner;
                }
                else
                {
                    var agilityshipment = await _uow.Shipment.GetAsync(x => x.Waybill == waybill, "ShipmentItems");
                    if (agilityshipment != null)
                    {
                        Shipmentdto = Mapper.Map<PreShipmentMobileDTO>(agilityshipment);

                        CustomerType customerType = (CustomerType)Enum.Parse(typeof(CustomerType), agilityshipment.CustomerType);
                        var CustomerDetails = await _customerService.GetCustomer(agilityshipment.CustomerId, customerType);

                        Shipmentdto.SenderAddress = CustomerDetails.Address;
                        Shipmentdto.SenderName = CustomerDetails.Name;
                        Shipmentdto.SenderPhoneNumber = CustomerDetails.PhoneNumber;

                        Shipmentdto.PreShipmentItems = new List<PreShipmentItemMobileDTO>();

                        foreach (var shipments in agilityshipment.ShipmentItems)
                        {
                            var item = Mapper.Map<PreShipmentItemMobileDTO>(shipments);
                            item.ItemName = shipments.Description;
                            item.ImageUrl = "";
                            Shipmentdto.PreShipmentItems.Add(item);
                        }

                        var country = await _uow.Country.GetAsync(agilityshipment.DepartureCountryId);
                        if (country != null)
                        {
                            Shipmentdto.CurrencyCode = country.CurrencyCode;
                            Shipmentdto.CurrencySymbol = country.CurrencySymbol;
                        }
                    }
                    else
                    {
                        throw new GenericException($"Shipment with waybill: {waybill} does not exist");
                    }
                }

                if (Shipmentdto != null)
                {
                    string groupCode = await _uow.MobileGroupCodeWaybillMapping.GetGroupCode(waybill);
                    Shipmentdto.GroupCodeNumber = groupCode;
                }
                return await Task.FromResult(Shipmentdto);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<PreShipmentMobileDTO>> GetPreShipmentForUser()
        {
            try
            {
                var currentUser = await _userService.GetCurrentUserId();
                var user = await _uow.User.GetUserById(currentUser);
                var mobileShipment = await _uow.PreShipmentMobile.FindAsync(x => x.CustomerCode.Equals(user.UserChannelCode), "PreShipmentItems,SenderLocation,ReceiverLocation");

                List<PreShipmentMobileDTO> shipmentDto = (from r in mobileShipment
                                                          select new PreShipmentMobileDTO()
                                                          {
                                                              PreShipmentMobileId = r.PreShipmentMobileId,
                                                              Waybill = r.Waybill,
                                                              ActualDateOfArrival = r.ActualDateOfArrival,
                                                              DateCreated = r.DateCreated,
                                                              DateModified = r.DateModified,
                                                              ExpectedDateOfArrival = r.ExpectedDateOfArrival,
                                                              ReceiverAddress = r.ReceiverAddress,
                                                              SenderAddress = r.SenderAddress,
                                                              SenderPhoneNumber = r.SenderPhoneNumber,
                                                              ReceiverCountry = r.ReceiverCountry,
                                                              SenderStationId = r.SenderStationId,
                                                              ReceiverStationId = r.ReceiverStationId,
                                                              ReceiverEmail = r.ReceiverEmail,
                                                              ReceiverName = r.ReceiverName,
                                                              ReceiverPhoneNumber = r.ReceiverPhoneNumber,
                                                              ReceiverState = r.ReceiverState,
                                                              SenderName = r.SenderName,
                                                              UserId = r.UserId,
                                                              Value = r.Value,
                                                              shipmentstatus = r.shipmentstatus,
                                                              GrandTotal = r.GrandTotal,
                                                              DeliveryPrice = r.DeliveryPrice,
                                                              CalculatedTotal = r.CalculatedTotal,
                                                              CustomerCode = r.CustomerCode,
                                                              VehicleType = r.VehicleType,
                                                              ReceiverLocation = new LocationDTO
                                                              {
                                                                  Longitude = r.ReceiverLocation.Longitude,
                                                                  Latitude = r.ReceiverLocation.Latitude,
                                                                  Name = r.ReceiverLocation.Name,
                                                                  FormattedAddress = r.ReceiverLocation.FormattedAddress
                                                              },
                                                              SenderLocation = new LocationDTO
                                                              {
                                                                  Longitude = r.SenderLocation.Longitude,
                                                                  Latitude = r.SenderLocation.Latitude,
                                                                  Name = r.ReceiverLocation.Name,
                                                                  FormattedAddress = r.ReceiverLocation.FormattedAddress
                                                              }
                                                          }).OrderByDescending(x => x.DateCreated).Take(20).ToList();

                var agilityShipment = await GetPreShipmentForEcommerce(user.UserChannelCode);

                //added agility shipment to Giglgo list of shipments.
                foreach (var shipment in shipmentDto)
                {

                    if (agilityShipment.Exists(s => s.Waybill == shipment.Waybill))
                    {
                        var s = agilityShipment.Where(x => x.Waybill == shipment.Waybill).FirstOrDefault();
                        agilityShipment.Remove(s);
                    }

                    var partnerId = await _uow.MobilePickUpRequests.GetAsync(r => r.Waybill == shipment.Waybill && r.Status == MobilePickUpRequestStatus.Delivered.ToString());
                    if (partnerId != null)
                    {
                        var partneruser = await _uow.User.GetUserById(partnerId.UserId);
                        if (partneruser != null)
                        {
                            shipment.PartnerFirstName = partneruser.FirstName;
                            shipment.PartnerLastName = partneruser.LastName;
                            shipment.PartnerImageUrl = partneruser.PictureUrl;
                        }
                    }

                    shipment.IsRated = false;

                    var rating = await _uow.MobileRating.GetAsync(j => j.Waybill == shipment.Waybill);
                    if (rating != null)
                    {
                        shipment.IsRated = rating.IsRatedByCustomer;
                    }

                    var country = await _uow.Country.GetCountryByStationId(shipment.SenderStationId);
                    if (country != null)
                    {
                        shipment.CurrencyCode = country.CurrencyCode;
                        shipment.CurrencySymbol = country.CurrencySymbol;
                    }

                }

                var newlist = shipmentDto.Union(agilityShipment);
                return await Task.FromResult(newlist.OrderByDescending(x => x.DateCreated).Take(20).ToList());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<IEnumerable<SpecialDomesticPackageDTO>> GetSpecialDomesticPackages()
        {
            try
            {
                //Exclude those package from showing on gig go
                int[] excludePackage = new int[] { 11, 12, 13, 14, 15, 16, 17, 32, 33, 34, 35, 36, 37, 38 };

                var result = _uow.SpecialDomesticPackage.GetAll().Where(x => !excludePackage.Contains(x.SpecialDomesticPackageId));

                var packages = from s in result
                               select new SpecialDomesticPackageDTO
                               {
                                   SpecialDomesticPackageId = s.SpecialDomesticPackageId,
                                   SpecialDomesticPackageType = s.SpecialDomesticPackageType,
                                   Name = s.Name,
                                   Status = s.Status,
                                   SubCategory = new SubCategoryDTO
                                   {
                                       SubCategoryName = s.SubCategory.SubCategoryName,
                                       Category = new CategoryDTO
                                       {
                                           CategoryId = s.SubCategory.Category.CategoryId,
                                           CategoryName = s.SubCategory.Category.CategoryName
                                       },
                                       SubCategoryId = s.SubCategory.SubCategoryId,
                                       CategoryId = s.SubCategory.CategoryId,
                                       Weight = s.SubCategory.Weight,
                                       WeightRange = s.SubCategory.WeightRange
                                   }
                               };
                return await Task.FromResult(packages);
            }
            catch (Exception)
            {
                throw new GenericException("Please an error occured while getting Special Packages");
            }
        }

        public async Task<List<CategoryDTO>> GetCategories()
        {
            try
            {
                var categories = await _categoryservice.GetCategories();
                return categories;
            }
            catch (Exception)
            {
                throw new GenericException("Please an error occured while getting categories.Please try again");
            }
        }

        public async Task<List<SubCategoryDTO>> GetSubCategories()
        {
            try
            {
                var subcategories = await _subcategoryservice.GetSubCategories();
                return subcategories;
            }
            catch (Exception)
            {
                throw new GenericException("Please an error occurred while getting sub-categories.Please try again");
            }
        }

        public async Task ScanMobileShipment(ScanDTO scan)
        {
            // verify the waybill number exists in the system
            try
            {
                var shipment = await GetMobileShipmentForScan(scan.WaybillNumber);
                string scanStatus = scan.ShipmentScanStatus.ToString();
                if (shipment != null)
                {
                    await _mobiletrackingservice.AddMobileShipmentTracking(new MobileShipmentTrackingDTO
                    {
                        DateTime = DateTime.Now,
                        Status = scanStatus,
                        Waybill = scan.WaybillNumber,
                    }, scan.ShipmentScanStatus);
                }
            }
            catch (Exception)
            {
                throw new GenericException("Please an error occurred while trying to scan shipment.");
            }
        }

        public async Task<PreShipmentMobile> GetMobileShipmentForScan(string waybill)
        {
            try
            {
                var shipment = await _uow.PreShipmentMobile.GetAsync(x => x.Waybill.Equals(waybill));
                if (shipment == null)
                {
                    throw new GenericException("Waybill does not exist");
                }
                return shipment;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<MobileShipmentTrackingHistoryDTO> TrackShipment(string waybillNumber)
        {
            try
            {
                var result = await _mobiletrackingservice.GetMobileShipmentTrackings(waybillNumber);
                return result;
            }
            catch
            {
                throw new GenericException("Error: You cannot track this waybill number.");
            }
        }

        public async Task<PreShipmentMobileDTO> AddMobilePickupRequest(MobilePickUpRequestsDTO pickuprequest)
        {
            try
            {
                if (pickuprequest == null)
                {
                    throw new GenericException("Group does not exist");
                }

                var newPreShipment = new PreShipmentMobileDTO();

                if (pickuprequest.UserId == null)
                {
                    pickuprequest.UserId = await _userService.GetCurrentUserId();
                }

                var preshipmentmobile = await _uow.PreShipmentMobile.GetAsync(s => s.Waybill == pickuprequest.Waybill, "PreShipmentItems,SenderLocation,ReceiverLocation,serviceCentreLocation");
                if (preshipmentmobile == null)
                {
                    throw new GenericException("Shipment does not exist");
                }
                else
                {
                    if (pickuprequest.Status == MobilePickUpRequestStatus.Rejected.ToString() || pickuprequest.Status == MobilePickUpRequestStatus.TimedOut.ToString()
                       || pickuprequest.Status == MobilePickUpRequestStatus.Missed.ToString())
                    {
                        var request = await _uow.MobilePickUpRequests.GetAsync(s => s.Waybill == pickuprequest.Waybill && s.UserId == pickuprequest.UserId);

                        if (request == null)
                        {
                            await _mobilepickuprequestservice.AddMobilePickUpRequests(pickuprequest);
                        }
                        else
                        {
                            //if the current status is Missed, update it else do nothing
                            if (request.Status == MobilePickUpRequestStatus.Missed.ToString())
                            {
                                request.Status = pickuprequest.Status;
                            }
                        }
                    }
                    else if (preshipmentmobile.shipmentstatus == "Shipment created" || preshipmentmobile.shipmentstatus == MobilePickUpRequestStatus.Processing.ToString())
                    {
                        pickuprequest.Status = MobilePickUpRequestStatus.Accepted.ToString();
                        await _mobilepickuprequestservice.AddOrUpdateMobilePickUpRequests(pickuprequest);

                        //Update Activity Status
                        await UpdateActivityStatus(pickuprequest.UserId, ActivityStatus.OnDelivery);
                    }
                    else
                    {
                        throw new GenericException($"Shipment has already been accepted..");
                    }

                    if (pickuprequest.ServiceCentreId != null)
                    {
                        var DestinationServiceCentreId = await _uow.ServiceCentre.GetAsync(s => s.Code == pickuprequest.ServiceCentreId);
                        preshipmentmobile.ServiceCentreAddress = DestinationServiceCentreId.Address;
                        var Locationdto = new LocationDTO
                        {
                            Latitude = DestinationServiceCentreId.Latitude,
                            Longitude = DestinationServiceCentreId.Longitude
                        };
                        var LOcation = Mapper.Map<Location>(Locationdto);
                        preshipmentmobile.serviceCentreLocation = LOcation;
                    }

                    if (pickuprequest.Status == MobilePickUpRequestStatus.Accepted.ToString())
                    {
                        preshipmentmobile.shipmentstatus = "Assigned for Pickup";

                        await ScanMobileShipment(new ScanDTO
                        {
                            WaybillNumber = pickuprequest.Waybill,
                            ShipmentScanStatus = ShipmentScanStatus.MAPT
                        });
                    }

                    newPreShipment = Mapper.Map<PreShipmentMobileDTO>(preshipmentmobile);

                    if (pickuprequest.ServiceCentreId != null)
                    {
                        newPreShipment.ReceiverAddress = preshipmentmobile.ServiceCentreAddress;
                        newPreShipment.ReceiverLocation.Latitude = preshipmentmobile.serviceCentreLocation.Latitude;
                        newPreShipment.ReceiverLocation.Longitude = preshipmentmobile.serviceCentreLocation.Longitude;
                    }

                    var Country = await _uow.Country.GetCountryByStationId(preshipmentmobile.SenderStationId);

                    if (Country != null)
                    {
                        newPreShipment.CurrencyCode = Country.CurrencyCode;
                        newPreShipment.CurrencySymbol = Country.CurrencySymbol;
                    }

                    await _uow.CompleteAsync();
                }

                return newPreShipment;
            }
            catch
            {
                throw;
            }
        }

        public async Task<List<PreShipmentMobileDTO>> AddMobilePickupRequestMultipleShipment(MobilePickUpRequestsDTO pickuprequest)
        {
            try
            {
                if (pickuprequest == null)
                {
                    throw new GenericException("Group does not exist");
                }

                if (pickuprequest.UserId == null)
                {
                    pickuprequest.UserId = await _userService.GetCurrentUserId();
                }

                var groupList = await _uow.MobileGroupCodeWaybillMapping.FindAsync(x => x.GroupCodeNumber == pickuprequest.GroupCodeNumber);
                if (groupList.Any())
                {
                    var waybillHashSet = new HashSet<string>();

                    //To handle null waybill
                    foreach (var item in groupList)
                    {
                        if (item.WaybillNumber != null)
                        {
                            waybillHashSet.Add(item.WaybillNumber);
                        }
                    }
                    List<string> waybillList = waybillHashSet.ToList();

                    //var allpreshipmentmobile = _uow.PreShipmentMobile.GetAllAsQueryable().Where(s => waybillList.Contains(s.Waybill)).ToList();
                    var allpreshipmentmobile = await _uow.PreShipmentMobile.FindAsync(x => waybillList.Contains(x.Waybill), "PreShipmentItems,SenderLocation,ReceiverLocation,serviceCentreLocation");

                    //var preshipmentmobile = await _uow.PreShipmentMobile.GetAsync(s => s.Waybill == pickuprequest.Waybill, "PreShipmentItems,SenderLocation,ReceiverLocation,serviceCentreLocation");

                    //map the result 
                    List<PreShipmentMobileDTO> newPreShipmentDTO = Mapper.Map<List<PreShipmentMobileDTO>>(allpreshipmentmobile);

                    if (pickuprequest.Status == MobilePickUpRequestStatus.Rejected.ToString()
                        || pickuprequest.Status == MobilePickUpRequestStatus.TimedOut.ToString()
                        || pickuprequest.Status == MobilePickUpRequestStatus.Missed.ToString())
                    {
                        var request = _uow.MobilePickUpRequests.GetAllAsQueryable().Where(s => waybillList.Contains(s.Waybill) && s.UserId == pickuprequest.UserId).ToList();

                        if (request.Any())
                        {
                            //if any status is Missed, update all else do nothing
                            if (request.Any(x => x.Status == MobilePickUpRequestStatus.Missed.ToString()))
                            {
                                request.ForEach(x => x.Status = pickuprequest.Status);
                            }
                        }
                        else
                        {
                            await _mobilepickuprequestservice.AddOrUpdateMobilePickUpRequestsMultipleShipments(pickuprequest, waybillList);
                        }
                    }
                    else if (pickuprequest.Status == MobilePickUpRequestStatus.Accepted.ToString() && allpreshipmentmobile.All(x => x.shipmentstatus == "Shipment created" || x.shipmentstatus == MobilePickUpRequestStatus.Processing.ToString()))
                    {
                        //update the shipment status
                        foreach (var item in allpreshipmentmobile)
                        {
                            item.shipmentstatus = "Assigned for Pickup";
                        }

                        //if some shipment going outside the state, update the location of those shipment
                        if (!string.IsNullOrEmpty(pickuprequest.ServiceCentreId))
                        {
                            var DestinationServiceCentreId = await _uow.ServiceCentre.GetAsync(s => s.Code == pickuprequest.ServiceCentreId);

                            Location location = new Location
                            {
                                Latitude = DestinationServiceCentreId.Latitude,
                                Longitude = DestinationServiceCentreId.Longitude
                            };

                            //update only the shipment going outside the state
                            foreach (var item in allpreshipmentmobile)
                            {
                                if (item.ZoneMapping != 1)
                                {
                                    item.serviceCentreLocation = location;
                                }
                            }

                            //update the return data
                            LocationDTO locationDTO = Mapper.Map<LocationDTO>(location);

                            //update the location for return data
                            foreach (var item in newPreShipmentDTO)
                            {
                                if (item.ZoneMapping != 1)
                                {
                                    item.ReceiverAddress = DestinationServiceCentreId.Address;
                                    item.ReceiverLocation.Latitude = DestinationServiceCentreId.Latitude;
                                    item.ReceiverLocation.Longitude = DestinationServiceCentreId.Longitude;
                                    item.serviceCentreLocation = locationDTO;
                                }
                            }
                        }

                        newPreShipmentDTO.ForEach(x => x.GroupCodeNumber = pickuprequest.GroupCodeNumber);

                        await _mobilepickuprequestservice.AddOrUpdateMobilePickUpRequestsMultipleShipments(pickuprequest, waybillList);

                        //Add tracking history
                        foreach (var waybill in waybillList)
                        {
                            await ScanMobileShipment(new ScanDTO
                            {
                                WaybillNumber = waybill,
                                ShipmentScanStatus = ShipmentScanStatus.MAPT
                            });
                        }

                        //update the rider status
                        await UpdateActivityStatus(pickuprequest.UserId, ActivityStatus.OnDelivery);

                        //Update the country detail for teh return data
                        var country = await _uow.Country.GetCountryByStationId(newPreShipmentDTO.FirstOrDefault().SenderStationId);

                        if (country != null)
                        {
                            newPreShipmentDTO.ForEach(x => x.CurrencyCode = country.CurrencyCode);
                            newPreShipmentDTO.ForEach(x => x.CurrencySymbol = country.CurrencySymbol);
                        }
                    }
                    else
                    {
                        throw new GenericException($"Shipment has already been accepted..");
                    }

                    return newPreShipmentDTO;
                }
                else
                {
                    throw new GenericException("Group does not exist");
                }
            }
            catch (Exception) { throw; }
        }

        private async Task UpdateActivityStatus(string userId, ActivityStatus activity)
        {
            var partner = await _uow.Partner.GetAsync(x => x.UserId == userId);
            if (partner != null)
            {
                partner.ActivityStatus = activity;
                partner.ActivityDate = DateTime.Now;
            }
            await _uow.CompleteAsync();
        }

        public async Task<bool> UpdateMobilePickupRequest(MobilePickUpRequestsDTO pickuprequest)
        {
            try
            {
                var userId = await _userService.GetCurrentUserId();
                pickuprequest.UserId = userId;

                await _mobilepickuprequestservice.UpdateMobilePickUpRequests(pickuprequest, userId);

                if (pickuprequest.Status == MobilePickUpRequestStatus.Confirmed.ToString())
                {
                    await ConfirmMobilePickupRequest(pickuprequest, userId);
                }

                else if (pickuprequest.Status == MobilePickUpRequestStatus.Delivered.ToString())
                {
                    await DeliveredMobilePickupRequest(pickuprequest, userId);
                    await UpdateActivityStatus(pickuprequest.UserId, ActivityStatus.OffDelivery);

                }

                else if (pickuprequest.Status == MobilePickUpRequestStatus.Cancelled.ToString())
                {
                    await ScanMobileShipment(new ScanDTO
                    {
                        WaybillNumber = pickuprequest.Waybill,
                        ShipmentScanStatus = ShipmentScanStatus.SSC
                    });

                    await UpdateActivityStatus(pickuprequest.UserId, ActivityStatus.OffDelivery);

                }

                else if (pickuprequest.Status == MobilePickUpRequestStatus.Dispute.ToString())
                {
                    var preshipmentmobile = await _uow.PreShipmentMobile.GetAsync(s => s.Waybill == pickuprequest.Waybill);
                    preshipmentmobile.shipmentstatus = MobilePickUpRequestStatus.Dispute.ToString();
                    pickuprequest.Status = MobilePickUpRequestStatus.Dispute.ToString();
                    await _mobilepickuprequestservice.UpdateMobilePickUpRequests(pickuprequest, userId);
                    await _uow.CompleteAsync();
                }
                else if (pickuprequest.Status == MobilePickUpRequestStatus.Rejected.ToString())
                {
                    var preshipmentmobile = await _uow.PreShipmentMobile.GetAsync(s => s.Waybill == pickuprequest.Waybill);
                    preshipmentmobile.shipmentstatus = MobilePickUpRequestStatus.Processing.ToString();
                    await UpdateActivityStatus(pickuprequest.UserId, ActivityStatus.OffDelivery);
                    await _uow.CompleteAsync();
                }

                else if (pickuprequest.Status == MobilePickUpRequestStatus.LogVisit.ToString())
                {
                    await LogVisitMobilePickupRequest(pickuprequest, userId);
                    await UpdateActivityStatus(pickuprequest.UserId, ActivityStatus.OffDelivery);
                }

                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<bool> UpdateMobilePickupRequestUsingGroupCode(MobilePickUpRequestsDTO pickuprequest)
        {
            try
            {
                if (pickuprequest == null)
                {
                    throw new GenericException("Pick Up Request is Null");
                }

                if (string.IsNullOrEmpty(pickuprequest.GroupCodeNumber))
                {
                    throw new GenericException("Group Code can not be null");
                }

                bool result = false;
                var userId = await _userService.GetCurrentUserId();
                pickuprequest.UserId = userId;

                string rejected = MobilePickUpRequestStatus.Rejected.ToString();
                string enrouteToPickUp = MobilePickUpRequestStatus.EnrouteToPickUp.ToString();
                string arrived = MobilePickUpRequestStatus.Arrived.ToString();
                string cancelled = MobilePickUpRequestStatus.Cancelled.ToString();
                string logVisit = MobilePickUpRequestStatus.LogVisit.ToString();

                if (pickuprequest.Status == rejected || pickuprequest.Status == enrouteToPickUp || pickuprequest.Status == arrived || pickuprequest.Status == cancelled || pickuprequest.Status == logVisit)
                {
                    var groupList = await _uow.MobileGroupCodeWaybillMapping.FindAsync(x => x.GroupCodeNumber == pickuprequest.GroupCodeNumber);
                    if (groupList == null)
                    {
                        throw new GenericException("Group does not exist");
                    }
                    else
                    {
                        var waybillHashSet = new HashSet<string>();

                        foreach (var item in groupList)
                        {
                            if (item.WaybillNumber != null)
                            {
                                waybillHashSet.Add(item.WaybillNumber);
                            }
                        }
                        List<string> waybillList = waybillHashSet.ToList();

                        //you can use loop for this.  
                        if (pickuprequest.Status == cancelled)
                        {
                            _mobilepickuprequestservice.UpdateMobilePickUpRequestsForWaybillList(waybillList, pickuprequest.UserId, pickuprequest.Status);

                            foreach (var waybill in waybillList)
                            {
                                await ScanMobileShipment(new ScanDTO
                                {
                                    WaybillNumber = waybill,
                                    ShipmentScanStatus = ShipmentScanStatus.SSC
                                });
                            }

                            await UpdateActivityStatus(pickuprequest.UserId, ActivityStatus.OffDelivery);
                        }
                        else if (pickuprequest.Status == rejected)
                        {
                            _mobilepickuprequestservice.UpdateMobilePickUpRequestsForWaybillList(waybillList, pickuprequest.UserId, pickuprequest.Status);

                            var preshipmentmobile = _uow.PreShipmentMobile.GetAllAsQueryable().Where(s => waybillList.Contains(s.Waybill)).ToList();
                            if (preshipmentmobile.Any())
                            {
                                preshipmentmobile.ForEach(u => u.shipmentstatus = MobilePickUpRequestStatus.Processing.ToString());
                            }

                            await UpdateActivityStatus(pickuprequest.UserId, ActivityStatus.OffDelivery);
                            //await _mobilepickuprequestservice.UpdatePreShipmentMobileStatus(waybillList, MobilePickUpRequestStatus.Processing.ToString());
                        }
                        else if (pickuprequest.Status == logVisit)
                        {
                            await LogVisitMobilePickupRequestByGroup(waybillList, pickuprequest.UserId);
                        }
                        else
                        {
                            _mobilepickuprequestservice.UpdateMobilePickUpRequestsForWaybillList(waybillList, pickuprequest.UserId, pickuprequest.Status);

                            if (pickuprequest.Status == enrouteToPickUp || pickuprequest.Status == arrived)
                            {
                                await UpdateActivityStatus(pickuprequest.UserId, ActivityStatus.OnDelivery);
                            }
                        }

                        await _uow.CompleteAsync();
                        result = true;
                    }
                }
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> UpdateMobilePickupRequestUsingWaybill(MobilePickUpRequestsDTO pickuprequest)
        {
            try
            {
                if (pickuprequest == null)
                {
                    throw new GenericException("Pick Up Request is Null");
                }

                if (string.IsNullOrEmpty(pickuprequest.Waybill))
                {
                    throw new GenericException("Waybill can not be null");
                }

                bool result = false;
                var userId = await _userService.GetCurrentUserId();
                pickuprequest.UserId = userId;

                string delivered = MobilePickUpRequestStatus.Delivered.ToString();
                string dispute = MobilePickUpRequestStatus.Dispute.ToString();
                string confirmed = MobilePickUpRequestStatus.Confirmed.ToString();

                if (pickuprequest.Status == delivered || pickuprequest.Status == dispute || pickuprequest.Status == confirmed)
                {
                    //you can use loop for this.  
                    if (pickuprequest.Status == confirmed)
                    {
                        await _mobilepickuprequestservice.UpdateMobilePickUpRequests(pickuprequest, userId);
                        await ConfirmMobilePickupRequest(pickuprequest, userId);
                    }

                    if (pickuprequest.Status == delivered)
                    {
                        //await _mobilepickuprequestservice.UpdateMobilePickUpRequests(pickuprequest, userId);
                        await DeliveredMobilePickupRequest(pickuprequest, userId);
                        await UpdateActivityStatus(pickuprequest.UserId, ActivityStatus.OffDelivery);
                    }

                    if (pickuprequest.Status == dispute)
                    {
                        //use the method I mentioned to update shipment details
                        var preshipmentmobile = await _uow.PreShipmentMobile.GetAsync(s => s.Waybill == pickuprequest.Waybill);
                        preshipmentmobile.shipmentstatus = MobilePickUpRequestStatus.Dispute.ToString();
                        await _uow.CompleteAsync();
                    }
                    result = true;
                }
                return result;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task ConfirmMobilePickupRequest(MobilePickUpRequestsDTO pickuprequest, string userId)
        {
            try
            {
                var preshipmentmobile = await _uow.PreShipmentMobile.GetAsync(s => s.Waybill == pickuprequest.Waybill, "PreShipmentItems,SenderLocation,ReceiverLocation");
                if (preshipmentmobile == null)
                {
                    throw new GenericException("Shipment item does not exist");
                }

                int destinationServiceCentreId = 0;
                int departureServiceCentreId = 0;

                //shipment witin a state 
                if (preshipmentmobile.ZoneMapping == 1)
                {
                    var gigGOServiceCentre = await _userService.GetGIGGOServiceCentre();
                    destinationServiceCentreId = gigGOServiceCentre.ServiceCentreId;
                    departureServiceCentreId = gigGOServiceCentre.ServiceCentreId;
                }
                else
                {
                    //shipment outside a state -- Inter State Shipment
                    var DepartureStation = await _uow.Station.GetAsync(s => s.StationId == preshipmentmobile.SenderStationId);
                    departureServiceCentreId = DepartureStation.SuperServiceCentreId;

                    var DestinationStation = await _uow.Station.GetAsync(s => s.StationId == preshipmentmobile.ReceiverStationId);
                    destinationServiceCentreId = DestinationStation.SuperServiceCentreId;
                }

                var CustomerId = await _uow.IndividualCustomer.GetAsync(s => s.CustomerCode == preshipmentmobile.CustomerCode);

                int customerid = 0;
                if (CustomerId != null)
                {
                    customerid = CustomerId.IndividualCustomerId;
                }
                else
                {
                    var companyid = await _uow.Company.GetAsync(s => s.CustomerCode == preshipmentmobile.CustomerCode);
                    customerid = companyid.CompanyId;
                }

                var MobileShipment = new ShipmentDTO
                {
                    Waybill = preshipmentmobile.Waybill,
                    ReceiverName = preshipmentmobile.ReceiverName,
                    ReceiverPhoneNumber = preshipmentmobile.ReceiverPhoneNumber,
                    ReceiverEmail = preshipmentmobile.ReceiverEmail,
                    ReceiverAddress = preshipmentmobile.ReceiverAddress,
                    DeliveryOptionId = 1,
                    GrandTotal = preshipmentmobile.GrandTotal,
                    Insurance = preshipmentmobile.InsuranceValue,
                    Vat = preshipmentmobile.Vat,
                    SenderAddress = preshipmentmobile.SenderAddress,
                    IsCashOnDelivery = false,
                    CustomerCode = preshipmentmobile.CustomerCode,
                    DestinationServiceCentreId = destinationServiceCentreId,
                    DepartureServiceCentreId = departureServiceCentreId,
                    CustomerId = customerid,
                    UserId = userId,
                    PickupOptions = PickupOptions.HOMEDELIVERY,
                    IsdeclaredVal = preshipmentmobile.IsdeclaredVal,
                    ShipmentPackagePrice = preshipmentmobile.GrandTotal,
                    ApproximateItemsWeight = 0.00,
                    ReprintCounterStatus = false,
                    CustomerType = preshipmentmobile.CustomerType,
                    CompanyType = preshipmentmobile.CompanyType,
                    Value = preshipmentmobile.Value,
                    PaymentStatus = PaymentStatus.Paid,
                    IsFromMobile = true,
                    ShipmentItems = preshipmentmobile.PreShipmentItems.Select(s => new ShipmentItemDTO
                    {
                        Description = s.Description,
                        IsVolumetric = s.IsVolumetric,
                        Weight = s.Weight,
                        Nature = s.ItemType,
                        Price = (decimal)s.CalculatedPrice,
                        Quantity = s.Quantity

                    }).ToList()
                };
                var status = await _shipmentService.AddShipmentFromMobile(MobileShipment);
                preshipmentmobile.shipmentstatus = MobilePickUpRequestStatus.PickedUp.ToString();
                preshipmentmobile.IsConfirmed = true;

                await _uow.CompleteAsync();

                await ScanMobileShipment(new ScanDTO
                {
                    WaybillNumber = pickuprequest.Waybill,
                    ShipmentScanStatus = ShipmentScanStatus.MSHC
                });

                //var item = Mapper.Map<PreShipmentMobileDTO>(preshipmentmobile);
                //await CheckDeliveryTimeAndSendMail(item);

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task DeliveredMobilePickupRequest(MobilePickUpRequestsDTO pickuprequest, string userId)
        {
            try
            {
                var preshipmentmobile = await _uow.PreShipmentMobile.GetAsync(s => s.Waybill == pickuprequest.Waybill, "PreShipmentItems,SenderLocation,ReceiverLocation");
                if (preshipmentmobile == null)
                {
                    throw new GenericException("Shipment item does not exist");
                }

                preshipmentmobile.IsDelivered = true;

                if (preshipmentmobile.ZoneMapping == 1)
                {
                    decimal shipmentPrice = preshipmentmobile.GrandTotal;

                    var gigGoPromoPrice = await CalculatePromoPriceForDipatchRider(preshipmentmobile);
                    if (gigGoPromoPrice > 0)
                    {
                        shipmentPrice = gigGoPromoPrice;
                    }

                    var Partnerpaymentfordelivery = new PartnerPayDTO
                    {
                        ShipmentPrice = shipmentPrice,
                        ZoneMapping = (int)preshipmentmobile.ZoneMapping
                    };

                    decimal price = await _partnertransactionservice.GetPriceForPartner(Partnerpaymentfordelivery);
                    var partneruser = await _userService.GetUserById(userId);
                    var wallet = await _uow.Wallet.GetAsync(s => s.CustomerCode == partneruser.UserChannelCode);
                    preshipmentmobile.shipmentstatus = MobilePickUpRequestStatus.Delivered.ToString();
                    if (wallet != null)
                    {
                        wallet.Balance = wallet.Balance + price;
                    }

                    var partnertransactions = new PartnerTransactionsDTO
                    {
                        Destination = preshipmentmobile.ReceiverAddress,
                        Departure = preshipmentmobile.SenderAddress,
                        AmountReceived = price,
                        Waybill = preshipmentmobile.Waybill
                    };

                    await _uow.CompleteAsync();

                    var id = await _partnertransactionservice.AddPartnerPaymentLog(partnertransactions);
                    var defaultServiceCenter = await _userService.GetGIGGOServiceCentre();
                    var transaction = new WalletTransactionDTO
                    {
                        WalletId = wallet.WalletId,
                        CreditDebitType = CreditDebitType.Credit,
                        Amount = price,
                        ServiceCentreId = defaultServiceCenter.ServiceCentreId,
                        Waybill = preshipmentmobile.Waybill,
                        Description = "Credit for Delivered Shipment Request",
                        PaymentType = PaymentType.Online,
                        UserId = userId
                    };
                    var walletTransaction = await _walletTransactionService.AddWalletTransaction(transaction);

                    await ScanMobileShipment(new ScanDTO
                    {
                        WaybillNumber = pickuprequest.Waybill,
                        ShipmentScanStatus = ShipmentScanStatus.MAHD
                    });

                    var messageextensionDTO = new MobileMessageDTO()
                    {
                        SenderName = preshipmentmobile.ReceiverName,
                        WaybillNumber = preshipmentmobile.Waybill,
                        SenderPhoneNumber = preshipmentmobile.ReceiverPhoneNumber
                    };
                    await _messageSenderService.SendMessage(MessageType.OKC, EmailSmsType.SMS, messageextensionDTO);
                }
                else
                {
                    if (preshipmentmobile.shipmentstatus == MobilePickUpRequestStatus.OnwardProcessing.ToString())
                    {
                        var Pickuprice = await GetPickUpPrice(preshipmentmobile.VehicleType, preshipmentmobile.CountryId, preshipmentmobile.UserId = null);
                        pickuprequest.Status = MobilePickUpRequestStatus.Delivered.ToString();
                        await _mobilepickuprequestservice.UpdateMobilePickUpRequests(pickuprequest, userId);

                        var Partner = new PartnerPayDTO
                        {
                            ShipmentPrice = preshipmentmobile.GrandTotal,
                            PickUprice = Pickuprice
                        };
                        decimal price = await _partnertransactionservice.GetPriceForPartner(Partner);
                        var partneruser = await _userService.GetUserById(userId);
                        var wallet = await _uow.Wallet.GetAsync(s => s.CustomerCode == partneruser.UserChannelCode);
                        wallet.Balance = wallet.Balance + price;
                        var partnertransactions = new PartnerTransactionsDTO
                        {
                            Destination = preshipmentmobile.ReceiverAddress,
                            Departure = preshipmentmobile.SenderAddress,
                            AmountReceived = price,
                            Waybill = preshipmentmobile.Waybill,
                            UserId = userId

                        };
                        await _partnertransactionservice.AddPartnerPaymentLog(partnertransactions);

                        var defaultServiceCenter = await _userService.GetGIGGOServiceCentre();
                        var transaction = new WalletTransactionDTO
                        {
                            WalletId = wallet.WalletId,
                            CreditDebitType = CreditDebitType.Credit,
                            Amount = price,
                            ServiceCentreId = defaultServiceCenter.ServiceCentreId,
                            Waybill = preshipmentmobile.Waybill,
                            Description = "Credit for Delivered Shipment Request",
                            PaymentType = PaymentType.Online,
                            UserId = userId
                        };
                        var walletTransaction = await _walletTransactionService.AddWalletTransaction(transaction);
                        return;
                    }
                    else
                    {
                        pickuprequest.Status = MobilePickUpRequestStatus.Confirmed.ToString();
                        await _mobilepickuprequestservice.UpdateMobilePickUpRequests(pickuprequest, userId);
                        throw new GenericException("This is an interstate delivery, drop at assigned service centre!!");
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task LogVisitMobilePickupRequest(MobilePickUpRequestsDTO pickuprequest, string userId)
        {
            try
            {
                var mobileRequest = await _uow.MobilePickUpRequests.GetAsync(s => s.Waybill == pickuprequest.Waybill);
                if (mobileRequest == null)
                {
                    throw new GenericException("Shipment item does not exist in Pickup");
                }
                else
                {
                    mobileRequest.Status = MobilePickUpRequestStatus.Visited.ToString();
                }
                var preshipmentMobile = await _uow.PreShipmentMobile.GetAsync(s => s.Waybill == pickuprequest.Waybill);
                if (preshipmentMobile == null)
                {
                    throw new GenericException("Shipment item does not exist");
                }
                preshipmentMobile.shipmentstatus = MobilePickUpRequestStatus.Visited.ToString();

                await _uow.CompleteAsync();

                var user = await _userService.GetUserByChannelCode(preshipmentMobile.CustomerCode);

                var emailMessageExtensionDTO = new MobileMessageDTO()
                {
                    SenderName = user.FirstName + " " + user.LastName,
                    SenderEmail = user.Email,
                    WaybillNumber = preshipmentMobile.Waybill,
                    SenderPhoneNumber = preshipmentMobile.SenderPhoneNumber
                };

                var smsMessageExtensionDTO = new MobileMessageDTO()
                {
                    SenderName = preshipmentMobile.ReceiverName,
                    WaybillNumber = preshipmentMobile.Waybill,
                    SenderPhoneNumber = preshipmentMobile.ReceiverPhoneNumber
                };

                await _messageSenderService.SendGenericEmailMessage(MessageType.MATD, emailMessageExtensionDTO);
                await _messageSenderService.SendMessage(MessageType.MATD, EmailSmsType.SMS, smsMessageExtensionDTO);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task LogVisitMobilePickupRequestByGroup(List<string> waybills, string userId)
        {
            try
            {
                var mobileRequest = _uow.MobilePickUpRequests.GetAllAsQueryable().Where(s => waybills.Contains(s.Waybill) && s.UserId == userId).ToList();
                if (mobileRequest.Any())
                {
                    mobileRequest.ForEach(u => u.Status = MobilePickUpRequestStatus.Visited.ToString());
                }

                var preshipmentmobile = _uow.PreShipmentMobile.GetAllAsQueryable().Where(s => waybills.Contains(s.Waybill)).ToList();
                if (preshipmentmobile.Any())
                {
                    preshipmentmobile.ForEach(u => u.shipmentstatus = MobilePickUpRequestStatus.Visited.ToString());

                    //update rider status
                    await UpdateActivityStatus(userId, ActivityStatus.OffDelivery);

                    var user = await _userService.GetUserByChannelCode(preshipmentmobile.FirstOrDefault().CustomerCode);

                    var emailMessageExtensionDTO = new MobileMessageDTO()
                    {
                        SenderName = user.FirstName + " " + user.LastName,
                        SenderEmail = user.Email,
                        WaybillNumber = preshipmentmobile.FirstOrDefault().Waybill,
                        SenderPhoneNumber = preshipmentmobile.FirstOrDefault().SenderPhoneNumber
                    };

                    var smsMessageExtensionDTO = new MobileMessageDTO()
                    {
                        SenderName = preshipmentmobile.FirstOrDefault().ReceiverName,
                        WaybillNumber = preshipmentmobile.FirstOrDefault().Waybill,
                        SenderPhoneNumber = preshipmentmobile.FirstOrDefault().ReceiverPhoneNumber
                    };

                    await _messageSenderService.SendGenericEmailMessage(MessageType.MATD, emailMessageExtensionDTO);
                    await _messageSenderService.SendMessage(MessageType.MATD, EmailSmsType.SMS, smsMessageExtensionDTO);
                }

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<MobilePickUpRequestsDTO>> GetMobilePickupRequest()
        {
            try
            {
                var mobilerequests = await _mobilepickuprequestservice.GetAllMobilePickUpRequests();
                return mobilerequests;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> UpdatePreShipmentMobileDetails(List<PreShipmentItemMobileDTO> preshipmentmobile)
        {
            try
            {
                //Get Price

                foreach (var preShipment in preshipmentmobile)
                {
                    var preshipment = await _uow.PreShipmentItemMobile.GetAsync(s => s.PreShipmentMobileId == preShipment.PreShipmentMobileId && s.PreShipmentItemMobileId == preShipment.PreShipmentItemMobileId);
                    if (preShipment != null)
                    {
                        preshipment.CalculatedPrice = preShipment.CalculatedPrice;
                        preshipment.Description = preShipment.Description;
                        preshipment.EstimatedPrice = preShipment.EstimatedPrice;
                        preshipment.ImageUrl = preShipment.ImageUrl;
                        preshipment.IsVolumetric = preShipment.IsVolumetric;
                        preshipment.ItemName = preShipment.ItemName;
                        preshipment.ItemType = preShipment.ItemType;
                        preshipment.Length = preShipment.Length;
                        preshipment.Quantity = preShipment.Quantity;
                        preshipment.Weight = (double)preShipment.Weight;
                        preshipment.Width = preShipment.Width;
                        preshipment.Height = preShipment.Height;
                    }
                }
                await _uow.CompleteAsync();
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<SpecialResultDTO> GetSpecialPackages()
        {
            try
            {
                var packages = await GetSpecialDomesticPackages();
                var Categories = await GetCategories();
                var Subcategories = await GetSubCategories();
                var dictionaryCategories = await GetDictionaryCategories(Categories, Subcategories);
                var WeightDictionaryCategories = await GetWeightRangeDictionaryCategories(Categories, Subcategories);
                var result = new SpecialResultDTO
                {
                    Specialpackages = packages,
                    Categories = Categories,
                    SubCategories = Subcategories,
                    DictionaryCategory = dictionaryCategories,
                    WeightRangeDictionaryCategory = WeightDictionaryCategories
                };
                return result;
            }
            catch (Exception)
            {
                throw new GenericException("Please an error occurred while trying to get all special packages.");
            }
        }

        private async Task<Dictionary<string, List<string>>> GetDictionaryCategories(List<CategoryDTO> categories, List<SubCategoryDTO> subcategories)
        {
            try
            {
                Dictionary<string, List<string>> finalDictionary = new Dictionary<string, List<string>>
                {
                    //1. category
                    { "CATEGORY", categories.Select(s => s.CategoryName).OrderBy(s => s).ToList() }
                };

                //2. subcategory
                //var Subcategories = await GetSubCategories();
                foreach (var category in categories)
                {
                    var listOfSubcategory = subcategories.Where(s => s.Category.CategoryId == category.CategoryId).
                        Select(s => s.SubCategoryName).Distinct().ToList();

                    //add to dictionary
                    finalDictionary.Add(category.CategoryName, listOfSubcategory);
                }

                //3. subsubcategory
                //var specialDomesticPackages = await GetSpecialDomesticPackages();
                foreach (var subcategory in subcategories)
                {
                    var list = new List<decimal>();
                    var listOfWeights = subcategories.Where(s => s.SubCategoryName == subcategory.SubCategoryName).
                        Select(s => s.Weight.ToString()).ToList();

                    //add to dictionary
                    if (!finalDictionary.ContainsKey(subcategory.SubCategoryName))
                    {
                        finalDictionary.Add(subcategory.SubCategoryName, listOfWeights);
                    }
                }
                return await Task.FromResult(finalDictionary);
            }
            catch (Exception)
            {
                throw new GenericException("Please an error occurred while trying to get the categorization of special packages.");
            }
        }

        private async Task<Dictionary<string, List<string>>> GetDictionaryCategories()
        {
            try
            {
                Dictionary<string, List<string>> finalDictionary = new Dictionary<string, List<string>>();

                //1. category
                var Categories = await GetCategories();
                finalDictionary.Add("CATEGORY", Categories.Select(s => s.CategoryName).OrderBy(s => s).ToList());

                //2. subcategory
                var Subcategories = await GetSubCategories();
                foreach (var category in Categories)
                {
                    var listOfSubcategory = Subcategories.Where(s => s.Category.CategoryId == category.CategoryId).
                        Select(s => s.SubCategoryName).Distinct().ToList();

                    //add to dictionary
                    finalDictionary.Add(category.CategoryName, listOfSubcategory);
                }


                //3. subsubcategory
                //var specialDomesticPackages = await GetSpecialDomesticPackages();
                foreach (var subcategory in Subcategories)
                {
                    var list = new List<decimal>();
                    var listOfWeights = Subcategories.Where(s => s.SubCategoryName == subcategory.SubCategoryName).
                        Select(s => s.Weight.ToString()).ToList();

                    //add to dictionary
                    if (!finalDictionary.ContainsKey(subcategory.SubCategoryName))
                    {
                        finalDictionary.Add(subcategory.SubCategoryName, listOfWeights);
                    }
                }
                return finalDictionary;
            }
            catch (Exception)
            {
                throw new GenericException("Please an error occurred while trying to get the categorization of special packages.");
            }
        }

        private async Task<Dictionary<string, List<string>>> GetWeightRangeDictionaryCategories(List<CategoryDTO> categories, List<SubCategoryDTO> subcategories)
        {
            try
            {
                Dictionary<string, List<string>> finalDictionary = new Dictionary<string, List<string>>
                {
                    //1. category
                    { "CATEGORY", categories.Select(s => s.CategoryName).OrderBy(s => s).ToList() }
                };

                //2. subcategory
                foreach (var category in categories)
                {
                    var listOfSubcategory = subcategories.Where(s => s.Category.CategoryId == category.CategoryId).
                        Select(s => s.SubCategoryName).Distinct().ToList();

                    //add to dictionary
                    finalDictionary.Add(category.CategoryName, listOfSubcategory);
                }

                //3. subsubcategory
                foreach (var subcategory in subcategories)
                {
                    var list = new List<decimal>();
                    var listOfWeights = subcategories.Where(s => s.SubCategoryName == subcategory.SubCategoryName).
                        Select(s => s.WeightRange).ToList();

                    //add to dictionary
                    if (!finalDictionary.ContainsKey(subcategory.SubCategoryName))
                    {
                        finalDictionary.Add(subcategory.SubCategoryName, listOfWeights);
                    }
                }
                return await Task.FromResult(finalDictionary);
            }
            catch (Exception)
            {
                throw new GenericException("Please an error occurred while trying to get the categorization of special packages.");
            }
        }

        private async Task<Dictionary<string, List<string>>> GetWeightRangeDictionaryCategories()
        {
            try
            {
                //
                Dictionary<string, List<string>> finalDictionary = new Dictionary<string, List<string>>();

                //1. category
                var Categories = await GetCategories();
                finalDictionary.Add("CATEGORY", Categories.Select(s => s.CategoryName).OrderBy(s => s).ToList());


                //2. subcategory
                var Subcategories = await GetSubCategories();
                foreach (var category in Categories)
                {
                    var listOfSubcategory = Subcategories.Where(s => s.Category.CategoryId == category.CategoryId).
                        Select(s => s.SubCategoryName).Distinct().ToList();

                    //add to dictionary
                    finalDictionary.Add(category.CategoryName, listOfSubcategory);

                }

                //3. subsubcategory
                //var specialDomesticPackages = await GetSpecialDomesticPackages();
                foreach (var subcategory in Subcategories)
                {
                    var list = new List<decimal>();
                    var listOfWeights = Subcategories.Where(s => s.SubCategoryName == subcategory.SubCategoryName).
                        Select(s => s.WeightRange.ToString()).ToList();

                    //add to dictionary
                    if (!finalDictionary.ContainsKey(subcategory.SubCategoryName))
                    {
                        finalDictionary.Add(subcategory.SubCategoryName, listOfWeights);
                    }
                }
                return finalDictionary;
            }
            catch (Exception)
            {
                throw new GenericException("Please an error occurred while trying to get the categorization of special packages.");
            }
        }

        public async Task<List<PreShipmentMobileDTO>> GetDisputePreShipment()
        {
            try
            {
                var user = await _userService.GetCurrentUserId();
                var shipments = await _uow.PreShipmentMobile.FindAsync(s => s.UserId == user && s.shipmentstatus == MobilePickUpRequestStatus.Dispute.ToString(), "PreShipmentItems");
                var shipment = shipments.OrderByDescending(s => s.DateCreated);
                var newPreShipment = Mapper.Map<List<PreShipmentMobileDTO>>(shipment);
                foreach (var Shipment in newPreShipment)
                {
                    var country = await _uow.Country.GetCountryByStationId(Shipment.SenderStationId);
                    Shipment.CurrencyCode = country.CurrencyCode;
                    Shipment.CurrencySymbol = country.CurrencySymbol;
                }
                return newPreShipment;
            }
            catch (Exception)
            {
                throw new GenericException("Please an error occurred while trying to get shipments in dispute.");
            }
        }
        public async Task<SummaryTransactionsDTO> GetPartnerWalletTransactions()
        {
            try
            {
                string currencyCode = "";
                string currencySymbol = "";
                var user = await _userService.GetCurrentUserId();
                var transactions = await _uow.PartnerTransactions.GetPartnerTransactionByUser(user);
                var partneruser = await _userService.GetUserById(user);
                var wallet = await _uow.Wallet.GetAsync(s => s.CustomerCode == partneruser.UserChannelCode);
                var country = await _uow.Country.GetAsync(s => s.CountryId == partneruser.UserActiveCountryId);
                if (country != null)
                {
                    currencyCode = country.CurrencyCode;
                    currencySymbol = country.CurrencySymbol;
                }

                var summary = new SummaryTransactionsDTO
                {
                    CurrencySymbol = currencySymbol,
                    CurrencyCode = currencyCode,
                    WalletBalance = wallet.Balance,
                    Transactions = transactions
                };

                return summary;
            }
            catch (Exception)
            {
                throw new GenericException("Please an error occurred while trying to get partner's transactions.");
            }
        }

        public async Task<object> ResolveDisputeForMobile(PreShipmentMobileDTO preShipment)
        {
            try
            {
                var preshipmentmobilegrandtotal = await _uow.PreShipmentMobile.GetAsync(s => s.PreShipmentMobileId == preShipment.PreShipmentMobileId);

                if (preshipmentmobilegrandtotal.shipmentstatus == MobilePickUpRequestStatus.PickedUp.ToString() || preshipmentmobilegrandtotal.shipmentstatus == MobilePickUpRequestStatus.Delivered.ToString())
                {
                    throw new GenericException("This Shipment cannot be placed in Dispute, because it has been" + " " + preshipmentmobilegrandtotal.shipmentstatus);
                }

                foreach (var id in preShipment.DeletedItems)
                {
                    var preshipmentitemmobile = await _uow.PreShipmentItemMobile.GetAsync(s => s.PreShipmentItemMobileId == id && s.PreShipmentMobileId == preShipment.PreShipmentMobileId);
                    preshipmentitemmobile.IsCancelled = true;
                    _uow.PreShipmentItemMobile.Remove(preshipmentitemmobile);
                }

                var PreshipmentPriceDTO = await GetPrice(preShipment);
                var difference = (preshipmentmobilegrandtotal.GrandTotal - PreshipmentPriceDTO.GrandTotal);

                foreach (var item in preShipment.PreShipmentItems)
                {
                    var preshipmentitemmobile = await _uow.PreShipmentItemMobile.GetAsync(s => s.PreShipmentItemMobileId == item.PreShipmentItemMobileId && s.PreShipmentMobileId == preShipment.PreShipmentMobileId);
                    preshipmentitemmobile.Quantity = item.Quantity;
                    preshipmentitemmobile.Value = item.Value;
                    preshipmentitemmobile.Weight = (double)item.Weight;
                    preshipmentitemmobile.Description = item.Description;
                    preshipmentitemmobile.Height = item.Height;
                    preshipmentitemmobile.ImageUrl = item.ImageUrl;
                    preshipmentitemmobile.ItemName = item.ItemName;
                    preshipmentitemmobile.Length = item.Length;
                    preshipmentitemmobile.CalculatedPrice = PreshipmentPriceDTO.PreshipmentMobile.PreShipmentItems.Where(x => x.PreShipmentItemMobileId == item.PreShipmentItemMobileId).Select(y => y.CalculatedPrice).FirstOrDefault();
                    if (!string.IsNullOrEmpty(item.Value))
                    {
                        preshipmentmobilegrandtotal.Value = preshipmentmobilegrandtotal.Value + decimal.Parse(item.Value);
                    }
                }

                //update shipment information
                preshipmentmobilegrandtotal.shipmentstatus = MobilePickUpRequestStatus.Resolved.ToString();
                preshipmentmobilegrandtotal.GrandTotal = (decimal)PreshipmentPriceDTO.GrandTotal;
                preshipmentmobilegrandtotal.InsuranceValue = PreshipmentPriceDTO.PreshipmentMobile.InsuranceValue;
                preshipmentmobilegrandtotal.DiscountValue = PreshipmentPriceDTO.PreshipmentMobile.DiscountValue;
                preshipmentmobilegrandtotal.DeliveryPrice = PreshipmentPriceDTO.PreshipmentMobile.DeliveryPrice;
                preshipmentmobilegrandtotal.CalculatedTotal = PreshipmentPriceDTO.PreshipmentMobile.CalculatedTotal;

                var updatedwallet = await _uow.Wallet.GetAsync(s => s.CustomerCode == preShipment.CustomerCode);

                if (difference < 0.00M)
                {
                    var newdiff = Math.Abs((decimal)difference);
                    if (newdiff > updatedwallet.Balance)
                    {
                        throw new GenericException("Insufficient Wallet Balance");
                    }
                }

                updatedwallet.Balance = updatedwallet.Balance + (decimal)difference;

                var pickuprequests = await _uow.MobilePickUpRequests.GetAsync(s => s.Waybill == preshipmentmobilegrandtotal.Waybill && s.Status == MobilePickUpRequestStatus.Dispute.ToString());
                if (pickuprequests != null)
                {
                    pickuprequests.Status = MobilePickUpRequestStatus.Resolved.ToString();
                }
                await _uow.CompleteAsync();
                return new { IsResolved = true };
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<object> ResolveDisputeForMultipleShipments(PreShipmentMobileDTO preShipment)
        {
            try
            {
                var preshipmentmobilegrandtotal = await _uow.PreShipmentMobile.GetAsync(s => s.PreShipmentMobileId == preShipment.PreShipmentMobileId);
                var pickupPrice = preshipmentmobilegrandtotal.ShipmentPickupPrice;

                if (preshipmentmobilegrandtotal.shipmentstatus == MobilePickUpRequestStatus.PickedUp.ToString() || preshipmentmobilegrandtotal.shipmentstatus == MobilePickUpRequestStatus.Delivered.ToString())
                {
                    throw new GenericException("This Shipment cannot be placed in Dispute, because it has been" + " " + preshipmentmobilegrandtotal.shipmentstatus);
                }

                foreach (var id in preShipment.DeletedItems)
                {
                    var preshipmentitemmobile = await _uow.PreShipmentItemMobile.GetAsync(s => s.PreShipmentItemMobileId == id && s.PreShipmentMobileId == preShipment.PreShipmentMobileId);
                    preshipmentitemmobile.IsCancelled = true;
                    _uow.PreShipmentItemMobile.Remove(preshipmentitemmobile);
                }

                foreach (var item in preShipment.PreShipmentItems)
                {
                    var preshipmentitemmobile = await _uow.PreShipmentItemMobile.GetAsync(s => s.PreShipmentItemMobileId == item.PreShipmentItemMobileId && s.PreShipmentMobileId == preShipment.PreShipmentMobileId);
                    preshipmentitemmobile.Quantity = item.Quantity;
                    preshipmentitemmobile.Value = item.Value;
                    preshipmentitemmobile.Weight = (double)item.Weight;
                    preshipmentitemmobile.Description = item.Description;
                    preshipmentitemmobile.Height = item.Height;
                    preshipmentitemmobile.ImageUrl = item.ImageUrl;
                    preshipmentitemmobile.ItemName = item.ItemName;
                    preshipmentitemmobile.Length = item.Length;
                }

                var PreshipmentPriceDTO = await GetPriceForResolveDispute(preShipment, pickupPrice);
                preshipmentmobilegrandtotal.shipmentstatus = MobilePickUpRequestStatus.Resolved.ToString();
                var difference = (preshipmentmobilegrandtotal.GrandTotal - PreshipmentPriceDTO.GrandTotal);

                var updatedwallet = await _uow.Wallet.GetAsync(s => s.CustomerCode == preShipment.CustomerCode);

                if (difference < 0.00M)
                {
                    var newdiff = Math.Abs((decimal)difference);
                    if (newdiff > updatedwallet.Balance)
                    {
                        throw new GenericException("Insufficient Wallet Balance");
                    }
                }

                updatedwallet.Balance = updatedwallet.Balance + (decimal)difference;

                var pickuprequests = await _uow.MobilePickUpRequests.GetAsync(s => s.Waybill == preshipmentmobilegrandtotal.Waybill && s.Status == MobilePickUpRequestStatus.Dispute.ToString());
                if (pickuprequests != null)
                {
                    pickuprequests.Status = MobilePickUpRequestStatus.Resolved.ToString();
                }
                await _uow.CompleteAsync();
                return new { IsResolved = true };
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<object> CancelShipment(string Waybill)
        {
            try
            {
                var preshipmentmobile = await _uow.PreShipmentMobile.GetAsync(s => s.Waybill == Waybill);
                if (preshipmentmobile == null)
                {
                    throw new GenericException("Shipment does not exist");
                }

                var updatedwallet = await _uow.Wallet.GetAsync(s => s.CustomerCode == preshipmentmobile.CustomerCode);

                var pickuprequests = await _uow.MobilePickUpRequests.GetAsync(s => s.Waybill == preshipmentmobile.Waybill && (s.Status != MobilePickUpRequestStatus.Rejected.ToString() || s.Status != MobilePickUpRequestStatus.TimedOut.ToString()));
                if (pickuprequests != null)
                {
                    var pickuprice = await GetPickUpPrice(preshipmentmobile.VehicleType, preshipmentmobile.CountryId, preshipmentmobile.UserId = null);
                    updatedwallet.Balance = ((updatedwallet.Balance + preshipmentmobile.GrandTotal) - Convert.ToDecimal(pickuprice));
                    var Partnersprice = (0.4M * Convert.ToDecimal(pickuprice));

                    var rider = await _userService.GetUserById(pickuprequests.UserId);
                    var riderWallet = await _uow.Wallet.GetAsync(s => s.CustomerCode == rider.UserChannelCode);
                    riderWallet.Balance = riderWallet.Balance + Partnersprice;

                    preshipmentmobile.shipmentstatus = MobilePickUpRequestStatus.Cancelled.ToString();
                    pickuprequests.Status = MobilePickUpRequestStatus.Cancelled.ToString();

                    var partnertransactions = new PartnerTransactionsDTO
                    {
                        Destination = preshipmentmobile.ReceiverAddress,
                        Departure = preshipmentmobile.SenderAddress,
                        AmountReceived = Partnersprice,
                        Waybill = preshipmentmobile.Waybill
                    };
                    await _partnertransactionservice.AddPartnerPaymentLog(partnertransactions);
                }
                else
                {
                    preshipmentmobile.shipmentstatus = MobilePickUpRequestStatus.Cancelled.ToString();
                    updatedwallet.Balance = ((updatedwallet.Balance + preshipmentmobile.GrandTotal));
                }
                await _uow.CompleteAsync();
                return new { IsCancelled = true };
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<object> AddRatings(MobileRatingDTO rating)
        {
            try
            {
                var user = await _userService.GetCurrentUserId();
                var partneruser = await _userService.GetUserById(user);
                var existingrating = await _uow.MobileRating.GetAsync(s => s.Waybill == rating.Waybill);
                if (existingrating != null)
                {
                    if (rating.UserChannelType == UserChannelType.IndividualCustomer.ToString())
                    {
                        if (existingrating.IsRatedByCustomer == true)
                        {
                            throw new GenericException("Customer has rated this Partner already!");
                        }
                        existingrating.CustomerCode = partneruser.UserChannelCode;
                        existingrating.PartnerRating = rating.Rating;
                        existingrating.CommentByCustomer = rating.Comment;
                        existingrating.IsRatedByCustomer = true;
                        existingrating.DateCustomerRated = DateTime.Now;
                    }
                    if (rating.UserChannelType == UserChannelType.Partner.ToString())
                    {
                        if (existingrating.IsRatedByPartner == true)
                        {
                            throw new GenericException("Partner has rated this Customer already!");
                        }
                        existingrating.PartnerCode = partneruser.UserChannelCode;
                        existingrating.CustomerRating = rating.Rating;
                        existingrating.CommentByPartner = rating.Comment;
                        existingrating.IsRatedByPartner = true;
                        existingrating.DatePartnerRated = DateTime.Now;
                    }
                }
                else
                {
                    if (rating.UserChannelType == UserChannelType.IndividualCustomer.ToString())
                    {
                        rating.CustomerCode = partneruser.UserChannelCode;
                        rating.PartnerRating = rating.Rating;
                        rating.CommentByCustomer = rating.Comment;
                        rating.IsRatedByCustomer = true;
                        rating.DateCustomerRated = DateTime.Now;
                    }
                    if (rating.UserChannelType == UserChannelType.Partner.ToString())
                    {
                        rating.PartnerCode = partneruser.UserChannelCode;
                        rating.CustomerRating = rating.Rating;
                        rating.CommentByPartner = rating.Comment;
                        rating.IsRatedByPartner = true;
                        rating.DatePartnerRated = DateTime.Now;
                    }
                    var ratings = Mapper.Map<MobileRating>(rating);
                    _uow.MobileRating.Add(ratings);
                }
                await _uow.CompleteAsync();
                return new { IsRated = true };
            }
            catch (Exception)
            {
                throw;
            }

        }

        public async Task<Partnerdto> GetMonthlyPartnerTransactions()
        {
            try
            {
                var mobilerequests = await _mobilepickuprequestservice.GetMonthlyTransactions();
                return mobilerequests;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> CreateCustomer(string CustomerCode)
        {
            try
            {
                var user = await _userService.GetUserByChannelCode(CustomerCode);
                if (user.UserChannelType != UserChannelType.Ecommerce)
                {
                    var customer = await _uow.IndividualCustomer.GetAsync(s => s.CustomerCode == CustomerCode);
                    if (customer == null)
                    {
                        var customerDTO = new IndividualCustomerDTO
                        {
                            Email = user.Email,
                            PhoneNumber = user.PhoneNumber,
                            FirstName = user.FirstName,
                            LastName = user.LastName,
                            Password = user.Password,
                            CustomerCode = user.UserChannelCode,
                            PictureUrl = user.PictureUrl,
                            userId = user.Id,
                            IsRegisteredFromMobile = true,
                            UserActiveCountryId = user.UserActiveCountryId
                        };
                        var individualCustomer = Mapper.Map<IndividualCustomer>(customerDTO);
                        _uow.IndividualCustomer.Add(individualCustomer);
                        await _uow.CompleteAsync();
                    }
                }

                return true; ;
            }
            catch
            {
                throw new GenericException("An error occurred while trying to create a customer record(L).");
            }
        }

        public async Task<PartnerDTO> CreatePartner(string CustomerCode)
        {
            try
            {
                var partnerDTO = new PartnerDTO();
                var user = await _userService.GetUserByChannelCode(CustomerCode);
                var partner = await _uow.Partner.GetAsync(s => s.PartnerCode == CustomerCode);
                if (partner == null)
                {
                    if (user.SystemUserRole == "Dispatch Rider")
                    {
                        partnerDTO.PartnerType = PartnerType.InternalDeliveryPartner;
                        partnerDTO.IsActivated = true;
                    }
                    else
                    {
                        partnerDTO.PartnerType = PartnerType.DeliveryPartner;
                        partnerDTO.IsActivated = false;
                    }

                    partnerDTO.PartnerName = user.FirstName + "" + user.LastName;
                    partnerDTO.PartnerCode = user.UserChannelCode;
                    partnerDTO.FirstName = user.FirstName;
                    partnerDTO.LastName = user.LastName;
                    partnerDTO.Email = user.Email;
                    partnerDTO.PhoneNumber = user.PhoneNumber;
                    partnerDTO.UserId = user.Id;
                    partnerDTO.UserActiveCountryId = user.UserActiveCountryId;
                    partnerDTO.ActivityDate = DateTime.Now;
                    var FinalPartner = Mapper.Map<Partner>(partnerDTO);
                    _uow.Partner.Add(FinalPartner);
                    await _uow.CompleteAsync();

                    partnerDTO.PartnerId = FinalPartner.PartnerId;
                    return partnerDTO;
                }
                var finalPartner = Mapper.Map<PartnerDTO>(partner);
                return finalPartner;
            }
            catch (Exception ex)
            {
                throw new GenericException($"An error occurred while trying to create a Partner record(L).{ex.Message} {ex}"); ;
            }
        }

        public async Task<bool> UpdateDeliveryNumber(MobileShipmentNumberDTO detail)
        {
            try
            {
                var userId = await _userService.GetCurrentUserId();
                var number = await _uow.DeliveryNumber.GetAsync(s => s.Number.ToLower() == detail.DeliveryNumber.ToLower());
                if (number == null)
                {
                    throw new GenericException("Delivery Number does not exist");
                }
                else
                {
                    if (number.IsUsed == true)
                    {
                        throw new GenericException("Delivery Number has been used ");
                    }
                    else
                    {
                        number.IsUsed = true;
                        number.UserId = userId;
                        var shipment = await _uow.Shipment.GetAsync(s => s.Waybill == detail.WayBill);
                        if (shipment != null)
                        {
                            shipment.DeliveryNumber = detail.DeliveryNumber;
                            await _uow.CompleteAsync();
                        }
                        else
                        {
                            throw new GenericException("Waybill does not exist in Shipments");
                        }
                    }
                }
                return true;
            }
            catch
            {
                throw;
            }
        }
        public async Task<bool> deleterecord(string detail)
        {
            try
            {
                var user = await _userService.GetUserByEmail(detail);
                if (user != null)
                {
                    var wallet = await _uow.Wallet.GetAsync(s => s.CustomerCode == user.UserChannelCode);
                    if (wallet != null)
                    {
                        var transactions = await _uow.WalletTransaction.FindAsync(s => s.WalletId == wallet.WalletId);
                        if (transactions.Count() > 0)
                        {
                            foreach (var transaction in transactions.ToList())
                            {
                                _uow.WalletTransaction.Remove(transaction);
                            }
                        }
                        _uow.Wallet.Remove(wallet);
                    }
                    var userDTO = await _uow.User.Remove(user.Id);
                }
                var Customer = await _uow.IndividualCustomer.GetAsync(s => s.Email == detail);
                if (Customer != null)
                {
                    _uow.IndividualCustomer.Remove(Customer);
                }
                var partner = await _uow.Partner.GetAsync(s => s.Email == detail);
                if (partner != null)
                {
                    var vehicles = await _uow.VehicleType.FindAsync(s => s.Partnercode == partner.PartnerCode);
                    if (vehicles.Count() > 0)
                    {
                        foreach (var vehicle in vehicles)
                        {
                            _uow.VehicleType.Remove(vehicle);
                        }
                    }
                    _uow.Partner.Remove(partner);
                }
                await _uow.CompleteAsync();
                return true; ;
            }
            catch
            {
                throw new GenericException("An error occurred while attempting to delete record.");
            }
        }

        public async Task<bool> VerifyPartnerDetails(PartnerDTO partnerDto)
        {
            try
            {
                //to get images information
                var images = new ImageDTO();
                var Partner = await _uow.Partner.GetAsync(s => s.Email == partnerDto.Email);
                if (Partner != null)
                {
                    Partner.Address = partnerDto.Address;
                    Partner.Email = partnerDto.Email;
                    Partner.FirstName = partnerDto.FirstName;
                    Partner.IsActivated = true;
                    Partner.LastName = partnerDto.LastName;
                    Partner.OptionalPhoneNumber = partnerDto.OptionalPhoneNumber;
                    Partner.PartnerName = partnerDto.PartnerName;
                    Partner.PhoneNumber = partnerDto.PhoneNumber;
                    Partner.PictureUrl = partnerDto.PictureUrl;
                    Partner.AccountName = partnerDto.AccountName;
                    Partner.AccountNumber = partnerDto.AccountNumber;
                    Partner.BankName = partnerDto.BankName;
                    Partner.VehicleLicenseExpiryDate = partnerDto.VehicleLicenseExpiryDate;
                    images.PartnerFullName = partnerDto.FirstName + partnerDto.LastName;
                    if (partnerDto.FleetPartnerCode != null)
                    {
                        Partner.FleetPartnerCode = partnerDto.FleetPartnerCode;
                    }
                    if (partnerDto.VehicleLicenseImageDetails != null && !partnerDto.VehicleLicenseImageDetails.Contains("agilityblob"))
                    {
                        images.FileType = ImageFileType.VehicleLicense;
                        images.ImageString = partnerDto.VehicleLicenseImageDetails;
                        partnerDto.VehicleLicenseImageDetails = await LoadImage(images);
                    }
                    Partner.VehicleLicenseImageDetails = partnerDto.VehicleLicenseImageDetails;
                    Partner.VehicleLicenseNumber = partnerDto.VehicleLicenseNumber;
                    if (partnerDto.VehicleTypeDetails.Count() == 0)
                    {
                        throw new GenericException("Partner does not any Vehicle attached. Kindly review!!");
                    }
                    foreach (var vehicle in partnerDto.VehicleTypeDetails)
                    {
                        if (vehicle.VehicleInsurancePolicyDetails != null && !vehicle.VehicleInsurancePolicyDetails.Contains("agilityblob"))
                        {
                            images.FileType = ImageFileType.VehiceInsurancePolicy;
                            images.ImageString = vehicle.VehicleInsurancePolicyDetails;
                            vehicle.VehicleInsurancePolicyDetails = await LoadImage(images);
                        }

                        if (vehicle.VehicleRoadWorthinessDetails != null && !vehicle.VehicleRoadWorthinessDetails.Contains("agilityblob"))
                        {
                            images.FileType = ImageFileType.VehiceRoadWorthiness;
                            images.ImageString = vehicle.VehicleRoadWorthinessDetails;
                            vehicle.VehicleRoadWorthinessDetails = await LoadImage(images);
                        }

                        if (vehicle.VehicleParticularsDetails != null && !vehicle.VehicleParticularsDetails.Contains("agilityblob"))
                        {
                            images.FileType = ImageFileType.VehicleParticulars;
                            images.ImageString = vehicle.VehicleParticularsDetails;
                            vehicle.VehicleParticularsDetails = await LoadImage(images);
                        }
                        var VehicleDetails = await _uow.VehicleType.GetAsync(s => s.VehicleTypeId == vehicle.VehicleTypeId && s.Partnercode == partnerDto.PartnerCode);
                        if (VehicleDetails != null)
                        {
                            VehicleDetails.VehicleInsurancePolicyDetails = vehicle.VehicleInsurancePolicyDetails;
                            VehicleDetails.VehicleRoadWorthinessDetails = vehicle.VehicleRoadWorthinessDetails;
                            VehicleDetails.VehicleParticularsDetails = vehicle.VehicleParticularsDetails;
                            VehicleDetails.VehiclePlateNumber = vehicle.VehiclePlateNumber;
                            VehicleDetails.Vehicletype = vehicle.Vehicletype;
                            VehicleDetails.IsVerified = vehicle.IsVerified;
                        }
                        else
                        {
                            var Vehicle = Mapper.Map<VehicleType>(vehicle);
                            _uow.VehicleType.Add(Vehicle);
                        }
                    }
                    await _uow.CompleteAsync();
                }
                else
                {
                    throw new GenericException("Partner Information does not exist!");
                }
                return true;
            }
            catch
            {
                throw;
            }
        }

        public async Task<PartnerDTO> GetPartnerDetails(string Email)
        {
            var partnerdto = new PartnerDTO();
            try
            {
                var Partner = await _uow.Partner.GetAsync(s => s.Email == Email);
                if (Partner != null)
                {
                    partnerdto = Mapper.Map<PartnerDTO>(Partner);
                    var VehicleDetails = await _uow.VehicleType.FindAsync(s => s.Partnercode == partnerdto.PartnerCode);
                    if (VehicleDetails != null)
                    {
                        var vehicles = Mapper.Map<List<VehicleTypeDTO>>(VehicleDetails);
                        partnerdto.VehicleTypeDetails = vehicles;
                    }
                }
                else
                {
                    throw new GenericException("Partner Information does not exist!");
                }
                return partnerdto;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> UpdateReceiverDetails(PreShipmentMobileDTO receiver)
        {
            try
            {
                var shipment = await _uow.PreShipmentMobile.GetAsync(s => s.Waybill == receiver.Waybill);
                if (shipment != null)
                {
                    shipment.ActualReceiverFirstName = receiver.ActualReceiverFirstName;
                    shipment.ActualReceiverLastName = receiver.ActualReceiverLastName;
                    shipment.ActualReceiverPhoneNumber = receiver.ActualReceiverPhoneNumber;
                }
                else
                {
                    throw new GenericException("Shipment Information does not exist!");
                }
                await _uow.CompleteAsync();
                return true;
            }
            catch
            {
                throw;
            }
        }

        public async Task<int> GetCountryId()
        {
            int userActiveCountryId = 1;
            return await Task.FromResult(userActiveCountryId);
        }

        public async Task<decimal> GetPickUpPrice(string vehicleType, int CountryId, string UserId)
        {
            try
            {
                var PickUpPrice = 0.0M;
                if (vehicleType != null)
                {
                    if (UserId == null)
                    {
                        PickUpPrice = await GetPickUpPriceForIndividual(vehicleType, CountryId);
                    }
                    else
                    {

                        var user = await _userService.GetUserById(UserId);
                        var exists = await _uow.Company.ExistAsync(s => s.CustomerCode == user.UserChannelCode);
                        if (user.UserChannelType == UserChannelType.Ecommerce || exists)
                        {
                            PickUpPrice = await GetPickUpPriceForEcommerce(vehicleType, CountryId);
                        }
                        else
                        {
                            PickUpPrice = await GetPickUpPriceForIndividual(vehicleType, CountryId);
                        }
                    }

                }
                return PickUpPrice;
            }
            catch
            {
                throw new GenericException("Please an error occurred while getting PickupPrice!!");
            }
        }

        //Get PickUpPrice For Multiple Shipments Flow
        public async Task<decimal> GetPickUpPriceForMultipleShipment(string customerType, string vehicleType, int CountryId)
        {
            try
            {
                var PickUpPrice = 0.0M;
                if (customerType == UserChannelType.Ecommerce.ToString())
                {
                    PickUpPrice = await GetPickUpPriceForEcommerce(vehicleType, CountryId);
                }
                else
                {
                    PickUpPrice = await GetPickUpPriceForIndividual(vehicleType, CountryId);
                }

                return PickUpPrice;
            }
            catch
            {
                throw new GenericException("An error occurred while getting Pickup Price!!!");
            }
        }

        //method for converting a base64 string to an image and saving to Azure
        public async Task<string> LoadImage(ImageDTO images)
        {
            try
            {
                //To get only the base64 string
                var baseString = images.ImageString.Split(',')[1];
                byte[] bytes = Convert.FromBase64String(baseString);
                string filename = images.PartnerFullName + "_" + images.FileType.ToString() + ".png";
                //Save to AzureBlobStorage
                var blobname = await AzureBlobServiceUtil.UploadAsync(bytes, filename);

                return await Task.FromResult(blobname);

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        public async Task<List<Uri>> DisplayImages()
        {
            var result = await AzureBlobServiceUtil.DisplayAll();

            return result;
        }

        public async Task<PreShipmentSummaryDTO> GetShipmentDetailsFromDeliveryNumber(string DeliveryNumber)
        {
            try
            {
                var ShipmentSummaryDetails = new PreShipmentSummaryDTO();
                var result = await _uow.Shipment.GetAsync(s => s.DeliveryNumber == DeliveryNumber || s.Waybill == DeliveryNumber);
                if (result != null)
                {
                    ShipmentSummaryDetails = await GetPartnerDetailsFromWaybill(result.Waybill);
                    var details = await _uow.PreShipmentMobile.GetAsync(s => s.Waybill == result.Waybill, "PreShipmentItems");
                    if (details == null)
                    {
                        throw new GenericException("Shipment cannot be found in PreshipmentMobile");
                    }
                    var PreShipmentdto = Mapper.Map<PreShipmentMobileDTO>(details);
                    ShipmentSummaryDetails.shipmentdetails = PreShipmentdto;
                    return ShipmentSummaryDetails;
                }
                else if (result == null)
                {
                    var details = await _uow.PreShipmentMobile.GetAsync(s => s.Waybill == DeliveryNumber, "PreShipmentItems");
                    if (details == null)
                    {
                        throw new GenericException("Shipment cannot be found in PreshipmentMobile");
                    }
                    else
                    {
                        ShipmentSummaryDetails = await GetPartnerDetailsFromWaybill(details.Waybill);
                        var PreShipmentdto = Mapper.Map<PreShipmentMobileDTO>(details);
                        ShipmentSummaryDetails.shipmentdetails = PreShipmentdto;
                        return ShipmentSummaryDetails;
                    }

                }
                else
                {
                    throw new GenericException("Shipment cannot be found");
                }
            }
            catch (Exception)
            {
                throw;
            }

        }

        public async Task<bool> ApproveShipment(ApproveShipmentDTO detail)
        {
            try
            {
                var preshipmentmobile = await _uow.PreShipmentMobile.GetAsync(s => s.Waybill == detail.WaybillNumber, "PreShipmentItems,SenderLocation,ReceiverLocation");

                if (preshipmentmobile == null)
                {
                    throw new GenericException($"This shipment {detail.WaybillNumber} does not exist, Kindly confirm the waybill again!!!");
                }
                else
                {
                    if (preshipmentmobile.shipmentstatus == MobilePickUpRequestStatus.Cancelled.ToString())
                    {
                        throw new GenericException($"This shipment {detail.WaybillNumber} has been Cancelled. It can not be processed!!!");
                    }
                    else
                    {
                        if (preshipmentmobile.ZoneMapping != 1)
                        {
                            if (preshipmentmobile.shipmentstatus == MobilePickUpRequestStatus.PickedUp.ToString() || preshipmentmobile.shipmentstatus == MobilePickUpRequestStatus.OnwardProcessing.ToString())
                            {
                                if (preshipmentmobile.IsApproved != true)
                                {
                                    var CustomerId = await _uow.IndividualCustomer.GetAsync(s => s.CustomerCode == preshipmentmobile.CustomerCode);

                                    int customerid = 0;
                                    if (CustomerId != null)
                                    {
                                        customerid = CustomerId.IndividualCustomerId;
                                    }
                                    else
                                    {
                                        var companyid = await _uow.Company.GetAsync(s => s.CustomerCode == preshipmentmobile.CustomerCode);
                                        customerid = companyid.CompanyId;
                                    }
                                    int departureCountryId = await GetCountryByServiceCentreId(detail.SenderServiceCentreId);
                                    int destinationCountryId = await GetCountryByServiceCentreId(detail.ReceiverServiceCentreId);
                                    var user = await _userService.GetCurrentUserId();
                                    var pickupprice = await GetPickUpPrice(preshipmentmobile.VehicleType, preshipmentmobile.CountryId, preshipmentmobile.UserId);

                                    var MobileShipment = new ShipmentDTO
                                    {
                                        Waybill = preshipmentmobile.Waybill,
                                        ReceiverName = preshipmentmobile.ReceiverName,
                                        ReceiverPhoneNumber = preshipmentmobile.ReceiverPhoneNumber,
                                        ReceiverEmail = preshipmentmobile.ReceiverEmail,
                                        ReceiverAddress = preshipmentmobile.ReceiverAddress,
                                        DeliveryOptionId = 2,
                                        GrandTotal = preshipmentmobile.GrandTotal,
                                        Insurance = preshipmentmobile.InsuranceValue,
                                        Vat = preshipmentmobile.Vat,
                                        SenderAddress = preshipmentmobile.SenderAddress,
                                        IsCashOnDelivery = false,
                                        CustomerCode = preshipmentmobile.CustomerCode,
                                        DestinationServiceCentreId = detail.ReceiverServiceCentreId,
                                        DepartureServiceCentreId = detail.SenderServiceCentreId,
                                        CustomerId = customerid,
                                        UserId = user,
                                        PickupOptions = PickupOptions.HOMEDELIVERY,
                                        IsdeclaredVal = preshipmentmobile.IsdeclaredVal,
                                        ShipmentPackagePrice = preshipmentmobile.GrandTotal,
                                        ApproximateItemsWeight = 0.00,
                                        ReprintCounterStatus = false,
                                        CustomerType = preshipmentmobile.CustomerType,
                                        CompanyType = preshipmentmobile.CompanyType,
                                        Value = preshipmentmobile.Value,
                                        PaymentStatus = PaymentStatus.Paid,
                                        IsFromMobile = true,
                                        ShipmentPickupPrice = pickupprice,
                                        DestinationCountryId = destinationCountryId,
                                        DepartureCountryId = departureCountryId,
                                        ShipmentItems = preshipmentmobile.PreShipmentItems.Select(s => new ShipmentItemDTO
                                        {
                                            Description = s.Description,
                                            IsVolumetric = s.IsVolumetric,
                                            Weight = s.Weight,
                                            Nature = s.ItemType,
                                            Price = (decimal)s.CalculatedPrice,
                                            Quantity = s.Quantity

                                        }).ToList()
                                    };
                                    var status = await _shipmentService.AddShipmentFromMobile(MobileShipment);

                                    preshipmentmobile.shipmentstatus = MobilePickUpRequestStatus.OnwardProcessing.ToString();
                                    preshipmentmobile.IsApproved = true;

                                    //add scan status into Mobiletracking and Shipmenttracking
                                    await ScanMobileShipment(new ScanDTO
                                    {
                                        WaybillNumber = detail.WaybillNumber,
                                        ShipmentScanStatus = ShipmentScanStatus.MSVC
                                    });

                                    await _shipmentService.ScanShipment(new ScanDTO
                                    {
                                        WaybillNumber = detail.WaybillNumber,
                                        ShipmentScanStatus = ShipmentScanStatus.ARO
                                    });
                                    await _uow.CompleteAsync();
                                }
                                else
                                {
                                    throw new GenericException("Shipment has already been approved!!!");
                                }
                            }
                            else
                            {
                                throw new GenericException($"This shipment {detail.WaybillNumber} has not been marked as Picked Up. Delivery Partner should confirm pick up from his app.");
                            }
                        }
                        else
                        {
                            throw new GenericException("This shipment is not an interstate delivery,take to the assigned receiver's location");
                        }
                        return true;
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> CreateCompany(string CustomerCode)
        {
            try
            {
                var user = await _userService.GetUserByChannelCode(CustomerCode);
                var company = await _uow.Company.GetAsync(s => s.CustomerCode == CustomerCode);
                if (company == null)
                {
                    var companydto = new CompanyDTO
                    {
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Name = user.FirstName + " " + user.LastName,
                        CompanyType = CompanyType.Ecommerce,
                        Password = user.Password,
                        CustomerCode = user.UserChannelCode,
                        IsRegisteredFromMobile = true,
                        UserActiveCountryId = user.UserActiveCountryId,
                        IsEligible = true
                    };
                    var Company = Mapper.Map<Company>(companydto);
                    _uow.Company.Add(Company);
                    await _uow.CompleteAsync();
                }
                return true; ;
            }
            catch
            {
                throw new GenericException("An error occurred while trying to create company");
            }
        }

        public async Task<MobilePriceDTO> GetHaulagePrice(HaulagePriceDTO haulagePricingDto)
        {
            try
            {
                decimal price = 0;

                //check haulage exists
                var haulage = await _haulageService.GetHaulageById(haulagePricingDto.Haulageid);
                if (haulage == null)
                {
                    throw new GenericException("The Tonne specified has not been mapped");
                }

                var country = await _uow.Country.GetCountryByStationId(haulagePricingDto.DestinationStationId);
                var IsWithinProcessingTime = await WithinProcessingTime(country.CountryId);
                var DiscountPercent = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.DiscountPercentage, country.CountryId);

                var Percentage = (Convert.ToDecimal(DiscountPercent.Value));
                var PercentageTobeUsed = ((100M - Percentage) / 100M);
                var discount = (1 - PercentageTobeUsed);

                //get the distance based on the stations
                var haulageDistanceMapping = await _haulageDistanceMappingService.GetHaulageDistanceMappingForMobile(haulagePricingDto.DepartureStationId, haulagePricingDto.DestinationStationId);
                int distance = haulageDistanceMapping.Distance;

                //set the default distance to 1
                if (distance == 0)
                {
                    distance = 1;
                }

                //Get Haulage Maximum Fixed Distance
                var maximumFixedDistanceObj = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.HaulageMaximumFixedDistance, country.CountryId);
                int maximumFixedDistance = int.Parse(maximumFixedDistanceObj.Value);

                //calculate price for the haulage
                if (distance <= maximumFixedDistance)
                {
                    price = haulage.FixedRate;
                }
                else
                {
                    //1. get the fixed rate and substract the maximum fixed distance from distance
                    distance = distance - maximumFixedDistance;

                    //2. multiply the remaining distance with the additional pate
                    price = haulage.FixedRate + distance * haulage.AdditionalRate;
                }

                return new MobilePriceDTO
                {
                    DeliveryPrice = price,
                    GrandTotal = Math.Round(price * PercentageTobeUsed),
                    CurrencySymbol = country.CurrencySymbol,
                    CurrencyCode = country.CurrencyCode,
                    IsWithinProcessingTime = IsWithinProcessingTime,
                    Discount = Math.Round(price * discount)
                };
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> EditProfile(UserDTO user)
        {
            try
            {
                string currentUserId = await _userService.GetCurrentUserId();
                var currentUser = await _userService.GetUserById(currentUserId);

                currentUser.FirstName = user.FirstName;
                currentUser.LastName = user.LastName;
                currentUser.PictureUrl = user.PictureUrl;
                await _userService.UpdateUser(currentUser.Id, currentUser);

                string userChannelType = user.UserChannelType.ToString();
                user.UserChannelCode = currentUser.UserChannelCode;

                if (userChannelType == UserChannelType.Partner.ToString()
                    || userChannelType == UserChannelType.IndividualCustomer.ToString()
                    || userChannelType == UserChannelType.Ecommerce.ToString())
                {
                    await UpdatePartner(user);
                    await UpdateCustomer(user);
                    await UpdateCompany(user);
                }
                await _uow.CompleteAsync();
                return true;
            }
            catch
            {
                throw new GenericException("Please an error occurred while updating profile.");
            }
        }

        public async Task<bool> UpdateVehicleProfile(UserDTO user)
        {
            try
            {
                var partner = await _uow.Partner.GetAsync(s => s.PartnerCode == user.UserChannelCode);
                if (partner == null)
                {
                    var partnerDTO = new PartnerDTO
                    {
                        PartnerType = PartnerType.InternalDeliveryPartner,
                        PartnerName = user.FirstName + " " + user.LastName,
                        PartnerCode = user.UserChannelCode,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Email = user.Email,
                        PhoneNumber = user.PhoneNumber,
                        UserId = user.Id,
                        IsActivated = false,
                    };
                    var FinalPartner = Mapper.Map<Partner>(partnerDTO);
                    _uow.Partner.Add(FinalPartner);
                }
                foreach (var vehicle in user.VehicleType)
                {
                    var Vehicle = new VehicleTypeDTO
                    {
                        Vehicletype = vehicle.ToUpper(),
                        Partnercode = user.UserChannelCode
                    };
                    var vehicletype = Mapper.Map<VehicleType>(Vehicle);
                    _uow.VehicleType.Add(vehicletype);
                }
            }
            catch
            {
                throw new GenericException("Please an error occurred while updating profile.");
            }
            await _uow.CompleteAsync();
            return true;
        }

        private async Task<bool> WithinProcessingTime(int CountryId)
        {
            bool IsWithinTime = false;
            var Startime = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.PickUpStartTime, CountryId);
            var Endtime = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.PickUpEndTime, CountryId);
            TimeSpan start = new TimeSpan(Convert.ToInt32(Startime.Value), 0, 0);
            TimeSpan end = new TimeSpan(Convert.ToInt32(Endtime.Value), 0, 0);
            TimeSpan now = DateTime.Now.TimeOfDay;
            if (now > start && now < end)
            {
                IsWithinTime = true;
            }
            return IsWithinTime;
        }
        private async Task<int> CalculateTimeBasedonLocation(PreShipmentMobileDTO item)
        {
            var Location = new LocationDTO
            {
                DestinationLatitude = (double)item.ReceiverLocation.Latitude,
                DestinationLongitude = (double)item.ReceiverLocation.Longitude,
                OriginLatitude = (double)item.SenderLocation.Latitude,
                OriginLongitude = (double)item.SenderLocation.Longitude
            };
            RootObject details = await _partnertransactionservice.GetGeoDetails(Location);
            var time = (details.rows[0].elements[0].duration.value / 60);
            return time;

        }
        public async Task DetermineShipmentstatus(string waybill, int countryid, DateTime ExpectedTimeOfDelivery)
        {
            var status = await _uow.MobilePickUpRequests.GetAsync(s => s.Waybill == waybill && (s.Status != MobilePickUpRequestStatus.Rejected.ToString() || s.Status != MobilePickUpRequestStatus.TimedOut.ToString()));
            if (status != null)
            {
                if (status.Status != MobilePickUpRequestStatus.Delivered.ToString())
                {
                    var time = string.Format("{0:f}", ExpectedTimeOfDelivery);
                    var Email = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.EmailForDeliveryTime, countryid);
                    var message = new MobileMessageDTO
                    {
                        SenderEmail = Email.Value,
                        WaybillNumber = waybill,
                        ExpectedTimeofDelivery = time
                    };
                    await _messageSenderService.SendGenericEmailMessage(MessageType.UDM, message);
                }
            }

        }
        private async Task CheckDeliveryTimeAndSendMail(PreShipmentMobileDTO item)
        {
            var time = await CalculateTimeBasedonLocation(item);
            var country = await _uow.Country.GetCountryByStationId(item.SenderStationId);
            var additionaltime = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.AddedMinutesForDeliveryTime, country.CountryId);
            var addedtime = int.Parse(additionaltime.Value);
            var totaltime = time + addedtime;
            var NewTime = DateTime.Now.AddMinutes(totaltime);
            BackgroundJob.Schedule(() => DetermineShipmentstatus(item.Waybill, country.CountryId, NewTime), TimeSpan.FromMinutes(totaltime));

        }

        public async Task<object> CancelShipmentWithNoCharge(string Waybill, string Userchanneltype)
        {
            try
            {
                var preshipmentmobile = await _uow.PreShipmentMobile.GetAsync(s => s.Waybill == Waybill);

                if (preshipmentmobile == null)
                {
                    throw new GenericException("Shipment cannot be found");
                }

                if (Userchanneltype == UserChannelType.IndividualCustomer.ToString())
                {
                    if (preshipmentmobile.shipmentstatus == "Shipment created" || preshipmentmobile.shipmentstatus == MobilePickUpRequestStatus.Rejected.ToString() || preshipmentmobile.shipmentstatus == MobilePickUpRequestStatus.TimedOut.ToString())
                    {
                        preshipmentmobile.shipmentstatus = MobilePickUpRequestStatus.Cancelled.ToString();

                        var defaultServiceCenter = await _userService.GetGIGGOServiceCentre();
                        var updatedwallet = await _uow.Wallet.GetAsync(s => s.CustomerCode == preshipmentmobile.CustomerCode);
                        updatedwallet.Balance = ((updatedwallet.Balance + preshipmentmobile.GrandTotal));

                        var transaction = new WalletTransactionDTO
                        {
                            WalletId = updatedwallet.WalletId,
                            CreditDebitType = CreditDebitType.Credit,
                            Amount = preshipmentmobile.GrandTotal,
                            ServiceCentreId = defaultServiceCenter.ServiceCentreId,
                            Waybill = preshipmentmobile.Waybill,
                            Description = "Cancelled Shipment",
                            PaymentType = PaymentType.Online,
                            UserId = preshipmentmobile.UserId
                        };

                        var walletTransaction = await _walletTransactionService.AddWalletTransaction(transaction);
                        await ScanMobileShipment(new ScanDTO
                        {
                            WaybillNumber = Waybill,
                            ShipmentScanStatus = ShipmentScanStatus.MSCC
                        });
                    }
                    else
                    {
                        throw new GenericException("Shipment cannot be cancelled because it has been processed.");
                    }
                }
                //FOR PARTNER TRYING TO CANCEL  A SHIPMENT
                else
                {
                    //Get user
                    var user = await _userService.GetCurrentUserId();

                    //var pickuprequests = await _uow.MobilePickUpRequests.GetAsync(s => s.Waybill == preshipmentmobile.Waybill && s.Status == MobilePickUpRequestStatus.Accepted.ToString());
                    var pickuprequests = await _uow.MobilePickUpRequests.GetAsync(s => s.Waybill == preshipmentmobile.Waybill && s.UserId == user);

                    if (pickuprequests != null)
                    {
                        pickuprequests.Status = MobilePickUpRequestStatus.Cancelled.ToString();
                    }

                    preshipmentmobile.shipmentstatus = MobilePickUpRequestStatus.Processing.ToString();

                }
                await _uow.CompleteAsync();
                return new { IsCancelled = true };
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<PreShipmentSummaryDTO> GetPartnerDetailsFromWaybill(string Waybill)
        {
            var ShipmentSummaryDetails = new PreShipmentSummaryDTO();
            var partner = await _uow.MobilePickUpRequests.GetAsync(s => s.Waybill == Waybill && (s.Status != MobilePickUpRequestStatus.Rejected.ToString() || s.Status != MobilePickUpRequestStatus.TimedOut.ToString()));
            if (partner != null)
            {
                var Partnerdetails = await _uow.Partner.GetAsync(s => s.UserId == partner.UserId);
                if (Partnerdetails == null)
                {
                    throw new GenericException("Partner Details does not exist!!");
                }
                else
                {
                    var userdetails = await _uow.User.GetUserById(partner.UserId);
                    var partnerinfo = Mapper.Map<PartnerDTO>(Partnerdetails);
                    ShipmentSummaryDetails.partnerdetails = partnerinfo;
                    if (userdetails != null)
                    {
                        ShipmentSummaryDetails.partnerdetails.PictureUrl = userdetails.PictureUrl;
                    }
                }
            }
            return ShipmentSummaryDetails;
        }

        private async Task UpdatePartner(UserDTO user)
        {
            var partner = await _uow.Partner.GetAsync(s => s.PartnerCode == user.UserChannelCode);
            if (partner != null)
            {
                partner.FirstName = user.FirstName;
                partner.LastName = user.LastName;
                partner.PartnerName = user.FirstName + " " + user.LastName;
                partner.PictureUrl = user.PictureUrl;
            }
        }
        private async Task UpdateCustomer(UserDTO user)
        {
            var customer = await _uow.IndividualCustomer.GetAsync(s => s.CustomerCode == user.UserChannelCode);
            if (customer != null)
            {
                customer.FirstName = user.FirstName;
                customer.LastName = user.LastName;
                customer.PictureUrl = user.PictureUrl;
            }
        }
        private async Task UpdateCompany(UserDTO user)
        {
            var company = await _uow.Company.GetAsync(s => s.CustomerCode == user.UserChannelCode);
            if (company != null)
            {
                company.FirstName = user.FirstName;
                company.LastName = user.LastName;

                if (!company.Name.Equals(user.Organisation, StringComparison.OrdinalIgnoreCase))
                {
                    company.Name = user.Organisation;
                }
            }
        }
        private async Task<int> GetCountryByServiceCentreId(int ServicecentreId)
        {
            int CountryId = 0;
            var ServiceCentre = await _uow.ServiceCentre.GetAsync(s => s.ServiceCentreId == ServicecentreId);
            if (ServiceCentre != null)
            {
                var Country = await _uow.Country.GetCountryByStationId(ServiceCentre.StationId);
                if (Country != null)
                {
                    CountryId = Country.CountryId;
                }
            }
            return CountryId;
        }

        private async Task<decimal> CalculateGeoDetailsBasedonLocation(PreShipmentMobileDTO item)
        {
            var FixedDistance = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.GiglgoMaximumFixedDistance, item.CountryId);
            var FixedPriceForTime = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.GiglgoFixedPriceForTime, item.CountryId);
            var FixedPriceForDistance = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.GiglgoFixedPriceForDistance, item.CountryId);

            var FixedDistanceValue = int.Parse(FixedDistance.Value);
            var FixedPriceForTimeValue = Convert.ToDecimal(FixedPriceForTime.Value);
            var FixedPriceForDistanceValue = Convert.ToDecimal(FixedPriceForDistance.Value);

            var Location = new LocationDTO
            {
                DestinationLatitude = (double)item.ReceiverLocation.Latitude,
                DestinationLongitude = (double)item.ReceiverLocation.Longitude,
                OriginLatitude = (double)item.SenderLocation.Latitude,
                OriginLongitude = (double)item.SenderLocation.Longitude
            };

            RootObject details = await _partnertransactionservice.GetGeoDetails(Location);
            var time = (details.rows[0].elements[0].duration.value / 60);
            var distance = (details.rows[0].elements[0].distance.value / 1000);

            decimal amount = time * FixedPriceForTimeValue;

            if (distance > FixedDistanceValue)
            {
                var distancedifference = (distance - FixedDistanceValue);
                amount += distancedifference * FixedPriceForDistanceValue;
            }

            return amount;
        }

        public async Task<List<PreShipmentMobileDTO>> GetPreShipmentForEcommerce(string userChannelCode)
        {
            try
            {
                var shipment = await _uow.Shipment.FindAsync(x => x.CustomerCode.Equals(userChannelCode) && x.IsCancelled == false);

                List<PreShipmentMobileDTO> shipmentDto = (from r in shipment
                                                          select new PreShipmentMobileDTO()
                                                          {
                                                              PreShipmentMobileId = r.ShipmentId,
                                                              Waybill = r.Waybill,
                                                              ActualDateOfArrival = r.ActualDateOfArrival,
                                                              DateCreated = r.DateCreated,
                                                              DateModified = r.DateModified,
                                                              ExpectedDateOfArrival = r.ExpectedDateOfArrival,
                                                              ReceiverAddress = r.ReceiverAddress,
                                                              SenderAddress = r.SenderAddress,
                                                              ReceiverCountry = r.ReceiverCountry,
                                                              ReceiverEmail = r.ReceiverEmail,
                                                              ReceiverName = r.ReceiverName,
                                                              ReceiverPhoneNumber = r.ReceiverPhoneNumber,
                                                              ReceiverState = r.ReceiverState,
                                                              UserId = r.UserId,
                                                              Value = r.Value,
                                                              GrandTotal = r.GrandTotal,
                                                              CustomerCode = r.CustomerCode,
                                                              DepartureServiceCentreId = r.DepartureServiceCentreId,
                                                              shipmentstatus = "Shipment",
                                                              CustomerId = r.CustomerId,
                                                              CustomerType = r.CustomerType,
                                                              CountryId = r.DepartureCountryId
                                                          }).OrderByDescending(x => x.DateCreated).Take(20).ToList();

                foreach (var shipments in shipmentDto)
                {
                    var country = await _uow.Country.GetAsync(shipments.CountryId);

                    if (country != null)
                    {
                        shipments.CurrencyCode = country.CurrencyCode;
                        shipments.CurrencySymbol = country.CurrencySymbol;
                    }

                    if (shipments.CustomerType == "Individual")
                    {
                        shipments.CustomerType = CustomerType.IndividualCustomer.ToString();
                    }

                    if (shipments.SenderAddress == null)
                    {
                        CustomerType customerType = (CustomerType)Enum.Parse(typeof(CustomerType), shipments.CustomerType);
                        var CustomerDetails = await _customerService.GetCustomer(shipments.CustomerId, customerType);
                        shipments.SenderAddress = CustomerDetails.Address;
                        shipments.SenderName = CustomerDetails.Name;
                    }
                }

                return await Task.FromResult(shipmentDto);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task<decimal> GetPickUpPriceForIndividual(string vehicleType, int CountryId)
        {
            var PickUpPrice = 0.0M;
            if (vehicleType.ToLower() == Vehicletype.Car.ToString().ToLower())
            {
                var carPickUprice = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.CarPickupPrice, CountryId);
                PickUpPrice = (Convert.ToDecimal(carPickUprice.Value));
            }
            else if (vehicleType.ToLower() == Vehicletype.Bike.ToString().ToLower())
            {
                var bikePickUprice = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.BikePickUpPrice, CountryId);
                PickUpPrice = (Convert.ToDecimal(bikePickUprice.Value));
            }
            else if (vehicleType.ToLower() == Vehicletype.Van.ToString().ToLower())
            {
                var vanPickUprice = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.VanPickupPrice, CountryId);
                PickUpPrice = (Convert.ToDecimal(vanPickUprice.Value));
            }
            return PickUpPrice;
        }

        private async Task<decimal> GetPickUpPriceForEcommerce(string vehicleType, int CountryId)
        {
            var PickUpPrice = 0.0M;
            if (vehicleType.ToLower() == Vehicletype.Car.ToString().ToLower())
            {
                var carPickUprice = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.EcommerceCarPickupPrice, CountryId);
                PickUpPrice = (Convert.ToDecimal(carPickUprice.Value));
            }
            else if (vehicleType.ToLower() == Vehicletype.Bike.ToString().ToLower())
            {
                var bikePickUprice = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.EcommerceBikePickUpPrice, CountryId);
                PickUpPrice = (Convert.ToDecimal(bikePickUprice.Value));
            }
            else if (vehicleType.ToLower() == Vehicletype.Van.ToString().ToLower())
            {
                var vanPickUprice = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.EcommerceVanPickupPrice, CountryId);
                PickUpPrice = (Convert.ToDecimal(vanPickUprice.Value));
            }
            return PickUpPrice;
        }

        public async Task<List<GiglgoStationDTO>> GetGoStations()
        {
            var stations = await _giglgoStationService.GetGoStations();
            return stations;

        }

        //promote for GIG Go Feb 2020
        private async Task<MobilePriceDTO> CalculatePromoPrice(PreShipmentMobileDTO preShipmentDTO, int zoneId, decimal pickupValue)
        {
            var result = new MobilePriceDTO
            {
                GrandTotal = 0,
                Discount = 0
            };
            if (preShipmentDTO.VehicleType != null)
            {
                //1.if the shipment Vehicle Type is Bike and the zone is lagos, Calculate the Promo price
                if (preShipmentDTO.VehicleType.ToLower() == Vehicletype.Bike.ToString().ToLower() && zoneId == 1)
                {
                    //GIG Go Promo Price
                    bool totalWeight = await GetTotalWeight(preShipmentDTO);

                    if (totalWeight)
                    {
                        //1. Get the Promo price 999
                        var gigGoPromo = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.GIGGOPromo, preShipmentDTO.CountryId);
                        decimal promoPrice = decimal.Parse(gigGoPromo.Value);

                        if (promoPrice > 0)
                        {
                            //3. Make the Promo Price to be GrandTotal 
                            result.GrandTotal = promoPrice;

                            //2. Substract the Promo price from GrandTotal and bind the different to Discount
                            result.Discount = (decimal)preShipmentDTO.CalculatedTotal + pickupValue - promoPrice;
                        }
                    }
                }
            }

            return result;
        }

        private async Task<bool> GetTotalWeight(PreShipmentMobileDTO preShipmentDTO)
        {
            foreach (var preShipmentItem in preShipmentDTO.PreShipmentItems)
            {
                if (preShipmentItem.ShipmentType == ShipmentType.Special)
                {
                    var getPackageWeight = await _uow.SpecialDomesticPackage.GetAsync(x => x.SpecialDomesticPackageId == preShipmentItem.SpecialPackageId);
                    if (getPackageWeight != null && getPackageWeight.Weight > 3)
                    {
                        return false;
                    }
                }
                else if (preShipmentItem.ShipmentType == ShipmentType.Regular)
                {
                    if (preShipmentItem.Weight > 3)
                    {
                        return false;
                    }
                }
            };
            return true;
        }

        private async Task<decimal> CalculatePromoPriceForDipatchRider(PreShipmentMobile preshipmentMobile)
        {
            decimal result = 0;

            //1.if the shipment Vehicle Type is Bike and the zone is lagos, Calculate the Promo price
            if (preshipmentMobile.VehicleType.ToLower() == Vehicletype.Bike.ToString().ToLower() && preshipmentMobile.ZoneMapping == 1)
            {
                bool totalweight = await GetTotalWeight(preshipmentMobile);

                if (totalweight)
                {
                    //1. Get the Promo price 999
                    var gigGoPromo = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.GIGGOPromo, preshipmentMobile.CountryId);
                    decimal promoPrice = decimal.Parse(gigGoPromo.Value);

                    if (promoPrice > 0)
                    {
                        result = preshipmentMobile.GrandTotal + (decimal)preshipmentMobile.DiscountValue;
                    }
                }
            }

            return result;
        }

        private async Task<bool> GetTotalWeight(PreShipmentMobile preShipmentDTO)
        {
            foreach (var preShipmentItem in preShipmentDTO.PreShipmentItems)
            {
                if (preShipmentItem.ShipmentType == ShipmentType.Special)
                {
                    var getPackageWeight = await _uow.SpecialDomesticPackage.GetAsync(x => x.SpecialDomesticPackageId == preShipmentItem.SpecialPackageId);
                    if (getPackageWeight != null && getPackageWeight.Weight > 3)
                    {
                        return false;
                    }
                }
                else if (preShipmentItem.ShipmentType == ShipmentType.Regular)
                {
                    if (preShipmentItem.Weight > 3)
                    {
                        return false;
                    }
                }
            };
            return true;
        }

        //Change
        private async Task<PreShipmentMobileDTO> ChangePreshipmentItemQuantity(PreShipmentMobileDTO preShipmentDTO, int zoneId)
        {
            if (preShipmentDTO.VehicleType != null)
            {
                //1.if the shipment Vehicle Type is Bike and the zone is lagos, Calculate the Promo price
                if (preShipmentDTO.VehicleType.ToLower() == Vehicletype.Bike.ToString().ToLower() && zoneId == 1)
                {
                    //GIG Go Promo Price
                    bool totalWeight = await GetTotalWeight(preShipmentDTO);

                    if (totalWeight)
                    {
                        //1. Get the Promo price 900
                        var gigGoPromo = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.GIGGOPromo, preShipmentDTO.CountryId);
                        decimal promoPrice = decimal.Parse(gigGoPromo.Value);

                        //if there is promo then change the quantity to 1
                        if (promoPrice > 0)
                        {
                            foreach (var preShipmentItem in preShipmentDTO.PreShipmentItems)
                            {
                                preShipmentItem.Quantity = 1;
                            }
                        }
                    }
                }
            }

            return preShipmentDTO;
        }

        public async Task<List<LocationDTO>> GetPresentDayShipmentLocations()
        {
            //Excluding It Test
            string excludeUserList = ConfigurationManager.AppSettings["excludeUserList"];
            string[] testUserId = excludeUserList.Split(',').ToArray();

            var startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            var endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day).AddDays(1);

            var shipmentsResult = _uow.PreShipmentMobile.GetAllAsQueryable().Where(s => s.DateCreated >= startDate && s.DateCreated < endDate &&
                                        !testUserId.Contains(s.UserId));


            List<LocationDTO> locationDTOs = (from r in shipmentsResult
                                              select new LocationDTO()
                                              {
                                                  Longitude = r.SenderLocation.Longitude,
                                                  Latitude = r.SenderLocation.Latitude
                                              }).ToList();

            return await Task.FromResult(locationDTOs.OrderByDescending(x => x.DateCreated).ToList());
        }
    }
}