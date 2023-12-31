﻿using GIGL.POST.Core.Domain;
using POST.Core.DTO.Shipments;
using POST.Core.DTO.ShipmentScan;
using POST.Core.IRepositories.Shipments;
using POST.Core.View;
using POST.CORE.DTO.Report;
using POST.Infrastructure.Persistence;
using POST.Infrastructure.Persistence.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace POST.INFRASTRUCTURE.Persistence.Repositories.Shipments
{
    public class ShipmentTrackingRepository : Repository<ShipmentTracking, GIGLSContext>, IShipmentTrackingRepository
    {
        private GIGLSContext _context;
        private GIGLSContextForView _GIGLSContextForView;
        public ShipmentTrackingRepository(GIGLSContext context)
            : base(context)
        {
            _context = context;
            _GIGLSContextForView = new GIGLSContextForView();
        }

        public Task<List<ShipmentTrackingDTO>> GetShipmentTrackingsAsync()
        {
            try
            {
                var shipmentTrackings = Context.ShipmentTracking;

                var shipmentTrackingDto = from shipmentTracking in shipmentTrackings
                                          select new ShipmentTrackingDTO
                                          {
                                              DateTime = shipmentTracking.DateTime,
                                              Location = shipmentTracking.Location,
                                              Waybill = shipmentTracking.Waybill,
                                              ShipmentTrackingId = shipmentTracking.ShipmentTrackingId,
                                              TrackingType = shipmentTracking.TrackingType,
                                              User = shipmentTracking.User.FirstName + " " + shipmentTracking.User.LastName,
                                              Status = shipmentTracking.Status,
                                              ServiceCentreId = shipmentTracking.ServiceCentreId,
                                              ShipmentCancel = Context.ShipmentCancel.Where(c => c.Waybill == shipmentTracking.Waybill).Select(x => new ShipmentCancelDTO
                                              {
                                                  CancelReason = x.CancelReason
                                              }).FirstOrDefault(),
                                              ShipmentReroute = Context.ShipmentReroute.Where(c => c.WaybillOld == shipmentTracking.Waybill).Select(x => new ShipmentRerouteDTO
                                              {
                                                  RerouteReason = x.RerouteReason
                                              }).FirstOrDefault(),
                                          };
                return Task.FromResult(shipmentTrackingDto.ToList());
            }
            catch (Exception)
            {

                throw;
            }

        }

        public Task<List<ShipmentTrackingDTO>> GetShipmentTrackingsAsync(string waybill)
        {
            try
            {
                var shipmentTrackings = Context.ShipmentTracking.Where(x => x.Waybill == waybill);

                var shipmentTrackingDto = from shipmentTracking in shipmentTrackings
                                          select new ShipmentTrackingDTO
                                          {
                                              DateTime = shipmentTracking.DateTime,
                                              Location = shipmentTracking.Location,
                                              Waybill = shipmentTracking.Waybill,
                                              ShipmentTrackingId = shipmentTracking.ShipmentTrackingId,
                                              TrackingType = shipmentTracking.TrackingType,
                                              User = shipmentTracking.User.FirstName + " " + shipmentTracking.User.LastName,
                                              Status = shipmentTracking.Status,
                                              ServiceCentreId = shipmentTracking.ServiceCentreId,
                                              ScanStatus = Context.ScanStatus.Where(c => c.Code == shipmentTracking.Status).Select(x => new ScanStatusDTO
                                              {
                                                  Code = x.Code,
                                                  Incident = x.Incident,
                                                  Reason = x.Reason,
                                                  Comment = x.Comment
                                              }).FirstOrDefault(),
                                              ShipmentCancel = Context.ShipmentCancel.Where(c => c.Waybill == shipmentTracking.Waybill).Select(x => new ShipmentCancelDTO
                                              {
                                                  CancelReason = x.CancelReason
                                              }).FirstOrDefault(),
                                              ShipmentReroute = Context.ShipmentReroute.Where(c => c.WaybillOld == shipmentTracking.Waybill || c.WaybillNew == shipmentTracking.Waybill ) .Select(x => new ShipmentRerouteDTO
                                              {
                                                  RerouteReason = x.RerouteReason
                                              }).FirstOrDefault(),
                                          };
                return Task.FromResult(shipmentTrackingDto.ToList().OrderByDescending(x => x.DateTime).ToList());
            }
            catch (Exception)
            {
                throw;
            }

        }

        public IQueryable<ShipmentTrackingView> GetShipmentTrackingsFromViewAsync(ScanTrackFilterCriteria f_Criteria)
        {
            var scanTrackingView = _GIGLSContextForView.ScanTrackingView.AsQueryable();
            if(f_Criteria != null)
            {
                scanTrackingView = f_Criteria.GetQueryFromParameters(scanTrackingView);
            }
            return scanTrackingView;
        }


        public IQueryable<ShipmentTracking> GetShipmentTrackingsAsync(ScanTrackFilterCriteria f_Criteria)
        {
            var scanTracking = _context.ShipmentTracking.AsQueryable();
            if (f_Criteria != null)
            {
                scanTracking = f_Criteria.GetQueryFromParameters(scanTracking);
            }
            return scanTracking;
        }


        public Task<List<ShipmentTrackingDTO>> GetShipmentTrackingsForMobileAsync(string waybill)
        {
            try
            {
                var shipmentTrackings = Context.ShipmentTracking.Where(x => x.Waybill == waybill);

                var shipmentTrackingDto = from shipmentTracking in shipmentTrackings
                                          select new ShipmentTrackingDTO
                                          {
                                              DateTime = shipmentTracking.DateTime,
                                              Location = shipmentTracking.Location,
                                              Waybill = shipmentTracking.Waybill,
                                              ShipmentTrackingId = shipmentTracking.ShipmentTrackingId,
                                              TrackingType = shipmentTracking.TrackingType,
                                              User = shipmentTracking.User.FirstName + " " + shipmentTracking.User.LastName,
                                              Status = shipmentTracking.Status,
                                              ServiceCentreId = shipmentTracking.ServiceCentreId,
                                              ScanStatus = Context.ScanStatus.Where(c => c.Code == shipmentTracking.Status).Select(x => new ScanStatusDTO
                                              {
                                                  Code = x.Code,
                                                  Incident = x.Incident,
                                                  Reason = x.Reason,
                                                  Comment = x.Comment
                                              }).FirstOrDefault()
                                          };
                return Task.FromResult(shipmentTrackingDto.ToList());
            }
            catch (Exception)
            {
                throw;
            }

        }

        public Shipment GetShipmentByWayBill(string waybill)
        {
            try
            {
                return  _context.Shipment.FirstOrDefault(x => x.Waybill == waybill);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
