﻿using POST.Core.DTO.Report;
using POST.Core.DTO.ServiceCentres;
using POST.Core.Enums;
using POST.CORE.Domain;
using POST.CORE.DTO.Shipments;
using POST.CORE.IRepositories.Shipments;
using POST.Infrastructure.Persistence;
using POST.Infrastructure.Persistence.Repository;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace POST.INFRASTRUCTURE.Persistence.Repositories.Shipments
{
    public class ShipmentCollectionRepository : Repository<ShipmentCollection, GIGLSContext>, IShipmentCollectionRepository
    {
        private GIGLSContext _context;
        public ShipmentCollectionRepository(GIGLSContext context) : base(context)
        {
            _context = context;
        }


        public IQueryable<ShipmentCollectionDTO> ShipmentCollectionsForEcommerceAsQueryable(bool isEcommerce)
        {
            var shipmentCollectionAsQueryable = (from shipmentCollection in _context.ShipmentCollection
                                                 join shipment in _context.Shipment on
                                                 new { shipmentCollection.Waybill, Key2 = isEcommerce } equals
                                                 new { shipment.Waybill, Key2 = (shipment.CompanyType == "Ecommerce") }
                                                 select new ShipmentCollectionDTO()
                                                 {
                                                     Waybill = shipmentCollection.Waybill,
                                                     Name = shipmentCollection.Name,
                                                     PhoneNumber = shipmentCollection.PhoneNumber,
                                                     Email = shipmentCollection.Email,
                                                     Address = shipmentCollection.Address,
                                                     City = shipmentCollection.City,
                                                     State = shipmentCollection.State,
                                                     IndentificationUrl = shipmentCollection.IndentificationUrl,
                                                     ShipmentScanStatus = shipmentCollection.ShipmentScanStatus,
                                                     UserId = shipmentCollection.UserId,
                                                     DateCreated = shipmentCollection.DateCreated,
                                                     DestinationServiceCentreId = shipmentCollection.DestinationServiceCentreId,
                                                     OriginalDepartureServiceCentre = Context.ServiceCentre.Where(c => c.ServiceCentreId == shipment.DepartureServiceCentreId).Select(x => new ServiceCentreDTO
                                                     {
                                                         Code = x.Code,
                                                         Name = x.Name
                                                     }).FirstOrDefault(),

                                                     OriginalDestinationServiceCentre = Context.ServiceCentre.Where(c => c.ServiceCentreId == shipment.DestinationServiceCentreId).Select(x => new ServiceCentreDTO
                                                     {
                                                         Code = x.Code,
                                                         Name = x.Name
                                                     }).FirstOrDefault()
                                                 });


            return shipmentCollectionAsQueryable;
        }


        public async Task<List<ShipmentCollectionForContactDTO>> GetShipmentCollectionForContact(ShipmentContactFilterCriteria baseFilterCriteria)
        {
            try
            {
                var queryDate = baseFilterCriteria.getStartDateAndEndDate();
                var startDate1 = queryDate.Item1;
                var endDate1 = queryDate.Item2;

                //declare parameters for the stored procedure
                SqlParameter startDate = new SqlParameter("@StartDate", startDate1);
                SqlParameter endDate = new SqlParameter("@EndDate", endDate1);
                SqlParameter serviceCentreId = new SqlParameter("@ServiceCentreId", baseFilterCriteria.ServiceCentreId);
                SqlParameter scanStatus = new SqlParameter("@ScanStatus",ShipmentScanStatus.ARF);

                SqlParameter[] param = new SqlParameter[]
                {
                    serviceCentreId,
                    startDate,
                    endDate,
                    scanStatus
                };

                var result =  _context.Database.SqlQuery<ShipmentCollectionForContactDTO>("ShipmentCollectionForContacts " +
                   "@ServiceCentreId,@StartDate, @EndDate, @ScanStatus",
                   param).ToList();


                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public async Task<List<ShipmentCollectionDTOForArrived>> GetArrivedShipmentCollection(ShipmentContactFilterCriteria baseFilterCriteria)
        {
            try
            {
                var queryDate = baseFilterCriteria.getStartDateAndEndDate();
                var startDate1 = queryDate.Item1;
                var endDate1 = queryDate.Item2;

                //declare parameters for the stored procedure
                SqlParameter startDate = new SqlParameter("@StartDate", startDate1);
                SqlParameter endDate = new SqlParameter("@EndDate", endDate1);
                SqlParameter destinationCentreId = new SqlParameter("@destinationCentreId", baseFilterCriteria.DestinationServiceCentreId);
                SqlParameter departureCentreId = new SqlParameter("@departureCentreId", baseFilterCriteria.DepartureServiceCentreId);

                SqlParameter[] param = new SqlParameter[]
                {
                    startDate,
                    endDate,
                    destinationCentreId,
                    departureCentreId
                };

                var result = _context.Database.SqlQuery<ShipmentCollectionDTOForArrived>("ArrivedShipment " +
                   "@StartDate, @EndDate, @destinationCentreId, @departureCentreId",
                   param).ToList();


                return result;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


    }
}
