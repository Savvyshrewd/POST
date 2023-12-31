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
    [RoutePrefix("api/specialdomesticpackage")]
    public class SpecialDomesticPackageController : BaseWebApiController
    {
        private readonly ISpecialDomesticPackageService _specialPackageService;
        public SpecialDomesticPackageController(ISpecialDomesticPackageService specialPackageService):base(nameof(SpecialDomesticPackageController))
        {
            _specialPackageService = specialPackageService;
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("")]
        public async Task<IServiceResponse<IEnumerable<SpecialDomesticPackageDTO>>> GetSpecialDomesticPackages()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var packages = await _specialPackageService.GetSpecialDomesticPackages();

                return new ServiceResponse<IEnumerable<SpecialDomesticPackageDTO>>
                {
                    Object = packages
                };

            });

        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("active")]
        public async Task<IServiceResponse<IEnumerable<SpecialDomesticPackageDTO>>> GetActiveSpecialDomesticPackages()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var packages = await _specialPackageService.GetActiveSpecialDomesticPackages();

                return new ServiceResponse<IEnumerable<SpecialDomesticPackageDTO>>
                {
                    Object = packages
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("{specialDomesticPackageId:int}")]
        public async Task<IServiceResponse<SpecialDomesticPackageDTO>> GetSpecialDomesticPackage(int specialDomesticPackageId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var package = await _specialPackageService.GetSpecialDomesticPackageById(specialDomesticPackageId);
                return new ServiceResponse<SpecialDomesticPackageDTO>
                {
                    Object = package
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Create")]
        [HttpPost]
        [Route("")]
        public async Task<IServiceResponse<object>> AddSpecialDomesticPackage(SpecialDomesticPackageDTO newSpecialDomesticPackage)
        {

            return await HandleApiOperationAsync(async () => {
                var price = await _specialPackageService.AddSpecialDomesticPackage(newSpecialDomesticPackage);
                return new ServiceResponse<object>
                {
                    Object = price
                };
            });

        }

        [GIGLSActivityAuthorize(Activity = "Update")]
        [HttpPut]
        [Route("{specialDomesticPackageId:int}")]
        public async Task<IServiceResponse<bool>> UpdateSpecialDomesticPackage(int specialDomesticPackageId, SpecialDomesticPackageDTO specialDomesticPackage)
        {

            return await HandleApiOperationAsync(async () =>
            {
                await _specialPackageService.UpdateSpecialDomesticPackage(specialDomesticPackageId, specialDomesticPackage);
                return new ServiceResponse<bool>
                {
                    Object = true
                };
            });

        }

        [GIGLSActivityAuthorize(Activity = "Update")]
        [HttpPut]
        [Route("{specialDomesticPackageId:int}/status/{status}")]
        public async Task<IServiceResponse<bool>> UpdateSpecialDomesticPackage(int specialDomesticPackageId, bool status)
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _specialPackageService.UpdateSpecialDomesticPackage(specialDomesticPackageId, status);
                return new ServiceResponse<bool>
                {
                    Object = true
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Delete")]
        [HttpDelete]
        [Route("{specialDomesticPackageId:int}")]
        public async Task<IServiceResponse<bool>> DeleteSpecialDomesticZonePrice(int specialDomesticPackageId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _specialPackageService.DeleteSpecialDomesticPackage(specialDomesticPackageId);
                return new ServiceResponse<bool>
                {
                    Object = true
                };
            });
        }
    }
}
