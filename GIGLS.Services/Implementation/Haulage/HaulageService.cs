﻿using AutoMapper;
using POST.Core;
using POST.Core.IServices;
using POST.Infrastructure;
using System.Collections.Generic;
using System.Threading.Tasks;
using POST.Core.DTO.Haulage;
using System.Net;

namespace POST.Services.Implementation
{
    public class HaulageService : IHaulageService
    {
        private readonly IUnitOfWork _uow;

        public HaulageService(IUnitOfWork uow)
        {
            _uow = uow;
            MapperConfig.Initialize();
        }

        public async Task<IEnumerable<HaulageDTO>> GetHaulages()
        {
            var haulages = await _uow.Haulage.GetHaulagesAsync();
            return haulages;
        }

        public async Task<IEnumerable<HaulageDTO>> GetActiveHaulages()
        {
            var haulages = await _uow.Haulage.GetAsync(x => x.Status == true);
            return Mapper.Map<IEnumerable<HaulageDTO>>(haulages);
        }

        public async Task<HaulageDTO> GetHaulageById(int haulageId)
        {
            var haulage = await _uow.Haulage.GetAsync(haulageId);

            if (haulage == null)
            {
                throw new GenericException("HAULAGE INFORMATION DOES NOT EXIST", $"{(int)HttpStatusCode.NotFound}");
            }
            return Mapper.Map<HaulageDTO>(haulage);
        }

        public async Task<object> AddHaulage(HaulageDTO haulageDto)
        {
            if (await _uow.Haulage.ExistAsync(v => v.Tonne == haulageDto.Tonne))
            {
                throw new GenericException($"{haulageDto.Tonne} Tonne already exist", $"{(int)HttpStatusCode.Forbidden}");
            }

            var newHaulage = new Core.Domain.Haulage
            {
                Tonne = haulageDto.Tonne,
                Status = true,
                FixedRate = haulageDto.FixedRate,
                AdditionalRate = haulageDto.AdditionalRate,
                Description = haulageDto.Description
            };

            _uow.Haulage.Add(newHaulage);
            await _uow.CompleteAsync();
            return new { id = newHaulage.HaulageId };
        }

        public async Task UpdateHaulage(int haulageId, HaulageDTO haulageDto)
        {
            var haulage = await _uow.Haulage.GetAsync(haulageId);

            if (haulage == null)
            {
                throw new GenericException("HAULAGE INFORMATION DOES NOT EXIST");
            }

            haulage.Tonne = haulageDto.Tonne;
            haulage.Status = haulageDto.Status;
            haulage.FixedRate = haulageDto.FixedRate;
            haulage.AdditionalRate = haulageDto.AdditionalRate;
            haulage.Description = haulageDto.Description;
            await _uow.CompleteAsync();
        }

        public async Task UpdateHaulageStatus(int haulageId, bool status)
        {
            var haulage = await _uow.Haulage.GetAsync(haulageId);

            if (haulage == null)
            {
                throw new GenericException("HAULAGE INFORMATION DOES NOT EXIST");
            }
            
            haulage.Status = status;
            await _uow.CompleteAsync();
        }

        public async Task RemoveHaulage(int haulageId)
        {
            var haulage = await _uow.Haulage.GetAsync(haulageId);

            if (haulage == null)
            {
                throw new GenericException("HAULAGE INFORMATION DOES NOT EXIST");
            }
            _uow.Haulage.Remove(haulage);
            await _uow.CompleteAsync();
        }        
    }
}