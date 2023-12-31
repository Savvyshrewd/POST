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
    [RoutePrefix("api/emailsendlog")]
    public class EmailSendLogController : BaseWebApiController
    {
        private readonly IEmailSendLogService _messageService;

        public EmailSendLogController(IEmailSendLogService messageService) : base(nameof(EmailSendLogController))
        {
            _messageService = messageService;
        }
        
        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("")]
        public async Task<IServiceResponse<IEnumerable<EmailSendLogDTO>>> GetEmailSendLogs(MessageFilterOption filter)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var email = await _messageService.GetEmailSendLogAsync(filter);

                return new ServiceResponse<IEnumerable<EmailSendLogDTO>>
                {
                    Object = email
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("search")]
        public async Task<IServiceResponse<IEnumerable<EmailSendLogDTO>>> GetEmailSendLogs([FromUri]FilterOptionsDto filterOptionsDto)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var smsTuple = await _messageService.GetEmailSendLogAsync(filterOptionsDto);
                return new ServiceResponse<IEnumerable<EmailSendLogDTO>>
                {
                    Object = smsTuple.Item1,
                    Total = smsTuple.Item2
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("{messageId:int}")]
        public async Task<IServiceResponse<EmailSendLogDTO>> GetEmailSendLog(int messageId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var message = await _messageService.GetEmailSendLogById(messageId);

                return new ServiceResponse<EmailSendLogDTO>
                {
                    Object = message
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("{email}/email")]
        public async Task<IServiceResponse<List<EmailSendLogDTO>>> GetEmailSendLog([FromUri]string email)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var messages = await _messageService.GetEmailSendLog(email);

                return new ServiceResponse<List<EmailSendLogDTO>>
                {
                    Object = messages
                };
            });
        }
    }
}