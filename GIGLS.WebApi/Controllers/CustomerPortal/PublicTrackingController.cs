﻿using GIGLS.Core.DTO;
using GIGLS.Core.DTO.Admin;
using GIGLS.Core.DTO.Shipments;
using GIGLS.Core.IServices;
using GIGLS.Core.IServices.CustomerPortal;
using GIGLS.Core.IServices.Website;
using GIGLS.Services.Implementation;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace GIGLS.WebApi.Controllers.CustomerPortal
{
    [AllowAnonymous]
    [RoutePrefix("api/webtracking")]
    public class PublicTrackingController : BaseWebApiController
    {
        private readonly ICustomerPortalService _portalService;
        private readonly IWebsiteService _websiteService;

        public PublicTrackingController(ICustomerPortalService portalService, IWebsiteService websiteService) : base(nameof(PublicTrackingController))
        {
            _portalService = portalService;
            _websiteService = websiteService;
        }

        [HttpGet]
        [Route("reportsummary")]
        public async Task<IServiceResponse<AdminReportDTO>> GetWebsiteData()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var data = await _portalService.WebsiteData();
                return new ServiceResponse<AdminReportDTO>
                {
                    Object = data

                };
            });
        }

        [HttpGet]
        [Route("track/{waybillNumber}")]
        public async Task<IServiceResponse<IEnumerable<ShipmentTrackingDTO>>> PublicTrackShipment(string waybillNumber)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _portalService.PublicTrackShipment(waybillNumber);

                return new ServiceResponse<IEnumerable<ShipmentTrackingDTO>>
                {
                    Object = result
                };
            });
        }

        [HttpPost]
        [Route("schedulePickup")]
        public async Task<IServiceResponse<bool>> SendSchedulePickupMail(WebsiteMessageDTO obj)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _websiteService.SendSchedulePickupMail(obj);

                return new ServiceResponse<bool>
                {
                    Object = result
                };
            });
        }

        [HttpPost]
        [Route("requestQuote")]
        public async Task<IServiceResponse<bool>> SendRequestQuoteMail(WebsiteMessageDTO obj)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _websiteService.SendQuoteMail(obj);

                return new ServiceResponse<bool>
                {
                    Object = result
                };
            });
        }

        [HttpGet]
        [Route("trackshipment/{waybillNumber}")]
        public async Task<IServiceResponse<MobileShipmentTrackingHistoryDTO>> TrackMobileShipment(string waybillNumber)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _portalService.trackShipment(waybillNumber);

                return new ServiceResponse<MobileShipmentTrackingHistoryDTO>
                {
                    Object = result
                };
            });
        }
    }
}
