﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GIGL.POST.Core.Domain;
using POST.Core.DTO.Shipments;
using POST.Core.IRepositories.Shipments;
using POST.Infrastructure.Persistence;
using POST.Infrastructure.Persistence.Repository;
using System.Linq;
using POST.Core.DTO.ServiceCentres;

namespace POST.INFRASTRUCTURE.Persistence.Repositories.Shipments
{
    public class ManifestRepository : Repository<Manifest, GIGLSContext>, IManifestRepository
    {
        private GIGLSContext _context;
        public ManifestRepository(GIGLSContext context)
            : base(context)
        {
            _context = context;
        }

        public Task<List<ManifestDTO>> GetManifests()
        {
            try
            {
                var manifest = Context.Manifest;

                var manifestDto = from r in manifest
                                  select new ManifestDTO()
                                  {
                                      DateTime = r.DateTime,
                                      ManifestCode = r.ManifestCode,
                                      ManifestId = r.ManifestId,
                                      //MasterWaybill = r.MasterWaybill,
                                      //ReceiverBy
                                      //VehicleTrip
                                      //Shipments
                                      //DispatchedBy = r.DispatechedBy,                                     
                                  };
                return Task.FromResult(manifestDto.ToList());
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task<List<ManifestDTO>> GetManifest(List<Manifest> manifests)
        {
            try
            {
                var manifestDto = from r in manifests
                                  select new ManifestDTO()
                                  {
                                      DateTime = r.DateTime,
                                      ManifestCode = r.ManifestCode,
                                      ManifestId = r.ManifestId,
                                      DateCreated = r.DateCreated,
                                      DateModified = r.DateModified,
                                      SuperManifestCode = r.SuperManifestCode,
                                      SuperManifestStatus = r.SuperManifestStatus,
                                      HasSuperManifest = r.HasSuperManifest,
                                      DepartureServiceCentreId = r.DepartureServiceCentreId,
                                      DestinationServiceCentreId = r.DestinationServiceCentreId,
                                      DestinationServiceCentre = Context.ServiceCentre.Where(c => c.ServiceCentreId == r.DestinationServiceCentreId).Select(x => new ServiceCentreDTO
                                      {
                                          Code = x.Code,
                                          Name = x.Name
                                      }).FirstOrDefault(),
                                      DepartureServiceCentre = Context.ServiceCentre.Where(c => c.ServiceCentreId == r.DestinationServiceCentreId).Select(x => new ServiceCentreDTO
                                      {
                                          Code = x.Code,
                                          Name = x.Name
                                      }).FirstOrDefault(),
                                  };

                return Task.FromResult(manifestDto.OrderByDescending(x => x.DateModified).ToList());
            }
            catch (Exception)
            {
                throw;
            }
        }


        public Task<List<AllManifestAndGroupWaybillDTO>> GetManifestsInSuperManifests(string code)
        {
            var manifests = _context.Manifest.AsQueryable().Where(s => s.SuperManifestCode == code);
            try
            {
                var manifestDto = from r in manifests
                                  select new AllManifestAndGroupWaybillDTO()
                                  {
                                      ManifestCode = r.ManifestCode,
                                      DateCreated = r.DateCreated,
                                      DateModified = r.DateModified,
                                      SuperManifestCode = r.SuperManifestCode,
                                      SuperManifestStatus = r.SuperManifestStatus,
                                      HasSuperManifest = r.HasSuperManifest,
                                      DepartureServiceCentreId = r.DepartureServiceCentreId,
                                      DestinationServiceCentreId = r.DestinationServiceCentreId,
                                      ExpressDelivery = r.ExpressDelivery,
                                      IsBulky = r.IsBulky,
                                      ManifestType = r.ManifestType,
                                      DestinationServiceCentre = Context.ServiceCentre.Where(c => c.ServiceCentreId == r.DestinationServiceCentreId).Select(x => new ServiceCentreDTO
                                      {
                                          Code = x.Code,
                                          Name = x.Name
                                      }).FirstOrDefault(),
                                      DepartureServiceCentre = Context.ServiceCentre.Where(c => c.ServiceCentreId == r.DestinationServiceCentreId).Select(x => new ServiceCentreDTO
                                      {
                                          Code = x.Code,
                                          Name = x.Name
                                      }).FirstOrDefault(),
                                  };

                return Task.FromResult(manifestDto.OrderByDescending(x => x.DateModified).ToList());
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
