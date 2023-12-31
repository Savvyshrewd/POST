﻿using POST.Core.IServices;
using POST.Core.DTO.Zone;
using POST.Core.IServices.Zone;
using POST.Services.Implementation;
using System.Threading.Tasks;
using System.Web.Http;
using System.Collections.Generic;
using POST.WebApi.Filters;

namespace POST.WebApi.Controllers.Zone
{
    [Authorize(Roles = "Admin, ViewAdmin")]
    [RoutePrefix("api/weightlimit")]
    public class WeightLimitController : BaseWebApiController
    {
        private readonly IWeightLimitService _weightService;

        public WeightLimitController(IWeightLimitService weightService) : base(nameof(WeightLimitController))
        {
            _weightService = weightService;
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("")]
        public async Task<IServiceResponse<IEnumerable<WeightLimitDTO>>> GetWeightLimit()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var weightLimit = await _weightService.GetWeightLimits();
                return new ServiceResponse<IEnumerable<WeightLimitDTO>>
                {
                    Object = weightLimit
                };
            });
        }
        
        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("{weightLimitId:int}")]
        public async Task<IServiceResponse<WeightLimitDTO>> GetWeightLimitId(int weightLimitId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var weightLimit = await _weightService.GetWeightLimitById(weightLimitId);

                return new ServiceResponse<WeightLimitDTO>
                {
                    Object = weightLimit
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Create")]
        [HttpPost]
        [Route("")]
        public async Task<IServiceResponse<object>> AddWeightLimit(WeightLimitDTO weightLimitDto)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var weightLimit = await _weightService.AddWeightLimit(weightLimitDto);
                return new ServiceResponse<object>
                {
                    Object = weightLimit
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Update")]
        [HttpPut]
        [Route("{weightLimitId:int}")]
        public async Task<IServiceResponse<object>> UpdateWeightLimit(int weightLimitId, WeightLimitDTO weightLimitDto)
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _weightService.UpdateWeightLimit(weightLimitId, weightLimitDto);
                return new ServiceResponse<object>
                {
                    Object = true
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Delete")]
        [HttpDelete]
        [Route("{weightLimitId:int}")]
        public async Task<IServiceResponse<bool>> DeleteWeightLimit(int weightLimitId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _weightService.RemoveWeightLimit(weightLimitId);

                return new ServiceResponse<bool>
                {
                    Object = true
                };
            });
        }
    }
}
