﻿using POST.Core.DTO.Wallet;
using POST.Core.Enums;
using POST.Core.IServices;
using POST.Core.IServices.CashOnDeliveryAccount;
using POST.Services.Implementation;
using POST.WebApi.Filters;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace POST.WebApi.Controllers.Wallet
{
    [Authorize(Roles = "Account")]
    [RoutePrefix("api/cashondeliveryaccount")]
    public class CashOnDeliveryAccountController : BaseWebApiController
    {
        private readonly ICashOnDeliveryAccountService _cashOnDeliveryAccountService;

        public CashOnDeliveryAccountController(ICashOnDeliveryAccountService cashOnDeliveryAccountService) :base(nameof(CashOnDeliveryAccountController))
        {
            _cashOnDeliveryAccountService = cashOnDeliveryAccountService;
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("")]
        public async Task<IServiceResponse<IEnumerable<CashOnDeliveryAccountDTO>>> GetCashOnDeliveryAccounts()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var cashOnDeliveryAccounts = await _cashOnDeliveryAccountService.GetCashOnDeliveryAccounts();
                return new ServiceResponse<IEnumerable<CashOnDeliveryAccountDTO>>
                {
                    Object = cashOnDeliveryAccounts
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("{cashOnDeliveryAccountId:int}")]
        public async Task<IServiceResponse<CashOnDeliveryAccountDTO>> GetCashOnDeliveryAccountById(int cashOnDeliveryAccountId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _cashOnDeliveryAccountService.GetCashOnDeliveryAccountById(cashOnDeliveryAccountId);

                return new ServiceResponse<CashOnDeliveryAccountDTO>
                {
                    Object = result
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("{walletNumber}/summary")]
        public async Task<IServiceResponse<CashOnDeliveryAccountSummaryDTO>> GetCashOnDeliveryAccountByWallet(string walletNumber)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _cashOnDeliveryAccountService.GetCashOnDeliveryAccountByWallet(walletNumber);

                return new ServiceResponse<CashOnDeliveryAccountSummaryDTO>
                {
                    Object = result
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Create")]
        [HttpPost]
        [Route("")]
        public async Task<IServiceResponse<object>> AddCashOnDeliveryAccount(CashOnDeliveryAccountDTO newCashOnDeliveryAccount)
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _cashOnDeliveryAccountService.AddCashOnDeliveryAccount(newCashOnDeliveryAccount);
                return new ServiceResponse<object>
                {
                    Object = true
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Update")]
        [HttpPut]
        [Route("{cashOnDeliveryAccountId:int}")]
        public async Task<IServiceResponse<object>> UpdateCashOnDeliveryAccount(int cashOnDeliveryAccountId, CashOnDeliveryAccountDTO cashOnDeliveryAccountDTO)
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _cashOnDeliveryAccountService.UpdateCashOnDeliveryAccount(cashOnDeliveryAccountId, cashOnDeliveryAccountDTO);
                return new ServiceResponse<object>
                {
                    Object = true
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Create")]
        [HttpPost]
        [Route("processToPending")]
        public async Task<IServiceResponse<bool>> ProcessToPending(List<CashOnDeliveryBalanceDTO> data)
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _cashOnDeliveryAccountService.ProcessToPending(data);
                return new ServiceResponse<bool>
                {
                    Object = true
                };
            });
        }
        
        [GIGLSActivityAuthorize(Activity = "Create")]
        [HttpPost]
        [Route("processpaymentsheet")]
        public async Task<IServiceResponse<bool>> ProcessCashOnDeliveryPaymentSheet(List<CashOnDeliveryBalanceDTO> data)
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _cashOnDeliveryAccountService.ProcessCashOnDeliveryPaymentSheet(data);
                return new ServiceResponse<bool>
                {
                    Object = true
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("{walletNumber}/statussummary/{status}")]
        public async Task<IServiceResponse<CashOnDeliveryAccountSummaryDTO>> GetCashOnDeliveryAccountByStatus(string walletNumber, CODStatus status)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _cashOnDeliveryAccountService.GetCashOnDeliveryAccountByStatus(walletNumber, status);

                return new ServiceResponse<CashOnDeliveryAccountSummaryDTO>
                {
                    Object = result
                };
            });
        }

    }
}
