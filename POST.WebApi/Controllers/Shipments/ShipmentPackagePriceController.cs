﻿using POST.Core.DTO.Report;
using POST.Core.DTO.Shipments;
using POST.Core.DTO.Stores;
using POST.Core.IServices;
using POST.Core.IServices.Shipments;
using POST.CORE.DTO.Report;
using POST.Services.Implementation;
using POST.WebApi.Filters;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace POST.WebApi.Controllers.Shipments
{
    [Authorize(Roles = "Admin, ViewAdmin")]
    [RoutePrefix("api/shipmentpackageprice")]
    public class ShipmentPackagePriceController : BaseWebApiController
    {

        private readonly IShipmentPackagePriceService _packagePriceService;
        public ShipmentPackagePriceController(IShipmentPackagePriceService packagePriceService) : base(nameof(ShipmentPackagePriceController))
        {
            _packagePriceService = packagePriceService;
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("")]
        public async Task<IServiceResponse<IEnumerable<ShipmentPackagePriceDTO>>> GetShipmentPackagePrices()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var shipmentPackagePrices = await _packagePriceService.GetShipmentPackagePrices();

                return new ServiceResponse<IEnumerable<ShipmentPackagePriceDTO>>
                {
                    Object = shipmentPackagePrices
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Create")]
        [HttpPost]
        [Route("")]
        public async Task<IServiceResponse<object>> AddShipmentPackagePrice(ShipmentPackagePriceDTO shipmentPackagePriceDTO)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var shipmentPackagePrice = await _packagePriceService.AddShipmentPackagePrice(shipmentPackagePriceDTO);

                return new ServiceResponse<object>
                {
                    Object = shipmentPackagePrice
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("{shipmentPackagePriceId:int}")]
        public async Task<IServiceResponse<ShipmentPackagePriceDTO>> GetShipmentPackagePrice(int shipmentPackagePriceId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var shipmentPackagePrice = await _packagePriceService.GetShipmentPackagePriceById(shipmentPackagePriceId);

                return new ServiceResponse<ShipmentPackagePriceDTO>
                {
                    Object = shipmentPackagePrice
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Delete")]
        [HttpDelete]
        [Route("{shipmentPackagePriceId:int}")]
        public async Task<IServiceResponse<bool>> DeleteShipmentPackagePrice(int shipmentPackagePriceId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _packagePriceService.DeleteShipmentPackagePrice(shipmentPackagePriceId);

                return new ServiceResponse<bool>
                {
                    Object = true
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Update")]
        [HttpPut]
        [Route("{shipmentPackagePriceId:int}")]
        public async Task<IServiceResponse<bool>> UpdateShipmentPackagePrice(int shipmentPackagePriceId, ShipmentPackagePriceDTO shipmentPackagePriceDTO)
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _packagePriceService.UpdateShipmentPackagePrice(shipmentPackagePriceId, shipmentPackagePriceDTO);

                return new ServiceResponse<bool>
                {
                    Object = true
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("country")]
        public async Task<IServiceResponse<IEnumerable<ShipmentPackagePriceDTO>>> GetShipmentPackagePriceByCountry()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var shipmentPackagePrices = await _packagePriceService.GetShipmentPackagePriceByCountry();

                return new ServiceResponse<IEnumerable<ShipmentPackagePriceDTO>>
                {
                    Object = shipmentPackagePrices
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpPut]
        [Route("packagetopup/{shipmentPackagePriceId:int}")]
        public async Task<IServiceResponse<bool>> UpdateShipmentPackageQuantity(int shipmentPackagePriceId, ShipmentPackagePriceDTO shipmentPackagePriceDTO)
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _packagePriceService.UpdateShipmentPackageQuantity(shipmentPackagePriceId, shipmentPackagePriceDTO);

                return new ServiceResponse<bool>
                {
                    Object = true
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("addpackage")]
        public async Task<IServiceResponse<object>> AddShipmentPackage(ShipmentPackagePriceDTO shipmentPackagePriceDTO)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var shipmentPackagePrice = await _packagePriceService.AddShipmentPackage(shipmentPackagePriceDTO);

                return new ServiceResponse<object>
                {
                    Object = shipmentPackagePrice
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("packagetransactions")]
        public async Task<IServiceResponse<IEnumerable<ShipmentPackagingTransactionsDTO>>> GetShipmentPackageTransactions(BankDepositFilterCriteria filterCriteria)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var shipmentPackageTransactions = await _packagePriceService.GetShipmentPackageTransactions(filterCriteria);

                return new ServiceResponse<IEnumerable<ShipmentPackagingTransactionsDTO>>
                {
                    Object = shipmentPackageTransactions
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("servicecenterpackages")]
        public async Task<IServiceResponse<IEnumerable<ServiceCenterPackageDTO>>> GetShipmentPackageForServiceCenter()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var shipmentPackages = await _packagePriceService.GetShipmentPackageForServiceCenter();

                return new ServiceResponse<IEnumerable<ServiceCenterPackageDTO>>
                {
                    Object = shipmentPackages
                };
            });
        }

    }
}
