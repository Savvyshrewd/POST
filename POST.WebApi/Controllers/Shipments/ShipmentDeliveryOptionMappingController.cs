﻿using POST.Core.DTO.Shipments;
using POST.Core.IServices;
using POST.Core.IServices.Shipments;
using POST.Services.Implementation;
using POST.WebApi.Filters;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace POST.WebApi.Controllers.Shipments
{
    [Authorize(Roles = "Admin, ViewAdmin")]
    [RoutePrefix("api/shipmentdeliveryoptionmapping")]
    public class ShipmentDeliveryOptionMappingController : BaseWebApiController
    {
        private readonly IShipmentDeliveryOptionMappingService _service;

        public ShipmentDeliveryOptionMappingController(IShipmentDeliveryOptionMappingService service) : base(nameof(ShipmentDeliveryOptionMappingController))
        {
            _service = service;
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("")]
        public async Task<IServiceResponse<List<ShipmentDeliveryOptionMappingDTO>>> GetShipmentDeliveryOptionMappings()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var mappings = await _service.GetAllShipmentDeliveryOptionMappings();
                return new ServiceResponse<List<ShipmentDeliveryOptionMappingDTO>>
                {
                    Object = mappings
                };
            });
        }


        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("{waybill}")]
        public async Task<IServiceResponse<List<ShipmentDeliveryOptionMappingDTO>>> GetShipmentDeliveryOptionMappings(string waybill)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var mappings = await _service.GetAllShipmentDeliveryOptionMappings();
                return new ServiceResponse<List<ShipmentDeliveryOptionMappingDTO>>
                {
                    Object = mappings
                };
            });
        }

       // [GIGLSActivityAuthorize(Activity = "Create")]
        //[HttpPost]
        //[Route("")]
        //public async Task<IServiceResponse<object>> AddMapping(ShimpmentDeliveryOptionMappingDTO newMapping)
        //{
        //    return await HandleApiOperationAsync(async () =>
        //    {
        //        var mapping = await _service.AddShimpmentDeliveryOptionMapping(newMapping);
        //        return new ServiceResponse<object>
        //        {
        //            Object = newMapping
        //        };
        //    });
        //}

    }
}
