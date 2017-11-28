﻿using GIGL.GIGLS.Core.Domain;
using GIGLS.Core.IRepositories.ServiceCentres;
using GIGLS.Core.IRepositories.Shipments;
using GIGLS.CORE.DTO.Shipments;
using GIGLS.Infrastructure.Persistence;
using GIGLS.Infrastructure.Persistence.Repository;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

namespace GIGLS.INFRASTRUCTURE.Persistence.Repositories.Shipments
{
    public class GroupWaybillNumberMappingRepository : Repository<GroupWaybillNumberMapping, GIGLSContext>, IGroupWaybillNumberMappingRepository
    {
        private GIGLSContext _context;
        public GroupWaybillNumberMappingRepository(GIGLSContext context)
            : base(context)
        {
            _context = context;
        }

        public Task<List<GroupWaybillNumberMappingDTO>> GetGroupWaybillMappings(int[] serviceCentreIds)
        {
            var groupwaybillMapping = Context.GroupWaybillNumberMapping.AsQueryable();

            var serviceCentreWaybills = new List<string>();
            if (serviceCentreIds.Length > 0)
            {
                serviceCentreWaybills = _context.Shipment.Where(s => serviceCentreIds.Contains(s.DepartureServiceCentreId)).
                    Select(s => s.Waybill).ToList();
            }


            groupwaybillMapping = groupwaybillMapping.Where(s => serviceCentreWaybills.Contains(s.WaybillNumber));

            var groupwaybillMappingDto = from gw in groupwaybillMapping
                                         select new GroupWaybillNumberMappingDTO
                                         {
                                             GroupWaybillNumber = gw.GroupWaybillNumber,
                                             WaybillNumber = gw.WaybillNumber,
                                             GroupWaybillNumberMappingId = gw.GroupWaybillNumberMappingId,
                                             IsActive = gw.IsActive,
                                             DateMapped = gw.DateMapped,
                                             DateCreated = gw.DateCreated,
                                             DateModified = gw.DateModified
                                         };
            return Task.FromResult(groupwaybillMappingDto.ToList());
        }

    }
}
