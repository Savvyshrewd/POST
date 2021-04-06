﻿using AutoMapper;
using GIGL.GIGLS.Core.Domain;
using GIGLS.Core;
using GIGLS.Core.Domain;
using GIGLS.Core.Domain.DHL;
using GIGLS.Core.Domain.Wallet;
using GIGLS.Core.DTO;
using GIGLS.Core.DTO.Account;
using GIGLS.Core.DTO.Customers;
using GIGLS.Core.DTO.DHL;
using GIGLS.Core.DTO.PaymentTransactions;
using GIGLS.Core.DTO.Report;
using GIGLS.Core.DTO.ServiceCentres;
using GIGLS.Core.DTO.Shipments;
using GIGLS.Core.DTO.User;
using GIGLS.Core.DTO.Wallet;
using GIGLS.Core.DTO.Zone;
using GIGLS.Core.Enums;
using GIGLS.Core.IMessageService;
using GIGLS.Core.IServices.Business;
using GIGLS.Core.IServices.Customers;
using GIGLS.Core.IServices.DHL;
using GIGLS.Core.IServices.Node;
using GIGLS.Core.IServices.ServiceCentres;
using GIGLS.Core.IServices.Shipments;
using GIGLS.Core.IServices.User;
using GIGLS.Core.IServices.Utility;
using GIGLS.Core.IServices.Wallet;
using GIGLS.Core.IServices.Zone;
using GIGLS.Core.View;
using GIGLS.CORE.DTO.Report;
using GIGLS.CORE.DTO.Shipments;
using GIGLS.Infrastructure;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GIGLS.Services.Implementation.Shipments
{
    public class ShipmentService : IShipmentService
    {
        private readonly IUnitOfWork _uow;
        private readonly IDeliveryOptionService _deliveryService;
        private readonly IServiceCentreService _centreService;
        private readonly IUserServiceCentreMappingService _userServiceCentre;
        private readonly INumberGeneratorMonitorService _numberGeneratorMonitorService;
        private readonly ICustomerService _customerService;
        private readonly IUserService _userService;
        private readonly IMessageSenderService _messageSenderService;
        private readonly ICompanyService _companyService;
        private readonly IDomesticRouteZoneMapService _domesticRouteZoneMapService;
        private readonly IWalletService _walletService;
        private readonly IShipmentTrackingService _shipmentTrackingService;
        private readonly IGlobalPropertyService _globalPropertyService;
        private readonly ICountryRouteZoneMapService _countryRouteZoneMapService;
        private readonly IPaymentService _paymentService;
        private readonly IGIGGoPricingService _gIGGoPricingService;
        private readonly INodeService _nodeService;
        private readonly IDHLService _DhlService;
        private readonly IWaybillPaymentLogService _waybillPaymentLogService;

        public ShipmentService(IUnitOfWork uow, IDeliveryOptionService deliveryService,
            IServiceCentreService centreService, IUserServiceCentreMappingService userServiceCentre,
            INumberGeneratorMonitorService numberGeneratorMonitorService,
            ICustomerService customerService, IUserService userService,
            IMessageSenderService messageSenderService, ICompanyService companyService,
            IDomesticRouteZoneMapService domesticRouteZoneMapService,
            IWalletService walletService, IShipmentTrackingService shipmentTrackingService,
            IGlobalPropertyService globalPropertyService, ICountryRouteZoneMapService countryRouteZoneMapService,
            IPaymentService paymentService, IGIGGoPricingService gIGGoPricingService, INodeService nodeService, IDHLService dHLService, IWaybillPaymentLogService waybillPaymentLogService)
        {
            _uow = uow;
            _deliveryService = deliveryService;
            _centreService = centreService;
            _userServiceCentre = userServiceCentre;
            _numberGeneratorMonitorService = numberGeneratorMonitorService;
            _customerService = customerService;
            _userService = userService;
            _messageSenderService = messageSenderService;
            _companyService = companyService;
            _domesticRouteZoneMapService = domesticRouteZoneMapService;
            _walletService = walletService;
            _shipmentTrackingService = shipmentTrackingService;
            _globalPropertyService = globalPropertyService;
            _countryRouteZoneMapService = countryRouteZoneMapService;
            _paymentService = paymentService;
            _gIGGoPricingService = gIGGoPricingService;
            _nodeService = nodeService;
            _DhlService = dHLService;
            _waybillPaymentLogService = waybillPaymentLogService;
            MapperConfig.Initialize();
        }

        public async Task<Tuple<List<ShipmentDTO>, int>> GetShipments(FilterOptionsDto filterOptionsDto)
        {
            try
            {
                var serviceCenters = await _userService.GetPriviledgeServiceCenters();
                return await _uow.Shipment.GetShipments(filterOptionsDto, serviceCenters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Tuple<List<IntlShipmentDTO>, int>> GetIntlTransactionShipments(FilterOptionsDto filterOptionsDto)
        {
            try
            {
                var serviceCenters = await _userService.GetPriviledgeServiceCenters();
                return await _uow.IntlShipmentRequest.GetIntlTransactionShipmentRequest(filterOptionsDto, serviceCenters);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Tuple<List<IntlShipmentDTO>, int>> GetIntlTransactionShipments(DateFilterCriteria filterOptionsDto)
        {
            try
            {
                return await _uow.IntlShipmentRequest.GetIntlTransactionShipmentRequest(filterOptionsDto);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<InvoiceViewDTO>> GetIncomingShipments(FilterOptionsDto filterOptionsDto)
        {
            try
            {
                var serviceCenters = await _userService.GetPriviledgeServiceCenters();

                var allShipments = _uow.Invoice.GetAllFromInvoiceAndShipments().Where(s => s.IsShipmentCollected == false);
                var incomingShipments = new List<InvoiceViewDTO>();

                if (serviceCenters.Length > 0)
                {
                    //Get shipments coming to the service centre 
                    //var shipmentResult = allShipments.Where(s => serviceCenters.Contains(s.DestinationServiceCentreId)).ToList();
                    allShipments = allShipments.Where(s => serviceCenters.Contains(s.DestinationServiceCentreId));

                    //For waybill to be collected it must have satisfy the follwoing Shipment Scan Status
                    //Collected by customer (OKC & OKT), Return (SSR), Reroute (SRR) : All status satisfy IsShipmentCollected above
                    //shipments that have arrived destination service centre should not be displayed in expected shipments
                    //List<string> shipmentCollection = _uow.ShipmentCollection.GetAllAsQueryable()
                    //    .Where(x => serviceCenters.Contains(x.DestinationServiceCentreId) && !(x.ShipmentScanStatus == ShipmentScanStatus.OKC && x.ShipmentScanStatus == ShipmentScanStatus.OKT
                    //    && x.ShipmentScanStatus == ShipmentScanStatus.SSR && x.ShipmentScanStatus == ShipmentScanStatus.SRR)).Select(w => w.Waybill).ToList();

                    var shipmentCollection = _uow.ShipmentCollection.GetAllAsQueryable()
                        .Where(x => serviceCenters.Contains(x.DestinationServiceCentreId)).Select(w => w.Waybill).Distinct();

                    //remove all the waybills that at the collection center from the income shipments
                    var shipmentResult = allShipments.Where(s => !shipmentCollection.Contains(s.Waybill)).ToList();
                    incomingShipments = Mapper.Map<List<InvoiceViewDTO>>(shipmentResult);
                }

                //Use to populate service centre 
                var allServiceCentres = await _centreService.GetServiceCentres();
                var deliveryOptions = await _deliveryService.GetDeliveryOptions();

                //populate the service centres
                foreach (var invoiceViewDTO in incomingShipments)
                {
                    invoiceViewDTO.DepartureServiceCentre = allServiceCentres.SingleOrDefault(x => x.ServiceCentreId == invoiceViewDTO.DepartureServiceCentreId);
                    invoiceViewDTO.DestinationServiceCentre = allServiceCentres.SingleOrDefault(x => x.ServiceCentreId == invoiceViewDTO.DestinationServiceCentreId);
                    invoiceViewDTO.DeliveryOption = deliveryOptions.SingleOrDefault(x => x.DeliveryOptionId == invoiceViewDTO.DeliveryOptionId);
                }

                return await Task.FromResult(incomingShipments);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task<List<ShipmentDTO>> GetShipments(int[] serviceCentreIds)
        {
            try
            {
                return _uow.Shipment.GetShipments(serviceCentreIds);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task DeleteShipment(int shipmentId)
        {
            try
            {
                var shipment = await _uow.Shipment.GetAsync(x => x.ShipmentId == shipmentId);
                if (shipment == null)
                {
                    throw new GenericException("Shipment Information does not exist");
                }
                _uow.Shipment.Remove(shipment);
                _uow.Complete();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task DeleteShipment(string waybill)
        {
            try
            {
                var shipment = await _uow.Shipment.GetAsync(x => x.Waybill.Equals(waybill));
                if (shipment == null)
                {
                    throw new GenericException($"Shipment with waybill: {waybill} does not exist");
                }
                _uow.Shipment.Remove(shipment);
                _uow.Complete();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ShipmentDTO> GetShipmentOld(string waybill)
        {
            try
            {
                var shipment = await _uow.Shipment.GetAsync(x => x.Waybill.Equals(waybill));

                if (shipment == null)
                {
                    throw new GenericException($"Shipment with waybill: {waybill} does not exist", $"{(int)HttpStatusCode.NotFound}");
                }

                return await GetShipment(shipment.ShipmentId);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ShipmentDTO> GetShipment(string waybill)
        {
            try
            {
                var shipmentDto = await _uow.Shipment.GetShipment(waybill);
                if (shipmentDto == null)
                {
                    throw new GenericException("Shipment Information does not exist", $"{(int)HttpStatusCode.NotFound}");
                }

                // get ServiceCentre
                var departureServiceCentre = await _centreService.GetServiceCentreById(shipmentDto.DepartureServiceCentreId);
                var destinationServiceCentre = await _centreService.GetServiceCentreById(shipmentDto.DestinationServiceCentreId);
                shipmentDto.DepartureServiceCentre = departureServiceCentre;
                shipmentDto.DestinationServiceCentre = destinationServiceCentre;

                //get CustomerDetails
                if (shipmentDto.CustomerType.Contains("Individual"))
                {
                    shipmentDto.CustomerType = CustomerType.IndividualCustomer.ToString();
                }

                CustomerType customerType = (CustomerType)Enum.Parse(typeof(CustomerType), shipmentDto.CustomerType);
                shipmentDto.CustomerDetails = await _customerService.GetCustomer(shipmentDto.CustomerId, customerType);
                shipmentDto.CustomerDetails.Address = shipmentDto.SenderAddress ?? shipmentDto.CustomerDetails.Address;
                shipmentDto.CustomerDetails.State = shipmentDto.SenderState ?? shipmentDto.CustomerDetails.State;
                shipmentDto.Customer = new List<CustomerDTO>
                {
                    shipmentDto.CustomerDetails
                };

                if (shipmentDto.IsCancelled)
                {
                    var cancel = await _uow.ShipmentCancel.GetAsync(s => s.Waybill == shipmentDto.Waybill);
                    shipmentDto.ShipmentCancel = Mapper.Map<ShipmentCancelDTO>(cancel);
                }
                else
                {
                    if (shipmentDto.Invoice.IsShipmentCollected)
                    {
                        var demurrage = await _uow.Demurrage.GetAsync(s => s.WaybillNumber == shipmentDto.Waybill);
                        var demurrageDTO = Mapper.Map<DemurrageDTO>(demurrage);
                        shipmentDto.Demurrage = demurrageDTO;
                    }

                    var shipmentCollection = await _uow.ShipmentCollection.GetAsync(s => s.Waybill == shipmentDto.Waybill);
                    if (shipmentCollection != null)
                    {
                        var shipmentCollectionDto = Mapper.Map<ShipmentCollectionDTO>(shipmentCollection);
                        shipmentDto.ShipmentCollection = shipmentCollectionDto;

                        //Get Demurage if shipment has not been collected
                        if (!shipmentDto.Invoice.IsShipmentCollected)
                        {
                            //set Default Demurrage info in ShipmentDTO for Company customer
                            shipmentDto.Demurrage = new DemurrageDTO
                            {
                                Amount = 0.00M,
                                DayCount = 0,
                                WaybillNumber = shipmentDto.Waybill
                            };

                            //Demurage should be exclude from Company customer. Only individual customer should have demurage
                            //HomeDelivery shipments should not have demurrage for Individual Shipments
                            if (customerType == CustomerType.IndividualCustomer && shipmentDto.PickupOptions == PickupOptions.SERVICECENTER)
                            {
                                //get Demurrage information for Individual customer
                                await GetDemurrageInformation(shipmentDto);
                            }
                        }
                    }

                    if (shipmentDto.IsFromMobile)
                    {
                        var deliveryNumber = await _uow.DeliveryNumber.GetAsync(x => x.Waybill == shipmentDto.Waybill);
                        if (deliveryNumber != null)
                        {
                            shipmentDto.SenderCode = deliveryNumber.SenderCode;

                            if (shipmentDto.PickupOptions == PickupOptions.SERVICECENTER)
                            {
                                shipmentDto.ReceiverCode = deliveryNumber.ReceiverCode;
                            }
                        }
                    }
                }
                return shipmentDto;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ShipmentDTO> GetShipment(int shipmentId)
        {
            try
            {
                var shipment = await _uow.Shipment.GetAsync(x => x.ShipmentId == shipmentId, "DeliveryOption, ShipmentItems");
                if (shipment == null)
                {
                    throw new GenericException("Shipment Information does not exist", $"{(int)HttpStatusCode.NotFound}");
                }

                var shipmentDto = Mapper.Map<ShipmentDTO>(shipment);

                // get ServiceCentre
                var departureServiceCentre = await _centreService.GetServiceCentreById(shipment.DepartureServiceCentreId);
                var destinationServiceCentre = await _centreService.GetServiceCentreById(shipment.DestinationServiceCentreId);

                //Change the Service Centre Code to country name if the shipment is International shipment
                //if (shipmentDto.IsInternational)
                //{
                //    departureServiceCentre.Code = departureServiceCentre.Country;
                //    destinationServiceCentre.Code = destinationServiceCentre.Country;
                //}

                shipmentDto.DepartureServiceCentre = departureServiceCentre;
                shipmentDto.DestinationServiceCentre = destinationServiceCentre;

                //get CustomerDetails
                if (shipmentDto.CustomerType.Contains("Individual"))
                {
                    shipmentDto.CustomerType = CustomerType.IndividualCustomer.ToString();
                }

                CustomerType customerType = (CustomerType)Enum.Parse(typeof(CustomerType), shipmentDto.CustomerType);
                shipmentDto.CustomerDetails = await _customerService.GetCustomer(shipmentDto.CustomerId, customerType);
                shipmentDto.Customer = new List<CustomerDTO>();
                shipmentDto.Customer.Add(shipmentDto.CustomerDetails);

                //get wallet number
                //var wallets = await _walletService.GetWallets();
                //var customerWallet = _uow.Wallet.SingleOrDefault(
                //    s => s.CustomerId == shipmentDto.CustomerId && s.CustomerType == customerType);
                //shipmentDto.WalletNumber = customerWallet?.WalletNumber;

                var customerWallet = _uow.Wallet.SingleOrDefault(s => s.CustomerCode == shipmentDto.CustomerCode);
                shipmentDto.WalletNumber = customerWallet?.WalletNumber;

                if (shipmentDto.IsCancelled)
                {
                    //get the Cancellation Reason
                    var desc = _uow.ShipmentCancel.SingleOrDefault(s => s.Waybill == shipmentDto.Waybill);
                    var descCollection = Mapper.Map<ShipmentCancelDTO>(desc);
                    shipmentDto.ShipmentCancel = descCollection;
                }

                //only if the shipment is collected
                //get ShipmentCollection if it exists
                var shipmentCollection = _uow.ShipmentCollection.SingleOrDefault(s => s.Waybill == shipmentDto.Waybill);
                var shipmentCollectionDTO = Mapper.Map<ShipmentCollectionDTO>(shipmentCollection);
                shipmentDto.ShipmentCollection = shipmentCollectionDTO;


                if (shipmentCollection != null)
                {
                    //get Invoice if it exists
                    var invoice = _uow.Invoice.SingleOrDefault(s => s.Waybill == shipmentDto.Waybill);
                    var invoiceDTO = Mapper.Map<InvoiceDTO>(invoice);
                    shipmentDto.Invoice = invoiceDTO;

                    if (shipmentDto.Invoice.IsShipmentCollected)
                    {
                        var demurrage = await _uow.Demurrage.GetAsync(s => s.WaybillNumber == shipmentDto.Waybill);
                        var demurrageDTO = Mapper.Map<DemurrageDTO>(demurrage);
                        shipmentDto.Demurrage = demurrageDTO;
                    }

                    //Demurage should be exclude from Ecommerce and Corporate customer. Only individual customer should have demurage
                    //HomeDelivery shipments should not have demurrage for Individual Shipments
                    else
                    {
                        //set Default Demurrage info in ShipmentDTO for Company customer
                        shipmentDto.Demurrage = new DemurrageDTO
                        {
                            Amount = 0,
                            DayCount = 0,
                            WaybillNumber = shipmentDto.Waybill
                        };

                        if (customerType != CustomerType.Company || shipmentDto.PickupOptions != PickupOptions.HOMEDELIVERY)
                        {
                            //get Demurrage information for Individual customer
                            await GetDemurrageInformation(shipmentDto);
                        }
                    }
                }

                //Set the Senders AAddress for the Shipment in the CustomerDetails
                shipmentDto.CustomerDetails.Address = shipmentDto.SenderAddress ?? shipmentDto.CustomerDetails.Address;
                shipmentDto.CustomerDetails.State = shipmentDto.SenderState ?? shipmentDto.CustomerDetails.State;

                if (shipment.IsFromMobile == true)
                {
                    var preShipmentMobile = await _uow.PreShipmentMobile.GetAsync(x => x.Waybill == shipment.Waybill && x.IsFromAgility == true);
                    if (preShipmentMobile != null)
                    {
                        var deliveryNumber = await _uow.DeliveryNumber.GetAsync(x => x.Waybill == preShipmentMobile.Waybill);
                        shipmentDto.SenderCode = deliveryNumber.SenderCode;

                        if (shipment.PickupOptions == PickupOptions.SERVICECENTER)
                        {
                            shipmentDto.ReceiverCode = deliveryNumber.ReceiverCode;
                        }
                    }
                }

                return shipmentDto;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<PreShipmentDTO> GetTempShipment(string code)
        {
            try
            {
                var shipment = await _uow.PreShipment.GetAsync(x => x.TempCode == code, "PreShipmentItems");
                if (shipment == null)
                {
                    throw new GenericException("Pre Shipment Information does not exist");
                }
                if (shipment.IsProcessed)
                {
                    throw new GenericException($" {code} has been processed already");
                }

                var shipmentDto = Mapper.Map<PreShipmentDTO>(shipment);

                //Get the customer wallet balance
                var wallet = await _walletService.GetWalletBalance(shipment.CustomerCode);
                shipmentDto.WalletBalance = wallet.Balance;

                return shipmentDto;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ShipmentDTO> GetDropOffShipmentForProcessing(string code)
        {
            try
            {
                var shipment = await _uow.PreShipment.GetAsync(x => x.TempCode == code, "PreShipmentItems");
                if (shipment == null)
                {
                    throw new GenericException("Pre Shipment Information does not exist", $"{(int)HttpStatusCode.NotFound}");
                }

                if (shipment.IsProcessed)
                {
                    throw new GenericException($" {code} has been processed already. The processed waybill for the code  is {shipment.Waybill} ", $"{(int)HttpStatusCode.Forbidden}");
                }

                var shipmentDto = new ShipmentDTO
                {
                    //shipment info
                    TempCode = shipment.TempCode,
                    Waybill = shipment.Waybill,
                    PickupOptions = shipment.PickupOptions,
                    Value = shipment.Value,
                    ApproximateItemsWeight = shipment.ApproximateItemsWeight,
                    CompanyType = shipment.CompanyType,
                    DeclarationOfValueCheck = shipment.Value,
                    CustomerType = shipment.CompanyType,

                    //reciever info
                    ReceiverName = shipment.ReceiverName,
                    ReceiverPhoneNumber = shipment.ReceiverPhoneNumber,
                    ReceiverAddress = shipment.ReceiverAddress,
                    ReceiverCity = shipment.ReceiverCity,
                    DestinationServiceCentreId = shipment.DestinationServiceCenterId,
                    LGA = shipment.LGA

                };

                if (shipment.IsAgent)
                {
                    string senderPhoneNumber = shipment.SenderPhoneNumber;

                    if (!string.IsNullOrWhiteSpace(shipment.SenderPhoneNumber))
                    {
                        if (shipment.SenderPhoneNumber.StartsWith("0"))
                        {
                            senderPhoneNumber = shipment.SenderPhoneNumber.Remove(0, 1);
                        }
                    }

                    //check if customer information already exist 
                    var individualCustomer = await _uow.IndividualCustomer.GetAsync(x => x.PhoneNumber.Contains(senderPhoneNumber));
                    if (individualCustomer != null)
                    {
                        IndividualCustomerDTO individualCustomerDTO = Mapper.Map<IndividualCustomerDTO>(individualCustomer);
                        var customerDTO = Mapper.Map<CustomerDTO>(individualCustomerDTO);
                        customerDTO.CustomerType = CustomerType.IndividualCustomer;
                        customerDTO.WalletBalance = 0.0M;
                        customerDTO.RowVersion = null;

                        shipmentDto.CustomerDetails = customerDTO;
                        shipmentDto.CustomerCode = customerDTO.CustomerCode;
                    }
                    else
                    {
                        shipmentDto.CompanyType = UserChannelType.IndividualCustomer.ToString();
                        shipmentDto.CustomerType = UserChannelType.IndividualCustomer.ToString();
                        shipmentDto.CustomerDetails = new CustomerDTO
                        {
                            PhoneNumber = shipment.SenderPhoneNumber,
                            WalletBalance = 0.0M,
                            Address = shipment.SenderCity,
                            CompanyType = CompanyType.Client,
                            CustomerType = CustomerType.IndividualCustomer,
                            City = shipment.SenderCity
                        };

                        string[] words = shipment.SenderName.Split(' ');
                        shipmentDto.CustomerDetails.FirstName = words.FirstOrDefault();
                        shipmentDto.CustomerDetails.LastName = words.Skip(1).FirstOrDefault();
                    }

                    shipmentDto.CustomerDetails.Address = shipment.SenderCity;
                    shipmentDto.CustomerDetails.State = shipment.SenderCity;
                }
                else
                {
                    shipmentDto.CustomerCode = shipment.CustomerCode;
                    UserChannelType customerType = (UserChannelType)Enum.Parse(typeof(UserChannelType), shipment.CompanyType);
                    shipmentDto.CustomerDetails = await _customerService.GetCustomer(shipment.CustomerCode, customerType);

                    ////Get the customer wallet balance                
                    var wallet = await _walletService.GetWalletBalance(shipment.CustomerCode);
                    shipmentDto.CustomerDetails.WalletBalance = wallet.Balance;
                    shipmentDto.CustomerDetails.RowVersion = null;

                    if (customerType == UserChannelType.IndividualCustomer)
                    {
                        shipmentDto.CustomerDetails.Address = shipment.SenderCity;
                        shipmentDto.CustomerDetails.State = shipment.SenderCity;
                    }
                }

                shipmentDto.Customer = new List<CustomerDTO>
                {
                    shipmentDto.CustomerDetails
                };

                shipmentDto.ShipmentItems = new List<ShipmentItemDTO>();

                //Shipment Item
                foreach (var item in shipment.PreShipmentItems)
                {
                    shipmentDto.ShipmentItems.Add(new ShipmentItemDTO
                    {
                        Description = item.Description,
                        ShipmentType = item.ShipmentType,
                        Height = item.Height,
                        IsVolumetric = item.IsVolumetric,
                        Description_s = item.Description_s,
                        Length = item.Length,
                        Nature = item.Nature,
                        Quantity = item.Quantity,
                        Price = item.Price,
                        SerialNumber = item.SerialNumber,
                        Weight = item.Weight,
                        Width = item.Width,
                        ShipmentItemId = item.SpecialPackageId == null ? 0 : (int)item.SpecialPackageId  //use special item to represent package id for special shipment
                    });
                }

                //Get Departure Service Center and Destination Service centre
                var serviceCenterIds = await _userService.GetPriviledgeServiceCenters();
                shipmentDto.DepartureServiceCentreId = serviceCenterIds[0];

                //Get SuperCentre for Home Delivery
                if (shipmentDto.PickupOptions == PickupOptions.HOMEDELIVERY)
                {
                    var station = await _uow.Station.GetAsync(x => x.StationId == shipment.DestinationStationId);

                    if (station != null)
                    {
                        shipmentDto.DestinationServiceCentreId = station.SuperServiceCentreId;
                    }
                }

                if (shipmentDto.DestinationServiceCentreId == 0)
                {
                    var serviceCentres = await _uow.ServiceCentre.GetAsync(x => x.StationId == shipment.DestinationStationId);

                    if (serviceCentres != null)
                    {
                        shipmentDto.DestinationServiceCentreId = serviceCentres.ServiceCentreId;
                    }
                }
                //get service centre info
                var destCentre = await _uow.ServiceCentre.GetAsync(x => x.ServiceCentreId == shipmentDto.DestinationServiceCentreId);
                if (destCentre != null)
                {
                    shipmentDto.DestinationServiceCentreName = destCentre.Name;
                }


                return shipmentDto;
            }
            catch (Exception)
            {
                throw;
            }
        }

        //get basic shipment details
        public async Task<ShipmentDTO> GetBasicShipmentDetail(string waybill)
        {
            try
            {
                return await _uow.Shipment.GetBasicShipmentDetail(waybill);
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task GetDemurrageInformationOld(ShipmentDTO shipmentDto)
        {
            var price = 0;
            var demurrageDays = 0;

            //get GlobalProperty
            var demurrageCountObj = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.DemurrageDayCount, shipmentDto.DestinationCountryId);
            var demurragePriceObj = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.DemurragePrice, shipmentDto.DestinationCountryId);

            //validate
            if (demurrageCountObj == null || demurragePriceObj == null)
            {
                shipmentDto.Demurrage = new DemurrageDTO
                {
                    Amount = price,
                    DayCount = demurrageDays,
                    WaybillNumber = shipmentDto.Waybill,
                };
                return;
            }

            //get ShipmentCollection
            var shipmentCollection = shipmentDto.ShipmentCollection;

            var today = DateTime.Now;
            var demurrageStartDate = shipmentCollection.DateCreated.AddDays(int.Parse(demurrageCountObj.Value));
            demurrageDays = today.Subtract(demurrageStartDate).Days;

            if (demurrageDays > 0)
            {
                price = demurrageDays * (int.Parse(demurragePriceObj.Value));
            }

            //set Demurrage info in ShipmentDTO
            shipmentDto.Demurrage = new DemurrageDTO
            {
                Amount = price,
                DayCount = demurrageDays,
                WaybillNumber = shipmentDto.Waybill
            };
        }

        private async Task GetDemurrageInformation(ShipmentDTO shipmentDto)
        {
            //get GlobalProperty
            var demurrageCountObj = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.DemurrageDayCount, shipmentDto.DestinationCountryId);
            var demurragePriceObj = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.DemurragePrice, shipmentDto.DestinationCountryId);

            if (demurrageCountObj != null && demurragePriceObj != null)
            {
                //Calculate Demurage Price
                var today = DateTime.Now;
                var shipmentCollection = shipmentDto.ShipmentCollection;
                var demurrageStartDate = shipmentCollection.DateCreated.AddDays(int.Parse(demurrageCountObj.Value));
                shipmentDto.Demurrage.DayCount = today.Subtract(demurrageStartDate).Days;

                if (shipmentDto.Demurrage.DayCount > 0)
                {
                    shipmentDto.Demurrage.Amount = shipmentDto.Demurrage.DayCount * (int.Parse(demurragePriceObj.Value));
                }
            }
        }

        public async Task UpdateShipment(int shipmentId, ShipmentDTO shipmentDto)
        {
            try
            {
                await _deliveryService.GetDeliveryOptionById(shipmentDto.DeliveryOptionId);
                await _centreService.GetServiceCentreById(shipmentDto.DepartureServiceCentreId);
                await _centreService.GetServiceCentreById(shipmentDto.DestinationServiceCentreId);

                var shipment = await _uow.Shipment.GetAsync(shipmentId);
                var customer = await _uow.IndividualCustomer.GetAsync(shipmentDto.CustomerId);

                if (shipment == null || shipmentId != shipment.ShipmentId)
                {
                    throw new GenericException("Shipment Information does not exist");
                }

                shipment.SealNumber = shipmentDto.SealNumber;
                shipment.Value = shipmentDto.Value;
                shipment.UserId = shipmentDto.UserId;
                shipment.ReceiverState = shipmentDto.ReceiverState;
                shipment.ReceiverPhoneNumber = shipmentDto.ReceiverPhoneNumber;
                shipment.ReceiverName = shipmentDto.ReceiverName;
                shipment.ReceiverEmail = shipmentDto.ReceiverEmail;
                shipment.ReceiverAddress = shipmentDto.ReceiverAddress;
                shipment.ReceiverCountry = shipmentDto.ReceiverCountry;
                shipment.ReceiverCity = shipmentDto.ReceiverCity;
                shipment.PaymentStatus = shipmentDto.PaymentStatus;
                shipment.ExpectedDateOfArrival = shipmentDto.ExpectedDateOfArrival;
                shipment.DestinationServiceCentreId = shipmentDto.DestinationServiceCentreId;
                shipment.DepartureServiceCentreId = shipmentDto.DepartureServiceCentreId;
                shipment.DeliveryTime = shipmentDto.DeliveryTime;
                shipment.DeliveryOptionId = shipmentDto.DeliveryOptionId;
                shipment.CustomerType = shipmentDto.CustomerType;
                shipment.CustomerId = shipmentDto.CustomerId;
                shipment.ActualDateOfArrival = shipmentDto.ActualDateOfArrival;
                shipment.GrandTotal = shipmentDto.GrandTotal;
                shipment.Total = shipmentDto.Total;

                customer.Email = shipmentDto.CustomerDetails.Email;
                customer.FirstName = shipmentDto.CustomerDetails.FirstName;
                customer.LastName = shipmentDto.CustomerDetails.LastName;
                customer.PhoneNumber = shipmentDto.CustomerDetails.PhoneNumber;
                customer.Address = shipmentDto.CustomerDetails.Address;

                await _uow.CompleteAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task UpdateShipment(string waybill, ShipmentDTO shipmentDto)
        {
            try
            {
                await _deliveryService.GetDeliveryOptionById(shipmentDto.DeliveryOptionId);
                await _centreService.GetServiceCentreById(shipmentDto.DepartureServiceCentreId);
                await _centreService.GetServiceCentreById(shipmentDto.DestinationServiceCentreId);

                var shipment = await _uow.Shipment.GetAsync(x => x.Waybill.Equals(waybill));
                if (shipment == null)
                {
                    throw new GenericException($"Shipment with waybill: {waybill} does not exist");
                }

                shipment.SealNumber = shipmentDto.SealNumber;
                shipment.Value = shipmentDto.Value;
                shipment.UserId = shipmentDto.UserId;
                shipment.ReceiverState = shipmentDto.ReceiverState;
                shipment.ReceiverPhoneNumber = shipmentDto.ReceiverPhoneNumber;
                shipment.ReceiverName = shipmentDto.ReceiverName;
                shipment.ReceiverCountry = shipmentDto.ReceiverCountry;
                shipment.ReceiverCity = shipmentDto.ReceiverCity;
                shipment.PaymentStatus = shipmentDto.PaymentStatus;
                //shipment.IsDomestic = shipmentDto.IsDomestic;
                //shipment.IndentificationUrl = shipmentDto.IndentificationUrl;
                //shipment.IdentificationType = shipmentDto.IdentificationType;
                //shipment.GroupWaybill = shipmentDto.GroupWaybill;
                shipment.ExpectedDateOfArrival = shipmentDto.ExpectedDateOfArrival;
                shipment.DestinationServiceCentreId = shipmentDto.DestinationServiceCentreId;
                shipment.DepartureServiceCentreId = shipmentDto.DepartureServiceCentreId;
                shipment.DeliveryTime = shipmentDto.DeliveryTime;
                shipment.DeliveryOptionId = shipmentDto.DeliveryOptionId;
                shipment.CustomerType = shipmentDto.CustomerType;
                shipment.CustomerId = shipmentDto.CustomerId;
                //shipment.Comments = shipmentDto.Comments;
                //shipment.ActualreceiverPhone = shipmentDto.ActualreceiverPhone;
                //shipment.ActualReceiverName = shipmentDto.ActualReceiverName;
                shipment.ActualDateOfArrival = shipmentDto.ActualDateOfArrival;

                await _uow.CompleteAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        //
        public async Task<bool> RePrintCountUpdater()
        {
            var userActiveCountryId = await _userService.GetUserActiveCountryId();

            try
            {
                //Get the global properties of the number of days to allow reprint to stop
                var globalpropertiesreprintlimitObj = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.ReprintDays, userActiveCountryId);
                string globalpropertiesreprintStr = globalpropertiesreprintlimitObj?.Value;

                var globalpropertiesreprintcounter = 0;
                bool success_counter = int.TryParse(globalpropertiesreprintStr, out globalpropertiesreprintcounter);

                //===========================================================================

                //Get the global properties of the date to start using this service
                var globalpropertiesreObj = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.ReprintFeatureStartDate, userActiveCountryId);
                string globalpropertiesdateStr = globalpropertiesreObj?.Value;

                var globalpropertiesreprintdate = DateTime.MinValue;
                bool success = DateTime.TryParse(globalpropertiesdateStr, out globalpropertiesreprintdate);

                //============================================================================

                //var serviceCenters = await _userService.GetPriviledgeServiceCenters();
                var shipments = _uow.Shipment.GetAllAsQueryable().Where(s => s.ReprintCounterStatus == false && s.DateCreated >= globalpropertiesreprintdate).ToList();
                var today = DateTime.Now;

                var newShipmentLists = new List<Shipment>();

                foreach (var shipment in shipments)
                {
                    var creationDate = shipment.DateCreated;
                    int daysDiff = ((TimeSpan)(today - creationDate)).Days;

                    if (daysDiff >= globalpropertiesreprintcounter)
                    {
                        newShipmentLists.Add(shipment);
                    }
                }

                newShipmentLists.ForEach(a => a.ReprintCounterStatus = true);
                await _uow.CompleteAsync();

                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<ShipmentDTO> AddShipment(ShipmentDTO shipmentDTO)
        {
            try
            {
                if (!String.IsNullOrEmpty(shipmentDTO.ReceiverPhoneNumber))
                {
                    shipmentDTO.ReceiverPhoneNumber = shipmentDTO.ReceiverPhoneNumber.Trim();
                }

                if (!String.IsNullOrEmpty(shipmentDTO.Customer[0].PhoneNumber))
                {
                    shipmentDTO.Customer[0].PhoneNumber = shipmentDTO.Customer[0].PhoneNumber.Trim();
                }

                if (!string.IsNullOrEmpty(shipmentDTO.TempCode))
                {
                    //check if it has been processed 
                    var dropoff = await _uow.PreShipment.GetAsync(s => s.TempCode == shipmentDTO.TempCode);

                    if (dropoff.IsProcessed)
                    {
                        throw new GenericException($"This drop off {shipmentDTO.TempCode} has already been processed");
                    }
                }

                var hashString = await ComputeHash(shipmentDTO);
                var checkForHash = await _uow.ShipmentHash.GetAsync(x => x.HashedShipment == hashString);

                if (checkForHash != null)
                {
                    DateTime dateTime = DateTime.Now.AddMinutes(-30);
                    int timeResult = DateTime.Compare(checkForHash.DateModified, dateTime);

                    //if (timeResult > 0)
                    //{
                    //    throw new GenericException("A similar shipment already exists on Agility, kindly view your created shipment to confirm.");
                    //}
                    //else
                    //{
                    checkForHash.DateModified = DateTime.Now;
                    // }
                }
                else
                {
                    var hasher = new ShipmentHash()
                    {
                        HashedShipment = hashString
                    };
                    _uow.ShipmentHash.Add(hasher);
                }

                // create the customer, if information does not exist in our record
                var customerId = await CreateCustomer(shipmentDTO);

                //Block account that has been suspended/pending from create shipment
                if (shipmentDTO.CompanyType == CompanyType.Corporate.ToString() || shipmentDTO.CompanyType == CompanyType.Ecommerce.ToString())
                {
                    if (customerId.CompanyStatus != CompanyStatus.Active)
                    {
                        throw new GenericException($"{customerId.Name} account has been {customerId.CompanyStatus}, contact support for assistance", $"{(int)HttpStatusCode.Forbidden}");
                    }
                }

                // create the shipment and shipmentItems
                var newShipment = await CreateShipment(shipmentDTO);
                shipmentDTO.DepartureCountryId = newShipment.DepartureCountryId;

                // create the Invoice and GeneralLedger
                await CreateInvoice(shipmentDTO);
                CreateGeneralLedger(shipmentDTO);

                //QR Code
                await GenerateDeliveryNumber(newShipment.Waybill);

                // complete transaction if all actions are successful
                //add to shipmentmonitor table
                var deptCentre = await _centreService.GetServiceCentreById(shipmentDTO.DepartureServiceCentreId);
                var userInfo = await _uow.User.GetUserById(newShipment.UserId);
                var timeMonitor = new ShipmentTimeMonitor()
                {
                    Waybill = newShipment.Waybill,
                    UserId = newShipment.UserId,
                    UserName = $"{userInfo.FirstName} {userInfo.LastName}",
                    TimeInSeconds = shipmentDTO.TimeInSeconds,
                    UserServiceCentreId = shipmentDTO.DepartureServiceCentreId,
                    UserServiceCentreName = deptCentre.Name
                };
                _uow.ShipmentTimeMonitor.Add(timeMonitor);
                await _uow.CompleteAsync();

                if (!string.IsNullOrEmpty(shipmentDTO.TempCode))
                {
                    await UpdateDropOff(newShipment.Waybill, shipmentDTO.TempCode);
                }

                //scan the shipment for tracking
                await ScanShipment(new ScanDTO
                {
                    WaybillNumber = newShipment.Waybill,
                    ShipmentScanStatus = ShipmentScanStatus.CRT
                });

                //For Corporate Customers, Pay for their shipments through wallet immediately
                if (CompanyType.Corporate.ToString() == shipmentDTO.CompanyType || CompanyType.Ecommerce.ToString() == shipmentDTO.CompanyType)
                {
                    var walletEnumeration = await _uow.Wallet.FindAsync(x => x.CustomerCode.Equals(customerId.CustomerCode));
                    var wallet = walletEnumeration.FirstOrDefault();

                    if (wallet != null)
                    {
                        await _paymentService.ProcessPayment(new PaymentTransactionDTO()
                        {
                            PaymentType = PaymentType.Wallet,
                            TransactionCode = wallet.WalletNumber,
                            Waybill = newShipment.Waybill
                        });
                    }
                }
                return newShipment;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<ShipmentDTO> AddAgilityShipmentToGIGGo(PreShipmentMobileFromAgilityDTO shipment)
        {
            try
            {
                var isDisable = ConfigurationManager.AppSettings["DisableShipmentCreation"];
                bool disableShipmentCreation = bool.Parse(isDisable);

                if (disableShipmentCreation)
                {
                    string message = ConfigurationManager.AppSettings["DisableShipmentCreationMessage"];
                    throw new GenericException(message, $"{(int)HttpStatusCode.ServiceUnavailable}");
                }

                if (string.IsNullOrEmpty(shipment.VehicleType))
                {
                    throw new GenericException("Please select a vehicle type");
                }

                if (!shipment.ShipmentItems.Any())
                {
                    throw new GenericException("Shipment Items cannot be empty");
                }

                if (shipment.PickupOptions == PickupOptions.SERVICECENTER)
                {
                    var receieverServiceCenter = await _uow.ServiceCentre.GetAsync(x => x.ServiceCentreId == shipment.DestinationServiceCentreId);
                    if (receieverServiceCenter.Latitude == null || receieverServiceCenter.Longitude == null)
                    {
                        throw new GenericException("Destination Service Center Longitude and Latitude details not found");

                    }
                    shipment.ReceiverAddress = $"GIGL {receieverServiceCenter.FormattedServiceCentreName} SERVICE CENTER ({shipment.ReceiverAddress})";
                }

                if (shipment.PickupOptions == PickupOptions.HOMEDELIVERY && (shipment.ReceiverLocation.Longitude == null || shipment.ReceiverLocation.Latitude == null))
                {
                    throw new GenericException("Receiver Longitude and Latitude details not found");
                }

                shipment.CustomerCode = shipment.Customer[0].CustomerCode;
                if (shipment.PaymentType == PaymentType.Wallet)
                {
                    var wallet = await _uow.Wallet.GetAsync(s => s.CustomerCode == shipment.CustomerCode);
                    if (wallet.Balance < shipment.GrandTotal)
                    {
                        throw new GenericException("Insufficient Balance in the Wallet");
                    }
                    shipment.TransactionCode = wallet.WalletNumber;
                }

                shipment.DateCreated = DateTime.Now;
                var zoneid = await _domesticRouteZoneMapService.GetZoneMobile(shipment.SenderStationId, shipment.ReceiverStationId);
                shipment.ZoneMapping = zoneid.ZoneId;
                if (shipment.ZoneMapping != 1)
                {
                    throw new GenericException("This is only available for shipments within a City");
                }


                var shipmentDTO = Mapper.Map<ShipmentDTO>(shipment);
                shipmentDTO.IsFromMobile = true;
                shipmentDTO.IsGIGGOExtension = true;

                //Update SenderAddress for corporate customers
                shipmentDTO.SenderAddress = null;
                shipmentDTO.SenderState = null;
                if (shipmentDTO.Customer[0].CompanyType == CompanyType.Corporate)
                {
                    shipmentDTO.SenderAddress = shipmentDTO.Customer[0].Address;
                    shipmentDTO.SenderState = shipmentDTO.Customer[0].State;
                }

                //set some data to null
                shipmentDTO.ShipmentCollection = null;
                shipmentDTO.Demurrage = null;
                shipmentDTO.Invoice = null;
                shipmentDTO.ShipmentCancel = null;
                shipmentDTO.ShipmentReroute = null;
                shipmentDTO.DeliveryOption = null;

                var hashString = await ComputeHash(shipmentDTO);
                var checkForHash = await _uow.ShipmentHash.GetAsync(x => x.HashedShipment == hashString);

                if (checkForHash != null)
                {
                    DateTime dateTime = DateTime.Now.AddMinutes(-30);
                    int timeResult = DateTime.Compare(checkForHash.DateModified, dateTime);

                    if (timeResult > 0)
                    {
                        throw new GenericException("A similar shipment already exists on Agility, kindly view your created shipment to confirm.");
                    }
                    else
                    {
                        checkForHash.DateModified = DateTime.Now;
                    }
                }
                else
                {
                    var hasher = new ShipmentHash()
                    {
                        HashedShipment = hashString
                    };
                    _uow.ShipmentHash.Add(hasher);
                }

                // create the customer, if information does not exist in our record
                var customerId = await CreateCustomer(shipmentDTO);
                var serviceCenterIds = await _userService.GetPriviledgeServiceCenters();
                var departureId = serviceCenterIds[0];

                var senderInfo = await _uow.ServiceCentre.GetAsync(x => x.ServiceCentreId == departureId);
                shipment.SenderAddress = $"GIGL {senderInfo.FormattedServiceCentreName} SERVICE CENTER ({senderInfo.Address})";
                shipment.SenderLocation = new LocationDTO()
                {
                    Latitude = senderInfo.Latitude,
                    Longitude = senderInfo.Longitude

                };

                //get price
                var preshipmentDTO = Mapper.Map<PreShipmentMobileDTO>(shipment);


                preshipmentDTO.IsFromAgility = true;
                preshipmentDTO.Value = (decimal)shipment.DeclarationOfValueCheck;
                var priceDTO = await GetGIGGOPrice(preshipmentDTO);

                decimal total = 0.0M;
                if (shipment.CountryId == 1)
                {

                    if (shipmentDTO.Customer[0].CustomerType != CustomerType.Company)
                    {
                        total = await RoundShipmentTotal((decimal)priceDTO.GrandTotal, -2);
                    }
                    else
                    {
                        total = (decimal)priceDTO.GrandTotal;
                    }
                }
                else
                {
                    if (shipmentDTO.Customer[0].CustomerType != CustomerType.Company)
                    {
                        total = await RoundShipmentTotal((decimal)priceDTO.GrandTotal, 0);
                    }
                }

                shipmentDTO.GrandTotal = total;
                preshipmentDTO.GrandTotal = total;


                //Block account that has been suspended/pending from create shipment
                if (shipmentDTO.CompanyType == CompanyType.Corporate.ToString() || shipmentDTO.CompanyType == CompanyType.Ecommerce.ToString())
                {
                    if (customerId.CompanyStatus != CompanyStatus.Active)
                    {
                        throw new GenericException($"{customerId.Name} account has been {customerId.CompanyStatus}, contact support for assistance", $"{(int)HttpStatusCode.Forbidden}");
                    }
                }

                // create the shipment and shipmentItems
                var newShipment = await CreateShipment(shipmentDTO);
                shipmentDTO.DepartureCountryId = newShipment.DepartureCountryId;

                preshipmentDTO.Waybill = newShipment.Waybill;
                preshipmentDTO.DepartureServiceCentreId = newShipment.DepartureServiceCentreId;

                // create the Invoice and GeneralLedger
                await CreateInvoice(shipmentDTO);
                CreateGeneralLedger(shipmentDTO);

                //QR Code
                await GenerateDeliveryNumber(newShipment.Waybill);

                // complete transaction if all actions are successful
                await _uow.CompleteAsync();

                //Pay For the Shipment 
                var paymentDTO = new PaymentTransactionDTO
                {
                    PaymentType = shipment.PaymentType,
                    TransactionCode = shipment.TransactionCode,
                    Waybill = newShipment.Waybill

                };
                await _paymentService.ProcessPayment(paymentDTO);

                //Add to PreShipment Table
                await CreatePreShipmentFromAgility(preshipmentDTO, priceDTO);

                //scan the shipment for tracking
                await ScanShipment(new ScanDTO
                {
                    WaybillNumber = newShipment.Waybill,
                    ShipmentScanStatus = ShipmentScanStatus.CRT
                });

                return newShipment;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<ShipmentDTO> AddShipmentForPaymentWaiver(ShipmentDTO shipmentDTO)
        {
            try
            {
                var customerDto = await GetGIGLCorporateAccount(shipmentDTO);

                if (customerDto.CompanyId < 1)
                {
                    throw new GenericException("Corporate Acount does not exist.", $"{(int)HttpStatusCode.NotFound}");
                }

                shipmentDTO.Customer = new List<CustomerDTO>
                {
                    customerDto
                };

                var hashString = await ComputeHash(shipmentDTO);

                var checkForHash = await _uow.ShipmentHash.GetAsync(x => x.HashedShipment == hashString);
                if (checkForHash != null)
                {
                    DateTime dateTime = DateTime.Now.AddMinutes(-30);
                    int timeResult = DateTime.Compare(checkForHash.DateModified, dateTime);

                    if (timeResult > 0)
                    {
                        throw new GenericException("A similar shipment already exists on Agility, kindly view your created shipment to confirm.");
                    }
                    else
                    {
                        checkForHash.DateModified = DateTime.Now;
                    }
                }
                else
                {
                    var hasher = new ShipmentHash()
                    {
                        HashedShipment = hashString
                    };
                    _uow.ShipmentHash.Add(hasher);
                }

                // create the shipment and shipmentItems
                var newShipment = await CreateShipmentForPaymentWaiver(shipmentDTO);
                shipmentDTO.DepartureCountryId = newShipment.DepartureCountryId;

                // create the Invoice and GeneralLedger
                await CreateInvoiceForPaymentWaiver(shipmentDTO);
                CreateGeneralLedgerForPaymentWaiverShipment(shipmentDTO);

                // complete transaction if all actions are successful
                await _uow.CompleteAsync();

                //scan the shipment for tracking
                await ScanShipment(new ScanDTO
                {
                    WaybillNumber = newShipment.Waybill,
                    ShipmentScanStatus = ShipmentScanStatus.CRT
                });

                //var message = new ScanDTO
                //{
                //    WaybillNumber = newShipment.Waybill,

                //};
                //send message to regional manager on shipment creation
                await SendEmailToRegionalManagersForStoreShipments(newShipment.Waybill, newShipment.DestinationServiceCentreId, newShipment.DepartureServiceCentreId);

                return newShipment;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<string> ComputeHash(ShipmentDTO shipmentDTO)
        {
            //1. Departure Service centre can be zero or null
            //2. Description should be converted to lower case and you must check for null too  -- Done
            //3. How do you handle special shipment that might not have weight
            //4. 

            //get user Service center
            var serviceCenterIds = await _userService.GetPriviledgeServiceCenters();
            shipmentDTO.DepartureServiceCentreId = serviceCenterIds[0];

            //Create Hash Set
            var shipmentHash = new HashSet<ShipmentHashDTO>();

            var shipmentHashDto = new ShipmentHashDTO
            {
                DeptServId = shipmentDTO.DepartureServiceCentreId,
                DestServId = shipmentDTO.DestinationServiceCentreId,
                SenderPhoneNumber = shipmentDTO.Customer[0].PhoneNumber,
                ReceiverPhoneNumber = shipmentDTO.ReceiverPhoneNumber,
                Weight = new List<double>()
            };

            if (shipmentDTO.Description != null)
                shipmentHashDto.Description = shipmentDTO.Description.ToLower();


            foreach (var item in shipmentDTO.ShipmentItems)
            {
                shipmentHashDto.Weight.Add(item.Weight);
            }

            shipmentHash.Add(shipmentHashDto);

            // Create a SHA256   
            using (SHA256 sha256Hash = SHA256.Create())
            {
                //convert the object to a byte array
                byte[] objectToArray = ObjectToByteArray(shipmentHash);

                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(objectToArray);

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < bytes.Length; i++)
                {
                    builder.Append(bytes[i].ToString("x2"));
                }
                return await Task.FromResult(builder.ToString());
            }
        }

        //Update Drop Off
        private async Task UpdateDropOff(string waybill, string dropOffCode)
        {
            var dropOff = await _uow.PreShipment.GetAsync(s => s.TempCode == dropOffCode);

            dropOff.Waybill = waybill;
            dropOff.IsProcessed = true;

            await _uow.CompleteAsync();
        }

        //Update Shipment Package
        private async Task UpdateShipmentPackage(ShipmentDTO newShipment)
        {
            var user = await _userService.GetCurrentUserId();
            var serviceCenterIds = await _userService.GetPriviledgeServiceCenters();
            var currentServiceCenterId = serviceCenterIds[0];

            List<ShipmentPackagingTransactions> packageOutflow = new List<ShipmentPackagingTransactions>();

            foreach (var shipmentItem in newShipment.ShipmentItems)
            {
                if (shipmentItem.ShipmentType == ShipmentType.Store)
                {
                    var shipmentPackage = await _uow.ShipmentPackagePrice.GetAsync(x => x.ShipmentPackagePriceId == shipmentItem.ShipmentPackagePriceId);
                    if (shipmentItem.Quantity <= 0)
                    {
                        throw new GenericException($"The quantity {shipmentItem.Quantity} for {shipmentPackage.Description} is invalid ", $"{(int)HttpStatusCode.Forbidden}");
                    }
                    if (shipmentPackage.InventoryOnHand < shipmentItem.Quantity)
                    {
                        throw new GenericException($"The quantity {shipmentItem.Quantity} being dispatched is more than the available stock .  {shipmentPackage.Description} has {shipmentPackage.InventoryOnHand} currently in store", $"{(int)HttpStatusCode.Forbidden}");
                    }
                    shipmentPackage.InventoryOnHand -= shipmentItem.Quantity;

                    var newOutflow = new ShipmentPackagingTransactions
                    {
                        ServiceCenterId = currentServiceCenterId,
                        ShipmentPackageId = shipmentPackage.ShipmentPackagePriceId,
                        Quantity = shipmentItem.Quantity,
                        Waybill = newShipment.Waybill,
                        UserId = user,
                        ReceiverServiceCenterId = newShipment.DestinationServiceCentreId,
                        PackageTransactionType = Core.Enums.PackageTransactionType.OutflowFromStore
                    };

                    packageOutflow.Add(newOutflow);
                }

            }

            _uow.ShipmentPackagingTransactions.AddRange(packageOutflow);
        }

        // Convert an object to a byte array
        private static byte[] ObjectToByteArray(HashSet<ShipmentHashDTO> obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            using (var ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                return ms.ToArray();
            }
        }

        private async Task<CustomerDTO> CreateCustomer(ShipmentDTO shipmentDTO)
        {
            var customerDTO = shipmentDTO.Customer[0];
            var customerType = shipmentDTO.CustomerType;

            if (customerDTO.UserActiveCountryId == 0)
            {
                customerDTO.UserActiveCountryId = await GetUserCountryId();
            }

            //reset rowversion
            customerDTO.RowVersion = null;

            // company
            if (CustomerType.Company.ToString() == customerType)
            {
                customerDTO.CustomerType = CustomerType.Company;
            }
            else
            {
                // individualCustomer
                customerDTO.CustomerType = CustomerType.IndividualCustomer;
            }

            var createdObject = new CustomerDTO();

            if (shipmentDTO.IsFromMobile)
            {
                createdObject = await GetAndCreateCustomer(customerDTO);
            }
            else
            {
                createdObject = await _customerService.CreateCustomer(customerDTO);
            }


            // set the customerId
            // company
            if (CustomerType.Company.ToString() == customerType)
            {
                shipmentDTO.CustomerId = createdObject.CompanyId;
            }
            else
            {
                // individualCustomer
                customerDTO.CustomerType = CustomerType.IndividualCustomer;
                shipmentDTO.CustomerId = createdObject.IndividualCustomerId;
            }

            //set the actual company type - Corporate, Ecommerce, Individual
            if (CustomerType.Company.ToString() == customerType)
            {
                var company = await _uow.Company.GetAsync(s => s.CompanyId == shipmentDTO.CustomerId);
                createdObject.CompanyStatus = company.CompanyStatus;
                if (company.CompanyType == CompanyType.Corporate)
                {
                    shipmentDTO.CompanyType = CompanyType.Corporate.ToString();
                }
                else
                {
                    shipmentDTO.CompanyType = CompanyType.Ecommerce.ToString();
                }
            }
            else
            {
                shipmentDTO.CompanyType = CustomerType.IndividualCustomer.ToString();
            }

            //set the customerCode in the shipment
            var currentCustomerObject = await _customerService.GetCustomer(shipmentDTO.CustomerId, customerDTO.CustomerType);
            shipmentDTO.CustomerCode = currentCustomerObject.CustomerCode;
            shipmentDTO.IsClassShipment = currentCustomerObject.Rank == Rank.Class ? true : false;

            return createdObject;
        }

        private async Task<CustomerDTO> GetGIGLCorporateAccount(ShipmentDTO shipmentDTO)
        {
            var giglAccount = await _customerService.GetGIGLCorporateAccount();
            shipmentDTO.CustomerId = giglAccount.CompanyId;
            shipmentDTO.CompanyType = CompanyType.Corporate.ToString();
            shipmentDTO.CustomerCode = giglAccount.CustomerCode;
            return giglAccount;
        }

        private async Task<ShipmentDTO> CreateShipment(ShipmentDTO shipmentDTO)
        {
            await _deliveryService.GetDeliveryOptionById(shipmentDTO.DeliveryOptionId);
            var destinationSC = await _centreService.GetServiceCentreById(shipmentDTO.DestinationServiceCentreId);

            //Get SuperCentre for Home Delivery
            if (shipmentDTO.PickupOptions == PickupOptions.HOMEDELIVERY)
            {
                //also check that the destination is not a hub
                if (destinationSC.IsHUB != true)
                {
                    var serviceCentreForHomeDelivery = await _centreService.GetServiceCentreForHomeDelivery(shipmentDTO.DestinationServiceCentreId);
                    shipmentDTO.DestinationServiceCentreId = serviceCentreForHomeDelivery.ServiceCentreId;
                }
            }

            // get deliveryOptionIds and set the first value in shipment
            var deliveryOptionIds = shipmentDTO.DeliveryOptionIds;
            if (deliveryOptionIds.Any())
            {
                shipmentDTO.DeliveryOptionId = deliveryOptionIds[0];
            }

            // get the current user info
            var currentUserId = await _userService.GetCurrentUserId();
            var serviceCenterIds = await _userService.GetPriviledgeServiceCenters();

            shipmentDTO.DepartureServiceCentreId = serviceCenterIds[0];
            shipmentDTO.UserId = currentUserId;

            var departureServiceCentre = new ServiceCentreDTO();

            if (shipmentDTO.IsGIGGOExtension == true)
            {
                departureServiceCentre = await _userService.GetGIGGOServiceCentre();
            }
            else
            {
                departureServiceCentre = await _centreService.GetServiceCentreById(shipmentDTO.DepartureServiceCentreId);
            }


            if (shipmentDTO.Waybill != null && !shipmentDTO.Waybill.Contains("AWR"))
            {
                if (shipmentDTO.Waybill.Contains("MWR"))
                {
                    //do nothing
                }
                else
                {
                    var waybill = await _numberGeneratorMonitorService.GenerateNextNumber(NumberGeneratorType.WaybillNumber, departureServiceCentre.Code);
                    shipmentDTO.Waybill = waybill;
                }

            }

            if (shipmentDTO.Waybill == null)
            {
                var waybill = await _numberGeneratorMonitorService.GenerateNextNumber(NumberGeneratorType.WaybillNumber, departureServiceCentre.Code);
                shipmentDTO.Waybill = waybill;
            }

            var newShipment = Mapper.Map<Shipment>(shipmentDTO);

            // set declared value of the shipment
            if (shipmentDTO.IsdeclaredVal)
            {
                newShipment.DeclarationOfValueCheck = shipmentDTO.DeclarationOfValueCheck;
            }
            else
            {
                newShipment.DeclarationOfValueCheck = null;
            }

            newShipment.ApproximateItemsWeight = 0;

            // add serial numbers to the ShipmentItems
            var serialNumber = 1;

            var numOfPackages = shipmentDTO.PackageOptionIds.Count;
            var numOfShipmentItems = newShipment.ShipmentItems.Count;

            if (numOfPackages > numOfShipmentItems)
            {
                throw new GenericException("Number of Packages should not be more then Shipment Items!", $"{(int)HttpStatusCode.BadRequest}");
            }

            if (shipmentDTO.PackageOptionIds.Any())
            {
                await UpdatePackageTransactions(shipmentDTO);
            }

            for (var i = 0; i < numOfPackages; i++)
            {
                newShipment.ShipmentItems[i].ShipmentPackagePriceId = shipmentDTO.PackageOptionIds[i];
                newShipment.ShipmentItems[i].PackageQuantity = 1;
            }

            foreach (var shipmentItem in newShipment.ShipmentItems)
            {
                shipmentItem.SerialNumber = serialNumber;

                //sum item weight 
                //check for volumetric weight
                if (shipmentItem.IsVolumetric)
                {
                    double volume = (shipmentItem.Length * shipmentItem.Height * shipmentItem.Width) / 5000;
                    double Weight = shipmentItem.Weight > volume ? shipmentItem.Weight : volume;

                    newShipment.ApproximateItemsWeight += Weight;
                }
                else
                {
                    newShipment.ApproximateItemsWeight += shipmentItem.Weight;
                }
                serialNumber++;
            }

            //do not save the child objects
            //newShipment.DepartureServiceCentre = null;
            //newShipment.DestinationServiceCentre = null;
            newShipment.DeliveryOption = null;

            //save the display value of Insurance and Vat
            newShipment.Vat = shipmentDTO.vatvalue_display;
            newShipment.DiscountValue = shipmentDTO.InvoiceDiscountValue_display;

            ////--start--///Set the DepartureCountryId and DestinationCountryId
            var departureCountry = await _uow.Country.GetCountryByServiceCentreId(shipmentDTO.DepartureServiceCentreId);
            var destinationCountry = await _uow.Country.GetCountryByServiceCentreId(shipmentDTO.DestinationServiceCentreId);

            //check if the shipment contains cod
            if (newShipment.IsCashOnDelivery == true)
            {
                //collect the cods and add to CashOnDeliveryRegisterAccount()
                var cashondeliveryentity = new CashOnDeliveryRegisterAccount
                {
                    Amount = newShipment.CashOnDeliveryAmount ?? 0,
                    CODStatusHistory = CODStatushistory.Created,
                    Description = "Cod From Sales",
                    ServiceCenterId = 0,
                    Waybill = newShipment.Waybill,
                    UserId = newShipment.UserId,
                    DepartureServiceCenterId = newShipment.DepartureServiceCentreId,
                    DestinationCountryId = destinationCountry.CountryId
                };

                _uow.CashOnDeliveryRegisterAccount.Add(cashondeliveryentity);
            }

            newShipment.DepartureCountryId = departureCountry.CountryId;
            newShipment.DestinationCountryId = destinationCountry.CountryId;
            newShipment.CurrencyRatio = departureCountry.CurrencyRatio;
            newShipment.ShipmentPickupPrice = shipmentDTO.ShipmentPickupPrice;
            ////--end--///Set the DepartureCountryId and DestinationCountryId

            //Make GiGo Shipments not available for grouping
            if (shipmentDTO.IsGIGGOExtension)
            {
                newShipment.IsGrouped = true;
            }
            _uow.Shipment.Add(newShipment);

            //save into DeliveryOptionMapping table
            foreach (var deliveryOptionId in deliveryOptionIds)
            {
                var deliveryOptionMapping = new ShipmentDeliveryOptionMapping()
                {
                    Waybill = newShipment.Waybill,
                    DeliveryOptionId = deliveryOptionId
                };
                _uow.ShipmentDeliveryOptionMapping.Add(deliveryOptionMapping);
            }

            //set before returning
            shipmentDTO.DepartureCountryId = departureCountry.CountryId;
            shipmentDTO.DestinationCountryId = destinationCountry.CountryId;

            return shipmentDTO;
        }

        private async Task<ShipmentDTO> CreateShipmentForPaymentWaiver(ShipmentDTO shipmentDTO)
        {
            await _deliveryService.GetDeliveryOptionById(shipmentDTO.DeliveryOptionId);
            var destinationSC = await _centreService.GetServiceCentreById(shipmentDTO.DestinationServiceCentreId);

            //Get SuperCentre for Home Delivery
            if (shipmentDTO.PickupOptions == PickupOptions.HOMEDELIVERY)
            {
                //also check that the destination is not a hub
                if (destinationSC.IsHUB != true)
                {
                    var serviceCentreForHomeDelivery = await _centreService.GetServiceCentreForHomeDelivery(shipmentDTO.DestinationServiceCentreId);
                    shipmentDTO.DestinationServiceCentreId = serviceCentreForHomeDelivery.ServiceCentreId;
                }
            }

            // get deliveryOptionIds and set the first value in shipment
            var deliveryOptionIds = shipmentDTO.DeliveryOptionIds;
            if (deliveryOptionIds.Any())
            {
                shipmentDTO.DeliveryOptionId = deliveryOptionIds[0];
            }

            // get the current user info
            var currentUserId = await _userService.GetCurrentUserId();
            var serviceCenterIds = await _userService.GetPriviledgeServiceCenters();

            shipmentDTO.DepartureServiceCentreId = serviceCenterIds[0];
            shipmentDTO.UserId = currentUserId;

            var departureServiceCentre = await _centreService.GetServiceCentreById(shipmentDTO.DepartureServiceCentreId);
            var waybill = await _numberGeneratorMonitorService.GenerateNextNumber(NumberGeneratorType.WaybillNumber, departureServiceCentre.Code);

            shipmentDTO.Waybill = waybill;

            await UpdateShipmentPackage(shipmentDTO);
            var newShipment = Mapper.Map<Shipment>(shipmentDTO);

            // set declared value of the shipment
            if (shipmentDTO.IsdeclaredVal)
            {
                newShipment.DeclarationOfValueCheck = shipmentDTO.DeclarationOfValueCheck;
            }
            else
            {
                newShipment.DeclarationOfValueCheck = null;
            }

            newShipment.ApproximateItemsWeight = 0;
            newShipment.isInternalShipment = true;

            // add serial numbers to the ShipmentItems
            var serialNumber = 1;
            foreach (var shipmentItem in newShipment.ShipmentItems)
            {
                shipmentItem.SerialNumber = serialNumber;

                //sum item weight
                //check for volumetric weight
                if (shipmentItem.IsVolumetric)
                {
                    double volume = (shipmentItem.Length * shipmentItem.Height * shipmentItem.Width) / 5000;
                    double Weight = shipmentItem.Weight > volume ? shipmentItem.Weight : volume;

                    newShipment.ApproximateItemsWeight += Weight;
                }
                else
                {
                    newShipment.ApproximateItemsWeight += shipmentItem.Weight;
                }

                serialNumber++;
            }

            //do not save the child objects
            //newShipment.DepartureServiceCentre = null;
            //newShipment.DestinationServiceCentre = null;
            newShipment.DeliveryOption = null;

            //save the display value of Insurance and Vat
            newShipment.Vat = shipmentDTO.vatvalue_display;
            newShipment.DiscountValue = shipmentDTO.InvoiceDiscountValue_display;

            ////--start--///Set the DepartureCountryId and DestinationCountryId
            var departureCountry = await _uow.Country.GetCountryByServiceCentreId(shipmentDTO.DepartureServiceCentreId);
            var destinationCountry = await _uow.Country.GetCountryByServiceCentreId(shipmentDTO.DestinationServiceCentreId);

            newShipment.DepartureCountryId = departureCountry.CountryId;
            newShipment.DestinationCountryId = destinationCountry.CountryId;
            newShipment.CurrencyRatio = departureCountry.CurrencyRatio;
            newShipment.ShipmentPickupPrice = shipmentDTO.ShipmentPickupPrice;
            ////--end--///Set the DepartureCountryId and DestinationCountryId

            _uow.Shipment.Add(newShipment);

            //save into DeliveryOptionMapping table
            foreach (var deliveryOptionId in deliveryOptionIds)
            {
                var deliveryOptionMapping = new ShipmentDeliveryOptionMapping()
                {
                    Waybill = newShipment.Waybill,
                    DeliveryOptionId = deliveryOptionId
                };
                _uow.ShipmentDeliveryOptionMapping.Add(deliveryOptionMapping);
            }

            //set before returning
            shipmentDTO.DepartureCountryId = departureCountry.CountryId;
            shipmentDTO.DestinationCountryId = destinationCountry.CountryId;

            return shipmentDTO;
        }

        private async Task<string> CreateInvoice(ShipmentDTO shipmentDTO)
        {
            var invoice = new Invoice();
            var departureServiceCentre = await _centreService.GetServiceCentreById(shipmentDTO.DepartureServiceCentreId);
            var invoiceNo = await _numberGeneratorMonitorService.GenerateNextNumber(NumberGeneratorType.Invoice, departureServiceCentre.Code);

            var settlementPeriod = 0;
            if (shipmentDTO.CustomerType == CustomerType.Company.ToString())
            {
                var company = await _companyService.GetCompanyById(shipmentDTO.CustomerId);
                settlementPeriod = company.SettlementPeriod;
            }

            //added this check for Mobile Shipments
            if (shipmentDTO.IsFromMobile == true)
            {
                invoice = new Invoice()
                {
                    InvoiceNo = invoiceNo,
                    Amount = shipmentDTO.GrandTotal,
                    PaymentStatus = PaymentStatus.Paid,
                    Waybill = shipmentDTO.Waybill,
                    PaymentDate = DateTime.Now,
                    PaymentMethod = PaymentType.Wallet.ToString(),
                    DueDate = DateTime.Now.AddDays(settlementPeriod),
                    IsInternational = shipmentDTO.IsInternational,
                    ServiceCentreId = departureServiceCentre.ServiceCentreId,
                    CountryId = shipmentDTO.DepartureCountryId
                };
            }
            else
            {
                invoice = new Invoice()
                {
                    InvoiceNo = invoiceNo,
                    Amount = shipmentDTO.GrandTotal,
                    PaymentStatus = (shipmentDTO.PaymentStatus == PaymentStatus.Paid) ? shipmentDTO.PaymentStatus : PaymentStatus.Pending,
                    PaymentMethod = (string.IsNullOrEmpty(shipmentDTO.PaymentMethod)) ? "" : shipmentDTO.PaymentMethod,
                    Waybill = shipmentDTO.Waybill,
                    PaymentDate = DateTime.Now,
                    DueDate = DateTime.Now.AddDays(settlementPeriod),
                    IsInternational = shipmentDTO.IsInternational,
                    ServiceCentreId = departureServiceCentre.ServiceCentreId,
                    CountryId = shipmentDTO.DepartureCountryId
                };
            }

            _uow.Invoice.Add(invoice);
            return invoiceNo;
        }

        private async Task CreateInvoiceForPaymentWaiver(ShipmentDTO shipmentDTO)
        {
            var departureServiceCentre = await _centreService.GetServiceCentreById(shipmentDTO.DepartureServiceCentreId);
            var invoiceNo = await _numberGeneratorMonitorService.GenerateNextNumber(NumberGeneratorType.Invoice, departureServiceCentre.Code);

            var settlementPeriod = 0;
            var invoice = new Invoice()
            {
                InvoiceNo = invoiceNo,
                Amount = shipmentDTO.GrandTotal,
                PaymentStatus = PaymentStatus.Paid,
                Waybill = shipmentDTO.Waybill,
                PaymentDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(settlementPeriod),
                IsInternational = shipmentDTO.IsInternational,
                ServiceCentreId = departureServiceCentre.ServiceCentreId,
                CountryId = shipmentDTO.DepartureCountryId
            };

            _uow.Invoice.Add(invoice);
        }

        private void CreateGeneralLedger(ShipmentDTO shipmentDTO)
        {
            var generalLedger = new GeneralLedger()
            {
                DateOfEntry = DateTime.Now,
                ServiceCentreId = shipmentDTO.DepartureServiceCentreId,
                UserId = shipmentDTO.UserId,
                Amount = shipmentDTO.GrandTotal,
                CreditDebitType = CreditDebitType.Credit,
                Description = "Payment for Shipment",
                IsDeferred = true,
                Waybill = shipmentDTO.Waybill,
                IsInternational = shipmentDTO.IsInternational,
                CountryId = shipmentDTO.DepartureCountryId
                //ClientNodeId = shipment.c
            };

            _uow.GeneralLedger.Add(generalLedger);
        }

        private void CreateGeneralLedgerForPaymentWaiverShipment(ShipmentDTO shipmentDTO)
        {
            var generalLedger = new GeneralLedger()
            {
                DateOfEntry = DateTime.Now,
                PaymentType = PaymentType.Waiver,
                ServiceCentreId = shipmentDTO.DepartureServiceCentreId,
                UserId = shipmentDTO.UserId,
                Amount = shipmentDTO.GrandTotal,
                CreditDebitType = CreditDebitType.Credit,
                Description = "Payment for Shipment",
                IsDeferred = true,
                Waybill = shipmentDTO.Waybill,
                IsInternational = shipmentDTO.IsInternational,
                CountryId = shipmentDTO.DepartureCountryId
            };

            _uow.GeneralLedger.Add(generalLedger);
        }

        public async Task<int> GetUserCountryId()
        {
            int userCountryId = 1; //default country

            var currentUserId = await _userService.GetCurrentUserId();
            var currentUser = await _userService.GetUserById(currentUserId);

            if (currentUser.UserActiveCountryId > 0)
            {
                userCountryId = currentUser.UserActiveCountryId;
            }

            return userCountryId;
        }

        //This is used because I don't want an Exception to be thrown when calling it
        public async Task<Shipment> GetShipmentForScan(string waybill)
        {
            var shipment = await _uow.Shipment.GetAsync(x => x.Waybill.Equals(waybill), "ShipmentItems");
            return shipment;
        }

        public async Task<List<InvoiceViewDTO>> GetUnGroupedWaybillsForServiceCentre(FilterOptionsDto filterOptionsDto, bool filterByDestinationSC = false)
        {
            try
            {
                //1. get shipments for that Service Centre
                var serviceCenters = await _userService.GetPriviledgeServiceCenters();
                var shipmentsQueryable = _uow.Invoice.GetAllFromInvoiceAndShipments().Where(s => s.IsShipmentCollected == false && s.IsGrouped == false);

                //apply filters for Service Centre
                if (serviceCenters.Length > 0)
                {
                    shipmentsQueryable = shipmentsQueryable.Where(s => serviceCenters.Contains(s.DepartureServiceCentreId));
                }

                //filter by Local or International Shipment
                if (filterOptionsDto.IsInternational != null)
                {
                    shipmentsQueryable = shipmentsQueryable.Where(s => s.IsInternational == filterOptionsDto.IsInternational);
                }

                //filter by DestinationServiceCentreId
                var filter = filterOptionsDto.filter;
                var filterValue = filterOptionsDto.filterValue;
                //int destinationSCId = 0;
                var boolResult = int.TryParse(filterValue, out int destinationSCId);
                if (!string.IsNullOrEmpty(filter) && !string.IsNullOrEmpty(filterValue))
                {
                    if (filter == "DestinationServiceCentreId" && boolResult)
                    {
                        shipmentsQueryable = shipmentsQueryable.Where(s => s.DestinationServiceCentreId == destinationSCId);
                    }
                }

                var shipmentsBySC = shipmentsQueryable.ToList();

                //2. get only paid shipments from Invoice for Individuals
                // and allow Ecommerce and Corporate customers to be grouped
                //var paidShipments = new List<InvoiceView>();
                var finalUngroupedList = new HashSet<InvoiceView>();

                foreach (var shipmentItem in shipmentsBySC)
                {
                    //var invoice = shipmentItem;

                    if (shipmentItem.PaymentStatus == PaymentStatus.Paid || shipmentItem.CompanyType == CompanyType.Corporate.ToString())
                    {
                        finalUngroupedList.Add(shipmentItem);
                    }
                }

                //3. get all grouped waybills for that Service Centre
                //var groupWayBillNumberMappings = await _uow.GroupWaybillNumberMapping.GetGroupWaybillMappingWaybills(serviceCenters);

                //4. filter the two lists
                //var groupedWaybillsAsStringList = groupWayBillNumberMappings.ToList().Select(a => a.WaybillNumber);
                //var groupedWaybillsAsHashSet = new HashSet<string>(groupWayBillNumberMappings);
                //var ungroupedWaybills = paidShipments.Where(s => !groupedWaybillsAsHashSet.Contains(s.Waybill)).ToList();
                //var ungroupedWaybills = paidShipments;

                //new solution
                //1. Get Transit Waybill that is not completed, not group and service centre belong to login user
                //2. Loop through output of 1 and get the shipment details, then add it to the ungrouped
                //var finalUngroupedList = new List<InvoiceView>();

                //foreach (var item in ungroupedWaybills)
                //{
                //    finalUngroupedList.Add(item);
                //}
                int currentServiceCentre = serviceCenters[0];

                var allTransitWaybillNumberList = _uow.TransitWaybillNumber.GetAllAsQueryable()
                    .Where(x => x.ServiceCentreId == currentServiceCentre && x.IsGrouped == false && x.IsTransitCompleted == false).ToList();

                foreach (var item in allTransitWaybillNumberList)
                {
                    var shipment = _uow.Invoice.GetAllFromInvoiceAndShipments()
                        .FirstOrDefault(s => s.IsShipmentCollected == false && s.Waybill == item.WaybillNumber && s.DestinationServiceCentreId == destinationSCId);

                    if (shipment != null)
                    {
                        finalUngroupedList.Add(shipment);
                    }
                }

                //7.
                var finalList = new List<InvoiceViewDTO>();

                //var allServiceCenters = await _centreService.GetServiceCentres();
                var allServiceCenters = await _uow.ServiceCentre.GetServiceCentresWithoutStation();

                foreach (var finalUngroupedItem in finalUngroupedList)
                {
                    //var shipment = finalUngroupedItem;
                    if (finalUngroupedItem != null)
                    {
                        var invoiceViewDTO = Mapper.Map<InvoiceViewDTO>(finalUngroupedItem);

                        invoiceViewDTO.DepartureServiceCentre = allServiceCenters.Where(x => x.ServiceCentreId == finalUngroupedItem.DepartureServiceCentreId).Select(s => new ServiceCentreDTO
                        {
                            Name = s.Name,
                            Code = s.Code,
                            ServiceCentreId = s.ServiceCentreId
                        }).FirstOrDefault();

                        invoiceViewDTO.DestinationServiceCentre = allServiceCenters.Where(x => x.ServiceCentreId == finalUngroupedItem.DestinationServiceCentreId).Select(s => new ServiceCentreDTO
                        {
                            Name = s.Name,
                            Code = s.Code,
                            ServiceCentreId = s.ServiceCentreId
                        }).FirstOrDefault();

                        finalList.Add(invoiceViewDTO);
                    }
                }

                return finalList;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<InvoiceView>> GetUnGroupedWaybillsForServiceCentreDropDown(FilterOptionsDto filterOptionsDto, bool filterByDestinationSC = false)
        {
            try
            {
                //1. get shipments for that Service Centre
                var serviceCenters = await _userService.GetPriviledgeServiceCenters();

                var shipmentsQueryable = _uow.Invoice.GetAllFromInvoiceAndShipments()
                    .Where(s => s.IsShipmentCollected == false && s.IsGrouped == false && (s.PaymentStatus == PaymentStatus.Paid || s.CompanyType == CompanyType.Corporate.ToString()));

                //apply filters for Service Centre
                if (serviceCenters.Length > 0)
                {
                    shipmentsQueryable = shipmentsQueryable.Where(s => serviceCenters.Contains(s.DepartureServiceCentreId));
                }

                //filter by Local or International Shipment
                if (filterOptionsDto.IsInternational != null)
                {
                    shipmentsQueryable = shipmentsQueryable.Where(s => s.IsInternational == filterOptionsDto.IsInternational);
                }

                //filter by DestinationServiceCentreId
                var filter = filterOptionsDto.filter;
                var filterValue = filterOptionsDto.filterValue;
                var boolResult = int.TryParse(filterValue, out int destinationSCId);
                if (!string.IsNullOrEmpty(filter) && !string.IsNullOrEmpty(filterValue))
                {
                    if (filter == "DestinationServiceCentreId" && boolResult)
                    {
                        shipmentsQueryable = shipmentsQueryable.Where(s => s.DestinationServiceCentreId == destinationSCId);
                    }
                }

                var shipmentsBySC = shipmentsQueryable.ToList();

                var allTransitWaybillNumberList = _uow.TransitWaybillNumber.GetAllAsQueryable()
                    .Where(x => serviceCenters.Contains(x.ServiceCentreId) && x.IsGrouped == false && x.IsTransitCompleted == false).ToList();

                List<string> transitWaybills = allTransitWaybillNumberList.Select(x => x.WaybillNumber).ToList();

                if (allTransitWaybillNumberList.Any())
                {
                    var shipment = _uow.Invoice.GetAllFromInvoiceAndShipments().Where(s => s.IsShipmentCollected == false && transitWaybills.Contains(s.Waybill)).ToList();

                    if (shipment.Any())
                    {
                        shipmentsBySC.AddRange(shipment);
                    }
                }

                return shipmentsBySC;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<ServiceCentreDTO>> GetUnGroupMappingServiceCentres()
        {
            try
            {
                var filterOptionsDto = new FilterOptionsDto
                {
                    count = 1000000,
                    page = 1,
                    sortorder = "0"
                };

                var ungroupedWaybills = await GetUnGroupedWaybillsForServiceCentreDropDown(filterOptionsDto);

                //Get only service centre without station
                var allServiceCenters = await _centreService.GetServiceCentres();

                var grp = new HashSet<int>();

                foreach (var item in ungroupedWaybills)
                {
                    if (item?.DestinationServiceCentreId > 0)
                    {
                        grp.Add(item.DestinationServiceCentreId);
                    }
                }

                var ungroupedServiceCentres = allServiceCenters.ToList().Where(
                    s => grp.Contains(s.ServiceCentreId)).ToList();

                return ungroupedServiceCentres;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<GroupWaybillNumberDTO>> GetUnmappedGroupedWaybillsForServiceCentre(FilterOptionsDto filterOptionsDto)
        {
            try
            {
                var serviceCenters = await _userService.GetPriviledgeServiceCenters();

                //Get Groupwaybill not yet manifest for the login user
                var groupedWaybillsBySc = _uow.GroupWaybillNumber.GetAllAsQueryable().Where(x => x.HasManifest == false);

                if (serviceCenters.Length > 0)
                {
                    groupedWaybillsBySc = groupedWaybillsBySc.Where(s => serviceCenters.Contains(s.DepartureServiceCentreId));
                }

                //Filter it by the destination service centre send from filter option
                var filter = filterOptionsDto.filter;
                int filterValue = Convert.ToInt32(filterOptionsDto.filterValue);
                if (!string.IsNullOrEmpty(filter) && filterValue > 0)
                {
                    groupedWaybillsBySc = groupedWaybillsBySc.Where(s => s.ServiceCentreId == filterValue);
                }

                var result = groupedWaybillsBySc.ToList();

                var resultDTO = Mapper.Map<List<GroupWaybillNumberDTO>>(result);

                var DestinationServiceCentre = await _uow.ServiceCentre.GetAsync(filterValue);

                foreach (var item in resultDTO)
                {
                    item.DestinationServiceCentre = DestinationServiceCentre;
                }

                return resultDTO;
            }
            catch (Exception)
            {
                throw;
            }
        }

        //Movement Manifest
        public async Task<List<ManifestDTO>> GetManifestForMovementManifestServiceCentre(MovementManifestFilterCriteria dateFilterCriteria)
        {
            try
            {
                //get startDate and endDate
                var queryDate = dateFilterCriteria.getStartDateAndEndDate();
                var startDate = queryDate.Item1;
                var endDate = queryDate.Item2;

                var serviceCenters = await _userService.GetPriviledgeServiceCenters();
                var manifests = _uow.Manifest.GetAllAsQueryable().Where(x => x.IsDispatched == true && x.MovementStatus == MovementStatus.InProgress);

                if (serviceCenters.Length > 0)
                {
                    manifests = manifests.Where(s => serviceCenters.Contains(s.DepartureServiceCentreId));
                }
                var result = manifests.ToList();
                //Filter it by the destination service centre send from filter option
                int filterValue = Convert.ToInt32(dateFilterCriteria.filterValue);
                if (filterValue > 0 && filterValue != 99999)
                {
                    manifests = manifests.Where(s => s.DestinationServiceCentreId == filterValue);
                }

                var resultDTO = Mapper.Map<List<ManifestDTO>>(result);

                if (filterValue != 99999)
                {
                    var destinationServiceCentre = await _uow.ServiceCentre.GetAsync(filterValue);
                    var destinationServiceCentreDTO = Mapper.Map<ServiceCentreDTO>(destinationServiceCentre);

                    foreach (var item in resultDTO)
                    {
                        item.DestinationServiceCentre = destinationServiceCentreDTO;
                    }
                }
                return resultDTO;
            }
            catch (Exception)
            {
                throw;
            }
        }

        //Super Manifest
        public async Task<List<ManifestDTO>> GetUnmappedManifestForServiceCentre(ShipmentCollectionFilterCriteria dateFilterCriteria)
        {
            try
            {
                //get startDate and endDate
                var queryDate = dateFilterCriteria.getStartDateAndEndDate();
                var startDate = queryDate.Item1;
                var endDate = queryDate.Item2;

                var serviceCenters = await _userService.GetPriviledgeServiceCenters();

                var manifests = _uow.Manifest.GetAllAsQueryable().Where(x => (x.SuperManifestStatus == SuperManifestStatus.ArrivedScan || x.SuperManifestStatus == SuperManifestStatus.Pending)
                                                                    && x.DateModified >= startDate && x.DateModified < endDate);

                if (serviceCenters.Length > 0)
                {
                    manifests = manifests.Where(s => serviceCenters.Contains(s.DepartureServiceCentreId));
                }

                //Filter it by the destination service centre send from filter option
                int filterValue = Convert.ToInt32(dateFilterCriteria.ServiceCentreId);
                if (filterValue > 0 && filterValue != 99999)
                {
                    manifests = manifests.Where(s => s.DestinationServiceCentreId == filterValue);
                }

                var result = manifests.ToList();

                var resultDTO = Mapper.Map<List<ManifestDTO>>(result);

                if (filterValue != 99999)
                {
                    var destinationServiceCentre = await _uow.ServiceCentre.GetAsync(filterValue);
                    var destinationServiceCentreDTO = Mapper.Map<ServiceCentreDTO>(destinationServiceCentre);

                    foreach (var item in resultDTO)
                    {
                        item.DestinationServiceCentre = destinationServiceCentreDTO;
                    }
                }
                return resultDTO;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<ServiceCentreDTO>> GetUnmappedManifestServiceCentres()
        {
            try
            {
                // get groupedWaybills that have not been mapped to a manifest for that Service Centre
                var serviceCenters = await _userService.GetPriviledgeServiceCenters();
                var groupwaybills = _uow.GroupWaybillNumber.GetAllAsQueryable().Where(x => x.HasManifest == false);

                if (serviceCenters.Length > 0)
                {
                    groupwaybills = groupwaybills.Where(s => serviceCenters.Contains(s.DepartureServiceCentreId));
                }

                //Filter the service centre details using the destination of the waybill
                var allServiceCenters = _uow.ServiceCentre.GetAllAsQueryable();
                var result = allServiceCenters.Where(s => groupwaybills.Any(x => x.ServiceCentreId == s.ServiceCentreId)).Select(p => p.ServiceCentreId).ToList();

                //Fetch all Service Centre including their Station Detail into Memory
                var allServiceCenterDTOs = await _centreService.GetServiceCentres();

                var unmappedGroupServiceCentres = allServiceCenterDTOs.Where(s => result.Any(r => r == s.ServiceCentreId));

                return unmappedGroupServiceCentres.ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> CheckReleaseMovementManifest(string movementManifestCode)
        {
            try
            {
                var movementManifest = await _uow.MovementManifestNumber.FindAsync(x => x.MovementManifestCode == movementManifestCode);
                var ManifestNumber = movementManifest.FirstOrDefault();

                if (ManifestNumber.IsDriverValid == false && ManifestNumber.MovementStatus != MovementStatus.EnRoute)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> ReleaseMovementManifest(ReleaseMovementManifestDto valMovementManifest)
        {
            try
            {
                var retVal = false;
                if (valMovementManifest.flag == MovementManifestActivationTypes.AgentActivation)
                {
                    var movementManifest = await _uow.MovementManifestNumber.FindAsync(x => x.MovementManifestCode == valMovementManifest.movementManifestCode);
                    var ManifestNumber = movementManifest.FirstOrDefault();

                    if (ManifestNumber.DestinationServiceCentreCode == valMovementManifest.code)
                    {
                        ManifestNumber.IsDriverValid = true;
                        ManifestNumber.MovementStatus = MovementStatus.ProcessEnded;
                        await _uow.CompleteAsync();
                        retVal = true;
                    }
                    else
                    {
                        throw new Exception("Sorry, The Code is invalid for releasing this shipment");
                    }
                }

                if (valMovementManifest.flag == MovementManifestActivationTypes.DispatchActivation)
                {
                    var movementManifest = await _uow.MovementManifestNumber.FindAsync(x => x.MovementManifestCode == valMovementManifest.movementManifestCode);
                    var ManifestNumber = movementManifest.FirstOrDefault();

                    if (ManifestNumber.DriverCode == valMovementManifest.code)
                    {
                        ManifestNumber.IsDestinationServiceCentreValid = true;
                        ManifestNumber.MovementStatus = MovementStatus.ProcessEnded;
                        await _uow.CompleteAsync();
                        retVal = true;
                    }
                    else
                    {
                        throw new Exception("Sorry, The Code is invalid for releasing this shipment");
                    }
                }

                return retVal;

            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<List<ServiceCentreDTO>> GetUnmappedMovementManifestServiceCentres()
        {
            try
            {
                // get groupedWaybills that have not been mapped to a manifest for that Service Centre
                var serviceCenters = await _userService.GetPriviledgeServiceCenters();
                var ManifestNumbers = _uow.Manifest.GetAllAsQueryable().Where(x => x.MovementStatus == MovementStatus.InProgress && x.IsDispatched == true);

                if (serviceCenters.Length > 0)
                {
                    ManifestNumbers = ManifestNumbers.Where(s => serviceCenters.Contains(s.DepartureServiceCentreId));
                }

                var ManifestNumbersResult = ManifestNumbers.ToList();

                //Filter the service centre details using the destination of the waybill
                var allServiceCenters = _uow.ServiceCentre.GetAllAsQueryable();

                var result = allServiceCenters.Where(s => ManifestNumbers.Any(x => x.DestinationServiceCentreId == s.ServiceCentreId)).Select(p => p.ServiceCentreId).ToList(); var re = allServiceCenters.Where(a => ManifestNumbers.Any(b => b.DepartureServiceCentreId == a.ServiceCentreId)).ToList();

                //Fetch all Service Centre including their Station Detail into Memory
                var allServiceCenterDTOs = await _centreService.GetServiceCentres();

                var unmappedGroupServiceCentres = allServiceCenterDTOs.Where(s => result.Any(r => r == s.ServiceCentreId));

                return unmappedGroupServiceCentres.ToList();
            }
            catch (Exception)
            {
                throw;
            }
        }
        //For Super Manifest
        public async Task<List<ServiceCentreDTO>> GetUnmappedManifestServiceCentresForSuperManifest()
        {
            try
            {
                var serviceCenters = await _userService.GetPriviledgeServiceCenters();

                var manifests = _uow.Manifest.GetAllAsQueryable().Where(x => x.SuperManifestStatus == SuperManifestStatus.ArrivedScan || x.SuperManifestStatus == SuperManifestStatus.Pending);

                if (serviceCenters.Length > 0)
                {
                    manifests = manifests.Where(s => serviceCenters.Contains(s.DepartureServiceCentreId));
                }

                //Filter the service centre details using the destination of the waybill
                var allServiceCenters = _uow.ServiceCentre.GetAllAsQueryable();
                var result = allServiceCenters.Where(s => manifests.Any(x => x.DestinationServiceCentreId == s.ServiceCentreId)).Select(p => p.ServiceCentreId).ToList();

                var resultWithoutDest = allServiceCenters.Where(s => manifests.Any(x => x.DestinationServiceCentreId == 0)).Select(p => p.ServiceCentreId).ToList();

                //Fetch all Service Centre including their Station Detail into Memory
                var allServiceCenterDTOs = await _centreService.GetServiceCentres();

                var unmappedGroupServiceCentres = allServiceCenterDTOs.Where(s => result.Any(r => r == s.ServiceCentreId)).ToList();

                if (resultWithoutDest.Any())
                {
                    var virtualServiceCentreDTO = new ServiceCentreDTO
                    {
                        Name = "Others",
                        ServiceCentreId = 99999,
                        StationName = "Others"
                    };

                    //add it to the last element
                    unmappedGroupServiceCentres.Add(virtualServiceCentreDTO);
                }

                return unmappedGroupServiceCentres;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<DomesticRouteZoneMapDTO> GetZone(int destinationServiceCentre)
        {
            // use currentUser login servicecentre
            var serviceCenters = await _userService.GetPriviledgeServiceCenters();
            if (serviceCenters.Length > 1)
            {
                throw new GenericException("This user is assign to more than one(1) Service Centre  ");
            }

            var zone = await _domesticRouteZoneMapService.GetZone(serviceCenters[0], destinationServiceCentre);
            return zone;
        }

        public async Task<CountryRouteZoneMapDTO> GetCountryZone(int destinationCountry)
        {
            // use currentUser login servicecentre
            var serviceCenters = await _userService.GetPriviledgeServiceCenters();
            if (serviceCenters.Length > 1)
            {
                throw new GenericException("This user is assign to more than one(1) Service Centre  ");
            }

            var serviceCentreDetail = await _centreService.GetServiceCentreById(serviceCenters[0]);

            var zone = await _countryRouteZoneMapService.GetZone(serviceCentreDetail.CountryId, destinationCountry);
            return zone;
        }


        public async Task<ServiceCentreDTO> getServiceCenterById(int ServiceCenterId)
        {
            return await _centreService.GetServiceCentreById(ServiceCenterId);
        }

        public async Task<DailySalesDTO> GetDailySales(AccountFilterCriteria accountFilterCriteria)
        {
            //filter by User Active Country
            var userActiveCountry = await _userService.GetUserActiveCountry();
            accountFilterCriteria.CountryId = userActiveCountry.CountryId;


            //set defaults
            if (accountFilterCriteria.StartDate == null)
            {
                accountFilterCriteria.StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            }

            if (accountFilterCriteria.EndDate == null)
            {
                accountFilterCriteria.EndDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            }

            var serviceCenterIds = await _userService.GetPriviledgeServiceCenters();
            var invoices = await _uow.Invoice.GetInvoicesFromViewAsyncFromSP(accountFilterCriteria, serviceCenterIds);

            //get customer details
            foreach (var item in invoices)
            {
                //get CustomerDetails
                if (item.CustomerType.Contains("Individual"))
                {
                    item.CustomerType = CustomerType.IndividualCustomer.ToString();
                }
                CustomerType customerType = (CustomerType)Enum.Parse(typeof(CustomerType), item.CustomerType);
                //var customerDetails = await GetCustomer(item.CustomerId, customerType);
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
            }

            var dailySalesDTO = new DailySalesDTO()
            {
                StartDate = (DateTime)accountFilterCriteria.StartDate,
                EndDate = (DateTime)accountFilterCriteria.EndDate,
                Invoices = invoices,
                SalesCount = invoices.Count,
                TotalSales = invoices.Sum(s => s.Amount)
            };

            return dailySalesDTO;
        }

        public async Task<ColoredInvoiceMonitorDTO> GetShipmentMonitor(AccountFilterCriteria accountFilterCriteria)
        {
            //filter by User Active Country
            var userActiveCountry = await _userService.GetUserActiveCountry();
            accountFilterCriteria.CountryId = userActiveCountry.CountryId;

            var serviceCenterIds = await _userService.GetPriviledgeServiceCenters();
            var results1 = await _uow.Invoice.GetShipmentMonitorSetSP(accountFilterCriteria, serviceCenterIds);

            var results = new List<InvoiceMonitorDTO>();

            if (serviceCenterIds.Length > 0)
            {
                results = results1.Where(s => serviceCenterIds.Contains(s.DepartureServiceCentreId)).ToList();
            }
            else
            {
                results = results1;
            }

            var result = new MulitipleInvoiceMonitorDTO()
            {
                ShipmentCreated = results
            };

            var shipmentscreated = result.ShipmentCreated;
            var obj = ReturnShipmentCreated(shipmentscreated, accountFilterCriteria);

            return obj;
        }

        public async Task<ColoredInvoiceMonitorDTO> GetShipmentMonitorx(AccountFilterCriteria accountFilterCriteria)
        {
            //filter by User Active Country
            var userActiveCountry = await _userService.GetUserActiveCountry();
            accountFilterCriteria.CountryId = userActiveCountry.CountryId;

            var serviceCenterIds = await _userService.GetPriviledgeServiceCenters();
            var results1 = await _uow.Invoice.GetShipmentMonitorSetSPExpected(accountFilterCriteria, serviceCenterIds);
            var collectionResults1 = await _uow.Invoice.GetShipmentWaitingForCollection(accountFilterCriteria, serviceCenterIds);

            var results = new List<InvoiceMonitorDTO>();
            var collectionResults = new List<InvoiceMonitorDTO>();

            if (serviceCenterIds.Length > 0)
            {
                results = results1.Where(s => serviceCenterIds.Contains(s.DestinationServiceCentreId)).ToList();
                collectionResults = collectionResults1.Where(s => serviceCenterIds.Contains(s.DestinationServiceCentreId)).ToList();
            }
            else
            {
                results = results1;
                collectionResults = collectionResults1;
            }

            var result = new MulitipleInvoiceMonitorDTO()
            {
                ShipmentCreated = results,
                ShipmentCollection = collectionResults
            };

            var shipmentsexpected = result.ShipmentCreated;
            var shipmentscollected = result.ShipmentCollection;

            var obj = ReturnShipmentCreatedx(shipmentsexpected, shipmentscollected, accountFilterCriteria);

            return obj;
        }

        public async Task<Object[]> GetShipmentCreatedByDateMonitor(AccountFilterCriteria accountFilterCriteria, LimitDates Limitdates)
        {
            //filter by User Active Country
            var userActiveCountry = await _userService.GetUserActiveCountry();
            accountFilterCriteria.CountryId = userActiveCountry.CountryId;

            var serviceCenterIds = await _userService.GetPriviledgeServiceCenters();
            var results1 = await _uow.Invoice.GetShipmentMonitorSetSP(accountFilterCriteria, serviceCenterIds);

            var results = new List<InvoiceMonitorDTO>();

            if (serviceCenterIds.Length > 0)
            {
                results = results1.Where(s => serviceCenterIds.Contains(s.DepartureServiceCentreId)).ToList();
            }
            else
            {
                results = results1;
            }

            var shipmentscreated = results;

            var obj = ReturnShipmentCreatedByLimitDates(shipmentscreated, accountFilterCriteria, Limitdates);

            return obj;
        }

        public async Task<Object[]> GetShipmentCreatedByDateMonitorx(AccountFilterCriteria accountFilterCriteria, LimitDates Limitdates)
        {
            //filter by User Active Country
            var userActiveCountry = await _userService.GetUserActiveCountry();
            accountFilterCriteria.CountryId = userActiveCountry.CountryId;

            var serviceCenterIds = await _userService.GetPriviledgeServiceCenters();

            var results = new List<InvoiceMonitorDTO>();

            if ((Limitdates.StartLimit == 4 && Limitdates.EndLimit == 5) || (Limitdates.StartLimit == 5 && Limitdates.EndLimit == 6))
            {

                results = await _uow.Invoice.GetShipmentWaitingForCollection(accountFilterCriteria, serviceCenterIds);
            }
            else
            {
                results = await _uow.Invoice.GetShipmentMonitorSetSPExpected(accountFilterCriteria, serviceCenterIds);
            }

            if (serviceCenterIds.Length > 0)
            {
                results = results.Where(s => serviceCenterIds.Contains(s.DestinationServiceCentreId)).ToList();
            }

            var obj = ReturnShipmentCreatedByLimitDates(results, accountFilterCriteria, Limitdates);

            return obj;
        }

        public async Task<List<InvoiceViewDTOUNGROUPED2>> GetShipmentWaybillsByDateMonitor(AccountFilterCriteria accountFilterCriteria, LimitDates Limitdates)
        {
            var dataValues = ReturnStartAndEndDateLimit(accountFilterCriteria, Limitdates);
            var LimitStartDate = dataValues.Item1;
            var LimitEndDate = dataValues.Item2;

            //filter by User Active Country
            var userActiveCountry = await _userService.GetUserActiveCountry();
            accountFilterCriteria.CountryId = userActiveCountry.CountryId;

            var serviceCenterIds = await _userService.GetPriviledgeServiceCenters();
            var results1 = await _uow.Invoice.GetShipmentMonitorSetSP_NotGrouped(accountFilterCriteria, serviceCenterIds);

            var results = new List<InvoiceViewDTOUNGROUPED>();

            if (serviceCenterIds.Length > 0)
            {
                results = results1.Where(s => serviceCenterIds.Contains(s.DepartureServiceCentreId)).ToList();
            }
            else
            {
                results = results1;
            }

            var v = new List<InvoiceViewDTOUNGROUPED2>();

            if (Limitdates.StartLimit == 1 && Limitdates.EndLimit == 2)
            {
                v = (from list in results
                     where list.DateCreated > LimitStartDate && list.DateCreated <= LimitEndDate && list.Name == Limitdates.ScName
                     select new InvoiceViewDTOUNGROUPED2()
                     {
                         DestinationServiceCentreName = list.Name,
                         Waybill = list.Waybill,
                         ShipmentDate = list.DateCreated,
                         PaymentMethod = list.PaymentMethod,
                         Amount = list.Amount
                     }).ToList();
            }
            else if (Limitdates.StartLimit == 2 && Limitdates.EndLimit == 3)
            {
                v = (from list in results
                     where list.DateCreated > LimitStartDate && list.DateCreated <= LimitEndDate && list.Name == Limitdates.ScName
                     select new InvoiceViewDTOUNGROUPED2()
                     {
                         DestinationServiceCentreName = list.Name,
                         Waybill = list.Waybill,
                         ShipmentDate = list.DateCreated,
                         PaymentMethod = list.PaymentMethod,
                         Amount = list.Amount
                     }).ToList();
            }
            else if (Limitdates.StartLimit == 3 && Limitdates.EndLimit == 4)
            {
                v = (from list in results
                     where list.DateCreated > LimitStartDate && list.DateCreated <= LimitEndDate && list.Name == Limitdates.ScName
                     select new InvoiceViewDTOUNGROUPED2()
                     {
                         DestinationServiceCentreName = list.Name,
                         Waybill = list.Waybill,
                         ShipmentDate = list.DateCreated,
                         PaymentMethod = list.PaymentMethod,
                         Amount = list.Amount
                     }).ToList();
            }

            return v;
        }

        public async Task<List<InvoiceViewDTOUNGROUPED2>> GetShipmentWaybillsByDateMonitorx(AccountFilterCriteria accountFilterCriteria, LimitDates Limitdates)
        {
            var dataValues = ReturnStartAndEndDateLimit(accountFilterCriteria, Limitdates);
            var LimitStartDate = dataValues.Item1;
            var LimitEndDate = dataValues.Item2;

            //filter by User Active Country
            var userActiveCountry = await _userService.GetUserActiveCountry();
            accountFilterCriteria.CountryId = userActiveCountry.CountryId;

            var serviceCenterIds = await _userService.GetPriviledgeServiceCenters();

            var results = new List<InvoiceViewDTOUNGROUPED>();

            if ((Limitdates.StartLimit == 4 && Limitdates.EndLimit == 5) || (Limitdates.StartLimit == 5 && Limitdates.EndLimit == 6))
            {
                results = await _uow.Invoice.GetShipmentWaitingForCollection_NotGrouped(accountFilterCriteria, serviceCenterIds);
            }
            else
            {
                results = await _uow.Invoice.GetShipmentMonitorSetSP_NotGroupedx(accountFilterCriteria, serviceCenterIds);
            }

            if (serviceCenterIds.Length > 0)
            {
                results = results.Where(s => serviceCenterIds.Contains(s.DestinationServiceCentreId)).ToList();
            }

            var v = new List<InvoiceViewDTOUNGROUPED2>();

            if (Limitdates.StartLimit == 1 && Limitdates.EndLimit == 2)
            {
                v = (from list in results
                     where list.DateCreated > LimitStartDate && list.DateCreated <= LimitEndDate && list.Name == Limitdates.ScName
                     select new InvoiceViewDTOUNGROUPED2()
                     {
                         DestinationServiceCentreName = list.Name,
                         Waybill = list.Waybill,
                         ShipmentDate = list.DateCreated,
                         PaymentMethod = list.PaymentMethod,
                         Amount = list.Amount
                     }).ToList();
            }
            else if (Limitdates.StartLimit == 2 && Limitdates.EndLimit == 3)
            {
                v = (from list in results
                     where list.DateCreated > LimitStartDate && list.DateCreated <= LimitEndDate && list.Name == Limitdates.ScName
                     select new InvoiceViewDTOUNGROUPED2()
                     {
                         DestinationServiceCentreName = list.Name,
                         Waybill = list.Waybill,
                         ShipmentDate = list.DateCreated,
                         PaymentMethod = list.PaymentMethod,
                         Amount = list.Amount
                     }).ToList();
            }
            else if (Limitdates.StartLimit == 3 && Limitdates.EndLimit == 4)
            {
                v = (from list in results
                     where list.DateCreated > LimitStartDate && list.DateCreated <= LimitEndDate && list.Name == Limitdates.ScName
                     select new InvoiceViewDTOUNGROUPED2()
                     {
                         DestinationServiceCentreName = list.Name,
                         Waybill = list.Waybill,
                         ShipmentDate = list.DateCreated,
                         PaymentMethod = list.PaymentMethod,
                         Amount = list.Amount
                     }).ToList();
            }
            else if (Limitdates.StartLimit == 4 && Limitdates.EndLimit == 5)
            {
                v = (from list in results
                     where list.PickupOptions == PickupOptions.SERVICECENTER && list.DateCreated <= LimitEndDate && list.Name == Limitdates.ScName
                     select new InvoiceViewDTOUNGROUPED2()
                     {
                         DestinationServiceCentreName = list.Name,
                         Waybill = list.Waybill,
                         ShipmentDate = list.DateCreated,
                         PaymentMethod = list.PaymentMethod,
                         Amount = list.Amount
                     }).ToList();
            }
            else if (Limitdates.StartLimit == 5 && Limitdates.EndLimit == 6)
            {
                v = (from list in results
                     where list.PickupOptions == PickupOptions.HOMEDELIVERY && list.DateCreated <= LimitEndDate && list.Name == Limitdates.ScName
                     select new InvoiceViewDTOUNGROUPED2()
                     {
                         DestinationServiceCentreName = list.Name,
                         Waybill = list.Waybill,
                         ShipmentDate = list.DateCreated,
                         PaymentMethod = list.PaymentMethod,
                         Amount = list.Amount
                     }).ToList();
            }

            return v;
        }

        private Tuple<DateTime, DateTime> ReturnStartAndEndDateLimit(AccountFilterCriteria accountFilterCriteria, LimitDates Limitdates)
        {
            var StartDate = new DateTime();
            var EndDate = new DateTime();
            DateTime now = DateTime.Now;

            var dashboardStartDate = (DateTime)accountFilterCriteria.StartDate;

            if (Limitdates.StartLimit == 1 && Limitdates.EndLimit == 2)
            {
                StartDate = now.AddHours(-24);
                EndDate = now;
            }
            else if (Limitdates.StartLimit == 2 && Limitdates.EndLimit == 3)
            {
                StartDate = now.AddHours(-48);
                EndDate = now.AddHours(-24);
            }
            else if (Limitdates.StartLimit == 3 && Limitdates.EndLimit == 4)
            {
                StartDate = dashboardStartDate;
                EndDate = now.AddHours(-48);
            }
            else if (Limitdates.StartLimit == 4 && Limitdates.EndLimit == 5)
            {
                StartDate = dashboardStartDate;
                EndDate = now.AddHours(-48);
            }
            else if (Limitdates.StartLimit == 5 && Limitdates.EndLimit == 6)
            {
                StartDate = dashboardStartDate;
                EndDate = now.AddHours(-48);
            }

            return Tuple.Create(StartDate, EndDate);
        }

        private Object[] ReturnShipmentCreatedByLimitDates(List<InvoiceMonitorDTO> shipmentscreated, AccountFilterCriteria accountFilterCriteria, LimitDates Limitdates)
        {
            var obj = new InvoiceMonitorDTO2();

            var dataValues = ReturnStartAndEndDateLimit(accountFilterCriteria, Limitdates);
            var LimitStartDate = dataValues.Item1;
            var LimitEndDate = dataValues.Item2;

            var result = new List<InvoiceMonitorDTO3>();

            if (Limitdates.StartLimit == 1 && Limitdates.EndLimit == 2)
            {
                result = (from list in shipmentscreated
                          where list.DateCreated > LimitStartDate && list.DateCreated <= LimitEndDate
                          select new InvoiceMonitorDTO3()
                          {
                              label = list.Name,
                              waybill = list.Waybill
                          }).ToList();
            }
            else if (Limitdates.StartLimit == 2 && Limitdates.EndLimit == 3)
            {
                result = (from list in shipmentscreated
                          where list.DateCreated > LimitStartDate && list.DateCreated <= LimitEndDate
                          select new InvoiceMonitorDTO3()
                          {
                              label = list.Name,
                              waybill = list.Waybill
                          }).ToList();
            }
            else if (Limitdates.StartLimit == 3 && Limitdates.EndLimit == 4)
            {
                result = (from list in shipmentscreated
                          where list.DateCreated > LimitStartDate && list.DateCreated <= LimitEndDate
                          select new InvoiceMonitorDTO3()
                          {
                              label = list.Name,
                              waybill = list.Waybill
                          }).ToList();
            }
            else if (Limitdates.StartLimit == 4 && Limitdates.EndLimit == 5)
            {
                result = (from list in shipmentscreated
                          where list.PickupOptions == PickupOptions.SERVICECENTER && list.DateCreated <= LimitEndDate
                          select new InvoiceMonitorDTO3()
                          {
                              label = list.Name,
                              waybill = list.Waybill
                          }).ToList();
            }
            else if (Limitdates.StartLimit == 5 && Limitdates.EndLimit == 6)
            {
                result = (from list in shipmentscreated
                          where list.PickupOptions == PickupOptions.HOMEDELIVERY && list.DateCreated <= LimitEndDate
                          select new InvoiceMonitorDTO3()
                          {
                              label = list.Name,
                              waybill = list.Waybill
                          }).ToList();
            }

            var v = (from a in result
                     group a by a.label into g
                     select new InvoiceMonitorDTO2()
                     {
                         label = g.Key,
                         y = g.Count(),
                     }).ToList();

            var finalresult = ReturnChartDataArray(v);
            return finalresult;
        }


        public async Task<ColoredInvoiceMonitorDTO> GetShipmentMonitorEXpected(AccountFilterCriteria accountFilterCriteria)
        {
            //filter by User Active Country
            var userActiveCountry = await _userService.GetUserActiveCountry();
            accountFilterCriteria.CountryId = userActiveCountry.CountryId;

            var serviceCenterIds = await _userService.GetPriviledgeServiceCenters();
            var results = await _uow.Invoice.GetShipmentMonitorSetSPExpected(accountFilterCriteria, serviceCenterIds);

            var result = new MulitipleInvoiceMonitorDTO()
            {
                ShipmentCreated = results
            };

            var shipmentscreated = result.ShipmentCreated;
            var obj = ReturnShipmentEXpected(shipmentscreated, accountFilterCriteria);

            return obj;
        }

        private ColoredInvoiceMonitorDTO ReturnShipmentCreatedx(List<InvoiceMonitorDTO> shipmentsexpected, List<InvoiceMonitorDTO> shipmentscollected, AccountFilterCriteria accountFilterCriteria)
        {
            var obj = new ColoredInvoiceMonitorDTO();

            var now = DateTime.Now;
            var dashboardStartDate = (DateTime)accountFilterCriteria.StartDate;

            var totalGreen = (from item in shipmentsexpected
                              where item.DateCreated > now.AddHours(-24) && item.DateCreated <= now
                              select item.Waybill).Count();

            var totalyellow = (from item in shipmentsexpected
                               where item.DateCreated > now.AddHours(-48) && item.DateCreated <= now.AddHours(-24)
                               select item.Waybill).Count();

            var totalRed = (from item in shipmentsexpected
                            where item.DateCreated > dashboardStartDate && item.DateCreated <= now.AddHours(-48)
                            select item.Waybill).Count();

            var totalPurple = (from item in shipmentscollected
                               where item.PickupOptions == PickupOptions.SERVICECENTER && item.DateCreated < now.AddHours(-48)
                               select item.Waybill).Count();

            var totalBrown = (from item in shipmentscollected
                              where item.PickupOptions == PickupOptions.HOMEDELIVERY && item.DateCreated < now.AddHours(-48)
                              select item.Waybill).Count();

            var totalzones = ReturnTotalZonesArray(totalGreen, totalyellow, totalRed, totalPurple, totalBrown);
            obj.totalZones = totalzones;

            return obj;
        }


        private ColoredInvoiceMonitorDTO ReturnShipmentCreated(List<InvoiceMonitorDTO> shipmentscreated, AccountFilterCriteria accountFilterCriteria)
        {
            var obj = new ColoredInvoiceMonitorDTO();

            var now = DateTime.Now;
            var dashboardStartDate = (DateTime)accountFilterCriteria.StartDate;

            var totalGreen = (from item in shipmentscreated
                              where item.DateCreated > now.AddHours(-24) && item.DateCreated <= now
                              select item.Waybill).Count();

            var totalyellow = (from item in shipmentscreated
                               where item.DateCreated > now.AddHours(-48) && item.DateCreated <= now.AddHours(-24)
                               select item.Waybill).Count();

            var totalRed = (from item in shipmentscreated
                            where item.DateCreated > dashboardStartDate && item.DateCreated <= now.AddHours(-48)
                            select item.Waybill).Count();

            var totalzones = ReturnTotalZonesArray(totalGreen, totalyellow, totalRed);
            obj.totalZones = totalzones;

            return obj;
        }

        public object[] ReturnChartDataArray(List<InvoiceMonitorDTO2> values)
        {
            var chartData = new object[values.Count() + 1];

            if (values != null)
            {
                chartData[0] = new object[]
                {
                    "ServiceCenter",
                    "WaybillCount"
                };

                int j = 0;
                foreach (var i in values)
                {
                    j++;
                    chartData[j] = new object[] {
                        i.label,
                        i.y
                    };
                }
            }

            return chartData;
        }

        public object[] ReturnTotalZonesArray(double totalgreen, double totalyellow, double totalred, double terminal = 0, double homeDelivery = 0)
        {

            //var chartData = new object[3];
            List<object[]> termsList = new List<object[]>
            {
                new object[]
                    {
                    "Zones",
                    "WaybillCount",
                    "Color"
                    },

                new object[] {
                    "Shipments Created within 24hrs",
                    totalgreen,
                    "green"
                },
                new object[] {
                    "Shipments created between 24 - 48 hrs",
                    totalyellow,
                    "yellow"
                },

                new object[] {
                    "Shipments created between 48 - 72 hrs",
                    totalred,
                    "red"
                },
                new object[] {
                    "Terminal Delivery Shipments Over 48 hrs",
                    terminal,
                    "purple"
                },
                new object[] {
                    "Home Delivery Shipments Over 48 hrs",
                    homeDelivery,
                    "brown"
                }
            };

            // You can convert it back to an array if you would like to
            object[] chartData = termsList.ToArray();

            return chartData;
        }

        private ColoredInvoiceMonitorDTO ReturnShipmentEXpected(List<InvoiceMonitorDTO> shipmentsexpected, AccountFilterCriteria accountFilterCriteria)
        {
            return null;
        }


        public async Task<DailySalesDTO> GetSalesForServiceCentre(AccountFilterCriteria accountFilterCriteria)
        {
            //set defaults
            if (accountFilterCriteria.StartDate == null)
            {
                accountFilterCriteria.StartDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            }

            if (accountFilterCriteria.EndDate == null)
            {
                accountFilterCriteria.EndDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            }

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

            var invoices = await _uow.Shipment.GetSalesForServiceCentre(accountFilterCriteria, serviceCenterIds);

            //Update to change the Corporate Paid status from 'Paid' to 'Credit'
            foreach (var item in invoices)
            {
                item.PaymentStatusDisplay = item.PaymentStatus.ToString();
                if ((CompanyType.Corporate.ToString() == item.CompanyType))
                {
                    item.PaymentStatusDisplay = "Credit";
                }
            }

            var dailySalesDTO = new DailySalesDTO()
            {
                StartDate = (DateTime)accountFilterCriteria.StartDate,
                EndDate = (DateTime)accountFilterCriteria.EndDate,
                Invoices = invoices,
                SalesCount = invoices.Count,
                TotalSales = invoices.Where(s => s.PaymentStatus == PaymentStatus.Paid).Sum(s => s.Amount),
            };

            return dailySalesDTO;
        }

        public async Task<DailySalesDTO> GetWaybillForServiceCentre(string waybill)
        {

            int[] serviceCenterIds = null;
            serviceCenterIds = await _userService.GetPriviledgeServiceCenters();


            var invoices = await _uow.Shipment.GetWaybillForServiceCentre(waybill, serviceCenterIds);

            //Update to change the Corporate Paid status from 'Paid' to 'Credit'
            foreach (var item in invoices)
            {
                item.PaymentStatusDisplay = item.PaymentStatus.ToString();
                if ((CompanyType.Corporate.ToString() == item.CompanyType))
                {
                    item.PaymentStatusDisplay = "Credit";
                }
            }

            var dailySalesDTO = new DailySalesDTO()
            {
                Invoices = invoices,
                SalesCount = invoices.Count,
                TotalSales = invoices.Where(s => s.PaymentStatus == PaymentStatus.Paid).Sum(s => s.Amount),
            };

            return dailySalesDTO;
        }

        public async Task<DailySalesDTO> GetDailySalesByServiceCentre(AccountFilterCriteria accountFilterCriteria)
        {
            var dailySales = await GetDailySales(accountFilterCriteria);

            var result = new List<DailySalesByServiceCentreDTO>();

            //1. sort by service centre
            var serviceCentreList = new HashSet<string>();
            foreach (var item in dailySales.Invoices)
            {
                serviceCentreList.Add(item.DepartureServiceCentreName);
            }

            //2. group by service centre
            foreach (var serviceCentreName in serviceCentreList)
            {
                var invoicesByServiceCentre = dailySales.Invoices.Where(s => s.DepartureServiceCentreName == serviceCentreName).ToList();

                var dailySalesByServiceCentreDTO = new DailySalesByServiceCentreDTO()
                {
                    StartDate = (DateTime)accountFilterCriteria.StartDate,
                    EndDate = (DateTime)accountFilterCriteria.EndDate,
                    Invoices = invoicesByServiceCentre,
                    SalesCount = invoicesByServiceCentre.Count,
                    TotalSales = invoicesByServiceCentre.Sum(s => (decimal?)s.Amount ?? 0),
                    DepartureServiceCentreId = invoicesByServiceCentre[0].DepartureServiceCentreId,
                    DepartureServiceCentreName = serviceCentreName
                };

                result.Add(dailySalesByServiceCentreDTO);
            }

            //3. add to parent object
            dailySales.DailySalesByServiceCentres = result;

            return dailySales;
        }

        ///////////
        public async Task<bool> ScanShipment(ScanDTO scan)
        {
            // verify the waybill number exists in the system
            var shipment = await GetShipmentForScan(scan.WaybillNumber);

            string scanStatus = scan.ShipmentScanStatus.ToString();

            if (shipment != null)
            {
                var newShipmentTracking = await _shipmentTrackingService.AddShipmentTracking(new ShipmentTrackingDTO
                {
                    DateTime = DateTime.Now,
                    Status = scanStatus,
                    Waybill = scan.WaybillNumber,
                }, scan.ShipmentScanStatus);
            }

            return true;
        }

        //utility method, called by another service and added here
        //to prevent ninject cyclic dependency
        public async Task<CustomerDTO> GetCustomer(int customerId, CustomerType customerType)
        {
            return await _customerService.GetCustomer(customerId, customerType);
        }

        /// <summary>
        /// This method is responsible for cancelling a shipment.
        /// It ensures that accounting entries are reversed accordingly or rolls back if an error occurs.
        /// A shipment can be cancelled if a dispatch has not yet been created for that waybill.
        /// 
        /// Steps for this process
        /// 1. Update shipment to 'cancelled' status
        /// 2. Update Invoice to 'cancelled' status
        /// 3. Create new entry in General Ledger for Invoice amount (debit)
        /// 4. Update customers wallet (credit)
        /// 5. Scan the Shipment for cancellation
        /// </summary>
        /// <param name="waybill"></param>
        /// <returns>true if the operation was successful or false if it fails</returns>

        public async Task<bool> CancelShipmentForMagaya(string waybill)
        {
            var boolRresult = false;
            try
            {
                //Check if waybill sales has already been deposited
                //remove from bank deposit order if it is there
                var bankDepositOrder = await _uow.BankProcessingOrderForShipmentAndCOD.GetAsync(s => s.Waybill == waybill && s.DepositType == DepositType.Shipment);

                if (bankDepositOrder != null)
                {
                    if (bankDepositOrder.Status == DepositStatus.Deposited || bankDepositOrder.Status == DepositStatus.Verified)
                    {
                        //throw new GenericException($"Error Cancelling the Shipment." +
                        //        $" The shipment with waybill number {waybill} has already been deposited in the bank with ref code {bankDepositOrder.RefCode}.");

                        var bankDeposit = await _uow.BankProcessingOrderCodes.GetAsync(s => s.Code == bankDepositOrder.RefCode);
                        bankDeposit.TotalAmount = bankDeposit.TotalAmount - bankDepositOrder.GrandTotal;
                        bankDepositOrder.IsDeleted = true;
                    }
                }


                //1. check if there is a dispatch for the waybill (Manifest -> Group -> Waybill)
                //If there is, throw an exception (since, the shipment has already left the terminal)
                var groupwaybillMapping = _uow.GroupWaybillNumberMapping.SingleOrDefault(s => s.WaybillNumber == waybill);
                if (groupwaybillMapping != null)
                {
                    var mainfestMapping = _uow.ManifestGroupWaybillNumberMapping.
                        SingleOrDefault(s => s.GroupWaybillNumber == groupwaybillMapping.GroupWaybillNumber);

                    if (mainfestMapping != null)
                    {
                        var dispatch = _uow.Dispatch.SingleOrDefault(s => s.ManifestNumber == mainfestMapping.ManifestCode);

                        if (dispatch != null)
                        {
                            throw new GenericException($"Error Cancelling the Shipment." +
                                $" The shipment with waybill number {waybill} has already been dispatched by" +
                                $" vehicle number {dispatch.RegistrationNumber}.");
                        }
                    }

                    //remove waybill from manifest and groupwaybill
                    await RemoveWaybillNumberFromGroupForCancelledShipment(groupwaybillMapping.GroupWaybillNumber, groupwaybillMapping.WaybillNumber);
                }

                //2.1 Update shipment to cancelled
                var shipment = _uow.Shipment.SingleOrDefault(s => s.Waybill == waybill);
                shipment.IsCancelled = true;

                var invoice = _uow.Invoice.SingleOrDefault(s => s.Waybill == waybill);

                if (invoice.PaymentStatus == PaymentStatus.Paid || invoice.PaymentStatus ==PaymentStatus.Pending || invoice.PaymentStatus ==PaymentStatus.Failed)
                {
                    //2. Reverse accounting entries

                    //2.3 Create new entry in General Ledger for Invoice amount (debit)
                    var currentUserId = await _userService.GetCurrentUserId();

                    ////--start--///Set the DepartureCountryId
                    int countryIdFromServiceCentreId = shipment.DepartureCountryId;
                    ////--end--///Set the DepartureCountryId

                    var generalLedger = new GeneralLedger()
                    {
                        DateOfEntry = DateTime.Now,
                        ServiceCentreId = shipment.DepartureServiceCentreId,
                        CountryId = countryIdFromServiceCentreId,
                        UserId = currentUserId,
                        Amount = invoice.Amount,
                        CreditDebitType = CreditDebitType.Debit,
                        Description = "Debit for Shipment Cancellation",
                        IsDeferred = false,
                        Waybill = waybill,
                        PaymentServiceType = PaymentServiceType.Shipment
                    };
                    _uow.GeneralLedger.Add(generalLedger);

                    //2.4.1 Update customers wallet (credit)
                    //get CustomerDetails
                    if (shipment.CustomerType.Contains("Individual"))
                    {
                        shipment.CustomerType = CustomerType.IndividualCustomer.ToString();
                    }
                    CustomerType customerType = (CustomerType)Enum.Parse(typeof(CustomerType), shipment.CustomerType);

                    //only add the money to wallet if the payment type is by wallet
                    var walletTransaction = await _uow.WalletTransaction.GetAsync(x => x.Waybill == waybill && x.CreditDebitType == CreditDebitType.Debit && x.PaymentType == PaymentType.Wallet);

                    if (walletTransaction != null)
                    {
                        //return the actual amount collected in case shipment departure and destination country is different
                        // var wallet = _uow.Wallet.SingleOrDefault(s => s.CustomerId == shipment.CustomerId && s.CustomerType == customerType);
                        var wallet = await _uow.Wallet.GetAsync(s => s.CustomerCode == shipment.CustomerCode);

                        decimal amountToCredit = invoice.Amount;
                        amountToCredit = await GetActualAmountToCredit(shipment, amountToCredit);
                        //2.4.2 Update customers wallet's Transaction (credit)
                        var newWalletTransaction = new WalletTransaction
                        {
                            WalletId = wallet.WalletId,
                            Amount = amountToCredit,
                            DateOfEntry = DateTime.Now,
                            ServiceCentreId = shipment.DepartureServiceCentreId,
                            UserId = currentUserId,
                            CreditDebitType = CreditDebitType.Credit,
                            PaymentType = PaymentType.Wallet,
                            Waybill = waybill,
                            Description = "Credit for Shipment Cancellation"
                        };
                        if (newWalletTransaction.CreditDebitType == CreditDebitType.Credit)
                        {
                            newWalletTransaction.BalanceAfterTransaction = wallet.Balance + newWalletTransaction.Amount;
                        }
                        else
                        {
                            newWalletTransaction.BalanceAfterTransaction = wallet.Balance - newWalletTransaction.Amount;
                        }

                        wallet.Balance = wallet.Balance + amountToCredit;
                        _uow.WalletTransaction.Add(newWalletTransaction);
                    }
                }

                //2.2 Update Invoice PaymentStatus to cancelled
                invoice.PaymentStatus = PaymentStatus.Cancelled;

                //2.5 Scan the Shipment for cancellation
                await ScanShipment(new ScanDTO
                {
                    WaybillNumber = waybill,
                    ShipmentScanStatus = ShipmentScanStatus.SSC
                });

                //send message
                //await _messageSenderService.SendMessage(MessageType.ShipmentCreation, EmailSmsType.All, waybill);
                boolRresult = true;

                return boolRresult;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<bool> CancelShipment(string waybill)
        {
            var boolRresult = false;
            try
            {
                //Check if waybill sales has already been deposited
                //remove from bank deposit order if it is there
                var bankDepositOrder = await _uow.BankProcessingOrderForShipmentAndCOD.GetAsync(s => s.Waybill == waybill && s.DepositType == DepositType.Shipment);

                if (bankDepositOrder != null)
                {
                    if (bankDepositOrder.Status != DepositStatus.Deposited && bankDepositOrder.Status != DepositStatus.Verified)
                    {
                        //throw new GenericException($"Error Cancelling the Shipment." +
                        //        $" The shipment with waybill number {waybill} has already been deposited in the bank with ref code {bankDepositOrder.RefCode}.");

                        var bankDeposit = await _uow.BankProcessingOrderCodes.GetAsync(s => s.Code == bankDepositOrder.RefCode);
                        bankDeposit.TotalAmount = bankDeposit.TotalAmount - bankDepositOrder.GrandTotal;
                        bankDepositOrder.IsDeleted = true;
                    }
                }


                //1. check if there is a dispatch for the waybill (Manifest -> Group -> Waybill)
                //If there is, throw an exception (since, the shipment has already left the terminal)
                var groupwaybillMapping = _uow.GroupWaybillNumberMapping.SingleOrDefault(s => s.WaybillNumber == waybill);
                if (groupwaybillMapping != null)
                {
                    var mainfestMapping = _uow.ManifestGroupWaybillNumberMapping.
                        SingleOrDefault(s => s.GroupWaybillNumber == groupwaybillMapping.GroupWaybillNumber);

                    if (mainfestMapping != null)
                    {
                        var dispatch = _uow.Dispatch.SingleOrDefault(s => s.ManifestNumber == mainfestMapping.ManifestCode);

                        if (dispatch != null)
                        {
                            throw new GenericException($"Error Cancelling the Shipment." +
                                $" The shipment with waybill number {waybill} has already been dispatched by" +
                                $" vehicle number {dispatch.RegistrationNumber}.");
                        }
                    }

                    //remove waybill from manifest and groupwaybill
                    await RemoveWaybillNumberFromGroupForCancelledShipment(groupwaybillMapping.GroupWaybillNumber, groupwaybillMapping.WaybillNumber);
                }

                //2.1 Update shipment to cancelled
                var shipment = _uow.Shipment.SingleOrDefault(s => s.Waybill == waybill);
                shipment.IsCancelled = true;

                var invoice = _uow.Invoice.SingleOrDefault(s => s.Waybill == waybill);

                if (invoice.PaymentStatus == PaymentStatus.Paid)
                {
                    //2. Reverse accounting entries

                    //2.3 Create new entry in General Ledger for Invoice amount (debit)
                    var currentUserId = await _userService.GetCurrentUserId();

                    ////--start--///Set the DepartureCountryId
                    int countryIdFromServiceCentreId = shipment.DepartureCountryId;
                    ////--end--///Set the DepartureCountryId

                    var generalLedger = new GeneralLedger()
                    {
                        DateOfEntry = DateTime.Now,
                        ServiceCentreId = shipment.DepartureServiceCentreId,
                        CountryId = countryIdFromServiceCentreId,
                        UserId = currentUserId,
                        Amount = invoice.Amount,
                        CreditDebitType = CreditDebitType.Debit,
                        Description = "Debit for Shipment Cancellation",
                        IsDeferred = false,
                        Waybill = waybill,
                        PaymentServiceType = PaymentServiceType.Shipment
                    };
                    _uow.GeneralLedger.Add(generalLedger);

                    //2.4.1 Update customers wallet (credit)
                    //get CustomerDetails
                    if (shipment.CustomerType.Contains("Individual"))
                    {
                        shipment.CustomerType = CustomerType.IndividualCustomer.ToString();
                    }
                    CustomerType customerType = (CustomerType)Enum.Parse(typeof(CustomerType), shipment.CustomerType);

                    //only add the money to wallet if the payment type is by wallet
                    var walletTransaction = await _uow.WalletTransaction.GetAsync(x => x.Waybill == waybill && x.CreditDebitType == CreditDebitType.Debit && x.PaymentType == PaymentType.Wallet);

                    if (walletTransaction != null)
                    {
                        //return the actual amount collected in case shipment departure and destination country is different
                        // var wallet = _uow.Wallet.SingleOrDefault(s => s.CustomerId == shipment.CustomerId && s.CustomerType == customerType);
                        var wallet = await _uow.Wallet.GetAsync(s => s.CustomerCode == shipment.CustomerCode);

                        decimal amountToCredit = invoice.Amount;
                        amountToCredit = await GetActualAmountToCredit(shipment, amountToCredit);
                        //2.4.2 Update customers wallet's Transaction (credit)
                        var newWalletTransaction = new WalletTransaction
                        {
                            WalletId = wallet.WalletId,
                            Amount = amountToCredit,
                            DateOfEntry = DateTime.Now,
                            ServiceCentreId = shipment.DepartureServiceCentreId,
                            UserId = currentUserId,
                            CreditDebitType = CreditDebitType.Credit,
                            PaymentType = PaymentType.Wallet,
                            Waybill = waybill,
                            Description = "Credit for Shipment Cancellation"
                        };
                        if (newWalletTransaction.CreditDebitType == CreditDebitType.Credit)
                        {
                            newWalletTransaction.BalanceAfterTransaction = wallet.Balance + newWalletTransaction.Amount;
                        }
                        else
                        {
                            newWalletTransaction.BalanceAfterTransaction = wallet.Balance - newWalletTransaction.Amount;
                        }

                        wallet.Balance = wallet.Balance + amountToCredit;
                        _uow.WalletTransaction.Add(newWalletTransaction);
                    }
                }

                //2.2 Update Invoice PaymentStatus to cancelled
                invoice.PaymentStatus = PaymentStatus.Cancelled;

                //2.5 Scan the Shipment for cancellation
                await ScanShipment(new ScanDTO
                {
                    WaybillNumber = waybill,
                    ShipmentScanStatus = ShipmentScanStatus.SSC
                });

                //send message
                //await _messageSenderService.SendMessage(MessageType.ShipmentCreation, EmailSmsType.All, waybill);
                boolRresult = true;

                return boolRresult;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task RemoveWaybillNumberFromGroupForCancelledShipment(string groupWaybillNumber, string waybillNumber)
        {
            try
            {
                var groupWaybillNumberMapping = _uow.GroupWaybillNumberMapping.SingleOrDefault(x => (x.GroupWaybillNumber == groupWaybillNumber) && (x.WaybillNumber == waybillNumber));
                if (groupWaybillNumberMapping != null)
                {
                    _uow.GroupWaybillNumberMapping.Remove(groupWaybillNumberMapping);
                }

                await _uow.CompleteAsync();

                //Delete the GroupWaybill If All the Waybills attached to it have been deleted.
                var checkIfWaybillExistForGroup = await _uow.GroupWaybillNumberMapping.FindAsync(x => x.GroupWaybillNumber == groupWaybillNumber);
                if (!checkIfWaybillExistForGroup.Any())
                {
                    //ensure that the Manifest containing the Groupwaybill has not been dispatched
                    var manifestGroupWaybillNumberMapping = _uow.ManifestGroupWaybillNumberMapping.SingleOrDefault(x => x.GroupWaybillNumber == groupWaybillNumber);
                    if (manifestGroupWaybillNumberMapping != null)
                    {
                        //Delete the manifest mapping if groupway has been mapped to manifest
                        _uow.ManifestGroupWaybillNumberMapping.Remove(manifestGroupWaybillNumberMapping);
                    }

                    //remove group waybill
                    var groupWaybillNumberDTO = await _uow.GroupWaybillNumber.GetAsync(x => x.GroupWaybillCode.Equals(groupWaybillNumber));
                    if (groupWaybillNumberDTO != null)
                    {
                        _uow.GroupWaybillNumber.Remove(groupWaybillNumberDTO);
                    }

                    await _uow.CompleteAsync();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<ServiceCentreDTO>> GetAllWarehouseServiceCenters()
        {
            var userActiveCountryId = await _userService.GetUserActiveCountryId();

            try
            {
                string[] warehouseServiceCentres = { };
                // filter by global property for warehouse service centre
                var warehouseServiceCentreObj = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.WarehouseServiceCentre, userActiveCountryId);
                if (warehouseServiceCentreObj != null)
                {
                    var warehouseServiceCentre = warehouseServiceCentreObj.Value;
                    warehouseServiceCentres = warehouseServiceCentre.Split(',');
                    warehouseServiceCentres = warehouseServiceCentres.Select(s => s.Trim()).ToArray();
                }

                //get all warehouse service centre
                var allServiceCenters = await _centreService.GetServiceCentreByCode(warehouseServiceCentres);

                return allServiceCenters;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> AddShipmentFromMobile(ShipmentDTO shipment)
        {
            try
            {
                //check if shipment already exists
                var shipmentexists = await _uow.Shipment.GetAsync(s => s.Waybill == shipment.Waybill, "ShipmentItems");

                if (shipmentexists != null)
                {
                    shipmentexists.DestinationServiceCentreId = shipment.DestinationServiceCentreId;
                    shipmentexists.DepartureServiceCentreId = shipment.DepartureServiceCentreId;
                    shipmentexists.DepartureCountryId = shipment.DepartureCountryId;
                    shipmentexists.DestinationCountryId = shipment.DestinationCountryId;
                    shipmentexists.PickupOptions = shipment.PickupOptions;
                    shipmentexists.IsClassShipment = shipment.IsClassShipment;

                    if (shipment.PackageOptionIds.Any())
                    {
                        var numOfPackages = shipment.PackageOptionIds.Count;
                        var numOfShipmentItems = shipmentexists.ShipmentItems.Count;

                        if (numOfPackages > numOfShipmentItems)
                        {
                            throw new GenericException("Number of Packages should not be more then Shipment Items!", $"{(int)HttpStatusCode.BadRequest}");
                        }

                        for (var i = 0; i < numOfPackages; i++)
                        {
                            shipmentexists.ShipmentItems[i].ShipmentPackagePriceId = shipment.PackageOptionIds[i];
                            shipmentexists.ShipmentItems[i].PackageQuantity = 1;

                        }

                        await UpdatePackageTransactions(shipment);

                    }

                    var invoice = await _uow.Invoice.GetAsync(s => s.Waybill == shipment.Waybill);
                    if (invoice != null)
                    {
                        invoice.CountryId = shipment.DepartureCountryId;
                        invoice.ServiceCentreId = shipment.DepartureServiceCentreId;
                    }
                    var GeneralLedger = await _uow.GeneralLedger.GetAsync(s => s.Waybill == shipment.Waybill);
                    if (GeneralLedger != null)
                    {
                        GeneralLedger.CountryId = shipment.DepartureCountryId;
                        GeneralLedger.ServiceCentreId = shipment.DepartureServiceCentreId;
                    }

                    await _uow.CompleteAsync();
                    return true;
                }
                else
                {
                    shipment.ApproximateItemsWeight = 0;

                    // add serial numbers to the ShipmentItems
                    var serialNumber = 1;
                    foreach (var shipmentItem in shipment.ShipmentItems)
                    {
                        shipmentItem.SerialNumber = serialNumber;

                        //sum item weight
                        //check for volumetric weight
                        if (shipmentItem.IsVolumetric)
                        {
                            double volume = (shipmentItem.Length * shipmentItem.Height * shipmentItem.Width) / 5000;
                            double Weight = shipmentItem.Weight > volume ? shipmentItem.Weight : volume;

                            shipment.ApproximateItemsWeight += Weight;
                        }
                        else
                        {
                            shipment.ApproximateItemsWeight += shipmentItem.Weight;
                        }

                        serialNumber++;

                    }

                    await CreateInvoice(shipment);
                    CreateGeneralLedger(shipment);
                    var newShipment = Mapper.Map<Shipment>(shipment);
                    if (shipment.PackageOptionIds.Any())
                    {
                        var numOfPackages = shipment.PackageOptionIds.Count;
                        var numOfShipmentItems = newShipment.ShipmentItems.Count;

                        if (numOfPackages > numOfShipmentItems)
                        {
                            throw new GenericException("Number of Packages should not be more then Shipment Items!", $"{(int)HttpStatusCode.BadRequest}");
                        }

                        for (var i = 0; i < numOfPackages; i++)
                        {
                            newShipment.ShipmentItems[i].ShipmentPackagePriceId = shipment.PackageOptionIds[i];
                            newShipment.ShipmentItems[i].PackageQuantity = 1;

                        }
                        await UpdatePackageTransactions(shipment);

                    }

                    _uow.Shipment.Add(newShipment);
                    return true;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        //Process Payment for Customer Week
        public async Task<bool> ProcessPaymentForCustomerWeek(ShipmentDTO shipment)
        {
            var processPayment = false;

            var customerWeek = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.CustomerWeekDate, shipment.DepartureCountryId);

            if (customerWeek == null)
                return processPayment;

            var customerWeekDate = Convert.ToDateTime(customerWeek.Value);
            var startDate = DateTime.Now.Date;

            //if today is customer week
            if (startDate.ToLongDateString() == customerWeekDate.ToLongDateString())
            {
                var endDate = startDate.AddDays(1);

                //1. Get all Individual customer waybills for the service centre for 08/10/2019 sort by date created
                string individual = CustomerType.IndividualCustomer.ToString();

                var data = _uow.Shipment.GetAllAsQueryable()
                    .Where(x => x.DateCreated >= startDate && x.DateCreated < endDate && x.DepartureServiceCentreId == shipment.DepartureServiceCentreId
                    && x.CompanyType == individual).OrderBy(x => x.DateCreated).Select(x => x.Waybill).ToList();

                //2. Get the Index of the waybill -- eg. if findIndex = 1, then index = 2 or findIndex = -1(Nothing find), then index = 0 
                int waybillIndex = data.FindIndex(x => x == shipment.Waybill) + 1;

                //3. If the waybill fall Shipment between 1st, 5th, 10, 15, 20
                var shipmentCount = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.CustomerWeekCount, shipment.DepartureCountryId);

                if (shipmentCount == null)
                    return processPayment;

                int[] freeShippingItem = shipmentCount.Value.Split(',').Select(x => int.Parse(x.Trim())).ToArray();

                //4. Process payment for the customer else don't     
                processPayment = freeShippingItem.Contains(waybillIndex);

                if (processPayment)
                {
                    var generalLedgerEntity = await _uow.GeneralLedger.GetAsync(s => s.Waybill == shipment.Waybill);
                    var invoiceEntity = await _uow.Invoice.GetAsync(s => s.Waybill == shipment.Waybill);
                    var oldShipment = _uow.Shipment.SingleOrDefault(s => s.Waybill == shipment.Waybill);

                    generalLedgerEntity.Amount = 0;
                    invoiceEntity.Amount = 0;
                    oldShipment.GrandTotal = 0;

                    await _uow.CompleteAsync();

                    PaymentTransactionDTO paymentTransaction = new PaymentTransactionDTO
                    {
                        Waybill = shipment.Waybill,
                        PaymentType = PaymentType.Cash
                    };

                    var result = await _paymentService.ProcessPayment(paymentTransaction);
                }
            }
            return processPayment;
        }

        private async Task<decimal> GetActualAmountToCredit(Shipment shipment, decimal amountToDebit)
        {
            //1. Get Customer Country detail
            int customerCountryId = 1;

            if (UserChannelType.Ecommerce.ToString() == shipment.CompanyType || UserChannelType.Corporate.ToString() == shipment.CompanyType)
            {
                customerCountryId = _uow.Company.GetAllAsQueryable().Where(x => x.CustomerCode.ToLower() == shipment.CustomerCode.ToLower()).Select(x => x.UserActiveCountryId).FirstOrDefault();
            }
            else
            {
                customerCountryId = _uow.IndividualCustomer.GetAllAsQueryable().Where(x => x.CustomerCode.ToLower() == shipment.CustomerCode.ToLower()).Select(x => x.UserActiveCountryId).FirstOrDefault();
            }

            //2. If the customer country !== Departure Country, Convert the payment
            if (customerCountryId != shipment.DepartureCountryId)
            {
                var countryRateConversion = await _countryRouteZoneMapService.GetZone(shipment.DestinationCountryId, shipment.DepartureCountryId);

                double amountToDebitDouble = (double)amountToDebit * countryRateConversion.Rate;

                amountToDebit = (decimal)Math.Round(amountToDebitDouble, 2);
            }

            return amountToDebit;
        }

        public async Task<bool> SendEmailToRegionalManagersForStoreShipments(string waybill, int destinationId, int departureId)
        {
            var destServiceCentreDTO = _uow.ServiceCentre.Get(destinationId);
            var deptServiceCentreDTO = _uow.ServiceCentre.Get(departureId);

            // Get all the Regional Managers assigned to the ServiceCentre where Scan took place
            List<UserDTO> allRegionalManagers = await _shipmentTrackingService.GetAllRegionalManagersForServiceCentre(destinationId);

            // Use a loop to send to all Regional Managers
            foreach (var regionalManager in allRegionalManagers)
            {
                // Create MessageExtensionDTO to hold custom message info
                var messageExtensionDTO = new MessageExtensionDTO()
                {
                    RegionalManagerName = regionalManager.FirstName + " " + regionalManager.LastName,
                    RegionalManagerEmail = regionalManager.Email,
                    ServiceCenterAgentName = deptServiceCentreDTO.Name,
                    ServiceCenterName = destServiceCentreDTO.Name,
                    WaybillNumber = waybill,
                };

                // send message
                await _messageSenderService.SendGenericEmailMessage(MessageType.SRMEmail, messageExtensionDTO);
            }

            return true;
        }

        private async Task UpdatePackageTransactions(ShipmentDTO shipment)
        {
            List<ShipmentPackagingTransactions> packageoutflow = new List<ShipmentPackagingTransactions>();
            List<ServiceCenterPackage> servicePackage = new List<ServiceCenterPackage>();

            foreach (var packageId in shipment.PackageOptionIds)
            {
                var shipmentPackage = await _uow.ShipmentPackagePrice.GetAsync(x => x.ShipmentPackagePriceId == packageId);
                var serviceCenterPackage = await _uow.ServiceCenterPackage.GetAsync(x => x.ShipmentPackageId == shipmentPackage.ShipmentPackagePriceId && x.ServiceCenterId == shipment.DepartureServiceCentreId);

                if (serviceCenterPackage == null)
                {
                    var newshipmentPackage = new ServiceCenterPackage
                    {
                        ServiceCenterId = shipment.DepartureServiceCentreId,
                        ShipmentPackageId = shipmentPackage.ShipmentPackagePriceId,
                        InventoryOnHand = 0,
                        MinimunRequired = 0,
                    };
                    servicePackage.Add(newshipmentPackage);
                }
                else
                {
                    serviceCenterPackage.InventoryOnHand -= 1;
                }

                var newOutflow = new ShipmentPackagingTransactions
                {
                    ServiceCenterId = shipment.DepartureServiceCentreId,
                    ShipmentPackageId = shipmentPackage.ShipmentPackagePriceId,
                    Quantity = 1,
                    Waybill = shipment.Waybill,
                    UserId = shipment.UserId,
                    PackageTransactionType = Core.Enums.PackageTransactionType.OutflowFromServiceCentre
                };
                packageoutflow.Add(newOutflow);
            }
            _uow.ShipmentPackagingTransactions.AddRange(packageoutflow);
            _uow.ServiceCenterPackage.AddRange(servicePackage);
        }

        private async Task<PreShipmentMobileDTO> CreatePreShipmentFromAgility(PreShipmentMobileDTO preShipment, MobilePriceDTO priceDTO)
        {
            try
            {
                preShipment.CustomerCode = preShipment.Customer[0].CustomerCode;
                preShipment.CustomerCode = preShipment.CustomerCode;
                var user = await _uow.User.GetUserUsingCustomerForCustomerPortal(preShipment.CustomerCode);
                if (user != null)
                {
                    preShipment.UserId = user.Id;
                }
                else
                {
                    preShipment.UserId = await _userService.GetCurrentUserId();
                }

                if (preShipment.CustomerType == CustomerType.Company.ToString())
                {
                    var company = await _uow.Company.GetAsync(x => x.CustomerCode == preShipment.CustomerCode);
                    preShipment.CustomerCode = company.CustomerCode;
                    preShipment.CompanyType = company.CompanyType.ToString();
                    preShipment.CustomerType = CustomerType.Company.ToString();
                    preShipment.SenderName = company.Name;
                }
                else
                {
                    var individual = await _uow.IndividualCustomer.GetAsync(x => x.CustomerCode == preShipment.CustomerCode);
                    preShipment.CustomerType = "Individual";
                    preShipment.CompanyType = CustomerType.IndividualCustomer.ToString();
                    preShipment.SenderName = preShipment.Customer[0].FirstName + " " + preShipment.Customer[0].LastName;
                }

                if (preShipment.PickupOptions == PickupOptions.SERVICECENTER)
                {
                    var receieverServiceCenter = await _uow.ServiceCentre.GetAsync(x => x.ServiceCentreId == preShipment.DestinationServiceCentreId);
                    preShipment.ReceiverAddress = $"GIGL {receieverServiceCenter.FormattedServiceCentreName} SERVICE CENTER ({receieverServiceCenter.Address})";
                    preShipment.ReceiverLocation = new LocationDTO
                    {
                        Latitude = (double)receieverServiceCenter.Latitude,
                        Longitude = (double)receieverServiceCenter.Longitude
                    };
                }

                var mobileShipment = new PreShipmentMobileDTO
                {
                    CalculatedTotal = (double)priceDTO.DeliveryPrice,
                    IsHomeDelivery = preShipment.PickupOptions == PickupOptions.HOMEDELIVERY ? true : false,
                    IsScheduled = false,
                    Waybill = preShipment.Waybill,
                    ReceiverName = preShipment.ReceiverName,
                    ReceiverPhoneNumber = preShipment.ReceiverPhoneNumber,
                    ReceiverEmail = preShipment.ReceiverEmail,
                    ReceiverAddress = preShipment.ReceiverAddress,
                    ReceiverStationId = preShipment.ReceiverStationId,
                    ReceiverLocation = new LocationDTO
                    {
                        Latitude = preShipment.ReceiverLocation.Latitude,
                        Longitude = preShipment.ReceiverLocation.Longitude
                    },
                    GrandTotal = preShipment.GrandTotal,
                    SenderAddress = preShipment.SenderAddress,
                    SenderStationId = preShipment.SenderStationId,
                    SenderLocation = new LocationDTO
                    {
                        Latitude = preShipment.SenderLocation.Latitude,
                        Longitude = preShipment.SenderLocation.Longitude
                    },
                    SenderName = preShipment.SenderName,
                    SenderPhoneNumber = preShipment.Customer[0].PhoneNumber,
                    CustomerCode = preShipment.Customer[0].CustomerCode,
                    VehicleType = preShipment.VehicleType,
                    UserId = preShipment.UserId,
                    IsdeclaredVal = preShipment.IsdeclaredVal,
                    DiscountValue = priceDTO.Discount,
                    InsuranceValue = priceDTO.InsuranceValue,
                    Vat = priceDTO.Vat,
                    DeliveryPrice = priceDTO.DeliveryPrice,
                    SenderLocality = preShipment.SenderLocality,
                    ZoneMapping = preShipment.ZoneMapping,

                    CountryId = preShipment.CountryId,
                    CustomerType = preShipment.CustomerType,
                    CompanyType = preShipment.CompanyType,
                    Value = (decimal)preShipment.Value,
                    ShipmentPickupPrice = (decimal)(priceDTO.PickUpCharge == null ? 0.0M : priceDTO.PickUpCharge),
                    IsConfirmed = false,
                    IsDelivered = false,
                    shipmentstatus = "Shipment created",
                    PreShipmentItems = preShipment.PreShipmentItems.Select(s => new PreShipmentItemMobileDTO
                    {
                        Description = s.Description,
                        SpecialPackageId = s.SpecialPackageId,
                        ShipmentType = s.ShipmentType,
                        IsVolumetric = s.IsVolumetric,
                        Weight = (decimal)s.Weight,
                        Quantity = s.Quantity,
                        CalculatedPrice = s.CalculatedPrice,
                        ItemName = s.Description,
                        Value = s.Value

                    }).ToList()
                };

                var newPreShipment = Mapper.Map<PreShipmentMobile>(mobileShipment);

                newPreShipment.DateCreated = DateTime.Now;
                newPreShipment.IsFromAgility = true;

                _uow.PreShipmentMobile.Add(newPreShipment);
                await _uow.CompleteAsync();

                var gigGOServiceCenter = await _userService.GetGIGGOServiceCentre();
                await AddMobileShipmentTracking(new MobileShipmentTrackingDTO
                {
                    DateTime = DateTime.Now,
                    Status = ShipmentScanStatus.MCRT.ToString(),
                    Waybill = newPreShipment.Waybill,
                    User = newPreShipment.UserId,
                    ServiceCentreId = gigGOServiceCenter.ServiceCentreId
                }, ShipmentScanStatus.MCRT);

                //Fire and forget
                //Send the Payload to Partner Cloud Handler 
                await NodeApiCreateShipment(newPreShipment);
                return mobileShipment;
            }
            catch
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

        private async Task AddMobileShipmentTracking(MobileShipmentTrackingDTO tracking, ShipmentScanStatus scanStatus)
        {
            try
            {
                //check if the waybill has not been scan for the status before
                bool shipmentTracking = await _uow.MobileShipmentTracking.ExistAsync(x => x.Waybill.Equals(tracking.Waybill) && x.Status.Equals(tracking.Status));

                if (!shipmentTracking || scanStatus.Equals(ShipmentScanStatus.AD))
                {
                    var newShipmentTracking = new MobileShipmentTracking
                    {
                        Waybill = tracking.Waybill,
                        Status = tracking.Status,
                        DateTime = DateTime.Now,
                        UserId = tracking.User,
                        ServiceCentreId = tracking.ServiceCentreId
                    };
                    _uow.MobileShipmentTracking.Add(newShipmentTracking);
                    await _uow.CompleteAsync();
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task GenerateDeliveryNumber(string waybill)
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
            var number = new DeliveryNumber
            {
                SenderCode = "DN" + strippedText.ToUpper(),
                IsUsed = false,
                Waybill = waybill
            };
            _uow.DeliveryNumber.Add(number);
        }

        //For GIG Go Extension Feature
        private async Task<CustomerDTO> GetAndCreateCustomer(CustomerDTO customerDTO)
        {

            // handle Company customers
            if (CustomerType.Company.Equals(customerDTO.CustomerType))
            {
                int companyId = 0;

                var companyByCode = await _uow.Company.GetAsync(x => x.CustomerCode == customerDTO.CustomerCode);

                if (companyByCode == null)
                {
                    var CompanyByName = await _uow.Company.FindAsync(c => c.Name.ToLower() == customerDTO.Name.ToLower()
                    || c.PhoneNumber == customerDTO.PhoneNumber || c.Email == customerDTO.Email || c.CustomerCode == customerDTO.CustomerCode);

                    foreach (var item in CompanyByName)
                    {
                        companyId = item.CompanyId;
                    }
                }
                else
                {
                    companyId = companyByCode.CompanyId;
                }

                if (companyId > 0)
                {
                    customerDTO.CompanyId = companyId;
                    var companyDTO = Mapper.Map<CompanyDTO>(customerDTO);
                }
            }

            // handle IndividualCustomers
            if (CustomerType.IndividualCustomer.Equals(customerDTO.CustomerType))
            {
                int individualCustomerId = 0;
                var individualCustomerByPhone = await _uow.IndividualCustomer.
                    GetAsync(c => c.PhoneNumber == customerDTO.PhoneNumber || c.CustomerCode == customerDTO.CustomerCode);

                if (individualCustomerByPhone != null)
                {
                    individualCustomerId = individualCustomerByPhone.IndividualCustomerId;
                }

                if (individualCustomerId > 0)
                {
                    // update
                    customerDTO.IndividualCustomerId = individualCustomerId;
                    var individualCustomerDTO = Mapper.Map<IndividualCustomerDTO>(customerDTO);

                    individualCustomerByPhone.FirstName = customerDTO.FirstName;
                    individualCustomerByPhone.LastName = customerDTO.LastName;
                    individualCustomerByPhone.Email = customerDTO.Email;
                    individualCustomerByPhone.Address = customerDTO.Address;
                    individualCustomerByPhone.City = customerDTO.City;
                    individualCustomerByPhone.Gender = customerDTO.Gender;
                    individualCustomerByPhone.PictureUrl = customerDTO.PictureUrl;
                    individualCustomerByPhone.PhoneNumber = customerDTO.PhoneNumber;
                    individualCustomerByPhone.State = customerDTO.State;
                    individualCustomerByPhone.Password = customerDTO.Password;
                    individualCustomerByPhone.UserActiveCountryId = customerDTO.UserActiveCountryId;

                    await _uow.CompleteAsync();
                }
                else
                {
                    if (customerDTO.PhoneNumber.StartsWith("0"))
                    {
                        var country = await _uow.Country.GetAsync(x => x.CountryId == customerDTO.UserActiveCountryId);
                        if (country != null)
                        {
                            customerDTO.PhoneNumber = customerDTO.PhoneNumber.Substring(1, customerDTO.PhoneNumber.Length - 1);
                            string phone = $"{country.PhoneNumberCode}{customerDTO.PhoneNumber}";
                            customerDTO.PhoneNumber = phone;

                        }
                    }

                    // create new
                    var individualCustomerDTO = Mapper.Map<IndividualCustomerDTO>(customerDTO);
                    if (await _uow.IndividualCustomer.ExistAsync(c => c.PhoneNumber == customerDTO.PhoneNumber.Trim()))
                    {
                        throw new GenericException($"Individual Customer Phone Number {customerDTO.PhoneNumber } Already Exist");
                    }

                    var newCustomer = Mapper.Map<IndividualCustomer>(individualCustomerDTO);

                    //generate customer code
                    var customerCode = await _numberGeneratorMonitorService.GenerateNextNumber(NumberGeneratorType.CustomerCodeIndividual);
                    newCustomer.CustomerCode = customerCode;

                    _uow.IndividualCustomer.Add(newCustomer);

                    await _uow.CompleteAsync();

                    customerDTO.IndividualCustomerId = newCustomer.IndividualCustomerId;
                    customerDTO.CustomerCode = newCustomer.CustomerCode;
                }
            }

            return customerDTO;
        }

        public async Task<MobilePriceDTO> GetGIGGOPrice(PreShipmentMobileDTO preShipment)
        {
            if (preShipment.Value > 0)
            {
                preShipment.PreShipmentItems[0].Value = preShipment.Value.ToString();
            }

            return await _gIGGoPricingService.GetGIGGOPrice(preShipment);
        }

        private async Task<decimal> RoundShipmentTotal(decimal number, int precision)
        {
            decimal factor = (decimal)Math.Pow(10, precision);
            return Math.Round(number * factor) / factor;
        }


        public async Task<List<CODShipmentDTO>> GetCODShipments(BaseFilterCriteria baseFilterCriteria)
        {
            try
            {
                var codShipments = await _uow.Shipment.GetCODShipments(baseFilterCriteria);
                if (codShipments.Any())
                {
                    var statuses = codShipments.Select(x => x.ShipmentScanStatus).ToList();
                    var scanST = _uow.ScanStatus.GetAll().Where(x => statuses.Contains(x.Code));
                    foreach (var item in codShipments)
                    {
                        item.ShipmentStatus = scanST.Where(x => x.Code == item.ShipmentScanStatus.ToString()).FirstOrDefault().Reason;
                    }
                }
                return codShipments;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task<List<CargoMagayaShipmentDTO>> GetCargoMagayaShipments(BaseFilterCriteria baseFilterCriteria)
        {
            try
            {
                var shipments = await _uow.Shipment.GetCargoMagayaShipments(baseFilterCriteria);
                return shipments;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> MarkMagayaShipmentsAsCargoed(List<CargoMagayaShipmentDTO> cargoMagayaShipmentDTOs)
        {
            bool result = true;
            try
            {
                if (cargoMagayaShipmentDTOs.Any())
                {
                    var waybills = cargoMagayaShipmentDTOs.Select(x => x.Waybill);
                    var waybillInfo = _uow.Shipment.GetAll().Where(x => waybills.Contains(x.Waybill)).ToList();
                    foreach (var item in cargoMagayaShipmentDTOs)
                    {
                        // update shipment to cargoed
                        var shipmentItem = waybillInfo.Where(x => x.Waybill == item.Waybill).FirstOrDefault();
                        shipmentItem.IsCargoed = true;
                        shipmentItem.DateModified = DateTime.Now;

                        var shipmentDTO = Mapper.Map<ShipmentDTO>(shipmentItem);

                        await _shipmentTrackingService.SendEmailToCustomerWhenIntlShipmentIsCargoed(shipmentDTO);
                    }
                    _uow.Complete();
                }
                return result;
            }
            catch (Exception ex)
            {
                result = false;
                throw;
            }
        }

        //DHL Get Price
        public async Task<TotalNetResult> GetInternationalShipmentPrice(InternationalShipmentDTO shipmentDTO)
        {
            try
            {
                var result = await _DhlService.GetInternationalShipmentPrice(shipmentDTO);
                return await GetTotalPriceBreakDown(result, shipmentDTO);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task<TotalNetResult> GetTotalPriceBreakDown(TotalNetResult total, InternationalShipmentDTO shipmentDTO)
        {
            var countryId = await _userService.GetUserActiveCountryId();

            var aditionalPrice = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.InternationalAdditionalPrice, countryId);
            var additionalAmount = Convert.ToDecimal(aditionalPrice.Value);
            total.Amount = total.Amount + additionalAmount;

            var vatDTO = await _uow.VAT.GetAsync(x => x.CountryId == countryId);
            decimal vat = (vatDTO != null) ? (vatDTO.Value / 100) : (7.5M / 100);
            total.VAT = total.Amount * vat;

            total.GrandTotal = total.Amount + total.VAT;

            //Get Insurance
            if (shipmentDTO.DeclarationOfValueCheck != null && shipmentDTO.DeclarationOfValueCheck > 0)
            {
                decimal insurance = (decimal)shipmentDTO.DeclarationOfValueCheck * 0.01M;
                total.GrandTotal = total.GrandTotal + insurance;
                total.Insurance = insurance;
            }

            return total;
        }

        //Add DHL International shipment 
        public async Task<ShipmentDTO> AddInternationalShipment(InternationalShipmentDTO shipmentDTO)
        {
            try
            {
                //1. Get the Price
                var price = await _DhlService.GetInternationalShipmentPrice(shipmentDTO);

                //update price to contain VAT, INSURANCE ETC
                var priceUpdate = await GetTotalPriceBreakDown(price, shipmentDTO);

                //validate the different from DHL
                if (shipmentDTO.GrandTotal != priceUpdate.GrandTotal)
                {
                    throw new GenericException($"There was an issue processing your request, shipment pricing is not accurate");
                }

                //if the customer is not an individual, pay by wallet
                //2. Get the Wallet Balance if payment is by wallet and check if the customer has the amount in its wallet
                if (shipmentDTO.PaymentType == PaymentType.Wallet)
                {

                }

                //Bind Agility Shipment Payload
                var shipment = await BindShipmentPayload(shipmentDTO);
                shipmentDTO.CustomerDetails = shipment.CustomerDetails;

                //update price to contain VAT, INSURANCE ETC
                shipment.Total = priceUpdate.Amount;
                shipment.GrandTotal = priceUpdate.GrandTotal;
                shipment.Insurance = priceUpdate.Insurance;
                shipment.Vat = priceUpdate.VAT;

                //Block account that has been suspended/pending from create shipment
                if (shipment.CustomerDetails.CustomerType == CustomerType.Company)
                {
                    if (shipment.CustomerDetails.CompanyStatus != CompanyStatus.Active)
                    {
                        throw new GenericException($"{shipment.CustomerDetails.Name} account has been {shipment.CustomerDetails.CompanyStatus}, contact support for assistance", $"{(int)HttpStatusCode.Forbidden}");
                    }
                }

                //3. Create shipment on DHL
                var dhlShipment = await _DhlService.CreateInternationalShipment(shipmentDTO);
                shipment.InternationalShipmentType = InternationalShipmentType.DHL;
                shipment.IsInternational = true;

                //4. Add the Shipment to Agility
                var createdShipment = await AddDHLShipmentToAgility(shipment, dhlShipment, shipmentDTO.PaymentType);
                return createdShipment;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task<ShipmentDTO> BindShipmentPayload(InternationalShipmentDTO shipmentDTO)
        {
            //Bind CustomerDetail to the payload
            var shipment = Mapper.Map<ShipmentDTO>(shipmentDTO);
            shipment.Customer = new List<CustomerDTO>();

            var customer = await HandleCustomer(shipmentDTO.CustomerDetails);
            shipment.CustomerDetails = customer;
            shipment.Customer.Add(shipmentDTO.CustomerDetails);
            shipment.CustomerType = shipment.CustomerDetails.CustomerType.ToString();
            shipment.CustomerCode = shipment.CustomerDetails.CustomerCode;

            if (CustomerType.Company.Equals(customer.CustomerType))
            {
                shipment.CustomerId = shipment.CustomerDetails.CompanyId;
                shipment.CompanyType = shipment.CustomerDetails.CompanyType.ToString();
            }
            else
            {
                shipment.CustomerId = shipment.CustomerDetails.IndividualCustomerId;
                shipment.CompanyType = shipment.CustomerDetails.CustomerType.ToString();
            }

            shipment.InternationalShipmentType = InternationalShipmentType.DHL;
            shipment.IsInternational = true;
            return shipment;
        }

        private async Task<CustomerDTO> HandleCustomer(CustomerDTO customerDetails)
        {
            //reset rowversion
            customerDetails.RowVersion = null;

            if (customerDetails.UserActiveCountryId == 0)
            {
                customerDetails.UserActiveCountryId = await GetUserCountryId();
            }

            var createdObject = await GetAndCreateCustomer(customerDetails);
            return createdObject;
        }

        private void AddDHLWaybill(InternationalShipmentWaybillDTO dhlWaybill)
        {
            var result = Mapper.Map<InternationalShipmentWaybill>(dhlWaybill);
            _uow.InternationalShipmentWaybill.Add(result);
        }

        private async Task<ShipmentDTO> AddDHLShipmentToAgility(ShipmentDTO shipmentDTO, InternationalShipmentWaybillDTO dhlShipment, PaymentType paymentType)
        {
            try
            {
                //Get Approximate Items Weight
                foreach (var item in shipmentDTO.ShipmentItems)
                {
                    shipmentDTO.ApproximateItemsWeight = shipmentDTO.ApproximateItemsWeight + item.Weight;
                }

                // create the shipment and shipmentItems
                var newShipment = await CreateInternationalShipmentOnAgility(shipmentDTO);
                shipmentDTO.DepartureCountryId = newShipment.DepartureCountryId;

                // create the Invoice and GeneralLedger
                await CreateInvoice(shipmentDTO);
                CreateGeneralLedger(shipmentDTO);

                //5. Add DHL Waybill Payload to the different table
                dhlShipment.Waybill = shipmentDTO.Waybill;
                AddDHLWaybill(dhlShipment);

                //QR Code
                await GenerateDeliveryNumber(shipmentDTO.Waybill);

                // complete transaction if all actions are successful
                await _uow.CompleteAsync();

                if (paymentType == PaymentType.Wallet)
                {
                    var walletEnumeration = await _uow.Wallet.FindAsync(x => x.CustomerCode.Equals(shipmentDTO.CustomerDetails.CustomerCode));
                    var wallet = walletEnumeration.FirstOrDefault();

                    if (wallet != null)
                    {
                        await _paymentService.ProcessPayment(new PaymentTransactionDTO()
                        {
                            PaymentType = PaymentType.Wallet,
                            TransactionCode = wallet.WalletNumber,
                            Waybill = newShipment.Waybill
                        });
                    }
                }

                //scan the shipment for tracking
                await ScanShipment(new ScanDTO
                {
                    WaybillNumber = newShipment.Waybill,
                    ShipmentScanStatus = ShipmentScanStatus.CRT
                });

                //For Corporate Customers, Pay for their shipments through wallet immediately
                if (CustomerType.Company.ToString() == shipmentDTO.CustomerType)
                {
                    var walletEnumeration = await _uow.Wallet.FindAsync(x => x.CustomerCode == shipmentDTO.CustomerCode);
                    var wallet = walletEnumeration.FirstOrDefault();

                    if (wallet != null)
                    {
                        await _paymentService.ProcessPayment(new PaymentTransactionDTO()
                        {
                            PaymentType = PaymentType.Wallet,
                            TransactionCode = wallet.WalletNumber,
                            Waybill = newShipment.Waybill
                        });
                    }
                }

                return newShipment;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private async Task<ShipmentDTO> CreateInternationalShipmentOnAgility(ShipmentDTO shipmentDTO)
        {
            //Make GIGGO as Destination Service centre or Create a new service centre for this
            var destination = await _userService.GetInternationalOutBoundServiceCentre();
            shipmentDTO.DestinationServiceCentreId = destination.ServiceCentreId;

            //set HOME SERVICE ON-FORWARDING LOS (LPPO) as default Delivery option for international shipment
            shipmentDTO.DeliveryOptionId = 6;
            shipmentDTO.DeliveryOptionIds.Add(6);

            // get the current user info
            var currentUserId = await _userService.GetCurrentUserId();
            var serviceCenterIds = await _userService.GetPriviledgeServiceCenters();

            shipmentDTO.DepartureServiceCentreId = serviceCenterIds[0];
            shipmentDTO.UserId = currentUserId;
            var departureServiceCentre = await _centreService.GetServiceCentreById(shipmentDTO.DepartureServiceCentreId);
            var waybill = await _numberGeneratorMonitorService.GenerateNextNumber(NumberGeneratorType.WaybillNumber, departureServiceCentre.Code);
            shipmentDTO.Waybill = waybill;
            var newShipment = Mapper.Map<Shipment>(shipmentDTO);

            // set declared value of the shipment
            if (shipmentDTO.IsdeclaredVal)
            {
                newShipment.DeclarationOfValueCheck = shipmentDTO.DeclarationOfValueCheck;
            }
            else
            {
                newShipment.DeclarationOfValueCheck = 0.00M;
            }

            newShipment.ApproximateItemsWeight = 0;

            // add serial numbers to the ShipmentItems
            var serialNumber = 1;

            var numOfPackages = shipmentDTO.PackageOptionIds.Count;
            var numOfShipmentItems = newShipment.ShipmentItems.Count;

            if (numOfPackages > numOfShipmentItems)
            {
                throw new GenericException("Number of Packages should not be more then Shipment Items!", $"{(int)HttpStatusCode.BadRequest}");
            }

            if (shipmentDTO.PackageOptionIds.Any())
            {
                await UpdatePackageTransactions(shipmentDTO);
            }

            for (var i = 0; i < numOfPackages; i++)
            {
                newShipment.ShipmentItems[i].ShipmentPackagePriceId = shipmentDTO.PackageOptionIds[i];
                newShipment.ShipmentItems[i].PackageQuantity = 1;
            }

            foreach (var shipmentItem in newShipment.ShipmentItems)
            {
                shipmentItem.SerialNumber = serialNumber;

                //sum item weight 
                //check for volumetric weight
                if (shipmentItem.IsVolumetric)
                {
                    double volume = (shipmentItem.Length * shipmentItem.Height * shipmentItem.Width) / 5000;
                    double Weight = shipmentItem.Weight > volume ? shipmentItem.Weight : volume;

                    newShipment.ApproximateItemsWeight += Weight;
                }
                else
                {
                    newShipment.ApproximateItemsWeight += shipmentItem.Weight;
                }
                serialNumber++;
            }

            //do not save the child objects
            //newShipment.DepartureServiceCentre = null;
            //newShipment.DestinationServiceCentre = null;
            newShipment.DeliveryOption = null;

            //save the display value of Insurance and Vat
            newShipment.Vat = shipmentDTO.vatvalue_display;
            newShipment.DiscountValue = shipmentDTO.InvoiceDiscountValue_display;

            var departureCountry = await _uow.Country.GetCountryByServiceCentreId(shipmentDTO.DepartureServiceCentreId);
            newShipment.DepartureCountryId = departureCountry.CountryId;
            newShipment.CurrencyRatio = departureCountry.CurrencyRatio;
            newShipment.ShipmentPickupPrice = shipmentDTO.ShipmentPickupPrice;

            //Make GiGo Shipments not available for grouping
            if (shipmentDTO.IsGIGGOExtension)
            {
                newShipment.IsGrouped = true;
            }
            _uow.Shipment.Add(newShipment);

            //save into DeliveryOptionMapping table
            foreach (var deliveryOptionId in shipmentDTO.DeliveryOptionIds)
            {
                var deliveryOptionMapping = new ShipmentDeliveryOptionMapping()
                {
                    Waybill = newShipment.Waybill,
                    DeliveryOptionId = deliveryOptionId
                };
                _uow.ShipmentDeliveryOptionMapping.Add(deliveryOptionMapping);
            }

            //set before returning
            shipmentDTO.DepartureCountryId = departureCountry.CountryId;
            return shipmentDTO;
        }


        public async Task<ShipmentDTO> ProcessInternationalShipmentOnAgility(ShipmentDTO shipmentDTO)
        {
            //Update SenderAddress for corporate customers
            shipmentDTO.SenderAddress = null;
            shipmentDTO.SenderState = null;
            shipmentDTO.ShipmentScanStatus = ShipmentScanStatus.CRT;
            if (shipmentDTO.Customer[0].CompanyType == CompanyType.Corporate)
            {
                shipmentDTO.SenderAddress = shipmentDTO.Customer[0].Address;
                shipmentDTO.SenderState = shipmentDTO.Customer[0].State;
            }
            var shipments = _uow.IntlShipmentRequest.GetAll("ShipmentRequestItems").Where(x => x.RequestNumber == shipmentDTO.RequestNumber).FirstOrDefault();
            var intlRequest = await _uow.IntlShipmentRequest.GetAsync(x => x.RequestNumber == shipmentDTO.RequestNumber);
            if (intlRequest.RequestProcessingCountryId == 0)
            {
                shipmentDTO.DepartureCountryId = 207;
            }
            else
            {
                shipmentDTO.DepartureCountryId = intlRequest.RequestProcessingCountryId;
            }
            var centre = await _centreService.GetServiceCentresBySingleCountry(shipmentDTO.DepartureCountryId);
            if (centre.Any())
            {
                shipmentDTO.DepartureServiceCentreId = centre.FirstOrDefault().ServiceCentreId;
            }

            //filter phone number issue
            if (!String.IsNullOrEmpty(shipmentDTO.ReceiverPhoneNumber))
            {
                shipmentDTO.ReceiverPhoneNumber = shipmentDTO.ReceiverPhoneNumber.Trim();
            }

            if (!String.IsNullOrEmpty(shipmentDTO.Customer[0].PhoneNumber))
            {
                shipmentDTO.Customer[0].PhoneNumber = shipmentDTO.Customer[0].PhoneNumber.Trim();
            }

            //set some data to null
            shipmentDTO.ShipmentCollection = null;
            shipmentDTO.Demurrage = null;
            shipmentDTO.Invoice = null;
            shipmentDTO.ShipmentCancel = null;
            shipmentDTO.ShipmentReroute = null;
            shipmentDTO.DeliveryOption = null;
            shipmentDTO.IsInternational = true;
            var shipment = await AddShipment(shipmentDTO);
            if (!String.IsNullOrEmpty(shipment.Waybill))
            {
                var invoiceObj = await _uow.Invoice.GetAsync(X => X.Waybill == shipment.Waybill);
                if (invoiceObj == null || (invoiceObj != null && invoiceObj.PaymentStatus == PaymentStatus.Pending))
                {
                    shipment.CustomerDetails = shipment.Customer[0];
                    var dest = await _centreService.GetServiceCentreById(shipment.DestinationServiceCentreId);
                    var dept = await _centreService.GetServiceCentreById(shipment.DepartureServiceCentreId);
                    shipment.DestinationServiceCentre = dest;
                    shipment.DepartureServiceCentre = dept;
                    shipment.SenderCode = shipment.CustomerDetails.CustomerCode;

                    //await _messageSenderService.SendGenericEmailMessage(MessageType.INTLPEMAIL, shipment);

                    //Get the two possible payment links for Waybill (Nigeria  and US)
                    var waybillPayment = new WaybillPaymentLogDTO()
                    {
                        Waybill = shipment.Waybill,
                        OnlinePaymentType = OnlinePaymentType.Paystack,
                        Email = shipment.Customer[0].Email
                    };

                    int[] listOfCountryForPayment = { 1, 207 };
                    List<string> paymentLinks = new List<string>();
                    foreach ( var country in listOfCountryForPayment)
                    {
                        waybillPayment.PaymentCountryId = country;
                        waybillPayment.PaystackCountrySecret = "PayStackLiveSecret";
                        var response = await _waybillPaymentLogService.AddWaybillPaymentLogForIntlShipment(waybillPayment);
                        paymentLinks.Add(response.data.Authorization_url);
                    }
                    
                    await _messageSenderService.SendOverseasShipmentReceivedMails(shipment, paymentLinks);
                }

                // get the current user info
                var currentUserId = await _userService.GetCurrentUserId();
                var user = await _userService.GetUserById(currentUserId);
                intlRequest.IsProcessed = true;
                foreach (var item in intlRequest.ShipmentRequestItems)
                {
                    item.Received = true;
                    item.ReceivedBy = user.FirstName + " " + user.LastName;
                }
                _uow.Complete();
            }
            return shipmentDTO;
        }

    }
}