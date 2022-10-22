﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GIGL.POST.Core.Domain;
using POST.Core.DTO.Zone;
using POST.Core.IRepositories.Zone;
using POST.Infrastructure.Persistence;
using POST.Infrastructure.Persistence.Repository;
using System.Linq;

namespace POST.INFRASTRUCTURE.Persistence.Repositories.Zone
{
    public class SpecialDomesticZonePriceRepository : Repository<SpecialDomesticZonePrice, GIGLSContext>, ISpecialDomesticZonePriceRepository
    {
        public SpecialDomesticZonePriceRepository(GIGLSContext context) : base(context)
        {
        }

        public Task<List<SpecialDomesticZonePriceDTO>> GetSpecialDomesticZonesPrice()
        {
            try
            {
                var zoneDto = from s in Context.SpecialDomesticZonePrice
                              select new SpecialDomesticZonePriceDTO
                              {
                                  SpecialDomesticZonePriceId = s.SpecialDomesticZonePriceId,
                                  Description = s.Description,
                                  Weight = s.Weight ?? 0,
                                  Price = s.Price,
                                  SpecialDomesticPackageId = s.SpecialDomesticPackageId,
                                  SpecialDomesticPackageName = s.SpecialDomesticPackage.Name,
                                  ZoneId = s.ZoneId,
                                  ZoneName = s.Zone.ZoneName,
                                  CountryId = s.CountryId
                                  //UserName = s.User.FirstName + " "+ s.User.LastName
                              };
                return Task.FromResult(zoneDto.ToList());
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
