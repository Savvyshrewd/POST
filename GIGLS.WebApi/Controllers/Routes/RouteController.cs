﻿using POST.Core.Common.Helpers;
using POST.Core.DTO;
using POST.Core.DTO.Routes;
using POST.Core.IServices;
using POST.Core.IServices.RouteServices;
using POST.Services.Implementation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace POST.WebApi.Controllers.Routes
{
    //[Authorize(Roles = "Admin, ViewAdmin")]
    [RoutePrefix("api/routes")]
    public class RouteController : BaseWebApiController
    {
        private readonly IRouteService _routeService;
        public RouteController(IRouteService routeService) : base(nameof(RouteController))
        {
            _routeService = routeService;
        }

        [HttpGet]
        [Route("")]
        public async Task<IServiceResponse<PagedList<RouteDto>>> GetCountries(int page, int size, string keyword)
        {
            return await HandleApiOperationAsync(async () =>
            {

                var routes = await _routeService.GetRoutes
                (new BaseSearchDto { PageSize = size, Keyword = keyword, PageIndex = page });

                return new ServiceResponse<PagedList<RouteDto>>
                {
                    Object = routes
                };
            });
        }

        [HttpPost]
        [Route("")]
        public async Task<IServiceResponse<CreateRouteDto>> CreateRoute(CreateRouteDto model)
        {
            return await HandleApiOperationAsync(async () =>
            {
                if (!ModelState.IsValid)
                    throw new Exception(ModelState.Values.FirstOrDefault().Errors.Select(p => p.ErrorMessage).FirstOrDefault());

                var routes = await _routeService.AddRoute(model);

                return new ServiceResponse<CreateRouteDto>
                {
                    Object = routes
                };
            });
        }
    }
}
