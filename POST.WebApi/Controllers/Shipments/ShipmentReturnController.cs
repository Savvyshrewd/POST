﻿using POST.Core.IServices;
using POST.Services.Implementation;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using POST.CORE.IServices.Shipments;
using POST.CORE.DTO.Shipments;
using POST.WebApi.Filters;

namespace POST.WebApi.Controllers.Shipments
{
    [Authorize(Roles = "Shipment, ViewAdmin")]
    [RoutePrefix("api/shipmentreturn")]
    public class ShipmentReturnController : BaseWebApiController
    {
        private readonly IShipmentReturnService _service;

        public ShipmentReturnController(IShipmentReturnService service) : base(nameof(ShipmentReturnController))
        {
            _service = service;
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("")]
        public async Task<IServiceResponse<IEnumerable<ShipmentReturnDTO>>> GetAllShipmentReturns()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var shipmentReturns = await _service.GetShipmentReturns();

                return new ServiceResponse<IEnumerable<ShipmentReturnDTO>>
                {
                    Object = shipmentReturns
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("search")]
        public async Task<IServiceResponse<IEnumerable<ShipmentReturnDTO>>> GetAllShipmentReturns([FromUri]FilterOptionsDto filterOptionsDto)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var shipmentReturnTuple = await _service.GetShipmentReturns(filterOptionsDto);
                return new ServiceResponse<IEnumerable<ShipmentReturnDTO>>
                {
                    Object = shipmentReturnTuple.Item1,
                    Total = shipmentReturnTuple.Item2
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("{waybill}")]
        public async Task<IServiceResponse<ShipmentReturnDTO>> GetShipmentReturnByWaybill(string waybill)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var shipmentReturn = await _service.GetShipmentReturnById(waybill);

                return new ServiceResponse<ShipmentReturnDTO>
                {
                    Object = shipmentReturn
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Delete")]
        [HttpDelete]
        [Route("{waybill}")]
        public async Task<IServiceResponse<bool>> DeleteShipmentReturnByCode(string waybill)
        {
            return await HandleApiOperationAsync(async () => {
                await _service.RemoveShipmentReturn(waybill);
                return new ServiceResponse<bool>
                {
                    Object = true
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Create")]
        [HttpPost]
        [Route("")]
        public async Task<IServiceResponse<bool>> AddShipmentReturn(ShipmentReturnDTO shipmentReturn)
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _service.AddShipmentReturn(shipmentReturn);

                return new ServiceResponse<bool>
                {
                    Object = true
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Create")]
        [HttpPost]
        [Route("{waybill}")]
        public async Task<IServiceResponse<string>> AddShipmentReturn(string waybill)
        {
            return await HandleApiOperationAsync(async () =>
            {
                string newWaybill = await _service.AddShipmentReturn(waybill);

                return new ServiceResponse<string>
                {
                    Object = newWaybill
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Update")]
        [HttpPut]
        [Route("")]
        public async Task<IServiceResponse<bool>> UpdateShipmentReturnBywaybill(ShipmentReturnDTO shipmentReturn)
        {
            return await HandleApiOperationAsync(async () => {
                await _service.UpdateShipmentReturn(shipmentReturn);
                return new ServiceResponse<bool>
                {
                    Object = true
                };
            });
        }
    }
}
