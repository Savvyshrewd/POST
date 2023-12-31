﻿using POST.Core.DTO;
using POST.Core.DTO.Account;
using POST.Core.DTO.Dashboard;
using POST.Core.DTO.Report;
using POST.Core.DTO.Shipments;
using POST.Core.DTO.ShipmentScan;
using POST.Core.Enums;
using POST.Core.IMessage;
using POST.Core.IServices;
using POST.Core.IServices.Utility;
using POST.Core.View;
using POST.CORE.DTO.Report;
using POST.CORE.DTO.Shipments;
using POST.CORE.IServices.Report;
using POST.Services.Implementation;
using POST.WebApi.Filters;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace POST.WebApi.Controllers.Report
{
    [Authorize(Roles = "Report, ViewAdmin")]
    [RoutePrefix("api/report")]
    public class ReportsController : BaseWebApiController
    {
        private readonly IShipmentReportService _shipmentService;
        private readonly IAccountReportService _accountService;
        private readonly IEmailService _emailService;
        private INumberGeneratorMonitorService _numberGeneratorMonitorService;

        public ReportsController(IShipmentReportService shipmentService, IAccountReportService accountService, IEmailService emailService, INumberGeneratorMonitorService numberGeneratorMonitorService) : base(nameof(ReportsController))
        {
            _shipmentService = shipmentService;
            _accountService = accountService;
            _emailService = emailService;
            _numberGeneratorMonitorService = numberGeneratorMonitorService;
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("todayshipments")]
        public async Task<IServiceResponse<List<ShipmentDTO>>> GetTodayShipments()
        {
            return await HandleApiOperationAsync(async () =>
            {

                var shipments = await _shipmentService.GetTodayShipments();

                return new ServiceResponse<List<ShipmentDTO>>
                {
                    Object = shipments
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("shipments")]
        public async Task<IServiceResponse<List<ShipmentDTO>>> GetShipments(ShipmentFilterCriteria filterCriteria)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var shipments = await _shipmentService.GetShipments(filterCriteria);

                return new ServiceResponse<List<ShipmentDTO>>
                {
                    Object = shipments
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("income")]
        public async Task<IServiceResponse<List<GeneralLedgerDTO>>> GetIncome(AccountFilterCriteria accountFilterCriteria)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var incomes = await _accountService.GetIncomeReports(accountFilterCriteria);

                return new ServiceResponse<List<GeneralLedgerDTO>>
                {
                    Object = incomes
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("demurage")]
        public async Task<IServiceResponse<List<GeneralLedgerDTO>>> GetDemurage(AccountFilterCriteria accountFilterCriteria)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var incomes = await _accountService.GetDemurageReports(accountFilterCriteria);

                return new ServiceResponse<List<GeneralLedgerDTO>>
                {
                    Object = incomes
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("expenditure")]
        public async Task<IServiceResponse<List<GeneralLedgerDTO>>> GetExpenditure(AccountFilterCriteria accountFilterCriteria)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var expenditures = await _accountService.GetExpenditureReports(accountFilterCriteria);

                return new ServiceResponse<List<GeneralLedgerDTO>>
                {
                    Object = expenditures
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("invoice")]
        public async Task<IServiceResponse<List<InvoiceDTO>>> GetInvoice(AccountFilterCriteria accountFilterCriteria)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var invoices = await _accountService.GetInvoiceReports(accountFilterCriteria);

                return new ServiceResponse<List<InvoiceDTO>>
                {
                    Object = invoices
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("invoiceFromView")]
        public async Task<IServiceResponse<List<InvoiceViewDTO>>> GetInvoiceFromView(AccountFilterCriteria accountFilterCriteria)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var invoices = await _accountService.GetInvoiceReportsFromView(accountFilterCriteria);

                return new ServiceResponse<List<InvoiceViewDTO>>
                {
                    Object = invoices 
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("invoiceFromViewWithDeliverTime")]
        public async Task<IServiceResponse<List<InvoiceViewDTO>>> invoiceFromViewWithDeliverTime(AccountFilterCriteria accountFilterCriteria)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var invoices = await _accountService.GetInvoiceReportsFromViewPlusDeliveryTime(accountFilterCriteria);

                return new ServiceResponse<List<InvoiceViewDTO>>
                {
                    Object = invoices
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("customershipments")]
        public async Task<IServiceResponse<List<ShipmentDTO>>> GetCustomerShipments(ShipmentFilterCriteria f_Criteria)
        {
            return await HandleApiOperationAsync(async () =>
            {

                var shipments = await _shipmentService.GetCustomerShipments(f_Criteria);

                return new ServiceResponse<List<ShipmentDTO>>
                {
                    Object = shipments
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("scanstatusFromView")]
        public async Task<IServiceResponse<List<ScanStatusReportDTO>>> GetScanStatusFromView(ScanTrackFilterCriteria f_Criteria)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var shipmentTrackings = await _shipmentService.GetShipmentTrackingFromViewReport(f_Criteria);

                return new ServiceResponse<List<ScanStatusReportDTO>>
                {
                    Object = shipmentTrackings
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("shipmentprogresssummary")]
        public async Task<IServiceResponse<DashboardDTO>> GetShipmentProgressSummary(ShipmentProgressSummaryFilterCriteria baseFilterCriteria)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var summary = await _shipmentService.GetShipmentProgressSummary(baseFilterCriteria);

                return new ServiceResponse<DashboardDTO>
                {
                    Object = summary
                };
            });
        }


        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("shipmentprogresssummarybreakdown")]
        public async Task<IServiceResponse<List<InvoiceViewDTO>>> GetShipmentProgressSummaryBreakDown(ShipmentProgressSummaryFilterCriteria baseFilterCriteria)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var summary = await _shipmentService.GetShipmentProgressSummaryBreakDown(baseFilterCriteria);

                return new ServiceResponse<List<InvoiceViewDTO>>
                {
                    Object = summary
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("giggoshipment")]
        public async Task<IServiceResponse<List<PreShipmentMobileReportDTO>>> PreShipmentMobileReport(MobileShipmentFilterCriteria baseFilterCriteria)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var summary = await _shipmentService.GetPreShipmentMobile(baseFilterCriteria);

                return new ServiceResponse<List<PreShipmentMobileReportDTO>>
                {
                    Object = summary
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("earningsbreakdown")]
        public async Task<IServiceResponse<EarningsBreakdownDTO>> GetEarningsBreakdown(DashboardFilterCriteria dashboardFilter)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var earnings = await _accountService.GetEarningsBreakdown(dashboardFilter);

                return new ServiceResponse<EarningsBreakdownDTO>
                {
                    Object = earnings
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("financialreport")]
        public async Task<IServiceResponse<List<FinancialReportDTO>>> GetFinancialBreakdownByType(AccountFilterCriteria accountFilter)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var financialreport = await _accountService.GetFinancialBreakdownByType(accountFilter);

                return new ServiceResponse<List<FinancialReportDTO>>
                {
                    Object = financialreport
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("walletfundingbreakdown")]
        public async Task<IServiceResponse<List<WalletPaymentLogView>>> GetWalletPaymentLogBreakdown(DashboardFilterCriteria dashboardFilter)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var report = await _accountService.GetWalletPaymentLogBreakdown(dashboardFilter);

                return new ServiceResponse<List<WalletPaymentLogView>>
                {
                    Object = report
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("outboundfinancialreport")]
        public async Task<IServiceResponse<List<OutboundFinancialReportDTO>>> GetFinancialBreakdownOfOutboundShipments(AccountFilterCriteria accountFilter)
        {
            return await HandleApiOperationAsync(async () =>
            {
                int queryType = 0;
                var financialreport = await _accountService.GetFinancialBreakdownOfOutboundShipments(accountFilter, queryType);

                return new ServiceResponse<List<OutboundFinancialReportDTO>>
                {
                    Object = financialreport
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("inboundfinancialreport")]
        public async Task<IServiceResponse<List<OutboundFinancialReportDTO>>> GetFinancialBreakdownOfInboundShipments(AccountFilterCriteria accountFilter)
        {
            return await HandleApiOperationAsync(async () =>
            {
                int queryType = 1;
                var financialreport = await _accountService.GetFinancialBreakdownOfOutboundShipments(accountFilter, queryType);

                return new ServiceResponse<List<OutboundFinancialReportDTO>>
                {
                    Object = financialreport
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("customerinvoice")]
        public async Task<IServiceResponse<CustomerInvoiceDTO>> GetCoporateTransactionsByCode(DateFilterForDropOff filter)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var report = await _shipmentService.GetCoporateTransactionsByCode(filter);

                return new ServiceResponse<CustomerInvoiceDTO>
                {
                    Object = report
                };
            });
        }
        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("generatecustomerinvoice")]
        public async Task<IServiceResponse<bool>> GenerateCustomerInvoice(CustomerInvoiceDTO customerInvoiceDTO)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var report = await _shipmentService.GenerateCustomerInvoice(customerInvoiceDTO);

                return new ServiceResponse<bool>
                {
                    Object = report
                };
            });
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("getcustomerinvoice")]
        public async Task<bool> GenerateCustomerInvoice()
        {
            try
            {
                var now = DateTime.Now;
                var res = ConfigurationManager.AppSettings["InvoicingDay"];
                var day = Convert.ToInt32(res);
                DateTime firstDay = new DateTime(now.Year, now.Month, day);
                DateTime lastDay = firstDay.AddMonths(1).AddDays(-1);
                if (firstDay.Date == now.Date)
                {
                    var shipments = await _shipmentService.GetMonthlyCoporateTransactions();
                    if (shipments.Any())
                    {
                        foreach (var item in shipments)
                        {
                            DateFilterForDropOff filter = new DateFilterForDropOff();
                            filter.CustomerCode = item.CustomerCode;
                            DateTime fDay = new DateTime(now.Year, now.Month, 1);
                            fDay = fDay.AddMonths(-1);
                            DateTime lDay = fDay.AddMonths(1).AddDays(-1);
                            filter.StartDate = fDay;
                            filter.EndDate = lDay;
                            var invoice = await GetCoporateTransactionsByCode(filter);
                            item.InvoiceViewDTOs = invoice.Object.InvoiceViewDTOs;
                            item.InvoiceDate = filter.StartDate.Value;
                            //send email to customer
                            var message = new MessageDTO()
                            {
                                ToEmail = item.Email,
                                To = item.Email,
                            };
                            var alreadyExist = await _shipmentService.CheckIfInvoiceAlreadyExist(item);
                            if (!alreadyExist)
                            {
                                item.InvoiceRefNo = await _numberGeneratorMonitorService.GenerateInvoiceRefNoWithDate(NumberGeneratorType.Invoice, item.CustomerCode, firstDay, lastDay);
                                var pdf = await _shipmentService.GeneratePDF(item);
                                message.MessageTemplate = "CooperateEmail";

                                //check if user has a nuban account, create if not
                                var acc = await _shipmentService.CreateNUBAN(item);
                                if (acc)
                                {
                                    message.PDF = pdf;
                                    message.CustomerInvoice = item;
                                    var result = await _emailService.ConfigSendGridMonthlyCorporateTransactions(message);
                                    var saved = await _shipmentService.AddCustomerInvoice(message.CustomerInvoice);

                                }
                            }
                        }
                    } 
                }
                return true;
            }
            catch (Exception ex)
            {
                throw;
            }


        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("customerinvoicelist")]
        public async Task<IServiceResponse<List<CustomerInvoiceDTO>>> GetCustomerInvoiceList(DateFilterForDropOff filter)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var report = await _shipmentService.GetCustomerInvoiceList(filter);

                return new ServiceResponse<List<CustomerInvoiceDTO>>
                {
                    Object = report
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("markaspaid")]
        public async Task<IServiceResponse<bool>> MarkInvoiceasPaid(List<CustomerInvoiceDTO> customerInvoices)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var report = await _shipmentService.MarkInvoiceasPaid(customerInvoices);

                return new ServiceResponse<bool>
                {
                    Object = report
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("getgofasterreport")]
        public async Task<IServiceResponse<List<InvoiceViewDTO>>> GetGoFasterReport(NewFilterOptionsDto filter)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var report = await _shipmentService.GetGoFasterReport(filter);

                return new ServiceResponse<List<InvoiceViewDTO>>
                {
                    Object = report
                };
            });
        }
        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("getgofasterreportbycentre")]
        public async Task<IServiceResponse<List<InvoiceViewDTO>>> GetGoFasterShipmentsByServiceCentre(NewFilterOptionsDto filter)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var report = await _shipmentService.GetGoFasterShipmentsByServiceCentre(filter);

                return new ServiceResponse<List<InvoiceViewDTO>>
                {
                    Object = report
                };
            });
        }
    }
}