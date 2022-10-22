﻿using AutoMapper;
using POST.Core;
using POST.Core.Domain.Partnership;
using POST.Core.DTO.Partnership;
using POST.Core.IServices.Partnership;
using System;
using System.Threading.Tasks;

namespace POST.Services.Implementation.Partnership
{
    public class PartnerPayoutService : IPartnerPayoutService
    {
        private readonly IUnitOfWork _uow;

        public PartnerPayoutService(IUnitOfWork uow)
        {
            _uow = uow;
            MapperConfig.Initialize();
        }

        public async Task<object> AddPartnerPayout(PartnerPayoutDTO partnerPayoutDTO)
        {
            var newPartnerPayout = Mapper.Map<PartnerPayout>(partnerPayoutDTO);
            newPartnerPayout.DateProcessed = DateTime.Now;

            _uow.PartnerPayout.Add(newPartnerPayout);
            await _uow.CompleteAsync();
            return new { id = newPartnerPayout.PartnerPayoutId };
        }
    }
}
