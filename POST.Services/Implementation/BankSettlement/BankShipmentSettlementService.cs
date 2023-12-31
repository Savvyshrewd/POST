﻿using AutoMapper;
using POST.Core;
using POST.Core.Domain;
using POST.Core.Domain.BankSettlement;
using POST.Core.Domain.Wallet;
using POST.Core.DTO.Account;
using POST.Core.DTO.BankSettlement;
using POST.Core.DTO.MessagingLog;
using POST.Core.DTO.Report;
using POST.Core.DTO.Wallet;
using POST.Core.Enums;
using POST.Core.IMessageService;
using POST.Core.IServices.Account;
using POST.Core.IServices.BankSettlement;
using POST.Core.IServices.ServiceCentres;
using POST.Core.IServices.User;
using POST.Core.IServices.Utility;
using POST.Core.IServices.Wallet;
using POST.Infrastructure;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace POST.Services.Implementation.Wallet
{
    public class BankShipmentSettlementService : IBankShipmentSettlementService
    {
        private readonly IUnitOfWork _uow;
        private readonly IWalletService _walletService;
        private readonly IUserService _userService;
        private readonly INumberGeneratorMonitorService _service;
        private readonly IGlobalPropertyService _globalPropertyService;
        private readonly IInvoiceService _invoiceService;
        private readonly IBankService _bankService;
        private readonly IMessageSenderService _messageSenderService;
        private readonly IServiceCentreService _serviceCenter;
        private IRegionServiceCentreMappingService _regionServiceCentreMappingService;

        public BankShipmentSettlementService(IUnitOfWork uow, IWalletService walletService, IUserService userService, INumberGeneratorMonitorService service, IGlobalPropertyService globalPropertyService,
            IInvoiceService invoiceservice, IBankService bankService, IMessageSenderService messageSenderService, IServiceCentreService serviceCenter, IRegionServiceCentreMappingService regionServiceCentreMappingService)
        {
            _uow = uow;
            _walletService = walletService;
            _userService = userService;
            _service = service;
            _invoiceService = invoiceservice;
            _bankService = bankService;
            _messageSenderService = messageSenderService;
            MapperConfig.Initialize();
            _globalPropertyService = globalPropertyService;
            _serviceCenter = serviceCenter;
            _regionServiceCentreMappingService = regionServiceCentreMappingService;
        }

        public BankShipmentSettlementService()
        {

        }

        public async Task<IEnumerable<InvoiceViewDTO>> GetCashShipmentSettlement()
        {
            var serviceCenters = await _userService.GetPriviledgeServiceCenters();
            var allShipments = _uow.Invoice.GetAllFromInvoiceAndShipments();
            allShipments = allShipments.Where(s => s.PaymentMethod == "Cash" && s.PaymentStatus == PaymentStatus.Paid);

            var cashShipments = new List<InvoiceViewDTO>();
            if (serviceCenters.Length > 0)
            {
                var shipmentResult = allShipments.Where(s => serviceCenters.Contains(s.DestinationServiceCentreId)).ToList();
                cashShipments = Mapper.Map<List<InvoiceViewDTO>>(shipmentResult);
            }

            return await Task.FromResult(cashShipments);
        }

        public async Task<object> GetBankProcessingOrderForShipment_ScheduleTask(int serviceCenterId, DepositType type)
        {
            var userActiveCountryId = 1;
            var serviceCentersId = serviceCenterId;

            var userid = "7b8a19f4-b634-4074-88a0-ebe78ebcef1b";

            //Get Bank Deposit Module StartDate
            var globalpropertiesdateObj = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.BankDepositModuleStartDate, userActiveCountryId);
            string globalpropertiesdateStr = globalpropertiesdateObj?.Value;

            var globalpropertiesdate = DateTime.MinValue;
            bool success = DateTime.TryParse(globalpropertiesdateStr, out globalpropertiesdate);

            var allShipments = _uow.Invoice.GetAllFromInvoiceAndShipments();

            allShipments = allShipments.Where(s => s.PaymentMethod == "Cash" && s.PaymentStatus == PaymentStatus.Paid);
            allShipments = allShipments.Where(s => s.DepositStatus == DepositStatus.Unprocessed && s.DateCreated >= globalpropertiesdate);

            //A. get partial payment values
            var allShipmentsPartial = _uow.Invoice.GetAllFromInvoiceAndShipments().Where(s => s.DepositStatus == DepositStatus.Unprocessed && s.DateCreated >= globalpropertiesdate && s.PaymentMethod.Contains("Partial"));

            //B. combine list for partial and cash shipment
            var cashShipments = new List<InvoiceViewDTO>();
            if (serviceCentersId > 0)
            {
                var shipmentResult = allShipments.Where(s => s.DepartureServiceCentreId == serviceCentersId).ToList();
                var allShipmentsPartialVals = allShipmentsPartial.Where(s => s.DepartureServiceCentreId == serviceCentersId).ToList();

                shipmentResult.AddRange(allShipmentsPartialVals);
                cashShipments = Mapper.Map<List<InvoiceViewDTO>>(shipmentResult);
            }

            var cashShipmentsVal = new List<InvoiceViewDTO>();
            foreach (var item in cashShipments)
            {
                //1. cash first
                if (item.PaymentMethod == "Cash")
                {
                    cashShipmentsVal.Add(item);
                }

                //2. partial
                if (item.PaymentMethod.Contains("Partial"))
                {
                    var partialPaymentCash = await returnPartialPaymentCashByWaybill(item.Waybill);

                    if (partialPaymentCash.Item1 != null && partialPaymentCash.Item1.Count > 0)
                    {
                        item.GrandTotal = partialPaymentCash.Item2;
                        cashShipmentsVal.Add(item);
                    }
                }
            }

            //3. sum total
            decimal total = cashShipmentsVal.Sum(s => s.GrandTotal);
            var ServiceCenter = await _serviceCenter.GetServiceCentreById(serviceCentersId);
            BankProcessingOrderCodesDTO bankDep = new BankProcessingOrderCodesDTO();

            //Generate the refcode
            string refcode = "00000000";
            if (total > 0)
            {
                refcode = await _service.GenerateNextNumber(NumberGeneratorType.BankProcessingOrderForShipment, ServiceCenter.Code);

                //Add the generated Waybills to current service Center
                bankDep = new BankProcessingOrderCodesDTO()
                {
                    Code = refcode,
                    TotalAmount = total,
                    DateAndTimeOfDeposit = DateTime.Now,
                    UserId = userid,
                    FullName = "Task Scheduler",
                    ServiceCenter = serviceCentersId,
                    ScName = ServiceCenter.Name,
                    DepositType = DepositType.Shipment,
                    StartDateTime = DateTime.Now,
                    Status = DepositStatus.Pending,
                    VerifiedBy = "",
                    BankName = "",
                    AmountInputted = 0,
                    ShipmentAndCOD = cashShipmentsVal.Select(s => new BankProcessingOrderForShipmentAndCODDTO()
                    {
                        Waybill = s.Waybill,
                        RefCode = refcode,
                        DepositType = DepositType.Shipment,
                        GrandTotal = s.GrandTotal,
                        CODAmount = s.Amount,
                        ServiceCenterId = serviceCentersId,
                        ServiceCenter = ServiceCenter.Name,
                        UserId = userid,
                        Status = DepositStatus.Pending
                    }).ToList()
                };
            }
            if (bankDep.ShipmentAndCOD.Count > 0)
            {
                await AddBankProcessingOrderCode_ScheduleTask(bankDep);
            }
            return Task.FromResult(0);
        }

        public async Task<BankProcessingOrderCodesDTO> AddBankProcessingOrderCode_ScheduleTask(BankProcessingOrderCodesDTO bkoc)
        {
            try
            {
                if (bkoc == null)
                {
                    throw new GenericException("NULL INPUT");
                }

                if (!bkoc.ShipmentAndCOD.Any())
                {
                    throw new GenericException("Processing Information Not Found");
                }

                if (string.IsNullOrWhiteSpace(bkoc.Code))
                {
                    throw new GenericException("Ref Code can not be empty");
                }
                else
                {
                    //Validate the code if it follow the code format
                    string getCode = bkoc.Code.Length > 2 ? bkoc.Code.Substring(0, 2) : "0";

                    if (int.TryParse(getCode, out int numCode))
                    {
                        int enumShipment = (int)NumberGeneratorType.BankProcessingOrderForShipment;
                        int enumCod = (int)NumberGeneratorType.BankProcessingOrderForCOD;
                        int enumDemurage = (int)NumberGeneratorType.BankProcessingOrderForDemurrage;

                        if (numCode != enumShipment && numCode != enumCod && numCode != enumDemurage)
                        {
                            throw new GenericException("Ref Code not accepted, Contact IT", $"{(int)HttpStatusCode.Forbidden}");
                        }
                    }
                    else
                    {
                        throw new GenericException("Ref Code not accepted, Contact IT", $"{(int)HttpStatusCode.Forbidden}");
                    }
                }

                //Validate if the code already exist
                var dataExist = await _uow.BankProcessingOrderForShipmentAndCOD.ExistAsync(x => x.RefCode == bkoc.Code);
                if (dataExist)
                {
                    throw new GenericException("Ref Code Already exist, Contact IT", $"{(int)HttpStatusCode.Forbidden}");
                }

                //1. get the current service user
                bkoc.UserId = bkoc.UserId;
                bkoc.FullName = "Task Scheduler";
                var userActiveCountryId = 1; 

                //3. Get Bank Deposit Module StartDate
                var globalpropertiesdateObj = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.BankDepositModuleStartDate, userActiveCountryId);
                string globalpropertiesdateStr = globalpropertiesdateObj?.Value;

                var globalpropertiesdate = DateTime.MinValue;
                bool success = DateTime.TryParse(globalpropertiesdateStr, out globalpropertiesdate);
                var bankordercodes = new BankProcessingOrderCodes();

                //4. updating the startdate
                bkoc.StartDateTime = globalpropertiesdate;

                //5. commence preparatiion to insert records in the BankProcessingOrderForShipmentAndCOD
                var enddate = bkoc.DateAndTimeOfDeposit;
                if (bkoc.DepositType == DepositType.Shipment)
                {
                    //all shipments from payload JSON
                    var allShipmentsVals = bkoc.ShipmentAndCOD;
                    var result = allShipmentsVals.Select(s => s.Waybill);

                    //--------------------------Validation Section -------------------------------------------//
                    var allprocessingordeforshipment = _uow.BankProcessingOrderForShipmentAndCOD.GetAll().Where(s => s.DepositType == bkoc.DepositType && result.Contains(s.Waybill));

                    //var validateInsertWaybills = false;
                    if (allprocessingordeforshipment.Any())
                    {
                        throw new GenericException("Error validating one or more waybills, Please try requesting again for a fresh record.");
                    }

                    //--------------------------Validation Section -------------------------------------------//
                    var bankorderforshipmentandcod = allShipmentsVals.Select(s => new BankProcessingOrderForShipmentAndCOD()
                    {
                        Waybill = s.Waybill,
                        RefCode = bkoc.Code,
                        DepositType = bkoc.DepositType,
                        GrandTotal = s.GrandTotal,
                        CODAmount = s.CODAmount,
                        ServiceCenterId = bkoc.ServiceCenter,
                        ServiceCenter = bkoc.ScName,
                        UserId = bkoc.UserId,
                        Status = DepositStatus.Pending
                    });

                    var arrWaybills = allShipmentsVals.Select(x => x.Waybill).ToArray();
                    bankordercodes = Mapper.Map<BankProcessingOrderCodes>(bkoc);
                    bankordercodes.TotalAmount = bkoc.TotalAmount;
                    _uow.BankProcessingOrderCodes.Add(bankordercodes);
                    _uow.BankProcessingOrderForShipmentAndCOD.AddRange(bankorderforshipmentandcod);

                    //select a list of values that contains the allshipment from the invoice view
                    var nonDepsitedValue = _uow.Shipment.GetAll().Where(x => arrWaybills.Contains(x.Waybill)).ToList();
                    var nonDepsitedValueunprocessed = nonDepsitedValue.Where(s => s.DepositStatus == DepositStatus.Unprocessed && s.DateCreated >= globalpropertiesdate).ToList();
                    nonDepsitedValueunprocessed.ForEach(a => a.DepositStatus = DepositStatus.Pending);
                }
                else if (bkoc.DepositType == DepositType.COD)
                {
                    var serviceCenters = bkoc.ServiceCenter;

                    //--------------------------Validation Section -------------------------------------------//
                    //all shipments from payload JSON
                    var allprocessingordeforshipment = bkoc.ShipmentAndCOD;
                    var resultVals = allprocessingordeforshipment.Select(s => s.Waybill);

                    //--------------------------Validation Section -------------------------------------------//
                    var allprocessingordeforcods = _uow.BankProcessingOrderForShipmentAndCOD.GetAll().Where(s => s.DepositType == bkoc.DepositType && resultVals.Contains(s.Waybill));

                    if (allprocessingordeforcods.Any())
                    {
                        throw new GenericException("Error validating one or more CODs, Please try requesting again for a fresh record.");
                    }

                    //--------------------------Validation Section -------------------------------------------//
                    var bankorderforshipmentandcod = allprocessingordeforshipment.Select(s => new BankProcessingOrderForShipmentAndCOD()
                    {
                        Waybill = s.Waybill,
                        RefCode = bkoc.Code,
                        ServiceCenterId = bkoc.ServiceCenter,
                        CODAmount = s.CODAmount,
                        DepositType = bkoc.DepositType,
                        ServiceCenter = bkoc.ScName,
                        Status = DepositStatus.Pending
                    });

                    //1. get data from COD register account as queryable from CashOnDeliveryRegisterAccount table
                    var allCODs = _uow.CashOnDeliveryRegisterAccount.GetCODAsQueryable();
                    allCODs = allCODs.Where(s => s.DepositStatus == DepositStatus.Unprocessed && s.PaymentType == PaymentType.Cash);
                    allCODs = allCODs.Where(s => s.CODStatusHistory == CODStatushistory.RecievedAtServiceCenter || s.CODStatusHistory == CODStatushistory.CollectedByDispatch);

                    //select a values from 
                    var nonDepsitedValueunprocessed = allCODs.Where(s => resultVals.Contains(s.Waybill)).ToList();

                    //Collect total shipment unproceessed and its total
                    decimal codTotal = 0;
                    foreach (var item in allprocessingordeforshipment)
                    {
                        codTotal += item.CODAmount;
                    }

                    bkoc.TotalAmount = codTotal;
                    bankordercodes = Mapper.Map<BankProcessingOrderCodes>(bkoc);
                    nonDepsitedValueunprocessed.ForEach(a => a.DepositStatus = DepositStatus.Pending);
                    nonDepsitedValueunprocessed.ForEach(a => a.RefCode = bkoc.Code);

                    _uow.BankProcessingOrderCodes.Add(bankordercodes);
                    _uow.BankProcessingOrderForShipmentAndCOD.AddRange(bankorderforshipmentandcod);
                }

                await _uow.CompleteAsync();

                return new BankProcessingOrderCodesDTO()
                {
                    CodeId = bankordercodes.CodeId,
                    Code = bankordercodes.Code,
                    DateAndTimeOfDeposit = bankordercodes.DateAndTimeOfDeposit
                };
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        //New bank processing order for shipment
        public async Task<Tuple<string, List<InvoiceViewDTO>, decimal>> GetBankProcessingOrderForShipment(DepositType type)
        {
            var userActiveCountryId = await _userService.GetUserActiveCountryId();
            var serviceCenters = await _userService.GetPriviledgeServiceCenters();

            //Get Bank Deposit Module StartDate
            var globalpropertiesdateObj = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.BankDepositModuleStartDate, userActiveCountryId);
            string globalpropertiesdateStr = globalpropertiesdateObj?.Value;

            var globalpropertiesdate = DateTime.MinValue;
            bool success = DateTime.TryParse(globalpropertiesdateStr, out globalpropertiesdate);

            var allShipments = _uow.Invoice.GetAllFromInvoiceAndShipments(); //invoice view

            allShipments = allShipments.Where(s => s.PaymentMethod == "Cash" && s.PaymentStatus == PaymentStatus.Paid);
            allShipments = allShipments.Where(s => s.DepositStatus == DepositStatus.Unprocessed && s.DateCreated >= globalpropertiesdate);

            //A. get partial payment values
            var allShipmentsPartial = _uow.Invoice.GetAllFromInvoiceAndShipments().Where(s => s.DepositStatus == DepositStatus.Unprocessed && s.DateCreated >= globalpropertiesdate && s.PaymentMethod.Contains("Partial"));

            //B. combine list for partial and cash shipment
            var cashShipments = new List<InvoiceViewDTO>();
            if (serviceCenters.Length > 0)
            {
                var shipmentResult = allShipments.Where(s => serviceCenters.Contains(s.DepartureServiceCentreId)).ToList();
                var allShipmentsPartialVals = allShipmentsPartial.Where(s => serviceCenters.Contains(s.DepartureServiceCentreId)).ToList();

                shipmentResult.AddRange(allShipmentsPartialVals);
                cashShipments = Mapper.Map<List<InvoiceViewDTO>>(shipmentResult);
            }

            var cashShipmentsVal = new List<InvoiceViewDTO>();
            foreach (var item in cashShipments)
            {
                //1. cash first
                if (item.PaymentMethod == "Cash")
                {
                    cashShipmentsVal.Add(item);
                }

                //2. partial
                if (item.PaymentMethod.Contains("Partial"))
                {
                    var partialPaymentCash = await returnPartialPaymentCashByWaybill(item.Waybill);

                    if (partialPaymentCash.Item1 != null && partialPaymentCash.Item1.Count > 0)
                    {
                        item.GrandTotal = partialPaymentCash.Item2;
                        cashShipmentsVal.Add(item);
                    }
                }
            }

            //3. sum total
            decimal total = cashShipmentsVal.Sum(s => s.GrandTotal);

            //Generate the refcode
            string refcode = "00000000";
            if (total > 0)
            {
                var getServiceCenterCode = await _userService.GetCurrentServiceCenter();
                refcode = await _service.GenerateNextNumber(NumberGeneratorType.BankProcessingOrderForShipment, getServiceCenterCode[0].Code);
            }

            var comboresult = Tuple.Create(refcode, cashShipmentsVal, total);
            return await Task.FromResult(comboresult);
        }

        private async Task<Tuple<List<PaymentPartialTransaction>, decimal>> returnPartialPaymentCashByWaybill(string waybill)
        {
            decimal total = 0;
            var cashPaymentPartial = _uow.PaymentPartialTransaction.GetAll().Where(s => s.Waybill == waybill && s.PaymentType == PaymentType.Cash).ToList();

            foreach (var item in cashPaymentPartial)
            {
                total += item.Amount;
            }

            var vals = Tuple.Create(cashPaymentPartial, total);

            var comboresult = Tuple.Create(cashPaymentPartial, total);
            return await Task.FromResult(comboresult);
        }

        public async Task<object> GetBankProcessingOrderForDemurrage_ScheduleTask(DepositType type, int servicecenterId)
        {
            decimal total = 0;
            string userid = "7b8a19f4-b634-4074-88a0-ebe78ebcef1b";

            var serviceCenter = servicecenterId;
            var allDemurrages = _uow.DemurrageRegisterAccount.GetDemurrageAsQueryable();
            allDemurrages = allDemurrages.Where(s => s.DEMStatusHistory == CODStatushistory.RecievedAtServiceCenter);
            allDemurrages = allDemurrages.Where(s => s.DepositStatus == DepositStatus.Unprocessed && s.PaymentType == PaymentType.Cash);

            var demurrageResults = new List<DemurrageRegisterAccount>();
            if (serviceCenter > 0)
            {
                demurrageResults = allDemurrages.Where(s => s.ServiceCenterId == serviceCenter && s.Amount >0).ToList();
            }

            foreach (var item in demurrageResults)
            {
                total += item.Amount;
            }

            //Generate the refcode
            BankProcessingOrderCodesDTO bankDep = new BankProcessingOrderCodesDTO();
            string refcode = "00000000";
            if (total > 0)
            {
                var getServiceCenterCode = await _serviceCenter.GetServiceCentreById(serviceCenter);
                refcode = await _service.GenerateNextNumber(NumberGeneratorType.BankProcessingOrderForDemurrage, getServiceCenterCode.Code);

                //Add to db
                bankDep = new BankProcessingOrderCodesDTO()
                {
                    Code = refcode,
                    TotalAmount = total,
                    DateAndTimeOfDeposit = DateTime.Now,
                    UserId = userid,
                    FullName = "Task Scheduler",
                    ServiceCenter = serviceCenter,
                    ScName = getServiceCenterCode.Name,
                    DepositType = DepositType.Demurrage,
                    StartDateTime = DateTime.Now,
                    Status = DepositStatus.Pending,
                    VerifiedBy = "",
                    BankName = "",
                    AmountInputted = 0,
                    ShipmentAndCOD = demurrageResults.Select(s => new BankProcessingOrderForShipmentAndCODDTO()
                    {
                        Waybill = s.Waybill,
                        RefCode = refcode,
                        DepositType = DepositType.Shipment,
                        DemurrageAmount = s.Amount,
                        ServiceCenterId = serviceCenter,
                        ServiceCenter = getServiceCenterCode.Name,
                        UserId = userid,
                        Status = DepositStatus.Pending
                    }).Where(s => s.DemurrageAmount > 0).ToList()
                };
            }
            if (bankDep.ShipmentAndCOD.Any())
            {
                await AddBankProcessingOrderCodeDemurrageOnly_ScheduleTask(bankDep);
            }
            return Task.FromResult(0);
        }

        //New bank processing order for Demurrage
        public async Task<Tuple<string, List<DemurrageRegisterAccountDTO>, decimal>> GetBankProcessingOrderForDemurrage(DepositType type)
        {
            decimal total = 0;

            var serviceCenters = await _userService.GetPriviledgeServiceCenters();
            var allDemurrages = _uow.DemurrageRegisterAccount.GetDemurrageAsQueryable();
            allDemurrages = allDemurrages.Where(s => s.DEMStatusHistory == CODStatushistory.RecievedAtServiceCenter);
            allDemurrages = allDemurrages.Where(s => s.DepositStatus == DepositStatus.Unprocessed && s.PaymentType == PaymentType.Cash);

            var demurrageResults = new List<DemurrageRegisterAccount>();
            if (serviceCenters.Length > 0)
            {
                demurrageResults = allDemurrages.Where(s => serviceCenters.Contains(s.ServiceCenterId)).ToList();
            }

            foreach (var item in demurrageResults)
            {
                total += item.Amount;
            }

            //Generate the refcode
            string refcode = "00000000";
            if (total > 0)
            {
                var getServiceCenterCode = await _userService.GetCurrentServiceCenter();
                refcode = await _service.GenerateNextNumber(NumberGeneratorType.BankProcessingOrderForDemurrage, getServiceCenterCode[0].Code);
            }

            var cashdemurrage = Mapper.Map<List<DemurrageRegisterAccountDTO>>(demurrageResults);
            var comboresult = Tuple.Create(refcode, cashdemurrage, total);
            return await Task.FromResult(comboresult);
        }

        //New bank processing order for COD
        public async Task<object> GetBankProcessingOrderForCOD_ScheduledTask(DepositType type, int ServiceCenterId)
        {
            decimal total = 0;
            string userid = "7b8a19f4-b634-4074-88a0-ebe78ebcef1b";
            var serviceCenter = ServiceCenterId;
            var allCODs = _uow.CashOnDeliveryRegisterAccount.GetCODAsQueryable();
            allCODs = allCODs.Where(s => s.CODStatusHistory == CODStatushistory.RecievedAtServiceCenter || s.CODStatusHistory == CODStatushistory.CollectedByDispatch);
            allCODs = allCODs.Where(s => s.DepositStatus == DepositStatus.Unprocessed && s.PaymentType == PaymentType.Cash);

            var codResults = new List<CashOnDeliveryRegisterAccount>();
            if (serviceCenter > 0)
            {
                codResults = allCODs.Where(s => s.ServiceCenterId == serviceCenter).ToList();
            }

            foreach (var item in codResults)
            {
                total += item.Amount;
            }

            //Generate the refcode
            BankProcessingOrderCodesDTO bankDep = new BankProcessingOrderCodesDTO();
            string refcode = "00000000";
            if (total > 0)
            {
                var getServiceCenterCode = await _serviceCenter.GetServiceCentreById(serviceCenter);
                refcode = await _service.GenerateNextNumber(NumberGeneratorType.BankProcessingOrderForCOD, getServiceCenterCode.Code);

                //Add the generated Waybills to current service Center
                bankDep = new BankProcessingOrderCodesDTO()
                {
                    Code = refcode,
                    TotalAmount = total,
                    DateAndTimeOfDeposit = DateTime.Now,
                    UserId = userid,
                    FullName = "Task Scheduler",
                    ServiceCenter = serviceCenter,
                    ScName = getServiceCenterCode.Name,
                    DepositType = DepositType.COD,
                    StartDateTime = DateTime.Now,
                    Status = DepositStatus.Pending,
                    VerifiedBy = "",
                    BankName = "",
                    AmountInputted = 0,
                    ShipmentAndCOD = codResults.Select(s => new BankProcessingOrderForShipmentAndCODDTO()
                    {
                        Waybill = s.Waybill,
                        RefCode = refcode,
                        DepositType = DepositType.COD,
                        CODAmount = s.Amount,
                        ServiceCenterId = serviceCenter,
                        ServiceCenter = getServiceCenterCode.Name,
                        UserId = userid,
                        Status = DepositStatus.Pending
                    }).ToList()
                };
            }

            if (bankDep.ShipmentAndCOD.Any())
            {
                await AddBankProcessingOrderCode_ScheduleTask(bankDep);
            }
            return Task.FromResult(0);
        }

        //New bank processing order for COD
        public async Task<Tuple<string, List<CashOnDeliveryRegisterAccountDTO>, decimal>> GetBankProcessingOrderForCOD(DepositType type)
        {
            decimal total = 0;

            var serviceCenters = await _userService.GetPriviledgeServiceCenters();
            var allCODs = _uow.CashOnDeliveryRegisterAccount.GetCODAsQueryable();
            allCODs = allCODs.Where(s => s.CODStatusHistory == CODStatushistory.RecievedAtServiceCenter || s.CODStatusHistory == CODStatushistory.CollectedByDispatch);
            allCODs = allCODs.Where(s => s.DepositStatus == DepositStatus.Unprocessed && s.PaymentType == PaymentType.Cash);

            var codResults = new List<CashOnDeliveryRegisterAccount>();
            if (serviceCenters.Length > 0)
            {
                codResults = allCODs.Where(s => serviceCenters.Contains(s.ServiceCenterId)).ToList();
            }

            foreach (var item in codResults)
            {
                total += item.Amount;
            }

            //Generate the refcode
            string refcode = "00000000";
            if (total > 0)
            {
                var getServiceCenterCode = await _userService.GetCurrentServiceCenter();
                refcode = await _service.GenerateNextNumber(NumberGeneratorType.BankProcessingOrderForCOD, getServiceCenterCode[0].Code);
            }

            var cashcods = Mapper.Map<List<CashOnDeliveryRegisterAccountDTO>>(codResults);
            var comboresult = Tuple.Create(refcode, cashcods, total);
            return await Task.FromResult(comboresult);
        }


        //search from the accountants end of Agility
        public async Task<Tuple<string, List<BankProcessingOrderForShipmentAndCODDTO>, decimal, List<BankProcessingOrderCodesDTO>>> SearchBankProcessingOrder2(string _refcode, DepositType type)
        {
            var bankprcessingresult = await _uow.BankProcessingOrderCodes.GetBankOrderProcessingCode(type);
            var bankprcessingresultValue = bankprcessingresult.Where(s => s.Code == _refcode.Trim()).ToList();

            //get the start and end date for retrieving of waybills for the bank
            var refcode = _refcode.Trim();

            //Generate the refcode
            var getServiceCenterCode = await _userService.GetCurrentServiceCenter();
            decimal total = 0;

            var bankedShipments = new List<BankProcessingOrderForShipmentAndCODDTO>();

            var comboresult = Tuple.Create(refcode, bankedShipments, total, bankprcessingresultValue);
            return await Task.FromResult(comboresult);
        }

        //General Search
        public async Task<Tuple<string, List<BankProcessingOrderForShipmentAndCODDTO>, decimal, BankProcessingOrderCodesDTO>> SearchBankProcessingOrder(string _refcode, DepositType type)
        {
            var bankprcessingresult = await _uow.BankProcessingOrderCodes.GetBankOrderProcessingCode(type);
            var bankprcessingresultValue = bankprcessingresult.Where(s => s.Code == _refcode.Trim()).FirstOrDefault();

            var refcode = _refcode.Trim();

            //Generate the refcode
            var getServiceCenterCode = await _userService.GetCurrentServiceCenter();
            decimal total = 0;

            var serviceCenters = await _userService.GetPriviledgeServiceCenters();
            var accompanyWaybills = await _uow.BankProcessingOrderForShipmentAndCOD.GetAllWaybillsForBankProcessingOrders(type);
            var accompanyWaybillsVals = accompanyWaybills.Where(s => s.RefCode == refcode);

            var bankedShipments = new List<BankProcessingOrderForShipmentAndCODDTO>();
            if (serviceCenters.Length > 0)
            {
                var shipmentResult = accompanyWaybillsVals.Where(s => serviceCenters.Contains(s.ServiceCenterId) && (s.Status == DepositStatus.Pending || s.Status == DepositStatus.Deposited || s.Status == DepositStatus.Verified)).ToList();
                bankedShipments = Mapper.Map<List<BankProcessingOrderForShipmentAndCODDTO>>(shipmentResult);
            }

            if (type == DepositType.Shipment)
            {
                foreach (var item in bankedShipments)
                {
                    total += item.GrandTotal;
                }
            }
            else if (type == DepositType.COD)
            {
                foreach (var item in bankedShipments)
                {
                    total += item.CODAmount;
                    item.Amount = item.CODAmount;
                }
            }
            else if (type == DepositType.Demurrage)
            {
                foreach (var item in bankedShipments)
                {
                    total += item.DemurrageAmount;
                    item.Amount = item.DemurrageAmount;
                }
            }

            var comboresult = Tuple.Create(refcode, bankedShipments, total, bankprcessingresultValue);
            return await Task.FromResult(comboresult);
        }

        //General Search 2
        public async Task<Tuple<string, List<BankProcessingOrderForShipmentAndCODDTO>, decimal, BankProcessingOrderCodesDTO>> SearchBankProcessingOrderV2(string refcode, DepositType type)
        {
            try
            {
                refcode = refcode.Trim();
                var bankprcessingresultValue = await _uow.BankProcessingOrderCodes.GetBankOrderProcessingCodeV2(type, refcode);

                if (bankprcessingresultValue == null)
                {
                    throw new GenericException($"Ref Code {refcode} does not exist.");
                }

                decimal total = bankprcessingresultValue.TotalAmount;

                var serviceCenters = await _userService.GetPriviledgeServiceCenters();
                var accompanyWaybills = await _uow.BankProcessingOrderForShipmentAndCOD.GetAllWaybillsForBankProcessingOrdersV2(type, refcode, serviceCenters);

                if (type == DepositType.COD)
                {
                    foreach (var item in accompanyWaybills)
                    {
                        item.Amount = item.CODAmount;
                    }
                }
                else if (type == DepositType.Demurrage)
                {
                    foreach (var item in accompanyWaybills)
                    {
                        item.Amount = item.DemurrageAmount;
                    }
                }

                var comboresult = Tuple.Create(refcode, accompanyWaybills, total, bankprcessingresultValue);
                return await Task.FromResult(comboresult);

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public async Task<Tuple<string, List<BankProcessingOrderForShipmentAndCODDTO>, decimal, BankProcessingOrderCodesDTO>> SearchBankProcessingOrder3(string _refcode, DepositType type)
        {
            var bankprcessingresult = await _uow.BankProcessingOrderCodes.GetBankOrderProcessingCode(type);
            var bankprcessingresultValue = bankprcessingresult.Where(s => s.Code == _refcode.Trim()).FirstOrDefault();

            //get the start and end date for retrieving of waybills for the bank
            var refcode = _refcode.Trim();

            //Generate the refcode
            var getServiceCenterCode = await _userService.GetCurrentServiceCenter();
            decimal total = 0;

            var serviceCenters = await _userService.GetPriviledgeServiceCenters();
            var accompanyWaybills = await _uow.BankProcessingOrderForShipmentAndCOD.GetAllWaybillsForBankProcessingOrders(type);

            var accompanyWaybillsVals = accompanyWaybills.Where(s => s.RefCode == refcode);

            var bankedShipments = new List<BankProcessingOrderForShipmentAndCODDTO>();

            bankedShipments = accompanyWaybillsVals.OrderByDescending(s => s.DateCreated).ToList();

            if (type == DepositType.Shipment)
            {
                foreach (var item in bankedShipments)
                {
                    total += item.GrandTotal;
                }
            }
            else if (type == DepositType.COD)
            {
                foreach (var item in bankedShipments)
                {
                    total += item.CODAmount;
                }
            }

            else if (type == DepositType.Demurrage)
            {
                foreach (var item in bankedShipments)
                {
                    total += item.DemurrageAmount;
                }
            }

            var comboresult = Tuple.Create(refcode, bankedShipments, total, bankprcessingresultValue);
            return await Task.FromResult(comboresult);
        }

        public async Task<Tuple<decimal, List<InvoiceViewDTO>>> GetTotalAmountAndShipments(DateTime searchdate, DepositType type)
        {
            var enddate = searchdate;

            //Generate the refcode
            decimal total = 0;

            var serviceCenters = await _userService.GetPriviledgeServiceCenters();
            var allShipments = _uow.Invoice.GetAllFromInvoiceAndShipments();
            var userActiveCountryId = await _userService.GetUserActiveCountryId();

            //Get Bank Deposit Module StartDate
            var globalpropertiesdateObj = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.BankDepositModuleStartDate, userActiveCountryId);
            string globalpropertiesdateStr = globalpropertiesdateObj?.Value;

            var globalpropertiesdate = DateTime.MinValue;
            bool success = DateTime.TryParse(globalpropertiesdateStr, out globalpropertiesdate);

            //Filter by deposited code should come here
            allShipments = allShipments.Where(s => s.PaymentMethod == "Cash" && s.PaymentStatus == PaymentStatus.Paid);
            allShipments = allShipments.Where(s => s.DepositStatus == DepositStatus.Unprocessed && s.DateCreated >= globalpropertiesdate);

            var cashShipments = new List<InvoiceViewDTO>();
            if (serviceCenters.Length > 0)
            {
                var shipmentResult = allShipments.Where(s => serviceCenters.Contains(s.DepartureServiceCentreId)).ToList();
                cashShipments = Mapper.Map<List<InvoiceViewDTO>>(shipmentResult);
            }

            foreach (var item in cashShipments)
            {
                total += item.GrandTotal;
            }

            var comboresult = Tuple.Create(total, cashShipments);
            return await Task.FromResult(comboresult);
        }

        public async Task<BankProcessingOrderCodesDTO> AddBankProcessingOrderCodeDemurrageOnly_ScheduleTask(BankProcessingOrderCodesDTO bkoc)
        {
            try
            {
                //1. get the current service user
                bkoc.FullName = "Task Scheduler";

                //2. get the service centers for the current user
                var scs = bkoc.ServiceCenter;
                var userActiveCountryId = 1;

                //3. Get Bank Deposit Module StartDate
                var globalpropertiesdateObj = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.BankDepositModuleStartDate, userActiveCountryId);
                string globalpropertiesdateStr = globalpropertiesdateObj?.Value;

                var globalpropertiesdate = DateTime.MinValue;
                bool success = DateTime.TryParse(globalpropertiesdateStr, out globalpropertiesdate);

                //4. updating the startdate
                bkoc.StartDateTime = globalpropertiesdate;
                bkoc.DateAndTimeOfDeposit = DateTime.Now;
                bkoc.Status = DepositStatus.Pending;

                //5. commence preparatiion to insert records in the BankProcessingOrderForShipmentAndCOD
                var enddate = bkoc.DateAndTimeOfDeposit;

                //1. get data from Demurrage register account as queryable from DemurrageRegisterAccount table
                var allDemurrages = _uow.DemurrageRegisterAccount.GetDemurrageAsQueryable();
                allDemurrages = allDemurrages.Where(s => s.DepositStatus == DepositStatus.Unprocessed && s.PaymentType == PaymentType.Cash);
                allDemurrages = allDemurrages.Where(s => s.DEMStatusHistory == CODStatushistory.RecievedAtServiceCenter && s.Amount>0);

                //all shipments from payload JSON
                var allprocessingordeforshipment = bkoc.ShipmentAndCOD;
                List<string> result = allprocessingordeforshipment?.Select(s => s.Waybill).ToList();

                //--------------------------Validation Section -------------------------------------------//
                var allprocessingordefordemurrage = _uow.BankProcessingOrderForShipmentAndCOD.GetAll().Where(s => s.DepositType == bkoc.DepositType && result.Contains(s.Waybill));
                if (allprocessingordefordemurrage.Any())
                {
                    throw new GenericException("Error validating one or more Demurrages, Please try requesting again for a fresh record.");
                }

                //--------------------------Validation Section -------------------------------------------//

                //2. convert demurrage to list for validation and insert
                var demurrageforservicecenter = allDemurrages.Where(s => result.Contains(s.Waybill)).ToList();

                var bankorderforshipmentandcod = allprocessingordeforshipment.Select(s => new BankProcessingOrderForShipmentAndCOD()
                {
                    Waybill = s.Waybill,
                    RefCode = bkoc.Code,
                    ServiceCenterId = bkoc.ServiceCenter,
                    DemurrageAmount = s.DemurrageAmount,
                    DepositType = bkoc.DepositType,
                    ServiceCenter = bkoc.ScName,
                    Status = DepositStatus.Pending
                });

                var nonDepsitedValueunprocessed = demurrageforservicecenter;

                //Collect total shipment unproceessed and its total
                decimal demurrageTotal = 0;
                foreach (var item in allprocessingordeforshipment)
                {
                    demurrageTotal += item.DemurrageAmount;
                }

                bkoc.TotalAmount = demurrageTotal;
                var bankordercodes = Mapper.Map<BankProcessingOrderCodes>(bkoc);
                nonDepsitedValueunprocessed.ForEach(a => a.DepositStatus = DepositStatus.Pending);
                nonDepsitedValueunprocessed.ForEach(a => a.RefCode = bkoc.Code);

                _uow.BankProcessingOrderCodes.Add(bankordercodes);
                _uow.BankProcessingOrderForShipmentAndCOD.AddRange(bankorderforshipmentandcod);
                await _uow.CompleteAsync();

                return new BankProcessingOrderCodesDTO()
                {
                    CodeId = bankordercodes.CodeId,
                    Code = bankordercodes.Code,
                    DateAndTimeOfDeposit = bankordercodes.DateAndTimeOfDeposit
                };
            }
            catch (Exception)
            {
                throw;
            }
        }

        //Add Bank ProcessingOrder Code DemurrageOnly
        public async Task<BankProcessingOrderCodesDTO> AddBankProcessingOrderCodeDemurrageOnly(BankProcessingOrderCodesDTO bkoc)
        {
            try
            {
                //1. get the current service user
                var user = await _userService.retUser();
                bkoc.UserId = user.Id;
                bkoc.FullName = user.FirstName + " " + user.LastName;

                //2. get the service centers for the current user
                var scs = await _userService.GetCurrentServiceCenter();
                bkoc.ServiceCenter = scs[0].ServiceCentreId;
                bkoc.ScName = scs[0].Name;

                var userActiveCountryId = await _userService.GetUserActiveCountryId();

                //3. Get Bank Deposit Module StartDate
                var globalpropertiesdateObj = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.BankDepositModuleStartDate, userActiveCountryId);
                string globalpropertiesdateStr = globalpropertiesdateObj?.Value;

                var globalpropertiesdate = DateTime.MinValue;
                bool success = DateTime.TryParse(globalpropertiesdateStr, out globalpropertiesdate);

                //4. updating the startdate
                bkoc.StartDateTime = globalpropertiesdate;
                bkoc.DateAndTimeOfDeposit = DateTime.Now;
                bkoc.Status = DepositStatus.Pending;

                //5. commence preparatiion to insert records in the BankProcessingOrderForShipmentAndCOD
                var enddate = bkoc.DateAndTimeOfDeposit;

                var serviceCenters = await _userService.GetPriviledgeServiceCenters();

                //1. get data from Demurrage register account as queryable from DemurrageRegisterAccount table
                var allDemurrages = _uow.DemurrageRegisterAccount.GetDemurrageAsQueryable();

                allDemurrages = allDemurrages.Where(s => s.DepositStatus == DepositStatus.Unprocessed && s.PaymentType == PaymentType.Cash);
                allDemurrages = allDemurrages.Where(s => s.DEMStatusHistory == CODStatushistory.RecievedAtServiceCenter);

                //all shipments from payload JSON
                var allprocessingordeforshipment = bkoc.ShipmentAndCOD;

                var result = allprocessingordeforshipment.Select(s => s.Waybill);

                //--------------------------Validation Section -------------------------------------------//

                var allprocessingordefordemurrage = _uow.BankProcessingOrderForShipmentAndCOD.GetAll().Where(s => s.DepositType == bkoc.DepositType && result.Contains(s.Waybill));

                if (allprocessingordefordemurrage.Any())
                {
                    throw new GenericException("Error validating one or more Demurrages, Please try requesting again for a fresh record.");
                }

                //--------------------------Validation Section -------------------------------------------//

                //2. convert demurrage to list for validation and insert
                var demurrageforservicecenter = allDemurrages.Where(s => result.Contains(s.Waybill)).ToList();

                var bankorderforshipmentandcod = allprocessingordeforshipment.Select(s => new BankProcessingOrderForShipmentAndCOD()
                {
                    Waybill = s.Waybill,
                    RefCode = bkoc.Code,
                    ServiceCenterId = bkoc.ServiceCenter,
                    DemurrageAmount = s.Amount,
                    DepositType = bkoc.DepositType,
                    ServiceCenter = bkoc.ScName,
                    Status = DepositStatus.Pending
                });

                var nonDepsitedValueunprocessed = demurrageforservicecenter;

                //Collect total shipment unproceessed and its total
                decimal demurrageTotal = 0;
                foreach (var item in allprocessingordeforshipment)
                {
                    demurrageTotal += item.Amount;
                }

                bkoc.TotalAmount = demurrageTotal;
                var bankordercodes = Mapper.Map<BankProcessingOrderCodes>(bkoc);
                nonDepsitedValueunprocessed.ForEach(a => a.DepositStatus = DepositStatus.Pending);
                nonDepsitedValueunprocessed.ForEach(a => a.RefCode = bkoc.Code);

                _uow.BankProcessingOrderCodes.Add(bankordercodes);
                _uow.BankProcessingOrderForShipmentAndCOD.AddRange(bankorderforshipmentandcod);

                await _uow.CompleteAsync();

                return new BankProcessingOrderCodesDTO()
                {
                    CodeId = bankordercodes.CodeId,
                    Code = bankordercodes.Code,
                    DateAndTimeOfDeposit = bankordercodes.DateAndTimeOfDeposit
                };
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<BankProcessingOrderCodesDTO> AddBankProcessingOrderCode(BankProcessingOrderCodesDTO bkoc)
        {
            try
            {
                if (bkoc == null)
                {
                    throw new GenericException("NULL INPUT");
                }

                if (!bkoc.ShipmentAndCOD.Any())
                {
                    throw new GenericException("Processing Information Not Found");
                }

                if (string.IsNullOrWhiteSpace(bkoc.Code))
                {
                    throw new GenericException("Ref Code can not be empty");
                }
                else
                {
                    //Validate the code if it follow the code format
                    string getCode = bkoc.Code.Length > 2 ? bkoc.Code.Substring(0, 2) : "0";

                    if (int.TryParse(getCode, out int numCode))
                    {
                        int enumShipment = (int)NumberGeneratorType.BankProcessingOrderForShipment;
                        int enumCod = (int)NumberGeneratorType.BankProcessingOrderForCOD;
                        int enumDemurage = (int)NumberGeneratorType.BankProcessingOrderForDemurrage;

                        if (numCode != enumShipment && numCode != enumCod && numCode != enumDemurage)
                        {
                            throw new GenericException("Ref Code not accepted, Contact IT", $"{(int)HttpStatusCode.Forbidden}");
                        }
                    }
                    else
                    {
                        throw new GenericException("Ref Code not accepted, Contact IT", $"{(int)HttpStatusCode.Forbidden}");
                    }
                }

                //Validate if the code already exist
                var dataExist = await _uow.BankProcessingOrderForShipmentAndCOD.ExistAsync(x => x.RefCode == bkoc.Code);
                if (dataExist)
                {
                    throw new GenericException("Ref Code Already exist, Contact IT", $"{(int)HttpStatusCode.Forbidden}");
                }

                //1. get the current service user
                var user = await _userService.retUser();
                bkoc.UserId = user.Id;
                bkoc.FullName = user.FirstName + " " + user.LastName;

                //2. get the service centers for the current user
                var scs = await _userService.GetCurrentServiceCenter();
                bkoc.ServiceCenter = scs[0].ServiceCentreId;
                bkoc.ScName = scs[0].Name;

                var userActiveCountryId = await _userService.GetUserActiveCountryId();

                //3. Get Bank Deposit Module StartDate
                var globalpropertiesdateObj = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.BankDepositModuleStartDate, userActiveCountryId);
                string globalpropertiesdateStr = globalpropertiesdateObj?.Value;

                var globalpropertiesdate = DateTime.MinValue;
                bool success = DateTime.TryParse(globalpropertiesdateStr, out globalpropertiesdate);

                var bankordercodes = new BankProcessingOrderCodes();

                //4. updating the startdate
                bkoc.StartDateTime = globalpropertiesdate;
                bkoc.DateAndTimeOfDeposit = DateTime.Now;
                bkoc.Status = DepositStatus.Pending;

                //5. commence preparatiion to insert records in the BankProcessingOrderForShipmentAndCOD
                var enddate = bkoc.DateAndTimeOfDeposit;

                if (bkoc.DepositType == DepositType.Shipment)
                {
                    //all shipments from payload JSON
                    var allShipmentsVals = bkoc.ShipmentAndCOD;
                    decimal totalShipment = 0;

                    var result = allShipmentsVals.Select(s => s.Waybill);// new InvoiceViewDTO()
                    foreach (var item in allShipmentsVals)
                    {
                        totalShipment += item.GrandTotal;
                    }

                    //--------------------------Validation Section -------------------------------------------//
                    var allprocessingordeforshipment = _uow.BankProcessingOrderForShipmentAndCOD.GetAll().Where(s => s.DepositType == bkoc.DepositType && result.Contains(s.Waybill));

                    //var validateInsertWaybills = false;
                    if (allprocessingordeforshipment.Any())
                    {
                        throw new GenericException("Error validating one or more waybills, Please try requesting again for a fresh record.");
                    }

                    //--------------------------Validation Section -------------------------------------------//
                    var bankorderforshipmentandcod = allShipmentsVals.Select(s => new BankProcessingOrderForShipmentAndCOD()
                    {
                        Waybill = s.Waybill,
                        RefCode = bkoc.Code,
                        DepositType = bkoc.DepositType,
                        GrandTotal = s.GrandTotal,
                        CODAmount = s.CODAmount,
                        ServiceCenterId = bkoc.ServiceCenter,
                        ServiceCenter = bkoc.ScName,
                        UserId = bkoc.UserId,
                        Status = DepositStatus.Pending
                    });

                    var arrWaybills = allShipmentsVals.Select(x => x.Waybill).ToArray();

                    bankordercodes = Mapper.Map<BankProcessingOrderCodes>(bkoc);
                    bankordercodes.TotalAmount = totalShipment;
                    _uow.BankProcessingOrderCodes.Add(bankordercodes);
                    _uow.BankProcessingOrderForShipmentAndCOD.AddRange(bankorderforshipmentandcod);

                    //select a list of values that contains the allshipment from the invoice view
                    var nonDepsitedValue = _uow.Shipment.GetAll().Where(x => arrWaybills.Contains(x.Waybill)).ToList();
                    var nonDepsitedValueunprocessed = nonDepsitedValue.Where(s => s.DepositStatus == DepositStatus.Unprocessed && s.DateCreated >= globalpropertiesdate).ToList();
                    nonDepsitedValueunprocessed.ForEach(a => a.DepositStatus = DepositStatus.Pending);

                }
                else if (bkoc.DepositType == DepositType.COD)
                {
                    var serviceCenters = await _userService.GetPriviledgeServiceCenters();

                    //--------------------------Validation Section -------------------------------------------//
                    //all shipments from payload JSON
                    var allprocessingordeforshipment = bkoc.ShipmentAndCOD;

                    var result = allprocessingordeforshipment.Select(s => s.Waybill);

                    //--------------------------Validation Section -------------------------------------------//

                    var allprocessingordeforcods = _uow.BankProcessingOrderForShipmentAndCOD.GetAll().Where(s => s.DepositType == bkoc.DepositType && result.Contains(s.Waybill));

                    if (allprocessingordeforcods.Any())
                    {
                        throw new GenericException("Error validating one or more CODs, Please try requesting again for a fresh record.");
                    }

                    //--------------------------Validation Section -------------------------------------------//

                    var bankorderforshipmentandcod = allprocessingordeforshipment.Select(s => new BankProcessingOrderForShipmentAndCOD()
                    {
                        Waybill = s.Waybill,
                        RefCode = bkoc.Code,
                        ServiceCenterId = bkoc.ServiceCenter,
                        CODAmount = s.Amount,
                        DepositType = bkoc.DepositType,
                        ServiceCenter = bkoc.ScName,
                        Status = DepositStatus.Pending
                    });

                    //1. get data from COD register account as queryable from CashOnDeliveryRegisterAccount table
                    var allCODs = _uow.CashOnDeliveryRegisterAccount.GetCODAsQueryable();

                    allCODs = allCODs.Where(s => s.DepositStatus == DepositStatus.Unprocessed && s.PaymentType == PaymentType.Cash);
                    allCODs = allCODs.Where(s => s.CODStatusHistory == CODStatushistory.RecievedAtServiceCenter || s.CODStatusHistory == CODStatushistory.CollectedByDispatch);

                    //select a values from 
                    var nonDepsitedValueunprocessed = allCODs.Where(s => result.Contains(s.Waybill)).ToList();

                    //Collect total shipment unproceessed and its total
                    decimal codTotal = 0;
                    foreach (var item in allprocessingordeforshipment)
                    {
                        codTotal += item.Amount;
                    }

                    bkoc.TotalAmount = codTotal;
                    bankordercodes = Mapper.Map<BankProcessingOrderCodes>(bkoc);
                    nonDepsitedValueunprocessed.ForEach(a => a.DepositStatus = DepositStatus.Pending);
                    nonDepsitedValueunprocessed.ForEach(a => a.RefCode = bkoc.Code);

                    _uow.BankProcessingOrderCodes.Add(bankordercodes);
                    _uow.BankProcessingOrderForShipmentAndCOD.AddRange(bankorderforshipmentandcod);

                }

                await _uow.CompleteAsync();

                return new BankProcessingOrderCodesDTO()
                {
                    CodeId = bankordercodes.CodeId,
                    Code = bankordercodes.Code,
                    DateAndTimeOfDeposit = bankordercodes.DateAndTimeOfDeposit
                };
            }
            catch (Exception)
            {
                throw;
            }
        }

        /// <summary>
        /// Mark shipment bank order processing as deposited
        /// </summary>
        /// <param name="bankrefcode"></param>
        /// <returns></returns>
        public async Task UpdateBankOrderProcessingCode(BankProcessingOrderCodesDTO bankrefcode)
        {
            var bankorder = _uow.BankProcessingOrderCodes.Find(s => s.Code == bankrefcode.Code.Trim()).FirstOrDefault();

            if (bankorder == null)
            {
                throw new GenericException("Bank Order Request Does not Exist!", $"{(int)HttpStatusCode.NotFound}");
            }

            string maxDiff = ConfigurationManager.AppSettings["maxDiffBDO"];
            var decimalVal = Convert.ToDecimal(maxDiff);
            decimal diff = bankorder.TotalAmount - bankrefcode.AmountInputted;

            if (diff > decimalVal)
            {
                var message = new BankDepositMessageDTO()
                {
                    DepositorName = bankorder.FullName,
                    ServiceCenter = bankorder.ScName,
                    TotalAmount = bankorder.TotalAmount,
                    AmountInputted = bankrefcode.AmountInputted,
                };

                await SendMailToAccountants(message);
                throw new GenericException($"Amount Deposited {bankrefcode.AmountInputted} is lower than the Actual Bank Deposit {bankorder.TotalAmount}", $"{(int)HttpStatusCode.Forbidden}");
            }

            else
            {
                //update BankProcessingOrderCodes
                bankorder.Status = DepositStatus.Deposited;
                bankorder.DateAndTimeOfDeposit = DateTime.Now;

                var userActiveCountryId = await _userService.GetUserActiveCountryId();

                //Get Bank Deposit Module StartDate
                var globalpropertiesdateObj = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.BankDepositModuleStartDate, userActiveCountryId);
                string globalpropertiesdateStr = globalpropertiesdateObj?.Value;

                var globalpropertiesdate = DateTime.MinValue;
                bool success = DateTime.TryParse(globalpropertiesdateStr, out globalpropertiesdate);

                var serviceCenters = await _userService.GetCurrentServiceCenter();
                var currentCenter = serviceCenters[0].ServiceCentreId;
                var accompanyWaybills = await _uow.BankProcessingOrderForShipmentAndCOD.GetAllWaybillsForBankProcessingOrdersAsQueryable(bankrefcode.DepositType);

                //update BankProcessingOrderForShipmentAndCOD
                var accompanyWaybillsVals = accompanyWaybills.Where(s => s.RefCode == bankrefcode.Code).ToList();
                accompanyWaybillsVals.ForEach(a => a.Status = DepositStatus.Deposited);

                var arrWaybills = accompanyWaybillsVals.Select(x => x.Waybill).ToArray();

                var nonDepsitedValueQ = _uow.Shipment.GetAll().Where(x => x.DepositStatus == DepositStatus.Pending && x.DepartureServiceCentreId == currentCenter && x.DateCreated >= globalpropertiesdate);
                var nonDepsitedValue = nonDepsitedValueQ.Where(x => arrWaybills.Contains(x.Waybill)).ToList();

                //update Shipment
                nonDepsitedValue.ForEach(a => a.DepositStatus = DepositStatus.Deposited);
                bankorder.BankName = bankrefcode.BankName;
                await _uow.CompleteAsync();
            }
        }

        private async Task<bool> SendMailToAccountants(BankDepositMessageDTO messageDTO)
        {
            //Tell accountants
            //string mailList = ConfigurationManager.AppSettings["accountEmails"];

            var mailList = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.AccountMonitoringEmails, 1);
            string[] emails = mailList.Value.Split(',').ToArray();

            foreach (var email in emails)
            {
                messageDTO.Email = email;
                await _messageSenderService.SendGenericEmailMessage(MessageType.DBDO, messageDTO);
            }
            return true;

        }

        public async Task MarkAsVerified(BankProcessingOrderCodesDTO bankrefcode)
        {
            var bankorder = _uow.BankProcessingOrderCodes.Find(s => s.Code == bankrefcode.Code).FirstOrDefault();

            if (bankorder == null)
            {
                throw new GenericException("Bank Order Request Does not Exist!", $"{(int)HttpStatusCode.NotFound}");
            }

            //update BankProcessingOrderCodes
            bankorder.Status = DepositStatus.Verified;

            //Verifield by
            var user = await _userService.retUser();
            bankorder.UserId = user.Id;
            bankorder.VerifiedBy = user.FirstName + " " + user.LastName;

            var accompanyWaybills = await _uow.BankProcessingOrderForShipmentAndCOD.GetAllWaybillsForBankProcessingOrdersAsQueryable(bankrefcode.DepositType);

            //update BankProcessingOrderForShipmentAndCOD
            var accompanyWaybillsVals = accompanyWaybills.Where(s => s.RefCode == bankrefcode.Code).ToList();
            accompanyWaybillsVals.ForEach(a => a.Status = DepositStatus.Verified);
            accompanyWaybillsVals.ForEach(a => a.VerifiedBy = bankorder.VerifiedBy);

            var arrWaybills = accompanyWaybillsVals.Select(x => x.Waybill).ToArray();

            var nonDepsitedValueQ = _uow.Shipment.GetAll().Where(x => x.DepositStatus == DepositStatus.Deposited);
            var nonDepsitedValue = nonDepsitedValueQ.Where(x => arrWaybills.Contains(x.Waybill)).ToList();

            //update Shipment
            nonDepsitedValue.ForEach(a => a.DepositStatus = DepositStatus.Verified);

            await _uow.CompleteAsync();
        }

        /// <summary>
        /// Mark COD bank order processing as deposited
        /// </summary>
        /// <param name="bankrefcode"></param>
        /// <returns></returns>
        public async Task UpdateBankOrderProcessingCode_demurrage(BankProcessingOrderCodesDTO bankrefcode)
        {
            var bankorder = _uow.BankProcessingOrderCodes.Find(s => s.Code == bankrefcode.Code).FirstOrDefault();

            if (bankorder == null)
            {
                throw new GenericException("Bank Order Request Does not Exist!", $"{(int)HttpStatusCode.NotFound}");
            }

            string maxDiff = ConfigurationManager.AppSettings["maxDiffBDO"];
            var decimalVal = Convert.ToDecimal(maxDiff);
            decimal diff = bankorder.TotalAmount - bankrefcode.AmountInputted;

            if (diff > decimalVal)
            {
                var message = new BankDepositMessageDTO()
                {
                    DepositorName = bankorder.FullName,
                    ServiceCenter = bankorder.ScName,
                    TotalAmount = bankorder.TotalAmount,
                    AmountInputted = bankrefcode.AmountInputted,
                };

                await SendMailToAccountants(message);
                throw new GenericException($"Amount Deposited {bankrefcode.AmountInputted} is lower than the Actual Bank Deposit {bankorder.TotalAmount}", $"{(int)HttpStatusCode.Forbidden}");
            }

            else
            {
                var serviceCenters = await _userService.GetPriviledgeServiceCenters();
                var allDemurrages = _uow.DemurrageRegisterAccount.GetDemurrageAsQueryable();
                allDemurrages = allDemurrages.Where(s => s.DepositStatus == DepositStatus.Pending);
                var codsforservicecenter = allDemurrages.Where(s => serviceCenters.Contains(s.ServiceCenterId)).ToList();

                var accompanyWaybills = await _uow.BankProcessingOrderForShipmentAndCOD.GetAllWaybillsForBankProcessingOrdersAsQueryable(bankrefcode.DepositType);

                //update BankProcessingOrderForShipmentAndCOD
                var accompanyWaybillsVals = accompanyWaybills.Where(s => s.RefCode == bankrefcode.Code).ToList();
                accompanyWaybillsVals.ForEach(a => a.Status = DepositStatus.Deposited);

                codsforservicecenter.ForEach(a => a.DepositStatus = DepositStatus.Deposited);
                bankorder.Status = bankrefcode.Status;
                bankorder.BankName = bankrefcode.BankName;
                bankorder.DateAndTimeOfDeposit = DateTime.Now;
                await _uow.CompleteAsync();
            }
        }

        /// <summary>
        /// Mark COD bank order processing as deposited
        /// </summary>
        /// <param name="bankrefcode"></param>
        /// <returns></returns>
        public async Task UpdateBankOrderProcessingCode_cod(BankProcessingOrderCodesDTO bankrefcode)
        {
            var bankorder = _uow.BankProcessingOrderCodes.Find(s => s.Code == bankrefcode.Code).FirstOrDefault();

            if (bankorder == null)
            {
                throw new GenericException("Bank Order Request Does not Exist!", $"{(int)HttpStatusCode.NotFound}");
            }

            string maxDiff = ConfigurationManager.AppSettings["maxDiffBDO"];
            var decimalVal = Convert.ToDecimal(maxDiff);
            decimal diff = bankorder.TotalAmount - bankrefcode.AmountInputted;

            if (diff > decimalVal)
            {
                var message = new BankDepositMessageDTO()
                {
                    DepositorName = bankorder.FullName,
                    ServiceCenter = bankorder.ScName,
                    TotalAmount = bankorder.TotalAmount,
                    AmountInputted = bankrefcode.AmountInputted,
                };

                await SendMailToAccountants(message);
                throw new GenericException($"Amount Deposited {bankrefcode.AmountInputted} is lower than the Actual Bank Deposit {bankorder.TotalAmount}", $"{(int)HttpStatusCode.Forbidden}");
            }
            else
            {
                var serviceCenters = await _userService.GetPriviledgeServiceCenters();
                var allCODs = _uow.CashOnDeliveryRegisterAccount.GetCODAsQueryable();
                allCODs = allCODs.Where(s => s.DepositStatus == DepositStatus.Pending);
                var codsforservicecenter = allCODs.Where(s => serviceCenters.Contains(s.ServiceCenterId)).ToList();

                var accompanyWaybills = await _uow.BankProcessingOrderForShipmentAndCOD.GetAllWaybillsForBankProcessingOrdersAsQueryable(bankrefcode.DepositType);

                //update BankProcessingOrderForShipmentAndCOD
                var accompanyWaybillsVals = accompanyWaybills.Where(s => s.RefCode == bankrefcode.Code).ToList();
                accompanyWaybillsVals.ForEach(a => a.Status = DepositStatus.Deposited);

                codsforservicecenter.ForEach(a => a.DepositStatus = DepositStatus.Deposited);
                bankorder.Status = bankrefcode.Status;
                bankorder.BankName = bankrefcode.BankName;
                bankorder.DateAndTimeOfDeposit = DateTime.Now;
                await _uow.CompleteAsync();
            }
        }

        public async Task MarkAsVerified_demurrage(BankProcessingOrderCodesDTO bankrefcode)
        {
            var bankorder = _uow.BankProcessingOrderCodes.Find(s => s.Code == bankrefcode.Code).FirstOrDefault();

            if (bankorder == null)
            {
                throw new GenericException("Bank Order Request Does not Exist!", $"{(int)HttpStatusCode.NotFound}");
            }

            //Verifield by
            var user = await _userService.retUser();
            bankorder.UserId = user.Id;
            bankorder.VerifiedBy = user.FirstName + " " + user.LastName;

            var allDemurrages = _uow.DemurrageRegisterAccount.GetDemurrageAsQueryable();
            allDemurrages = allDemurrages.Where(s => s.DepositStatus == DepositStatus.Deposited && s.RefCode == bankrefcode.Code);

            var allDemurragesResult = allDemurrages.ToList();
            var accompanyWaybills = await _uow.BankProcessingOrderForShipmentAndCOD.GetAllWaybillsForBankProcessingOrdersAsQueryable(bankrefcode.DepositType);

            //update BankProcessingOrderForShipmentAndCOD
            var accompanyWaybillsVals = accompanyWaybills.Where(s => s.RefCode == bankrefcode.Code).ToList();
            accompanyWaybillsVals.ForEach(a => a.Status = DepositStatus.Verified);
            accompanyWaybillsVals.ForEach(a => a.VerifiedBy = bankorder.VerifiedBy);

            allDemurragesResult.ForEach(a => a.DepositStatus = DepositStatus.Verified);
            bankorder.Status = bankrefcode.Status;
            await _uow.CompleteAsync();
        }

        public async Task MarkAsVerified_cod(BankProcessingOrderCodesDTO bankrefcode)
        {
            var bankorder = _uow.BankProcessingOrderCodes.Find(s => s.Code == bankrefcode.Code).FirstOrDefault();

            if (bankorder == null)
            {
                throw new GenericException("Bank Order Request Does not Exist!", $"{(int)HttpStatusCode.NotFound}");
            }

            //Verifield by
            var user = await _userService.retUser();
            bankorder.UserId = user.Id;
            bankorder.VerifiedBy = user.FirstName + " " + user.LastName;

            var allCODs = _uow.CashOnDeliveryRegisterAccount.GetCODAsQueryable();
            allCODs = allCODs.Where(s => s.DepositStatus == DepositStatus.Deposited && s.RefCode == bankrefcode.Code);

            var allCODsResult = allCODs.ToList();
            var accompanyWaybills = await _uow.BankProcessingOrderForShipmentAndCOD.GetAllWaybillsForBankProcessingOrdersAsQueryable(bankrefcode.DepositType);

            //update BankProcessingOrderForShipmentAndCOD
            var accompanyWaybillsVals = accompanyWaybills.Where(s => s.RefCode == bankrefcode.Code).ToList();
            accompanyWaybillsVals.ForEach(a => a.Status = DepositStatus.Verified);
            accompanyWaybillsVals.ForEach(a => a.VerifiedBy = bankorder.VerifiedBy);

            allCODsResult.ForEach(a => a.DepositStatus = DepositStatus.Verified);
            bankorder.Status = bankrefcode.Status;
            await _uow.CompleteAsync();
        }

        public async Task UpdateBankProcessingOrderForShipmentAndCOD(BankProcessingOrderForShipmentAndCODDTO refcodeobj)
        {
            var bankorder = await _uow.BankProcessingOrderForShipmentAndCOD.GetAsync(refcodeobj.ProcessingOrderId);
            bankorder.Status = DepositStatus.Deposited;
            await _uow.CompleteAsync();
        }

        //var getServiceCenterCode = await _userService.GetCurrentServiceCenter();
        public async Task<List<BankProcessingOrderCodesDTO>> GetBankOrderProcessingCodeByDate(DepositType type, BankDepositFilterCriteria dateFilterCriteria)
        {
            var result = await _uow.BankProcessingOrderCodes.GetBankOrderProcessingCodeByDate(type, dateFilterCriteria);
            return await Task.FromResult(result);
        }
         
        public async Task<List<BankProcessingOrderCodesDTO>> GetBankOrderProcessingCodeByServiceCenter(DepositType type, BankDepositFilterCriteria dateFilterCriteria)
        {
            var ServiceCenter = await _userService.GetCurrentServiceCenter();
            var result = await _uow.BankProcessingOrderCodes.GetBankOrderProcessingCodeByServiceCenter(type, dateFilterCriteria, ServiceCenter);
            return await Task.FromResult(result);
        }

        public async Task<List<BankProcessingOrderCodesDTO>> GetRegionalBankOrderProcessingCodeByDate(DepositType type, BankDepositFilterCriteria dateFilterCriteria)
        {
            var serviceCenters = await _userService.GetPriviledgeServiceCenters();
            var result = await _uow.BankProcessingOrderCodes.GetBankOrderProcessingCodeByDate(type, dateFilterCriteria, serviceCenters);
            return await Task.FromResult(result);
        }

        //Helps to get bank processing order from bankprocessingorder table
        public async Task<List<BankProcessingOrderForShipmentAndCODDTO>> GetBankProcessingOrderForShipmentAndCOD(DepositType type)
        {
            var result = await _uow.BankProcessingOrderForShipmentAndCOD.GetProcessingOrderForShipmentAndCOD(type);
            return await Task.FromResult(result);
        }

        //New Cod lists for payment
        public async Task<List<NewInvoiceViewDTO>> GetCODCustomersWhoNeedPayOut()
        {
            var result = await _uow.CashOnDeliveryAccount.GetCODCustomersWhoNeedPayOut();
            return await Task.FromResult(result.ToList());
        }

        //Cod paid out list
        public async Task<List<CodPayOutList>> GetPaidOutCODLists()
        {
            var results = await _uow.CashOnDeliveryAccount.GetPaidOutCODListsAsQueryable();
            return await Task.FromResult(results.ToList());
        }

        public async Task<List<CodPayOutList>> GetPaidOutCODListsByCustomer(string customercode)
        {
            var results = await _uow.CashOnDeliveryAccount.GetPaidOutCODListsAsQueryable();
            var resultVals = results.Where(s => s.CustomerCode == customercode);
            return await Task.FromResult(results.ToList());
        }

        public async Task UpdateCODCustomersWhoNeedPayOut(NewInvoiceViewDTO invoiceviewinfo)
        {
            //update shipment table after paid out has been made
            var result = await _uow.CashOnDeliveryAccount.GetShipmentByWaybill(invoiceviewinfo.Waybill);
            result.IsCODPaidOut = true;

            //insert in the cod payout table
            var payoutinfo = new CodPayOutList()
            {
                Waybill = invoiceviewinfo.Waybill,
                TotalAmount = invoiceviewinfo.Total ?? 0,
                DateAndTimeOfDeposit = DateTime.Now,
                UserId = invoiceviewinfo.UserId,
                CustomerCode = invoiceviewinfo.CustomerCode,
                Name = invoiceviewinfo.Name,
                ServiceCenter = invoiceviewinfo.DepartureServiceCentreId,
                ScName = invoiceviewinfo.DepartureServiceCentreName,
                IsCODPaidOut = true,
                VerifiedBy = invoiceviewinfo.UserId
            };

            _uow.CodPayOutList.Add(payoutinfo);

            await _uow.CompleteAsync();
        }

        public Task<IEnumerable<BankDTO>> GetBanks()
        {
            return _bankService.GetBanks();
        }

        public async Task<List<BankProcessingOrderCodesDTO>> GetRegionalAndECOBankOrderProcessingCodeByDate(DepositType type, BankDepositFilterCriteria dateFilterCriteria)
        {
            int[] serviceCenterIds = { };
            var currentUserId = await _userService.GetCurrentUserId();
            var currentUser = await _userService.GetUserById(currentUserId);
            var userRoles = await _userService.GetUserRoles(currentUserId);
            
            if (currentUser.SystemUserRole.Contains("Ecommerce"))
            {
                //get ecommerce hub
                var regionServiceCentreMappingDTOList = await _regionServiceCentreMappingService.GetServiceCentresInRegion(44);
                serviceCenterIds = regionServiceCentreMappingDTOList.Where(s => s.ServiceCentre != null).Select(s => s.ServiceCentre.ServiceCentreId).ToArray();
            }
            else
            {
                serviceCenterIds = await _userService.GetPriviledgeServiceCenters();
            }
            var result = await _uow.BankProcessingOrderCodes.GetBankOrderProcessingCodeByDate(type, dateFilterCriteria, serviceCenterIds);
            return await Task.FromResult(result);
        }


        public async Task MarkMultipleBankProcessingOrderAsVerified(List<BankProcessingOrderCodesDTO> bankrefcodes, string type)
        {
            if (bankrefcodes.Count == 0 || String.IsNullOrEmpty(type))
            {
                throw new GenericException("invalid payload!", $"{(int)HttpStatusCode.BadRequest}");
            }

            if (type.ToLower() == "demurrage")
            {
                foreach (var item in bankrefcodes)
                {
                    await MarkAsVerified_demurrage(item);
                }
            }
            else if (type.ToLower() == "cod")
            {
                foreach (var item in bankrefcodes)
                {
                    await MarkAsVerified_cod(item);
                }
            }
            else if (type.ToLower() == "shipment")
            {
                foreach (var item in bankrefcodes)
                {
                    await MarkAsVerified(item);
                }
            }

        }
    }
}