﻿using POST.Core;
using POST.CORE.IServices.Report;
using System.Collections.Generic;
using System.Threading.Tasks;
using POST.Core.DTO.Shipments;
using POST.CORE.DTO.Report;
using POST.Core.IServices.User;
using POST.Core.DTO.Account;
using System.Web;
using SpreadsheetLight;
using POST.Core.Enums;
using POST.Core.View;
using System.Linq;
using POST.Core.DTO.ShipmentScan;
using System;
using POST.Core.DTO.Dashboard;
using POST.Core.IServices.ServiceCentres;
using POST.Core.DTO.Report;
using AutoMapper;
using System.Net;
using POST.Infrastructure;
using OfficeOpenXml;
using System.Drawing;
using System.Web.Hosting;
using OfficeOpenXml.Style;
using System.IO;
using OfficeOpenXml.Drawing;
using POST.Core.Domain;
using Newtonsoft.Json.Linq;
using POST.Core.IServices.Utility;
using iTextSharp.text;
using iTextSharp.text.pdf;
using POST.Services.Implementation.Shipments;
using POST.Core.DTO.OnlinePayment;
using POST.Core.IServices.Wallet;
using Image = iTextSharp.text.Image;
using System.Configuration;
using Font = iTextSharp.text.Font;
using POST.CORE.DTO.Shipments;
using POST.Core.DTO.Wallet;

namespace POST.Services.Implementation.Report
{
    public class ShipmentReportService : IShipmentReportService
    {
        private readonly IUnitOfWork _uow;
        private readonly IUserService _userService;
        private IServiceCentreService _serviceCenterService;
        private INumberGeneratorMonitorService _numberGeneratorMonitorService;
        private readonly IPaystackPaymentService _paystackPaymentService;
        private readonly IWalletService _walletService;

        public ShipmentReportService(IUnitOfWork uow, IUserService userService, IServiceCentreService serviceCenterService, INumberGeneratorMonitorService numberGeneratorMonitorService, IPaystackPaymentService paystackPaymentService, IWalletService walletService)
        {
            _uow = uow;
            _userService = userService;
            _serviceCenterService = serviceCenterService;
            _numberGeneratorMonitorService = numberGeneratorMonitorService;
            _paystackPaymentService = paystackPaymentService;
            _walletService = walletService;
            MapperConfig.Initialize();
        }

        public async Task<List<ShipmentDTO>> GetShipments(ShipmentFilterCriteria filterCriteria)
        {
            var serviceCenters = await _userService.GetPriviledgeServiceCenters();
            var shipmentDto = await _uow.Shipment.GetShipments(filterCriteria, serviceCenters);

            foreach (var item in shipmentDto)
            {
                var user = await _uow.User.GetUserById(item.UserId);
                item.UserId = user.FirstName + " " + user.LastName;
            }
            return shipmentDto;
        }

        public async Task<List<ShipmentDTO>> GetTodayShipments()
        {
            ShipmentFilterCriteria filterCriteria = new ShipmentFilterCriteria
            {
                StartDate = System.DateTime.Today
            };

            var serviceCenters = await _userService.GetPriviledgeServiceCenters();
            return await _uow.Shipment.GetShipments(filterCriteria, serviceCenters);
        }

        public async Task<List<ShipmentDTO>> GetCustomerShipments(ShipmentFilterCriteria f_Criteria)
        {
            return await _uow.Shipment.GetCustomerShipments(f_Criteria);
        }

        public async Task<object> GetDailySalesByServiceCentreReport(DailySalesDTO dailySalesDTO)
        {
            var rootDir = HttpContext.Current.Server.MapPath("~");
            var filePath = rootDir + @"\ReportTemplate\dailysales.xlsx";
            SLDocument sl = new SLDocument(filePath);

            //heading
            var heading = $"AGILITY SALES REPORT BETWEEN {dailySalesDTO.StartDate.ToShortDateString()} AND {dailySalesDTO.EndDate.ToShortDateString()}";
            sl.SetCellValue("D7", heading);

            var serviceCentreRow = 10;
            foreach (var serviceCentre in dailySalesDTO.DailySalesByServiceCentres)
            {
                // copy from sheet Report2 to Report1
                sl.CopyCellFromWorksheet("Report2", 9, 1, 11, 26, serviceCentreRow - 1, 1);
                var rowHeight = sl.GetRowHeight(serviceCentreRow);

                sl.SetCellValue(serviceCentreRow, 3, serviceCentre.DepartureServiceCentreName + " ");

                //align the service centre name
                SLStyle styleSC = sl.GetCellStyle(serviceCentreRow, 3);
                styleSC.Alignment.Vertical = DocumentFormat.OpenXml.Spreadsheet.VerticalAlignmentValues.Top;
                sl.SetCellStyle(serviceCentreRow, 3, styleSC);

                var row = serviceCentreRow;

                sl.SetRowHeight(row, rowHeight);

                foreach (var invoice in serviceCentre.Invoices)
                {
                    sl.SetCellValue(row, 4, invoice.Waybill);
                    sl.SetCellValue(row, 6, invoice.DepartureServiceCentreName);
                    sl.SetCellValue(row, 7, invoice.DestinationServiceCentreName);
                    sl.SetCellValue(row, 8, invoice.Vat);
                    sl.SetCellValue(row, 9, invoice.DiscountValue);
                    sl.SetCellValue(row, 10, invoice.Insurance);
                    sl.SetCellValue(row, 11, invoice.ShipmentPackagePrice);
                    sl.SetCellValue(row, 12, invoice.Amount);
                    sl.SetCellValue(row, 14, ((PaymentStatus)invoice.PaymentStatus).ToString());
                    sl.SetCellValue(row, 15, invoice.PaymentMethod);
                    sl.SetCellValue(row, 17, invoice.CustomerDetails.CustomerName);
                    sl.SetCellValue(row, 18, invoice.CustomerDetails.PhoneNumber);
                    sl.SetCellValue(row, 19, invoice.CustomerDetails.CustomerCode.Substring(0, 3));
                    sl.SetCellValue(row, 20, invoice.ReceiverName);
                    sl.SetCellValue(row, 21, invoice.ReceiverPhoneNumber);
                    sl.SetCellValue(row, 22, invoice.UserName);
                    sl.SetCellValue(row, 23, invoice.DateCreated);

                    //insert row and add styles
                    sl.InsertRow(row + 1, 1);
                    sl.SetRowHeight(row + 1, sl.GetRowHeight(row));
                    for (int col = 4; col <= 23; col++)
                    {
                        var style = sl.GetCellStyle(row, col);
                        sl.SetCellStyle(row + 1, col, style);
                    }

                    row++;
                }

                //add subtotal values
                sl.SetCellValue(row + 1, 12, serviceCentre.TotalSales);

                //merge cells
                sl.MergeWorksheetCells(serviceCentreRow, 3, row + 1, 3);

                //update serviceCentreRow
                serviceCentreRow = row + 5;
            }

            //---Total
            // copy from sheet Report2 to Report1
            sl.CopyCellFromWorksheet("Report2", 13, 1, 13, 26, serviceCentreRow, 1);

            //add Total values
            sl.SetCellValue(serviceCentreRow, 12, dailySalesDTO.TotalSales);

            //remove the Report2 sheet
            sl.DeleteWorksheet("Report2");

            //save to file system
            var filename = $"dailysales_{System.Guid.NewGuid()}.xlsx";
            var fileToSave = rootDir + @"\ReportTemplate\download\" + filename;
            sl.SaveAs(fileToSave);

            return await Task.FromResult(filename);
        }

        public async Task<List<ShipmentTrackingView>> GetShipmentTrackingFromView(ScanTrackFilterCriteria f_Criteria)
        {
            var queryable = _uow.ShipmentTracking.GetShipmentTrackingsFromViewAsync(f_Criteria);
            var result = await Task.FromResult(queryable.ToList());
            return result;
        }

        /// <summary>
        /// This method gets the Shipment Tracking Report from based on filter parameters
        /// </summary>
        /// <param name="f_Criteria">Criteria used for filtering</param>
        /// <returns>List of ScanStatusReport objects</returns>
        public async Task<List<ScanStatusReportDTO>> GetShipmentTrackingFromViewReport(ScanTrackFilterCriteria f_Criteria)
        {
            //var queryable = _uow.ShipmentTracking.GetShipmentTrackingsFromViewAsync(f_Criteria);
            var queryable = _uow.ShipmentTracking.GetShipmentTrackingsAsync(f_Criteria);
            var queryableList = queryable.Select(s =>
            new ShipmentTrackingDTO
            {
                Status = s.Status,
                Location = s.Location,
                ShipmentTrackingId = s.ShipmentTrackingId,
                ServiceCentreId = s.ServiceCentreId
            }).ToList();

            //1. Group by Service Centre
            var scanStatusReportList = new List<ScanStatusReportDTO>();
            //var allServiceCentreNames = _uow.ServiceCentre.GetAllAsQueryable().Select(s => s.Name).ToList();
            //var allServiceCentreNames = _uow.ServiceCentre.GetAllAsQueryable().Select(s => new { s.ServiceCentreId, s.Name }).ToList();

            //Get only Nigeria Service centre 
            var countryIds = await _userService.GetPriviledgeCountryIds();
            var allServiceCentreNames = await _uow.ServiceCentre.GetLocalServiceCentres(countryIds);

            var allScanStatus = _uow.ScanStatus.GetAllAsQueryable().ToList();
            foreach (var scName in allServiceCentreNames)
            {
                var scanStatusReportDTO = new ScanStatusReportDTO
                {
                    StartDate = f_Criteria.StartDate,
                    EndDate = f_Criteria.EndDate,
                    Location = scName.Name,
                    ServiceCentreId = scName.ServiceCentreId
                };

                //1.1 Group by Scan Status
                PopulateScanStatusReport(scanStatusReportDTO, queryableList, scName.ServiceCentreId);

                //1.2 Add to report list
                scanStatusReportList.Add(scanStatusReportDTO);
            }

            var result = await Task.FromResult(scanStatusReportList);
            return result;
        }

        /// <summary>
        /// This helper method populates the Scan Status Report
        /// </summary>
        /// <param name="scanStatusReportDTO"></param>
        /// <param name="queryableList"></param>
        /// <param name="serviceCentreName"></param>
        private void PopulateScanStatusReport(ScanStatusReportDTO scanStatusReportDTO,
            List<ShipmentTrackingDTO> queryableList, int serviceCentreId)
        {
            var shipmentScanStatusValues = Enum.GetNames(typeof(ShipmentScanStatus));

            foreach (var shipmentScanStatusName in shipmentScanStatusValues)
            {
                var count_status = queryableList.Where(s => s.ServiceCentreId == serviceCentreId &&
                s.Status == shipmentScanStatusName).Select(x => x.ShipmentTrackingId).Count();
                scanStatusReportDTO.StatusCountMap.Add(shipmentScanStatusName, count_status);
            }
        }


        public async Task<DashboardDTO> GetShipmentProgressSummary(ShipmentProgressSummaryFilterCriteria baseFilterCriteria)
        {
            var dashboardDTO = new DashboardDTO()
            {
                TotalCustomers = 0,
                TotalShipmentAwaitingCollection = 0,
                TotalShipmentDelivered = 0,
                TotalShipmentExpected = 0,
                TotalShipmentOrdered = 0
            };

            var serviceCenterIds = await _userService.GetPriviledgeServiceCenters();

            try
            {
                if (baseFilterCriteria.ServiceCentreId > 0)
                {
                    serviceCenterIds = new int[] { baseFilterCriteria.ServiceCentreId };
                }

                if (baseFilterCriteria.StationId > 0)
                {
                    serviceCenterIds = _uow.ServiceCentre.GetAllAsQueryable()
                        .Where(x => x.StationId == baseFilterCriteria.StationId).Select(x => x.ServiceCentreId).ToArray();
                }

                if (baseFilterCriteria.StateId > 0)
                {
                    var stations = _uow.Station.GetAllAsQueryable().Where(x => x.StateId == baseFilterCriteria.StateId).Select(x => x.StationId);
                    serviceCenterIds = _uow.ServiceCentre.GetAllAsQueryable()
                        .Where(w => stations.Contains(w.StationId)).Select(s => s.ServiceCentreId).ToArray();
                }

                dashboardDTO = await GetShipmentProgressSummary(serviceCenterIds, baseFilterCriteria);
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return dashboardDTO;
        }


        private async Task<DashboardDTO> GetShipmentProgressSummary(int[] serviceCenterId, ShipmentProgressSummaryFilterCriteria baseFilterCriteria)
        {
            var dashboardDTO = new DashboardDTO();

            //get startDate and endDate
            var queryDate = baseFilterCriteria.getStartDateAndEndDate();
            var startDate = queryDate.Item1;
            var endDate = queryDate.Item2;

            //1. Get Total Shipment Expected filter by date using Date Created
            //1a. Get shipments coming to the service centre 
            var allShipments = _uow.Invoice.GetAllFromInvoiceAndShipments();

            if (baseFilterCriteria.IsCOD)
            {
                allShipments = allShipments.Where(x => x.CashOnDeliveryAmount > 0);
            }

            var allShipmentsResult = allShipments.Where(s => s.IsShipmentCollected == false && serviceCenterId.Contains(s.DestinationServiceCentreId)
                && s.DateCreated >= startDate && s.DateCreated < endDate && s.ShipmentScanStatus != ShipmentScanStatus.ARF).Select(x => x.Waybill).Distinct();

            //1b. For waybill to be collected it must have satisfy the follwoing Shipment Scan Status
            //Collected by customer (OKC & OKT), Return (SSR), Reroute (SRR) : All status satisfy IsShipmentCollected above
            //shipments that have arrived destination service centre or cancelled should not be displayed in expected shipments
            //var shipmetCollection = _uow.ShipmentCollection.GetAllAsQueryable()
            //    .Where(x => serviceCenterId.Contains(x.DestinationServiceCentreId) && !(x.ShipmentScanStatus == ShipmentScanStatus.OKC && x.ShipmentScanStatus == ShipmentScanStatus.OKT
            //    && x.ShipmentScanStatus == ShipmentScanStatus.SSR && x.ShipmentScanStatus == ShipmentScanStatus.SRR
            //    && x.ShipmentScanStatus == ShipmentScanStatus.ARF && x.ShipmentScanStatus == ShipmentScanStatus.SSC)).Select(w => w.Waybill);

            //var shipmetCollection = _uow.ShipmentCollection.GetAllAsQueryable()
            //    .Where(x => serviceCenterId.Contains(x.DestinationServiceCentreId)).Select(w => w.Waybill).Distinct();

            //1c. remove all the waybills that at the collection center from the income shipments
            //allShipmentsResult = allShipmentsResult.Where(s => !shipmetCollection.Contains(s));
            dashboardDTO.TotalShipmentExpected = allShipmentsResult.Count();


            //2. Get Total Shipment Awaiting Collection
            //var shipmentsInWaybills = _uow.Invoice.GetAllFromInvoiceAndShipments()
            //    .Where(s => s.IsShipmentCollected == false && serviceCenterId.Contains(s.DestinationServiceCentreId));

            //if (baseFilterCriteria.IsCOD)
            //{
            //    shipmentsInWaybills = shipmentsInWaybills.Where(x => x.CashOnDeliveryAmount > 0);
            //}

            //var shipmentsInWaybillsResult = shipmentsInWaybills.Select(x => x.Waybill).Distinct();

            //var shipmentInCollection = _uow.ShipmentCollection.GetAllAsQueryable()
            //    .Where(x => x.ShipmentScanStatus == ShipmentScanStatus.ARF
            //    && x.DateCreated >= startDate && x.DateCreated < endDate).Select(x => x.Waybill);

            //shipmentsInWaybillsResult = shipmentsInWaybillsResult.Where(s => shipmentInCollection.Contains(s));
            //dashboardDTO.TotalShipmentAwaitingCollection = shipmentsInWaybillsResult.Count();

            var shipmentInCollection = _uow.ShipmentCollection.GetAllAsQueryable()
                .Where(x => x.ShipmentScanStatus == ShipmentScanStatus.ARF && serviceCenterId.Contains(x.DestinationServiceCentreId)
                && x.DateCreated >= startDate && x.DateCreated < endDate);

            if (baseFilterCriteria.IsCOD)
            {
                shipmentInCollection = shipmentInCollection.Where(x => x.IsCashOnDelivery == true);
            }

            var shipmentsInWaybillsResult = shipmentInCollection.Select(x => x.Waybill).Distinct();

            dashboardDTO.TotalShipmentAwaitingCollection = shipmentsInWaybillsResult.Count();

            //3. Get Total Shipment Created that has not depart service centre
            var allShipmentsQueryable = _uow.Invoice.GetAllFromInvoiceAndShipments()
                .Where(s => s.PaymentStatus == PaymentStatus.Paid && s.DateCreated >= startDate && s.DateCreated < endDate);

            if (baseFilterCriteria.IsCOD)
            {
                allShipmentsQueryable = allShipmentsQueryable.Where(s => s.CashOnDeliveryAmount > 0);
            }

            allShipmentsQueryable = allShipmentsQueryable.Where(s => serviceCenterId.Contains(s.DepartureServiceCentreId));

            //var shipmentTrackingHistory = _uow.ShipmentTracking.GetAllAsQueryable()
            //    .Where(x => serviceCenterId.Contains(x.ServiceCentreId) && (x.Status == ShipmentScanStatus.DSC.ToString()
            //    || x.Status == ShipmentScanStatus.TRO.ToString() || x.Status == ShipmentScanStatus.DTR.ToString())).Select(x => x.Waybill).Distinct();

            //allShipmentsQueryable = allShipmentsQueryable.Where(s => !shipmentTrackingHistory.Contains(s.Waybill));

            //allShipmentsQueryable = allShipmentsQueryable.Where(x => !(x.ShipmentScanStatus == ShipmentScanStatus.DSC 
            //|| x.ShipmentScanStatus == ShipmentScanStatus.TRO || x.ShipmentScanStatus == ShipmentScanStatus.DTR));

            allShipmentsQueryable = allShipmentsQueryable.Where(x => x.ShipmentScanStatus == ShipmentScanStatus.CRT);

            dashboardDTO.TotalShipmentOrdered = allShipmentsQueryable.Count();

            //4. Get Total Shipment Delivered   
            //4a. Get collected shipment by date filtering : use current date if not date selected
            if (baseFilterCriteria.StartDate == null & baseFilterCriteria.EndDate == null)
            {
                startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day).AddDays(1);
            }

            //var shipmentCollection = _uow.ShipmentCollection.GetAllAsQueryable()
            //    .Where(x => (x.ShipmentScanStatus == ShipmentScanStatus.OKT)
            //    && x.DateModified >= startDate && x.DateModified < endDate).Select(x => x.Waybill).Distinct();

            ////4b. Get Shipments that its destination is the service centre
            //var shipmentsWaybills = _uow.Invoice.GetAllFromInvoiceAndShipments();

            //if (baseFilterCriteria.IsCOD)
            //{
            //    shipmentsWaybills = shipmentsWaybills.Where(x => x.CashOnDeliveryAmount > 0);
            //}

            //var shipmentsWaybillsResult = shipmentsWaybills.Where(s => serviceCenterId.Contains(s.DestinationServiceCentreId) && s.IsShipmentCollected == true).Select(x => x.Waybill).Distinct();

            ////4c. Extras the current login staff service centre shipment from the shipment collection
            //shipmentsWaybillsResult = shipmentsWaybillsResult.Where(x => shipmentCollection.Contains(x));
            //dashboardDTO.TotalShipmentDelivered = shipmentsWaybillsResult.Count();

            var shipmentCollection = _uow.ShipmentCollection.GetAllAsQueryable()
                .Where(x => (x.ShipmentScanStatus == ShipmentScanStatus.OKT || x.ShipmentScanStatus == ShipmentScanStatus.OKC)
                && serviceCenterId.Contains(x.DestinationServiceCentreId) && x.DateCreated >= startDate && x.DateCreated < endDate);

            if (baseFilterCriteria.IsCOD)
            {
                shipmentCollection = shipmentCollection.Where(x => x.IsCashOnDelivery == true);
            }

            var shipmentsWaybillsResult = shipmentCollection.Select(x => x.Waybill).Distinct();
            dashboardDTO.TotalShipmentDelivered = shipmentsWaybillsResult.Count();

            dashboardDTO.MostRecentOrder = new List<ShipmentOrderDTO> { };
            dashboardDTO.GraphData = new List<GraphDataDTO> { };

            return await Task.FromResult(dashboardDTO);
        }

        //Shipment Breakdown
        public async Task<List<InvoiceViewDTO>> GetShipmentProgressSummaryBreakDown(ShipmentProgressSummaryFilterCriteria baseFilterCriteria)
        {
            var dashboardDTO = new List<InvoiceViewDTO>() { };

            var serviceCenterIds = await _userService.GetPriviledgeServiceCenters();

            try
            {
                if (baseFilterCriteria.ServiceCentreId > 0)
                {
                    serviceCenterIds = new int[] { baseFilterCriteria.ServiceCentreId };
                }

                if (baseFilterCriteria.StationId > 0)
                {
                    serviceCenterIds = _uow.ServiceCentre.GetAllAsQueryable()
                        .Where(x => x.StationId == baseFilterCriteria.StationId).Select(x => x.ServiceCentreId).ToArray();
                }

                if (baseFilterCriteria.StateId > 0)
                {
                    var stations = _uow.Station.GetAllAsQueryable().Where(x => x.StateId == baseFilterCriteria.StateId).Select(x => x.StationId);
                    serviceCenterIds = _uow.ServiceCentre.GetAllAsQueryable()
                        .Where(w => stations.Contains(w.StationId)).Select(s => s.ServiceCentreId).ToArray();
                }

                switch (baseFilterCriteria.ShipmentProgressSummaryType)
                {
                    case ShipmentProgressSummaryType.ExpectedShipment:
                        return dashboardDTO = await GetShipmentProgressSummaryForExpectedShipment(serviceCenterIds, baseFilterCriteria);
                    case ShipmentProgressSummaryType.AwaitingCollectionShipment:
                        return dashboardDTO = await GetShipmentProgressSummaryForAwaitingCollectionShipment(serviceCenterIds, baseFilterCriteria);
                    case ShipmentProgressSummaryType.DeliveredShipment:
                        return dashboardDTO = await GetShipmentProgressSummaryForDeliveredShipment(serviceCenterIds, baseFilterCriteria);
                    case ShipmentProgressSummaryType.OrderedShipment:
                        return dashboardDTO = await GetShipmentProgressSummaryForOrderedShipment(serviceCenterIds, baseFilterCriteria);
                    default:
                        return dashboardDTO;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        //break the result into ShipmentProgressSummaryType
        private async Task<List<InvoiceViewDTO>> GetShipmentProgressSummaryForExpectedShipment(int[] serviceCenterId, ShipmentProgressSummaryFilterCriteria baseFilterCriteria)
        {
            var dashboardDTO = new List<InvoiceViewDTO>() { };

            //get startDate and endDate
            var queryDate = baseFilterCriteria.getStartDateAndEndDate();
            var startDate = queryDate.Item1;
            var endDate = queryDate.Item2;

            //3. Get Total Shipment Expected filter by date using Date Created
            //3a. Get shipments coming to the service centre 
            var allShipments = _uow.Invoice.GetAllFromInvoiceAndShipments()
                .Where(s => s.IsShipmentCollected == false && serviceCenterId.Contains(s.DestinationServiceCentreId)
                && s.DateCreated >= startDate && s.DateCreated < endDate && s.ShipmentScanStatus != ShipmentScanStatus.ARF);

            if (baseFilterCriteria.IsCOD)
            {
                allShipments = allShipments.Where(x => x.CashOnDeliveryAmount > 0);
            }

            //3b. For waybill to be collected it must have satisfy the follwoing Shipment Scan Status
            //Collected by customer (OKC & OKT), Return (SSR), Reroute (SRR) : All status satisfy IsShipmentCollected above
            //shipments that have arrived destination service centre or cancelled should not be displayed in expected shipments
            //var shipmentCollection = _uow.ShipmentCollection.GetAllAsQueryable()
            //    .Where(x => serviceCenterId.Contains(x.DestinationServiceCentreId) && !(x.ShipmentScanStatus == ShipmentScanStatus.OKC && x.ShipmentScanStatus == ShipmentScanStatus.OKT
            //    && x.ShipmentScanStatus == ShipmentScanStatus.SSR && x.ShipmentScanStatus == ShipmentScanStatus.SRR
            //    && x.ShipmentScanStatus == ShipmentScanStatus.ARF && x.ShipmentScanStatus == ShipmentScanStatus.SSC)).Select(w => w.Waybill);

            //var shipmentCollection = _uow.ShipmentCollection.GetAllAsQueryable()
            //            .Where(x => serviceCenterId.Contains(x.DestinationServiceCentreId)).Select(w => w.Waybill).Distinct();

            //3c. remove all the waybills that at the collection center from the income shipments
            //allShipments = allShipments.Where(s => !shipmentCollection.Any(x => x == s.Waybill));
            dashboardDTO = Mapper.Map<List<InvoiceViewDTO>>(allShipments.OrderByDescending(x => x.DateCreated).ToList());

            //Use to populate service centre 
            var allServiceCentres = await _serviceCenterService.GetServiceCentres();

            //populate the service centres
            foreach (var invoiceViewDTO in dashboardDTO)
            {
                invoiceViewDTO.DepartureServiceCentre = allServiceCentres.SingleOrDefault(x => x.ServiceCentreId == invoiceViewDTO.DepartureServiceCentreId);
                invoiceViewDTO.DestinationServiceCentre = allServiceCentres.SingleOrDefault(x => x.ServiceCentreId == invoiceViewDTO.DestinationServiceCentreId);
            }

            return dashboardDTO;
        }

        private async Task<List<InvoiceViewDTO>> GetShipmentProgressSummaryForOrderedShipment(int[] serviceCenterId, ShipmentProgressSummaryFilterCriteria baseFilterCriteria)
        {
            var dashboardDTO = new List<InvoiceViewDTO>() { };

            //get startDate and endDate
            var queryDate = baseFilterCriteria.getStartDateAndEndDate();
            var startDate = queryDate.Item1;
            var endDate = queryDate.Item2;

            var allShipmentsQueryable = _uow.Invoice.GetAllFromInvoiceAndShipments()
                .Where(s => s.PaymentStatus == PaymentStatus.Paid && s.DateCreated >= startDate && s.DateCreated < endDate);
            allShipmentsQueryable = allShipmentsQueryable.Where(s => serviceCenterId.Contains(s.DepartureServiceCentreId));

            if (baseFilterCriteria.IsCOD)
            {
                allShipmentsQueryable = allShipmentsQueryable.Where(x => x.CashOnDeliveryAmount > 0);
            }

            //allShipmentsQueryable = allShipmentsQueryable.Where(x => !(x.ShipmentScanStatus == ShipmentScanStatus.DSC
            //|| x.ShipmentScanStatus == ShipmentScanStatus.TRO || x.ShipmentScanStatus == ShipmentScanStatus.DTR));

            allShipmentsQueryable = allShipmentsQueryable.Where(x => x.ShipmentScanStatus == ShipmentScanStatus.CRT);

            //var shipmentTrackingHistory = _uow.ShipmentTracking.GetAllAsQueryable()
            //    .Where(x => x.Status == ShipmentScanStatus.DSC.ToString() || x.Status == ShipmentScanStatus.DPC.ToString()).Select(x => x.Waybill).Distinct();

            //allShipmentsQueryable = allShipmentsQueryable.Where(s => !shipmentTrackingHistory.Contains(s.Waybill));

            dashboardDTO = Mapper.Map<List<InvoiceViewDTO>>(allShipmentsQueryable.OrderByDescending(x => x.DateCreated).ToList());

            //Use to populate service centre 
            var allServiceCentres = await _serviceCenterService.GetServiceCentres();

            //populate the service centres
            foreach (var invoiceViewDTO in dashboardDTO)
            {
                invoiceViewDTO.DepartureServiceCentre = allServiceCentres.SingleOrDefault(x => x.ServiceCentreId == invoiceViewDTO.DepartureServiceCentreId);
                invoiceViewDTO.DestinationServiceCentre = allServiceCentres.SingleOrDefault(x => x.ServiceCentreId == invoiceViewDTO.DestinationServiceCentreId);
            }

            return dashboardDTO;
        }

        private async Task<List<InvoiceViewDTO>> GetShipmentProgressSummaryForDeliveredShipment(int[] serviceCenterId, ShipmentProgressSummaryFilterCriteria baseFilterCriteria)
        {
            var dashboardDTO = new List<InvoiceViewDTO>() { };

            //get startDate and endDate
            var queryDate = baseFilterCriteria.getStartDateAndEndDate();
            var startDate = queryDate.Item1;
            var endDate = queryDate.Item2;

            //2. Get Total Shipment Delivered   
            if (baseFilterCriteria.StartDate == null & baseFilterCriteria.EndDate == null)
            {
                startDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
                endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day).AddDays(1);
            }

            //var shipmentCollection = _uow.ShipmentCollection.GetAllAsQueryable()
            //    .Where(x => (x.ShipmentScanStatus == ShipmentScanStatus.OKT || x.ShipmentScanStatus == ShipmentScanStatus.OKC)
            //    && serviceCenterId.Contains(x.DestinationServiceCentreId) && x.DateCreated >= startDate && x.DateCreated < endDate);

            //if (baseFilterCriteria.IsCOD)
            //{
            //    shipmentCollection = shipmentCollection.Where(x => x.IsCashOnDelivery == true);
            //}

            //var shipmentsWaybillsResult = shipmentCollection.Select(x => x.Waybill).Distinct();
            //dashboardDTO.TotalShipmentDelivered = shipmentsWaybillsResult.Count();

            //2a. Get collected shipment by date filtering
            //var shipmentCollection = _uow.ShipmentCollection.GetAllAsQueryable()
            //    .Where(x => (x.ShipmentScanStatus == ShipmentScanStatus.OKT)
            //    && x.DateModified >= startDate && x.DateModified < endDate).Select(x => x.Waybill).Distinct();

            var shipmentCollection = _uow.ShipmentCollection.GetAllAsQueryable()
                .Where(x => (x.ShipmentScanStatus == ShipmentScanStatus.OKT || x.ShipmentScanStatus == ShipmentScanStatus.OKC)
                && serviceCenterId.Contains(x.DestinationServiceCentreId) && x.DateCreated >= startDate && x.DateCreated < endDate);

            if (baseFilterCriteria.IsCOD)
            {
                shipmentCollection = shipmentCollection.Where(x => x.IsCashOnDelivery == true);
            }

            var shipmentsWaybillsResult = shipmentCollection.Select(x => x.Waybill).Distinct();

            //2b. Get Shipments that its destination is the service centre
            //var shipmentsWaybills = _uow.Invoice.GetAllFromInvoiceAndShipments()
            //    .Where(s => serviceCenterId.Contains(s.DestinationServiceCentreId) && s.IsShipmentCollected == true);

            var shipmentsWaybills = _uow.Invoice.GetAllFromInvoiceAndShipments();

            //if (baseFilterCriteria.IsCOD)
            //{
            //    shipmentsWaybills = shipmentsWaybills.Where(x => x.CashOnDeliveryAmount > 0);
            //}

            //2c. Extras the current login staff service centre shipment from the shipment collection
            //shipmentsWaybills = shipmentsWaybills.Where(x => shipmentCollection.Any(w => w == x.Waybill));
            shipmentsWaybills = shipmentsWaybills.Where(x => shipmentsWaybillsResult.Any(w => w == x.Waybill));
            dashboardDTO = Mapper.Map<List<InvoiceViewDTO>>(shipmentsWaybills.OrderByDescending(x => x.DateCreated).ToList());

            //Use to populate service centre 
            var allServiceCentres = await _serviceCenterService.GetServiceCentres();

            //populate the service centres
            foreach (var invoiceViewDTO in dashboardDTO)
            {
                invoiceViewDTO.DepartureServiceCentre = allServiceCentres.SingleOrDefault(x => x.ServiceCentreId == invoiceViewDTO.DepartureServiceCentreId);
                invoiceViewDTO.DestinationServiceCentre = allServiceCentres.SingleOrDefault(x => x.ServiceCentreId == invoiceViewDTO.DestinationServiceCentreId);
            }

            return dashboardDTO;
        }

        private async Task<List<InvoiceViewDTO>> GetShipmentProgressSummaryForAwaitingCollectionShipment(int[] serviceCenterId, ShipmentProgressSummaryFilterCriteria baseFilterCriteria)
        {
            var dashboardDTO = new List<InvoiceViewDTO>() { };

            //get startDate and endDate
            var queryDate = baseFilterCriteria.getStartDateAndEndDate();
            var startDate = queryDate.Item1;
            var endDate = queryDate.Item2;

            //4. Get Total Shipment Awaiting Collection
            //var shipmentsInWaybills = _uow.Invoice.GetAllFromInvoiceAndShipments()
            //    .Where(s => s.IsShipmentCollected == false && serviceCenterId.Contains(s.DestinationServiceCentreId));

            //if (baseFilterCriteria.IsCOD)
            //{
            //    shipmentsInWaybills = shipmentsInWaybills.Where(x => x.CashOnDeliveryAmount > 0);
            //}

            //var shipmentInCollection = _uow.ShipmentCollection.GetAllAsQueryable()
            //    .Where(x => x.ShipmentScanStatus == ShipmentScanStatus.ARF
            //    && x.DateCreated >= startDate && x.DateCreated < endDate).Select(x => x.Waybill);

            //shipmentsInWaybills = shipmentsInWaybills.Where(s => shipmentInCollection.Contains(s.Waybill));

            //dashboardDTO = Mapper.Map<List<InvoiceViewDTO>>(shipmentsInWaybills.OrderByDescending(x => x.DateCreated).ToList());

            var shipmentInCollection = _uow.ShipmentCollection.GetAllAsQueryable()
                .Where(x => x.ShipmentScanStatus == ShipmentScanStatus.ARF && serviceCenterId.Contains(x.DestinationServiceCentreId)
                && x.DateCreated >= startDate && x.DateCreated < endDate);

            if (baseFilterCriteria.IsCOD)
            {
                shipmentInCollection = shipmentInCollection.Where(x => x.IsCashOnDelivery == true);
            }

            var shipmentsInWaybillsResult = shipmentInCollection.Select(x => x.Waybill).Distinct();

            var shipmentsInWaybills = _uow.Invoice.GetAllFromInvoiceAndShipments();
            shipmentsInWaybills = shipmentsInWaybills.Where(s => shipmentsInWaybillsResult.Contains(s.Waybill));

            dashboardDTO = Mapper.Map<List<InvoiceViewDTO>>(shipmentsInWaybills.OrderByDescending(x => x.DateCreated).ToList());

            //Use to populate service centre 
            var allServiceCentres = await _serviceCenterService.GetServiceCentres();

            //populate the service centres
            foreach (var invoiceViewDTO in dashboardDTO)
            {
                invoiceViewDTO.DepartureServiceCentre = allServiceCentres.SingleOrDefault(x => x.ServiceCentreId == invoiceViewDTO.DepartureServiceCentreId);
                invoiceViewDTO.DestinationServiceCentre = allServiceCentres.SingleOrDefault(x => x.ServiceCentreId == invoiceViewDTO.DestinationServiceCentreId);
            }

            return dashboardDTO;
        }

        public async Task<List<PreShipmentMobileReportDTO>> GetPreShipmentMobile(MobileShipmentFilterCriteria accountFilterCriteria)
        {
            if (accountFilterCriteria == null)
            {
                accountFilterCriteria = new MobileShipmentFilterCriteria
                {
                    StartDate = null,
                    EndDate = null
                };
            }

            return await _uow.PreShipmentMobile.GetPreShipments(accountFilterCriteria);
        }


        public async Task<CustomerInvoiceDTO> GetCoporateTransactionsByCode(DateFilterForDropOff filter)
        {
            // filter by cancelled shipments
            try
            {
                if (filter == null)
                {
                    throw new GenericException("Invalid payload", $"{(int)HttpStatusCode.BadRequest}");
                }
                if (String.IsNullOrEmpty(filter.CustomerCode))
                {
                    throw new GenericException("Customer code not provided", $"{(int)HttpStatusCode.BadRequest}");
                }
                filter.UserId = await _userService.GetCurrentUserId();
                var result = await _uow.Shipment.GetCoporateTransactionsByCode(filter);
                if (result.InvoiceViewDTOs.Any())
                {
                    //change dest and dept to station names
                    var IDs = new List<int>();
                    var destStationsIDs = result.InvoiceViewDTOs.Select(x => x.DestinationStationId);
                    var deptStationsIDs = result.InvoiceViewDTOs.Select(x => x.DepartureStationId);
                    IDs.AddRange(destStationsIDs);
                    IDs.AddRange(deptStationsIDs);
                    var stations = _uow.Station.GetAllAsQueryable().Where(x => IDs.Contains(x.StationId)).ToList();
                    foreach (var item in result.InvoiceViewDTOs)
                    {
                        item.DestinationServiceCentreName = stations.FirstOrDefault(x => x.StationId == item.DestinationStationId).StationName;
                        item.DepartureServiceCentreName = stations.FirstOrDefault(x => x.StationId == item.DepartureStationId).StationName;
                    }
                    result.InvoiceRefNo = await _numberGeneratorMonitorService.GenerateInvoiceRefNoWithDate(NumberGeneratorType.Invoice, filter.CustomerCode, filter.StartDate.Value, filter.EndDate.Value);
                }
                return result;
            }
            catch (Exception ex)
            {

                throw;

            }

        }

        public async Task<bool> GenerateCustomerInvoice(CustomerInvoiceDTO customerInvoiceDTO)
        {
            try
            {
                if (customerInvoiceDTO == null)
                {
                    throw new GenericException("Invalid payload", $"{(int)HttpStatusCode.BadRequest}");
                }
                if (!customerInvoiceDTO.InvoiceViewDTOs.Any())
                {
                    throw new GenericException("Invalid payload, No invoice detail", $"{(int)HttpStatusCode.BadRequest}");
                }
                var customerInvoice = JObject.FromObject(customerInvoiceDTO).ToObject<CustomerInvoice>();
                var waybills = customerInvoiceDTO.InvoiceViewDTOs.Select(x => x.Waybill).ToList();
                customerInvoice.UserID = await _userService.GetCurrentUserId();
                customerInvoice.Waybills = string.Join(",", waybills);
                _uow.CustomerInvoice.Add(customerInvoice);
                await _uow.CompleteAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        public async Task<List<CustomerInvoiceDTO>> GetMonthlyCoporateTransactions()
        {
            try
            {
                var shipments = await _uow.Shipment.GetMonthlyCoporateTransactions();
                return shipments;
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        public async Task<string> GeneratePDF(CustomerInvoiceDTO customerInvoice)
        {
            using (System.IO.MemoryStream memoryStream = new System.IO.MemoryStream())
            {
                var invoices = customerInvoice.InvoiceViewDTOs;
                Document document = new Document(PageSize.A4, 10, 10, 10, 10);
                PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
                document.Open();
                string[] headers = { "Waybill", "Dept", "Dest", "Weight(kg)", "Amount(#)", "DateCreated" };
                float[] widths = new float[] { 45, 45, 78, 30, 45, 78, 78, 151, 150 };


                PdfPTable table = new PdfPTable(headers.Length);
                var imageURL = ConfigurationManager.AppSettings["InvoiceImg"];
                Image pngImg = Image.GetInstance(imageURL);
                pngImg.ScaleToFit(570f, 420f);
                table.SpacingBefore = 20;
                table.WidthPercentage = 99;

                PdfPCell cell = new PdfPCell();
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.VerticalAlignment = Element.ALIGN_CENTER;

                cell = new PdfPCell(new Phrase($"Bill To: {customerInvoice.CustomerName}{System.Environment.NewLine}{System.Environment.NewLine}Email: {customerInvoice.Email}{System.Environment.NewLine}{System.Environment.NewLine}Invoice Ref No: {customerInvoice.InvoiceRefNo}"));
                cell.Colspan = 6;
                table.AddCell(cell);


                cell = new PdfPCell(new Phrase());
                cell = new PdfPCell(new Phrase($"Customer Code: {customerInvoice.CustomerCode.PadRight(20)} Date Created: {invoices.FirstOrDefault().DateCreated.ToString()}{System.Environment.NewLine}"));
                cell.Colspan = 6;
                table.AddCell(cell);

                cell = new PdfPCell(new Phrase("Invoice Detail"));
                cell.Colspan = 6;
                // cell.PaddingLeft = 50;
                cell.HorizontalAlignment = Element.ALIGN_CENTER;
                cell.VerticalAlignment = Element.ALIGN_CENTER;
                table.AddCell(cell);


                for (int i = 0; i < 1; i++)
                {
                    cell = new PdfPCell(new Phrase(headers[0]));
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.VerticalAlignment = Element.ALIGN_CENTER;
                    table.AddCell(cell);
                    cell = new PdfPCell(new Phrase(headers[1]));
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.VerticalAlignment = Element.ALIGN_CENTER;
                    table.AddCell(cell);
                    cell = new PdfPCell(new Phrase(headers[2]));
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.VerticalAlignment = Element.ALIGN_CENTER;
                    table.AddCell(cell);
                    cell = new PdfPCell(new Phrase(headers[3]));
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.VerticalAlignment = Element.ALIGN_CENTER;
                    table.AddCell(cell);
                    cell = new PdfPCell(new Phrase(headers[4]));
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.VerticalAlignment = Element.ALIGN_CENTER;
                    table.AddCell(cell);
                    cell = new PdfPCell(new Phrase(headers[5]));
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.VerticalAlignment = Element.ALIGN_CENTER;
                    table.AddCell(cell);
                    break;
                }
                foreach (var item in customerInvoice.InvoiceViewDTOs)
                {
                    cell = new PdfPCell(new Phrase(item.Waybill));
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.VerticalAlignment = Element.ALIGN_CENTER;
                    table.AddCell(cell);
                    cell = new PdfPCell(new Phrase(item.DepartureServiceCentreName));
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.VerticalAlignment = Element.ALIGN_CENTER;
                    table.AddCell(cell);
                    cell = new PdfPCell(new Phrase(item.DestinationServiceCentreName));
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.VerticalAlignment = Element.ALIGN_CENTER;
                    table.AddCell(cell);
                    cell = new PdfPCell(new Phrase(item.ApproximateItemsWeight.ToString()));
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.VerticalAlignment = Element.ALIGN_CENTER;
                    table.AddCell(cell);
                    cell = new PdfPCell(new Phrase(item.Amount.ToString()));
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.VerticalAlignment = Element.ALIGN_CENTER;
                    table.AddCell(cell);
                    cell = new PdfPCell(new Phrase(item.DateCreated.ToString()));
                    cell.HorizontalAlignment = Element.ALIGN_CENTER;
                    cell.VerticalAlignment = Element.ALIGN_CENTER;
                    table.AddCell(cell);
                }

                document.Add(pngImg);
                document.Add(table);
                document.Close();
                byte[] bytes = memoryStream.ToArray();
                memoryStream.Close();

                //save image to blob
                string filename = string.Empty;
                string fileMimeType = "application/pdf";
                var _task = await AzureBlobServiceUtil.UploadFileToBlobAsync(filename, bytes, fileMimeType);
                string fileUrl = _task;
                return fileUrl;

            }
        }

        public async Task<bool> AddCustomerInvoice(CustomerInvoiceDTO customerInvoiceDTO)
        {
            try
            {
                if (customerInvoiceDTO == null)
                {
                    throw new GenericException("Invalid payload", $"{(int)HttpStatusCode.BadRequest}");
                }
                if (!customerInvoiceDTO.InvoiceViewDTOs.Any())
                {
                    throw new GenericException("Invalid payload, No invoice detail", $"{(int)HttpStatusCode.BadRequest}");
                }
                var customerInvoice = JObject.FromObject(customerInvoiceDTO).ToObject<CustomerInvoice>();
                var waybills = customerInvoiceDTO.InvoiceViewDTOs.Select(x => x.Waybill).ToList();
                // customerInvoice.UserID = await _userService.GetCurrentUserId();
                customerInvoice.Waybills = string.Join(",", waybills);
                customerInvoice.Total = customerInvoiceDTO.InvoiceViewDTOs.Sum(x => x.Amount);
                _uow.CustomerInvoice.Add(customerInvoice);
                await _uow.CompleteAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        public async Task<bool> CreateNUBAN(CustomerInvoiceDTO customerInvoice)
        {
            //first create customer on paystack if customer doesnt exist already
            var nuban = new CreateNubanAccountResponseDTO();
            bool res = false;
            var company = await _uow.Company.GetAsync(x => x.CustomerCode == customerInvoice.CustomerCode);

            if (!String.IsNullOrEmpty(company.NUBANAccountNo)) { res = true; return res; }

            var nubanAcc = new CreateNubanAccountDTO()
            {
                customer = 0,
                email = company.Email,
                preferred_bank = "wema-bank",
                first_name = company.Name,
                last_name = company.Name,
                phone = company.PhoneNumber
            };
            if (String.IsNullOrEmpty(company.NUBANCustomerCode))
            {
                var nubanCustomer = await _paystackPaymentService.CreateNubanCustomer(nubanAcc);
                if (nubanCustomer.succeeded)
                {
                    company.NUBANCustomerId = nubanCustomer.data.id;
                    company.NUBANCustomerCode = nubanCustomer.data.customer_code;
                    nubanAcc.customer = nubanCustomer.data.id;
                    nuban = await _paystackPaymentService.CreateUserNubanAccount(nubanAcc);
                    if (nuban.succeeded)
                    {
                        if (company != null)
                        {
                            company.PrefferedNubanBank = nubanAcc.preferred_bank;
                            company.NUBANAccountNo = nuban.data.account_number;
                            company.NUBANCustomerName = nuban.data.account_name;
                        }
                        res = nuban.succeeded;
                    }
                }
            }

            else if (!String.IsNullOrEmpty(company.NUBANCustomerCode) && String.IsNullOrEmpty(company.NUBANAccountNo))
            {
                nubanAcc.customer = company.NUBANCustomerId;
                var customerNubanAccount = await _paystackPaymentService.CreateUserNubanAccount(nubanAcc);
                if (customerNubanAccount.succeeded)
                {
                    if (company != null)
                    {
                        company.PrefferedNubanBank = company.PrefferedNubanBank;
                        company.NUBANAccountNo = customerNubanAccount.data.account_number;
                    }
                }
                res = customerNubanAccount.succeeded;
            }
            return res;
        }

        public async Task<bool> CheckIfInvoiceAlreadyExist(CustomerInvoiceDTO customerInvoice)
        {
            bool res = false;
            var now = DateTime.Today;
            // var month = new DateTime(now.Year, now.Month, 1);
            var firstDay = new DateTime(now.Year, now.Month, 1);
            var lastDay = firstDay.AddMonths(1).AddDays(-1);
            //var now = DateTime.Now;
            //DateTime firstDay = new DateTime(now.Year, now.AddMonths(-1), 1);
            //DateTime lastDay = firstDay.AddMonths(1).AddDays(-1);
            if (firstDay != null && lastDay != null)
            {
                firstDay = firstDay.ToUniversalTime();
                firstDay = firstDay.AddHours(12).AddMinutes(00);
                lastDay = lastDay.ToUniversalTime();
                lastDay = lastDay.AddHours(23).AddMinutes(59);
            }
            var invoice = await _uow.CustomerInvoice.GetAsync(x => x.CustomerCode == customerInvoice.CustomerCode && x.DateCreated >= firstDay && x.DateCreated <= lastDay);
            if (invoice != null)
            {
                res = true;
            }
            return res;
        }
        public async Task<List<CustomerInvoiceDTO>> GetCustomerInvoiceList(DateFilterForDropOff filter)
        {
            try
            {
                return await _uow.Shipment.GetCoporateInvoiceList(filter);
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<bool> MarkInvoiceasPaid(List<CustomerInvoiceDTO> customerInvoices)
        {
            try
            {
                if (customerInvoices.Any())
                {
                    var refNos = customerInvoices.Select(c => c.InvoiceRefNo).ToList();
                    var invoices = _uow.CustomerInvoice.GetAllAsQueryable().Where(x => refNos.Contains(x.InvoiceRefNo)).ToList();
                    var codes = invoices.Select(x => x.CustomerCode).ToList();
                    if (invoices.Any())
                    {
                        var wallets = _uow.Wallet.GetAllAsQueryable().Where(x => codes.Contains(x.CustomerCode)).ToList();
                        foreach (var item in invoices)
                        {
                            item.PaymentStatus = PaymentStatus.Paid;
                            item.DateModified = DateTime.Now;
                            item.UserID = await _userService.GetCurrentUserId();

                            //now manually update user's wallet too
                            var wallet = wallets.Where(x => x.CustomerCode == item.CustomerCode).FirstOrDefault();
                            if (wallet != null)
                            {
                                await _walletService.UpdateWallet(wallet.WalletId, new WalletTransactionDTO()
                                {
                                    WalletId = wallet.WalletId,
                                    Amount = item.Total,
                                    Description = "Payment offset",
                                    PaymentTypeReference = item.InvoiceRefNo,
                                    CreditDebitType = CreditDebitType.Credit
                                });

    }
                        }
                        await _uow.CompleteAsync();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public async Task<List<InvoiceViewDTO>> GetGoFasterReport(NewFilterOptionsDto filter)
        {
            var result = new List<InvoiceViewDTO>();
            if (filter != null && filter.StartDate == null && filter.EndDate == null)
            {
                var now = DateTime.Now;
                filter.StartDate = now.Date;
                filter.EndDate = now.Date;
            }
            if (filter != null && filter.StartDate != null && filter.EndDate != null)
            {
                filter.StartDate = filter.StartDate.Value.ToUniversalTime();
                filter.StartDate = filter.StartDate.Value.AddHours(00).AddMinutes(00);
                filter.EndDate = filter.EndDate.Value.ToUniversalTime();
                filter.EndDate = filter.EndDate.Value.AddHours(23).AddMinutes(59);
            }
            var shipments = await _uow.Shipment.GetGoFasterShipments(filter);
            //group shipment by service centre
            if (shipments.Any())
            {
                var allWeight = shipments.Select(x => x.ApproximateItemsWeight).ToArray();
                int n = allWeight.Length;
                double freq = maxFreq(allWeight, n);
               
                result = shipments.GroupBy(x => new { x.DepartureServiceCentreId }).Select(s => new InvoiceViewDTO
                {
                    Waybill = s.FirstOrDefault().Waybill,
                    DepartureServiceCentreId = s.FirstOrDefault().DepartureServiceCentreId,
                    DestinationServiceCentreId = s.FirstOrDefault().DestinationServiceCentreId,
                    DepartureServiceCentreName = s.FirstOrDefault().DepartureServiceCentreName,
                    DestinationServiceCentreName = s.FirstOrDefault().DestinationServiceCentreName,
                    Amount = s.Sum(x => x.Amount),
                    DateCreated = s.FirstOrDefault().DateCreated,
                    CompanyType = s.FirstOrDefault().CompanyType,
                    CustomerCode = s.FirstOrDefault().CustomerCode,
                    ApproximateItemsWeight = s.Sum(x => x.ApproximateItemsWeight),
                    TotalWeight = s.Sum(x => x.ApproximateItemsWeight),
                    TotalShipment = s.Count(),
                    Cash = s.Sum(x => x.Cash),
                    CustomerType = s.FirstOrDefault().CustomerType,
                    TopWeight = freq
                }).ToList();

                result = result.OrderByDescending(x => x.TotalShipment).ToList();
            }
            return result;
        }
        public async Task<List<InvoiceViewDTO>> GetGoFasterShipmentsByServiceCentre(NewFilterOptionsDto filter)
        {
            if (filter != null && filter.StartDate == null && filter.EndDate == null)
            {
                var now = DateTime.Now;
                filter.StartDate = now.Date;
                filter.EndDate = now.Date;
            }
            if (filter != null && filter.StartDate != null && filter.EndDate != null)
            {
                filter.StartDate = filter.StartDate.Value.ToUniversalTime();
                filter.StartDate = filter.StartDate.Value.AddHours(00).AddMinutes(00);
                filter.EndDate = filter.EndDate.Value.ToUniversalTime();
                filter.EndDate = filter.EndDate.Value.AddHours(23).AddMinutes(59);
            }
            return await _uow.Shipment.GetGoFasterShipmentsByServiceCentre(filter);
        }

        public double maxFreq(double[] arr, int n)
        {
            int res = 0;
            int count = 1;
            for (int i = 1; i < n; i++)
            {
                if (arr[i] == arr[res])
                {
                    count++;
                }
                else
                {
                    count--;
                }

                if (count == 0)
                {
                    res = i;
                    count = 1;
                }

            }

            return arr[res];
        }

    }
}
