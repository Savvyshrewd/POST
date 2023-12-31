﻿using POST.Core.IServices.CustomerPortal;
using System.Collections.Generic;
using System.Threading.Tasks;
using POST.Core.DTO.Account;
using POST.Core.DTO.Shipments;
using POST.Core.IServices.Shipments;
using POST.Core;
using System.Linq;
using POST.Core.IServices.Account;
using POST.Core.IServices.Business;
using POST.Core.IServices.User;
using POST.Core.DTO.PaymentTransactions;
using POST.Core.DTO.Zone;
using POST.Core.DTO.ServiceCentres;
using POST.Core.DTO;
using POST.Core.IServices.Customers;
using System;
using POST.CORE.DTO.Shipments;
using POST.Core.DTO.User;
using POST.Core.Enums;
using POST.Core.IServices;
using POST.Core.IServices.Zone;
using POST.CORE.IServices.Shipments;
using POST.CORE.DTO.Report;
using POST.Core.IServices.TickectMan;
using POST.Core.IServices.ServiceCentres;
using Newtonsoft.Json.Linq;
using POST.Core.IServices.Wallet;
using POST.Core.DTO.DHL;
using AutoMapper;

namespace POST.Services.Business.CustomerPortal
{
    public class TickectManService : ITickectManService
    {
        private readonly IUnitOfWork _uow;
        private readonly IInvoiceService _invoiceService;
        private readonly IShipmentTrackService _iShipmentTrackService;
        private readonly IUserService _userService;
        private readonly IDeliveryOptionPriceService _deliveryOptionPriceService;
        private readonly IDomesticRouteZoneMapService _domesticRouteZoneMapService;
        private readonly IShipmentService _shipmentService;
        private readonly IShipmentPackagePriceService _packagePriceService;
        private readonly ICustomerService _customerService;
        private readonly IPricingService _pricing;
        private readonly IPaymentService _paymentService;
        private readonly ICustomerPortalService _portalService;
        private readonly IShipmentCollectionService _shipmentCollectionService;
        private readonly IServiceCentreService _serviceCentreService;
        private readonly ICountryService _countryService;
        private readonly ILGAService _lgaService;
        private readonly ISpecialDomesticPackageService _specialPackageService;
        private readonly ICellulantPaymentService _cellulantService;
        private readonly ICustomerPortalService _customerPortalService;
        private readonly IInsuranceService _insuranceService;
        private readonly IVATService _vatService;

        public TickectManService(IUnitOfWork uow, IDeliveryOptionPriceService deliveryOptionPriceService, IDomesticRouteZoneMapService domesticRouteZoneMapService, IShipmentService shipmentService,
           IShipmentPackagePriceService packagePriceService, ICustomerService customerService, IPricingService pricing,
           IPaymentService paymentService, ICustomerPortalService portalService, IShipmentCollectionService shipmentCollectionService, IServiceCentreService serviceCentreService, IUserService userService, ICountryService countryService, ILGAService lgaService,
           ISpecialDomesticPackageService specialPackageService, IInvoiceService invoiceService, ICellulantPaymentService cellulantService, ICustomerPortalService customerPortalService, IInsuranceService insuranceService, IVATService vatService)
        {
            _uow = uow;
            _deliveryOptionPriceService = deliveryOptionPriceService;
            _domesticRouteZoneMapService = domesticRouteZoneMapService;
            _shipmentService = shipmentService;
            _packagePriceService = packagePriceService;
            _customerService = customerService;
            _pricing = pricing;
            _paymentService = paymentService;
            _portalService = portalService;
            _shipmentCollectionService = shipmentCollectionService;
            _serviceCentreService = serviceCentreService;
            _userService = userService;
            _countryService = countryService;
            _lgaService = lgaService;
            _specialPackageService = specialPackageService;
            _invoiceService = invoiceService;
            _cellulantService = cellulantService;
            _customerPortalService = customerPortalService;
            _insuranceService = insuranceService;
            _vatService = vatService;
        }

        public async Task<ShipmentDTO> AddShipment(NewShipmentDTO newShipmentDTO)
        {
            //map to real shipmentdto
            var ShipmentDTO = JObject.FromObject(newShipmentDTO).ToObject<ShipmentDTO>();
            //Update SenderAddress for corporate customers
            ShipmentDTO.SenderAddress = null;
            ShipmentDTO.SenderState = null;
            if (ShipmentDTO.Customer[0].CompanyType == CompanyType.Corporate)
            {
                ShipmentDTO.SenderAddress = ShipmentDTO.Customer[0].Address;
                ShipmentDTO.SenderState = ShipmentDTO.Customer[0].State;
            }

            //set some data to null
            ShipmentDTO.ShipmentCollection = null;
            ShipmentDTO.Demurrage = null;
            ShipmentDTO.Invoice = null;
            ShipmentDTO.ShipmentCancel = null;
            ShipmentDTO.ShipmentReroute = null;
            ShipmentDTO.DeliveryOption = null;
            ShipmentDTO.IsFromMobile = false;
            ShipmentDTO.IsBulky = newShipmentDTO.IsBulky;
            ShipmentDTO.ExpressDelivery = newShipmentDTO.ExpressDelivery;

            //Add Insurance value to shipment table
            var insuranceDTO = await _insuranceService.GetInsuranceByCountry();
            ShipmentDTO.Insurance = insuranceDTO.Value;

            //Add Vat value to shipment table
            var vatDTO = await _vatService.GetVATByCountry();
            ShipmentDTO.Vat = vatDTO.Value;

            //
            ShipmentDTO.insurancevalue_display = (ShipmentDTO.Insurance / 100) * ShipmentDTO.DeclarationOfValueCheck;
            ShipmentDTO.vatvalue_display = (ShipmentDTO.Vat / 100) * ShipmentDTO.Total;

            var shipment = await _shipmentService.AddShipment(ShipmentDTO);
            if (!String.IsNullOrEmpty(shipment.Waybill))
            {
                var invoiceObj = await _invoiceService.GetInvoiceByWaybill(shipment.Waybill);
                if (invoiceObj != null)
                {
                    shipment.Invoice = invoiceObj;
                    shipment.DepartureServiceCentre = invoiceObj.Shipment.DepartureServiceCentre;
                    shipment.DestinationServiceCentre = invoiceObj.Shipment.DestinationServiceCentre;
                }
            }
            return shipment;
        }


        public async Task<IEnumerable<CountryDTO>> GetActiveCountries()
        {
            return await _countryService.GetActiveCountries();
        }

        public async Task<IEnumerable<SpecialDomesticPackageDTO>> GetActiveSpecialDomesticPackages()
        {
            return await _specialPackageService.GetActiveSpecialDomesticPackages();
        }

        public async Task<object> GetCustomerBySearchParam(string customerType, SearchOption option)
        {
            return await _customerService.GetCustomerBySearchParam(customerType, option);
        }

        public async Task<IEnumerable<DeliveryOptionPriceDTO>> GetDeliveryOptionPrices()
        {
            var deliveryOption = await _deliveryOptionPriceService.GetDeliveryOptionPrices();
            // deliveryOption = deliveryOption.Where(x => x.Price > 0).ToList();
            return deliveryOption;
        }

        public async Task<ShipmentDTO> GetDropOffShipmentForProcessing(string code)
        {
            return await _shipmentService.GetDropOffShipmentForProcessing(code);
        }

        public async Task<NewPricingDTO> GetGrandPriceForShipment(NewShipmentDTO newShipmentDTO)
        {
            return await _pricing.GetGrandPriceForShipment(newShipmentDTO);
        }


        public async Task<IEnumerable<LGADTO>> GetLGAs()
        {
            return await _lgaService.GetLGAs();
        }

        public async Task<decimal> GetPrice(PricingDTO pricingDto)
        {
            var userCountryId = await _pricing.GetUserCountryId();
            pricingDto.CountryId = userCountryId;
            return await _pricing.GetPrice(pricingDto);
        }

        //public async Task<MobilePriceDTO> GetPriceForDropOff(PreShipmentMobileDTO preshipmentMobile)
        //{
        //    var dropOffPrice = await _portalService.GetPriceForDropOff(preshipmentMobile);
        //    //apply dropoff price
        //    var countryId = await _userService.GetUserActiveCountryId();
        //    var discount = await _uow.GlobalProperty.GetAsync(x => x.Key == GlobalPropertyType.GIGGODropOffDiscount.ToString() && x.CountryId == countryId);
        //    if (discount != null)
        //    {
        //        var discountValue = Convert.ToDecimal(discount.Value);
        //        decimal discountResult = (discountValue / 100M);
        //        dropOffPrice.Discount = dropOffPrice.GrandTotal * discountResult;
        //        dropOffPrice.GrandTotal = dropOffPrice.GrandTotal - dropOffPrice.Discount;                  
        //    }
        //    var factor = Convert.ToDecimal(Math.Pow(10, -2));
        //    dropOffPrice.GrandTotal = Math.Round(dropOffPrice.GrandTotal.Value * factor) / factor;
        //    return dropOffPrice;
        //}

        public async Task<NewPricingDTO> GetPriceForDropOff(NewShipmentDTO newShipmentDTO)
        {
            var dropOffPrice = await _pricing.GetGrandPriceForShipment(newShipmentDTO);
            var countryId = await _userService.GetUserActiveCountryId();
            var discount = await _uow.GlobalProperty.GetAsync(x => x.Key == GlobalPropertyType.GIGGODropOffDiscount.ToString() && x.CountryId == countryId);
            if (discount != null)
            {
                var discountValue = Convert.ToDecimal(discount.Value);
                decimal discountResult = (discountValue / 100M);
                dropOffPrice.DiscountedValue = dropOffPrice.GrandTotal * discountResult;
                dropOffPrice.GrandTotal = dropOffPrice.GrandTotal - dropOffPrice.DiscountedValue;
            }
        
            decimal factor = 0;
            if (newShipmentDTO.CompanyType == CompanyType.Corporate.ToString() || newShipmentDTO.CompanyType == CompanyType.Ecommerce.ToString())
            {
                factor = Convert.ToDecimal(Math.Pow(10, 0));
            }
            else
            {
                factor = Convert.ToDecimal(Math.Pow(10, -2));
            }
            dropOffPrice.GrandTotal = Math.Round(dropOffPrice.GrandTotal * factor) / factor;
            return dropOffPrice;
        }

        public async Task<DailySalesDTO> GetSalesForServiceCentre(DateFilterForDropOff dateFilterCriteria)
        {
            var accountFilterCriteria = JObject.FromObject(dateFilterCriteria).ToObject<AccountFilterCriteria>();
            var dailySales = await _shipmentService.GetSalesForServiceCentre(accountFilterCriteria);
            if (dailySales.TotalSales > 0)
            {
                var factor = Convert.ToDecimal(Math.Pow(10, -2));
                dailySales.TotalSales = Math.Round(dailySales.TotalSales * factor) / factor;
            }
            return dailySales;
        }

        public async Task<List<ServiceCentreDTO>> GetActiveServiceCentresBySingleCountry(int countryId)
        {
            //2. priviledged users service centres
            //var usersServiceCentresId = await _userService.GetPriviledgeServiceCenters();
            //var serviceCenterIds = await _uow.ServiceCentre.GetAsync(usersServiceCentresId[0]);

            int stationId = 0;
            //if (serviceCenterIds.StationId == 4)
            //{
            //    stationId = serviceCenterIds.StationId;
            //}

            return await _portalService.GetActiveServiceCentresBySingleCountry(countryId, stationId);
        }

        public async Task<ShipmentDTO> GetShipment(string waybill)
        {
            var shipment = await _shipmentService.GetShipment(waybill);

            //Get the ETA for the shipment
            int eta = _uow.DomesticRouteZoneMap.GetAllAsQueryable()
                .Where(x => x.DepartureId == shipment.DepartureServiceCentre.StationId
                && x.DestinationId == shipment.DestinationServiceCentre.StationId).Select(x => x.ETA).FirstOrDefault();
            shipment.ETA = eta;

            if (shipment.GrandTotal > 0)
            {
                decimal factor = 0;
                if (shipment.CompanyType == CompanyType.Corporate.ToString() || shipment.CompanyType == CompanyType.Ecommerce.ToString())
                {
                    factor = Convert.ToDecimal(Math.Pow(10, 0));
                }
                else
                {
                    factor = Convert.ToDecimal(Math.Pow(10, -2));
                }
                shipment.GrandTotal = Math.Round(shipment.GrandTotal * factor) / factor;
                shipment.Vat = Math.Round((decimal)shipment.Vat * factor) / factor;
                shipment.vatvalue_display = Math.Round((decimal)shipment.vatvalue_display * factor) / factor;
                shipment.Total = Math.Round((decimal)shipment.Total * factor) / factor;
                shipment.DiscountValue = Math.Round((decimal)shipment.DiscountValue * factor) / factor;
                shipment.InvoiceDiscountValue_display = Math.Round((decimal)shipment.InvoiceDiscountValue_display * factor) / factor;
                shipment.offInvoiceDiscountvalue_display = Math.Round((decimal)shipment.InvoiceDiscountValue_display * factor) / factor;
                shipment.Insurance = Math.Round((decimal)shipment.Insurance * factor) / factor;
                shipment.CashOnDeliveryAmount = Math.Round((decimal)shipment.CashOnDeliveryAmount * factor) / factor;
                shipment.DeclarationOfValueCheck = Math.Round((decimal)shipment.DeclarationOfValueCheck * factor) / factor;

                foreach (var item in shipment.ShipmentItems)
                {
                    item.Price = Math.Round(item.Price * factor) / factor;
                }
            }
            if (!String.IsNullOrEmpty(shipment.Waybill))
            {
                var invoiceObj = await _invoiceService.GetInvoiceByWaybill(shipment.Waybill);
                if (invoiceObj != null)
                {
                    shipment.Invoice = invoiceObj;
                    if (shipment.Invoice.Shipment.GrandTotal > 0)
                    {
                        decimal factor = 0;
                        if (shipment.CompanyType == CompanyType.Corporate.ToString() || shipment.CompanyType == CompanyType.Ecommerce.ToString())
                        {
                            factor = Convert.ToDecimal(Math.Pow(10, 0));
                        }
                        else
                        {
                            factor = Convert.ToDecimal(Math.Pow(10, -2));
                        }
                        shipment.Invoice.Shipment.GrandTotal = Math.Round(shipment.GrandTotal * factor) / factor;
                        shipment.Invoice.Shipment.Vat = Math.Round((decimal)shipment.Vat * factor) / factor;
                        shipment.Invoice.Shipment.vatvalue_display = Math.Round((decimal)shipment.vatvalue_display * factor) / factor;
                        shipment.Invoice.Shipment.Total = Math.Round((decimal)shipment.Total * factor) / factor;
                        shipment.Invoice.Shipment.DiscountValue = Math.Round((decimal)shipment.DiscountValue * factor) / factor;
                        shipment.Invoice.Shipment.InvoiceDiscountValue_display = Math.Round((decimal)shipment.InvoiceDiscountValue_display * factor) / factor;
                        shipment.Invoice.Shipment.offInvoiceDiscountvalue_display = Math.Round((decimal)shipment.InvoiceDiscountValue_display * factor) / factor;
                        shipment.Invoice.Shipment.Insurance = Math.Round((decimal)shipment.Insurance * factor) / factor;
                        shipment.Invoice.Shipment.CashOnDeliveryAmount = Math.Round((decimal)shipment.CashOnDeliveryAmount * factor) / factor;
                        shipment.Invoice.Shipment.DeclarationOfValueCheck = Math.Round((decimal)shipment.DeclarationOfValueCheck * factor) / factor;
                        shipment.Invoice.Pos = Math.Round((decimal)shipment.Invoice.Pos * factor) / factor;


                        foreach (var item in shipment.Invoice.Shipment.ShipmentItems)
                        {
                            item.Price = Math.Round(item.Price * factor) / factor;
                        }
                    }
                }
            }

            //Calculate Insurance and vat value
            shipment.Invoice.Shipment.insurancevalue_display = (shipment.Invoice.Shipment.Insurance / 100) * shipment.Invoice.Shipment.DeclarationOfValueCheck;

            return shipment;
        }

        public async Task<ShipmentCollectionDTO> GetShipmentCollectionById(string waybill)
        {
            return await _shipmentCollectionService.GetShipmentCollectionById(waybill);
        }

        public async Task<List<ShipmentPackagePriceDTO>> GetShipmentPackagePrices()
        {
            return await _packagePriceService.GetShipmentPackagePrices();
        }

        public async Task<DailySalesDTO> GetWaybillForServiceCentre(string waybill)
        {
            return await _shipmentService.GetWaybillForServiceCentre(waybill);
        }

        public async Task<DomesticRouteZoneMapDTO> GetZone(int departure, int destination)
        {
            return await _domesticRouteZoneMapService.GetZone(departure, destination);
        }

        public async Task<bool> ProcessPayment(PaymentTransactionDTO paymentDto)
        {
            return await _paymentService.ProcessPayment(paymentDto);
        }

        public async Task<bool> ProcessPaymentPartial(PaymentPartialTransactionProcessDTO paymentPartialTransactionProcessDTO)
        {
            return await _paymentService.ProcessPaymentPartial(paymentPartialTransactionProcessDTO);
        }

        public async Task ReleaseShipmentForCollection(ShipmentCollectionDTOForFastTrack shipmentCollectionforDto)
        {
            var shipmentCollection = JObject.FromObject(shipmentCollectionforDto).ToObject<ShipmentCollectionDTO>();
            shipmentCollection.ShipmentScanStatus = ShipmentScanStatus.OKT;
            if (shipmentCollection.IsComingFromDispatch)
            {
                shipmentCollection.ShipmentScanStatus = ShipmentScanStatus.OKC;
            }
            shipmentCollectionforDto.UserId = await _userService.GetCurrentUserId();
            await _shipmentCollectionService.ReleaseShipmentForCollection(shipmentCollection);
        }

        public async Task<ServiceCentreDTO> GetServiceCentreById(int centreid)
        {

            return await _serviceCentreService.GetServiceCentreById(centreid);
        }

        public async Task<UserDTO> CheckDetailsForMobileScanner(string user)
        {

            return await _portalService.CheckDetailsForMobileScanner(user);
        }

        public async Task<int[]> GetPriviledgeServiceCenters(string userId)
        {
            return await _userService.GetPriviledgeServiceCenters(userId);
        }
        public async Task<PreShipmentSummaryDTO> GetShipmentDetailsFromDeliveryNumber(string DeliveryNumber)
        {
            var result = await _portalService.GetShipmentDetailsFromDeliveryNumber(DeliveryNumber);
            if (result != null && result.shipmentdetails != null)
            {
                decimal factor = 0;

                if (result.shipmentdetails.CompanyType == CompanyType.Corporate.ToString() || result.shipmentdetails.CompanyType == CompanyType.Ecommerce.ToString())
                {
                    factor = Convert.ToDecimal(Math.Pow(10, 0));
                }
                else
                {
                    factor = Convert.ToDecimal(Math.Pow(10, -2));
                }
                result.shipmentdetails.DiscountValue = Math.Round((decimal)result.shipmentdetails.DiscountValue * factor) / factor;
                result.shipmentdetails.Value = Math.Round((decimal)result.shipmentdetails.Value * factor) / factor;
                if (result.shipmentdetails.CashOnDeliveryAmount != null)
                {
                    result.shipmentdetails.CashOnDeliveryAmount = Math.Round((decimal)result.shipmentdetails.CashOnDeliveryAmount * factor) / factor; 
                }

            }
            return result;
        }
        public async Task<bool> ApproveShipment(ApproveShipmentDTO detail)
        {
            return await _portalService.ApproveShipment(detail);
        }

        public async Task<IEnumerable<ServiceCentreDTO>> GetServiceCentreByStation(int stationId)
        {

            return await _serviceCentreService.GetServiceCentresByStationId(stationId);
        }

        public async Task<ShipmentDTO> AddAgilityShipmentToGIGGo(PreShipmentMobileFromAgilityDTO shipment)
        {

            return await _shipmentService.AddAgilityShipmentToGIGGo(shipment);
        }

        public async Task<MobilePriceDTO> GetGIGGOPrice(PreShipmentMobileDTO preShipment)
        {
            preShipment.IsFromAgility = true;
            if (preShipment.Value > 0)
            {
                preShipment.PreShipmentItems[0].Value = preShipment.Value.ToString();
            }
            return await _shipmentService.GetGIGGOPrice(preShipment);
        }

        public async Task<List<InvoiceViewDTO>> GetInvoiceByServiceCentre()
        {
            var serviceCentreId = await _userService.GetPriviledgeServiceCenters();
            var items = await _invoiceService.GetInvoiceByServiceCentre(serviceCentreId[0]);
            if (items.Any())
            {
                foreach (var item in items)
                {
                    if (item.CompanyType == CustomerType.IndividualCustomer.ToString())
                    {
                        var cust = await _uow.IndividualCustomer.GetAsync(x => x.CustomerCode == item.CustomerCode);
                        if (cust != null)
                        {
                            item.SenderName = cust.FirstName + " " + cust.LastName;
                        }
                    }
                    else
                    {
                        var cust = await _uow.Company.GetAsync(x => x.CustomerCode == item.CustomerCode);
                        if (cust != null)
                        {
                            item.SenderName = cust.FirstName + " " + cust.LastName;
                        }
                    }
                }
            }
            return items;
        }
        public async Task<bool> ProcessBulkPaymentforWaybills(BulkWaybillPaymentDTO bulkWaybillPaymentDTO)
        {
            return await _invoiceService.ProcessBulkPaymentforWaybills(bulkWaybillPaymentDTO);
        }

        public async Task<List<TransferDetailsDTO>> GetTransferDetails(BaseFilterCriteria baseFilter)
        {
            return await _cellulantService.GetTransferDetails(baseFilter);
        }

        public async Task<List<TransferDetailsDTO>> GetTransferDetailsByAccountNumber(BaseFilterCriteria baseFilter)
        {
            return await _cellulantService.GetTransferDetailsByAccountNumber(baseFilter);
        }

        public async Task<IEnumerable<CountryDTO>> GetCountries()
        {
            return await _countryService.GetCountries();
        }

        public async Task<List<TotalNetResult>> GetInternationalshipmentQuote(InternationalShipmentQuoteDTO quoteDTO)
        {
            try
            {
                var shipment = Mapper.Map<InternationalShipmentDTO>(quoteDTO);
                return await _shipmentService.GetInternationalShipmentPrice(shipment);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<MobilePriceDTO> GetPriceQoute(PreShipmentMobileDTO preShipment)
        {
            return await _customerPortalService.GetPriceQoute(preShipment);
        }

        public async Task<List<TotalNetResult>> GetInternationalshipmentRate(RateInternationalShipmentDTO rateDTO)
        {
            return await _customerPortalService.GetInternationalshipmentRate(rateDTO);
        }

        public async Task<ShipmentDTO> AddInternationalShipment(InternationalShipmentDTO shipmentDTO)
        {
            return await _shipmentService.AddInternationalShipment(shipmentDTO);
        }

    }
}