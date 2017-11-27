﻿using GIGLS.Core;
using GIGLS.CORE.IServices.Report;
using System.Collections.Generic;
using System.Threading.Tasks;
using GIGLS.Core.DTO.Shipments;
using GIGLS.CORE.DTO.Report;
using GIGLS.Core.IServices.User;

namespace GIGLS.Services.Implementation.Report
{
    public class ShipmentReportService : IShipmentReportService
    {
        private readonly IUnitOfWork _uow;
        private readonly IUserService _userService;

        public ShipmentReportService(IUnitOfWork uow, IUserService userService)
        {
            _uow = uow;
            _userService = userService;
        }

        public async Task<List<ShipmentDTO>> GetShipments(ShipmentFilterCriteria filterCriteria)
        {
            var serviceCenters = _userService.GetPriviledgeServiceCenters().Result;
            return await _uow.Shipment.GetShipments(filterCriteria, serviceCenters);
        }

        public async Task<List<ShipmentDTO>> GetTodayShipments()
        {
            ShipmentFilterCriteria filterCriteria = new ShipmentFilterCriteria
            {
                StartDate = System.DateTime.Today
            };

            var serviceCenters = _userService.GetPriviledgeServiceCenters().Result;
            return await _uow.Shipment.GetShipments(filterCriteria, serviceCenters);
        }
    }
}
