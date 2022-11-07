﻿using GIGL.POST.Core.Repositories;
using POST.Core.Domain;
using POST.Core.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POST.Core.IServices
{
    public interface IGiglgoStationService: IServiceDependencyMarker
    {
        Task<List<GiglgoStationDTO>> GetGoStations();
        Task<GiglgoStationDTO> GetGoStationsById(int stationId);
    }
}
