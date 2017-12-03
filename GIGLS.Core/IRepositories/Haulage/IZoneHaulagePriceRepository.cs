﻿using GIGL.GIGLS.Core.Repositories;
using GIGLS.Core.Domain;
using GIGLS.Core.DTO.Haulage;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GIGLS.Core.IRepositories
{
    public interface IZoneHaulagePriceRepository : IRepository<ZoneHaulagePrice>
    {
        Task<List<ZoneHaulagePriceDTO>> GetZoneHaulagePricesAsync();
    }
}
