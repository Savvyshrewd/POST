﻿using POST.Core.DTO.OnlinePayment;
using POST.Core.IServices;
using POST.Core.IServices.Wallet;
using POST.Services.Implementation;
using System.Linq;
using System.Threading.Tasks;
using System.Web.Http;

namespace POST.WebApi.Controllers.PaymentWebhook
{
    [RoutePrefix("api/rave")]
    public class FlutterwaveController : BaseWebApiController
    {
        private readonly IFlutterwavePaymentService _service;

        public FlutterwaveController(IFlutterwavePaymentService service) : base(nameof(FlutterwaveController))
        {
            _service = service;
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("validatepayment")]
        public async Task<IServiceResponse<bool>> VerifyAndValidatePayment(FlutterWebhookDTO webhookData)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var response = new ServiceResponse<bool>
                {
                    Object = true
                };
                var request = Request;
                var headers = request.Headers;
                if (headers.Contains("verif-hash"))
                {
                    var key = await _service.GetSecurityKey();
                    string token = headers.GetValues("verif-hash").FirstOrDefault();
                    if (token == key)
                    {
                        await _service.VerifyAndValidatePayment(webhookData);
                    }
                }
                return response;
            });
        }
    }
}
