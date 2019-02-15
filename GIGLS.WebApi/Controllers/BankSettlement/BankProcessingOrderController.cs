﻿using GIGLS.Core.DTO.Account;
using GIGLS.Core.DTO.BankSettlement;
using GIGLS.Core.DTO.Wallet;
using GIGLS.Core.Enums;
using GIGLS.Core.IServices;
using GIGLS.Core.IServices.BankSettlement;
using GIGLS.Core.IServices.CashOnDeliveryBalance;
using GIGLS.Services.Implementation;
using GIGLS.WebApi.Filters;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace GIGLS.WebApi.Controllers.BankSettlement
{
    [Authorize(Roles = "Account")]
    [RoutePrefix("api/BankProcessingOrderWaybillsandCode")]
    public class BankProcessingOrderController : BaseWebApiController
    {
        private readonly IBankShipmentSettlementService _bankprocessingorder; 

        public BankProcessingOrderController(IBankShipmentSettlementService bankprocessingorder) :base(nameof(BankProcessingOrderController))
        {
            _bankprocessingorder = bankprocessingorder;
        }

        //This one searches for all Shipments recorded: InvoiceView
        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("RequestBankProcessingOrderForShipment")]
        public async Task<IServiceResponse<object>> RequestBankProcessingOrderForShipment(DateTime requestdate, DepositType type)
        {
            return await HandleApiOperationAsync(async () =>
            {
                //All cash shipments from sales
                var bankshipmentprocessingorders = await _bankprocessingorder.GetBankProcessingOrderForShipment(requestdate, type);
                return new ServiceResponse<object>
                {
                    Object = bankshipmentprocessingorders.Item2,
                    Total = bankshipmentprocessingorders.Item3,
                    RefCode = bankshipmentprocessingorders.Item1
                };
            });
        }

        //This one searches for all COD recorded: CashOnDeliveryRegisterAccount
        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("RequestBankProcessingOrderForCOD")]
        public async Task<IServiceResponse<object>> RequestBankProcessingOrderForCOD(DateTime requestdate, DepositType type)
        {
            return await HandleApiOperationAsync(async () =>
            {

                //All cash CODs from sales
                var bankshipmentprocessingorders = await _bankprocessingorder.GetBankProcessingOrderForCOD(requestdate, type);
                return new ServiceResponse<object>
                {
                    Object = bankshipmentprocessingorders.Item2,
                    Total = bankshipmentprocessingorders.Item3,
                    RefCode = bankshipmentprocessingorders.Item1
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("SearchBankOrder")]
        public async Task<IServiceResponse<object>> SearchBankOrder(string refCode, DepositType type) 
        {
            return await HandleApiOperationAsync(async () =>
            {
                //All cash shipments from sales
                var bankprocessingorders = await _bankprocessingorder.SearchBankProcessingOrder(refCode, type);
                return new ServiceResponse<object>
                {
                    Object = bankprocessingorders.Item2,
                    Total = bankprocessingorders.Item3,
                    RefCode = bankprocessingorders.Item1,
                    Shipmentcodref = bankprocessingorders.Item4
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Create")]
        [HttpPost]
        [Route("addbankprocessingorderCode")]
        public async Task<IServiceResponse<object>> AddBankProcessingOrderCode(BankProcessingOrderCodesDTO bkoc)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var bankshipmentprocessingorders = await _bankprocessingorder.AddBankProcessingOrderCode(bkoc);
                return new ServiceResponse<object>
                {
                };
            });
        }


        //[GIGLSActivityAuthorize(Activity = "Create")]
        //[HttpPost]
        //[Route("addbankprocessingOrderforshipmentandcod")]
        //public async Task<IServiceResponse<object>> AddBankProcessingOrderForShipmentAndCOD(BankProcessingOrderForShipmentAndCODDTO bkoc) 
        //{
        //    return await HandleApiOperationAsync(async () =>
        //    {
        //        //var bankshipmentprocessingorders = await _bankprocessingorder.AddBankProcessingOrderForShipmentAndCOD(bkoc);
        //        return new ServiceResponse<object>
        //        {
        //        };
        //    });
        //}

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("getbankprocessingorderForshipmentandcod")]
        public async Task<IServiceResponse<List<BankProcessingOrderForShipmentAndCODDTO>>> GetBankProcessingOrderForShipmentAndCOD(DepositType type)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var resValue = await _bankprocessingorder.GetBankProcessingOrderForShipmentAndCOD(type);
                return new ServiceResponse<List<BankProcessingOrderForShipmentAndCODDTO>>
                {
                    Object = resValue
                };
            });
        }

        //Helps get all processing order by the type: COD or Shipment from 
        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("getbankOrderprocessingcode")]
        public async Task<IServiceResponse<List<BankProcessingOrderCodesDTO>>> GetBankOrderProcessingCode(DepositType type) 
        {
            return await HandleApiOperationAsync(async () =>
            {
                var resValue = await _bankprocessingorder.GetBankOrderProcessingCode(type);
                return new ServiceResponse<List<BankProcessingOrderCodesDTO>>
                {
                    Object = resValue
                };
            });
        }


    }
}
