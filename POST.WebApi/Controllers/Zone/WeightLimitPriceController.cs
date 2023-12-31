﻿using POST.Core.IServices;
using POST.Core.DTO.Zone;
using POST.Core.IServices.Zone;
using POST.Services.Implementation;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using POST.WebApi.Filters;

namespace POST.WebApi.Controllers.Zone
{
    [Authorize(Roles = "Admin, ViewAdmin")]
    [RoutePrefix("api/weightlimitprice")]
    public class WeightLimitPriceController : BaseWebApiController
    {
        private readonly IWeightLimitPriceService _weightService;

        public WeightLimitPriceController(IWeightLimitPriceService weightService) : base(nameof(WeightLimitPriceController))
        {
            _weightService = weightService;
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("")]
        public async Task<IServiceResponse<IEnumerable<WeightLimitPriceDTO>>> GetWeightLimitPrice()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var weightLimit = await _weightService.GetWeightLimitPrices();
                return new ServiceResponse<IEnumerable<WeightLimitPriceDTO>>
                {
                    Object = weightLimit
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("{weightLimitPriceId:int}")]
        public async Task<IServiceResponse<WeightLimitPriceDTO>> GetWeightLimitPriceId(int weightLimitPriceId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var weightLimit = await _weightService.GetWeightLimitPriceById(weightLimitPriceId);

                return new ServiceResponse<WeightLimitPriceDTO>
                {
                    Object = weightLimit
                };
            });
        }

        //[GIGLSActivityAuthorize(Activity = "View")]
        //[HttpGet]
        //[Route("{zoneId:int}")]
        //public async Task<IServiceResponse<WeightLimitPriceDTO>> GetWeightLimitPriceByZoneId(int zoneId)
        //{
        //    return await HandleApiOperationAsync(async () =>
        //    {
        //        var weightLimit = await _weightService.GetWeightLimitPriceByZoneId(zoneId);

        //        return new ServiceResponse<WeightLimitPriceDTO>
        //        {
        //            Object = weightLimit
        //        };
        //    });
        //}

        [GIGLSActivityAuthorize(Activity = "Create")]
        [HttpPost]
        [Route("")]
        public async Task<IServiceResponse<object>> AddWeightLimitPrice(WeightLimitPriceDTO weightLimitDto)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var weightLimit = await _weightService.AddWeightLimitPrice(weightLimitDto);
                return new ServiceResponse<object>
                {
                    Object = weightLimit
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Update")]
        [HttpPut]
        [Route("{weightLimitPriceId:int}")]
        public async Task<IServiceResponse<object>> UpdateWeightLimitPrice(int weightLimitPriceId, WeightLimitPriceDTO weightLimitDto)
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _weightService.UpdateWeightLimitPrice(weightLimitPriceId, weightLimitDto);
                return new ServiceResponse<object>
                {
                    Object = true
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Delete")]
        [HttpDelete]
        [Route("{weightLimitPriceId:int}")]
        public async Task<IServiceResponse<bool>> DeleteWeightLimitPrice(int weightLimitPriceId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _weightService.RemoveWeightLimitPrice(weightLimitPriceId);

                return new ServiceResponse<bool>
                {
                    Object = true
                };
            });
        }

    }
}
