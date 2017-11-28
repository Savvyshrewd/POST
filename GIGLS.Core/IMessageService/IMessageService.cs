﻿using GIGLS.Core.Enums;
using GIGLS.Core.IServices;
using System.Threading.Tasks;

namespace GIGLS.Core.IMessageService
{
    public interface IMessageSenderService : IServiceDependencyMarker
    {
        Task<bool> SendMessage(MessageType messageType, EmailSmsType emailSmsType);
    }
}