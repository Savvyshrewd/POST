﻿using POST.Core.DTO.Fleets;
using POST.Core.IServices;
using POST.Core.IServices.Fleets;
using POST.Services.Implementation;
using POST.WebApi.Filters;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace POST.WebApi.Controllers.dispatchs
{
    [Authorize(Roles = "Shipment, ViewAdmin")]
    [RoutePrefix("api/dispatch")]
    public class DispatchController : BaseWebApiController
    {
        private readonly IDispatchService _dispatchService;

        public DispatchController(IDispatchService dispatchService) : base(nameof(DispatchController))
        {
            _dispatchService = dispatchService;
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("")]
        public async Task<IServiceResponse<IEnumerable<DispatchDTO>>> GetDispatchs()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var dispatchs = await _dispatchService.GetDispatchs();

                return new ServiceResponse<IEnumerable<DispatchDTO>>
                {
                    Object = dispatchs
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Create")]
        [HttpPost]
        [Route("")]
        public async Task<IServiceResponse<object>> AddDispatch(DispatchDTO dispatchDTO)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var dispatch = await _dispatchService.AddDispatch(dispatchDTO);

                return new ServiceResponse<object>
                {
                    Object = dispatch
                };
            });
        }


        [GIGLSActivityAuthorize(Activity = "Create")]
        [HttpPost]
        [Route("movementdispatch")]
        public async Task<IServiceResponse<object>> AddMovementDispatch(MovementDispatchDTO dispatchDTO) 
        {
            return await HandleApiOperationAsync(async () =>
            {
                var dispatch = await _dispatchService.AddMovementDispatch(dispatchDTO); 

                return new ServiceResponse<object>
                {
                    Object = dispatch
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("{dispatchId:int}")]
        public async Task<IServiceResponse<DispatchDTO>> GetDispatch(int dispatchId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var dispatch = await _dispatchService.GetDispatchById(dispatchId);

                return new ServiceResponse<DispatchDTO>
                {
                    Object = dispatch
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("MovementDispatch/movementmanifest/{movementManifestCode}")]
        public async Task<IServiceResponse<MovementDispatchDTO>> GetDispatchForMovementManifest(string movementManifestCode)  
        {
            return await HandleApiOperationAsync(async () =>
            {
                var dispatch = await _dispatchService.GetMovementDispatchManifestCode(movementManifestCode);

                return new ServiceResponse<MovementDispatchDTO>
                {
                    Object = dispatch
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("{manifest}/manifest")]
        public async Task<IServiceResponse<DispatchDTO>> GetDispatchManifestCode(string manifest)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var dispatch = await _dispatchService.GetDispatchManifestCode(manifest);

                return new ServiceResponse<DispatchDTO>
                {
                    Object = dispatch
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("captain")]
        public async Task<IServiceResponse<List<DispatchDTO>>> GetDispatchCaptainByName(string captain)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var dispatch = await _dispatchService.GetDispatchCaptainByName(captain);

                return new ServiceResponse<List<DispatchDTO>>
                {
                    Object = dispatch
            };
            });
        }


        [GIGLSActivityAuthorize(Activity = "Delete")]
        [HttpDelete]
        [Route("{dispatchId:int}")]
        public async Task<IServiceResponse<bool>> DeleteDispatch(int dispatchId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _dispatchService.DeleteDispatch(dispatchId);

                return new ServiceResponse<bool>
                {
                    Object = true
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Update")]
        [HttpPut]
        [Route("{dispatchId:int}")]
        public async Task<IServiceResponse<bool>> UpdateDispatch(int dispatchId, DispatchDTO dispatchDTO)
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _dispatchService.UpdateDispatch(dispatchId, dispatchDTO);

                return new ServiceResponse<bool>
                {
                    Object = true
                };
            });
        }

    }
}
