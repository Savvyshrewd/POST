﻿using POST.Core.DTO.Wallet;
using POST.Core.IServices;
using POST.Core.IServices.CashOnDeliveryBalance;
using POST.Services.Implementation;
using POST.WebApi.Filters;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace POST.WebApi.Controllers.CashOnDeliveryBalance
{
    [Authorize(Roles = "Account")]
    [RoutePrefix("api/cashondeliverybalance")]
    public class CashOnDeliveryBalanceController : BaseWebApiController
    {
        private readonly ICashOnDeliveryBalanceService _cashOnDeliveryBalanceService;

        public CashOnDeliveryBalanceController(ICashOnDeliveryBalanceService cashOnDeliveryBalanceService) :base(nameof(CashOnDeliveryBalanceController))
        {
            _cashOnDeliveryBalanceService = cashOnDeliveryBalanceService;
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("")]
        public async Task<IServiceResponse<IEnumerable<CashOnDeliveryBalanceDTO>>> GetCashOnDeliveryBalances()
        {
            return await HandleApiOperationAsync(async () =>
            {
                //var cashOnDeliveryBalances = await _cashOnDeliveryBalanceService.GetCashOnDeliveryBalances();
                var cashOnDeliveryBalances = await _cashOnDeliveryBalanceService.GetUnprocessedCashOnDeliveryPaymentSheet();
                return new ServiceResponse<IEnumerable<CashOnDeliveryBalanceDTO>>
                {
                    Object = cashOnDeliveryBalances
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("{cashOnDeliveryBalanceId:int}")]
        public async Task<IServiceResponse<CashOnDeliveryBalanceDTO>> GetCashOnDeliveryBalanceById(int cashOnDeliveryBalanceId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _cashOnDeliveryBalanceService.GetCashOnDeliveryBalanceById(cashOnDeliveryBalanceId);

                return new ServiceResponse<CashOnDeliveryBalanceDTO>
                {
                    Object = result
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("{walletNumber}/wallet")]
        public async Task<IServiceResponse<CashOnDeliveryBalanceDTO>> GetCashOnDeliveryBalanceByWallet(string walletNumber)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _cashOnDeliveryBalanceService.GetCashOnDeliveryBalanceByWallet(walletNumber);

                return new ServiceResponse<CashOnDeliveryBalanceDTO>
                {
                    Object = result
                };
            });
        }

        //COD Payment Sheet
        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("paymentsheet")]
        public async Task<IServiceResponse<IEnumerable<CashOnDeliveryBalanceDTO>>> GetCashOnDeliveryPaymentSheet()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var cashOnDeliveryBalances = await _cashOnDeliveryBalanceService.GetUnprocessedCashOnDeliveryPaymentSheet();
                return new ServiceResponse<IEnumerable<CashOnDeliveryBalanceDTO>>
                {
                    Object = cashOnDeliveryBalances
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("pendingpaymentsheet")]
        public async Task<IServiceResponse<IEnumerable<CashOnDeliveryBalanceDTO>>> GetPendingCashOnDeliveryPaymentSheet()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var cashOnDeliveryBalances = await _cashOnDeliveryBalanceService.GetPendingCashOnDeliveryPaymentSheet();
                return new ServiceResponse<IEnumerable<CashOnDeliveryBalanceDTO>>
                {
                    Object = cashOnDeliveryBalances
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("processedpaymentsheet")]
        public async Task<IServiceResponse<IEnumerable<CashOnDeliveryBalanceDTO>>> GetProcessedCashOnDeliveryPaymentSheet()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var cashOnDeliveryBalances = await _cashOnDeliveryBalanceService.GetProcessedCashOnDeliveryPaymentSheet();
                return new ServiceResponse<IEnumerable<CashOnDeliveryBalanceDTO>>
                {
                    Object = cashOnDeliveryBalances
                };
            });
        }

    }
}
