﻿using POST.Core.IServices.MessagingLog;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using POST.Core.DTO.MessagingLog;
using POST.Core;
using POST.Core.Domain.MessagingLog;
using AutoMapper;
using POST.Infrastructure;
using POST.Core.IServices.User;
using POST.CORE.DTO.Shipments;
using System.Linq;

namespace POST.Services.Implementation.Messaging
{
    public class EmailSendLogService : IEmailSendLogService
    {
        private readonly IUserService _userService;
        private readonly IUnitOfWork _uow;

        public EmailSendLogService(IUserService userService, IUnitOfWork uow)
        {
            _userService = userService;
            _uow = uow;
            MapperConfig.Initialize();
        }
        public async Task<object> AddEmailSendLog(EmailSendLogDTO messageDto)
        {
            var message = Mapper.Map<EmailSendLog>(messageDto);
            if (message.User == null)
            {
                message.User = await _userService.GetCurrentUserId();
            }
            _uow.EmailSendLog.Add(message);
            await _uow.CompleteAsync();
            return new { id = message.EmailSendLogId };
        }

        public Task<List<EmailSendLogDTO>> GetEmailSendLogAsync(MessageFilterOption filter)
        {
            var messages = _uow.EmailSendLog.GetEmailSendLogsAsync(filter);
            return messages;
        }

        public async Task<Tuple<List<EmailSendLogDTO>, int>> GetEmailSendLogAsync(FilterOptionsDto filterOptionsDto)
        {
            try
            {
                var emailCollection = await _uow.EmailSendLog.FindAsync(s => s.IsDeleted == false);
                var emailCollectionDto = Mapper.Map<IEnumerable<EmailSendLogDTO>>(emailCollection);
                emailCollectionDto = emailCollectionDto.OrderByDescending(x => x.DateCreated);

                var count = emailCollectionDto.Count(); 

                if (filterOptionsDto != null)
                {
                    //filter
                    var filter = filterOptionsDto.filter;
                    var filterValue = filterOptionsDto.filterValue;
                    if (!string.IsNullOrEmpty(filter) && !string.IsNullOrEmpty(filterValue))
                    {
                        emailCollectionDto = emailCollectionDto.Where(s => (s.GetType().GetProperty(filter).GetValue(s)) != null 
                            && (s.GetType().GetProperty(filter).GetValue(s)).ToString().Contains(filterValue)).ToList();
                    }

                    //sort
                    var sortorder = filterOptionsDto.sortorder;
                    var sortvalue = filterOptionsDto.sortvalue;

                    if (!string.IsNullOrEmpty(sortorder) && !string.IsNullOrEmpty(sortvalue))
                    {
                        System.Reflection.PropertyInfo prop = typeof(EmailSendLog).GetProperty(sortvalue);

                        if (sortorder == "0")
                        {
                            emailCollectionDto = emailCollectionDto.OrderBy(x => x.GetType().GetProperty(prop.Name).GetValue(x)).ToList();
                        }
                        else
                        {
                            emailCollectionDto = emailCollectionDto.OrderByDescending(x => x.GetType().GetProperty(prop.Name).GetValue(x)).ToList();
                        }
                    }

                    emailCollectionDto = emailCollectionDto.Skip(filterOptionsDto.count * (filterOptionsDto.page - 1)).Take(filterOptionsDto.count).ToList();
                }

                return new Tuple<List<EmailSendLogDTO>, int>(emailCollectionDto.ToList(), count);

            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<EmailSendLogDTO> GetEmailSendLogById(int messageId)
        {
            var message = await _uow.EmailSendLog.GetAsync(messageId);

            if (message == null)
            {
                throw new GenericException("MESSAGE INFORMATION DOES NOT EXIST");
            }
            return Mapper.Map<EmailSendLogDTO>(message);
        }

        public async Task<List<EmailSendLogDTO>> GetEmailSendLog(string email)
        {
            var message = _uow.EmailSendLog.GetAllAsQueryable().Where(x => x.To == email);
            return Mapper.Map<List<EmailSendLogDTO>>(message.OrderByDescending(x => x.DateCreated));
        }

        public async Task RemoveEmailSendLog(int messageId)
        {
            var message = await _uow.EmailSendLog.GetAsync(messageId);

            if (message == null)
            {
                throw new GenericException("MESSAGE INFORMATION DOES NOT EXIST");
            }
            _uow.EmailSendLog.Remove(message);
            await _uow.CompleteAsync();
        }

        public async Task UpdateEmailSendLog(int messageId, EmailSendLogDTO messageDto)
        {
            var message = await _uow.EmailSendLog.GetAsync(messageId);

            if (message == null)
            {
                throw new GenericException("MESSAGE INFORMATION DOES NOT EXIST");
            }
            
            message.From = messageDto.From;
            message.To = messageDto.To;
            message.Status = messageDto.Status;
            message.Message = messageDto.Message;
            message.User = messageDto.User;
            message.ResultStatus = messageDto.ResultStatus;
            message.ResultDescription = messageDto.ResultDescription;            
            await _uow.CompleteAsync();
        }
    }
}
