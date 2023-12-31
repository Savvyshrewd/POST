﻿using System;
using System.Threading.Tasks;
using POST.Core;
using GIGL.POST.Core.Domain;
using System.Collections.Generic;
using POST.Core.IServices.Zone;
using POST.Core.DTO.Zone;
using POST.Infrastructure;
using System.Net;

namespace POST.Services.Implementation.Zone
{
    public class DeliveryOptionService : IDeliveryOptionService
    {
        private readonly IUnitOfWork _uow;
        public DeliveryOptionService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<object> AddDeliveryOption(DeliveryOptionDTO option)
        {
            try
            {
                if (await _uow.DeliveryOption.ExistAsync(c => c.Code.ToLower() == option.Code.Trim().ToLower()))
                {
                    throw new GenericException($"Deliver Code {option.Code} already Exist");
                }

                var newOption = new DeliveryOption
                {
                    Code = option.Code,
                    IsActive = false,                    
                    Description = option.Description,
                    CustomerType = option.CustomerType
                    //UserId = option.UserId                 
                    //UserId = logged in user details                    
                };
                _uow.DeliveryOption.Add(newOption);
                await _uow.CompleteAsync();
                return new { Id = newOption.DeliveryOptionId};
            }
            catch (Exception ex)
            {
                throw new GenericException (ex.Message);
            }
        }

        public async Task DeleteDeliveryOption(int optionId)
        {
            try
            {
                var option = await _uow.DeliveryOption.GetAsync(optionId);
                if (option == null)
                {
                    throw new GenericException("Delivery Option Not Exist");
                }
                _uow.DeliveryOption.Remove(option);
                _uow.Complete();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<DeliveryOptionDTO> GetDeliveryOptionById(int optionId)
        {
            try
            {
                var option = await _uow.DeliveryOption.GetAsync(optionId);
                if (option == null)
                {
                    throw new GenericException("Delivery Option Not Exist", $"{(int)HttpStatusCode.NotFound}");
                }
                return new DeliveryOptionDTO
                {
                    DeliveryOptionId = option.DeliveryOptionId,
                    Code = option.Code,
                    Description = option.Description,
                    IsActive = option.IsActive,
                    DateModified = option.DateModified,
                    CustomerType = option.CustomerType                 
                };
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<DeliveryOptionDTO>> GetDeliveryOptions()
        {
            try
            {
                return await _uow.DeliveryOption.GetDeliveryOptions();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<DeliveryOptionDTO>> GetActiveDeliveryOptions()
        {
            try
            {
                return await _uow.DeliveryOption.GetActiveDeliveryOptions();
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task UpdateDeliveryOption(int optionId, DeliveryOptionDTO optionDto)
        {
            try
            {
                var option = await _uow.DeliveryOption.GetAsync(optionId);
                if (option == null || optionDto.DeliveryOptionId != optionId)
                {
                    throw new GenericException("Delivery Option Not Exist");
                }
                option.Description = optionDto.Description;
                option.Code = optionDto.Code;
                option.IsActive = true;
                option.CustomerType = optionDto.CustomerType;
                //UserId = logged in user details  
                _uow.Complete();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task UpdateStatusDeliveryOption(int optionId, bool status)
        {
            try
            {
                var option = await _uow.DeliveryOption.GetAsync(optionId);
                if (option == null)
                {
                    throw new GenericException("Delivery Option Not Exist");
                }
                option.IsActive = status;
                _uow.Complete();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}