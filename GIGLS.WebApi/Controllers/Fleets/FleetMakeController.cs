﻿using POST.Core.IServices;
using POST.Core.DTO.Fleets;
using POST.Core.IServices.Fleets;
using POST.Services.Implementation;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using POST.WebApi.Filters;

namespace POST.WebApi.Controllers.Fleets
{
    [Authorize(Roles = "Shipment")]
    [RoutePrefix("api/fleetmake")]
    public class FleetMakeController : BaseWebApiController
    {
        private readonly IFleetMakeService _service;

        public FleetMakeController(IFleetMakeService service) : base(nameof(FleetMakeController))
        {
            _service = service;
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("")]
        public async Task<IServiceResponse<IEnumerable<FleetMakeDTO>>> GetFleetMakes()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var Fleets = await _service.GetFleetMakes();

                return new ServiceResponse<IEnumerable<FleetMakeDTO>>
                {
                    Object = Fleets
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Create")]
        [HttpPost]
        [Route("")]
        public async Task<IServiceResponse<object>> AddFleetMake(FleetMakeDTO FleetDto)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var FleetMaker = await _service.AddFleetMake(FleetDto);

                return new ServiceResponse<object>
                {
                    Object = FleetMaker
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Delete")]
        [HttpGet]
        [Route("{FleetId:int}")]
        public async Task<IServiceResponse<FleetMakeDTO>> GetFleetMake(int FleetId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var Fleet = await _service.GetFleetMakeById(FleetId);

                return new ServiceResponse<FleetMakeDTO>
                {
                    Object = Fleet
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Delete")]
        [HttpDelete]
        [Route("{FleetId:int}")]
        public async Task<IServiceResponse<bool>> DeleteFleetMake(int FleetId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _service.DeleteFleetMake(FleetId);

                return new ServiceResponse<bool>
                {
                    Object = true
                }; 
            });
        }

        [GIGLSActivityAuthorize(Activity = "Update")]
        [HttpPut]
        [Route("{FleetId:int}")]
        public async Task<IServiceResponse<bool>> UpdateFleetMake(int FleetId, FleetMakeDTO FleetDto)
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _service.UpdateFleetMake(FleetId, FleetDto);

                return new ServiceResponse<bool>
                {
                    Object = true
                };
            });
        }

    }
}
