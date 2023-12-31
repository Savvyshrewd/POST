﻿using POST.Core.Domain;
using POST.Core.DTO;
using POST.Core.DTO.User;
using POST.Core.IRepositories;
using POST.Core.IRepositories.BankSettlement;
using POST.CORE.DTO.Shipments;
using POST.Infrastructure.Persistence.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace POST.Infrastructure.Persistence.Repositories.Shipments
{
    public class InternationalCargoManifestRepository : Repository<InternationalCargoManifest, GIGLSContext>, IInternationalCargoManifestRepository
    {
        private GIGLSContext _context;
        public InternationalCargoManifestRepository(GIGLSContext context) : base(context)
        {
            _context = context;
        }

        public Task<InternationalCargoManifestDTO> GetIntlCargoManifestByID(int cargoID)
        {
            var manifest = _context.InternationalCargoManifest.Where(x => x.InternationalCargoManifestId == cargoID);

            InternationalCargoManifestDTO result = (from s in manifest
                                                          join dept in Context.Country on s.DepartureCountry equals dept.CountryId
                                                          join dest in Context.Country on s.DestinationCountry equals dest.CountryId
                                                          select new InternationalCargoManifestDTO
                                                          {
                                                              AirlineWaybillNo = s.AirlineWaybillNo,
                                                              FlightNo = s.FlightNo,
                                                              FlightDate = s.FlightDate,
                                                              ManifestNo = s.ManifestNo,
                                                              DateCreated = s.DateCreated,
                                                              DestinationCountryName = s.DestinationCountry == 1 ? "LAGOS, NIGERIA" : dept.CountryName,
                                                              DepartureCountryName = dept.CountryName,
                                                              InternationalCargoManifestId = s.InternationalCargoManifestId,
                                                              InternationalCargoManifestDetails = Context.InternationalCargoManifestDetail.Where(i => i.InternationalCargoManifestId == s.InternationalCargoManifestId).Select(x => new InternationalCargoManifestDetailDTO
                                                              {
                                                                  Height = x.Height,
                                                                  Length = x.Length,
                                                                  Quantity = x.Quantity,
                                                                  Weight = x.Weight,
                                                                  Width = x.Width,
                                                                  ItemName = x.ItemName,
                                                                  ItemUniqueNo = x.ItemUniqueNo,
                                                                  RequestNumber = x.RequestNumber,
                                                                  Waybill = x.Waybill,
                                                                  CourierService = x.CourierService,
                                                                  ItemRequestCode = x.ItemRequestCode,
                                                                  NoOfPackageReceived = x.NoOfPackageReceived,
                                                                  Description = x.Description,
                                                                  GrandTotal = x.GrandTotal,
                                                                  DeclaredValue = x.DeclaredValue
                                                                  
                                                                 
                                                              }).ToList()
                                                          }).FirstOrDefault();
            return Task.FromResult(result);
        }

        public Task<List<InternationalCargoManifestDTO>> GetIntlCargoManifests(NewFilterOptionsDto filter)
        {
            var manifests = _context.InternationalCargoManifest.AsQueryable();
            if (filter != null && !String.IsNullOrEmpty(filter.FilterType))
            {
                manifests = _context.InternationalCargoManifest.AsQueryable().Where(x => x.ManifestNo == filter.FilterType || x.AirlineWaybillNo == filter.FilterType || x.FlightNo == filter.FilterType);
            }
            else
            {
                manifests = manifests.Where(x => x.DateCreated >= filter.StartDate && x.DateCreated <= filter.EndDate);
            }

            List<InternationalCargoManifestDTO> result = (from s in manifests
                                           join dept in Context.Country on s.DepartureCountry equals dept.CountryId
                                           join dest in Context.Country on s.DestinationCountry equals dest.CountryId
                                           select new InternationalCargoManifestDTO
                                           {
                                             AirlineWaybillNo = s.AirlineWaybillNo,
                                             FlightNo = s.FlightNo,
                                             FlightDate = s.FlightDate,
                                             ManifestNo = s.ManifestNo,
                                             DateCreated = s.DateCreated,
                                             DestinationCountryName = s.DestinationCountry == 1 ? "LAGOS, NIGERIA" : dept.CountryName,
                                             DepartureCountryName = dept.CountryName,
                                             InternationalCargoManifestId = s.InternationalCargoManifestId,
                                           }).ToList();
            var resultDto = result.OrderByDescending(x => x.DateCreated).ThenBy(x => x.ManifestNo).ToList();
            return Task.FromResult(resultDto);
        }
    }
}
