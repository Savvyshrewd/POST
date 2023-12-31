﻿using AutoMapper;
using POST.Core;
using POST.Core.DTO.Account;
using POST.Core.DTO.Admin;
using POST.Core.DTO.Customers;
using POST.Core.DTO.Report;
using POST.Core.DTO.ServiceCentres;
using POST.Core.Enums;
using POST.Core.IServices.Report;
using POST.Core.IServices.ServiceCentres;
using POST.Core.View;
using POST.Core.View.AdminReportView;
using POST.CORE.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POST.Services.Implementation.Report
{
    public class AdminReportService : IAdminReportService
    {
        private readonly IUnitOfWork _uow;
        private IServiceCentreService _serviceCenterService;

        public AdminReportService(IUnitOfWork uow, IServiceCentreService serviceCenterService)
        {
            _uow = uow;
            _serviceCenterService = serviceCenterService;
        }

        public async Task<AdminReportDTO> GetAdminReport(ShipmentCollectionFilterCriteria filterCriteria)
        {
            AdminReportDTO result = new AdminReportDTO
            {
                BusiestRoute = await GetBusiestRoutes(),
                TotalServiceCentreByState = await GetTotalServiceCentreByStates()

                //result.AllTimeSalesByCountry = await GetAllTimeSalesByCountries();
                //result.CustomerRevenue = await GetCustomerRevenues();
                //result.RevenuePerServiceCentre = await GetRevenuePerServiceCentres();
                //result.TotalOrdersDelivered = await GetTotalOrdersDelivered();
                //MostShippedItemByWeight = await GetMostShippedItemByWeights(),
            };

            //Get customer count
            result.NumberOfCustomer.Corporate = await GetCorporateCount(filterCriteria);
            result.NumberOfCustomer.Ecommerce = await GetEcommerceCount(filterCriteria);
            result.NumberOfCustomer.Individual = await GetIndividaulCount(filterCriteria);

            result.InvoiceReportDTO = await InvoiceInformation(filterCriteria);

            return result;
        }

        //To display data for the website
        public async Task<AdminReportDTO> DisplayWebsiteData()
        {
            AdminReportDTO result = new AdminReportDTO();
            
            result.NumberOfCustomer.Ecommerce = await GetEcommerceCount();
            result.NumberOfCustomer.Individual = await GetIndividaulCount();
            result.NumberOfCustomer.Corporate = await GetCorporateCount();
            result.TotalCustomers = result.NumberOfCustomer.Ecommerce + result.NumberOfCustomer.Individual + result.NumberOfCustomer.Corporate;

            result.TotalOrdersDelivered = await GetTotalOrdersDelivered();
            return result;
        }
                
        private async Task<List<Report_AllTimeSalesByCountry>> GetAllTimeSalesByCountries()
        {
            var result = _uow.Invoice.GetAllTimeSalesByCountry().ToList();
            return await Task.FromResult(result);
        }

        private async Task<List<Report_BusiestRoute>> GetBusiestRoutes()
        {
            var result = _uow.Invoice.GetBusiestRoute().OrderByDescending(x => x.TotalShipment).Take(5).ToList();
            return await Task.FromResult(result);
        }

        private async Task<List<Report_CustomerRevenue>> GetCustomerRevenues()
        {
            var result = _uow.Invoice.GetCustomerRevenue().ToList();
            return await Task.FromResult(result);
        }

        private async Task<InvoiceReportDTO> InvoiceInformation(ShipmentCollectionFilterCriteria filterCriteria)
        {
            var result = new InvoiceReportDTO();
            var invoice = _uow.Invoice.GetAllFromInvoiceAndShipments(filterCriteria).ToList();

            List<InvoiceView> invoices;
            List<InvoiceView> destInvoices;
            decimal avgDestShipmentCostPerSC = 0;
            decimal avgOriginShipmentCostPerSC = 0;
            var createdShipments = 0;
            var departedShipments = 0;
            int[] serviceCenterInStationIds = null;
            decimal avgStationShipments = 0;
            decimal avgOutStationShipments = 0;

            //Filter by Service Center
            if (filterCriteria.ServiceCentreId > 0)
            {
                //For the Departure Service Center Data Only
                invoices = invoice.Where(s => s.DepartureServiceCentreId == filterCriteria.ServiceCentreId).ToList();

                //For the Detination Service Center Data Only, so as to calaulate the average coming in
                destInvoices = invoice.Where(s => s.DestinationServiceCentreId == filterCriteria.ServiceCentreId).ToList();

                //Average Price Of Shipments coming to that service Center
                avgDestShipmentCostPerSC = (destInvoices.Sum(p => p.GrandTotal) / ((destInvoices.Count() == 0) ? 1 : destInvoices.Count()));
                
            }
            //Filter by Station
            else if (filterCriteria.StationId > 0)
            {
                //// get the service centre
                var serviceCentresInStation = _uow.ServiceCentre.GetAllAsQueryable().Where(x => x.StationId == filterCriteria.StationId).Select(s => s.ServiceCentreId);
                serviceCenterInStationIds = serviceCentresInStation.ToList().ToArray();

                //For the Departure Station Data Only
                invoices = invoice.Where(s => serviceCenterInStationIds.Contains(s.DepartureServiceCentreId)).ToList();

                //For the Destination station  Data Only, so as to calaulate the average coming in
                destInvoices = invoice.Where(s => serviceCenterInStationIds.Contains(s.DestinationServiceCentreId)).ToList();

                //Average Price Of Shipments coming to that station Center
                avgDestShipmentCostPerSC = (destInvoices.Sum(p => p.GrandTotal) / ((destInvoices.Count() == 0) ? 1 : destInvoices.Count()));

            }
            else
            {
                invoices = invoice;
            }
           

            var revenue = invoices.Sum(x => x.GrandTotal);
            var shipmentDeliverd = invoices.Where(x => x.IsShipmentCollected == true).Count();
            var shipmentOrdered = invoices.Count();

            //customer Type
            string individual = FilterCustomerType.IndividualCustomer.ToString();
            string ecommerce = FilterCustomerType.Ecommerce.ToString();
            string corporate = FilterCustomerType.Corporate.ToString();

            //Shipments per Customer Type
            var individualShipments = invoices.Where(x => x.CompanyType == individual);
            var ecommerceShipments = invoices.Where(x => x.CompanyType == ecommerce);
            var corporateShipments = invoices.Where(x => x.CompanyType == corporate);

            //Revenue per customer type
            var individualRevenue = individualShipments.Sum(x => x.GrandTotal);
            var ecommerceRevenue = ecommerceShipments.Sum(x => x.GrandTotal);
            var corporateRevenue = corporateShipments.Sum(x => x.GrandTotal);

            //Count of Shipments Per Customer Type 
            var individualShipmentCount = individualShipments.Count();
            var ecommerceShipmentCount = ecommerceShipments.Count();
            var corporateShipmentCount = corporateShipments.Count();

            //Get Average Spent Per Customer Type
            var avgIndividualCost = individualRevenue / ((individualShipmentCount == 0) ? 1 : individualShipmentCount) ;
            var avgEcommerceCost = ecommerceRevenue / ((ecommerceShipmentCount == 0) ? 1 : ecommerceShipmentCount);
            var avgCorporateCost = corporateRevenue / ((corporateShipmentCount == 0) ? 1 : corporateShipmentCount);

            //All home delivery
            var homeDeliveries = invoices.Where(x => x.PickupOptions == PickupOptions.HOMEDELIVERY).Count();

            //All Terminal Pickup
            var terminalPickups = invoices.Where(x => x.PickupOptions == PickupOptions.SERVICECENTER).Count();

            //Ecommerce home delivery
            var ecommerceHome = invoices.Where(x => x.PickupOptions == PickupOptions.HOMEDELIVERY && x.CompanyType == ecommerce).Count();

            //Ecommerce Terminal Pickup
            var ecommerceTerminal = invoices.Where(x => x.PickupOptions == PickupOptions.SERVICECENTER && x.CompanyType == ecommerce).Count();

            //Service Centers Revenue By Sales No Service Center Filtering
            var salesPerServiceCenter = await SalesPerServiceCenter(invoice);

            if(filterCriteria.ServiceCentreId > 0)
            {
                //Average Price of shipments leaving that service center
                avgOriginShipmentCostPerSC = revenue / ((shipmentOrdered == 0) ? 1 : shipmentOrdered);
            }
            else if(filterCriteria.StationId > 0)
            {
                avgOriginShipmentCostPerSC = revenue / ((shipmentOrdered == 0) ? 1 : shipmentOrdered);

                //Shipments within Station
                var stationShipments = invoice.Where(s => serviceCenterInStationIds.Contains(s.DepartureServiceCentreId) && serviceCenterInStationIds.Contains(s.DestinationServiceCentreId));
                var sumStationShipments = stationShipments.Sum(x => x.GrandTotal);
                var countStationShipments = stationShipments.Count();
                avgStationShipments = sumStationShipments / ((countStationShipments == 0 ? 1 : countStationShipments));

                //Shipments Leaving Station
                var outStationShipments = invoice.Where(s => serviceCenterInStationIds.Contains(s.DepartureServiceCentreId) && !serviceCenterInStationIds.Contains(s.DestinationServiceCentreId));
                var sumOutStationShipments = outStationShipments.Sum(x => x.GrandTotal);
                var countOutStationShipments = outStationShipments.Count();
                avgOutStationShipments = sumOutStationShipments / ((countOutStationShipments == 0 ? 1 : countOutStationShipments));
            }

            //Shipments Created
            createdShipments = invoices.Where(x => x.ShipmentScanStatus == ShipmentScanStatus.CRT).Count();

            //Shipments departed service center
            departedShipments = invoices.Where(x => x.ShipmentScanStatus == ShipmentScanStatus.DSC || x.ShipmentScanStatus == ShipmentScanStatus.DTR || x.ShipmentScanStatus == ShipmentScanStatus.DPC).Count();

            //Total Weight
            var totalWeight = invoices.Sum(x => x.ApproximateItemsWeight);

            //Average Price of Total Shipments
            var avgTotalShipments = revenue / ((shipmentOrdered == 0) ? 1 : shipmentOrdered);

            //Most Shipped Items By Weight
            var weight = await MostShippedItemByWeight(invoices);

            //get the distinct count of customers
            var individualCustomerShipments = await CountCustomers(individualShipments.ToList());
            var ecommerceCustomerShipments = await CountCustomers(ecommerceShipments.ToList());
            var corporateCustomerShipments = await CountCustomers(corporateShipments.ToList());

            var individualCustomerShipmentsCount = individualCustomerShipments.Count();
            var ecommerceCustomerShipmentsCount = ecommerceCustomerShipments.Count();
            var corporateCustomerShipmentsCount = corporateCustomerShipments.Count();

            //Number of COD Shipments
            var isCOD = ecommerceShipments.Where(s =>  s.IsCashOnDelivery == true).Count();
            var isNotCOD = ecommerceShipments.Where(s => s.IsCashOnDelivery == false).Count();
            var codAmount = ecommerceShipments.Sum(x => x.CODAmount);

            result.Revenue = revenue;
            result.ShipmentDelivered = shipmentDeliverd;
            result.ShipmentOrdered = shipmentOrdered;
            result.CorporateRevenue = corporateRevenue;
            result.EcommerceRevenue = ecommerceRevenue;
            result.IndividualRevenue = individualRevenue;
            result.IndividualShipments = individualShipmentCount;
            result.EcommerceShipments = ecommerceShipmentCount;
            result.CorporateShipments = corporateShipmentCount;

            result.AverageShipmentIndividual = avgIndividualCost;
            result.AverageShipmentECommerce = avgEcommerceCost;
            result.AverageShipmentCorporate = avgCorporateCost;

            result.ECommerceHomeDeliveries = ecommerceHome;
            result.ECommerceTerminalPickups = ecommerceTerminal;

            result.HomeDeliveries = homeDeliveries;
            result.TerminalPickups = terminalPickups;

            result.Sales = salesPerServiceCenter;
            result.AvgOriginCostPerServiceCenter = avgOriginShipmentCostPerSC;
            result.AvgDestCostPerServiceCenter = avgDestShipmentCostPerSC;
            result.CreatedShipments = createdShipments;
            result.DepartedShipments = departedShipments;
            result.TotalWeight = totalWeight;
            result.TotalShipmentAvg = avgTotalShipments;
            result.WeightData = weight;
            result.AvgStationShipment = avgStationShipments;
            result.AvgOutStationShipment = avgOutStationShipments;

            result.IndCustomerCount = individualCustomerShipmentsCount;
            result.EcomCustomerCount = ecommerceCustomerShipmentsCount;
            result.CorpCustomerCount = corporateCustomerShipmentsCount;

            result.IsCOD = isCOD;
            result.IsNotCOD = isNotCOD;
            result.CODAmount = codAmount;

            return result;
        }

        private async Task<List<object>> SalesPerServiceCenter(List<InvoiceView> invoice)
        {
            var salesData = await _uow.Invoice.SalesPerServiceCenter(invoice);
            return salesData;
        }

        private async Task<List<object>> MostShippedItemByWeight(List<InvoiceView> invoice)
        {
            var weightData = await _uow.Invoice.MostShippedItemsByWeight(invoice);
            return weightData;
        }

        private async Task<List<object>> CountCustomers(List<InvoiceView> invoice)
        {
            var custData = await _uow.Invoice.CountOfCustomers(invoice);
            return custData;
        }

        private async Task<List<Report_MostShippedItemByWeight>> GetMostShippedItemByWeights()
        {
            var result = _uow.Invoice.GetMostShippedItemByWeight().OrderByDescending(x => x.Total).Take(5).ToList();
            return await Task.FromResult(result);
        }

        private async Task<List<Report_RevenuePerServiceCentre>> GetRevenuePerServiceCentres()
        {
            var result = _uow.Invoice.GetRevenuePerServiceCentre().OrderByDescending(x => x.Total).ToList();
            return await Task.FromResult(result);
        }

        private async Task<List<Report_TotalServiceCentreByState>> GetTotalServiceCentreByStates()
        {
            var result = _uow.Invoice.GetTotalServiceCentreByState().OrderByDescending(x => x.TotalService).ToList();
            return await Task.FromResult(result);
        }

        private async Task<Report_TotalOrdersDelivered> GetTotalOrdersDelivered()
        {
            var result = _uow.Invoice.GetTotalOrdersDelivered().FirstOrDefault();
            return await Task.FromResult(result);
        }

        private async Task<int> GetCorporateCount()
        {
            var result = _uow.Company.GetAllAsQueryable().Where(x => x.CompanyType == CompanyType.Corporate).Count();
            return await Task.FromResult(result);
        }

        private async Task<int> GetCorporateCount(ShipmentCollectionFilterCriteria filterCriteria)
        {
            var queryDate = filterCriteria.getStartDateAndEndDate();
            var startDate = queryDate.Item1;
            var endDate = queryDate.Item2;

            var result = _uow.Company.GetAllAsQueryable().Where(x => x.CompanyType == CompanyType.Corporate && x.UserActiveCountryId == filterCriteria.CountryId
            && (x.DateCreated >= startDate && x.DateCreated < endDate) ).Count();
            return await Task.FromResult(result);
        }

        private async Task<int> GetEcommerceCount()
        {
            var result = _uow.Company.GetAllAsQueryable().Where(x => x.CompanyType == CompanyType.Ecommerce).Count();
            return await Task.FromResult(result);
        }
        private async Task<int> GetEcommerceCount(ShipmentCollectionFilterCriteria filterCriteria)
        {
            var queryDate = filterCriteria.getStartDateAndEndDate();
            var startDate = queryDate.Item1;
            var endDate = queryDate.Item2;
                        
            var result = _uow.Company.GetAllAsQueryable().Where(x => x.CompanyType == CompanyType.Ecommerce && x.UserActiveCountryId == filterCriteria.CountryId
            && (x.DateCreated >= startDate && x.DateCreated < endDate)).Count();
            return await Task.FromResult(result);
        }

        private async Task<int> GetIndividaulCount()
        {
            var result = _uow.IndividualCustomer.GetAllAsQueryable().Select(x => x.PhoneNumber).Distinct().Count();
            return await Task.FromResult(result);
        }
        private async Task<int> GetIndividaulCount(ShipmentCollectionFilterCriteria filterCriteria)
        {
            var queryDate = filterCriteria.getStartDateAndEndDate();
            var startDate = queryDate.Item1;
            var endDate = queryDate.Item2;

            var result = _uow.IndividualCustomer.GetAllAsQueryable().Where(x => x.UserActiveCountryId == filterCriteria.CountryId && x.DateCreated >= startDate && x.DateCreated < endDate )
                .Select(x => x.PhoneNumber).Distinct().Count();
            return await Task.FromResult(result);
        }
    }
}