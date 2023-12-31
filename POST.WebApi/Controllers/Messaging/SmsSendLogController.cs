﻿using POST.Core.DTO.MessagingLog;
using POST.Core.IServices;
using POST.Core.IServices.MessagingLog;
using POST.CORE.DTO.Shipments;
using POST.Services.Implementation;
using POST.WebApi.Filters;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace POST.WebApi.Controllers.Messaging
{
    [Authorize(Roles = "Admin, Shipment, ViewAdmin")]
    [RoutePrefix("api/smssendlog")]
    public class SmsSendLogController : BaseWebApiController
    {
        private readonly ISmsSendLogService _messageService;

        public SmsSendLogController(ISmsSendLogService messageService) : base(nameof(MessageController))
        {
            _messageService = messageService;
        }


        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("")]
        public async Task<IServiceResponse<IEnumerable<SmsSendLogDTO>>> GetSmsSendLogs(MessageFilterOption filter)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var sms = await _messageService.GetSmsSendLogAsync(filter);

                return new ServiceResponse<IEnumerable<SmsSendLogDTO>>
                {
                    Object = sms
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("search")]
        public async Task<IServiceResponse<IEnumerable<SmsSendLogDTO>>> GetSmsSendLogs([FromUri]FilterOptionsDto filterOptionsDto)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var smsTuple = _messageService.GetSmsSendLogAsync(filterOptionsDto);
                return new ServiceResponse<IEnumerable<SmsSendLogDTO>>
                {
                    Object = await smsTuple.Item1,
                    Total = smsTuple.Item2
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("{messageId:int}")]
        public async Task<IServiceResponse<SmsSendLogDTO>> GetSmsSendLog(int messageId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var message = await _messageService.GetSmsSendLogById(messageId);

                return new ServiceResponse<SmsSendLogDTO>
                {
                    Object = message
                };
            });
        }


        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("phonenumber")]
        public async Task<IServiceResponse<List<SmsSendLogDTO>>> GetSmsSendLog([FromUri]string phoneNumber)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var messages = await _messageService.GetSmsSendLog(phoneNumber);

                return new ServiceResponse<List<SmsSendLogDTO>>
                {
                    Object = messages
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("waybill")]
        public async Task<IServiceResponse<List<SmsSendLogDTO>>> GetSmsSendLogs([FromUri]string waybill)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var messages = await _messageService.GetSmsSendLogs(waybill);

                return new ServiceResponse<List<SmsSendLogDTO>>
                {
                    Object = messages
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("deliveryLog/{phoneNumber}")]
        public async Task<IServiceResponse<SmsDeliveryDTO>> GetSmsDeliveryLog(string phoneNumber)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var messages = await _messageService.SmsDeliveryLog(phoneNumber);

                return new ServiceResponse<SmsDeliveryDTO>
                {
                    Object = messages
                };
            });
        }
    }
}
