﻿using GIGLS.Core.DTO.Account;
using GIGLS.Core.DTO.ServiceCentres;
using GIGLS.Core.DTO.Shipments;
using GIGLS.Core.DTO.Zone;
using GIGLS.Core.IServices;
using GIGLS.Core.IServices.Shipments;
using GIGLS.CORE.DTO.Report;
using GIGLS.CORE.DTO.Shipments;
using GIGLS.Services.Implementation;
using GIGLS.WebApi.Filters;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using GIGLS.CORE.IServices.Report;
using GIGLS.Core.Enums;
using GIGLS.Core.IServices.User;
using System;
using System.Web.Http.Results;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace GIGLS.WebApi.Controllers.Shipments
{
    //[Authorize(Roles = "Shipment, ViewAdmin")]
    [RoutePrefix("api/shipment/magaya")]
    public class MagayaController : BaseWebApiController
    {
        private readonly IMagayaService _service;
        private readonly IUserService _userService;


        public MagayaController(IMagayaService service, IUserService userService) : base(nameof(MagayaController)) 
        {
            _service = service;
            _userService = userService;
        }


        //[GIGLSActivityAuthorize(Activity = "Create")]
        [HttpPost]
        [Route("AddShipment")]
        public async Task<IServiceResponse<string>> AddShipment(MagayaShipmentDTO MagayaShipmentDTO)
        {
            return await HandleApiOperationAsync(async () =>
            {

                //1. initialize the access key variable
                int access_key = 0;

                //2. Call the open connection to get the session key
                var openconn = _service.OpenConnection(out access_key);

                //3. Call the Magaya SetTransaction Method from MagayaService
                var result = _service.SetTransactions(access_key);
                
                //3. Pass the return to the view or caller
                return new ServiceResponse<string>()
                {
                    Object = result
                };
            });
        }

    }
}
