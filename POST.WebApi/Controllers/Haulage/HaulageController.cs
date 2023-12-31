﻿using POST.Core.IServices;
using POST.Services.Implementation;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using POST.WebApi.Filters;
using POST.Core.DTO.Haulage;

namespace POST.WebApi.Controllers.Haulage
{
    [Authorize(Roles = "Admin, ViewAdmin")]
    [RoutePrefix("api/haulage")]
    public class HaulageController : BaseWebApiController
    {
        private readonly IHaulageService _haulageService;

        public HaulageController(IHaulageService haulageService):base(nameof(HaulageController))
        {
            _haulageService = haulageService;
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("")]
        public async Task<IServiceResponse<IEnumerable<HaulageDTO>>> GetHaulages()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var haulage = await _haulageService.GetHaulages();

                return new ServiceResponse<IEnumerable<HaulageDTO>>
                {
                    Object = haulage
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("active")]
        public async Task<IServiceResponse<IEnumerable<HaulageDTO>>> GetActiveHaulages()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var haulage = await _haulageService.GetActiveHaulages();

                return new ServiceResponse<IEnumerable<HaulageDTO>>
                {
                    Object = haulage
                };
            });
        }
        
        [GIGLSActivityAuthorize(Activity = "Create")]
        [HttpPost]
        [Route("")]
        public async Task<IServiceResponse<object>> AddHaulage(HaulageDTO haulageDto)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var haulage = await _haulageService.AddHaulage(haulageDto);

                return new ServiceResponse<object>
                {
                    Object = haulage
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("{haulageId:int}")]
        public async Task<IServiceResponse<HaulageDTO>> GetHaulage(int haulageId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var haulage = await _haulageService.GetHaulageById(haulageId);

                return new ServiceResponse<HaulageDTO>
                {
                    Object = haulage
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Delete")]
        [HttpDelete]
        [Route("{haulageId:int}")]
        public async Task<IServiceResponse<bool>> DeleteHaulage(int haulageId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _haulageService.RemoveHaulage(haulageId);

                return new ServiceResponse<bool>
                {
                    Object = true
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Update")]
        [HttpPut]
        [Route("{haulageId:int}")]
        public async Task<IServiceResponse<bool>> UpdateHaulage(int haulageId, HaulageDTO haulageDto)
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _haulageService.UpdateHaulage(haulageId, haulageDto);

                return new ServiceResponse<bool>
                {
                    Object = true
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Update")]
        [HttpPut]
        [Route("{haulageId:int}/status/{status}")]
        public async Task<IServiceResponse<bool>> UpdateHaulageStatus(int haulageId, bool status)
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _haulageService.UpdateHaulageStatus(haulageId, status);

                return new ServiceResponse<bool>
                {
                    Object = true
                };

            });
        }
    }
}
