﻿using POST.Core.IServices.Zone;
using System.Collections.Generic;
using System.Threading.Tasks;
using POST.Core.DTO.Zone;
using POST.Core;
using POST.Infrastructure;
using AutoMapper;
using POST.Core.Domain;
using POST.Core.Enums;
using System.Linq;
using POST.Core.DTO;
using System.Net;

namespace POST.Services.Implementation.Zone
{
    public class WeightLimitPriceService : IWeightLimitPriceService
    {
        private readonly IUnitOfWork _uow;

        public WeightLimitPriceService(IUnitOfWork uow)
        {
            _uow = uow;
            MapperConfig.Initialize();
        }

        public async Task<object> AddWeightLimitPrice(WeightLimitPriceDTO weightLimitPrice)
        {
            if (weightLimitPrice == null)
                throw new GenericException("NULL_INPUT");
            var weightExist = await _uow.WeightLimitPrice.ExistAsync(x => x.ZoneId == weightLimitPrice.ZoneId && x.Weight.Equals(weightLimitPrice.Weight) 
            && x.RegularEcommerceType == weightLimitPrice.RegularEcommerceType && x.CountryId == weightLimitPrice.CountryId);

            if (weightExist == true)
                throw new GenericException("Weight limit Price already exist");

            var weight = new WeightLimitPrice
            {
                Weight = weightLimitPrice.Weight,
                ZoneId = weightLimitPrice.ZoneId,
                Price = weightLimitPrice.Price,
                RegularEcommerceType = weightLimitPrice.RegularEcommerceType,
                CountryId = weightLimitPrice.CountryId
            };
            _uow.WeightLimitPrice.Add(weight);
            await _uow.CompleteAsync();
            return new { Id = weight.WeightLimitPriceId };
        }

        public async Task<WeightLimitPriceDTO> GetWeightLimitPriceById(int weightLimitPriceId)
        {
            var weight = await _uow.WeightLimitPrice.GetAsync(x => x.WeightLimitPriceId == weightLimitPriceId);
            if (weight == null)
            {
                throw new GenericException("Weight limit price does not exist");
            }

            return Mapper.Map<WeightLimitPriceDTO>(weight);
        }

        public async Task<WeightLimitPriceDTO> GetWeightLimitPriceByZoneId(int zoneId, int countryId)
        {
            var weight = await _uow.WeightLimitPrice.GetAsync(x => x.ZoneId == zoneId && x.CountryId == countryId);
            if (weight == null)
            {
                throw new GenericException("Weight limit price does not exist");
            }

            return Mapper.Map<WeightLimitPriceDTO>(weight);
        }

        public async Task<WeightLimitPriceDTO> GetWeightLimitPriceByZoneId(int zoneId, RegularEcommerceType regularEcommerceType, int countryId)
        {
            var weight = await _uow.WeightLimitPrice.GetAsync(x => x.ZoneId == zoneId 
                && x.RegularEcommerceType == regularEcommerceType && x.CountryId == countryId);
            if (weight == null)
            {
                throw new GenericException("Weight limit price does not exist", $"{(int)HttpStatusCode.NotFound}");
            }

            return Mapper.Map<WeightLimitPriceDTO>(weight);
        }

        public async Task<List<WeightLimitPriceDTO>> GetWeightLimitPrices()
        {
            var weightLimitPrices = await _uow.WeightLimitPrice.GetWeightLimitPrices();

            var countries = _uow.Country.GetAll().ToList();
            var countriesDTO = Mapper.Map<IEnumerable<CountryDTO>>(countries);

            foreach (var weightLimitPrice in weightLimitPrices)
            {
                var country = countriesDTO.FirstOrDefault(s => s.CountryId == weightLimitPrice.CountryId);
                weightLimitPrice.Country = country;
            }


            return weightLimitPrices;
        }

        public async Task RemoveWeightLimitPrice(int weightLimitId)
        {
            var limit = await _uow.WeightLimitPrice.GetAsync(weightLimitId);

            if (limit == null)
            {
                throw new GenericException("Weight limit price does not exist");
            }
            _uow.WeightLimitPrice.Remove(limit);
            await _uow.CompleteAsync();
        }

        public async Task UpdateWeightLimitPrice(int weightLimitId, WeightLimitPriceDTO weightLimitPriceDto)
        {
            if (weightLimitPriceDto == null)
                throw new GenericException("NULL_INPUT");

            var weight = _uow.WeightLimitPrice.Get(weightLimitId);

            if (weight == null)
                throw new GenericException("Weight limit price does not exist");

            weight.Weight = weightLimitPriceDto.Weight;
            weight.Price = weightLimitPriceDto.Price;
            weight.ZoneId = weightLimitPriceDto.ZoneId;
            weight.RegularEcommerceType = weightLimitPriceDto.RegularEcommerceType;
            weight.CountryId = weightLimitPriceDto.CountryId;
            await _uow.CompleteAsync();
        }
    }
}
