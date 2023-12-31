﻿using POST.Core.IServices;
using POST.Core.DTO.Account;
using POST.Core.IServices.Account;
using POST.Services.Implementation;
using System.Threading.Tasks;
using System.Web.Http;
using POST.WebApi.Filters;
using System.Collections.Generic;

namespace POST.WebApi.Controllers.Account
{
    [Authorize(Roles = "Account, ViewAdmin")]
    [RoutePrefix("api/vat")]
    public class VATController : BaseWebApiController
    {
        private readonly IVATService _vatService;
        public VATController(IVATService vatService) : base(nameof(VATController))
        {
            _vatService = vatService;
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("")]
        public async Task<IServiceResponse<IEnumerable<VATDTO>>> GetVATs()
        {
            return await HandleApiOperationAsync(async () =>
            {

                var vat = await _vatService.GetVATs();

                return new ServiceResponse<IEnumerable<VATDTO>>
                {
                    Object = vat
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Create")]
        [HttpPost]
        [Route("")]
        public async Task<IServiceResponse<object>> AddVAT(VATDTO vatDto)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var vat = await _vatService.AddVAT(vatDto);

                return new ServiceResponse<object>
                {
                    Object = vat
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("{vatId:int}")]
        public async Task<IServiceResponse<VATDTO>> GetVAT(int vatId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var vat = await _vatService.GetVATById(vatId);

                return new ServiceResponse<VATDTO>
                {
                    Object = vat
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Delete")]
        [HttpDelete]
        [Route("{vatId:int}")]
        public async Task<IServiceResponse<bool>> DeleteVAT(int vatId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _vatService.RemoveVAT(vatId);

                return new ServiceResponse<bool>
                {
                    Object = true
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Update")]
        [HttpPut]
        [Route("{vatId:int}")]
        public async Task<IServiceResponse<bool>> UpdateVAT(int vatId, VATDTO vatDto)
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _vatService.UpdateVAT(vatId, vatDto);

                return new ServiceResponse<bool>
                {
                    Object = true
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("country")]
        public async Task<IServiceResponse<VATDTO>> GetVATByCountry()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var vat = await _vatService.GetVATByCountry();

                return new ServiceResponse<VATDTO>
                {
                    Object = vat
                };
            });
        }
    }
}
