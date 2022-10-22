﻿using POST.Core.DTO.Shipments;
using POST.Core.IServices;
using POST.Core.IServices.Business;
using POST.Services.Implementation;
using POST.WebApi.Filters;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace POST.WebApi.Controllers.Business
{
    [Authorize(Roles = "Shipment, ViewAdmin")]
    [RoutePrefix("api/shipmenttrack")]
    public class ShipmentTrackController : BaseWebApiController
    {
        private readonly IShipmentTrackService _shipmentTrack;

        public ShipmentTrackController(IShipmentTrackService shipmentTrack) : base(nameof(ShipmentTrackController))
        {
            _shipmentTrack = shipmentTrack;
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("{waybillNumber}")]
        public async Task<IServiceResponse<IEnumerable<ShipmentTrackingDTO>>> TrackShipment(string waybillNumber)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _shipmentTrack.TrackShipment(waybillNumber);

                return new ServiceResponse<IEnumerable<ShipmentTrackingDTO>>
                {
                    Object = result
                };
            });
        }
    }
}
