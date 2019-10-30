﻿using GIGLS.Core.DTO.OnlinePayment;
using GIGLS.Core.DTO.Wallet;
using GIGLS.Core.IServices;
using GIGLS.Core.IServices.Wallet;
using GIGLS.Services.Implementation;
using GIGLS.WebApi.Filters;
using System.Threading.Tasks;
using System.Web.Http;

namespace GIGLS.WebApi.Controllers.Wallet
{
    [Authorize(Roles = "Account, Shipment")]
    [RoutePrefix("api/waybillpaymentlog")]
    public class WaybillPaymentLogController : BaseWebApiController
    {
        private readonly IWaybillPaymentLogService _waybillPaymentLogService;

        public WaybillPaymentLogController(IWaybillPaymentLogService waybillPaymentLogService) : base(nameof(WaybillPaymentLogController))
        {
            _waybillPaymentLogService = waybillPaymentLogService;
        }


        [GIGLSActivityAuthorize(Activity = "Create")]
        [HttpPost]
        [Route("")]
        public async Task<IServiceResponse<PaymentInitiate>> AddWaybillPaymentLog(WaybillPaymentLogDTO waybillPaymentLogDTO)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var waybillPayment = await _waybillPaymentLogService.AddWaybillPaymentLog(waybillPaymentLogDTO);

                return new ServiceResponse<PaymentInitiate>
                {
                    Object = waybillPayment
                };
            });
        }
    }
}