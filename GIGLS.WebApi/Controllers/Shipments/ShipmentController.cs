﻿using POST.Core.DTO;
using POST.Core.DTO.Account;
using POST.Core.DTO.DHL;
using POST.Core.DTO.Report;
using POST.Core.DTO.ServiceCentres;
using POST.Core.DTO.Shipments;
using POST.Core.DTO.Zone;
using POST.Core.Enums;
using POST.Core.IServices;
using POST.Core.IServices.CustomerPortal;
using POST.Core.IServices.Shipments;
using POST.Core.IServices.User;
using POST.CORE.DTO.Report;
using POST.CORE.DTO.Shipments;
using POST.CORE.IServices.Report;
using POST.Services.Implementation;
using POST.WebApi.Filters;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace POST.WebApi.Controllers.Shipments
{
    [Authorize(Roles = "Shipment, ViewAdmin")]
    [RoutePrefix("api/shipment")]
    public class ShipmentController : BaseWebApiController
    {
        private readonly IShipmentService _service;
        private readonly IShipmentReportService _reportService;
        private readonly IUserService _userService;
        private readonly IPreShipmentService _preshipmentService;
        private readonly ICustomerPortalService _customerPortalService;
        private readonly IShipmentContactService _shipmentContactService;

        public ShipmentController(IShipmentService service, IShipmentReportService reportService,
            IUserService userService, IPreShipmentService preshipmentService, ICustomerPortalService customerPortalService, IShipmentContactService shipmentContactService) : base(nameof(ShipmentController))
        {
            _service = service;
            _reportService = reportService;
            _userService = userService;
            _preshipmentService = preshipmentService;
            _customerPortalService = customerPortalService;
            _shipmentContactService = shipmentContactService;
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("")]
        public async Task<IServiceResponse<IEnumerable<ShipmentDTO>>> GetShipments([FromUri] FilterOptionsDto filterOptionsDto)
        {
            //filter by User Active Country
            var userActiveCountry = await _userService.GetUserActiveCountry();
            filterOptionsDto.CountryId = userActiveCountry?.CountryId;

            return await HandleApiOperationAsync(async () =>
            {
                var shipments = await _service.GetShipments(filterOptionsDto);
                return new ServiceResponse<IEnumerable<ShipmentDTO>>
                {
                    Object = shipments.Item1,
                    Total = shipments.Item2
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("GetIntltransactionRequest")]
        public async Task<IServiceResponse<Tuple<List<IntlShipmentDTO>, int>>> GetIntltransactionRequest([FromUri] FilterOptionsDto filterOptionsDto)
        {
            var userActiveCountry = await _userService.GetUserActiveCountry();
            filterOptionsDto.CountryId = userActiveCountry?.CountryId;

            return await HandleApiOperationAsync(async () =>
            {
                var result = await _service.GetIntlTransactionShipments(filterOptionsDto);

                return new ServiceResponse<Tuple<List<IntlShipmentDTO>, int>>()
                {
                    Object = result
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("incomingshipments")]
        public async Task<IServiceResponse<IEnumerable<InvoiceViewDTO>>> GetIncomingShipments([FromUri] FilterOptionsDto filterOptionsDto)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var shipments = await _service.GetIncomingShipments(filterOptionsDto);
                return new ServiceResponse<IEnumerable<InvoiceViewDTO>>
                {
                    Object = shipments,
                    Total = shipments.Count
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Create")]
        [HttpPost]
        [Route("")]
        public async Task<IServiceResponse<ShipmentDTO>> AddShipment(ShipmentDTO ShipmentDTO)
        {
            return await HandleApiOperationAsync(async () =>
            {
                //Update SenderAddress for corporate customers
                ShipmentDTO.SenderAddress = null;
                ShipmentDTO.SenderState = null;
                ShipmentDTO.ShipmentScanStatus = ShipmentScanStatus.CRT;
                if (ShipmentDTO.Customer[0].CompanyType == CompanyType.Corporate)
                {
                    ShipmentDTO.SenderAddress = ShipmentDTO.Customer[0].Address;
                    ShipmentDTO.SenderState = ShipmentDTO.Customer[0].State;
                }

                //set some data to null
                ShipmentDTO.ShipmentCollection = null;
                ShipmentDTO.Demurrage = null;
                ShipmentDTO.Invoice = null;
                ShipmentDTO.ShipmentCancel = null;
                ShipmentDTO.ShipmentReroute = null;
                ShipmentDTO.DeliveryOption = null;

                var shipment = await _service.AddShipment(ShipmentDTO);
                return new ServiceResponse<ShipmentDTO>
                {
                    Object = shipment
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Create")]
        [HttpPost]
        [Route("capturestoreshipment")]
        public async Task<IServiceResponse<ShipmentDTO>> AddStoreShipment(ShipmentDTO ShipmentDTO)
        {
            return await HandleApiOperationAsync(async () =>
            {
                //Update SenderAddress for corporate customers
                ShipmentDTO.SenderAddress = null;
                ShipmentDTO.SenderState = null;

                //set some data to null
                ShipmentDTO.ShipmentCollection = null;
                ShipmentDTO.Demurrage = null;
                ShipmentDTO.Invoice = null;
                ShipmentDTO.ShipmentCancel = null;
                ShipmentDTO.ShipmentReroute = null;
                ShipmentDTO.DeliveryOption = null;

                var shipment = await _service.AddShipmentForPaymentWaiver(ShipmentDTO);
                return new ServiceResponse<ShipmentDTO>
                {
                    Object = shipment
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Create")]
        [HttpPost]
        [Route("giggoextension")]
        public async Task<IServiceResponse<ShipmentDTO>> AddGIGGOShipmentFromAgility(PreShipmentMobileFromAgilityDTO ShipmentDTO)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var shipment = await _service.AddAgilityShipmentToGIGGo(ShipmentDTO);
                return new ServiceResponse<ShipmentDTO>
                {
                    Object = shipment
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("{ShipmentId:int}")]
        public async Task<IServiceResponse<ShipmentDTO>> GetShipment(int ShipmentId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var shipment = await _service.GetShipment(ShipmentId);
                return new ServiceResponse<ShipmentDTO>
                {
                    Object = shipment
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("{waybill}/waybill")]
        public async Task<IServiceResponse<ShipmentDTO>> GetShipment(string waybill)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var shipment = await _service.GetShipment(waybill);
                return new ServiceResponse<ShipmentDTO>
                {
                    Object = shipment
                };
            });
        }

        //[GIGLSActivityAuthorize(Activity = "Delete")]
        //[HttpDelete]
        //[Route("{ShipmentId:int}")]
        //public async Task<IServiceResponse<bool>> DeleteShipment(int ShipmentId)
        //{
        //    return await HandleApiOperationAsync(async () =>
        //    {
        //        await _service.DeleteShipment(ShipmentId);
        //        return new ServiceResponse<bool>
        //        {
        //            Object = true
        //        };
        //    });
        //}


        //[GIGLSActivityAuthorize(Activity = "Delete")]
        //[HttpDelete]
        //[Route("{waybill}/waybill")]
        //public async Task<IServiceResponse<bool>> DeleteShipment(string waybill)
        //{
        //    return await HandleApiOperationAsync(async () =>
        //    {
        //        await _service.DeleteShipment(waybill);
        //        return new ServiceResponse<bool>
        //        {
        //            Object = true
        //        };
        //    });
        //}

        [GIGLSActivityAuthorize(Activity = "Update")]
        [HttpPut]
        [Route("{shipmentId:int}")]
        public async Task<IServiceResponse<bool>> UpdateShipment(int shipmentId, ShipmentDTO ShipmentDTO)
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _service.UpdateShipment(shipmentId, ShipmentDTO);

                return new ServiceResponse<bool>
                {
                    Object = true
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Update")]
        [HttpPost]
        [Route("UpdateShipment")]
        public async Task<IServiceResponse<bool>> UpdateShipment(ShipmentDTO ShipmentDTO)
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _service.UpdateShipment(ShipmentDTO.ShipmentId, ShipmentDTO);

                return new ServiceResponse<bool>
                {
                    Object = true
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Update")]
        [HttpPut]
        [Route("{waybill}")]
        public async Task<IServiceResponse<bool>> UpdateShipment(string waybill, ShipmentDTO ShipmentDTO)
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _service.UpdateShipment(waybill, ShipmentDTO);

                return new ServiceResponse<bool>
                {
                    Object = true
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("ungroupedwaybillsforservicecentre")]
        public async Task<IServiceResponse<IEnumerable<InvoiceViewDTO>>> GetUnGroupedWaybillsForServiceCentre([FromUri] FilterOptionsDto filterOptionsDto)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var shipments = await _service.GetUnGroupedWaybillsForServiceCentre(filterOptionsDto, true);
                return new ServiceResponse<IEnumerable<InvoiceViewDTO>>
                {
                    Object = shipments,
                    Total = shipments.Count
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("ungroupmappingservicecentre")]
        public async Task<IServiceResponse<IEnumerable<ServiceCentreDTO>>> GetUnGroupMappingServiceCentres()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var centres = await _service.GetUnGroupMappingServiceCentres();
                return new ServiceResponse<IEnumerable<ServiceCentreDTO>>
                {
                    Object = centres
                };
            });
        }

        //[GIGLSActivityAuthorize(Activity = "View")]
        //[HttpGet]
        //[Route("unmappedgroupedwaybillsforservicecentre")]
        //public async Task<IServiceResponse<IEnumerable<GroupWaybillNumberMappingDTO>>> GetUnmappedGroupedWaybillsForServiceCentre([FromUri]FilterOptionsDto filterOptionsDto)
        //{
        //    return await HandleApiOperationAsync(async () =>
        //    {
        //        var unmappedGroupWaybills = await _service.GetUnmappedGroupedWaybillsForServiceCentre(filterOptionsDto);
        //        return new ServiceResponse<IEnumerable<GroupWaybillNumberMappingDTO>>
        //        {
        //            Object = unmappedGroupWaybills,
        //            Total = unmappedGroupWaybills.Count
        //        };
        //    });
        //}

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("unmappedgroupedwaybillsforservicecentre")]
        public async Task<IServiceResponse<IEnumerable<GroupWaybillNumberDTO>>> GetUnmappedGroupedWaybillsForServiceCentre([FromUri] FilterOptionsDto filterOptionsDto)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var unmappedGroupWaybills = await _service.GetUnmappedGroupedWaybillsForServiceCentre(filterOptionsDto);
                return new ServiceResponse<IEnumerable<GroupWaybillNumberDTO>>
                {
                    Object = unmappedGroupWaybills,
                    Total = unmappedGroupWaybills.Count
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("manifestFormovementmanifestservicecentre")]
        public async Task<IServiceResponse<IEnumerable<ManifestDTO>>> GetManifestForMovementManifestServiceCentre([FromUri] MovementManifestFilterCriteria filterOptionsDto)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var unmappedGroupWaybills = await _service.GetManifestForMovementManifestServiceCentre(filterOptionsDto);
                return new ServiceResponse<IEnumerable<ManifestDTO>>
                {
                    Object = unmappedGroupWaybills,
                    Total = unmappedGroupWaybills.Count
                };
            });
        }


        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("unmappedmovementmanifestservicecentre")]
        public async Task<IServiceResponse<IEnumerable<ServiceCentreDTO>>> GetUnmappedMovementmanifestservicecentre()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var centres = await _service.GetUnmappedMovementManifestServiceCentres();
                return new ServiceResponse<IEnumerable<ServiceCentreDTO>>
                {
                    Object = centres
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("releaseMovementManifest/{movementmanifestcode}/{code}")]
        public async Task<IServiceResponse<bool>> ReleaseMovementManifest(ReleaseMovementManifestDto movementManifestVals)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var centres = await _service.ReleaseMovementManifest(movementManifestVals);
                return new ServiceResponse<bool>
                {
                    Object = centres
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("checkreleasemovementmanifest/{movementmanifestcode}")]
        public async Task<IServiceResponse<bool>> CheckReleaseManifest(string movementmanifestcode)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var centres = await _service.CheckReleaseMovementManifest(movementmanifestcode);
                return new ServiceResponse<bool>
                {
                    Object = centres
                };
            });
        }


        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("unmappedmanifestforservicecentre")]
        public async Task<IServiceResponse<IEnumerable<ManifestDTO>>> GetUnmappedManifestForServiceCentre(ShipmentCollectionFilterCriteria filterOptionsDto)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var unmappedManifests = await _service.GetUnmappedManifestForServiceCentre(filterOptionsDto);
                return new ServiceResponse<IEnumerable<ManifestDTO>>
                {
                    Object = unmappedManifests,
                    Total = unmappedManifests.Count
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("unmappedmanifestservicecentre")]
        public async Task<IServiceResponse<IEnumerable<ServiceCentreDTO>>> GetUnmappedManifestServiceCentres()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var centres = await _service.GetUnmappedManifestServiceCentres();
                return new ServiceResponse<IEnumerable<ServiceCentreDTO>>
                {
                    Object = centres
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("unmappedmanifestservicecentreforsupermanifest")]
        public async Task<IServiceResponse<IEnumerable<ServiceCentreDTO>>> GetUnmappedManifestServiceCentresForSuperManifest()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var centres = await _service.GetUnmappedManifestServiceCentresForSuperManifest();
                return new ServiceResponse<IEnumerable<ServiceCentreDTO>>
                {
                    Object = centres
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("zone/{destinationServiceCentreId:int}")]
        public async Task<IServiceResponse<DomesticRouteZoneMapDTO>> GetZone(int destinationServiceCentreId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var zone = await _service.GetZone(destinationServiceCentreId);

                return new ServiceResponse<DomesticRouteZoneMapDTO>
                {
                    Object = zone
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("countryzone/{destinationCountryId:int}")]
        public async Task<IServiceResponse<CountryRouteZoneMapDTO>> GetCountryZone(int destinationCountryId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var zone = await _service.GetCountryZone(destinationCountryId);

                return new ServiceResponse<CountryRouteZoneMapDTO>
                {
                    Object = zone
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("dailysales")]
        public async Task<IServiceResponse<DailySalesDTO>> GetDailySales(AccountFilterCriteria accountFilterCriteria)
        {
            return await HandleApiOperationAsync(async () =>
            {

                // string path = "http:/localhost/GIGLS/uploads/giglsdoc.json";

                var dailySales = await _service.GetDailySales(accountFilterCriteria);

                //create daily files and store in a folder
                //if (!File.Exists(path))
                //{
                //    // Create a file to write to.
                //    var createText = dailySales.Invoices ;
                //    string json = JsonConvert.SerializeObject(createText);
                //    File.WriteAllText(path, json);
                //}

                return new ServiceResponse<DailySalesDTO>
                {
                    Object = dailySales
                };
            });
        }


        //This is use to solve time out from service centre
        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("dailysalesforservicecentre")]
        public async Task<IServiceResponse<DailySalesDTO>> GetSalesForServiceCentre(AccountFilterCriteria accountFilterCriteria)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var dailySales = await _service.GetSalesForServiceCentre(accountFilterCriteria);

                return new ServiceResponse<DailySalesDTO>
                {
                    Object = dailySales
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("dailysalesbyservicecentre")]
        public async Task<IServiceResponse<DailySalesDTO>> GetDailySalesByServiceCentre(AccountFilterCriteria accountFilterCriteria)
        {
            return await HandleApiOperationAsync(async () =>
            {

                //string path = "http:/localhost/GIGLS/uploads/giglsdoc.json";

                var dailySalesByServiceCentre = await _service.GetDailySalesByServiceCentre(accountFilterCriteria);

                var reportObject = await _reportService.GetDailySalesByServiceCentreReport(dailySalesByServiceCentre);

                //create daily files and store in a folder
                //if (!File.Exists(path))
                //{
                //    // Create a file to write to.
                //    var createText = dailySales.Invoices;
                //    string json = JsonConvert.SerializeObject(createText);
                //    File.WriteAllText(path, json);
                //}

                dailySalesByServiceCentre.Filename = (string)reportObject;

                return new ServiceResponse<DailySalesDTO>
                {
                    Object = dailySalesByServiceCentre
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("{waybill}/waybillbyservicecentre")]
        public async Task<IServiceResponse<DailySalesDTO>> GetDailySaleByWaybillForServiceCentre(string waybill)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var shipment = await _service.GetWaybillForServiceCentre(waybill);
                return new ServiceResponse<DailySalesDTO>
                {
                    Object = shipment
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("warehouseservicecentre")]
        public async Task<IServiceResponse<IEnumerable<ServiceCentreDTO>>> GetAllWarehouseServiceCenters()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var centres = await _service.GetAllWarehouseServiceCenters();
                return new ServiceResponse<IEnumerable<ServiceCentreDTO>>
                {
                    Object = centres
                };
            });
        }

        // Shipment delivery monitor
        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("GetShipmentCreatedSummaryMonitor")]
        public async Task<IServiceResponse<System.Web.Mvc.JsonResult>> GetShipmentCreatedSummaryMonitor()
        {
            return await HandleApiOperationAsync(async () =>
            {

                var today = DateTime.Now.Date;
                var firstDayOfMonth = today.AddDays(-7);

                var accountFilterCriteria = new AccountFilterCriteria
                {
                    StartDate = firstDayOfMonth,
                    EndDate = today.AddDays(1)
                };

                var results = await _service.GetShipmentMonitor(accountFilterCriteria);

                return new ServiceResponse<System.Web.Mvc.JsonResult>()
                {
                    Object = new System.Web.Mvc.JsonResult { Data = results.totalZones, JsonRequestBehavior = System.Web.Mvc.JsonRequestBehavior.AllowGet }
                };
            });
        }

        // Shipment monitor
        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("GetShipmentCreatedByDateMonitor")]
        public async Task<IServiceResponse<System.Web.Mvc.JsonResult>> GetShipmentCreatedByDateMonitor(int limitStart, int limitEnd)
        {
            return await HandleApiOperationAsync(async () =>
            {

                var today = DateTime.Now.Date;
                var firstDayOfMonth = today.AddDays(-7);

                var accountFilterCriteria = new AccountFilterCriteria
                {
                    StartDate = firstDayOfMonth,
                    EndDate = today.AddDays(1)
                };

                var limitdates = new LimitDates
                {
                    StartLimit = limitStart,
                    EndLimit = limitEnd
                };

                var chartData = await _service.GetShipmentCreatedByDateMonitor(accountFilterCriteria, limitdates);

                return new ServiceResponse<System.Web.Mvc.JsonResult>()
                {
                    Object = new System.Web.Mvc.JsonResult { Data = chartData, JsonRequestBehavior = System.Web.Mvc.JsonRequestBehavior.AllowGet }
                };
            });
        }

        // Shipment monitor
        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("GetShipmentWaybillsByDateMonitor")]
        public async Task<IServiceResponse<System.Web.Mvc.JsonResult>> GetShipmentWaybillsByDateMonitor(int limitStart, int limitEnd, string scname)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var today = DateTime.Now.Date;
                var firstDayOfMonth = today.AddDays(-7);

                var accountFilterCriteria = new AccountFilterCriteria
                {
                    StartDate = firstDayOfMonth,
                    EndDate = today.AddDays(1)
                };

                var limitdates = new LimitDates
                {
                    StartLimit = limitStart,
                    EndLimit = limitEnd,
                    ScName = scname
                };

                var chartData = await _service.GetShipmentWaybillsByDateMonitor(accountFilterCriteria, limitdates);

                return new ServiceResponse<System.Web.Mvc.JsonResult>()
                {
                    Object = new System.Web.Mvc.JsonResult { Data = chartData, JsonRequestBehavior = System.Web.Mvc.JsonRequestBehavior.AllowGet }
                };
            });
        }


        // Shipment delivery monitor
        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("GetShipmentCreatedSummaryMonitorx")]
        public async Task<IServiceResponse<System.Web.Mvc.JsonResult>> GetShipmentCreatedSummaryMonitorx()
        {
            return await HandleApiOperationAsync(async () =>
            {

                var today = DateTime.Now.Date;
                var firstDayOfMonth = today.AddDays(-7);

                var accountFilterCriteria = new AccountFilterCriteria
                {
                    StartDate = firstDayOfMonth,
                    EndDate = today.AddDays(1)
                };

                var results = await _service.GetShipmentMonitorx(accountFilterCriteria);

                return new ServiceResponse<System.Web.Mvc.JsonResult>()
                {
                    Object = new System.Web.Mvc.JsonResult { Data = results.totalZones, JsonRequestBehavior = System.Web.Mvc.JsonRequestBehavior.AllowGet }
                };
            });
        }

        // Shipment monitor
        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("GetShipmentCreatedByDateMonitorx")]
        public async Task<IServiceResponse<System.Web.Mvc.JsonResult>> GetShipmentCreatedByDateMonitorx(int limitStart, int limitEnd)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var today = DateTime.Now.Date;
                var firstDayOfMonth = today.AddDays(-7);

                var accountFilterCriteria = new AccountFilterCriteria
                {
                    StartDate = firstDayOfMonth,
                    EndDate = today.AddDays(1)
                };

                var limitdates = new LimitDates
                {
                    StartLimit = limitStart,
                    EndLimit = limitEnd
                };

                var chartData = await _service.GetShipmentCreatedByDateMonitorx(accountFilterCriteria, limitdates);

                return new ServiceResponse<System.Web.Mvc.JsonResult>()
                {
                    Object = new System.Web.Mvc.JsonResult { Data = chartData, JsonRequestBehavior = System.Web.Mvc.JsonRequestBehavior.AllowGet }
                };
            });
        }

        // Shipment monitor
        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("GetShipmentWaybillsByDateMonitorx")]
        public async Task<IServiceResponse<System.Web.Mvc.JsonResult>> GetShipmentWaybillsByDateMonitorx(int limitStart, int limitEnd, string scname = null)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var today = DateTime.Now.Date;
                var firstDayOfMonth = today.AddDays(-7);

                var accountFilterCriteria = new AccountFilterCriteria
                {
                    StartDate = firstDayOfMonth,
                    EndDate = today.AddDays(1)
                };

                var limitdates = new LimitDates
                {
                    StartLimit = limitStart,
                    EndLimit = limitEnd,
                    ScName = scname
                };

                var chartData = await _service.GetShipmentWaybillsByDateMonitorx(accountFilterCriteria, limitdates);

                return new ServiceResponse<System.Web.Mvc.JsonResult>()
                {
                    Object = new System.Web.Mvc.JsonResult { Data = chartData, JsonRequestBehavior = System.Web.Mvc.JsonRequestBehavior.AllowGet }
                };
            });
        }

        //[GIGLSActivityAuthorize(Activity = "View")]
        //[HttpGet]
        //[Route("{code}/preshipment")]
        //public async Task<IServiceResponse<PreShipmentDTO>> GetTempShipment(string code)
        //{
        //    return await HandleApiOperationAsync(async () =>
        //    {
        //        var shipment = await _service.GetTempShipment(code);
        //        return new ServiceResponse<PreShipmentDTO>
        //        {
        //            Object = shipment
        //        };
        //    });
        //}

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("{code}/preshipment")]
        public async Task<IServiceResponse<ShipmentDTO>> GetDropOffShipment(string code)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var shipment = await _service.GetDropOffShipmentForProcessing(code);
                return new ServiceResponse<ShipmentDTO>
                {
                    Object = shipment
                };
            });
        }

        [HttpPut]
        [Route("getdropoffsbyphonenooruserchanelcode")]
        public async Task<IServiceResponse<List<PreShipmentDTO>>> GetDropOffsForUserByUserCodeOrPhoneNo(SearchOption searchOption)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var preshipment = await _preshipmentService.GetDropOffsForUserByUserCodeOrPhoneNo(searchOption);
                return new ServiceResponse<List<PreShipmentDTO>>
                {
                    Object = preshipment
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("getgiggoprice")]
        public async Task<IServiceResponse<MobilePriceDTO>> GetGIGGoPrice(PreShipmentMobileDTO preshipmentMobile)
        {
            return await HandleApiOperationAsync(async () =>
            {
                preshipmentMobile.IsFromAgility = true;
                var price = await _service.GetGIGGOPrice(preshipmentMobile);

                return new ServiceResponse<MobilePriceDTO>
                {
                    Object = price,
                };
            });
        }

        [HttpPost]
        [Route("shipmentcontact")]
        public async Task<IServiceResponse<List<ShipmentContactDTO>>> GetShipmentContacts(ShipmentContactFilterCriteria baseFilterCriteria)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var shipmentcontacts = await _shipmentContactService.GetShipmentContact(baseFilterCriteria);
                return new ServiceResponse<List<ShipmentContactDTO>>
                {
                    Object = shipmentcontacts
                };
            });
        }

        [HttpPut]
        [Route("updateshipmentcontact")]
        public async Task<IServiceResponse<bool>> AddOrUpdateContact(ShipmentContactDTO shipmentContactDTO)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var shipmentcontact = await _shipmentContactService.AddOrUpdateShipmentContactAndHistory(shipmentContactDTO);
                return new ServiceResponse<bool>
                {
                    Object = shipmentcontact
                };
            });
        }

        [HttpGet]
        [Route("getshipmentcontacthistory/{waybill}")]
        public async Task<IServiceResponse<List<ShipmentContactHistoryDTO>>> GetShipmentContactHistoryByWaybill(string waybill)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var history = await _shipmentContactService.GetShipmentContactHistoryByWaybill(waybill);
                return new ServiceResponse<List<ShipmentContactHistoryDTO>>
                {
                    Object = history
                };
            });
        }

        [HttpPost]
        [Route("getcodshipments")]
        public async Task<IServiceResponse<List<CODShipmentDTO>>> GetCODShipments(BaseFilterCriteria baseFilterCriteria)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var codShipments = await _service.GetCODShipments(baseFilterCriteria);
                return new ServiceResponse<List<CODShipmentDTO>>
                {
                    Object = codShipments
                };
            });
        }

        [HttpPost]
        [Route("getmagayashipmentstocargo")]
        public async Task<IServiceResponse<List<CargoMagayaShipmentDTO>>> GetCargoMagayaShipments(BaseFilterCriteria baseFilterCriteria)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var codShipments = await _service.GetCargoMagayaShipments(baseFilterCriteria);
                return new ServiceResponse<List<CargoMagayaShipmentDTO>>
                {
                    Object = codShipments
                };
            });
        }

        [HttpPost]
        [Route("cargomagayashipments")]
        public async Task<IServiceResponse<bool>> GetCargoMagayaShipments(List<CargoMagayaShipmentDTO> cargoMagayaShipmentDTOs)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var shipmentCargoed = await _service.MarkMagayaShipmentsAsCargoed(cargoMagayaShipmentDTOs);
                return new ServiceResponse<bool>
                {
                    Object = shipmentCargoed
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Create")]
        [HttpPost]
        [Route("addinternationalshipment")]
        public async Task<IServiceResponse<ShipmentDTO>> AddInternationalShipment(InternationalShipmentDTO shipmentDTO)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var shipment = await _service.AddInternationalShipment(shipmentDTO);
                return new ServiceResponse<ShipmentDTO>
                {
                    Object = shipment
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Create")]
        [HttpPost]
        [Route("getinternationalprice")]
        public async Task<IServiceResponse<List<TotalNetResult>>> GetInternationalShipmentPrice(InternationalShipmentDTO shipmentDTO)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var shipment = await _service.GetInternationalShipmentPrice(shipmentDTO);
                return new ServiceResponse<List<TotalNetResult>>
                {
                    Object = shipment
                };
            });
        }

        [HttpPost]
        [Route("ProcessIntlShipmentTransactions")]
        public async Task<IServiceResponse<object>> ProcessIntlShipment(ShipmentDTO shipmentDTO)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _service.ProcessInternationalShipmentOnAgility(shipmentDTO);
                return new ServiceResponse<object>
                {
                    Object = result
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("generalpayment")]
        public async Task<IServiceResponse<bool>> ProcessGeneralPaymentLinksForShipmentsOnAgility(GeneralPaymentDTO paymentDTO)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _service.ProcessGeneralPaymentLinksForShipmentsOnAgility(paymentDTO);

                return new ServiceResponse<bool>
                {
                    Object = result
                };
            });
        }

        [HttpPost]
        [Route("payforwaybillbywallet")]
        public async Task<IServiceResponse<object>> PayForWaybillByWallet(ShipmentPaymentDTO paymentDTO)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _service.PayForWaybillByWallet(paymentDTO);
                return new ServiceResponse<object>
                {
                    Object = result
                };
            });
        }

        [HttpPost]
        [Route("getshipmentsforexport")]
        public async Task<IServiceResponse<List<ShipmentExportDTO>>> GetShipmentExportNotYetExported(NewFilterOptionsDto filter)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var shipment = await _service.GetShipmentExportNotYetExported(filter);
                return new ServiceResponse<List<ShipmentExportDTO>>
                {
                    Object = shipment
                };
            });
        }

        [HttpPost]
        [Route("markshipmentsreadyforexport")]
        public async Task<IServiceResponse<bool>> MarkShipmentsReadyForExport(List<InvoiceViewDTO> shipments)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _service.MarkShipmentsReadyForExport(shipments);
                return new ServiceResponse<bool>
                {
                    Object = result
                };
            });
        }

        [HttpPost]
        [Route("exportshipments")]
        public async Task<IServiceResponse<bool>> ExportShipments(InternationalCargoManifestDTO shipment)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _service.ExportFlightManifest(shipment);
                return new ServiceResponse<bool>
                {
                    Object = result
                };
            });
        }

        [HttpPost]
        [Route("getintlcargomanifest")]
        public async Task<IServiceResponse<List<InternationalCargoManifestDTO>>> GetIntlCargoManifests(NewFilterOptionsDto filter)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var shipment = await _service.GetIntlCargoManifests(filter);
                return new ServiceResponse<List<InternationalCargoManifestDTO>>
                {
                    Object = shipment
                };
            });
        }

        [HttpGet]
        [Route("getintlcargomanifestbyid/{cargoID:int}")]
        public async Task<IServiceResponse<InternationalCargoManifestDTO>> GetShipmentContactHistoryByWaybill(int cargoID)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var item = await _service.GetIntlCargoManifestByID(cargoID);
                return new ServiceResponse<InternationalCargoManifestDTO>
                {
                    Object = item
                };
            });
        }

        [HttpGet]
        [Route("zonestation/{stationId:int}")]
        public async Task<IServiceResponse<DomesticRouteZoneMapDTO>> GetZoneByStation(int stationId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var zone = await _service.GetZoneByStation(stationId);

                return new ServiceResponse<DomesticRouteZoneMapDTO>
                {
                    Object = zone
                };
            });
        }

        [HttpPost]
        [Route("getunIdentifiedintlshipments")]
        public async Task<IServiceResponse<List<UnidentifiedItemsForInternationalShippingDTO>>> GetUnIdentifiedIntlShipments(NewFilterOptionsDto filter)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var shipment = await _service.GetUnIdentifiedIntlShipments(filter);
                return new ServiceResponse<List<UnidentifiedItemsForInternationalShippingDTO>>
                {
                    Object = shipment
                };
            });
        }

        [HttpPost]
        [Route("addunIdentifiedintlshipments")]
        public async Task<IServiceResponse<bool>> AddUnIdentifiedIntlShipments(List<UnidentifiedItemsForInternationalShippingDTO> shipments)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _service.AddUnIdentifiedIntlShipments(shipments);
                return new ServiceResponse<bool>
                {
                    Object = result
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("unidentifieditem/{ItemID:int}")]
        public async Task<IServiceResponse<UnidentifiedItemsForInternationalShippingDTO>> GetUnIdentifiedIntlShipmentByID(int ItemID)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var shipment = await _service.GetUnIdentifiedIntlShipmentByID(ItemID);
                return new ServiceResponse<UnidentifiedItemsForInternationalShippingDTO>
                {
                    Object = shipment
                };
            });
        }

        [HttpPost]
        [Route("getshipmentreceiverdetails")]
        public async Task<IServiceResponse<ReceiverDetailDTO>> GetShipmentReceiverDetails(NewFilterOptionsDto newFilterOptionsDto)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _service.GetShipmentReceiverDetails(newFilterOptionsDto);
                return new ServiceResponse<ReceiverDetailDTO>
                {
                    Object = result
                };
            });
        }

        [HttpPost]
        [Route("getaddressdetails")]
        public async Task<IServiceResponse<GoogleAddressDTO>> GetGoogleAddressDetails(GoogleAddressDTO location)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _service.GetGoogleAddressDetails(location);

                return new ServiceResponse<GoogleAddressDTO>
                {
                    Object = result
                };
            });
        }

        [HttpGet]
        [Route("getdelayeddelivery/{serviceCentreId}")]
        public Task<IServiceResponse<List<DelayedDeliveryDTO>>> GetDelayedDelivery(int serviceCentreId)
        {
            return HandleApiOperationAsync(async () =>
            {
                var result = await _service.GetEcommerceDelayedDeliveryShipment(serviceCentreId);
                return new ServiceResponse<List<DelayedDeliveryDTO>>
                {
                    Object = await Task.FromResult(result)
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("gethubshipmentdeliveryreport/{from}/{to}")]
        public async Task<IServiceResponse<ShipmentDeliveryReportForHubRepsDTO>> GetHupShipmentDeliveryReport(DateTime from, DateTime to)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _service.GetHubShipmentDeliveryReport(from, to);
                return new ServiceResponse<ShipmentDeliveryReportForHubRepsDTO>()
                {
                    Object = await Task.FromResult(result)
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("gatewayactivity")]
        public async Task<IServiceResponse<List<GatewatActivityDTO>>> GatewayActivity(BaseFilterCriteria FilterCriteria)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var shipments = await _service.GatewayActivity(FilterCriteria);

                return new ServiceResponse<List<GatewatActivityDTO>>
                {
                    Object = shipments
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("ecommercereport")]
        public Task<IServiceResponse<List<EcommerceShipmentSummaryReportDTO>>> GetEcommerceShipmentSummaryReport(EcommerceShipmentSummaryFilterCriteria filter)
        {
            return HandleApiOperationAsync(async () =>
            {
                var report = await _service.EcommerceShipmentSummaryReport(filter);

                return new ServiceResponse<List<EcommerceShipmentSummaryReportDTO>>
                {
                    Object = report
                };
            });
        }

        [HttpGet]
        [Route("confirmcodtransferstatus")]
        public async Task<IServiceResponse<string>> ConfirmCODTransferStatus([FromUri] string waybill)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _service.ValidateCODPayment(waybill);
                return new ServiceResponse<string>
                {
                    Object = result
                };
            });
        }

        [HttpGet]
        [Route("getcodshipmentbywaybill/{waybill}")]
        public async Task<IServiceResponse<List<CODShipmentDTO>>> GetCODShipmentByWaybill(string waybill)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var codShipments = await _service.GetCODShipmentByWaybill(waybill);
                return new ServiceResponse<List<CODShipmentDTO>>
                {
                    Object = codShipments
                };
            });
        }

        [HttpGet]
        [Route("checktransferstatusforeca")]
        public async Task<IServiceResponse<string>> CheckTransferStatusForECA([FromUri] string waybill)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _service.CheckTransferStatusForECA(waybill);
                return new ServiceResponse<string>
                {
                    Object = result
                };
            });
        }

        [HttpPost]
        [Route("allcodshipment")]
        public async Task<IServiceResponse<AllCODShipmentDTO>> GetAllCODShipmentsAgilityReport(PaginationDTO dto)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var res = await _service.GetAllCODShipmentsAgilityReport(dto);
                return new ServiceResponse<AllCODShipmentDTO>
                {
                    Object = res,
                };
            });
        }

        [Authorize(Roles = "Account,Shipment, ViewAdmin")]
        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("codsettlement")]
        public async Task<IServiceResponse<AllCODShipmentDTO>> CellulantShipmentCollectionReport(PaginationDTO dto)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var res = await _service.CellulantShipmentCollectionReport(dto);
                return new ServiceResponse<AllCODShipmentDTO>
                {
                    Object = res,
                };
            });
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("verifypayment/{waybill}")]
        public async Task<IServiceResponse<string>> VerifyPayment(string waybill)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var status = await _service.VerifyPayment(waybill);
                return new ServiceResponse<string>
                {
                    Object = status
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Create")]
        [HttpPost]
        [Route("updateinternationalshipment")]
        public async Task<IServiceResponse<ShipmentDTO>> UpdateInternationalShipment(InternationalShipmentDTO shipmentDTO)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var shipment = await _service.UpdateInternationalShipment(shipmentDTO);
                return new ServiceResponse<ShipmentDTO>
                {
                    Object = shipment
                };
            });
        }

        [HttpPost]
        [Route("supportutility")]
        public async Task<IServiceResponse<bool>> UtilitiesForSupport(SupportDTO request)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var shipment = await _service.UtilitiesForSupport(request);
                return new ServiceResponse<bool>
                {
                    Object = shipment
                };
            });
        }
    }
}
