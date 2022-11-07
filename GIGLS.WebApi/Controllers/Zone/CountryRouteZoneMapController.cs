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
    [RoutePrefix("api/countryroutezonemap")]
    public class CountryRouteZoneMapController : BaseWebApiController
    {
        private readonly ICountryRouteZoneMapService _mapService;
        public CountryRouteZoneMapController(ICountryRouteZoneMapService mapService):base(nameof(CountryRouteZoneMapController))
        {
            _mapService = mapService;
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("")]
        public async Task<IServiceResponse<IEnumerable<CountryRouteZoneMapDTO>>> GetCountryRouteZoneMaps()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var zones = await _mapService.GetCountryRouteZoneMaps();
                return new ServiceResponse<IEnumerable<CountryRouteZoneMapDTO>>
                {
                    Object = zones
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Create")]
        [HttpPost]
        [Route("")]
        public async Task<IServiceResponse<object>> AddCountryZone(CountryRouteZoneMapDTO zoneDto)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var zone = await _mapService.AddCountryRouteZoneMap(zoneDto);
                return new ServiceResponse<object>
                {
                    Object = zone
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("{mappingId:int}")]
        public async Task<IServiceResponse<CountryRouteZoneMapDTO>> GetCountryZone(int mappingId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var zone = await _mapService.GetCountryRouteZoneMapById(mappingId);
                return new ServiceResponse<CountryRouteZoneMapDTO>
                {
                    Object = zone
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("{departure:int}/{destination:int}")]
        public async Task<IServiceResponse<CountryRouteZoneMapDTO>> GetZone(int departure, int destination)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var zone = await _mapService.GetZone(departure, destination);

                return new ServiceResponse<CountryRouteZoneMapDTO>
                {
                    Object = zone
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("getzone")]
        public async Task<IServiceResponse<CountryRouteZoneMapDTO>> GetZone(CountryMapQueryDTO data)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var zone = await _mapService.GetZone(data.DepartureId, data.DestinationId, data.CompanyMap);

                return new ServiceResponse<CountryRouteZoneMapDTO>
                {
                    Object = zone
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Delete")]
        [HttpDelete]
        [Route("{mappingId:int}")]
        public async Task<IServiceResponse<bool>> DeleteCountryZone(int mappingId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _mapService.DeleteCountryRouteZoneMap(mappingId);
                return new ServiceResponse<bool>
                {
                    Object = true
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Update")]
        [HttpPut]
        [Route("{mappingId:int}")]
        public async Task<IServiceResponse<bool>> UpdateCountryZone(int mappingId, CountryRouteZoneMapDTO mappingDto)
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _mapService.UpdateCountryRouteZoneMap(mappingId, mappingDto);
                return new ServiceResponse<bool>
                {
                    Object = true
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Update")]
        [HttpPut]
        [Route("{mappingId:int}/status/{status}")]
        public async Task<IServiceResponse<bool>> UpdateStatus(int mappingId, bool status)
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _mapService.UpdateStatusCountryRouteZoneMap(mappingId, status);
                return new ServiceResponse<bool>
                {
                    Object = true
                };
            });
        }

    }
}
