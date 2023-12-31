﻿using POST.Core.DTO.Utility;
using POST.Core.IServices;
using POST.Core.IServices.User;
using POST.Core.IServices.Utility;
using POST.Services.Implementation;
using POST.WebApi.Filters;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace POST.WebApi.Controllers.Utility
{

    [Authorize(Roles = "Admin, ViewAdmin")]
    [RoutePrefix("api/globalproperty")]
    public class GlobalPropertyController : BaseWebApiController
    {

        private readonly IGlobalPropertyService _globalService;
        private readonly IUserService _userService;

        public GlobalPropertyController(IGlobalPropertyService globalService, IUserService userService) :base(nameof(GlobalPropertyController))
        {
            _globalService = globalService;
            _userService = userService;
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("")]
        public async Task<IServiceResponse<IEnumerable<GlobalPropertyDTO>>> GetGlobalProperties()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var globals = await _globalService.GetGlobalProperties();
                return new ServiceResponse<IEnumerable<GlobalPropertyDTO>>
                {
                    Object = globals
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Create")]
        [HttpPost]
        [Route("")]
        public async Task<IServiceResponse<object>> AddGlobalProperty(GlobalPropertyDTO globalPropertyDto)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var global = await _globalService.AddGlobalProperty(globalPropertyDto);

                return new ServiceResponse<object>
                {
                    Object = global
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("{globalPropertyId:int}")]
        public async Task<IServiceResponse<GlobalPropertyDTO>> GetGlobalProperty(int globalPropertyId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var global = await _globalService.GetGlobalPropertyById(globalPropertyId);

                return new ServiceResponse<GlobalPropertyDTO>
                {
                    Object = global
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Delete")]
        [HttpDelete]
        [Route("{globalPropertyId:int}")]
        public async Task<IServiceResponse<bool>> DeleteGlobalProperty(int globalPropertyId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _globalService.RemoveGlobalProperty(globalPropertyId);

                return new ServiceResponse<bool>
                {
                    Object = true
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Update")]
        [HttpPut]
        [Route("{globalPropertyId:int}")]
        public async Task<IServiceResponse<bool>> UpdateGlobalProperty(int globalPropertyId, GlobalPropertyDTO globalPropertyDto)
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _globalService.UpdateGlobalProperty(globalPropertyId, globalPropertyDto);

                return new ServiceResponse<bool>
                {
                    Object = true
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Update")]
        [HttpPut]
        [Route("{globalPropertyId:int}/status/{status}")]
        public async Task<IServiceResponse<object>> UpdateZone(int globalPropertyId, bool status)
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _globalService.UpdateGlobalProperty(globalPropertyId, status);
                return new ServiceResponse<object>
                {
                    Object = true
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("{dropoffdiscount}")]
        public async Task<IServiceResponse<decimal>> GetDropOffDiscountInGlobalProperty()
        {
            return await HandleApiOperationAsync(async () =>
            {
                int countryId = await _userService.GetUserActiveCountryId();
                var global = await _globalService.GetDropOffDiscountInGlobalProperty(countryId);

                return new ServiceResponse<decimal>
                {
                    Object = global
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("gofaster")]
        public async Task<IServiceResponse<decimal>> GetGoFasterPercentageInGlobalProperty()
        {
            return await HandleApiOperationAsync(async () =>
            {
                int countryId = await _userService.GetUserActiveCountryId();
                var global = await _globalService.GetGoFasterPercentageInGlobalProperty(countryId);

                return new ServiceResponse<decimal>
                {
                    Object = global
                };
            });
        }
    }
}
