﻿using POST.Core.DTO.MessagingLog;
using POST.CORE.DTO.Shipments;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace POST.Core.IServices.MessagingLog
{
    public interface ISmsSendLogService : IServiceDependencyMarker
    {
        Task<List<SmsSendLogDTO>> GetSmsSendLogAsync(MessageFilterOption filter);
        System.Tuple<Task<List<SmsSendLogDTO>>, int> GetSmsSendLogAsync(FilterOptionsDto filterOptionsDto);
        Task<SmsSendLogDTO> GetSmsSendLogById(int messageId);
        Task<object> AddSmsSendLog(SmsSendLogDTO messageDto);
        Task UpdateSmsSendLog(int messageId, SmsSendLogDTO messageDto);
        Task RemoveSmsSendLog(int messageId);
        Task<List<SmsSendLogDTO>> GetSmsSendLog(string phoneNumber);
        Task<List<SmsSendLogDTO>> GetSmsSendLogs(string waybill);
        Task<SmsDeliveryDTO> SmsDeliveryLog(string phonenumber);
    }
}
