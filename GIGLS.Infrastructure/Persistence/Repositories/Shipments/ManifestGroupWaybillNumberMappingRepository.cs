﻿using GIGLS.Core.IRepositories.Shipments;
using GIGLS.Core.Domain;
using GIGLS.Infrastructure.Persistence;
using GIGLS.Infrastructure.Persistence.Repository;
using GIGLS.Core.DTO.Shipments;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GIGLS.INFRASTRUCTURE.Persistence.Repositories.Shipments
{
    public class ManifestGroupWaybillNumberMappingRepository : Repository<ManifestGroupWaybillNumberMapping, GIGLSContext>, IManifestGroupWaybillNumberMappingRepository
    {
        private GIGLSContext _context;
        public ManifestGroupWaybillNumberMappingRepository(GIGLSContext context)
            : base(context)
        {
            _context = context;
        }

        public async Task<List<ManifestGroupWaybillNumberMappingDTO>> GetManifestGroupWaybillNumberMappings(int[] serviceCentreIds)
        {
            var manifestGroupwaybillMapping = Context.ManifestGroupWaybillNumberMapping.Where(s => s.IsDeleted == false).AsQueryable();

            var serviceCentreGroupWaybills = new List<string>();
            if (serviceCentreIds.Length > 0)
            {
                serviceCentreGroupWaybills = _context.GroupWaybillNumberMapping.Where(s => serviceCentreIds.Contains(s.DepartureServiceCentreId)).
                    Select(s => s.GroupWaybillNumber).ToList();
                manifestGroupwaybillMapping = manifestGroupwaybillMapping.Where(s => serviceCentreGroupWaybills.Contains(s.GroupWaybillNumber));
            }

            var manifestGroupwaybillMappingDTO = from mgw in manifestGroupwaybillMapping
                                                 select new ManifestGroupWaybillNumberMappingDTO
                                                 {
                                                     ManifestGroupWaybillNumberMappingId = mgw.ManifestGroupWaybillNumberMappingId,
                                                     ManifestCode = mgw.ManifestCode,
                                                     GroupWaybillNumber = mgw.GroupWaybillNumber,
                                                     IsActive = mgw.IsActive,
                                                     DateMapped = mgw.DateMapped,
                                                     DateCreated = mgw.DateCreated,
                                                     DateModified = mgw.DateModified,
                                                     IsDeleted = mgw.IsDeleted,
                                                     RowVersion = mgw.RowVersion,
                                                     ManifestDetails = Context.Manifest.Where(x => x.ManifestCode == mgw.ManifestCode).
                                                     Select(p => new ManifestDTO {
                                                         DateCreated = p.DateCreated,
                                                         DateModified = p.DateModified,
                                                         ManifestCode = p.ManifestCode,
                                                         ManifestType = p.ManifestType,
                                                         DateTime = p.DateTime,
                                                         IsDispatched = p.IsDispatched,
                                                         IsReceived = p.IsReceived,
                                                         DispatchedBy = Context.Users.Where(d => d.Id == p.DispatchedById).Select(x => x.LastName + " " + x.FirstName).FirstOrDefault(),
                                                         ReceiverBy  = Context.Users.Where(r => r.Id == p.ReceiverById).Select(x => x.LastName + " " + x.FirstName).FirstOrDefault()       
                                                     }).FirstOrDefault()
                                                 };

            return await Task.FromResult(manifestGroupwaybillMappingDTO.OrderByDescending(x => x.DateCreated).ToList());
        }

        public async Task<ManifestGroupWaybillNumberMappingDTO> GetManifestGroupWaybillNumberMappingsUsingGroupWaybill(string groupWaybill)
        {
            var manifestGroupwaybillMapping = Context.ManifestGroupWaybillNumberMapping.Where(s => s.GroupWaybillNumber == groupWaybill).AsQueryable();
            
            var manifestGroupwaybillMappingDTO = from mgw in manifestGroupwaybillMapping
                                                 select new ManifestGroupWaybillNumberMappingDTO
                                                 {
                                                     ManifestGroupWaybillNumberMappingId = mgw.ManifestGroupWaybillNumberMappingId,
                                                     ManifestCode = mgw.ManifestCode,
                                                     GroupWaybillNumber = mgw.GroupWaybillNumber,
                                                     IsActive = mgw.IsActive,
                                                     DateMapped = mgw.DateMapped,
                                                     DateCreated = mgw.DateCreated,
                                                     DateModified = mgw.DateModified,
                                                     IsDeleted = mgw.IsDeleted,
                                                     RowVersion = mgw.RowVersion,
                                                     ManifestDetails = Context.Manifest.Where(x => x.ManifestCode == mgw.ManifestCode).
                                                     Select(p => new ManifestDTO
                                                     {
                                                         DateCreated = p.DateCreated,
                                                         DateModified = p.DateModified,
                                                         ManifestCode = p.ManifestCode,
                                                         ManifestType = p.ManifestType,
                                                         DateTime = p.DateTime,
                                                         IsDispatched = p.IsDispatched,
                                                         IsReceived = p.IsReceived,
                                                         DispatchedBy = Context.Users.Where(d => d.Id == p.DispatchedById).Select(x => x.LastName + " " + x.FirstName).FirstOrDefault(),
                                                         ReceiverBy = Context.Users.Where(r => r.Id == p.ReceiverById).Select(x => x.LastName + " " + x.FirstName).FirstOrDefault()
                                                     }).FirstOrDefault()
                                                 };

            return await Task.FromResult(manifestGroupwaybillMappingDTO.FirstOrDefault());
        }
    }
}
