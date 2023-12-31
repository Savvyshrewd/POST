﻿using POST.Core.IServices;
using POST.Core.DTO.PaymentTransactions;
using POST.Core.IServices.Business;
using POST.Services.Implementation;
using System.Threading.Tasks;
using System.Web.Http;
using POST.WebApi.Filters;
using POST.Core.DTO.Shipments;

namespace POST.WebApi.Controllers.Business
{
    [Authorize(Roles = "Shipment, ViewAdmin")]
    [RoutePrefix("api/pricing")]
    public class PricingController : BaseWebApiController
    {
        private readonly IPricingService _pricing;

        public PricingController(IPricingService pricingService) : base(nameof(PricingController))
        {
            _pricing = pricingService;
        }

        [GIGLSActivityAuthorize(Activity = "Create")]
        [HttpPost]
        [Route("")]
        public async Task<IServiceResponse<decimal>> GetPrice(PricingDTO pricingDto)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var userCountryId = await _pricing.GetUserCountryId();
                pricingDto.CountryId = userCountryId;
                var price = await _pricing.GetPrice(pricingDto);

                return new ServiceResponse<decimal>
                {
                    Object = price
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Create")]
        [HttpPost]
        [Route("haulage")]
        public async Task<IServiceResponse<decimal>> GetHaulagePrice(HaulagePricingDTO haulagePricingDto)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var userCountryId = await _pricing.GetUserCountryId();
                haulagePricingDto.CountryId = userCountryId;

                var price = await _pricing.GetHaulagePrice(haulagePricingDto);

                return new ServiceResponse<decimal>
                {
                    Object = price
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Create")]
        [HttpPost]
        [Route("international")]
        public async Task<IServiceResponse<decimal>> GetInternationalPrice(PricingDTO pricingDto)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var userCountryId = await _pricing.GetUserCountryId();
                pricingDto.CountryId = userCountryId;

                var price = await _pricing.GetInternationalPrice(pricingDto);

                return new ServiceResponse<decimal>
                {
                    Object = price
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Create")]
        [HttpPost]
        [Route("reroute")]
        public async Task<IServiceResponse<ShipmentDTO>> GetRoutePrice(ReroutePricingDTO pricingDto)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var userCountryId = await _pricing.GetUserCountryId();
                pricingDto.CountryId = userCountryId;

                var price = await _pricing.GetReroutePrice(pricingDto);

                return new ServiceResponse<ShipmentDTO>
                {
                    Object = price
                };
            });
        }


        [GIGLSActivityAuthorize(Activity = "Create")]
        [HttpPost]
        [Route("getpriceuk")]
        public async Task<IServiceResponse<decimal>> GetPriceForUK(UKPricingDTO pricingDto)
        {
            return await HandleApiOperationAsync(async () =>
            {
                //var userCountryId = await _pricing.GetUserCountryId();
                //pricingDto.CountryId = userCountryId;
                var price = await _pricing.GetPriceByCategory(pricingDto);
                return new ServiceResponse<decimal>
                {
                    Object = price
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Create")]
        [HttpPost]
        [Route("getpriceusa")]
        public async Task<IServiceResponse<decimal>> GetPriceForUSA(UKPricingDTO pricingDto)
        {
            return await HandleApiOperationAsync(async () =>
            {
                //var userCountryId = await _pricing.GetUserCountryId();
                //pricingDto.CountryId = userCountryId;
                var price = await _pricing.GetPriceByCategoryForUSA(pricingDto);
                return new ServiceResponse<decimal>
                {
                    Object = price
                };
            });
        }
    }
}
