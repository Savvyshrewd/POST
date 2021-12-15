﻿using GIGLS.Core.IServices;
using GIGLS.Core.DTO.ServiceCentres;
using GIGLS.Core.IServices.ServiceCentres;
using GIGLS.Services.Implementation;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using GIGLS.WebApi.Filters;
using GIGLS.Core.DTO;

namespace GIGLS.WebApi.Controllers.ServiceCentres
{
    [Authorize(Roles = "Admin, ViewAdmin")]
    [RoutePrefix("api/station")]
    public class StationController : BaseWebApiController
    {
        private IStationService _stationService;
        private IServiceCentreService _serviceCentreService; 
        public StationController(IStationService stationService, IServiceCentreService serviceCentreService) : base(nameof(StationController))
        {
            _stationService = stationService;
            _serviceCentreService = serviceCentreService;
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("")]
        public async Task<IServiceResponse<IEnumerable<StationDTO>>> GetStations()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var stations = await _stationService.GetStations();
                return new ServiceResponse<IEnumerable<StationDTO>>
                {
                    Object = stations
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("local")]
        public async Task<IServiceResponse<IEnumerable<StationDTO>>> GetLocalStations()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var stations = await _stationService.GetLocalStations();
                return new ServiceResponse<IEnumerable<StationDTO>>
                {
                    Object = stations
                };
            });
        }


        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("international")]
        public async Task<IServiceResponse<IEnumerable<StationDTO>>> GetInternationalStations()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var stations = await _stationService.GetInternationalStations();
                return new ServiceResponse<IEnumerable<StationDTO>>
                {
                    Object = stations
                };
            });
        }


        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("{stationId:int}")]
        public async Task<IServiceResponse<StationDTO>> GetStationById(int stationId)
        {
            return await HandleApiOperationAsync(async () =>
            {

                var station =await  _stationService.GetStationById(stationId);

                return new ServiceResponse<StationDTO>
                {
                    Object = station
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("serviceCenter/{serviceCenterId:int}")]
        public async Task<IServiceResponse<StationDTO>> GetStationByServiceCenterId(int serviceCenterId) 
        {
            return await HandleApiOperationAsync(async () =>
            {
                var serviceCenter = await _serviceCentreService.GetServiceCentreById(serviceCenterId);
                var station = await _stationService.GetStationById(serviceCenter.StationId);

                return new ServiceResponse<StationDTO>
                {
                    Object = station
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Create")]
        [HttpPost]
        [Route("")]
        public async Task<IServiceResponse<object>> CreateStation(StationDTO newStation)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var station = await _stationService.AddStation(newStation);
                return new ServiceResponse<object>
                {
                    Object = station
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Update")]
        [HttpPut]
        [Route("{stationId:int}")]
        public async Task<IServiceResponse<object>> UpdateStation(int stationId, StationDTO updatedStation)
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _stationService.UpdateStation(stationId, updatedStation);
                return new ServiceResponse<object>
                {
                    Object = true
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Update")]
        [HttpPut]
        [Route("{stationId:int}/giggostatus/{status}")]
        public async Task<IServiceResponse<object>> UpdateGIGGoStationStatus(int stationId, bool status)
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _stationService.UpdateGIGGoStationStatus(stationId, status);
                return new ServiceResponse<object>
                {
                    Object = true
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Delete")]
        [HttpDelete]
        [Route("{stationId:int}")]
        public async Task<IServiceResponse<bool>> DeleteStation (int stationId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _stationService.DeleteStation(stationId);
                return new ServiceResponse<bool>
                {
                    Object = true
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("giggostations")]
        public async Task<IServiceResponse<IEnumerable<StationDTO>>> GetActiveGIGGoStations()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var stations = await _stationService.GetActiveGIGGoStations();
                return new ServiceResponse<IEnumerable<StationDTO>>
                {
                    Object = stations
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("{stationId:int}/giggostation")]
        public async Task<IServiceResponse<GiglgoStationDTO>> GetGIGGoStationById(int stationId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var station = await _stationService.GetGIGGoStationById(stationId);
                return new ServiceResponse<GiglgoStationDTO>
                {
                    Object = station
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("usercountry")]
        public async Task<IServiceResponse<IEnumerable<StationDTO>>> GetStationsByUserCountry()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var stations = await _stationService.GetStationsByUserCountry();
                return new ServiceResponse<IEnumerable<StationDTO>>
                {
                    Object = stations
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("{countryId:int}/country")]
        public async Task<IServiceResponse<IEnumerable<StationDTO>>> GetStationsByCountry(int countryId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var stations = await _stationService.GetStationsByCountry(countryId);
                return new ServiceResponse<IEnumerable<StationDTO>>
                {
                    Object = stations
                };
            });
        }
    }
}
