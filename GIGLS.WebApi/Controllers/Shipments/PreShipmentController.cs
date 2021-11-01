﻿using GIGLS.Core.DTO;
using GIGLS.Core.DTO.Customers;
using GIGLS.Core.DTO.Node;
using GIGLS.Core.DTO.Shipments;
using GIGLS.Core.IServices;
using GIGLS.Core.IServices.Shipments;
using GIGLS.CORE.DTO.Report;
using GIGLS.CORE.DTO.Shipments;
using GIGLS.Services.Implementation;
using GIGLS.WebApi.Filters;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace GIGLS.WebApi.Controllers.Shipments
{
    [Authorize(Roles = "Shipment, ViewAdmin")]
    [RoutePrefix("api/preshipment")]
    public class PreShipmentController : BaseWebApiController
    {
        private readonly IPreShipmentService _service;
        private readonly IPreShipmentMobileService _preShipmentMobileService;

        public PreShipmentController(IPreShipmentService service, IPreShipmentMobileService preShipmentMobileService) : base(nameof(ShipmentController))
        {
            _service = service;
            _preShipmentMobileService = preShipmentMobileService;
        }
        
        [GIGLSActivityAuthorize(Activity = "Delete")]
        [HttpDelete]
        [Route("{PreShipmentId:int}")]
        public async Task<IServiceResponse<bool>> DeletePreShipment(int preShipmentId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _service.DeletePreShipment(preShipmentId);
                return new ServiceResponse<bool>
                {
                    Object = true
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Delete")]
        [HttpDelete]
        [Route("{waybill}/waybill")]
        public async Task<IServiceResponse<bool>> DeletePreShipment(string waybill)
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _service.DeletePreShipment(waybill);
                return new ServiceResponse<bool>
                {
                    Object = true
                };
            });
        }              

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("GetMobileShipments")]
        public async Task<IServiceResponse<List<PreShipmentMobileDTO>>> GetPreShipmentsMobile(BaseFilterCriteria filter)
        {
            return await HandleApiOperationAsync(async () =>
            {
            var preshipmentMobile = await _preShipmentMobileService.GetShipments(filter);
            return new ServiceResponse<List<PreShipmentMobileDTO>>
            {
                Object = preshipmentMobile

            };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("getmobileshipments/{waybill}")]
        public async Task<IServiceResponse<PreShipmentMobileDTO>> GetPreShipmentsMobileByWaybill(string waybill)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var preshipmentMobile = await _preShipmentMobileService.GetShipmentByWaybill(waybill);
                return new ServiceResponse<PreShipmentMobileDTO>
                {
                    Object = preshipmentMobile

                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("waybill/{waybill}")]
        public async Task<IServiceResponse<PreShipmentMobileDTO>> GetPreShipmentMobileDetail(string waybill)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var preShipment = await _preShipmentMobileService.GetPreShipmentDetail(waybill);
                return new ServiceResponse<PreShipmentMobileDTO>
                {
                    Object = preShipment
                };
            });
        }
        
        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("GIGGoDashboard")]
        public async Task<IServiceResponse<GIGGoDashboardDTO>> GetDashboardInfo(BaseFilterCriteria filter)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var data = await _preShipmentMobileService.GetDashboardInfo(filter);
                return new ServiceResponse<GIGGoDashboardDTO>
                {
                    Object = data
                };
            });
        }
               
        [HttpGet]
        [Route("giggopresentdayshipments")]
        public async Task<IServiceResponse<List<LocationDTO>>> GetPresentDayShipmentLocations()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var preshipment = await _preShipmentMobileService.GetPresentDayShipmentLocations();
                return new ServiceResponse<List<LocationDTO>>
                {
                    Object = preshipment
                };
            });
        }

        [HttpGet]
        [Route("batchedpreshipmentmobile/{searchParam}")]
        public async Task<IServiceResponse<List<PreShipmentMobileDTO>>> GetBatchedPreShipmentMobile(string searchParam)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var preshipment = await _preShipmentMobileService.GetBatchPreShipmentMobile(searchParam);
                return new ServiceResponse<List<PreShipmentMobileDTO>>
                {
                    Object = preshipment
                };
            });
        }

        [HttpGet]
        [Route("batchedpreshipmentmobileowners")]
        public async Task<IServiceResponse<List<CompanyDTO>>> GetBatchPreShipmentMobileOwners()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var preshipment = await _preShipmentMobileService.GetBatchPreShipmentMobileOwners();
                return new ServiceResponse<List<CompanyDTO>>
                {
                    Object = preshipment
                };
            });
        }

        [HttpGet]
        [Route("removefromqueue/{waybill}")]
        public async Task<IServiceResponse<NewNodeResponse>> RemoveShipmentFromQueue(string waybill)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _preShipmentMobileService.RemoveShipmentFromQueue(waybill);
                return new ServiceResponse<NewNodeResponse>
                {
                    Object = result
                };
            });
        }

        [HttpGet]
        [Route("addtoqueue/{waybill}")]
        public async Task<IServiceResponse<bool>>AddPreShipmentMobileToQueue(string waybill)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _preShipmentMobileService.AddShipmentToQueue(waybill);
                return new ServiceResponse<bool>
                {
                    Object = result
                };
            });
        }

        [HttpGet]
        [Route("GetGIGGOProgressReport")]
        public async Task<IServiceResponse<object>> GetGIGGOProgressReport()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _preShipmentMobileService.GetGIGGOProgressReport();
                return new ServiceResponse<object>
                {
                    Object = result
                };
            });
        }
        [HttpGet]
        [Route("GetGIGGOProgressReportForShipmentCreated")]
        public async Task<IServiceResponse<List<PreShipmentMobileDTO>>> GetGIGGOProgressReportForShipmentCreated()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _preShipmentMobileService.GetGIGGOProgressReportForShipmentCreated();
                return new ServiceResponse<List<PreShipmentMobileDTO>>
                {
                    Object = result
                };
            });
        }
        [HttpGet]
        [Route("GetGIGGOProgressReportForShipmentAssigned")]
        public async Task<IServiceResponse<List<PreShipmentMobileDTO>>> GetGIGGOProgressReportForShipmentAssigned()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _preShipmentMobileService.GetGIGGOProgressReportForShipmentAssigned();
                return new ServiceResponse<List<PreShipmentMobileDTO>>
                {
                    Object = result
                };
            });
        }
        [HttpGet]
        [Route("GetGIGGOProgressReportForShipmentPicked")]
        public async Task<IServiceResponse<List<PreShipmentMobileDTO>>> GetGIGGOProgressReportForShipmentPicked()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _preShipmentMobileService.GetGIGGOProgressReportForShipmentPicked();
                return new ServiceResponse<List<PreShipmentMobileDTO>>
                {
                    Object = result
                };
            });
        }

        [HttpPost]
        [Route("getpreshipmentmobiletat")]
        public async Task<IServiceResponse<List<PreShipmentMobileTATDTO>>> GetPreshipmentMobileTAT(NewFilterOptionsDto newFilterOptionsDto)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _preShipmentMobileService.GetPreshipmentMobileTAT(newFilterOptionsDto);
                return new ServiceResponse<List<PreShipmentMobileTATDTO>>
                {
                    Object = result
                };
            });
        }


        [HttpPost]
        [Route("getpreshipmentmobiledeliverytat")]
        public async Task<IServiceResponse<List<PreShipmentMobileTATDTO>>> GetPreshipmentMobileDeliveryTAT(NewFilterOptionsDto newFilterOptionsDto)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _preShipmentMobileService.GetPreshipmentMobileDeliveryTAT(newFilterOptionsDto);
                return new ServiceResponse<List<PreShipmentMobileTATDTO>>
                {
                    Object = result
                };
            });
        }

        [HttpPost]
        [Route("removenodependingshipment")]
        public async Task<IServiceResponse<NewNodeResponse>> RemoveNodePendingShipment(PendingNodeShipmentDTO dto)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _preShipmentMobileService.RemoveNodePendingShipment(dto);
                return new ServiceResponse<NewNodeResponse>
                {
                    Object = result
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("cancelledshipmentreport")]
        public async Task<IServiceResponse<IEnumerable<CancelledShipmentDTO>>> GetCancelledShipment(DateFilterCriteria dateFilterCriteria)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var canceledShipment = await _preShipmentMobileService.GetCanceledShipment(dateFilterCriteria);
                return new ServiceResponse<IEnumerable<CancelledShipmentDTO>>
                {
                    Object = canceledShipment
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("cancelledshipmentreport/{waybill}")]
        public async Task<IServiceResponse<IEnumerable<CancelledShipmentDTO>>> GetCancelledShipment(string waybill)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var canceledShipment = await _preShipmentMobileService.GetCanceledShipment(waybill);
                return new ServiceResponse<IEnumerable<CancelledShipmentDTO>>
                {
                    Object = canceledShipment
                };
            });
        }

    }
}