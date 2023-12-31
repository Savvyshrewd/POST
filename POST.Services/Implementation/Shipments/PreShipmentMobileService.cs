﻿using POST.Core.IServices.Shipments;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using POST.Core.DTO.Shipments;
using POST.Core;
using POST.Core.IServices.Zone;
using POST.Core.IServices.Utility;
using POST.Core.Domain;
using AutoMapper;
using POST.Core.Enums;
using POST.Core.IServices.Business;
using POST.Core.DTO.PaymentTransactions;
using POST.Core.IServices.Wallet;
using POST.Infrastructure;
using POST.Core.DTO.Wallet;
using POST.CORE.DTO.Report;
using POST.Core.IServices.User;
using POST.Core.DTO.Zone;
using POST.Core.DTO;
using POST.Core.IServices;
using POST.Core.IServices.Partnership;
using POST.Core.DTO.Partnership;
using POST.Core.IMessageService;
using POST.Core.DTO.Customers;
using GIGL.POST.Core.Domain;
using POST.Core.Domain.Partnership;
using POST.Core.DTO.User;
using VehicleType = POST.Core.Domain.VehicleType;
using Hangfire;
using POST.Core.IServices.Customers;
using POST.Core.DTO.Utility;
using System.Configuration;
using System.Net;
using System.Net.Http;
using POST.Core.DTO.Report;
using System.Security.Cryptography;
using System.Text;
using PaymentType = POST.Core.Enums.PaymentType;
using POST.Core.DTO.Account;
using POST.Core.IServices.Account;
using POST.Core.IServices.Node;
using POST.CORE.Domain;
using POST.Core.DTO.Node;
using Newtonsoft.Json;
using POST.CORE.DTO.Shipments;
using POST.Core.Domain.Wallet;
using POST.Core.IServices.ServiceCentres;
using POST.Core.IServices.DHL;
using POST.Core.IServices.UPS;
using POST.Core.DTO.Alpha;
using POST.Core.IServices.Alpha;
using Newtonsoft.Json.Linq;

namespace POST.Services.Implementation.Shipments
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
        private readonly IFinancialReportService _financialReportService;
        private readonly INodeService _nodeService;
        private readonly IPaymentService _paymentService;
        private readonly IWaybillPaymentLogService _waybillPaymentLogService;
        private readonly IServiceCentreService _centreService;
        private readonly IDHLService _dhlService;
        private readonly IUPSService _uPSService;
        private readonly IInsuranceService _insuranceService;
        private readonly IAutoManifestAndGroupingService _autoManifestAndGroupingService;
        private readonly IAlphaService _alphaService;

        public PreShipmentMobileService(IUnitOfWork uow, IShipmentService shipmentService, INumberGeneratorMonitorService numberGeneratorMonitorService,
            IPricingService pricingService, IWalletService walletService, IWalletTransactionService walletTransactionService,
            IUserService userService, IMobileShipmentTrackingService mobiletrackingservice,
            IMobilePickUpRequestsService mobilepickuprequestservice, IDomesticRouteZoneMapService domesticroutezonemapservice, ICategoryService categoryservice, ISubCategoryService subcategoryservice,
            IPartnerTransactionsService partnertransactionservice, IGlobalPropertyService globalPropertyService, IMessageSenderService messageSenderService,
            IHaulageService haulageService, IHaulageDistanceMappingService haulageDistanceMappingService, IPartnerService partnerService, ICustomerService customerService,
            IGiglgoStationService giglgoStationService, IGroupWaybillNumberService groupWaybillNumberService, IFinancialReportService financialReportService,
            INodeService nodeService, IPaymentService paymentService, IWaybillPaymentLogService waybillPaymentLogService, IServiceCentreService centreService,
            IDHLService dHLService, IUPSService uPSService, IInsuranceService insuranceService, IAutoManifestAndGroupingService autoManifestAndGroupingService, IAlphaService alphaService)
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
            _financialReportService = financialReportService;
            _nodeService = nodeService;
            _paymentService = paymentService;
            _waybillPaymentLogService = waybillPaymentLogService;
            _centreService = centreService;
            _dhlService = dHLService;
            _uPSService = uPSService;
            _insuranceService = insuranceService;
            _autoManifestAndGroupingService = autoManifestAndGroupingService;
            _alphaService = alphaService;
            MapperConfig.Initialize();
        }

        public async Task<object> AddPreShipmentMobile(PreShipmentMobileDTO preShipment)
        {
            try
            {
                //null DateCreated
                preShipment.DateCreated = DateTime.Now;
                // check if it is international shipment
                if (preShipment.IsInternationalShipment && preShipment.IsInternationalShipment != null)
                {
                    preShipment.ReceiverStationId = 4;
                    preShipment.ReceiverLat = 6.54887241412068;
                    preShipment.ReceiverLng = 3.3891275341873115;
                    preShipment.ZoneMapping = 2;
                }
                var zoneid = await _domesticroutezonemapservice.GetZoneMobile(preShipment.SenderStationId, preShipment.ReceiverStationId);
                preShipment.ZoneMapping = zoneid.ZoneId;
                if (preShipment.IsInternationalShipment && preShipment.IsInternationalShipment != null)
                {
                    preShipment.ZoneMapping = 2;
                }
                var newPreShipment = await CreatePreShipment(preShipment);
                //await _uow.CompleteAsync();
                bool IsBalanceSufficient = true;
                string message = "Shipment created successfully";
                if (newPreShipment.IsBalanceSufficient == false)
                {
                    message = "Insufficient Wallet Balance";
                    IsBalanceSufficient = false;

                    throw new GenericException(message, $"{(int)HttpStatusCode.Forbidden}");
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

                    throw new GenericException(message, $"{(int)HttpStatusCode.Forbidden}");
                }
                return new { waybill = newPreShipment.Waybill, message = message, IsBalanceSufficient, Zone = zoneid.ZoneId };
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<PreShipmentMobileThirdPartyDTO> AddPreShipmentMobileThirdParty(CreatePreShipmentMobileDTO preShipments)
        {
            try
            {
                var preShipment = Mapper.Map<PreShipmentMobileDTO>(preShipments);

                //null DateCreated
                preShipment.DateCreated = DateTime.Now;

                if (preShipment.SenderStationId == 0)
                {
                    throw new GenericException("Please select Sender Station");
                }

                if (preShipment.ReceiverStationId == 0)
                {
                    throw new GenericException("Please select Receiver Station");
                }

                if (preShipment.SenderLocation.Latitude == null)
                {
                    throw new GenericException("Sender Latitude is required");
                }

                if (preShipment.SenderLocation.Longitude == null)
                {
                    throw new GenericException("Sender Longitude is required");
                }

                if (preShipment.ReceiverLocation.Longitude == null)
                {
                    throw new GenericException("Receiver Longitude is required");
                }

                if (preShipment.ReceiverLocation.Latitude == null)
                {
                    throw new GenericException("Receiver Latitude is required");
                }

                var zoneid = await _domesticroutezonemapservice.GetZoneMobile(preShipment.SenderStationId, preShipment.ReceiverStationId);
                preShipment.ZoneMapping = zoneid.ZoneId;
                var newPreShipment = await CreatePreShipmentThirdParty(preShipment);

                bool IsBalanceSufficient = true;
                string message = "Shipment created successfully";
                if (newPreShipment.IsBalanceSufficient == false)
                {
                    message = "Insufficient Wallet Balance";
                    IsBalanceSufficient = false;

                    throw new GenericException(message, $"{(int)HttpStatusCode.Forbidden}");
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

                    throw new GenericException(message, $"{(int)HttpStatusCode.Forbidden}");
                }

                var preshipmentRetuenObj = new PreShipmentMobileThirdPartyDTO();
                preshipmentRetuenObj.waybill = newPreShipment.Waybill;
                preshipmentRetuenObj.message = message;
                preshipmentRetuenObj.IsBalanceSufficient = IsBalanceSufficient;
                preshipmentRetuenObj.Zone = zoneid.ZoneId;
                preshipmentRetuenObj.PaymentUrl = newPreShipment.PaymentUrl;


                return preshipmentRetuenObj;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task SendSMSForMobileShipmentCreation(MobileShipmentCreationMessageDTO smsMessageExtensionDTO, MessageType messageType)
        {
            await _messageSenderService.SendMessage(messageType, EmailSmsType.SMS, smsMessageExtensionDTO);
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
                throw new GenericException("Maximum Number of Shipment on Global Property is not set", $"{(int)HttpStatusCode.NotFound}");
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
                    throw new GenericException($"Total number of Shipment items can not exceed {maxNumOfShipment.Value}", $"{(int)HttpStatusCode.Forbidden}");
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
               
                if (!String.IsNullOrEmpty(preShipmentDTO.ReceiverPhoneNumber))
                {
                    preShipmentDTO.ReceiverPhoneNumber = preShipmentDTO.ReceiverPhoneNumber.Trim();
                }

                if (!String.IsNullOrEmpty(preShipmentDTO.SenderPhoneNumber))
                {
                    preShipmentDTO.SenderPhoneNumber = preShipmentDTO.SenderPhoneNumber.Trim();
                }

                var PreshipmentPriceDTO = new MobilePriceDTO();

                // get the current user info
                var currentUserId = await _userService.GetCurrentUserId();
                preShipmentDTO.UserId = currentUserId;
                var user = await _userService.GetUserById(currentUserId);
                preShipmentDTO.CustomerCode = user.UserChannelCode;
                if (preShipmentDTO.IsInternationalShipment != null && preShipmentDTO.IsInternationalShipment)
                {
                    preShipmentDTO.ReceiverStateOrProvinceCode = preShipmentDTO.ReceiverStateOrProvinceCode.Length > 2 ? preShipmentDTO.ReceiverCountryCode : preShipmentDTO.ReceiverStateOrProvinceCode;
                }

                var country = await _uow.Country.GetCountryByStationId(preShipmentDTO.SenderStationId);
                if (country == null)
                {
                    throw new GenericException("Sender Station Country Not Found", $"{(int)HttpStatusCode.NotFound}");
                }
                preShipmentDTO.CountryId = country.CountryId;

                var customer = await _uow.Company.GetAsync(s => s.CustomerCode == user.UserChannelCode);
                if (customer != null)
                {
                    //If the account is not active, block the customer from creating shipment
                    if (customer.CompanyStatus != CompanyStatus.Active)
                    {
                        throw new GenericException($"Your account has been {customer.CompanyStatus}, contact support for assistance", $"{(int)HttpStatusCode.Forbidden}");
                    }
                    if (preShipmentDTO.IsCashOnDelivery && customer.Rank != Rank.Class)
                    {
                        throw new GenericException("Cash On Delivery feature not available for  Basic ecommerce users; please upgrade to Class to have access to Cash On Delivery.");
                    }

                    if (customer.IsEligible != true)
                    {
                        preShipmentDTO.IsEligible = false;
                        preShipmentDTO.IsCodNeeded = customer.isCodNeeded;
                        preShipmentDTO.CurrencySymbol = country.CurrencySymbol;
                        preShipmentDTO.CurrentWalletAmount = customer.WalletAmount != null ? (decimal)customer.WalletAmount : 0;
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
                else if (preShipmentDTO.VehicleType.ToLower() == Vehicletype.Bike.ToString().ToLower() && preShipmentDTO.ZoneMapping == 1)
                {
                    PreshipmentPriceDTO = await GetPriceForBike(preShipmentDTO);
                }
                else
                {
                    if (user.UserChannelType == UserChannelType.Ecommerce || customer != null)
                    {
                        preShipmentDTO.Shipmentype = ShipmentType.Ecommerce;
                    }
                    preShipmentDTO.IsFromShipment = true;
                    if (preShipmentDTO.IsInternationalShipment != null && preShipmentDTO.IsInternationalShipment)
                    {
                        var shipmentItems = Mapper.Map<List<ShipmentItemDTO>>(preShipmentDTO.PreShipmentItems);
                        var shipment = Mapper.Map<InternationalShipmentDTO>(preShipmentDTO);
                        for (int i = 0; i < preShipmentDTO.PreShipmentItems.Count(); i++)
                        {
                            shipmentItems[i].Price = Convert.ToInt32(preShipmentDTO.PreShipmentItems[i].Value);
                            // Quick fixes
                            if (shipmentItems[i].Weight == 0)
                            {
                                shipmentItems[i].InternationalShipmentItemCategory = InternationalShipmentItemCategory.Document;
                            }
                            else
                            {
                                shipmentItems[i].InternationalShipmentItemCategory = InternationalShipmentItemCategory.NonDocument;
                            }
                        }
                        if (shipment.CustomerDetails is null )
                        {
                            if (customer != null)
                            {
                                var custmerDetail = Mapper.Map<CustomerDTO>(customer);
                                shipment.CustomerDetails = custmerDetail; 
                            }
                            else
                            {
                                var individualCust = await _uow.IndividualCustomer.GetAsync(s => s.CustomerCode == user.UserChannelCode);
                                var custmerDetail = Mapper.Map<CustomerDTO>(individualCust);
                                shipment.CustomerDetails = custmerDetail;
                            }
                        }
                        shipment.ShipmentItems = shipmentItems;
                        shipment.DeclarationOfValueCheck = shipment.ShipmentItems.Sum(x => x.Price);
                        shipment.ItemDetails = shipment.ShipmentItems[0].Description;
                        shipment.IsFromMobile = true;
                        var isWithinProcessingTime = await WithinProcessingTime(preShipmentDTO.CountryId);
                        var intlData = await _shipmentService.GetInternationalShipmentPrice(shipment);
                        var result = intlData.FirstOrDefault(x => x.CompanyMap == preShipmentDTO.CompanyMap);
                        PreshipmentPriceDTO.Discount = result.Discount;
                        PreshipmentPriceDTO.InsuranceValue = result.Insurance;
                        PreshipmentPriceDTO.GrandTotal = result.GrandTotal;
                        PreshipmentPriceDTO.Vat = result.VAT;
                        PreshipmentPriceDTO.PreshipmentMobile = preShipmentDTO;
                        PreshipmentPriceDTO.IsWithinProcessingTime = isWithinProcessingTime;
                        preShipmentDTO.Vat = result.VAT;
                        preShipmentDTO.vatvalue_display = result.VAT;
                        preShipmentDTO.DiscountValue = result.Discount;
                        preShipmentDTO.vatvalue_display = result.Discount;
                        preShipmentDTO.InternationalShippingCost = result.InternationalShippingCost;
                        PreshipmentPriceDTO.InternationalShippingCost = result.InternationalShippingCost;
                        PreshipmentPriceDTO.PreshipmentMobile.InternationalShippingCost = result.InternationalShippingCost;
                    }
                    else
                    {
                        PreshipmentPriceDTO = await GetPrice(preShipmentDTO);
                    }
                }

                // update and apply coupon 
                if (preShipmentDTO.IsCoupon)
                {
                    PreshipmentPriceDTO.GrandTotal = await ComputeCouponAmount(preShipmentDTO.CouponCode, PreshipmentPriceDTO.GrandTotal.Value);
                    var coupon = _uow.CouponManagement.SingleOrDefault(x => x.CouponCode == preShipmentDTO.CouponCode);
                    coupon.IsCouponCodeUsed = true;
                }

                decimal shipmentGrandTotal = (decimal)PreshipmentPriceDTO.GrandTotal;
                decimal actualAmountToDebit = shipmentGrandTotal;
                var wallet = await _walletService.GetWalletBalance();

                //get departure country
                if (country != null)
                {
                    if (user.UserActiveCountryId != country.CountryId)
                    {
                        //get actual amount to debit
                        var countryRateConversion = await _uow.CountryRouteZoneMap.GetAsync(r =>
                        r.DestinationId == country.CountryId &&
                        r.DepartureId == user.UserActiveCountryId, "Zone,Destination,Departure");
                        if (countryRateConversion == null)
                            throw new GenericException("The Mapping of Route to Zone does not exist");

                        double amountToDebitDouble = countryRateConversion.Rate * (double)shipmentGrandTotal;
                        actualAmountToDebit = (decimal)Math.Round(amountToDebitDouble, 2);
                    }
                }

                //confirm the weight for special shipment type
                var specialPackageIDs = preShipmentDTO.PreShipmentItems.Select(x => x.SpecialPackageId).ToList();
                if (specialPackageIDs.Any())
                {
                    var specialPackages = _uow.SpecialDomesticPackage.GetAllAsQueryable().Where(x => specialPackageIDs.Contains(x.SpecialDomesticPackageId)).ToList();
                    //confirm the weight for special shipment type
                    foreach (var item in preShipmentDTO.PreShipmentItems)
                    {
                        if (item.SpecialPackageId != null && item.SpecialPackageId > 0)
                        {
                            var specialItem = specialPackages.Where(x => x.SpecialDomesticPackageId == item.SpecialPackageId).FirstOrDefault();
                            if (specialItem != null)
                            {
                                item.Weight = specialItem.Weight;
                            }
                        }
                    }
                }

                // set declared value of the shipment
                decimal totalDecValue = 0;
                foreach (var item in preShipmentDTO.PreShipmentItems)
                {
                    totalDecValue = totalDecValue + Convert.ToDecimal(item.Value);
                }
                preShipmentDTO.DeclarationOfValueCheck = totalDecValue;
                preShipmentDTO.Value = totalDecValue;

                if (wallet.Balance >= actualAmountToDebit)
                {
                    var gigGOServiceCenter = await _userService.GetGIGGOServiceCentre();

                    //generate waybill
                    var waybill = await _numberGeneratorMonitorService.GenerateNextNumber(NumberGeneratorType.WaybillNumber, gigGOServiceCenter.Code);
                    preShipmentDTO.Waybill = waybill;

                    var newPreShipment = Mapper.Map<PreShipmentMobile>(preShipmentDTO);

                    var message = new MobileShipmentCreationMessageDTO
                    {
                        SenderPhoneNumber = preShipmentDTO.SenderPhoneNumber,
                        WaybillNumber = preShipmentDTO.Waybill
                    };

                    if (user.UserChannelType == UserChannelType.Ecommerce)
                    {
                        newPreShipment.CustomerType = CustomerType.Company.ToString();
                        newPreShipment.CompanyType = CompanyType.Ecommerce.ToString();
                        message.SenderName = customer.Name;
                    }
                    else if (user.UserChannelType == UserChannelType.Corporate)
                    {
                        message.SenderName = customer.Name;
                    }
                    else
                    {
                        newPreShipment.CustomerType = "Individual";
                        newPreShipment.CompanyType = CustomerType.IndividualCustomer.ToString();

                        string[] words = preShipmentDTO.SenderName.Split(' ');
                        message.SenderName = words.FirstOrDefault();
                    }

                    if (newPreShipment.IsCashOnDelivery)
                    {

                        //collect the cods and add to CashOnDeliveryRegisterAccount()
                        var cashondeliveryentity = new CashOnDeliveryRegisterAccount
                        {
                            Amount = newPreShipment.CashOnDeliveryAmount ?? 0,
                            CODStatusHistory = CODStatushistory.Created,
                            Description = "Cod From Sales",
                            ServiceCenterId = 0,
                            Waybill = newPreShipment.Waybill,
                            UserId = newPreShipment.UserId,
                            DepartureServiceCenterId = preShipmentDTO.DepartureServiceCentreId,
                            DestinationCountryId = newPreShipment.CountryId
                        };
                        _uow.CashOnDeliveryRegisterAccount.Add(cashondeliveryentity);


                        newPreShipment.CODDescription = "COD Initiated";
                        newPreShipment.CODStatus = CODMobileStatus.Initiated;
                        newPreShipment.CODStatusDate = DateTime.Now;
                    }

                    newPreShipment.UserId = currentUserId;
                    newPreShipment.IsConfirmed = false;
                    newPreShipment.IsDelivered = false;
                    newPreShipment.shipmentstatus = "Shipment created";
                    newPreShipment.DateCreated = DateTime.Now;
                    newPreShipment.GrandTotal = shipmentGrandTotal;
                    newPreShipment.CustomerCode = user.UserChannelCode;
                    preShipmentDTO.IsBalanceSufficient = true;
                    preShipmentDTO.DiscountValue = PreshipmentPriceDTO.Discount;
                    newPreShipment.InternationalShippingCost = PreshipmentPriceDTO.InternationalShippingCost == null ? 0.00M : PreshipmentPriceDTO.InternationalShippingCost.Value;
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
                        UserId = newPreShipment.UserId,
                        TransactionCountryId = country.CountryId
                    };

                    if (transaction.CreditDebitType == CreditDebitType.Credit)
                    {
                        transaction.BalanceAfterTransaction = wallet.Balance + actualAmountToDebit;
                    }
                    else
                    {
                        transaction.BalanceAfterTransaction = wallet.Balance - actualAmountToDebit;
                    }

                    //update wallet
                    var updatedwallet = await _uow.Wallet.GetAsync(wallet.WalletId);

                    //double check in case something is wrong with the server before complete the transaction
                    if (updatedwallet.Balance < actualAmountToDebit)
                    {
                        preShipmentDTO.IsBalanceSufficient = false;
                        return preShipmentDTO;
                    }

                    decimal price = updatedwallet.Balance - actualAmountToDebit;
                    updatedwallet.Balance = price;
                    var walletTransaction = await _walletTransactionService.AddWalletTransaction(transaction);

                    //Pin Generation 
                    var number = await GenerateDeliveryCode();
                    var deliveryNumber = new DeliveryNumber
                    {
                        SenderCode = number,
                        IsUsed = false,
                        Waybill = waybill
                    };
                    _uow.DeliveryNumber.Add(deliveryNumber);

                    message.QRCode = deliveryNumber.SenderCode;

                    await _uow.CompleteAsync();

                    await ScanMobileShipment(new ScanDTO
                    {
                        WaybillNumber = newPreShipment.Waybill,
                        ShipmentScanStatus = ShipmentScanStatus.MCRT
                    });

                    await SendSMSForMobileShipmentCreation(message, MessageType.MCS);
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

        private async Task<PreShipmentMobileDTO> CreatePreShipmentThirdParty(PreShipmentMobileDTO preShipmentDTO)
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

                // get the current user info
                var currentUserId = await _userService.GetCurrentUserId();
                preShipmentDTO.UserId = currentUserId;
                var user = await _userService.GetUserById(currentUserId);
                preShipmentDTO.CustomerCode = user.UserChannelCode;

                var customer = await _uow.Company.GetAsync(s => s.CustomerCode == user.UserChannelCode);
                if (customer is null)
                {
                    throw new GenericException("customer is not an ecommerce user");
                }

                if (preShipmentDTO.IsCashOnDelivery && customer.Rank != Rank.Class)
                {
                    throw new GenericException("Cash On Delivery feature not available for  Basic ecommerce users; please upgrade to Class to have access to Cash On Delivery.");
                }
                var country = await _uow.Country.GetCountryByStationId(preShipmentDTO.SenderStationId);
                if (country == null)
                {
                    throw new GenericException("Sender Station Country Not Found", $"{(int)HttpStatusCode.NotFound}");
                }
                preShipmentDTO.CountryId = country.CountryId;

  
                if (customer != null)
                {
                    if (customer.IsEligible != true)
                    {
                        preShipmentDTO.IsEligible = false;
                        preShipmentDTO.IsCodNeeded = customer.isCodNeeded;
                        preShipmentDTO.CurrencySymbol = country.CurrencySymbol;
                        preShipmentDTO.CurrentWalletAmount = (decimal)customer.WalletAmount;
                        return preShipmentDTO;
                    }
                }

                var preshipmentPriceDTO = new MobilePriceDTO();
                if (preShipmentDTO.VehicleType.ToLower() == Vehicletype.Bike.ToString().ToLower() && preShipmentDTO.ZoneMapping == 1)
                {
                    preshipmentPriceDTO = await GetPriceForBike(preShipmentDTO);
                }
                else if (preShipmentDTO.VehicleType.ToLower() == Vehicletype.Truck.ToString().ToLower())
                {
                    preshipmentPriceDTO = await GetHaulagePrice(new HaulagePriceDTO
                    {
                        Haulageid = (preShipmentDTO.Haulageid != null) ? (int)preShipmentDTO.Haulageid : 0,
                        DepartureStationId = preShipmentDTO.SenderStationId,
                        DestinationStationId = preShipmentDTO.ReceiverStationId
                    });

                    preShipmentDTO.GrandTotal = (decimal)preshipmentPriceDTO.GrandTotal;

                    if (preShipmentDTO.PreShipmentItems.Any())
                    {
                        foreach (var shipment in preShipmentDTO.PreShipmentItems)
                        {
                            shipment.CalculatedPrice = preshipmentPriceDTO.GrandTotal;
                        }
                    }
                }
                else
                {
                    if (customer != null)
                    {
                        if (customer.CompanyType == CompanyType.Ecommerce)
                        {
                            preShipmentDTO.Shipmentype = ShipmentType.Ecommerce;
                        }
                    }

                    preShipmentDTO.IsFromShipment = true;
                    preshipmentPriceDTO = await GetPrice(preShipmentDTO);
                }

                decimal shipmentGrandTotal = (decimal)preshipmentPriceDTO.GrandTotal;
                var wallet = await _walletService.GetWalletBalance();
                if (wallet.Balance < shipmentGrandTotal && user.UserChannelType != UserChannelType.Corporate)
                {
                    preShipmentDTO.IsBalanceSufficient = false;
                    return preShipmentDTO;
                }
                else
                {
                    var gigGOServiceCenter = await _userService.GetGIGGOServiceCentre();
                    var newPreShipment = await ProcessWaybill(preShipmentDTO, gigGOServiceCenter.Code, user.UserChannelType);

                    var message = new MobileShipmentCreationMessageDTO
                    {
                        SenderPhoneNumber = preShipmentDTO.SenderPhoneNumber,
                        WaybillNumber = newPreShipment.Waybill,
                        SenderName = newPreShipment.SenderName
                    };

                    if (preShipmentDTO.IsCashOnDelivery)
                    {

                        //collect the cods and add to CashOnDeliveryRegisterAccount()
                        var cashondeliveryentity = new CashOnDeliveryRegisterAccount
                        {
                            Amount = preShipmentDTO.CashOnDeliveryAmount ?? 0,
                            CODStatusHistory = CODStatushistory.Created,
                            Description = "Cod From Sales",
                            ServiceCenterId = 0,
                            Waybill = newPreShipment.Waybill,
                            UserId = newPreShipment.UserId,
                            DepartureServiceCenterId = preShipmentDTO.DepartureServiceCentreId,
                            DestinationCountryId = newPreShipment.CountryId
                        };
                        _uow.CashOnDeliveryRegisterAccount.Add(cashondeliveryentity);


                        newPreShipment.CODDescription = "COD Initiated";
                        newPreShipment.CODStatus = CODMobileStatus.Initiated;
                        newPreShipment.CODStatusDate = DateTime.Now;
                        newPreShipment.IsCashOnDelivery = preShipmentDTO.IsCashOnDelivery;
                        newPreShipment.CashOnDeliveryAmount = preShipmentDTO.CashOnDeliveryAmount;
                    }

                    newPreShipment.UserId = currentUserId;
                    newPreShipment.GrandTotal = shipmentGrandTotal;
                    preShipmentDTO.IsBalanceSufficient = true;
                    preShipmentDTO.DiscountValue = preshipmentPriceDTO.Discount;
                    newPreShipment.ShipmentPickupPrice = (decimal)(preshipmentPriceDTO.PickUpCharge == null ? 0.0M : preshipmentPriceDTO.PickUpCharge);
                    if (customer.TransactionType == WalletTransactionType.BOT)
                    {
                        newPreShipment.shipmentstatus = MobilePickUpRequestStatus.Pending.ToString();
                    }
                    _uow.PreShipmentMobile.Add(newPreShipment);

                    if (customer.TransactionType != WalletTransactionType.BOT)
                    {

                        //process payment
                        var transaction = new WalletTransactionDTO
                        {
                            WalletId = wallet.WalletId,
                            CreditDebitType = CreditDebitType.Debit,
                            Amount = newPreShipment.GrandTotal,
                            ServiceCentreId = gigGOServiceCenter.ServiceCentreId,
                            Waybill = newPreShipment.Waybill,
                            Description = "Payment for Shipment",
                            PaymentType = (user.UserChannelType == UserChannelType.Corporate) ? PaymentType.Wallet : PaymentType.Online,
                            UserId = newPreShipment.UserId,
                            TransactionCountryId = country.CountryId
                        };

                        if (transaction.CreditDebitType == CreditDebitType.Credit)
                        {
                            transaction.BalanceAfterTransaction = wallet.Balance + transaction.Amount;
                        }
                        else
                        {
                            transaction.BalanceAfterTransaction = wallet.Balance - transaction.Amount;
                        }

                        //update wallet
                        var updatedwallet = await _uow.Wallet.GetAsync(wallet.WalletId);
                        //double check in case something is wrong with the server before complete the transaction
                        if (updatedwallet.Balance < shipmentGrandTotal && user.UserChannelType != UserChannelType.Corporate)
                        {
                            preShipmentDTO.IsBalanceSufficient = false;
                            return preShipmentDTO;
                        }
                        decimal price = updatedwallet.Balance - shipmentGrandTotal;
                        updatedwallet.Balance = price;
                        var walletTransaction = await _walletTransactionService.AddWalletTransaction(transaction);
                    }

                    else
                    {
                        //generate paymentlink 
                        var waybillPayment = new WaybillPaymentLogDTO()
                        {
                            Waybill = newPreShipment.Waybill,
                            OnlinePaymentType = OnlinePaymentType.Paystack,
                            Email = customer.Email,
                            Amount = newPreShipment.GrandTotal
                        };
                        waybillPayment.PaymentCountryId = customer.UserActiveCountryId;
                        waybillPayment.PaystackCountrySecret = "PayStackSecret";
                        var response = await _waybillPaymentLogService.PayForIntlShipmentUsingPaystack(waybillPayment);
                        preShipmentDTO.PaymentUrl = response.data.Authorization_url;
                    }

                    //Pin Generation 
                    var number = await GenerateDeliveryCode();
                    var deliveryNumber = new DeliveryNumber
                    {
                        SenderCode = number,
                        IsUsed = false,
                        Waybill = newPreShipment.Waybill
                    };
                    _uow.DeliveryNumber.Add(deliveryNumber);

                    message.QRCode = deliveryNumber.SenderCode;

                    await _uow.CompleteAsync();
                    await ScanMobileShipment(new ScanDTO
                    {
                        WaybillNumber = newPreShipment.Waybill,
                        ShipmentScanStatus = ShipmentScanStatus.MCRT
                    });

                    //Fire and forget
                    //Send the Payload to Partner Cloud Handler 
                    if (!preShipmentDTO.IsBatchPickUp && customer.TransactionType != WalletTransactionType.BOT)
                    {
                        await NodeApiCreateShipment(newPreShipment);
                    }

                    //We will send SMS & Email
                    await SendSMSForMobileShipmentCreation(message, MessageType.MCS);
                    return preShipmentDTO;
                }
            }
            catch
            {
                throw;
            }
        }

        private async Task<PreShipmentMobile> ProcessWaybill(PreShipmentMobileDTO preShipmentDTO, string gigGOServiceCentercode, UserChannelType userChannelType)
        {
            //generate waybill
            var waybill = await _numberGeneratorMonitorService.GenerateNextNumber(NumberGeneratorType.WaybillNumber, gigGOServiceCentercode);
            preShipmentDTO.Waybill = waybill;

            var newPreShipment = Mapper.Map<PreShipmentMobile>(preShipmentDTO);

            newPreShipment.IsConfirmed = false;
            newPreShipment.IsDelivered = false;
            newPreShipment.shipmentstatus = "Shipment created";
            newPreShipment.DateCreated = DateTime.Now;

            if (userChannelType == UserChannelType.Ecommerce)
            {
                newPreShipment.CustomerType = CustomerType.Company.ToString();
                newPreShipment.CompanyType = CompanyType.Ecommerce.ToString();
            }
            else if (userChannelType == UserChannelType.Corporate)
            {
                newPreShipment.CustomerType = CustomerType.Company.ToString();
                newPreShipment.CompanyType = CompanyType.Corporate.ToString();
            }
            else
            {
                newPreShipment.CustomerType = "Individual";
                newPreShipment.CompanyType = CustomerType.IndividualCustomer.ToString();
            }
            if (preShipmentDTO.DestinationServiceCentreId > 0)
            {
                 var centre = await _uow.ServiceCentre.GetAsync(x => x.ServiceCentreId == preShipmentDTO.DestinationServiceCentreId);
                if (centre != null)
                {
                    newPreShipment.DestinationServiceCenterId = centre.ServiceCentreId;
                    newPreShipment.InputtedReceiverAddress = preShipmentDTO.ReceiverAddress;
                    newPreShipment.ReceiverAddress = centre.FormattedServiceCentreName;
                    newPreShipment.IsHomeDelivery = false;
                    newPreShipment.ReceiverLocation.Latitude = centre.Latitude;
                    newPreShipment.ReceiverLocation.Longitude = centre.Longitude;
                }
            }

            if (preShipmentDTO.DestinationServiceCenterId > 0)
            {
                var centre = await _uow.ServiceCentre.GetAsync(x => x.ServiceCentreId == preShipmentDTO.DestinationServiceCenterId);
                if (centre != null)
                {
                    newPreShipment.DestinationServiceCenterId = centre.ServiceCentreId;
                    newPreShipment.InputtedReceiverAddress = preShipmentDTO.ReceiverAddress;
                    newPreShipment.ReceiverAddress = centre.FormattedServiceCentreName;
                    newPreShipment.IsHomeDelivery = false;
                    newPreShipment.ReceiverLocation.Latitude = centre.Latitude;
                    newPreShipment.ReceiverLocation.Longitude = centre.Longitude;
                }
            }

            return newPreShipment;

        }

        private async Task<Uri> NodeApiCreateShipmentOld(PreShipmentMobile newPreShipment)
        {
            try
            {
                var nodePayload = new CreateShipmentNodeDTO()
                {
                    waybillNumber = newPreShipment.Waybill,
                    customerId = newPreShipment.CustomerCode,
                    locality = newPreShipment.SenderLocality,
                    receiverAddress = newPreShipment.ReceiverAddress,
                    vehicleType = newPreShipment.VehicleType,
                    value = newPreShipment.Value,
                    zone = newPreShipment.ZoneMapping,
                    receiverLocation = new NodeLocationDTO()
                    {
                        lng = newPreShipment.ReceiverLocation.Longitude,
                        lat = newPreShipment.ReceiverLocation.Latitude
                    },
                    senderAddress = newPreShipment.SenderAddress,
                    senderLocation = new NodeLocationDTO()
                    {
                        lng = newPreShipment.SenderLocation.Longitude,
                        lat = newPreShipment.SenderLocation.Latitude
                    }
                };

                HttpClient client = new HttpClient();

                var nodeURL = ConfigurationManager.AppSettings["NodeTestUrl"];
                nodeURL = nodeURL + "shipment";

                HttpResponseMessage response = await client.PostAsJsonAsync(nodeURL, nodePayload);
                response.EnsureSuccessStatusCode();

                // return URI of the created resource.
                return response.Headers.Location;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task NodeApiCreateShipment(PreShipmentMobile newPreShipment)
        {
            try
            {
                var nodePayload = new CreateShipmentNodeDTO()
                {
                    waybillNumber = newPreShipment.Waybill,
                    customerId = newPreShipment.CustomerCode,
                    locality = newPreShipment.SenderLocality,
                    receiverAddress = newPreShipment.ReceiverAddress,
                    vehicleType = newPreShipment.VehicleType,
                    value = newPreShipment.Value,
                    zone = newPreShipment.ZoneMapping,
                    receiverLocation = new NodeLocationDTO()
                    {
                        lng = newPreShipment.ReceiverLocation.Longitude,
                        lat = newPreShipment.ReceiverLocation.Latitude
                    },
                    senderAddress = newPreShipment.SenderAddress,
                    senderLocation = new NodeLocationDTO()
                    {
                        lng = newPreShipment.SenderLocation.Longitude,
                        lat = newPreShipment.SenderLocation.Latitude
                    }
                };

                await _nodeService.CreateShipment(nodePayload);
            }
            catch (Exception)
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

            var country = await _uow.Country.GetCountryByStationId(preShipmentMobileDTO.SenderStationId);
            if (country == null)
            {
                throw new GenericException("Sender Station Country Not Found", $"{(int)HttpStatusCode.NotFound}");
            }
            preShipmentMobileDTO.CountryId = country.CountryId;
            preShipmentMobileDTO.CurrencyCode = country.CurrencyCode;
            preShipmentMobileDTO.CurrencySymbol = country.CurrencySymbol;
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
                    preShipmentMobileDTO.CurrencySymbol = country.CurrencySymbol;
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
            catch (Exception)
            {
                throw;
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

            if (preShipmentDTO.PreShipmentItems.Any())
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
            // get the current user info
            var currentUserId = await _userService.GetCurrentUserId();
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

                newPreShipment.UserId = currentUserId;
                newPreShipment.IsConfirmed = false;
                newPreShipment.IsDelivered = false;
                newPreShipment.shipmentstatus = "Shipment created";
                newPreShipment.DateCreated = DateTime.Now;
                newPreShipment.GrandTotal = receiver.GrandTotal;
                newPreShipment.CalculatedTotal = receiver.CalculatedTotal;
                receiver.IsBalanceSufficient = true;
                newPreShipment.DiscountValue = receiver.DiscountValue;
                newPreShipment.ShipmentPickupPrice = individualPickupPrice;
                var country = await _uow.Country.GetCountryByStationId(receiver.SenderStationId);
                if (country == null)
                {
                    throw new GenericException("Sender Station Country Not Found", $"{(int)HttpStatusCode.NotFound}");
                }
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
                    UserId = newPreShipment.UserId,
                    TransactionCountryId = country.CountryId
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
                if (preShipment == null)
                {
                    throw new GenericException("Invalid Data");
                }

                if (!preShipment.PreShipmentItems.Any())
                {
                    throw new GenericException("Preshipment Item Not Found");
                }
                if (preShipment.IsFromAgility)
                {
                    var userDTO = await _userService.GetUserUsingCustomerForCustomerPortal(preShipment.CustomerCode);
                    preShipment.UserId = userDTO.Id;
                }
                else
                {
                    var userId = await _userService.GetCurrentUserId();
                    preShipment.UserId = userId;
                }

                var zoneid = await _domesticroutezonemapservice.GetZoneMobile(preShipment.SenderStationId, preShipment.ReceiverStationId);

                var country = await _uow.Country.GetCountryByStationId(preShipment.SenderStationId);
                if (country == null)
                {
                    throw new GenericException("Sender Station Country Not Found", $"{(int)HttpStatusCode.NotFound}");
                }
                preShipment.CountryId = country.CountryId;

                //change the quantity of the preshipmentItem if it fall into promo category
                preShipment = await ChangePreshipmentItemQuantity(preShipment, zoneid.ZoneId);

                var Price = 0.0M;
                var amount = 0.0M;
                var IndividualPrice = 0.0M;
                decimal DeclaredValue = 0.0M;

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
                if (preShipment.IsFromAgility)
                {
                    var userChannel = await _uow.Company.GetAsync(x => x.CustomerCode == preShipment.CustomerCode);
                    if (userChannel != null)
                    {
                        preShipment.Shipmentype = ShipmentType.Ecommerce;
                    }
                }
                else
                {
                    var userChannelCode = await _userService.GetUserChannelCode();
                    preShipment.CustomerCode = userChannelCode;
                    var userChannel = await _uow.Company.GetAsync(x => x.CustomerCode == userChannelCode);
                    if (userChannel != null)
                    {
                        preShipment.Shipmentype = ShipmentType.Ecommerce;
                    }
                }

                //Get the vat value from VAT
                var vatDTO = await _uow.VAT.GetAsync(x => x.CountryId == preShipment.CountryId);
                decimal vat = (vatDTO != null) ? (vatDTO.Value / 100) : (7.5M / 100);

                foreach (var preShipmentItem in preShipment.PreShipmentItems)
                {
                    if (preShipmentItem.Quantity == 0)
                    {
                        throw new GenericException("Item Quantity cannot be zero");
                    }

                    if (preShipmentItem.SpecialPackageId == null)
                    {
                        preShipmentItem.SpecialPackageId = 0;
                    }

                    var PriceDTO = new PricingDTO
                    {
                        DepartureStationId = preShipment.SenderStationId,
                        DestinationStationId = preShipment.ReceiverStationId,
                        Weight = preShipmentItem.Weight,
                        SpecialPackageId = (int)preShipmentItem.SpecialPackageId,
                        ShipmentType = preShipmentItem.ShipmentType,
                        CountryId = preShipment.CountryId,
                        CustomerCode = preShipment.CustomerCode
                    };

                    if (preShipmentItem.ShipmentType == ShipmentType.Special)
                    {
                        if (preShipment.Shipmentype == ShipmentType.Ecommerce)
                        {
                            //PriceDTO.DeliveryOptionId = 4;
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

                        preShipmentItem.CalculatedPrice = await _pricingService.GetMobileRegularPrice(PriceDTO);
                        preShipmentItem.CalculatedPrice = preShipmentItem.CalculatedPrice * preShipmentItem.Quantity;
                        preShipmentItem.CalculatedPrice = preShipmentItem.CalculatedPrice + IndividualPrice;

                        //if (preShipment.Shipmentype == ShipmentType.Ecommerce)
                        //{
                        //    preShipmentItem.CalculatedPrice = await _pricingService.GetMobileEcommercePrice(PriceDTO);
                        //    preShipmentItem.CalculatedPrice = preShipmentItem.CalculatedPrice * preShipmentItem.Quantity;
                        //    preShipmentItem.CalculatedPrice = preShipmentItem.CalculatedPrice + IndividualPrice;
                        //}
                        //else
                        //{
                        //    preShipmentItem.CalculatedPrice = await _pricingService.GetMobileRegularPrice(PriceDTO);
                        //    preShipmentItem.CalculatedPrice = preShipmentItem.CalculatedPrice * preShipmentItem.Quantity;
                        //    preShipmentItem.CalculatedPrice = preShipmentItem.CalculatedPrice + IndividualPrice;
                        //}
                    }

                    var vatForPreshipment = (preShipmentItem.CalculatedPrice * vat);

                    if (!string.IsNullOrWhiteSpace(preShipmentItem.Value))
                    {
                        DeclaredValue += Convert.ToDecimal(preShipmentItem.Value);
                        var DeclaredValueForPreShipment = Convert.ToDecimal(preShipmentItem.Value);
                        //if (preShipment.Shipmentype == ShipmentType.Ecommerce)
                        //{
                        //    preShipmentItem.CalculatedPrice = preShipmentItem.CalculatedPrice + (DeclaredValueForPreShipment * 0.01M);
                        //}
                        //else
                        //{
                        //    preShipmentItem.CalculatedPrice = preShipmentItem.CalculatedPrice + (DeclaredValueForPreShipment * 0.01M) + vatForPreshipment;
                        //}
                        preShipmentItem.CalculatedPrice = preShipmentItem.CalculatedPrice + (DeclaredValueForPreShipment * 0.01M) + vatForPreshipment;
                        preShipment.IsdeclaredVal = true;
                    }
                    else
                    {
                        preShipmentItem.CalculatedPrice = preShipmentItem.CalculatedPrice + vatForPreshipment;
                        //if (preShipment.Shipmentype != ShipmentType.Ecommerce)
                        //{
                        //    preShipmentItem.CalculatedPrice = preShipmentItem.CalculatedPrice + vatForPreshipment;
                        //}
                    }

                    preShipmentItem.CalculatedPrice = (decimal)Math.Round((double)preShipmentItem.CalculatedPrice);
                    Price += (decimal)preShipmentItem.CalculatedPrice;
                };

                decimal percentage = 0.00M;
                if (!string.IsNullOrWhiteSpace(preShipment.VehicleType))
                {
                    if (preShipment.VehicleType.ToLower() == Vehicletype.Truck.ToString().ToLower())
                    {
                        percentage = 0.00M;
                    }
                    else
                    {
                        var discountPercent = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.DiscountPercentage, preShipment.CountryId);
                        percentage = Convert.ToDecimal(discountPercent.Value);
                    }
                }

                var percentageTobeUsed = ((100M - percentage) / 100M);
                decimal estimatedDeclaredPrice = preShipment.IsFromAgility ? Convert.ToDecimal(preShipment.Value) : Convert.ToDecimal(DeclaredValue);
                preShipment.DeliveryPrice = Price * percentageTobeUsed;
                preShipment.InsuranceValue = (estimatedDeclaredPrice * 0.01M);
                //preShipment.CalculatedTotal = (double)(preShipment.DeliveryPrice);
                preShipment.CalculatedTotal = (double)(Price);
                preShipment.CalculatedTotal = Math.Round((double)preShipment.CalculatedTotal);
                preShipment.Value = DeclaredValue;
                //var discount = Math.Round(Price - (decimal)preShipment.CalculatedTotal);
                var discount = Math.Round(Price - (decimal)preShipment.DeliveryPrice);
                preShipment.DiscountValue = discount;

                var Pickuprice = await GetPickUpPrice(preShipment.VehicleType, preShipment.CountryId, preShipment.UserId);
                var PickupValue = Convert.ToDecimal(Pickuprice);

                var IsWithinProcessingTime = await WithinProcessingTime(preShipment.CountryId);

                // decimal grandTotal = (decimal)preShipment.CalculatedTotal + PickupValue;
                decimal grandTotal = (decimal)preShipment.DeliveryPrice + PickupValue;

                // Special Discount Price
                //1. Check sender station eligibility
                var isSenderStationEligible = await CheckEligibleStation(preShipment.CountryId, preShipment.SenderStationId);

                if (isSenderStationEligible)
                {

                    //2. Get special discount price
                    var specialStationDiscount = await CalculateSpecialDiscount(preShipment, grandTotal, PickupValue);

                    grandTotal = (decimal)specialStationDiscount.GrandTotal.Value;
                    discount = (decimal)specialStationDiscount.DiscountValue;
                    preShipment.DiscountValue = discount;
                    preShipment.GrandTotal = grandTotal;
                }
                else
                {
                    //GIG Go Promo Price
                    var gigGoPromo = await CalculatePromoPrice(preShipment, zoneid.ZoneId, PickupValue);
                    if (gigGoPromo.GrandTotal > 0)
                    {
                        grandTotal = (decimal)gigGoPromo.GrandTotal;
                        discount = (decimal)gigGoPromo.Discount;
                        preShipment.DiscountValue = discount;
                        preShipment.GrandTotal = grandTotal;
                    }
                }


                var returnprice = new MobilePriceDTO()
                {
                    MainCharge = (decimal)preShipment.CalculatedTotal,
                    DeliveryPrice = preShipment.DeliveryPrice,
                    PickUpCharge = PickupValue,
                    InsuranceValue = preShipment.InsuranceValue,
                    GrandTotal = grandTotal,
                    PreshipmentMobile = preShipment,
                    CurrencySymbol = country.CurrencySymbol,
                    CurrencyCode = country.CurrencyCode,
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

        //Get Price for Bike Shipments within a city
        public async Task<MobilePriceDTO> GetPriceForBike(PreShipmentMobileDTO preShipment)
        {
            try
            {
                if (preShipment == null)
                {
                    throw new GenericException("Invalid Data");
                }

                if (!preShipment.PreShipmentItems.Any())
                {
                    throw new GenericException($"Shipment Items cannot be empty", $"{(int)HttpStatusCode.Forbidden}");
                }

                var zoneid = preShipment.ZoneMapping != null ? (int)preShipment.ZoneMapping : 0;

                var country = await _uow.Country.GetCountryByStationId(preShipment.SenderStationId);
                if (country == null)
                {
                    throw new GenericException("Sender Station Country Not Found", $"{(int)HttpStatusCode.NotFound}");
                }
                preShipment.CountryId = country.CountryId;

                var currentUserId = await _userService.GetCurrentUserId();
                preShipment.UserId = currentUserId;
                var user = await _userService.GetUserById(currentUserId);
                preShipment.CustomerCode = user.UserChannelCode;

                var basePriceBike = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.BikeBasePrice, preShipment.CountryId);
                var basePriceBikeValue = Convert.ToDecimal(basePriceBike.Value);

                //change the quantity of the preshipmentItem if it fall into promo category
                preShipment = await ChangePreshipmentItemQuantity(preShipment, zoneid);

                decimal discount = 0.0M;
                var amount = await CalculateBikePriceBasedonLocation(preShipment);

                decimal pickuprice = 0.0M;  //await GetPickUpPrice(preShipment.VehicleType, preShipment.CountryId, preShipment.UserId);
                decimal pickupValue = 0.0M; // Convert.ToDecimal(pickuprice);
                decimal mainCharge = basePriceBikeValue + amount;
                decimal percentage = 0.0M;

                //Get the customer Types
                if (preShipment.IsFromAgility)
                {
                    var userChannel = await _uow.Company.GetAsync(x => x.CustomerCode == preShipment.CustomerCode);
                    if (userChannel != null)
                    {
                        if (userChannel.CompanyType == CompanyType.Ecommerce)
                        {
                            preShipment.Shipmentype = ShipmentType.Ecommerce;
                        }
                    }
                }
                else
                {
                    preShipment.Shipmentype = await GetEcommerceCustomerShipmentType(preShipment.Shipmentype);
                }

                //Get Discount based on Customer Rank
                var priceDTO = new PricingDTO
                {
                    CountryId = preShipment.CountryId,
                    CustomerCode = preShipment.CustomerCode
                };
                mainCharge = await _pricingService.CalculateCustomerRankPrice(priceDTO, mainCharge);

                var discountPercent = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.DiscountBikePercentage, preShipment.CountryId);
                percentage = Convert.ToDecimal(discountPercent.Value);
                //if (preShipment.Shipmentype == ShipmentType.Ecommerce)
                //{
                //    var discountPercent = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.EcommerceGIGGOIntraStateBikeDiscount, preShipment.CountryId);
                //    percentage = Convert.ToDecimal(discountPercent.Value);

                //else
                //{
                //    var discountPercent = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.DiscountBikePercentage, preShipment.CountryId);
                //    percentage = Convert.ToDecimal(discountPercent.Value);
                //}

                var percentageTobeUsed = ((100M - percentage) / 100M);
                var calculatedTotal = (double)(mainCharge * percentageTobeUsed);
                calculatedTotal = Math.Round(calculatedTotal);
                preShipment.DeliveryPrice = (decimal)calculatedTotal;

                discount = Math.Round(mainCharge - (decimal)calculatedTotal);
                decimal grandTotal = (decimal)calculatedTotal + pickupValue;

                // Special Discount Price
                //1. Check sender station eligibility
                var isSenderStationEligible = await CheckEligibleStation(preShipment.CountryId, preShipment.SenderStationId);

                if (isSenderStationEligible)
                {
                    //2. Get special discount price
                    var specialStationDiscount = await CalculateSpecialDiscount(preShipment, grandTotal, pickupValue);

                    grandTotal = (decimal)specialStationDiscount.GrandTotal.Value;
                    discount = (decimal)specialStationDiscount.DiscountValue;
                    preShipment.DiscountValue = discount;
                    preShipment.GrandTotal = grandTotal;
                }
                else
                {
                    //GIG Go Promo Price
                    var gigGoPromo = await CalculatePromoPrice(preShipment, zoneid, pickupValue);
                    if (gigGoPromo.GrandTotal > 0)
                    {
                        grandTotal = (decimal)gigGoPromo.GrandTotal;
                        discount = (decimal)gigGoPromo.Discount;
                        preShipment.DiscountValue = discount;
                        preShipment.GrandTotal = grandTotal;
                    }
                }

                var countOfItems = preShipment.PreShipmentItems.Count;
                var individualPrice = grandTotal / countOfItems;

                foreach (var preShipmentItem in preShipment.PreShipmentItems)
                {
                    preShipmentItem.CalculatedPrice = individualPrice;
                };

                var IsWithinProcessingTime = await WithinProcessingTime(preShipment.CountryId);
                var returnprice = new MobilePriceDTO()
                {
                    MainCharge = mainCharge,
                    DeliveryPrice = preShipment.DeliveryPrice,
                    Vat = 0.0M,
                    PickUpCharge = pickuprice,
                    InsuranceValue = 0.0M,
                    GrandTotal = grandTotal,
                    PreshipmentMobile = preShipment,
                    CurrencySymbol = country.CurrencySymbol,
                    CurrencyCode = country.CurrencyCode,
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

        private async Task<ShipmentType> GetEcommerceCustomerShipmentType(ShipmentType shipmentType)
        {
            //Get the customer Type
            var userChannelCode = await _userService.GetUserChannelCode();
            var userChannel = await _uow.Company.GetAsync(x => x.CustomerCode == userChannelCode);

            if (userChannel != null)
            {
                if (userChannel.CompanyType == CompanyType.Ecommerce)
                {
                    shipmentType = ShipmentType.Ecommerce;
                }
            }

            return shipmentType;
        }

        public async Task<MobilePriceDTO> GetPriceForDropOff(PreShipmentMobileDTO preShipment)
        {
            try
            {
                if (preShipment == null)
                {
                    throw new GenericException("Invalid Data");
                }

                if (!preShipment.PreShipmentItems.Any())
                {
                    throw new GenericException("Preshipment Item Not Found");
                }

                var zoneid = await _domesticroutezonemapservice.GetZoneMobile(preShipment.SenderStationId, preShipment.ReceiverStationId);

                var country = await _uow.Country.GetCountryByStationId(preShipment.SenderStationId);
                if (country == null)
                {
                    throw new GenericException("Sender Station Country Not Found", $"{(int)HttpStatusCode.NotFound}");
                }
                preShipment.CountryId = country.CountryId;

                var Price = 0.0M;
                var amount = 0.0M;
                var IndividualPrice = 0.0M;
                decimal DeclaredValue = 0.0M;

                int deliveryOption = 10;
                if (preShipment.IsHomeDelivery)
                {
                    deliveryOption = 2;
                }

                //Get the customer Type
                var userChannelCode = await _userService.GetUserChannelCode();
                var userChannel = await _uow.Company.GetAsync(x => x.CustomerCode == userChannelCode);

                if (userChannel != null)
                {
                    if (userChannel.CompanyType == CompanyType.Ecommerce)
                    {
                        //deliveryOption = 4;
                        preShipment.Shipmentype = ShipmentType.Ecommerce;
                    }
                }

                //Get VAT
                var vatDTO = await _uow.VAT.GetAsync(x => x.CountryId == preShipment.CountryId);
                decimal vat = (vatDTO != null) ? (vatDTO.Value / 100) : (7.5M / 100);

                foreach (var preShipmentItem in preShipment.PreShipmentItems)
                {
                    if (preShipmentItem.Quantity == 0)
                    {
                        throw new GenericException("Item Quantity cannot be zero");
                    }

                    var PriceDTO = new PricingDTO
                    {
                        DepartureStationId = preShipment.SenderStationId,
                        DestinationStationId = preShipment.ReceiverStationId,
                        Weight = preShipmentItem.Weight,
                        SpecialPackageId = (int)preShipmentItem.SpecialPackageId,
                        ShipmentType = preShipmentItem.ShipmentType,
                        DeliveryOptionId = deliveryOption,
                        CountryId = preShipment.CountryId,
                        CustomerCode = userChannelCode
                    };


                    if (preShipmentItem.ShipmentType == ShipmentType.Special)
                    {
                        //if (preShipment.Shipmentype == ShipmentType.Ecommerce)
                        //{
                        //    PriceDTO.DeliveryOptionId = 4;
                        //}

                        preShipmentItem.CalculatedPrice = await _pricingService.GetDropOffSpecialPrice(PriceDTO);
                        preShipmentItem.CalculatedPrice = preShipmentItem.CalculatedPrice * preShipmentItem.Quantity;
                        preShipmentItem.CalculatedPrice = preShipmentItem.CalculatedPrice + IndividualPrice;
                    }
                    else if (preShipmentItem.ShipmentType == ShipmentType.Regular)
                    {
                        if (preShipmentItem.Weight == 0)
                        {
                            throw new GenericException("Item weight cannot be zero");
                        }

                        preShipmentItem.CalculatedPrice = await _pricingService.GetDropOffRegularPriceForIndividual(PriceDTO);
                        preShipmentItem.CalculatedPrice = preShipmentItem.CalculatedPrice * preShipmentItem.Quantity;
                        preShipmentItem.CalculatedPrice = preShipmentItem.CalculatedPrice + IndividualPrice;

                        //if (preShipment.Shipmentype == ShipmentType.Ecommerce)
                        //{
                        //    preShipmentItem.CalculatedPrice = await _pricingService.GetEcommerceDropOffPrice(PriceDTO);
                        //    preShipmentItem.CalculatedPrice = preShipmentItem.CalculatedPrice * preShipmentItem.Quantity;
                        //    preShipmentItem.CalculatedPrice = preShipmentItem.CalculatedPrice + IndividualPrice;
                        //}
                        //else
                        //{
                        //    preShipmentItem.CalculatedPrice = await _pricingService.GetDropOffRegularPriceForIndividual(PriceDTO);
                        //    preShipmentItem.CalculatedPrice = preShipmentItem.CalculatedPrice * preShipmentItem.Quantity;
                        //    preShipmentItem.CalculatedPrice = preShipmentItem.CalculatedPrice + IndividualPrice;
                        //}
                    }

                    var vatForPreshipment = (preShipmentItem.CalculatedPrice * vat);

                    if (!string.IsNullOrWhiteSpace(preShipmentItem.Value))
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

                decimal EstimatedDeclaredPrice = Convert.ToDecimal(DeclaredValue);
                preShipment.DeliveryPrice = Price;
                preShipment.InsuranceValue = (EstimatedDeclaredPrice * 0.01M);
                preShipment.CalculatedTotal = (double)(preShipment.DeliveryPrice);
                preShipment.CalculatedTotal = Math.Round((double)preShipment.CalculatedTotal);
                preShipment.Value = DeclaredValue;
                var discount = Math.Round(Price - (decimal)preShipment.CalculatedTotal);
                preShipment.DiscountValue = discount;

                decimal grandTotal = (decimal)preShipment.CalculatedTotal;

                var returnprice = new MobilePriceDTO()
                {
                    MainCharge = (decimal)preShipment.CalculatedTotal,
                    InsuranceValue = preShipment.InsuranceValue,
                    GrandTotal = grandTotal,
                    PreshipmentMobile = preShipment,
                    CurrencySymbol = country.CurrencySymbol,
                    CurrencyCode = country.CurrencyCode,
                    Discount = discount
                };

                if (preShipment.DeliveryType == DeliveryType.GOFASTER)
                {
                    var faster = await _uow.GlobalProperty.GetAsync(x => x.Key == GlobalPropertyType.GoFaster.ToString());
                    if (faster != null)
                    {
                        var fasterValue = Convert.ToDecimal(faster.Value);
                        var num = fasterValue / 100M;
                        var numAmount = returnprice.GrandTotal * num;
                        returnprice.GrandTotal = returnprice.GrandTotal + numAmount;
                    }
                }

                return returnprice;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<MobilePriceDTO> GetPriceForResolveDispute(PreShipmentMobileDTO preShipment, decimal pickupPrice)
        {
            try
            {
                if (!preShipment.PreShipmentItems.Any())
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
                    int ShipmentCount = preShipment.PreShipmentItems.Count;

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
                    int ShipmentCount = preShipment.PreShipmentItems.Count;
                    amount = await CalculateGeoDetailsBasedonLocation(preShipment);
                    IndividualPrice = (amount / ShipmentCount);
                }

                foreach (var preShipmentItem in preShipment.PreShipmentItems)
                {
                    if (preShipmentItem.Quantity == 0)
                    {
                        throw new GenericException($"Quantity cannot be zero for {preShipmentItem.ItemName}");
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
                            throw new GenericException($"Item weight cannot be zero for {preShipmentItem.ItemName}");
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
                var Percentage = Convert.ToDecimal(DiscountPercent.Value);
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
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<PreShipmentMobileDTO>> GetShipments(BaseFilterCriteria filterOptionsDto)
        {
            try
            {
                //Excluding It Test
                string excludeUserList = ConfigurationManager.AppSettings["excludeUserList"];
                string[] testUserId = excludeUserList.Split(',').ToArray();

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

                if (filterOptionsDto.StationId > 0)
                {
                    allShipmentsResult = allShipmentsResult.Where(s => s.DateCreated >= startDate && s.DateCreated < endDate
                                        && s.SenderStationId == filterOptionsDto.StationId && !testUserId.Contains(s.UserId));
                }
                else
                {
                    allShipmentsResult = allShipmentsResult.Where(s => s.DateCreated >= startDate && s.DateCreated < endDate && !testUserId.Contains(s.UserId));
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
                                                              VehicleType = r.VehicleType,
                                                              IsScheduled = r.IsScheduled,
                                                              ScheduledDate = r.ScheduledDate,
                                                              SenderLocality = r.SenderLocality,
                                                              CashOnDeliveryAmount = r.CashOnDeliveryAmount,
                                                              IsApproved = r.IsApproved,
                                                              IsBatchPickUp = r.IsBatchPickUp,
                                                              IsCashOnDelivery = r.IsCashOnDelivery
                                                          }).ToList();

                if (shipmentDto.Any())
                {
                    foreach (var item in shipmentDto)
                    {
                        if (item.CustomerType == CustomerType.IndividualCustomer.ToString())
                        {
                            var user = await _uow.IndividualCustomer.GetAsync(x => x.CustomerCode == item.CustomerCode);
                            if (user != null)
                            {
                                item.CustomerName = $"{user.FirstName} {user.LastName}";
                            }
                        }
                        else if (item.CustomerType == CustomerType.Company.ToString())
                        {
                            var user = await _uow.Company.GetAsync(x => x.CustomerCode == item.CustomerCode);
                            if (user != null)
                            {
                                item.CustomerName = $"{user.Name}";
                            }
                        }
                        else if (item.CustomerType == CustomerType.Partner.ToString())
                        {
                            var user = await _uow.Partner.GetAsync(x => x.PartnerCode == item.CustomerCode);
                            if (user != null)
                            {
                                item.CustomerName = $"{user.FirstName} {user.LastName}";
                            }
                        }
                    }
                }

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
                if (!String.IsNullOrEmpty(shipmentDTO.CustomerCode))
                {
                    if (shipmentDTO.CustomerType == CustomerType.IndividualCustomer.ToString())
                    {
                        var user = await _uow.IndividualCustomer.GetAsync(x => x.CustomerCode == shipmentDTO.CustomerCode);
                        if (user != null)
                        {
                            shipmentDTO.CustomerName = $"{user.FirstName} {user.LastName}";
                        }
                    }
                    else if (shipmentDTO.CustomerType == CustomerType.Company.ToString())
                    {
                        var user = await _uow.Company.GetAsync(x => x.CustomerCode == shipmentDTO.CustomerCode);
                        if (user != null)
                        {
                            shipmentDTO.CustomerName = $"{user.Name}";
                        }
                    }
                    else if (shipmentDTO.CustomerType == CustomerType.Partner.ToString())
                    {
                        var user = await _uow.Partner.GetAsync(x => x.PartnerCode == shipmentDTO.CustomerCode);
                        if (user != null)
                        {
                            shipmentDTO.CustomerName = $"{user.FirstName} {user.LastName}";
                        }
                    }
                }

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

                    //if (user.UserChannelType.ToString() != UserChannelType.Partner.ToString() || user.SystemUserRole != "Dispatch Rider")
                    //{
                    //    var qrCode = await _uow.DeliveryNumber.GetAsync(x => x.Waybill == shipment.Waybill);
                    //    if (qrCode != null)
                    //    {
                    //        Shipmentdto.QRCode = qrCode.SenderCode;
                    //    }
                    //}

                    var codes = await _uow.DeliveryNumber.GetAsync(x => x.Waybill == waybill);
                    if (codes != null)
                    {
                        Shipmentdto.QRCode = codes.SenderCode;
                        Shipmentdto.SenderCode = codes.SenderCode;
                        Shipmentdto.ReceiverCode = codes.ReceiverCode;
                    }
                }
                else
                {
                    var agilityshipment = await _uow.Shipment.GetAsync(x => x.Waybill == waybill, "ShipmentItems");
                    if (agilityshipment != null)
                    {
                        Shipmentdto = Mapper.Map<PreShipmentMobileDTO>(agilityshipment);

                        if (agilityshipment.CustomerType.Contains("Individual"))
                        {
                            agilityshipment.CustomerType = CustomerType.IndividualCustomer.ToString();
                        }

                        CustomerType customerType = (CustomerType)Enum.Parse(typeof(CustomerType), agilityshipment.CustomerType);
                        var CustomerDetails = await _customerService.GetCustomer(agilityshipment.CustomerId, customerType);

                        Shipmentdto.SenderAddress = CustomerDetails.Address;
                        Shipmentdto.SenderName = CustomerDetails.CustomerName;
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

                        var qrCode = await _uow.DeliveryNumber.GetAsync(x => x.Waybill == agilityshipment.Waybill);
                        if (qrCode != null)
                        {
                            Shipmentdto.QRCode = qrCode.SenderCode;
                            Shipmentdto.SenderCode = qrCode.SenderCode;
                            Shipmentdto.ReceiverCode = qrCode.ReceiverCode;
                        }
                    }
                    else
                    {
                        throw new GenericException($"Shipment with waybill: {waybill} does not exist", $"{(int)HttpStatusCode.NotFound}");
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
                var mobileShipment = await _uow.PreShipmentMobile.FindAsync(x => x.CustomerCode == user.UserChannelCode, "PreShipmentItems,SenderLocation,ReceiverLocation");

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
                                                              InputtedSenderAddress = r.InputtedSenderAddress,
                                                              InputtedReceiverAddress = r.InputtedReceiverAddress,
                                                              SenderLocality = r.SenderLocality,
                                                              ZoneMapping = r.ZoneMapping,
                                                              CashOnDeliveryAmount = r.CashOnDeliveryAmount,
                                                              IsCashOnDelivery = r.IsCashOnDelivery,
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
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<TransactionPreShipmentDTO>> GetPreShipmentForUser(UserDTO user, ShipmentCollectionFilterCriteria filterCriteria)
        {
            try
            {
                var shipmentDto = new List<PreShipmentMobileDTO>();

                var mobileShipment = _uow.PreShipmentMobile.GetPreShipmentForUser(user.UserChannelCode);

                if (filterCriteria.StartDate == null && filterCriteria.EndDate == null)
                {
                    shipmentDto = mobileShipment.OrderByDescending(x => x.DateCreated).Take(20).ToList();
                }
                else
                {
                    //get startDate and endDate
                    var queryDate = filterCriteria.getStartDateAndEndDate();
                    var startDate = queryDate.Item1;
                    var endDate = queryDate.Item2;

                    shipmentDto = mobileShipment.Where(s => s.DateCreated >= startDate && s.DateCreated < endDate).ToList();
                }

                var agilityShipment = await GetShipments(user.UserChannelCode, filterCriteria);

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

                        shipment.IsRated = false;
                        var rating = await _uow.MobileRating.GetAsync(j => j.Waybill == shipment.Waybill);
                        if (rating != null)
                        {
                            shipment.IsRated = rating.IsRatedByCustomer;
                        }
                    }
                }

                var newlist = shipmentDto.Union(agilityShipment);
                var result = Mapper.Map<IEnumerable<TransactionPreShipmentDTO>>(newlist);

                if (filterCriteria.StartDate == null && filterCriteria.EndDate == null)
                {
                    return await Task.FromResult(result.OrderByDescending(x => x.DateCreated).Take(20).ToList());
                }
                return await Task.FromResult(result.OrderByDescending(x => x.DateCreated).ToList());
            }
            catch (Exception)
            {
                throw;
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
                throw;
                //throw new GenericException("Please an error occured while getting Special Packages");
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
                throw;
                //throw new GenericException("Please an error occured while getting categories.Please try again");
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
                throw;
                //throw new GenericException("Please an error occurred while getting sub-categories.Please try again");
            }
        }

        public async Task ScanMobileShipment(ScanDTO scan)
        {
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
                throw;
                //throw new GenericException("Please an error occurred while trying to scan shipment.");
            }
        }

        public async Task<PreShipmentMobile> GetMobileShipmentForScan(string waybill)
        {
            try
            {
                var shipment = await _uow.PreShipmentMobile.GetAsync(x => x.Waybill.Equals(waybill));
                if (shipment == null)
                {
                    throw new GenericException("Waybill does not exist", $"{(int)HttpStatusCode.NotFound}");
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
                throw;
            }
        }

        public async Task<PreShipmentMobileDTO> AddMobilePickupRequest(MobilePickUpRequestsDTO pickuprequest)
        {
            try
            {
                if (pickuprequest == null)
                {
                    throw new GenericException("NULL INPUT");
                }

                var newPreShipment = new PreShipmentMobileDTO();

                if (pickuprequest.UserId == null)
                {
                    pickuprequest.UserId = await _userService.GetCurrentUserId();
                }

                var preshipmentmobile = await _uow.PreShipmentMobile.GetAsync(s => s.Waybill == pickuprequest.Waybill, "PreShipmentItems,SenderLocation,ReceiverLocation,serviceCentreLocation");
                if (preshipmentmobile == null)
                {
                    throw new GenericException("Shipment does not exist", $"{(int)HttpStatusCode.NotFound}");
                }
                else
                {

                    if (preshipmentmobile.shipmentstatus == MobilePickUpRequestStatus.Cancelled.ToString())
                    {
                        throw new GenericException($"Error processing shipment. This Shipment has already been cancelled", $"{(int)HttpStatusCode.Forbidden}");
                    }

                    if (pickuprequest.Status == MobilePickUpRequestStatus.Rejected.ToString() || pickuprequest.Status == MobilePickUpRequestStatus.TimedOut.ToString()
                       || pickuprequest.Status == MobilePickUpRequestStatus.Missed.ToString())
                    {
                        var request = await _uow.MobilePickUpRequests.GetAsync(s => s.Waybill == pickuprequest.Waybill && s.UserId == pickuprequest.UserId);

                        if (request == null)
                        {
                            await _mobilepickuprequestservice.AddMobilePickUpRequests(pickuprequest);

                            //Update alpha
                            if (preshipmentmobile.IsAlpha)
                            {
                                await UpdateAlphaOrderStatus(preshipmentmobile.Waybill, AlphaOrderStatus.processing.ToString());
                            }
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
                        preshipmentmobile.TimeAssigned = DateTime.Now;
                        await _mobilepickuprequestservice.AddOrUpdateMobilePickUpRequests(pickuprequest);

                        //Update Activity Status
                        await UpdateActivityStatus(pickuprequest.UserId, ActivityStatus.OnDelivery);
                    }
                    else
                    {
                        throw new GenericException($"Shipment has already been accepted..", $"{(int)HttpStatusCode.Forbidden}");
                    }

                    if (pickuprequest.ServiceCentreId != null)
                    {
                        var DestinationServiceCentreId = await _uow.ServiceCentre.GetAsync(s => s.Code == pickuprequest.ServiceCentreId);

                        if (DestinationServiceCentreId != null)
                        {
                            preshipmentmobile.ServiceCentreAddress = DestinationServiceCentreId.Address;
                            var Locationdto = new LocationDTO
                            {
                                Latitude = DestinationServiceCentreId.Latitude,
                                Longitude = DestinationServiceCentreId.Longitude
                            };
                            var LOcation = Mapper.Map<Location>(Locationdto);
                            preshipmentmobile.serviceCentreLocation = LOcation;
                        }
                    }

                    if (pickuprequest.Status == MobilePickUpRequestStatus.Accepted.ToString())
                    {
                        preshipmentmobile.shipmentstatus = "Assigned for Pickup";

                        //Remove this block of code Later
                        var deliveryNumber = await _uow.DeliveryNumber.GetAsync(x => x.Waybill == pickuprequest.Waybill && x.IsDeleted == false);
                        if (deliveryNumber != null && string.IsNullOrWhiteSpace(deliveryNumber.SenderCode) && !string.IsNullOrWhiteSpace(deliveryNumber.Number))
                        {
                            deliveryNumber.SenderCode = deliveryNumber.Number;
                            deliveryNumber.Number = null;
                        }
                        //end of block

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
                    throw new GenericException("NULL INPUT");
                }

                if (string.IsNullOrEmpty(pickuprequest.GroupCodeNumber))
                {
                    throw new GenericException("Group Code can not be null");
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

                    var allpreshipmentmobile = await _uow.PreShipmentMobile.FindAsync(x => waybillList.Contains(x.Waybill), "PreShipmentItems,SenderLocation,ReceiverLocation,serviceCentreLocation");

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

                            if (DestinationServiceCentreId != null)
                            {
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
                        throw new GenericException($"Shipment Already Accepted.", $"{(int)HttpStatusCode.Forbidden}");
                    }

                    return newPreShipmentDTO;
                }
                else
                {
                    throw new GenericException("Group Code does not exist", $"{(int)HttpStatusCode.NotFound}");
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
                if (pickuprequest == null)
                {
                    throw new GenericException("Null INPUT");
                }

                var preshipment = await _uow.PreShipmentMobile.GetAsync(x => x.Waybill == pickuprequest.Waybill);
                if (preshipment == null)
                {
                    throw new GenericException($"Error processing shipment. This Shipment does not exist", $"{(int)HttpStatusCode.Forbidden}");
                }

                //Block Process for any cancelled shipment
                var shipmentCancelled = await _uow.PreShipmentMobile.GetAsync(x => x.Waybill == pickuprequest.Waybill && x.shipmentstatus == MobilePickUpRequestStatus.Cancelled.ToString());
                if (shipmentCancelled != null)
                {
                    throw new GenericException($"Error processing shipment. This Shipment has already been cancelled", $"{(int)HttpStatusCode.Forbidden}");
                }

                if (preshipment.shipmentstatus.ToLower() == "shipment created" && pickuprequest.Status.ToLower() == MobilePickUpRequestStatus.Rejected.ToString().ToLower())
                {
                    return true;
                }

                var userId = await _userService.GetCurrentUserId();
                pickuprequest.UserId = userId;

                await _mobilepickuprequestservice.UpdateMobilePickUpRequests(pickuprequest, userId);

                if (pickuprequest.Status == MobilePickUpRequestStatus.Confirmed.ToString())
                {
                    //await ConfirmMobilePickupRequest(pickuprequest, userId);

                    throw new GenericException($"Your App version is Old, Kindly update to the latest version.", $"{(int)HttpStatusCode.Forbidden}");
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
                    if (preshipmentmobile == null)
                    {
                        throw new GenericException($"Waybill {pickuprequest.Waybill} does not exist", $"{(int)HttpStatusCode.NotFound}");
                    }
                    preshipmentmobile.shipmentstatus = MobilePickUpRequestStatus.Dispute.ToString();
                    pickuprequest.Status = MobilePickUpRequestStatus.Dispute.ToString();
                    await _mobilepickuprequestservice.UpdateMobilePickUpRequests(pickuprequest, userId);
                    await _uow.CompleteAsync();
                }
                else if (pickuprequest.Status == MobilePickUpRequestStatus.Rejected.ToString())
                {
                    var preshipmentmobile = await _uow.PreShipmentMobile.GetAsync(s => s.Waybill == pickuprequest.Waybill);
                    if (preshipmentmobile == null)
                    {
                        throw new GenericException($"Waybill {pickuprequest.Waybill} does not exist", $"{(int)HttpStatusCode.NotFound}");
                    }
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
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> UpdateMobilePickupRequestUsingGroupCode(MobilePickUpRequestsDTO pickuprequest)
        {
            try
            {
                if (pickuprequest == null)
                {
                    throw new GenericException("NULL INPUT");
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
                        throw new GenericException("Group does not exist", $"{(int)HttpStatusCode.NotFound}");
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
                    throw new GenericException("NULL INPUT");
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
                // var preshipmentmobile = await _uow.PreShipmentMobile.GetAsync(s => s.Waybill == pickuprequest.Waybill, "PreShipmentItems,SenderLocation,ReceiverLocation");
                var preshipmentmobile = await _uow.PreShipmentMobile.GetAsync(s => s.Waybill == pickuprequest.Waybill);
                if (preshipmentmobile == null)
                {
                    throw new GenericException("Shipment item does not exist", $"{(int)HttpStatusCode.NotFound}");
                }

                //int destinationServiceCentreId = 0;
                //int departureServiceCentreId = 0;

                ////shipment witin a state 
                //if (preshipmentmobile.ZoneMapping == 1)
                //{
                //    var gigGOServiceCentre = await _userService.GetGIGGOServiceCentre();
                //    destinationServiceCentreId = gigGOServiceCentre.ServiceCentreId;
                //    departureServiceCentreId = gigGOServiceCentre.ServiceCentreId;
                //}
                //else
                //{
                //    //shipment outside a state -- Inter State Shipment
                //    var DepartureStation = await _uow.Station.GetAsync(s => s.StationId == preshipmentmobile.SenderStationId);
                //    departureServiceCentreId = DepartureStation.SuperServiceCentreId;

                //    var DestinationStation = await _uow.Station.GetAsync(s => s.StationId == preshipmentmobile.ReceiverStationId);
                //    destinationServiceCentreId = DestinationStation.SuperServiceCentreId;
                //}

                //var CustomerId = await _uow.IndividualCustomer.GetAsync(s => s.CustomerCode == preshipmentmobile.CustomerCode);

                //int customerid = 0;
                //if (CustomerId != null)
                //{
                //    customerid = CustomerId.IndividualCustomerId;
                //}
                //else
                //{
                //    var companyid = await _uow.Company.GetAsync(s => s.CustomerCode == preshipmentmobile.CustomerCode);
                //    customerid = companyid.CompanyId;
                //}


                //var companyid = await _uow.Company.GetAsync(s => s.CustomerCode == preshipmentmobile.CustomerCode);

                //int customerid = 0;
                //if (companyid != null)
                //{
                //    customerid = companyid.CompanyId;
                //}
                //else
                //{
                //    var CustomerId = await _uow.IndividualCustomer.GetAsync(s => s.CustomerCode == preshipmentmobile.CustomerCode);
                //    customerid = CustomerId.IndividualCustomerId;
                //}

                //if (preshipmentmobile.IsApproved != true && preshipmentmobile.ZoneMapping != 1)
                //{
                //    var MobileShipment = new ShipmentDTO
                //    {
                //        Waybill = preshipmentmobile.Waybill,
                //        ReceiverName = preshipmentmobile.ReceiverName,
                //        ReceiverPhoneNumber = preshipmentmobile.ReceiverPhoneNumber,
                //        ReceiverEmail = preshipmentmobile.ReceiverEmail,
                //        ReceiverAddress = preshipmentmobile.ReceiverAddress,
                //        DeliveryOptionId = 1,
                //        GrandTotal = preshipmentmobile.GrandTotal,
                //        Insurance = preshipmentmobile.InsuranceValue,
                //        Vat = preshipmentmobile.Vat,
                //        SenderAddress = preshipmentmobile.SenderAddress,
                //        IsCashOnDelivery = false,
                //        CustomerCode = preshipmentmobile.CustomerCode,
                //        DestinationServiceCentreId = destinationServiceCentreId,
                //        DepartureServiceCentreId = departureServiceCentreId,
                //        CustomerId = customerid,
                //        UserId = userId,
                //        PickupOptions = PickupOptions.HOMEDELIVERY,
                //        IsdeclaredVal = preshipmentmobile.IsdeclaredVal,
                //        ShipmentPackagePrice = preshipmentmobile.GrandTotal,
                //        ApproximateItemsWeight = 0.00,
                //        ReprintCounterStatus = false,
                //        CustomerType = preshipmentmobile.CustomerType,
                //        CompanyType = preshipmentmobile.CompanyType,
                //        Value = preshipmentmobile.Value,
                //        PaymentStatus = PaymentStatus.Paid,
                //        IsFromMobile = true,
                //        ShipmentItems = preshipmentmobile.PreShipmentItems.Select(s => new ShipmentItemDTO
                //        {
                //            Description = s.Description,
                //            IsVolumetric = s.IsVolumetric,
                //            Weight = s.Weight,
                //            Nature = s.ItemType,
                //            Price = (decimal)s.CalculatedPrice,
                //            Quantity = s.Quantity
                //        }).ToList()
                //    };
                //    var status = await _shipmentService.AddShipmentFromMobile(MobileShipment);
                //}

                if (preshipmentmobile.shipmentstatus == MobilePickUpRequestStatus.OnwardProcessing.ToString() || preshipmentmobile.shipmentstatus == MobilePickUpRequestStatus.Delivered.ToString())
                {
                    throw new GenericException($"This shipment {pickuprequest.Waybill} has not been marked as {preshipmentmobile.shipmentstatus}", $"{(int)HttpStatusCode.Forbidden}");
                }

                preshipmentmobile.shipmentstatus = MobilePickUpRequestStatus.PickedUp.ToString();
                preshipmentmobile.IsConfirmed = true;

                await _uow.CompleteAsync();

                await ScanMobileShipment(new ScanDTO
                {
                    WaybillNumber = pickuprequest.Waybill,
                    ShipmentScanStatus = ShipmentScanStatus.MSHC
                });
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
                    throw new GenericException("Shipment item does not exist", $"{(int)HttpStatusCode.NotFound}");
                }

                var country = await _uow.Country.GetCountryByStationId(preshipmentmobile.SenderStationId);
                if (country == null)
                {
                    throw new GenericException("Sender Station Country Not Found", $"{(int)HttpStatusCode.NotFound}");
                }

                if (preshipmentmobile.shipmentstatus == MobilePickUpRequestStatus.PickedUp.ToString() || preshipmentmobile.shipmentstatus == MobilePickUpRequestStatus.OnwardProcessing.ToString())
                {
                    preshipmentmobile.IsDelivered = true;
                    preshipmentmobile.TimeDelivered = DateTime.Now;
                    preshipmentmobile.IndentificationUrl = pickuprequest.IndentificationUrl;
                    preshipmentmobile.DeliveryAddressImageUrl = pickuprequest.DeliveryAddressImageUrl;

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
                            UserId = userId,
                            TransactionCountryId = country.CountryId
                        };
                        var walletTransaction = await _walletTransactionService.AddWalletTransaction(transaction);

                        var partnertransactions = new PartnerTransactionsDTO
                        {
                            Destination = preshipmentmobile.ReceiverAddress,
                            Departure = preshipmentmobile.SenderAddress,
                            AmountReceived = price,
                            Waybill = preshipmentmobile.Waybill
                        };

                        var id = await _partnertransactionservice.AddPartnerPaymentLog(partnertransactions);

                        await _uow.CompleteAsync();

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

                        if (preshipmentmobile.IsFromAgility)
                        {
                            var trackingDTO = new ShipmentTrackingDTO
                            {
                                Waybill = preshipmentmobile.Waybill,
                                User = userId,
                                ServiceCentreId = defaultServiceCenter.ServiceCentreId,
                                Location = defaultServiceCenter.Name,
                            };
                            await UpdateShipmentToDelivered(trackingDTO);
                        }

                        //Add to Financial Reports
                        var financialReport = new FinancialReportDTO
                        {
                            Source = ReportSource.GIGGo,
                            Waybill = preshipmentmobile.Waybill,
                            PartnerEarnings = price,
                            GrandTotal = shipmentPrice,
                            Earnings = shipmentPrice - price,
                            Demurrage = 0.00M,
                            CountryId = preshipmentmobile.CountryId
                        };
                        await _financialReportService.AddReport(financialReport);

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
                                UserId = userId,
                                TransactionCountryId = country.CountryId
                            };
                            var walletTransaction = await _walletTransactionService.AddWalletTransaction(transaction);

                            //Add to Financial Reports
                            var financialReport = new FinancialReportDTO
                            {
                                Source = ReportSource.GIGGo,
                                Waybill = preshipmentmobile.Waybill,
                                PartnerEarnings = price,
                                GrandTotal = preshipmentmobile.GrandTotal,
                                Earnings = preshipmentmobile.GrandTotal - price,
                                Demurrage = 0.00M,
                                CountryId = preshipmentmobile.CountryId
                            };
                            await _financialReportService.AddReport(financialReport);
                            return;
                        }
                        else
                        {
                            pickuprequest.Status = MobilePickUpRequestStatus.Confirmed.ToString();
                            await _mobilepickuprequestservice.UpdateMobilePickUpRequests(pickuprequest, userId);
                            throw new GenericException("This is an interstate delivery, drop at assigned service centre!!", $"{(int)HttpStatusCode.Forbidden}");
                        }
                    }
                }
                else
                {
                    pickuprequest.Status = MobilePickUpRequestStatus.Confirmed.ToString();
                    await _mobilepickuprequestservice.UpdateMobilePickUpRequests(pickuprequest, userId);
                    throw new GenericException("Shipment has not been processed", $"{(int)HttpStatusCode.Forbidden}");
                }
                //update the actual receiver if neccessary
                if (pickuprequest.IsProxy)
                {
                    preshipmentmobile.ActualReceiverFirstName = pickuprequest.ProxyName;
                    preshipmentmobile.ActualReceiverPhoneNumber = pickuprequest.ProxyPhoneNumber;
                    preshipmentmobile.ActualReceiverLastName = pickuprequest.ProxyEmail;
                    await _uow.CompleteAsync();
                }
                if (preshipmentmobile.IsCashOnDelivery)
                {
                    var codtransferlog = await _uow.CODTransferRegister.GetAsync(x => x.Waybill == pickuprequest.Waybill && x.PaymentStatus == PaymentStatus.Paid);
                    if (codtransferlog is null)
                    {
                        preshipmentmobile.CODDescription = $"COD {CODMobileStatus.Collected.ToString()}({pickuprequest.PaymentType.ToString()})";
                        preshipmentmobile.CODStatus = CODMobileStatus.Collected;
                        preshipmentmobile.CODStatusDate = DateTime.Now;
                        await _uow.CompleteAsync(); 
                    }
                }
                if (preshipmentmobile.IsAlpha)
                {
                    await UpdateAlphaOrderStatus(preshipmentmobile.Waybill, AlphaOrderStatus.delivered.ToString());
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

                var preshipmentMobile = await _uow.PreShipmentMobile.GetAsync(s => s.Waybill == pickuprequest.Waybill);

                if (preshipmentMobile == null)
                {
                    throw new GenericException("Shipment item does not exist");
                }

                //Get the rider phone number
                var dispatchRider = await _uow.User.GetUserById(userId);

                var messageExtensionDTO = new MobileMessageDTO()
                {
                    SenderName = preshipmentMobile.SenderName,
                    WaybillNumber = preshipmentMobile.Waybill,
                    SenderPhoneNumber = preshipmentMobile.SenderPhoneNumber,
                    DispatchRiderPhoneNumber = dispatchRider.PhoneNumber
                };

                if (preshipmentMobile.shipmentstatus == "Assigned for Pickup")
                {
                    //Send message to Sender
                    await _messageSenderService.SendMessage(MessageType.APFS, EmailSmsType.SMS, messageExtensionDTO);
                }
                else if (preshipmentMobile.shipmentstatus == MobilePickUpRequestStatus.PickedUp.ToString())
                {
                    //Send message to Receiver
                    messageExtensionDTO.SenderName = preshipmentMobile.ReceiverName;
                    messageExtensionDTO.SenderPhoneNumber = preshipmentMobile.ReceiverPhoneNumber;
                    await _messageSenderService.SendMessage(MessageType.MATD, EmailSmsType.SMS, messageExtensionDTO);
                }

                //Update Status
                preshipmentMobile.shipmentstatus = MobilePickUpRequestStatus.Visited.ToString();
                mobileRequest.Status = MobilePickUpRequestStatus.Visited.ToString();

                await _uow.CompleteAsync();

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
                throw;
                //throw new GenericException("Please an error occurred while trying to get all special packages.");
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
                throw;
                //throw new GenericException("Please an error occurred while trying to get the categorization of special packages.");
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
                throw;
                //throw new GenericException("Please an error occurred while trying to get the categorization of special packages.");
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
                var shipments = await _uow.PreShipmentMobile.FindAsync(s => s.UserId == user && s.shipmentstatus == MobilePickUpRequestStatus.Dispute.ToString(), "PreShipmentItems,SenderLocation,ReceiverLocation");
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
                throw;
                //throw new GenericException("Please an error occurred while trying to get shipments in dispute.");
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
                throw;
                //throw new GenericException("Please an error occurred while trying to get partner's transactions.");
            }
        }

        public async Task<object> ResolveDisputeForMobileOld(PreShipmentMobileDTO preShipment)
        {
            try
            {
                var preshipmentmobilegrandtotal = await _uow.PreShipmentMobile.GetAsync(s => s.PreShipmentMobileId == preShipment.PreShipmentMobileId);

                if (preshipmentmobilegrandtotal.shipmentstatus == MobilePickUpRequestStatus.PickedUp.ToString() || preshipmentmobilegrandtotal.shipmentstatus == MobilePickUpRequestStatus.Delivered.ToString())
                {
                    throw new GenericException("This Shipment cannot be placed in Dispute, because it has been" + " " + preshipmentmobilegrandtotal.shipmentstatus, $"{(int)HttpStatusCode.Forbidden}");
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
                        throw new GenericException("Insufficient Wallet Balance", $"{(int)HttpStatusCode.Forbidden}");
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

        public async Task<object> ResolveDisputeForMobile(PreShipmentMobileDTO preShipment)
        {
            try
            {
                if (preShipment == null)
                {
                    throw new GenericException("NULL INPUT");
                }

                var preshipmentmobilegrandtotal = await _uow.PreShipmentMobile.GetAsync(s => s.PreShipmentMobileId == preShipment.PreShipmentMobileId, "SenderLocation,ReceiverLocation");

                if (preshipmentmobilegrandtotal.shipmentstatus == MobilePickUpRequestStatus.PickedUp.ToString() || preshipmentmobilegrandtotal.shipmentstatus == MobilePickUpRequestStatus.Delivered.ToString())
                {
                    throw new GenericException("This Shipment cannot be placed in Dispute, because it has been" + " " + preshipmentmobilegrandtotal.shipmentstatus, $"{(int)HttpStatusCode.Forbidden}");
                }

                //delete remove item from DB
                var preShipmentMobileItemToBeDeleted = _uow.PreShipmentItemMobile.GetAllAsQueryable()
                    .Where(x => preShipment.DeletedItems.Contains(x.PreShipmentItemMobileId) && x.PreShipmentMobileId == preShipment.PreShipmentMobileId).ToList();

                if (preShipmentMobileItemToBeDeleted.Any())
                {
                    preShipmentMobileItemToBeDeleted.ForEach(d => d.IsCancelled = true);
                    _uow.PreShipmentItemMobile.RemoveRange(preShipmentMobileItemToBeDeleted);
                }

                //Get the Geo Location Detail
                var senderLocation = Mapper.Map<LocationDTO>(preshipmentmobilegrandtotal.SenderLocation);
                var receiverLocation = Mapper.Map<LocationDTO>(preshipmentmobilegrandtotal.ReceiverLocation);
                preShipment.SenderLocation = senderLocation;
                preShipment.ReceiverLocation = receiverLocation;

                var preshipmentPriceDTO = new MobilePriceDTO();

                if (preshipmentmobilegrandtotal.VehicleType.ToLower() == Vehicletype.Bike.ToString().ToLower() && preshipmentmobilegrandtotal.ZoneMapping == 1)
                {
                    preshipmentPriceDTO = await GetPriceForBike(preShipment);
                }
                else if (preshipmentmobilegrandtotal.VehicleType.ToLower() == Vehicletype.Truck.ToString().ToLower())
                {
                    preshipmentPriceDTO = await GetHaulagePrice(new HaulagePriceDTO
                    {
                        Haulageid = (int)preShipment.Haulageid,
                        DepartureStationId = preShipment.SenderStationId,
                        DestinationStationId = preShipment.ReceiverStationId
                    });
                    preshipmentPriceDTO.GrandTotal = (decimal)preshipmentPriceDTO.GrandTotal;
                }
                else
                {
                    preshipmentPriceDTO = await GetPrice(preShipment);
                }

                var difference = (preshipmentmobilegrandtotal.GrandTotal - preshipmentPriceDTO.GrandTotal);

                if (preShipment.PreShipmentItems.Any())
                {
                    int[] PreShipmentItemMobileIds = preShipment.PreShipmentItems.Select(i => i.PreShipmentItemMobileId).ToArray();

                    //fetch all items from DB at once & update them in the memory
                    var preShipmentMobileItemToBeUpdated = _uow.PreShipmentItemMobile.GetAllAsQueryable()
                        .Where(x => PreShipmentItemMobileIds.Contains(x.PreShipmentItemMobileId) && x.PreShipmentMobileId == preShipment.PreShipmentMobileId).ToList();

                    if (preShipmentMobileItemToBeUpdated.Any())
                    {
                        foreach (var item in preShipmentMobileItemToBeUpdated)
                        {
                            var preshipmentitemmobile = preShipment.PreShipmentItems.Where(x => x.PreShipmentItemMobileId == item.PreShipmentItemMobileId).FirstOrDefault();
                            if (preshipmentitemmobile != null)
                            {
                                item.Quantity = preshipmentitemmobile.Quantity;
                                item.Value = preshipmentitemmobile.Value;
                                item.Weight = (double)preshipmentitemmobile.Weight;
                                item.Description = preshipmentitemmobile.Description;
                                item.Height = preshipmentitemmobile.Height;
                                item.ImageUrl = preshipmentitemmobile.ImageUrl;
                                item.ItemName = preshipmentitemmobile.ItemName;
                                item.Length = preshipmentitemmobile.Length;
                                item.SpecialPackageId = preshipmentitemmobile.SpecialPackageId;
                                item.WeightRange = preshipmentitemmobile.WeightRange;
                                item.CalculatedPrice = preshipmentPriceDTO.PreshipmentMobile.PreShipmentItems.Where(x => x.PreShipmentItemMobileId == item.PreShipmentItemMobileId).Select(y => y.CalculatedPrice).FirstOrDefault();
                                if (!string.IsNullOrEmpty(preshipmentitemmobile.Value))
                                {
                                    preshipmentmobilegrandtotal.Value = preshipmentmobilegrandtotal.Value + decimal.Parse(preshipmentitemmobile.Value);
                                }
                            }
                        }
                    }
                }

                //update shipment information
                preshipmentmobilegrandtotal.shipmentstatus = MobilePickUpRequestStatus.Resolved.ToString();
                preshipmentmobilegrandtotal.GrandTotal = (decimal)preshipmentPriceDTO.GrandTotal;
                preshipmentmobilegrandtotal.InsuranceValue = preshipmentPriceDTO.PreshipmentMobile.InsuranceValue;
                preshipmentmobilegrandtotal.DiscountValue = preshipmentPriceDTO.PreshipmentMobile.DiscountValue;
                preshipmentmobilegrandtotal.DeliveryPrice = preshipmentPriceDTO.PreshipmentMobile.DeliveryPrice;
                preshipmentmobilegrandtotal.CalculatedTotal = preshipmentPriceDTO.PreshipmentMobile.CalculatedTotal;

                var updatedwallet = await _uow.Wallet.GetAsync(s => s.CustomerCode == preShipment.CustomerCode);
                if (difference < 0.00M)
                {
                    var newdiff = Math.Abs((decimal)difference);
                    if (newdiff > updatedwallet.Balance)
                    {
                        throw new GenericException("Insufficient Wallet Balance", $"{(int)HttpStatusCode.Forbidden}");
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
                    throw new GenericException("This Shipment cannot be placed in Dispute, because it has been" + " " + preshipmentmobilegrandtotal.shipmentstatus, $"{(int)HttpStatusCode.Forbidden}");
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
                    preshipmentitemmobile.SpecialPackageId = item.SpecialPackageId;
                    preshipmentitemmobile.WeightRange = item.WeightRange;
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
                        throw new GenericException("Insufficient Wallet Balance", $"{(int)HttpStatusCode.Forbidden}");
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
                var report = await _uow.FinancialReport.GetAsync(s => s.Waybill == preshipmentmobile.Waybill);
                if (preshipmentmobile == null)
                {
                    throw new GenericException("Shipment does not exist", $"{(int)HttpStatusCode.NotFound}");
                }

                var pickuprequests = await _uow.MobilePickUpRequests.GetAsync(s => s.Waybill == preshipmentmobile.Waybill && (s.Status != MobilePickUpRequestStatus.Rejected.ToString() || s.Status != MobilePickUpRequestStatus.TimedOut.ToString()));
                if (pickuprequests != null)
                {
                    var pickuprice = await GetPickUpPrice(preshipmentmobile.VehicleType, preshipmentmobile.CountryId, preshipmentmobile.UserId = null);
                    var partnersPrice = (0.4M * Convert.ToDecimal(pickuprice));

                    var rider = await _userService.GetUserById(pickuprequests.UserId);
                    var riderWallet = await _uow.Wallet.GetAsync(s => s.CustomerCode == rider.UserChannelCode);
                    riderWallet.Balance = riderWallet.Balance + partnersPrice;

                    preshipmentmobile.shipmentstatus = MobilePickUpRequestStatus.Cancelled.ToString();
                    pickuprequests.Status = MobilePickUpRequestStatus.Cancelled.ToString();

                    //update wallet transaction for customer 
                    decimal amount = preshipmentmobile.GrandTotal - Convert.ToDecimal(pickuprice);
                    await UpdateCustomerWalletForCancelledShipment(preshipmentmobile.CustomerCode, preshipmentmobile.Waybill, amount);

                    var partnertransactions = new PartnerTransactionsDTO
                    {
                        Destination = preshipmentmobile.ReceiverAddress,
                        Departure = preshipmentmobile.SenderAddress,
                        AmountReceived = partnersPrice,
                        Waybill = preshipmentmobile.Waybill
                    };
                    await _partnertransactionservice.AddPartnerPaymentLog(partnertransactions);

                    //update financial report 
                    if (report != null)
                    {
                        report.PartnerEarnings = partnersPrice;
                        report.Earnings = 0.00M;
                        report.GrandTotal = 0.00M;
                    }
                }
                else
                {
                    preshipmentmobile.shipmentstatus = MobilePickUpRequestStatus.Cancelled.ToString();
                    await UpdateCustomerWalletForCancelledShipment(preshipmentmobile.CustomerCode, preshipmentmobile.Waybill, preshipmentmobile.GrandTotal);

                    //remove from report
                    if (report != null)
                    {
                        report.IsDeleted = true;
                    }
                }
                //also cancel shipment if its on shipment table already
                var shipment = await _uow.Shipment.GetAsync(s => s.Waybill == Waybill);
                if (shipment != null)
                {
                    var newCancel = new CancelShipmentDTO
                    {
                        Waybill = Waybill,
                        CancelReason = "None"
                    };
                    var user = await _userService.GetCurrentUserId();
                    var res = await _shipmentService.CancelShipmentForGIGGOExtension(newCancel);
                }
                await _uow.CompleteAsync();
                return new { IsCancelled = true };
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task UpdateCustomerWalletForCancelledShipment(string customerCode, string waybill, decimal amount)
        {
            var user = await _userService.GetCurrentUserId();
            var defaultServiceCenter = await _userService.GetGIGGOServiceCentre();
            var updatedwallet = await _uow.Wallet.GetAsync(s => s.CustomerCode == customerCode);

            if (updatedwallet == null)
            {
                throw new GenericException("Wallet does not exist", $"{(int)HttpStatusCode.NotFound}");
            }

            var preshipmentmobile = await _uow.PreShipmentMobile.GetAsync(x => x.Waybill == waybill);
            var country = await _uow.Country.GetCountryByStationId(preshipmentmobile.SenderStationId);
            if (country == null)
            {
                throw new GenericException("Sender Station Country Not Found", $"{(int)HttpStatusCode.NotFound}");
            }

            //Add wallet transaction
            var transaction = new WalletTransactionDTO
            {
                WalletId = updatedwallet.WalletId,
                CreditDebitType = CreditDebitType.Credit,
                Amount = amount,
                ServiceCentreId = defaultServiceCenter.ServiceCentreId,
                Waybill = waybill,
                Description = "Cancelled Shipment",
                PaymentType = PaymentType.Online,
                UserId = user,
                TransactionCountryId = country.CountryId
            };
            //get the balance after transaction
            if (transaction.CreditDebitType == CreditDebitType.Credit)
            {
                transaction.BalanceAfterTransaction = updatedwallet.Balance + transaction.Amount;
            }
            else
            {
                transaction.BalanceAfterTransaction = updatedwallet.Balance - transaction.Amount;
            }
            var walletTransaction = await _walletTransactionService.AddWalletTransaction(transaction);

            //Compute wallete transactions for the customer and update the wallet
            var walletTransactionHistory = await _uow.WalletTransaction.FindAsync(s => s.WalletId == updatedwallet.WalletId);
            decimal balance = 0;
            foreach (var item in walletTransactionHistory)
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

            updatedwallet.Balance = balance;
            await _uow.CompleteAsync();
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
                            throw new GenericException("Customer has rated this Partner already!", $"{(int)HttpStatusCode.Forbidden}");
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
                            throw new GenericException("Partner has rated this Customer already!", $"{(int)HttpStatusCode.Forbidden}");
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
            catch (Exception)
            {
                throw;
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
            catch (Exception)
            {
                throw;
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
                    throw new GenericException("Delivery Number does not exist", $"{(int)HttpStatusCode.NotFound}");
                }
                else
                {
                    if (number.IsUsed)
                    {
                        throw new GenericException("Delivery Number has been used", $"{(int)HttpStatusCode.Forbidden}");
                    }
                    else
                    {
                        var mobileShipment = await _uow.PreShipmentMobile.GetAsync(s => s.Waybill == detail.WayBill);

                        if (mobileShipment == null)
                        {
                            throw new GenericException("Waybill does not exist in Shipments", $"{(int)HttpStatusCode.NotFound}");
                        }

                        var shipment = await _uow.Shipment.GetAsync(s => s.Waybill == detail.WayBill);

                        if (shipment != null)
                        {
                            shipment.DeliveryNumber = detail.DeliveryNumber;
                        }

                        number.IsUsed = true;
                        number.UserId = userId;
                        mobileShipment.DeliveryNumber = detail.DeliveryNumber;
                        await _uow.CompleteAsync();
                    }
                }
                return true;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> UpdateDeliveryNumberV2(MobileShipmentNumberDTO detail)
        {
            try
            {
                //var userId = await _userService.GetCurrentUserId();
                var number = await _uow.DeliveryNumber.GetAsync(s => s.Number.ToLower() == detail.DeliveryNumber.ToLower());
                if (number == null)
                {
                    throw new GenericException("Delivery Number does not exist", $"{(int)HttpStatusCode.NotFound}");
                }
                else
                {
                    if (number.IsUsed)
                    {
                        throw new GenericException("Delivery Number has been used", $"{(int)HttpStatusCode.Forbidden}");
                    }
                    else
                    {
                        var mobileShipment = await _uow.PreShipmentMobile.GetAsync(s => s.Waybill == detail.WayBill);

                        if (mobileShipment == null)
                        {
                            throw new GenericException("Waybill does not exist in Shipments", $"{(int)HttpStatusCode.NotFound}");
                        }

                        var deliveryNumber = await _uow.DeliveryNumber.GetAsync(s => s.Waybill == detail.WayBill);

                        if (deliveryNumber == null)
                        {
                            throw new GenericException("No record in Delivery Number for this Waybill", $"{(int)HttpStatusCode.NotFound}");
                        }
                        //else
                        //{
                        //    if (deliveryNumber.UserId != userId)
                        //    {
                        //        throw new GenericException("This Waybill was not verified by you", $"{(int)HttpStatusCode.Forbidden}");
                        //    }
                        //}

                        var shipment = await _uow.Shipment.GetAsync(s => s.Waybill == detail.WayBill);

                        if (shipment != null)
                        {
                            shipment.DeliveryNumber = detail.DeliveryNumber;
                        }

                        deliveryNumber.Number = detail.DeliveryNumber;
                        deliveryNumber.IsUsed = true;
                        mobileShipment.DeliveryNumber = detail.DeliveryNumber;
                        _uow.DeliveryNumber.Remove(number);
                        await _uow.CompleteAsync();
                    }
                }
                return true;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> VerifyDeliveryCode(MobileShipmentNumberDTO detail)
        {
            try
            {
                var userId = await _userService.GetCurrentUserId();
                var deliveryNumber = await _uow.DeliveryNumber.GetAsync(s => s.Waybill == detail.WayBill && s.IsDeleted == false);

                if (deliveryNumber == null)
                {
                    throw new GenericException("No record in Delivery Number for this Waybill", $"{(int)HttpStatusCode.NotFound}");
                }

                var mobileShipment = await _uow.PreShipmentMobile.GetAsync(s => s.Waybill == detail.WayBill);

                if (mobileShipment == null)
                {
                    throw new GenericException("Waybill does not exist in Shipments", $"{(int)HttpStatusCode.NotFound}");
                }

                if (mobileShipment.shipmentstatus == "Assigned for Pickup" || mobileShipment.shipmentstatus == MobilePickUpRequestStatus.Resolved.ToString())
                {
                    if (deliveryNumber.SenderCode.ToLower() != detail.DeliveryNumber.ToLower())
                    {
                        throw new GenericException($"This Sender Delivery Code {detail.DeliveryNumber} is not attached to this waybill {detail.WayBill} ", $"{(int)HttpStatusCode.NotFound}");
                    }
                    deliveryNumber.UserId = userId;

                    //Generate Receiver Delivery Code
                    deliveryNumber.ReceiverCode = await GenerateDeliveryCode();

                    //Update Shipment Status
                    var request = new MobilePickUpRequestsDTO()
                    {
                        Status = MobilePickUpRequestStatus.Confirmed.ToString(),
                        Waybill = mobileShipment.Waybill
                    };
                    //await UpdateMobilePickupRequest(request);
                    await _mobilepickuprequestservice.UpdateMobilePickUpRequests(request, userId);
                    await ConfirmMobilePickupRequest(request, userId);

                    //Send SMS To Receiver with Delivery Code
                    await SendReceiverDeliveryCodeBySMS(mobileShipment, deliveryNumber.ReceiverCode);

                    //Update alpha
                    if (mobileShipment.IsAlpha)
                    {
                        await UpdateAlphaOrderStatus(mobileShipment.Waybill, AlphaOrderStatus.shipped.ToString());
                    }
                }
                else if (mobileShipment.shipmentstatus == MobilePickUpRequestStatus.PickedUp.ToString() && mobileShipment.ZoneMapping == 1)
                {
                    if (deliveryNumber.ReceiverCode.ToLower() != detail.DeliveryNumber.ToLower())
                    {
                        throw new GenericException($"This Receiver Delivery Code {detail.DeliveryNumber} is not attached to this waybill {detail.WayBill} ", $"{(int)HttpStatusCode.NotFound}");
                    }

                    //Update Shipment Status
                    var request = new MobilePickUpRequestsDTO()
                    {
                        Status = MobilePickUpRequestStatus.Delivered.ToString(),
                        Waybill = mobileShipment.Waybill

                    };
                    //await UpdateMobilePickupRequest(request);

                    await _mobilepickuprequestservice.UpdateMobilePickUpRequests(request, userId);
                    await DeliveredMobilePickupRequest(request, userId);
                    await UpdateActivityStatus(userId, ActivityStatus.OffDelivery);
                }
                await _uow.CompleteAsync();
                return true;
            }
            catch
            {
                throw;
            }
        }

        public async Task<string> GenerateDeliveryCode()
        {
            try
            {
                int maxSize = 6;
                char[] chars = new char[54];
                string a;
                a = "abcdefghjkmnpqrstuvwxyzABCDEFGHJKMNPQRSTUVWXYZ23456789";
                chars = a.ToCharArray();
                int size = maxSize;
                byte[] data = new byte[1];
                RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();
                crypto.GetNonZeroBytes(data);
                size = maxSize;
                data = new byte[size];
                crypto.GetNonZeroBytes(data);
                StringBuilder result = new StringBuilder(size);
                foreach (byte b in data)
                { result.Append(chars[b % (chars.Length - 1)]); }
                var strippedText = result.ToString();
                var number = "DN" + strippedText.ToUpper();
                return number;
            }
            catch
            {
                throw;
            }
        }

        public async Task<bool> SendReceiverDeliveryCodeBySMS(PreShipmentMobile preShipmentMobile, string number)
        {
            try
            {
                if (preShipmentMobile != null && preShipmentMobile.ZoneMapping == 1)
                {
                    var message = new MobileShipmentCreationMessageDTO
                    {
                        SenderPhoneNumber = preShipmentMobile.ReceiverPhoneNumber,
                        SenderName = preShipmentMobile.ReceiverName,
                        WaybillNumber = preShipmentMobile.Waybill,
                        QRCode = number
                    };

                    await SendSMSForMobileShipmentCreation(message, MessageType.RMCS);
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
                        if (transactions.Any())
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
                    if (vehicles.Any())
                    {
                        foreach (var vehicle in vehicles)
                        {
                            _uow.VehicleType.Remove(vehicle);
                        }
                    }
                    _uow.Partner.Remove(partner);
                }
                await _uow.CompleteAsync();
                return true;
            }
            catch
            {
                throw;
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
                    if (!partnerDto.VehicleTypeDetails.Any())
                    {
                        throw new GenericException("No Vehicle attached to the Partner. Kindly review!!!", $"{(int)HttpStatusCode.NotFound}");
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
                    throw new GenericException("Partner Information Not Found!", $"{(int)HttpStatusCode.NotFound}");
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
                var partner = await _uow.Partner.GetAsync(s => s.Email == Email);

                if (partner == null)
                {
                    throw new GenericException("Partner Information Not Found!", $"{(int)HttpStatusCode.NotFound}");
                }

                partnerdto = Mapper.Map<PartnerDTO>(partner);

                var VehicleDetails = await _uow.VehicleType.FindAsync(s => s.Partnercode == partnerdto.PartnerCode);
                if (VehicleDetails != null)
                {
                    var vehicles = Mapper.Map<List<VehicleTypeDTO>>(VehicleDetails);
                    partnerdto.VehicleTypeDetails = vehicles;
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
                    throw new GenericException("Shipment Information does not exist!", $"{(int)HttpStatusCode.NotFound}");
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
                if (!string.IsNullOrWhiteSpace(vehicleType))
                {
                    PickUpPrice = await GetPickUpPriceForIndividual(vehicleType, CountryId);

                    //if (string.IsNullOrWhiteSpace(UserId))
                    //{
                    //    PickUpPrice = await GetPickUpPriceForIndividual(vehicleType, CountryId);
                    //}
                    //else
                    //{
                    //    PickUpPrice = await GetPickUpPriceForIndividual(vehicleType, CountryId);
                    //    var user = await _userService.GetUserById(UserId);
                    //    var exists = await _uow.Company.ExistAsync(s => s.CustomerCode == user.UserChannelCode);
                    //    if (user.UserChannelType == UserChannelType.Ecommerce || exists)
                    //    {
                    //        PickUpPrice = await GetPickUpPriceForEcommerce(vehicleType, CountryId);
                    //    }
                    //    else
                    //    {
                    //        PickUpPrice = await GetPickUpPriceForIndividual(vehicleType, CountryId);
                    //    }
                    //}
                }
                return PickUpPrice;
            }
            catch (Exception)
            {
                throw;
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
            catch (Exception)
            {
                throw;
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
            catch (Exception)
            {
                throw;
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
                        throw new GenericException("Shipment cannot be found in PreshipmentMobile", $"{(int)HttpStatusCode.NotFound}");
                    }
                    var PreShipmentdto = Mapper.Map<PreShipmentMobileDTO>(details);
                    ShipmentSummaryDetails.shipmentdetails = PreShipmentdto;
                    return ShipmentSummaryDetails;
                }
                else if (result == null)
                {
                    var details = await _uow.PreShipmentMobile.GetAsync(s => s.Waybill == DeliveryNumber || s.DeliveryNumber == DeliveryNumber, "PreShipmentItems");
                    if (details == null)
                    {
                        throw new GenericException("Shipment Detail not found", $"{(int)HttpStatusCode.NotFound}");
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
                    throw new GenericException("Shipment cannot be found", $"{(int)HttpStatusCode.NotFound}");
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
                    throw new GenericException($"This shipment {detail.WaybillNumber} does not exist, Kindly confirm the waybill again!!!", $"{(int)HttpStatusCode.NotFound}");
                }
                else
                {
                    if (preshipmentmobile.shipmentstatus == MobilePickUpRequestStatus.Cancelled.ToString())
                    {
                        throw new GenericException($"This shipment {detail.WaybillNumber} has been Cancelled. It can not be processed!!!", $"{(int)HttpStatusCode.Forbidden}");
                    }
                    else
                    {
                        if (preshipmentmobile.IsApproved == true)
                        {
                            throw new GenericException("Shipment has already been approved!!!", $"{(int)HttpStatusCode.Forbidden}");
                        }
                        if (preshipmentmobile.IsApproved != true && preshipmentmobile.IsInternationalShipment == true)
                        {
                            var intlShipmentPrice = new Core.DTO.DHL.TotalNetResult();
                            var shipment = new InternationalShipmentDTO();
                            var intlShipment = new Core.DTO.DHL.InternationalShipmentWaybillDTO();
                            var customer = new CustomerDTO();
                            var company = await _uow.Company.GetAsync(s => s.CustomerCode == preshipmentmobile.CustomerCode);
                            if (company != null)
                            {
                                customer = Mapper.Map<CustomerDTO>(company);
                            }
                            else
                            {
                                var individual = await _uow.IndividualCustomer.GetAsync(s => s.CustomerCode == preshipmentmobile.CustomerCode);
                                if (individual != null)
                                {
                                    customer = Mapper.Map<CustomerDTO>(individual);
                                }
                            }

                            var shipmentItems = Mapper.Map<List<ShipmentItemDTO>>(preshipmentmobile.PreShipmentItems);
                            shipment = Mapper.Map<InternationalShipmentDTO>(preshipmentmobile);
                            for (int i = 0; i < preshipmentmobile.PreShipmentItems.Count(); i++)
                            {
                                shipmentItems[i].Price = Convert.ToInt32(preshipmentmobile.PreShipmentItems[i].Value);
                                // Quick fixes
                                if (shipmentItems[i].Weight == 0 || shipmentItems[i].Length == 0 || shipmentItems[i].Height == 0 || shipmentItems[i].Width == 0)
                                {
                                    shipmentItems[i].InternationalShipmentItemCategory = InternationalShipmentItemCategory.Document;
                                }
                                else
                                {
                                    shipmentItems[i].InternationalShipmentItemCategory = InternationalShipmentItemCategory.NonDocument;
                                }
                            }
                            shipment.ShipmentItems = shipmentItems;
                            shipment.DeclarationOfValueCheck = shipment.ShipmentItems.Sum(x => x.Price);
                            shipment.ItemDetails = shipment.ShipmentItems[0].Description;
                            shipment.CustomerDetails = customer;
                            shipment.IsFromMobile = true;
                            var response = await _shipmentService.GetInternationalShipmentPrice(shipment);
                            if (shipment.CompanyMap == CompanyMap.DHL)
                            {
                                intlShipmentPrice = response.FirstOrDefault(x => x.CompanyMap == CompanyMap.DHL);
                                //if (intlShipmentPrice.GrandTotal != preshipmentmobile.GrandTotal)
                                //{
                                //    throw new GenericException($"There was an issue processing your request, shipment pricing is not accurate");
                                //}
                                intlShipment = await _dhlService.CreateInternationalShipment(shipment);
                            }
                            else if (shipment.CompanyMap == CompanyMap.UPS)
                            {
                                intlShipmentPrice = response.FirstOrDefault(x => x.CompanyMap == CompanyMap.UPS);
                                //if (intlShipmentPrice.GrandTotal != preshipmentmobile.GrandTotal)
                                //{
                                //    throw new GenericException($"There was an issue processing your request, shipment pricing is not accurate");
                                //}
                                intlShipment = await _uPSService.CreateInternationalShipment(shipment);
                            }

                            // mapping international shipment payload
                            var MobileShipment = await MapIntlShipmentToShipment(detail, preshipmentmobile, customer, intlShipmentPrice,
                                intlShipment.ShipmentIdentificationNumber, intlShipment.PdfFormat, shipment.DeclarationOfValueCheck.Value);

                            var factor = Convert.ToDecimal(Math.Pow(10, -2));
                            MobileShipment.GrandTotal = Math.Round(MobileShipment.GrandTotal * factor) / factor;
                            var status = await _shipmentService.AddShipmentFromMobile(MobileShipment);

                            var result = Mapper.Map<Core.Domain.DHL.InternationalShipmentWaybill>(intlShipment);
                            result.IsFromMobile = true;
                            _uow.InternationalShipmentWaybill.Add(result);

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
                            if (preshipmentmobile.IsCashOnDelivery == true)
                            {
                                //collect the cods and add to CashOnDeliveryRegisterAccount()
                                var cashondeliveryentity = new CashOnDeliveryRegisterAccount
                                {
                                    Amount = preshipmentmobile.CashOnDeliveryAmount ?? 0,
                                    CODStatusHistory = CODStatushistory.Created,
                                    Description = "Cod From Sales",
                                    ServiceCenterId = 0,
                                    Waybill = preshipmentmobile.Waybill,
                                    UserId = preshipmentmobile.UserId,
                                    DepartureServiceCenterId = detail.SenderServiceCentreId,
                                    DestinationCountryId = preshipmentmobile.DestinationCountryId
                                };

                                _uow.CashOnDeliveryRegisterAccount.Add(cashondeliveryentity);
                            }

                            await _uow.CompleteAsync();
                            if (detail.IsBulky)
                            {
                                await _autoManifestAndGroupingService.MappingWaybillNumberToGroupForBulk(detail.WaybillNumber);
                            }
                            else
                            {
                                await _autoManifestAndGroupingService.MappingWaybillNumberToGroup(detail.WaybillNumber);
                            }
                            return true;
                        }

                        if (preshipmentmobile.ZoneMapping != 1 && preshipmentmobile.IsInternationalShipment == false)
                        {
                            if (preshipmentmobile.shipmentstatus == MobilePickUpRequestStatus.PickedUp.ToString() || preshipmentmobile.shipmentstatus == MobilePickUpRequestStatus.OnwardProcessing.ToString())
                            {
                                if (preshipmentmobile.IsApproved != true && preshipmentmobile.IsInternationalShipment == false)
                                {
                                    int customerid = 0;
                                    bool isClassShipment = false;
                                    var companyid = await _uow.Company.GetAsync(s => s.CustomerCode == preshipmentmobile.CustomerCode);
                                    if (companyid != null)
                                    {
                                        customerid = companyid.CompanyId;
                                        preshipmentmobile.CustomerType = CustomerType.Company.ToString();

                                        if (companyid.Rank == Rank.Class)
                                        {
                                            isClassShipment = true;
                                        }
                                    }
                                    else
                                    {
                                        var CustomerId = await _uow.IndividualCustomer.GetAsync(s => s.CustomerCode == preshipmentmobile.CustomerCode);
                                        if (CustomerId != null)
                                        {
                                            customerid = CustomerId.IndividualCustomerId;
                                            preshipmentmobile.CustomerType = CustomerType.IndividualCustomer.ToString();
                                        }
                                        else
                                        {
                                            var partnerID = await _uow.Partner.GetAsync(s => s.PartnerCode == preshipmentmobile.CustomerCode);
                                            customerid = partnerID.PartnerId;
                                            preshipmentmobile.CustomerType = CustomerType.Partner.ToString();
                                        }
                                    }


                                    var UserServiceCenters = await _userService.GetPriviledgeServiceCenters();

                                    //default sc
                                    if (UserServiceCenters.Any())
                                    {
                                        detail.SenderServiceCentreId = UserServiceCenters[0];
                                    }

                                    //Fix For Home Delivery for Lagos, Abuja , Port Harcourt For now
                                    //if (preshipmentmobile.ReceiverStationId == 4 || preshipmentmobile.ReceiverStationId == 3 || preshipmentmobile.ReceiverStationId == 30)
                                    //{
                                    //    var stationData = await _uow.Station.GetAsync(x => x.StationId == preshipmentmobile.ReceiverStationId);
                                    //    detail.ReceiverServiceCentreId = stationData.SuperServiceCentreId;
                                    //}

                                    int departureCountryId = await GetCountryByServiceCentreId(detail.SenderServiceCentreId);
                                    int destinationCountryId = await GetCountryByServiceCentreId(detail.ReceiverServiceCentreId);
                                    var user = await _userService.GetCurrentUserId();
                                    var pickupprice = await GetPickUpPrice(preshipmentmobile.VehicleType, preshipmentmobile.CountryId, preshipmentmobile.UserId);

                                    //update receiver address
                                    if (!string.IsNullOrWhiteSpace(detail.ReceiverAddress))
                                    {
                                        preshipmentmobile.ReceiverAddress = detail.ReceiverAddress;
                                    }

                                    var destinationSC = await _centreService.GetServiceCentreById(detail.ReceiverServiceCentreId);
                                    //Get SuperCentre for Home Delivery
                                    if (preshipmentmobile.IsHomeDelivery)
                                    {
                                        //also check that the destination is not a hub
                                        if (destinationSC.IsHUB != true)
                                        {
                                            var stationData = await _uow.Station.GetAsync(x => x.StationId == destinationSC.StationId);
                                            detail.ReceiverServiceCentreId = stationData.SuperServiceCentreId;
                                        }
                                    }

                                    if (!preshipmentmobile.IsHomeDelivery && preshipmentmobile.DestinationServiceCenterId > 0)
                                    {
                                        var centre = await _uow.ServiceCentre.GetAsync(x => x.ServiceCentreId == preshipmentmobile.DestinationServiceCenterId);
                                        if (centre != null)
                                        {
                                            detail.ReceiverServiceCentreId = centre.ServiceCentreId;
                                        }
                                    }

                                    var mobileShipment = new ShipmentDTO
                                    {
                                        Waybill = preshipmentmobile.Waybill,
                                        ReceiverName = preshipmentmobile.ReceiverName,
                                        ReceiverPhoneNumber = preshipmentmobile.ReceiverPhoneNumber,
                                        ReceiverEmail = preshipmentmobile.ReceiverEmail,
                                        ReceiverAddress = preshipmentmobile.ReceiverAddress,
                                        DeliveryOptionId = 2,
                                        GrandTotal = preshipmentmobile.GrandTotal,
                                        Insurance = preshipmentmobile.InsuranceValue == null ? 0 : preshipmentmobile.InsuranceValue,
                                        Vat = preshipmentmobile.Vat == null ? 0 : preshipmentmobile.Vat,
                                        SenderAddress = preshipmentmobile.SenderAddress,
                                        IsCashOnDelivery = preshipmentmobile.IsCashOnDelivery,
                                        CustomerCode = preshipmentmobile.CustomerCode,
                                        DestinationServiceCentreId = detail.ReceiverServiceCentreId,
                                        DepartureServiceCentreId = detail.SenderServiceCentreId,
                                        CustomerId = customerid,
                                        UserId = user,
                                        PickupOptions = preshipmentmobile.IsHomeDelivery == true ? PickupOptions.HOMEDELIVERY : PickupOptions.SERVICECENTER,
                                        //  PickupOptions = PickupOptions.HOMEDELIVERY,
                                        IsdeclaredVal = preshipmentmobile.IsdeclaredVal,
                                        ShipmentPackagePrice = preshipmentmobile.ShipmentPackagePrice == null ? 0.00m : preshipmentmobile.ShipmentPackagePrice.Value,
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
                                        PackageOptionIds = detail.PackageOptionIds,
                                        IsClassShipment = isClassShipment,
                                        IsBulky = detail.IsBulky,
                                        ExpressDelivery = detail.ExpressDelivery,
                                        CashOnDeliveryAmount = preshipmentmobile.CashOnDeliveryAmount,
                                        CODDescription = preshipmentmobile.CODDescription,
                                        CODStatus = preshipmentmobile.CODStatus,
                                        CODStatusDate = preshipmentmobile.CODStatusDate,
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

                                    var insuranceValue = await _insuranceService.GetInsuranceValueByCountry();
                                    var factor = Convert.ToDecimal(Math.Pow(10, -2));
                                    mobileShipment.GrandTotal = Math.Round(mobileShipment.GrandTotal * factor) / factor;
                                    mobileShipment.Insurance = insuranceValue;
                                    mobileShipment.DeclarationOfValueCheck = mobileShipment.Value;
                                    mobileShipment.Total = mobileShipment.ShipmentItems.Select(x => x.Price).Sum();
                                    var status = await _shipmentService.AddShipmentFromMobile(mobileShipment);

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

                                    if (preshipmentmobile.IsCashOnDelivery == true)
                                    {
                                        //collect the cods and add to CashOnDeliveryRegisterAccount()
                                        var cashondeliveryentity = new CashOnDeliveryRegisterAccount
                                        {
                                            Amount = preshipmentmobile.CashOnDeliveryAmount ?? 0,
                                            CODStatusHistory = CODStatushistory.Created,
                                            Description = "Cod From Sales",
                                            ServiceCenterId = 0,
                                            Waybill = preshipmentmobile.Waybill,
                                            UserId = preshipmentmobile.UserId,
                                            DepartureServiceCenterId = detail.SenderServiceCentreId,
                                            DestinationCountryId = destinationCountryId
                                        };

                                        _uow.CashOnDeliveryRegisterAccount.Add(cashondeliveryentity);
                                    }

                                    await _uow.CompleteAsync();
                                }
                                else
                                {
                                    throw new GenericException("Shipment has already been approved!!!", $"{(int)HttpStatusCode.Forbidden}");
                                }
                            }
                            else if (preshipmentmobile.IsInternationalShipment == false)
                            {
                                throw new GenericException($"This shipment {detail.WaybillNumber} has not been marked as Picked Up. Delivery Partner should confirm pick up from his app.", $"{(int)HttpStatusCode.Forbidden}");
                            }
                        }
                        else
                        {
                            throw new GenericException("This shipment is not an interstate delivery, take to the assigned receiver's location", $"{(int)HttpStatusCode.Forbidden}");
                        }
                        if (detail.IsBulky)
                        {
                            await _autoManifestAndGroupingService.MappingWaybillNumberToGroupForBulk(detail.WaybillNumber);
                        }
                        else
                        {
                            await _autoManifestAndGroupingService.MappingWaybillNumberToGroup(detail.WaybillNumber);
                        }
                        return true;
                    }
                }
            }
            catch (Exception ex)
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
                throw;
            }
        }

        public async Task<MobilePriceDTO> GetHaulagePrice(HaulagePriceDTO haulagePricingDto)
        {
            try
            {
                decimal price = 0;

                //check haulage exists
                var haulage = await _haulageService.GetHaulageById(haulagePricingDto.Haulageid);
                var country = await _uow.Country.GetCountryByStationId(haulagePricingDto.DestinationStationId);
                if (country == null)
                {
                    throw new GenericException("Destination Station Country Not Found", $"{(int)HttpStatusCode.NotFound}");
                }
                var isWithinProcessingTime = await WithinProcessingTime(country.CountryId);
                //var discountPercent = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.DiscountPercentage, country.CountryId);
                //var percentage = Convert.ToDecimal(discountPercent.Value);
                var percentage = 0.00M;

                var percentageTobeUsed = ((100M - percentage) / 100M);
                var discount = (1 - percentageTobeUsed);

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

                //VAT
                var vatDTO = await _uow.VAT.GetAsync(x => x.CountryId == country.CountryId);
                decimal vat = (vatDTO != null) ? (vatDTO.Value / 100) : (7.5M / 100);

                var vatForPreshipment = price * vat;
                price += vatForPreshipment;

                //Get Discount based on Customer Rank
                var customerCode = await _userService.GetUserChannelCode();
                var priceDTO = new PricingDTO
                {
                    CountryId = country.CountryId,
                    CustomerCode = customerCode
                };
                price = await _pricingService.CalculateCustomerRankPrice(priceDTO, price);

                return new MobilePriceDTO
                {
                    DeliveryPrice = price,
                    GrandTotal = Math.Round(price * percentageTobeUsed),
                    CurrencySymbol = country.CurrencySymbol,
                    CurrencyCode = country.CurrencyCode,
                    IsWithinProcessingTime = isWithinProcessingTime,
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
                if (!String.IsNullOrEmpty(user.Email))
                {
                    var company = await _uow.Company.GetAsync(s => s.CustomerCode == user.UserChannelCode);
                    if(company != null)
                    {
                        // Update node merchant details
                        await UpdateNodeMerchantDetails(company, user);
                    }
                    currentUser.Email = user.Email;
                }
                if (!String.IsNullOrEmpty(user.PhoneNumber))
                {
                    currentUser.PhoneNumber = user.PhoneNumber;
                }
                await _userService.UpdateUser(currentUser.Id, currentUser);

                string userChannelType = currentUser.UserChannelType.ToString();
                user.UserChannelCode = currentUser.UserChannelCode;

                if (userChannelType == UserChannelType.Partner.ToString()
                    || userChannelType == UserChannelType.IndividualCustomer.ToString()
                    || userChannelType == UserChannelType.Ecommerce.ToString() || userChannelType == UserChannelType.Corporate.ToString())
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
                throw;
            }
        }

        public async Task<bool> UpdateVehicleProfile(UserDTO user)
        {
            try
            {
                //get currently login user
                var userId = await _userService.GetCurrentUserId();
                var userDetail = await _userService.GetUserById(userId);
                user.UserChannelCode = userDetail.UserChannelCode;

                var partner = await _uow.Partner.GetAsync(s => s.PartnerCode == user.UserChannelCode);
                if (partner == null && userDetail.UserChannelType == UserChannelType.Employee)
                {
                    var partnerDTO = new PartnerDTO
                    {
                        PartnerType = PartnerType.InternalDeliveryPartner,
                        PartnerName = userDetail.FirstName + " " + userDetail.LastName,
                        PartnerCode = userDetail.UserChannelCode,
                        FirstName = userDetail.FirstName,
                        LastName = userDetail.LastName,
                        Email = userDetail.Email,
                        PhoneNumber = userDetail.PhoneNumber,
                        UserId = userId,
                        IsActivated = false,
                    };
                    var FinalPartner = Mapper.Map<Partner>(partnerDTO);
                    _uow.Partner.Add(FinalPartner);
                }

                //Get all the vehicle Type in the system for the user
                var vehicleTypeList = await _uow.VehicleType.FindAsync(x => x.Partnercode == user.UserChannelCode);
                var vehicleTypeArray = vehicleTypeList.Select(x => x.Vehicletype).ToList();

                List<VehicleType> newVehicleTypes = new List<VehicleType>();

                foreach (var vehicle in user.VehicleType)
                {
                    if (!vehicleTypeArray.Contains(vehicle))
                    {
                        var vehicleData = new VehicleType
                        {
                            Vehicletype = vehicle.ToUpper(),
                            Partnercode = user.UserChannelCode
                        };
                        newVehicleTypes.Add(vehicleData);
                    }
                }
                _uow.VehicleType.AddRange(newVehicleTypes);
                await _uow.CompleteAsync();
                return true;
            }
            catch
            {
                throw;
            }
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
                    throw new GenericException("Shipment cannot be found", $"{(int)HttpStatusCode.NotFound}");
                }
                //Get user
                var userID = await _userService.GetCurrentUserId();
                var user = await _uow.User.GetUserById(userID);
                //FOR PARTNER TRYING TO CANCEL  A SHIPMENT
                if (user.UserChannelType.ToString() == UserChannelType.Partner.ToString())
                {
                    var pickuprequests = await _uow.MobilePickUpRequests.GetAsync(s => s.Waybill == preshipmentmobile.Waybill && s.UserId == userID);
                    if (pickuprequests != null)
                    {
                        pickuprequests.Status = MobilePickUpRequestStatus.Cancelled.ToString();
                    }

                    preshipmentmobile.shipmentstatus = MobilePickUpRequestStatus.Processing.ToString();
                }
                else
                {
                    // check the invoice table to be sure money was collected for the shipment
                    var walletTransaction = await _uow.WalletTransaction.GetAsync(s => s.Waybill == Waybill);
                    if (preshipmentmobile.shipmentstatus == "Shipment created" || preshipmentmobile.shipmentstatus == MobilePickUpRequestStatus.Rejected.ToString() || preshipmentmobile.shipmentstatus == MobilePickUpRequestStatus.TimedOut.ToString() || preshipmentmobile.shipmentstatus == MobilePickUpRequestStatus.PendingCancellation.ToString() || preshipmentmobile.shipmentstatus.ToLower() == "pending cancellation")
                    {
                        preshipmentmobile.shipmentstatus = MobilePickUpRequestStatus.Cancelled.ToString();
                        if (walletTransaction != null)
                        {
                            await UpdateCustomerWalletForCancelledShipment(preshipmentmobile.CustomerCode, preshipmentmobile.Waybill, preshipmentmobile.GrandTotal);

                        }
                        await ScanMobileShipment(new ScanDTO
                        {
                            WaybillNumber = Waybill,
                            ShipmentScanStatus = ShipmentScanStatus.MSCC
                        });

                        //remove from report
                        var report = await _uow.FinancialReport.GetAsync(s => s.Waybill == Waybill);
                        if (report != null)
                        {
                            report.IsDeleted = true;
                        }
                    }
                    else
                    {
                        throw new GenericException($"Shipment cannot be cancelled because it has a current status of {preshipmentmobile.shipmentstatus}.", $"{(int)HttpStatusCode.Forbidden}");
                    }
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
                if (!String.IsNullOrEmpty(user.Email))
                {
                    partner.Email = user.Email;
                }
                if (!String.IsNullOrEmpty(user.PhoneNumber))
                {
                    partner.PhoneNumber = user.PhoneNumber;
                }
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
                if (!String.IsNullOrEmpty(user.Email))
                {
                    customer.Email = user.Email;
                }
                if (!String.IsNullOrEmpty(user.PhoneNumber))
                {
                    customer.PhoneNumber = user.PhoneNumber;
                }
            }
        }
        private async Task UpdateCompany(UserDTO user)
        {
            var company = await _uow.Company.GetAsync(s => s.CustomerCode == user.UserChannelCode);
            if (company != null)
            {
                company.FirstName = user.FirstName;
                company.LastName = user.LastName;
                company.AccountNumber = user.AccountNumber;
                company.AccountName = user.AccountName;
                company.BankName = user.BankName;
                if (!String.IsNullOrEmpty(user.Organisation))
                {
                    if (!company.Name.Equals(user.Organisation, StringComparison.OrdinalIgnoreCase))
                    {
                        var exist = await _uow.Company.GetAsync(s => s.CompanyId != company.CompanyId && s.Name.ToLower() == user.Organisation.ToLower());
                        if (exist != null)
                        {
                            throw new GenericException("Company name already exist!!");
                        }
                        company.Name = user.Organisation;
                    }

                }
                if (!String.IsNullOrEmpty(user.Email))
                {
                    company.Email = user.Email;
                }
                if (!String.IsNullOrEmpty(user.PhoneNumber))
                {
                    company.PhoneNumber = user.PhoneNumber;
                }
            }
        }

        private async Task UpdateNodeMerchantDetails(Company company, UserDTO user)
        {
            if (!company.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase))
            {
                //Update Node 
                var payload = new UpdateNodeMercantDetailsDTO
                {
                    MerchantCode = company.CustomerCode,
                    Email = user.Email
                };
                await _nodeService.UpdateMerchantDetails(payload);
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

        public async Task<decimal> CalculateBikePriceBasedonLocation(PreShipmentMobileDTO item)
        {
            var Location = new LocationDTO
            {
                DestinationLatitude = (double)item.ReceiverLocation.Latitude,
                DestinationLongitude = (double)item.ReceiverLocation.Longitude,
                OriginLatitude = (double)item.SenderLocation.Latitude,
                OriginLongitude = (double)item.SenderLocation.Longitude
            };

            RootObject details = await _partnertransactionservice.GetGeoDetails(Location);
            var distance = (details.rows[0].elements[0].distance.value / 1000);
            decimal price = 0.0M;

            if (distance <= 25)
            {
                var priceFor25AndBelowkm = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.BikePrice25KM, item.CountryId);
                var priceFor25AndBelowkmValue = Convert.ToDecimal(priceFor25AndBelowkm.Value);
                price = distance * priceFor25AndBelowkmValue;
            }
            else if (distance > 25 && distance <= 35)
            {
                var priceFor25To35km = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.BikePrice26TO35KM, item.CountryId);
                var priceFor25To35kmValue = Convert.ToDecimal(priceFor25To35km.Value);
                price = distance * priceFor25To35kmValue;
            }
            else if (distance > 35)
            {
                var priceFor36Above = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.BikePrice36KM, item.CountryId);
                var priceFor36AboveValue = Convert.ToDecimal(priceFor36Above.Value);
                price = distance * priceFor36AboveValue;
            }

            return price;
        }

        public async Task<List<PreShipmentMobileDTO>> GetPreShipmentForEcommerce(string userChannelCode)
        {
            try
            {
                var shipment = await _uow.Shipment.FindAsync(x => x.CustomerCode == userChannelCode && x.IsCancelled == false);

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
                                                              DestinationServiceCentreId = r.DestinationServiceCentreId,
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
                    if (String.IsNullOrEmpty(shipments.ReceiverAddress))
                    {
                        shipments.ReceiverAddress = _uow.ServiceCentre.GetAllAsQueryable().FirstOrDefault(x => x.ServiceCentreId == shipments.DestinationServiceCentreId).FormattedServiceCentreName;

                    }
                }

                return await Task.FromResult(shipmentDto);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<PreShipmentMobileDTO>> GetShipments(string userChannelCode, ShipmentCollectionFilterCriteria filterCriteria)
        {
            try
            {
                var shipmentDto = new List<PreShipmentMobileDTO>();

                var mobileShipment = _uow.PreShipmentMobile.GetShipments(userChannelCode);

                if (filterCriteria.StartDate == null && filterCriteria.EndDate == null)
                {
                    shipmentDto = mobileShipment.OrderByDescending(x => x.DateCreated).Take(20).ToList();
                }
                else
                {
                    //get startDate and endDate
                    var queryDate = filterCriteria.getStartDateAndEndDate();
                    var startDate = queryDate.Item1;
                    var endDate = queryDate.Item2;

                    shipmentDto = mobileShipment.Where(s => s.DateCreated >= startDate && s.DateCreated < endDate).ToList();
                }

                foreach (var shipments in shipmentDto)
                {
                    if (shipments.CustomerType == "Individual")
                    {
                        shipments.CustomerType = CustomerType.IndividualCustomer.ToString();
                    }

                    if (shipments.SenderAddress == null)
                    {
                        CustomerType customerType = (CustomerType)Enum.Parse(typeof(CustomerType), shipments.CustomerType);
                        var CustomerDetails = await _customerService.GetCustomer(shipments.CustomerId, customerType);
                        shipments.SenderAddress = CustomerDetails.Address;
                    }
                }

                return await Task.FromResult(shipmentDto);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<decimal> GetPickUpPriceForIndividual(string vehicleType, int CountryId)
        {
            decimal PickUpPrice = 0.0M;

            if (string.IsNullOrWhiteSpace(vehicleType))
            {
                return PickUpPrice;
            }

            vehicleType = vehicleType.ToLower();

            if (vehicleType == Vehicletype.Car.ToString().ToLower())
            {
                var carPickUprice = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.CarPickupPrice, CountryId);
                PickUpPrice = Convert.ToDecimal(carPickUprice.Value);
            }
            else if (vehicleType == Vehicletype.Bike.ToString().ToLower())
            {
                var bikePickUprice = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.BikePickUpPrice, CountryId);
                PickUpPrice = Convert.ToDecimal(bikePickUprice.Value);
            }
            else if (vehicleType == Vehicletype.Van.ToString().ToLower())
            {
                var vanPickUprice = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.VanPickupPrice, CountryId);
                PickUpPrice = Convert.ToDecimal(vanPickUprice.Value);
            }
            return PickUpPrice;
        }

        private async Task<decimal> GetPickUpPriceForEcommerce(string vehicleType, int CountryId)
        {
            var PickUpPrice = 0.0M;

            if (string.IsNullOrWhiteSpace(vehicleType))
            {
                return PickUpPrice;
            }

            vehicleType = vehicleType.ToLower();

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

            if (!string.IsNullOrWhiteSpace(preShipmentDTO.VehicleType))
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
            if (!string.IsNullOrWhiteSpace(preShipmentDTO.VehicleType))
            {
                //1.if the shipment Vehicle Type is Bike and the zone is lagos, Calculate the Promo price
                if (zoneId == 1 && preShipmentDTO.VehicleType.ToLower() == Vehicletype.Bike.ToString().ToLower())
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

            var shipmentsResult = _uow.PreShipmentMobile.GetAllAsQueryable().Where(s => s.DateCreated >= startDate && s.DateCreated < endDate && (s.shipmentstatus == MobilePickUpRequestStatus.Processing.ToString() || s.shipmentstatus == "Shipment created")
                                                                         && !testUserId.Contains(s.UserId));


            List<LocationDTO> locationDTOs = (from r in shipmentsResult
                                              select new LocationDTO()
                                              {
                                                  Longitude = r.SenderLocation.Longitude,
                                                  Latitude = r.SenderLocation.Latitude
                                              }).ToList();

            return await Task.FromResult(locationDTOs.OrderByDescending(x => x.DateCreated).ToList());
        }

        //method called after no action is done on a shipment with status 'Assigned for Pickup' after 90 minutes  
        public async Task<bool> ChangeShipmentOwnershipForPartner(PartnerReAssignmentDTO request)
        {
            try
            {
                var currentPartnerData = await _uow.Partner.GetAsync(x => x.UserId == request.CurrentPartnerId);
                if (currentPartnerData == null)
                {
                    throw new GenericException("Current Partner does not exist", $"{(int)HttpStatusCode.NotFound}");
                }

                var waybillData = await _uow.PreShipmentMobile.GetAsync(x => x.Waybill == request.Waybill);
                if (waybillData == null)
                {
                    throw new GenericException("Waybill does not exist", $"{(int)HttpStatusCode.NotFound}");
                }

                if (waybillData.shipmentstatus != "Assigned for Pickup")
                {
                    throw new GenericException("Riders can not be switched at this point", $"{(int)HttpStatusCode.Forbidden}");
                }
                var formerpickup = await _uow.MobilePickUpRequests.GetAsync(x => x.UserId == request.CurrentPartnerId && x.Waybill == request.Waybill);

                if (formerpickup == null)
                {
                    throw new GenericException($"Partner {currentPartnerData.PartnerName} is not currently assigned to this {request.Waybill}", $"{(int)HttpStatusCode.NotFound}");
                }

                if (formerpickup.Status != MobilePickUpRequestStatus.Accepted.ToString() && formerpickup.Status != "Enroute Pickup" && formerpickup.Status != MobilePickUpRequestStatus.Arrived.ToString())
                {
                    throw new GenericException($"Partner {currentPartnerData.PartnerName} status has to be Accepted", $"{(int)HttpStatusCode.Forbidden}");
                }

                formerpickup.Status = MobilePickUpRequestStatus.Moved.ToString();
                waybillData.shipmentstatus = "Shipment created";

                string scanStatus = ShipmentScanStatus.DLP.ToString();
                await _mobiletrackingservice.AddMobileShipmentTracking(new MobileShipmentTrackingDTO
                {
                    DateTime = DateTime.Now,
                    Status = scanStatus,
                    Waybill = request.Waybill,
                    User = request.CurrentPartnerId
                }, ShipmentScanStatus.DLP);

                //Update Activity Status
                await UpdateActivityStatus(request.CurrentPartnerId, ActivityStatus.OffDelivery);

                await _uow.CompleteAsync();
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<PreShipmentMobileDTO>> GetPreShipmentsAndShipmentsPaginated(ShipmentAndPreShipmentParamDTO shipmentAndPreShipmentParamDTO)
        {
            try
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
                int totalCount;
                var currentUser = await _userService.GetCurrentUserId();
                var user = await _uow.User.GetUserById(currentUser);
                var mobileShipments = new List<PreShipmentMobile>();
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
                    mobileShipments = _uow.PreShipmentMobile.Query(x => x.CustomerCode == user.UserChannelCode && x.DateCreated >= shipmentAndPreShipmentParamDTO.StartDate && x.DateCreated <= shipmentAndPreShipmentParamDTO.EndDate).Include(x => x.PreShipmentItems).Include(x => x.SenderLocation).Include(x => x.ReceiverLocation).SelectPage(shipmentAndPreShipmentParamDTO.Page, shipmentAndPreShipmentParamDTO.PageSize, out totalCount).ToList();
                }
                else
                {
                    mobileShipments = _uow.PreShipmentMobile.Query(x => x.CustomerCode == user.UserChannelCode).Include(x => x.PreShipmentItems).Include(x => x.SenderLocation).Include(x => x.ReceiverLocation).SelectPage(shipmentAndPreShipmentParamDTO.Page, shipmentAndPreShipmentParamDTO.PageSize, out totalCount).ToList();
                }

                List<PreShipmentMobileDTO> shipmentDto = (from r in mobileShipments
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
                                                              InputtedSenderAddress = r.InputtedSenderAddress,
                                                              InputtedReceiverAddress = r.InputtedReceiverAddress,
                                                              SenderLocality = r.SenderLocality,
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
                                                          }).OrderByDescending(x => x.DateCreated).ToList();
                var agilityShipment = await GetPreShipmentForEcommercePaginated(shipmentAndPreShipmentParamDTO, user.UserChannelCode);

                //get all waybills in shipmentlist
                var arrWaybills = shipmentDto.Select(x => x.Waybill).ToArray();
                var partnerMoblieRequests = await _uow.MobilePickUpRequests.FindAsync(r => arrWaybills.Contains(r.Waybill) && r.Status == MobilePickUpRequestStatus.Delivered.ToString());

                //partner information using id
                var partnerIds = partnerMoblieRequests.Select(x => x.UserId).ToArray();
                var partnerInfo = await _uow.User.GetUsers(partnerIds);

                //get all ratings for all waybill in the shipment list
                var ratings = await _uow.MobileRating.FindAsync(r => arrWaybills.Contains(r.Waybill));

                //get all stationids in the shipment list
                var stationIds = shipmentDto.Select(x => x.SenderStationId).ToArray();
                var stations = await _uow.Country.GetCountryByStationId(stationIds);

                //added agility shipment to Giglgo list of shipments.
                foreach (var shipment in shipmentDto)
                {
                    if (agilityShipment.Exists(s => s.Waybill == shipment.Waybill))
                    {
                        var s = agilityShipment.Where(x => x.Waybill == shipment.Waybill).FirstOrDefault();
                        agilityShipment.Remove(s);
                    }

                    var partner = partnerMoblieRequests.Where(r => r.Waybill == shipment.Waybill).FirstOrDefault();
                    if (partner != null)
                    {
                        var partneruser = partnerInfo.Where(x => x.Id == partner.UserId).FirstOrDefault();
                        if (partneruser != null)
                        {
                            shipment.PartnerFirstName = partneruser.FirstName;
                            shipment.PartnerLastName = partneruser.LastName;
                            shipment.PartnerImageUrl = partneruser.PictureUrl;
                        }
                    }

                    shipment.IsRated = false;

                    var rating = ratings.Where(j => j.Waybill == shipment.Waybill).FirstOrDefault();
                    if (rating != null)
                    {
                        shipment.IsRated = rating.IsRatedByCustomer;
                    }

                    var country = stations.Where(x => x.StationId == shipment.SenderStationId).FirstOrDefault();
                    if (country != null)
                    {
                        shipment.CurrencyCode = country.CurrencyCode;
                        shipment.CurrencySymbol = country.CurrencySymbol;
                    }
                }

                var newlist = shipmentDto.Union(agilityShipment);
                return await Task.FromResult(newlist.OrderByDescending(x => x.DateCreated).Take(shipmentAndPreShipmentParamDTO.PageSize).ToList());
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<PreShipmentMobileDTO>> GetPreShipmentForEcommercePaginated(ShipmentAndPreShipmentParamDTO shipmentAndPreShipmentParamDTO, string userChannelCode)
        {
            try
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
                int totalCount;
                var shipmentList = new List<Shipment>();
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
                    shipmentList = _uow.Shipment.Query(x => x.CustomerCode == userChannelCode && x.IsCancelled == false && x.DateCreated >= shipmentAndPreShipmentParamDTO.StartDate && x.DateCreated <= shipmentAndPreShipmentParamDTO.EndDate).SelectPage(shipmentAndPreShipmentParamDTO.Page, shipmentAndPreShipmentParamDTO.PageSize, out totalCount).ToList();
                }
                else
                {
                    shipmentList = _uow.Shipment.Query(x => x.CustomerCode == userChannelCode && x.IsCancelled == false).SelectPage(shipmentAndPreShipmentParamDTO.Page, shipmentAndPreShipmentParamDTO.PageSize, out totalCount).ToList();
                }

                List<PreShipmentMobileDTO> shipmentDto = (from r in shipmentList
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
                                                              shipmentstatus = r.ShipmentScanStatus.ToString(),
                                                              CustomerId = r.CustomerId,
                                                              CustomerType = r.CustomerType,
                                                              CountryId = r.DepartureCountryId
                                                          }).OrderByDescending(x => x.DateCreated).ToList();

                //get all stationids in the shipment list
                var countryIds = shipmentDto.Select(x => x.CountryId).ToArray();
                var countryInfo = await _uow.Country.GetCountries(countryIds);

                foreach (var shipments in shipmentDto)
                {
                    var country = countryInfo.Where(x => x.CountryId == shipments.CountryId).FirstOrDefault();

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
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<List<PreShipmentMobileDTO>> GetBatchPreShipmentMobile(string userChannelCode)
        {
            try
            {
                var batchedPreshipment = _uow.PreShipmentMobile.GetBatchedPreShipmentForUser(userChannelCode).ToList();
                return batchedPreshipment;
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<List<CompanyDTO>> GetBatchPreShipmentMobileOwners()
        {
            try
            {
                var customerCodes = _uow.PreShipmentMobile.GetAllBatchedPreShipment().GroupBy(x => x.CustomerCode).Select(x => x.Key).ToList();
                return await _uow.Company.GetCompaniesByCodes(customerCodes);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<object> UpdateShipmentToDelivered(ShipmentTrackingDTO trackingDTO)
        {
            try
            {
                object Id = null;
                //Get shipment Details
                var shipment = await _uow.Shipment.GetAsync(x => x.Waybill.Equals(trackingDTO.Waybill));

                if (shipment.PickupOptions == PickupOptions.SERVICECENTER)
                {
                    string scanStatus = ShipmentScanStatus.ARF.ToString();
                    trackingDTO.Status = scanStatus;
                    bool shipmentTrackingExists = await _uow.ShipmentTracking.ExistAsync(x => x.Waybill.Equals(trackingDTO.Waybill) && x.Status.Equals(scanStatus));
                    if (!shipmentTrackingExists)
                    {
                        if (trackingDTO.User == null)
                        {
                            trackingDTO.User = await _userService.GetCurrentUserId();
                        }

                        shipment.ShipmentScanStatus = ShipmentScanStatus.ARF;

                        if (shipment.PickupOptions == PickupOptions.SERVICECENTER)
                        {
                            //add service centre
                            var newShipmentCollection = new ShipmentCollection
                            {
                                Waybill = trackingDTO.Waybill,
                                ShipmentScanStatus = ShipmentScanStatus.ARF,
                                DepartureServiceCentreId = shipment.DepartureServiceCentreId,
                                DestinationServiceCentreId = shipment.DestinationServiceCentreId,
                                IsCashOnDelivery = shipment.IsCashOnDelivery
                            };

                            _uow.ShipmentCollection.Add(newShipmentCollection);
                        }

                        var newShipmentTracking = new ShipmentTracking
                        {
                            Waybill = trackingDTO.Waybill,
                            Location = trackingDTO.Location,
                            Status = trackingDTO.Status,
                            DateTime = DateTime.Now,
                            UserId = trackingDTO.User,
                            ServiceCentreId = trackingDTO.ServiceCentreId
                        };
                        _uow.ShipmentTracking.Add(newShipmentTracking);

                        Id = newShipmentTracking.ShipmentTrackingId;

                        if (shipment.PickupOptions == PickupOptions.SERVICECENTER)
                        {
                            var messageType = MessageType.ShipmentCreation;
                            foreach (var item in Enum.GetValues(typeof(MessageType)).Cast<MessageType>())
                            {
                                if (item.ToString() == scanStatus.ToString())
                                {
                                    messageType = (MessageType)Enum.Parse(typeof(MessageType), scanStatus.ToString());
                                    break;
                                }
                            }

                            //send message
                            await _messageSenderService.SendMessage(messageType, EmailSmsType.All, trackingDTO);
                        }

                    }
                }
                else if (shipment.PickupOptions == PickupOptions.HOMEDELIVERY)
                {
                    string scanStatus = ShipmentScanStatus.OKC.ToString();
                    trackingDTO.Status = scanStatus;
                    bool shipmentTrackingExists = await _uow.ShipmentTracking.ExistAsync(x => x.Waybill.Equals(trackingDTO.Waybill) && x.Status.Equals(scanStatus));
                    if (!shipmentTrackingExists)
                    {
                        if (trackingDTO.User == null)
                        {
                            trackingDTO.User = await _userService.GetCurrentUserId();
                        }

                        shipment.ShipmentScanStatus = ShipmentScanStatus.OKC;

                        ////add service centre
                        //var newShipmentCollection = new ShipmentCollection
                        //{
                        //    Waybill = trackingDTO.Waybill,
                        //    ShipmentScanStatus = ShipmentScanStatus.OKC,
                        //    DepartureServiceCentreId = shipment.DepartureServiceCentreId,
                        //    DestinationServiceCentreId = shipment.DestinationServiceCentreId,
                        //    IsCashOnDelivery = shipment.IsCashOnDelivery
                        //};
                        //_uow.ShipmentCollection.Add(newShipmentCollection);

                        var newShipmentTracking = new ShipmentTracking
                        {
                            Waybill = trackingDTO.Waybill,
                            Location = shipment.ReceiverAddress,
                            Status = trackingDTO.Status,
                            DateTime = DateTime.Now,
                            UserId = trackingDTO.User,
                            ServiceCentreId = trackingDTO.ServiceCentreId
                        };
                        _uow.ShipmentTracking.Add(newShipmentTracking);
                        Id = newShipmentTracking.ShipmentTrackingId;
                    }
                }
                else
                {
                    throw new GenericException($"Shipment with waybill: {trackingDTO.Waybill} already scan as 'SHIPMENT ARRIVED FINAL DESTINATION'");
                }

                await _uow.CompleteAsync();
                return new { Id };
            }

            catch (Exception ex)
            {
                throw;
            }

        }

        public async Task<NewNodeResponse> RemoveShipmentFromQueue(string waybill)
        {
            try
            {
                var result = await _nodeService.RemoveShipmentFromQueue(waybill);
                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public async Task<bool> AddShipmentToQueue(string waybill)
        {
            try
            {
                var preShipment = await _uow.PreShipmentMobile.GetAsync(x => x.Waybill == waybill, "PreShipmentItems,SenderLocation,ReceiverLocation,serviceCentreLocation");
                if (preShipment == null)
                {
                    throw new GenericException($"Wabill number -- {waybill} not found ", $"{(int)HttpStatusCode.NotFound}");
                }
                await NodeApiCreateShipment(preShipment);
                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public async Task<object> GetGIGGOProgressReport()
        {
            try
            {
                //1.After 2 hours agility should show exception reports of all shipments created that has not been picked up.
                //2.After 3 hours agility should show an exception report of all shipments assigned for picked - up and not pick up.
                //3.After 3 hours agility should show an exception report of all shipments picked up but not delivered.

                var dateFor2Hours = DateTime.Now.AddHours(-2);
                var dateFor3Hours = DateTime.Now.AddHours(-2);
                var preShipmentCreated = _uow.PreShipmentMobile.GetAllAsQueryable().Where(x => x.shipmentstatus == "Shipment created" && x.DateCreated < dateFor2Hours).Count();
                var preShipmentAssigned = _uow.PreShipmentMobile.GetAllAsQueryable().Where(x => x.shipmentstatus == "Assigned for Pickup" && x.DateModified < dateFor3Hours).Count();
                var preShipmentPicked = _uow.PreShipmentMobile.GetAllAsQueryable().Where(x => x.shipmentstatus == "PickedUp" && x.DateModified < dateFor3Hours).Count();

                return new
                {
                    shipmentCreate = preShipmentCreated,
                    shipmentAssigned = preShipmentAssigned,
                    shipmentPick = preShipmentPicked
                };
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<List<PreShipmentMobileDTO>> GetGIGGOProgressReportForShipmentCreated()
        {
            try
            {
                var dateFor2Hours = DateTime.Now.AddHours(-2);
                var preShipmentCreated = _uow.PreShipmentMobile.GetAllAsQueryable().Where(x => x.shipmentstatus == "Shipment created" && x.DateCreated < dateFor2Hours).OrderByDescending(x => x.DateCreated).ToList();
                return Mapper.Map<List<PreShipmentMobileDTO>>(preShipmentCreated);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<List<PreShipmentMobileDTO>> GetGIGGOProgressReportForShipmentAssigned()
        {
            try
            {
                var dateFor3Hours = DateTime.Now.AddHours(-3);
                var preShipmentAssigned = _uow.PreShipmentMobile.GetAllAsQueryable().Where(x => x.shipmentstatus == "Assigned for Pickup" && x.DateModified < dateFor3Hours).OrderByDescending(x => x.DateCreated).ToList();
                return Mapper.Map<List<PreShipmentMobileDTO>>(preShipmentAssigned);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<List<PreShipmentMobileDTO>> GetGIGGOProgressReportForShipmentPicked()
        {
            try
            {
                var dateFor3Hours = DateTime.Now.AddHours(-3);
                var preShipmentPicked = _uow.PreShipmentMobile.GetAllAsQueryable().Where(x => x.shipmentstatus == "PickedUp" && x.DateModified < dateFor3Hours).OrderByDescending(x => x.DateCreated).ToList();
                return Mapper.Map<List<PreShipmentMobileDTO>>(preShipmentPicked);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<List<PreShipmentMobileTATDTO>> GetPreshipmentMobileTAT(NewFilterOptionsDto newFilterOptionsDto)
        {
            try
            {
                var preshipmentmobile = new List<PreShipmentMobile>();
                var preshipmentmobileTATDTO = new List<PreShipmentMobileTATDTO>();

                var range = (int)(newFilterOptionsDto.EndDate.Value - newFilterOptionsDto.StartDate.Value).TotalDays;
                if (range > 32)
                {
                    throw new GenericException($"This report can not pull more than a month record ", $"{(int)HttpStatusCode.BadRequest}");
                }

                var dateFor24Hours = DateTime.Now;
                var dateFor72Hours = DateTime.Now;

                if (!String.IsNullOrEmpty(newFilterOptionsDto.FilterType) && newFilterOptionsDto.FilterType == "OverdueTATIntrastate")
                {
                    preshipmentmobile = _uow.PreShipmentMobile.GetAllAsQueryable().Where(x => x.ZoneMapping == 1 && x.DateCreated >= newFilterOptionsDto.StartDate && x.DateCreated <= newFilterOptionsDto.EndDate && x.IsCancelled == false && x.shipmentstatus != MobilePickUpRequestStatus.Cancelled.ToString()).OrderByDescending(x => x.DateCreated).ToList();
                    // preshipmentmobile = preshipmentmobile.Where(x => (int)(dateFor24Hours - x.DateCreated).TotalHours > 24).ToList();
                    if (preshipmentmobile.Any())
                    {
                        var waybills = preshipmentmobile.Select(x => x.Waybill).ToList();
                        var mobiletracking = _uow.MobileShipmentTracking.GetAllAsQueryable().Where(x => waybills.Contains(x.Waybill)).ToList();
                        var tats = await GetOverdueTATIntrastate(waybills, mobiletracking, preshipmentmobile);
                        preshipmentmobileTATDTO.AddRange(tats);
                    }
                }
                else
                {
                    preshipmentmobile = _uow.PreShipmentMobile.GetAllAsQueryable().Where(x => x.ZoneMapping > 1 && x.DateCreated >= newFilterOptionsDto.StartDate && x.DateCreated <= newFilterOptionsDto.EndDate && x.IsCancelled == false && x.shipmentstatus != MobilePickUpRequestStatus.Cancelled.ToString()).OrderByDescending(x => x.DateCreated).ToList();
                    // preshipmentmobile = preshipmentmobile.Where(x => (int)(dateFor72Hours - x.DateCreated).TotalHours > 24).ToList();
                    if (preshipmentmobile.Any())
                    {
                        var waybills = preshipmentmobile.Select(x => x.Waybill).ToList();
                        var mobiletracking = _uow.MobileShipmentTracking.GetAllAsQueryable().Where(x => waybills.Contains(x.Waybill)).ToList();
                        var shipmenttracking = _uow.ShipmentTracking.GetAllAsQueryable().Where(x => waybills.Contains(x.Waybill)).ToList();
                        var tats = await GetOverdueTATInterstate(waybills, mobiletracking, shipmenttracking, preshipmentmobile);
                        preshipmentmobileTATDTO.AddRange(tats);
                    }
                }
                return preshipmentmobileTATDTO;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task<PreShipmentMobileTATDTO> MapPreShipmentMobileTATDTO(PreShipmentMobile mobile)
        {
            try
            {
                var tat = new PreShipmentMobileTATDTO();
                tat.Waybill = mobile.Waybill;
                tat.SenderName = mobile.SenderName;
                tat.SenderPhoneNumber = mobile.SenderPhoneNumber;
                tat.SenderAddress = mobile.SenderAddress;
                tat.ReceiverName = mobile.ReceiverName;
                tat.ReceiverPhoneNumber = mobile.ReceiverPhoneNumber;
                tat.ReceiverAddress = mobile.ReceiverAddress;
                tat.CalculatedTotal = mobile.CalculatedTotal.Value;
                tat.VehicleType = mobile.VehicleType;
                tat.DateCreated = mobile.DateCreated;
                tat.IsHomeDelivery = mobile.IsHomeDelivery;
                tat.IsScheduled = mobile.IsScheduled;
                tat.SenderLocality = mobile.SenderLocality;
                tat.shipmentstatus = mobile.shipmentstatus;
                return tat;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task<List<PreShipmentMobileTATDTO>> GetOverdueTATIntrastate(List<string> waybills, List<MobileShipmentTracking> mobiletracking, List<PreShipmentMobile> preshipmentmobile)
        {
            try
            {
                var min = "Min";
                var hrs = "Hr";
                var preshipmentmobileTATDTO = new List<PreShipmentMobileTATDTO>();
                foreach (var item in preshipmentmobile)
                {
                    var tat = await MapPreShipmentMobileTATDTO(item);
                    //calc the remaining TATs
                    var shimentStatus = mobiletracking.Where(x => x.Waybill == item.Waybill).OrderByDescending(x => x.DateCreated).FirstOrDefault();
                    if (shimentStatus != null)
                    {
                        tat.ShipmentScanStatus = shimentStatus.Status;
                        tat.LastScanDate = shimentStatus.DateCreated;
                    }
                    else if (shimentStatus == null && item.shipmentstatus.ToLower() == "shipment created")
                    {
                        tat.ShipmentScanStatus = ShipmentScanStatus.MCRT.ToString();
                        tat.LastScanDate = item.DateCreated;
                    }
                    var dlvrd = mobiletracking.OrderByDescending(x => x.DateCreated).FirstOrDefault(x => x.Status == ShipmentScanStatus.MAHD.ToString() && x.Waybill == item.Waybill);
                    if (dlvrd != null)
                    {
                        var tatAge = (int)(dlvrd.DateCreated - item.DateCreated).TotalHours;
                        tat.OATAT = $"{tatAge}{hrs}";
                        if (tatAge <= 0)
                        {
                            tatAge = (int)(dlvrd.DateCreated - item.DateCreated).TotalMinutes;
                            tat.OATAT = $"{tatAge}{min}";
                        }
                    }
                    var assigned = mobiletracking.OrderByDescending(x => x.DateCreated).FirstOrDefault(x => x.Status == ShipmentScanStatus.MAPT.ToString() && x.Waybill == item.Waybill);
                    if (assigned != null)
                    {
                        var tatAge = (int)(assigned.DateCreated - item.DateCreated).TotalHours;
                        tat.AssignTAT = $"{tatAge}{hrs}";
                        if (tatAge <= 0)
                        {
                            tatAge = (int)(assigned.DateCreated - item.DateCreated).TotalMinutes;
                            tat.AssignTAT = $"{tatAge}{min}";
                        }
                    }

                    var picked = mobiletracking.OrderByDescending(x => x.DateCreated).FirstOrDefault(x => x.Status == ShipmentScanStatus.MSHC.ToString() && x.Waybill == item.Waybill);
                    if (picked != null && assigned != null)
                    {
                        var tatAge = (int)(picked.DateCreated - assigned.DateCreated).TotalHours;
                        tat.PickupTAT = $"{tatAge}{hrs}";
                        if (tatAge <= 0)
                        {
                            tatAge = (int)(picked.DateCreated - assigned.DateCreated).TotalMinutes;
                            tat.PickupTAT = $"{tatAge}{min}";
                        }
                    }

                    if (dlvrd != null && picked != null)
                    {
                        var tatAge = (int)(dlvrd.DateCreated - picked.DateCreated).TotalHours;
                        tat.DeliveryTAT = $"{tatAge}{hrs}";
                        if (tatAge <= 0)
                        {
                            tatAge = (int)(dlvrd.DateCreated - picked.DateCreated).TotalMinutes; ;
                            tat.DeliveryTAT = $"{tatAge}{min}";
                        }
                    }
                    preshipmentmobileTATDTO.Add(tat);
                }
                return preshipmentmobileTATDTO;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task<List<PreShipmentMobileTATDTO>> GetOverdueTATInterstate(List<string> waybills, List<MobileShipmentTracking> mobiletracking, List<ShipmentTracking> shipmenttracking, List<PreShipmentMobile> preshipmentmobile)
        {
            try
            {
                var min = "Min";
                var hrs = "Hr";
                var preshipmentmobileTATDTO = new List<PreShipmentMobileTATDTO>();
                foreach (var item in preshipmentmobile)
                {
                    var tat = await MapPreShipmentMobileTATDTO(item);
                    //calc the remaining TATs 
                    if (item.shipmentstatus.ToLower() == MobilePickUpRequestStatus.OnwardProcessing.ToString().ToLower())
                    {
                        var shimentStatus = shipmenttracking.Where(x => x.Waybill == item.Waybill).OrderByDescending(x => x.DateCreated).FirstOrDefault();
                        if (shimentStatus != null)
                        {
                            tat.ShipmentScanStatus = shimentStatus.Status;
                            tat.LastScanDate = shimentStatus.DateCreated;
                        }
                        else if (shimentStatus == null && item.shipmentstatus.ToLower() == "shipment created")
                        {
                            tat.ShipmentScanStatus = ShipmentScanStatus.MCRT.ToString();
                            tat.LastScanDate = item.DateCreated;
                        }

                        var dlvrd = shipmenttracking.OrderByDescending(x => x.DateCreated).FirstOrDefault(x => x.Waybill == item.Waybill && (x.Status == ShipmentScanStatus.OKC.ToString() || x.Status == ShipmentScanStatus.OKT.ToString()));
                        if (dlvrd != null)
                        {
                            var tatAge = (int)(dlvrd.DateCreated - item.DateCreated).TotalHours;
                            tat.OATAT = $"{tatAge}{hrs}";
                            if (tatAge <= 0)
                            {
                                tatAge = (int)(dlvrd.DateCreated - item.DateCreated).TotalMinutes;
                                tat.OATAT = $"{tatAge}{min}";
                            }
                        }
                        var assigned = shipmenttracking.OrderByDescending(x => x.DateCreated).FirstOrDefault(x => x.Status == ShipmentScanStatus.WC.ToString() && x.Waybill == item.Waybill);
                        if (assigned != null)
                        {
                            var tatAge = (int)(assigned.DateCreated - shipmenttracking.OrderByDescending(x => x.DateCreated).FirstOrDefault(x => x.Waybill == item.Waybill).DateCreated).TotalHours;
                            tat.AssignTAT = $"{tatAge}{hrs}";
                            if (tatAge <= 0)
                            {
                                tatAge = (int)(assigned.DateCreated - shipmenttracking.OrderByDescending(x => x.DateCreated).FirstOrDefault(x => x.Waybill == item.Waybill).DateCreated).TotalMinutes;
                                tat.AssignTAT = $"{tatAge}{min}";
                            }
                        }

                        var picked = shipmenttracking.OrderByDescending(x => x.DateCreated).FirstOrDefault(x => x.Status == ShipmentScanStatus.WC.ToString() && x.Waybill == item.Waybill);
                        if (picked != null && assigned != null)
                        {
                            var tatAge = (int)(picked.DateCreated - assigned.DateCreated).TotalHours;
                            tat.PickupTAT = $"{tatAge}{hrs}";
                            if (tatAge <= 0)
                            {
                                tatAge = (int)(picked.DateCreated - assigned.DateCreated).TotalMinutes;
                                tat.PickupTAT = $"{tatAge}{min}";
                            }
                        }

                        if (dlvrd != null && picked != null)
                        {
                            var tatAge = (int)(dlvrd.DateCreated - picked.DateCreated).TotalHours;
                            tat.DeliveryTAT = $"{tatAge}{hrs}";
                            if (tatAge <= 0)
                            {
                                tatAge = (int)(dlvrd.DateCreated - picked.DateCreated).TotalMinutes; ;
                                tat.DeliveryTAT = $"{tatAge}{min}";
                            }
                        }
                        preshipmentmobileTATDTO.Add(tat);
                    }
                    else
                    {
                        var tats = await GetOverdueTATIntrastate(waybills, mobiletracking, preshipmentmobile);
                        preshipmentmobileTATDTO.AddRange(tats);
                        break;
                    }

                }
                return preshipmentmobileTATDTO;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public async Task<List<PreShipmentMobileTATDTO>> GetPreshipmentMobileDeliveryTAT(NewFilterOptionsDto newFilterOptionsDto)
        {
            try
            {
                var preshipmentmobile = new List<PreShipmentMobile>();
                var preshipmentmobileTATDTO = new List<PreShipmentMobileTATDTO>();

                //var range = (int)(newFilterOptionsDto.EndDate - newFilterOptionsDto.StartDate).TotalDays;
                //if (range > 32)
                //{
                //    throw new GenericException($"This report can not pull more than a month record ", $"{(int)HttpStatusCode.BadRequest}");
                //}

                var dateFor24Hours = newFilterOptionsDto.StartDate.Value.AddHours(24);

                preshipmentmobile = _uow.PreShipmentMobile.GetAllAsQueryable().Where(x => x.ZoneMapping == 1 && x.DateCreated >= newFilterOptionsDto.StartDate && x.DateCreated <= newFilterOptionsDto.EndDate && x.IsCancelled == false && x.shipmentstatus != MobilePickUpRequestStatus.Cancelled.ToString() && x.shipmentstatus == MobilePickUpRequestStatus.Delivered.ToString()).OrderByDescending(x => x.DateCreated).ToList();
                preshipmentmobile = preshipmentmobile.Where(x => (int)(x.DateModified - x.DateCreated).TotalHours > 24).ToList();
                if (preshipmentmobile.Any())
                {
                    var waybills = preshipmentmobile.Select(x => x.Waybill).ToList();
                    var mobiletracking = _uow.MobileShipmentTracking.GetAllAsQueryable().Where(x => waybills.Contains(x.Waybill)).ToList();
                    var tats = await GetDeliveryTATIntrastate(waybills, mobiletracking, preshipmentmobile);
                    preshipmentmobileTATDTO.AddRange(tats);
                }
                return preshipmentmobileTATDTO;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task<List<PreShipmentMobileTATDTO>> GetDeliveryTATIntrastate(List<string> waybills, List<MobileShipmentTracking> mobiletracking, List<PreShipmentMobile> preshipmentmobile)
        {
            try
            {
                var min = "Min";
                var hrs = "Hr";
                var preshipmentmobileTATDTO = new List<PreShipmentMobileTATDTO>();
                var pickupRequests = _uow.MobilePickUpRequests.GetAllAsQueryable().Where(x => waybills.Contains(x.Waybill)).ToList();
                var userIDs = pickupRequests.Select(s => s.UserId).ToList();
                var partners = _uow.Partner.GetAllAsQueryable().Where(x => userIDs.Contains(x.UserId)).ToList();

                foreach (var item in preshipmentmobile)
                {
                    var tat = await MapPreShipmentMobileTATDTO(item);
                    //calc the remaining TATs
                    var shimentStatus = mobiletracking.Where(x => x.Waybill == item.Waybill).OrderByDescending(x => x.DateCreated).FirstOrDefault();
                    if (shimentStatus != null)
                    {
                        tat.ShipmentScanStatus = shimentStatus.Status;
                        tat.LastScanDate = shimentStatus.DateModified;
                    }
                    var dlvrd = mobiletracking.OrderByDescending(x => x.DateCreated).FirstOrDefault(x => x.Status == ShipmentScanStatus.MAHD.ToString() && x.Waybill == item.Waybill);
                    if (dlvrd != null)
                    {
                        var tatAge = (int)(dlvrd.DateCreated - item.DateCreated).TotalHours;
                        tat.OATAT = $"{tatAge}{hrs}";
                        if (tatAge <= 0)
                        {
                            tatAge = (int)(dlvrd.DateCreated - item.DateCreated).TotalMinutes;
                            tat.OATAT = $"{tatAge}{min}";
                        }
                    }

                    //also get the delivery partner
                    var itemDelivered = pickupRequests.Where(x => x.Status == MobilePickUpRequestStatus.Delivered.ToString() && x.Waybill == item.Waybill).FirstOrDefault();
                    if (itemDelivered != null)
                    {
                        var partner = partners.FirstOrDefault(x => x.UserId == itemDelivered.UserId);
                        if (partner != null)
                        {
                            tat.PartnerName = partner.PartnerName;
                            tat.DeliveredTime = itemDelivered.DateCreated;
                            tat.PartnerType = partner.PartnerType.ToString();
                        }
                    }
                    preshipmentmobileTATDTO.Add(tat);
                }
                return preshipmentmobileTATDTO;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<NewNodeResponse> RemoveNodePendingShipment(PendingNodeShipmentDTO dto)
        {
            try
            {
                return await _nodeService.RemovePendingShipment(dto);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<object> CancelShipmentWithNoChargeAndReason(CancelShipmentDTO shipment)
        {
            try
            {
                var preshipmentmobile = await _uow.PreShipmentMobile.GetAsync(s => s.Waybill == shipment.Waybill);
                if (preshipmentmobile == null)
                {
                    throw new GenericException("Shipment cannot be found", $"{(int)HttpStatusCode.NotFound}");
                }
                //Get user
                var userID = await _userService.GetCurrentUserId();
                var user = await _uow.User.GetUserById(userID);

                //FOR PARTNER TRYING TO CANCEL  A SHIPMENT
                if (user.UserChannelType.ToString() == UserChannelType.Partner.ToString())
                {
                    var pickuprequests = await _uow.MobilePickUpRequests.GetAsync(s => s.Waybill == preshipmentmobile.Waybill && s.UserId == userID);
                    if (pickuprequests != null)
                    {
                        pickuprequests.Status = MobilePickUpRequestStatus.Cancelled.ToString();
                    }

                    preshipmentmobile.shipmentstatus = MobilePickUpRequestStatus.Processing.ToString();
                }

                else
                {
                    if (preshipmentmobile.shipmentstatus == "Shipment created" || preshipmentmobile.shipmentstatus == MobilePickUpRequestStatus.Rejected.ToString() || preshipmentmobile.shipmentstatus == MobilePickUpRequestStatus.TimedOut.ToString() || preshipmentmobile.shipmentstatus == MobilePickUpRequestStatus.PendingCancellation.ToString() || preshipmentmobile.shipmentstatus.ToLower() == "pending cancellation")
                    {
                        // check the invoice table to be sure money was collected for the shipment
                        var walletTransaction = await _uow.WalletTransaction.GetAsync(s => s.Waybill == shipment.Waybill);
                        preshipmentmobile.shipmentstatus = MobilePickUpRequestStatus.Cancelled.ToString();
                        if (walletTransaction != null)
                        {
                            await UpdateCustomerWalletForCancelledShipment(preshipmentmobile.CustomerCode, preshipmentmobile.Waybill, preshipmentmobile.GrandTotal);

                        }
                        await ScanMobileShipment(new ScanDTO
                        {
                            WaybillNumber = shipment.Waybill,
                            ShipmentScanStatus = ShipmentScanStatus.MSCC
                        });

                        //remove from report
                        var report = await _uow.FinancialReport.GetAsync(s => s.Waybill == shipment.Waybill);
                        if (report != null)
                        {
                            report.IsDeleted = true;
                        }
                    }
                    else
                    {
                        throw new GenericException($"Shipment cannot be cancelled because it has a current status of {preshipmentmobile.shipmentstatus}.", $"{(int)HttpStatusCode.Forbidden}");
                    }
                }
                await _uow.CompleteAsync();
                return new { IsCancelled = true };
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<object> CancelShipmentWithReason(CancelShipmentDTO cancelPreShipmentMobile)
        {
            try
            {
                var preshipmentmobile = await _uow.PreShipmentMobile.GetAsync(s => s.Waybill == cancelPreShipmentMobile.Waybill);
                var report = await _uow.FinancialReport.GetAsync(s => s.Waybill == preshipmentmobile.Waybill);
                if (preshipmentmobile == null)
                {
                    throw new GenericException("Shipment does not exist", $"{(int)HttpStatusCode.NotFound}");
                }
                preshipmentmobile.CustomerCancelReason = cancelPreShipmentMobile.CancelReason;
                var pickuprequests = await _uow.MobilePickUpRequests.GetAsync(s => s.Waybill == preshipmentmobile.Waybill && (s.Status != MobilePickUpRequestStatus.Rejected.ToString() || s.Status != MobilePickUpRequestStatus.TimedOut.ToString()));
                if (pickuprequests != null)
                {
                    var pickuprice = await GetPickUpPrice(preshipmentmobile.VehicleType, preshipmentmobile.CountryId, preshipmentmobile.UserId = null);
                    var partnersPrice = (0.4M * Convert.ToDecimal(pickuprice));

                    var rider = await _userService.GetUserById(pickuprequests.UserId);
                    var riderWallet = await _uow.Wallet.GetAsync(s => s.CustomerCode == rider.UserChannelCode);
                    riderWallet.Balance = riderWallet.Balance + partnersPrice;

                    preshipmentmobile.shipmentstatus = MobilePickUpRequestStatus.Cancelled.ToString();
                    pickuprequests.Status = MobilePickUpRequestStatus.Cancelled.ToString();

                    //update wallet transaction for customer 
                    decimal amount = preshipmentmobile.GrandTotal - Convert.ToDecimal(pickuprice);
                    await UpdateCustomerWalletForCancelledShipment(preshipmentmobile.CustomerCode, preshipmentmobile.Waybill, amount);

                    var partnertransactions = new PartnerTransactionsDTO
                    {
                        Destination = preshipmentmobile.ReceiverAddress,
                        Departure = preshipmentmobile.SenderAddress,
                        AmountReceived = partnersPrice,
                        Waybill = preshipmentmobile.Waybill
                    };
                    await _partnertransactionservice.AddPartnerPaymentLog(partnertransactions);

                    //update financial report 
                    if (report != null)
                    {
                        report.PartnerEarnings = partnersPrice;
                        report.Earnings = 0.00M;
                        report.GrandTotal = 0.00M;
                    }
                }
                else
                {
                    preshipmentmobile.shipmentstatus = MobilePickUpRequestStatus.Cancelled.ToString();
                    await UpdateCustomerWalletForCancelledShipment(preshipmentmobile.CustomerCode, preshipmentmobile.Waybill, preshipmentmobile.GrandTotal);

                    //remove from report
                    if (report != null)
                    {
                        report.IsDeleted = true;
                    }
                }

                //also cancel shipment if its on shipment table already
                var shipment = await _uow.Shipment.GetAsync(s => s.Waybill == cancelPreShipmentMobile.Waybill);
                if (shipment != null)
                {
                    var user = await _userService.GetCurrentUserId();
                    var res = await _shipmentService.CancelShipmentForGIGGOExtension(cancelPreShipmentMobile);
                }
                await _uow.CompleteAsync();
                return new { IsCancelled = true };
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<MobilePriceDTO> GetPriceQuote(PreShipmentMobileDTO preShipment)
        {
            try
            {
                if (preShipment == null)
                {
                    throw new GenericException("Invalid Data");
                }

                if (!preShipment.PreShipmentItems.Any())
                {
                    throw new GenericException("Preshipment Item Not Found");
                }
                var zoneid = await _domesticroutezonemapservice.GetZoneMobile(preShipment.SenderStationId, preShipment.ReceiverStationId);
                var country = await _uow.Country.GetCountryByStationId(preShipment.SenderStationId);
                if (country == null)
                {
                    throw new GenericException("Sender Station Country Not Found", $"{(int)HttpStatusCode.NotFound}");
                }
                preShipment.CountryId = country.CountryId;

                //change the quantity of the preshipmentItem if it fall into promo category
                preShipment = await ChangePreshipmentItemQuantity(preShipment, zoneid.ZoneId);

                var Price = 0.0M;
                var amount = 0.0M;
                var IndividualPrice = 0.0M;
                decimal DeclaredValue = 0.0M;

                if (zoneid.ZoneId == 1 && preShipment.ReceiverLocation != null && preShipment.SenderLocation != null)
                {
                    if (preShipment.ReceiverLocation.Latitude != null && preShipment.SenderLocation.Latitude != null)
                    {
                        int ShipmentCount = preShipment.PreShipmentItems.Count;

                        amount = await CalculateGeoDetailsBasedonLocation(preShipment);
                        IndividualPrice = (amount / ShipmentCount);
                    }
                }

                //Get the vat value from VAT
                var vatDTO = await _uow.VAT.GetAsync(x => x.CountryId == preShipment.CountryId);
                decimal vat = (vatDTO != null) ? (vatDTO.Value / 100) : (7.5M / 100);

                foreach (var preShipmentItem in preShipment.PreShipmentItems)
                {
                    if (preShipmentItem.Quantity == 0)
                    {
                        throw new GenericException("Item Quantity cannot be zero");
                    }

                    if (preShipmentItem.SpecialPackageId == null)
                    {
                        preShipmentItem.SpecialPackageId = 0;
                    }

                    var PriceDTO = new PricingDTO
                    {
                        DepartureStationId = preShipment.SenderStationId,
                        DestinationStationId = preShipment.ReceiverStationId,
                        Weight = preShipmentItem.Weight,
                        SpecialPackageId = (int)preShipmentItem.SpecialPackageId,
                        ShipmentType = preShipmentItem.ShipmentType,
                        CountryId = preShipment.CountryId,
                    };

                    if (preShipmentItem.ShipmentType == ShipmentType.Special)
                    {
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

                        preShipmentItem.CalculatedPrice = await _pricingService.GetMobileRegularPrice(PriceDTO);
                        preShipmentItem.CalculatedPrice = preShipmentItem.CalculatedPrice * preShipmentItem.Quantity;
                        preShipmentItem.CalculatedPrice = preShipmentItem.CalculatedPrice + IndividualPrice;
                    }

                    var vatForPreshipment = (preShipmentItem.CalculatedPrice * vat);

                    if (!string.IsNullOrWhiteSpace(preShipmentItem.Value))
                    {
                        DeclaredValue += Convert.ToDecimal(preShipmentItem.Value);
                        var DeclaredValueForPreShipment = Convert.ToDecimal(preShipmentItem.Value);
                        preShipmentItem.CalculatedPrice = preShipmentItem.CalculatedPrice + (DeclaredValueForPreShipment * 0.01M) + vatForPreshipment;
                        preShipment.IsdeclaredVal = true;
                    }
                    else
                    {
                        preShipmentItem.CalculatedPrice = preShipmentItem.CalculatedPrice + vatForPreshipment;
                    }

                    preShipmentItem.CalculatedPrice = (decimal)Math.Round((double)preShipmentItem.CalculatedPrice);
                    Price += (decimal)preShipmentItem.CalculatedPrice;
                };

                decimal percentage = 0.00M;
                if (!string.IsNullOrWhiteSpace(preShipment.VehicleType))
                {
                    if (preShipment.VehicleType.ToLower() == Vehicletype.Truck.ToString().ToLower())
                    {
                        percentage = 0.00M;
                    }
                    else
                    {
                        var discountPercent = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.DiscountPercentage, preShipment.CountryId);
                        percentage = Convert.ToDecimal(discountPercent.Value);
                    }
                }

                var percentageTobeUsed = ((100M - percentage) / 100M);
                decimal estimatedDeclaredPrice = preShipment.IsFromAgility ? Convert.ToDecimal(preShipment.Value) : Convert.ToDecimal(DeclaredValue);
                preShipment.DeliveryPrice = Price * percentageTobeUsed;
                preShipment.InsuranceValue = (estimatedDeclaredPrice * 0.01M);
                preShipment.CalculatedTotal = (double)(Price);
                preShipment.CalculatedTotal = Math.Round((double)preShipment.CalculatedTotal);
                preShipment.Value = DeclaredValue;
                var discount = Math.Round(Price - (decimal)preShipment.DeliveryPrice);
                preShipment.DiscountValue = discount;

                var Pickuprice = await GetPickUpPrice(preShipment.VehicleType, preShipment.CountryId, preShipment.UserId);
                var PickupValue = Convert.ToDecimal(Pickuprice);
                var IsWithinProcessingTime = await WithinProcessingTime(preShipment.CountryId);
                decimal grandTotal = (decimal)preShipment.DeliveryPrice + PickupValue;

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
                    DeliveryPrice = preShipment.DeliveryPrice,
                    PickUpCharge = PickupValue,
                    InsuranceValue = preShipment.InsuranceValue,
                    GrandTotal = grandTotal,
                    PreshipmentMobile = preShipment,
                    CurrencySymbol = country.CurrencySymbol,
                    CurrencyCode = country.CurrencyCode,
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

        private async Task<decimal> ComputeCouponAmount(string couponCode, decimal amount)
        {
            decimal computedAmount = 0;
            var coupon = _uow.CouponManagement.SingleOrDefault(x => x.CouponCode == couponCode && x.IsCouponCodeUsed == false);
            if (coupon == null)
            {
                throw new GenericException($"Coupon code {couponCode} does not exists or has been used", $"{(int)HttpStatusCode.BadRequest}");
            }
            if (DateTime.Now.Date > coupon.ExpiryDay.Date)
            {
                throw new GenericException($"The coupon code has expired", $"{(int)HttpStatusCode.BadRequest}");
            }
            if (coupon.DiscountType == CouponDiscountType.Percentage)
            {
                var couponPer = (Convert.ToDecimal(coupon.CouponCodeValue) / 100) * amount;
                computedAmount = amount - couponPer;
            }
            if (coupon.DiscountType == CouponDiscountType.Flat)
            {
                computedAmount = amount - Convert.ToDecimal(coupon.CouponCodeValue);
            }
            if (computedAmount < 0)
            {
                return 0;
            }
            return computedAmount;
        }

        private async Task<ShipmentDTO> MapIntlShipmentToShipment(ApproveShipmentDTO detail, PreShipmentMobile mobile,
        CustomerDTO customer, Core.DTO.DHL.TotalNetResult totalNet, string intlWaybill, string pdfFormat, decimal declarationOfValueCheck)
        {
            try
            {
                // Saving to azure blob
                byte[] sPDFDecoded = Convert.FromBase64String(pdfFormat);
                var filename = $"{mobile.Waybill}-{mobile.CompanyMap.ToString()}.pdf";
                var blobname = await AzureBlobServiceUtil.UploadAsync(sPDFDecoded, filename);
                var currentUserId = await _userService.GetCurrentUserId();
                var destination = await _userService.GetInternationalOutBoundServiceCentre();
                var userServiceCenters = await _userService.GetPriviledgeServiceCenters();
                var shipment = Mapper.Map<ShipmentDTO>(mobile);
                //default sc
                if (userServiceCenters.Any())
                {
                    shipment.DepartureServiceCentreId = userServiceCenters[0];
                }
                shipment.Waybill = mobile.Waybill;
                shipment.DestinationServiceCentreId = destination.ServiceCentreId;
                shipment.PickupOptions = mobile.IsHomeDelivery == true ? PickupOptions.HOMEDELIVERY : PickupOptions.SERVICECENTER;
                shipment.IsClassShipment = customer.Rank == Rank.Class ? true : false;
                shipment.CustomerId = customer.CompanyId == 0 ? customer.IndividualCustomerId : customer.CompanyId;
                shipment.IsCashOnDelivery = false;
                shipment.UserId = currentUserId;
                shipment.DeliveryOptionId = 6;
                shipment.Insurance = totalNet.Insurance;
                shipment.Vat = totalNet.VAT;
                shipment.vatvalue_display = totalNet.VAT;
                shipment.GrandTotal = totalNet.GrandTotal;
                shipment.Total = totalNet.Amount;
                shipment.ApproximateItemsWeight = 0.00;
                shipment.PaymentStatus = PaymentStatus.Paid;
                shipment.IsFromMobile = true;
                shipment.ShipmentPickupPrice = 0;
                shipment.FileNameUrl = blobname;
                shipment.InternationalWayBill = intlWaybill;
                shipment.PackageOptionIds = detail.PackageOptionIds;
                shipment.DeclarationOfValueCheck = declarationOfValueCheck;
                shipment.InternationalShippingCost = totalNet.InternationalShippingCost;
                shipment.Courier = mobile.CompanyMap.ToString();
                shipment.IsBulky = detail.IsBulky;
                shipment.ExpressDelivery = detail.ExpressDelivery;
                shipment.IsCashOnDelivery = mobile.IsCashOnDelivery;
                shipment.CODStatusDate = mobile.CODStatusDate;
                shipment.CODDescription = mobile.CODDescription;
                shipment.CODStatus = mobile.CODStatus;
                shipment.ShipmentItems = mobile.PreShipmentItems.Select(s => new ShipmentItemDTO
                {
                    Description = s.Description,
                    IsVolumetric = s.IsVolumetric,
                    Weight = s.Weight,
                    Nature = s.ItemType,
                    Price = Convert.ToDecimal(s.Value),
                    Quantity = s.Quantity,
                    ShipmentType = ShipmentType.Regular
                }).ToList();
                return shipment;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IEnumerable<CancelledShipmentDTO>> GetCanceledShipment(DateFilterCriteria dateFilterCriteria)
        {
            if (dateFilterCriteria == null)
            {
                dateFilterCriteria = new DateFilterCriteria
                {
                    StartDate = null,
                    EndDate = null
                };
            }
            var queryDate = dateFilterCriteria.getStartDateAndEndDate();
            var startDate = queryDate.Item1;
            var endDate = queryDate.Item2;

            var canceledShipment = await _uow.PreShipmentMobile
                  .FindAsync(x => x.shipmentstatus == MobilePickUpRequestStatus.Cancelled.ToString()
                  && x.DateCreated >= startDate && x.DateCreated < endDate);
            return Mapper.Map<IEnumerable<CancelledShipmentDTO>>(canceledShipment);
        }

        public async Task<IEnumerable<CancelledShipmentDTO>> GetCanceledShipment(string waybill)
        {
            var canceledShipment = await _uow.PreShipmentMobile
                  .FindAsync(x => x.shipmentstatus == MobilePickUpRequestStatus.Cancelled.ToString()
                   && x.Waybill == waybill);
            return Mapper.Map<IEnumerable<CancelledShipmentDTO>>(canceledShipment);
        }

        public async Task<PreShipmentMobileDTO> GetPreShipmentMobileReceiverAndItemDetails(NewFilterOptionsDto filter)
        {
            if (filter == null)
            {
                throw new GenericException("Invalid payload", $"{(int)HttpStatusCode.BadRequest}");
            }
            var shipment = await _uow.PreShipmentMobile.GetPreshipmentMobileByWaybill(filter.FilterType);
            return shipment;
        }

        private async Task<SpecialDiscountPriceDTO> CalculateSpecialDiscount(PreShipmentMobileDTO preShipment, decimal grandTotal, decimal pickupValue)
        {
            var result = new SpecialDiscountPriceDTO { DiscountValue = 0.00m, GrandTotal = 0.00m };
            // Special Discount Price

            var specialStationDiscount = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.SpecialStationDiscount, preShipment.CountryId);
            var specialPercentage = Convert.ToDecimal(specialStationDiscount.Value);

            var specialPercentageTobeUsed = ((100M - specialPercentage) / 100M);
            var newCalculatedTotal = (double)(grandTotal * specialPercentageTobeUsed);
            newCalculatedTotal = Math.Round(newCalculatedTotal);

            result.DiscountValue = (decimal)preShipment.DeliveryPrice + pickupValue - (decimal)newCalculatedTotal;
            result.GrandTotal = (decimal)newCalculatedTotal;

            return result;
        }

        private async Task<bool> CheckEligibleStation(int CountryId, int SenderStationId)
        {
            bool result = false;
            if (CountryId == 0 || SenderStationId == 0)
            {
                return result;
            }

            var specialStation = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.SpecialStation, CountryId);
            var specialStationIds = specialStation.Value.Split(',');

            if (specialStationIds.Length > 0 && specialStationIds.Contains(SenderStationId.ToString()))
            {
                result = true;
            }

            return result;
        }

        private async Task UpdateAlphaOrderStatus(string waybill, string status)
        {
            var notifyAlphaPayload = new AlphaUpdateOrderStatusDTO { Status = status, WayBill = waybill };
            await _alphaService.UpdateOrderStatus(notifyAlphaPayload);
        }


        public async Task<object> AddMultiplePreShipmentMobile(PreShipmentMobileMultiMerchantDTO preShipments)
        {
            try
            {
                var shipments = new List<PreShipmentMobile>();
                var waybills = new List<Dictionary<string, string>>();
                bool IsBalanceSufficient = true;
                bool IsEligible = true;
                decimal grandTotal = 0;
                string message = "shipment created successfully";
                preShipments.DateCreated = DateTime.Now;
                if (!preShipments.Merchants.Any())
                {
                    message = "invalid payload; merchant not provided";
                    IsBalanceSufficient = false;
                    throw new GenericException(message, $"{(int)HttpStatusCode.BadRequest}");
                }
                var receiverExist = await _uow.User.GetUserByPhoneNumber(preShipments.ReceiverPhoneNumber);
                if (receiverExist == null && preShipments.PaymentType == PaymentType.Wallet)
                {
                    message = "customer does not exist";
                    IsBalanceSufficient = false;
                    throw new GenericException(message, $"{(int)HttpStatusCode.BadRequest}");
                }

                var currentUserId = await _userService.GetCurrentUserId();
                var user = await _userService.GetUserById(currentUserId);
                var company = await _uow.Company.GetAsync(x => x.CustomerCode == user.UserChannelCode);
                if (company is null)
                {
                    message = "user is not an ecommerce user";
                    IsBalanceSufficient = false;
                    throw new GenericException(message, $"{(int)HttpStatusCode.BadRequest}");
                }

                if (company.CompanyType != CompanyType.Corporate || (company.CompanyType != CompanyType.Ecommerce && company.Rank != Rank.Class))
                {
                    if (preShipments.IsCashOnDelivery)
                    {
                        message = "this feature is not available for Basic ecommerce customers";
                        IsBalanceSufficient = false;
                        throw new GenericException(message, $"{(int)HttpStatusCode.BadRequest}");
                    }
                }
                foreach (var preShipment in preShipments.Merchants)
                {
                    if (String.IsNullOrEmpty(preShipment.CustomerCode))
                    {
                        message = "merchant does not exist";
                        IsBalanceSufficient = false;
                        throw new GenericException(message, $"{(int)HttpStatusCode.BadRequest}");
                    }
                    var merchant = await _uow.User.GetUserByChannelCode(preShipment.CustomerCode);
                    var zoneid = await _domesticroutezonemapservice.GetZoneMobile(preShipment.SenderStationId, preShipments.ReceiverStationId);
                    preShipment.ZoneMapping = zoneid.ZoneId;
                    preShipments.ZoneMapping = zoneid.ZoneId;
                    var mobileShipment = JObject.FromObject(preShipment).ToObject<PreShipmentMobileDTO>();
                    mobileShipment.VehicleType = preShipments.VehicleType;
                    mobileShipment.PaymentType = preShipments.PaymentType;
                    mobileShipment.ZoneMapping = preShipment.ZoneMapping;

                    //set all merchant info
                    if (merchant != null)
                    {
                        mobileShipment.UserId = merchant.Id;
                        mobileShipment.ZoneMapping = preShipment.ZoneMapping;
                        mobileShipment.CustomerCode = user.UserChannelCode;
                        mobileShipment.SenderLocation = preShipment.SenderLocation;
                    }

                    //set receiver info
                    mobileShipment.ReceiverAddress = preShipments.ReceiverAddress;
                    mobileShipment.ReceiverPhoneNumber = preShipments.ReceiverPhoneNumber;
                    mobileShipment.ReceiverStationId = preShipments.ReceiverStationId;
                    mobileShipment.ReceiverLocation = preShipments.ReceiverLocation;
                    mobileShipment.ReceiverName = preShipments.ReceiverName;
                    mobileShipment.ReceiverEmail = preShipments.ReceiverEmail;
                    mobileShipment.ZoneMapping = zoneid.ZoneId;
                    mobileShipment.IsCashOnDelivery = preShipments.IsCashOnDelivery;
                    mobileShipment.CashOnDeliveryAmount = preShipments.CashOnDeliveryAmount;
                    if (receiverExist == null)
                    {
                        var receiver = await _uow.IndividualCustomer.GetAsync(x => x.PhoneNumber.Contains(preShipments.ReceiverPhoneNumber));
                        if (receiver == null)
                        {
                            //create this user first
                            var newCustomer = new CustomerDTO()
                            {
                                PhoneNumber = preShipments.ReceiverPhoneNumber,
                                FirstName = preShipments.ReceiverName,
                                LastName = preShipments.ReceiverName,
                                UserActiveCountryId = 1,
                                City = preShipment.SenderLocality,
                                CustomerType = CustomerType.IndividualCustomer,
                                Address = preShipments.ReceiverAddress,
                                ReturnAddress = preShipments.ReceiverAddress,
                                Gender = Gender.Male,
                                CustomerCode = await _numberGeneratorMonitorService.GenerateNextNumber(NumberGeneratorType.CustomerCodeIndividual)
                            };
                            var newIndCustomer = Mapper.Map<IndividualCustomer>(newCustomer);
                            _uow.IndividualCustomer.Add(newIndCustomer);
                            await _uow.CompleteAsync();
                        }
                        else if (receiver != null)
                        {
                            mobileShipment.ReceiverCode = receiver.CustomerCode;
                            mobileShipment.ReceiverEmail = receiver.Email;
                        }
                    }
                    else
                    {
                        mobileShipment.ReceiverCode = receiverExist.UserChannelCode;
                        mobileShipment.ReceiverEmail = receiverExist.Email;
                    }
                    mobileShipment.IsAlpha = true;
                    var newPreShipmentDTO = await CreateMultiplePreShipmentMobile(mobileShipment);
                    IsBalanceSufficient = newPreShipmentDTO.IsBalanceSufficient.Value;
                    IsEligible = false;
                    if (!newPreShipmentDTO.IsBalanceSufficient.Value)
                    {
                        message = "Insufficient Wallet Balance";
                        IsBalanceSufficient = false;
                        throw new GenericException(message, $"{(int)HttpStatusCode.Forbidden}");
                    }
                    if (merchant.UserChannelType == UserChannelType.Ecommerce || merchant.UserChannelType == UserChannelType.Corporate)
                    {
                        IsEligible = newPreShipmentDTO.IsEligible.Value;
                        if (!newPreShipmentDTO.IsEligible.Value)
                        {
                            var carPickUprice = new GlobalPropertyDTO();
                            if (newPreShipmentDTO.IsCodNeeded == false)
                            {
                                carPickUprice = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.EcommerceNoCodAmount, newPreShipmentDTO.CountryId);
                            }
                            else
                            {
                                carPickUprice = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.EcommerceCodAmount, newPreShipmentDTO.CountryId);
                            }
                            var remainingamount = (Convert.ToDecimal(carPickUprice.Value) - newPreShipmentDTO.CurrentWalletAmount);
                            message = $"You are not yet eligible to create shipment on the Platform. You need a minimum balance of {newPreShipmentDTO.CurrencySymbol}{carPickUprice.Value}, please fund your wallet with additional {newPreShipmentDTO.CurrencySymbol}{remainingamount} to complete the process. Thank you";
                            newPreShipmentDTO.Waybill = "";

                            throw new GenericException(message, $"{(int)HttpStatusCode.Forbidden}");
                        }
                    }
                    if (newPreShipmentDTO.IsBalanceSufficient.Value)
                    {
                        var newPreShipment = Mapper.Map<PreShipmentMobile>(newPreShipmentDTO);
                        var waybill = new Dictionary<string, string>();
                        waybill.Add(newPreShipment.Waybill, newPreShipment.CustomerCode);
                        waybills.Add(waybill);
                        newPreShipment.IsAlpha = true;
                        shipments.Add(newPreShipment);
                    }
                }

                if (shipments.Any())
                {
                    grandTotal = shipments.Sum(x => x.GrandTotal);
                    _uow.PreShipmentMobile.AddRange(shipments);
                    await _uow.CompleteAsync();

                    foreach (var item in shipments)
                    {
                        await ScanMobileShipment(new ScanDTO
                        {
                            WaybillNumber = item.Waybill,
                            ShipmentScanStatus = ShipmentScanStatus.MCRT
                        });

                        var messageDto = new MobileShipmentCreationMessageDTO
                        {
                            SenderPhoneNumber = item.SenderPhoneNumber,
                            WaybillNumber = item.Waybill,
                            SenderName = item.SenderName
                        };


                        if (item.IsCashOnDelivery)
                        {

                            //collect the cods and add to CashOnDeliveryRegisterAccount()
                            var cashondeliveryentity = new CashOnDeliveryRegisterAccount
                            {
                                Amount = item.CashOnDeliveryAmount ?? 0,
                                CODStatusHistory = CODStatushistory.Created,
                                Description = "Cod From Sales",
                                ServiceCenterId = 0,
                                Waybill = item.Waybill,
                                UserId = item.UserId,
                                DepartureServiceCenterId = 0,
                                DestinationCountryId = item.CountryId
                            };
                            _uow.CashOnDeliveryRegisterAccount.Add(cashondeliveryentity);


                            item.CODDescription = "COD Initiated";
                            item.CODStatus = CODMobileStatus.Initiated;
                            item.CODStatusDate = DateTime.Now;
                        }


                        //Pin Generation 
                        var number = await GenerateDeliveryCode();
                        var deliveryNumber = new DeliveryNumber
                        {
                            SenderCode = number,
                            IsUsed = false,
                            Waybill = item.Waybill
                        };
                        _uow.DeliveryNumber.Add(deliveryNumber);

                        messageDto.QRCode = deliveryNumber.SenderCode;

                        await _uow.CompleteAsync();
                        
                        //We will send SMS & Email
                        await SendSMSForMobileShipmentCreation(messageDto, MessageType.MCS);
                    }

                    foreach (var item in shipments)
                    {
                        //drop shipment on node
                        NodeApiCreateShipment(item);

                        //also send sms
                    }
                }

                return new { waybill = waybills, message = message, IsBalanceSufficient, Zone = preShipments.ZoneMapping, GrandTotal = grandTotal };
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private async Task<PreShipmentMobileDTO> CreateMultiplePreShipmentMobile(PreShipmentMobileDTO preShipmentDTO)
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

                if (!String.IsNullOrEmpty(preShipmentDTO.ReceiverPhoneNumber))
                {
                    preShipmentDTO.ReceiverPhoneNumber = preShipmentDTO.ReceiverPhoneNumber.Trim();
                }

                if (!String.IsNullOrEmpty(preShipmentDTO.SenderPhoneNumber))
                {
                    preShipmentDTO.SenderPhoneNumber = preShipmentDTO.SenderPhoneNumber.Trim();
                }

                var PreshipmentPriceDTO = new MobilePriceDTO();
                bool IsBalanceSufficient = true;

                var sender = await _userService.GetUserById(preShipmentDTO.UserId);
                preShipmentDTO.CustomerCode = sender.UserChannelCode;

                var country = await _uow.Country.GetCountryByStationId(preShipmentDTO.SenderStationId);
                if (country == null)
                {
                    throw new GenericException("Sender Station Country Not Found", $"{(int)HttpStatusCode.NotFound}");
                }
                preShipmentDTO.CountryId = country.CountryId;

                var customer = await _uow.Company.GetAsync(s => s.CustomerCode == sender.UserChannelCode);
                if (customer != null)
                {
                    preShipmentDTO.IsEligible = customer.IsEligible;
                    //If the account is not active, block the customer from creating shipment
                    if (customer.CompanyStatus != CompanyStatus.Active)
                    {
                        throw new GenericException($"Your account has been {customer.CompanyStatus}, contact support for assistance", $"{(int)HttpStatusCode.Forbidden}");
                    }

                    if (customer.IsEligible != true)
                    {
                        preShipmentDTO.IsEligible = false;
                        preShipmentDTO.IsCodNeeded = customer.isCodNeeded;
                        preShipmentDTO.CurrencySymbol = country.CurrencySymbol;
                        preShipmentDTO.CurrentWalletAmount = customer.WalletAmount != null ? (decimal)customer.WalletAmount : 0;
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
                else if (preShipmentDTO.VehicleType.ToLower() == Vehicletype.Bike.ToString().ToLower() && preShipmentDTO.ZoneMapping == 1)
                {
                    PreshipmentPriceDTO = await GetPriceForBike(preShipmentDTO);
                }
                else
                {
                    if (sender.UserChannelType == UserChannelType.Ecommerce || customer != null)
                    {
                        preShipmentDTO.Shipmentype = ShipmentType.Ecommerce;
                    }
                    preShipmentDTO.IsFromShipment = true;
                    PreshipmentPriceDTO = await GetPrice(preShipmentDTO);
                }

                // update and apply coupon 
                if (preShipmentDTO.IsCoupon)
                {
                    PreshipmentPriceDTO.GrandTotal = await ComputeCouponAmount(preShipmentDTO.CouponCode, PreshipmentPriceDTO.GrandTotal.Value);
                    var coupon = _uow.CouponManagement.SingleOrDefault(x => x.CouponCode == preShipmentDTO.CouponCode);
                    coupon.IsCouponCodeUsed = true;
                }

                decimal shipmentGrandTotal = (decimal)PreshipmentPriceDTO.GrandTotal;
                decimal actualAmountToDebit = shipmentGrandTotal;
                //get departure country
                if (country != null)
                {
                    if (sender.UserActiveCountryId != country.CountryId)
                    {
                        //get actual amount to debit
                        var countryRateConversion = await _uow.CountryRouteZoneMap.GetAsync(r =>
                        r.DepartureId == country.CountryId &&
                        r.DestinationId == sender.UserActiveCountryId, "Zone,Destination,Departure");
                        if (countryRateConversion == null)
                            throw new GenericException("The Mapping of Route to Zone does not exist");

                        double amountToDebitDouble = countryRateConversion.Rate * (double)shipmentGrandTotal;
                        actualAmountToDebit = (decimal)Math.Round(amountToDebitDouble, 2);
                    }
                }
                var gigGOServiceCenter = await _userService.GetGIGGOServiceCentre();

                //generate waybill
                var waybill = await _numberGeneratorMonitorService.GenerateNextNumber(NumberGeneratorType.WaybillNumber, gigGOServiceCenter.Code);
                preShipmentDTO.Waybill = waybill;
                preShipmentDTO.Waybill = $"APH-{preShipmentDTO.Waybill}";

                var newPreShipment = Mapper.Map<PreShipmentMobile>(preShipmentDTO);

                //if (sender.UserChannelType == UserChannelType.Ecommerce)
                //{
                //    newPreShipment.CustomerType = CustomerType.Company.ToString();
                //    newPreShipment.CompanyType = CompanyType.Ecommerce.ToString();
                //}

                //else
                //{
                //    newPreShipment.CustomerType = "Individual";
                //    newPreShipment.CompanyType = CustomerType.IndividualCustomer.ToString();
                //}
                newPreShipment.CustomerType = CustomerType.Company.ToString();
                newPreShipment.CompanyType = CompanyType.Ecommerce.ToString();
                newPreShipment.UserId = preShipmentDTO.UserId;
                newPreShipment.IsConfirmed = false;
                newPreShipment.IsDelivered = false;
                newPreShipment.shipmentstatus = "Shipment created";
                newPreShipment.DateCreated = DateTime.Now;
                newPreShipment.GrandTotal = shipmentGrandTotal;
                newPreShipment.CustomerCode = sender.UserChannelCode;
                preShipmentDTO.IsBalanceSufficient = true;
                preShipmentDTO.DiscountValue = PreshipmentPriceDTO.Discount;
                newPreShipment.ShipmentPickupPrice = (decimal)(PreshipmentPriceDTO.PickUpCharge == null ? 0.0M : PreshipmentPriceDTO.PickUpCharge);
                var newPreShipmentDTO = Mapper.Map<PreShipmentMobileDTO>(newPreShipment);
                newPreShipmentDTO.IsBalanceSufficient = preShipmentDTO.IsBalanceSufficient;
                newPreShipmentDTO.IsEligible = preShipmentDTO.IsEligible;
                //process payment
                if (preShipmentDTO.PaymentType == PaymentType.Wallet)
                {
                    var currentUserId = await _userService.GetCurrentUserId();
                    var user = await _userService.GetUserById(currentUserId);
                    var wallet = await _uow.Wallet.GetAsync(x => x.CustomerCode == user.UserChannelCode);
                    if (wallet.Balance >= actualAmountToDebit)
                    {
                        var transaction = new WalletTransactionDTO
                        {
                            WalletId = wallet.WalletId,
                            CreditDebitType = CreditDebitType.Debit,
                            Amount = newPreShipment.GrandTotal,
                            ServiceCentreId = gigGOServiceCenter.ServiceCentreId,
                            Waybill = waybill,
                            Description = "Payment for Shipment",
                            PaymentType = PaymentType.Wallet,
                            UserId = user.Id,
                            TransactionCountryId = user.UserActiveCountryId
                        };
                        if (transaction.CreditDebitType == CreditDebitType.Credit)
                        {
                            transaction.BalanceAfterTransaction = wallet.Balance + actualAmountToDebit;
                        }
                        else
                        {
                            transaction.BalanceAfterTransaction = wallet.Balance - actualAmountToDebit;
                        }

                        //update wallet
                        var updatedwallet = await _uow.Wallet.GetAsync(wallet.WalletId);
                        //double check in case something is wrong with the server before complete the transaction
                        if (updatedwallet.Balance < actualAmountToDebit)
                        {
                            preShipmentDTO.IsBalanceSufficient = false;
                            IsBalanceSufficient = false;
                            newPreShipmentDTO.IsBalanceSufficient = IsBalanceSufficient;
                            return newPreShipmentDTO;
                        }
                        decimal price = updatedwallet.Balance - actualAmountToDebit;
                        updatedwallet.Balance = price;
                        var walletTransaction = await _walletTransactionService.AddWalletTransaction(transaction);
                    }
                    else
                    {
                        preShipmentDTO.IsBalanceSufficient = false;
                        IsBalanceSufficient = false;
                        return preShipmentDTO;
                    }
                }
                newPreShipmentDTO = Mapper.Map<PreShipmentMobileDTO>(newPreShipment);
                newPreShipmentDTO.IsBalanceSufficient = IsBalanceSufficient;
                newPreShipmentDTO.IsEligible = preShipmentDTO.IsEligible;
                return newPreShipmentDTO;
            }
            catch
            {
                throw;
            }
        }

    }
}


