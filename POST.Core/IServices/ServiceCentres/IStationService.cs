﻿using POST.Core.DTO;
using POST.Core.DTO.ServiceCentres;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace POST.Core.IServices.ServiceCentres
{
    public interface IStationService : IServiceDependencyMarker
    {
        Task<IEnumerable<StationDTO>> GetStations();
        Task<List<StationDTO>> GetLocalStations();
        Task<List<StationDTO>> GetInternationalStations();
        Task<StationDTO> GetStationById(int stationId);
        Task<object> AddStation(StationDTO station);
        Task UpdateStation(int stationId, StationDTO station);
        Task DeleteStation(int stationId);
        Task UpdateGIGGoStationStatus(int stationId, bool status);
        Task<List<StationDTO>> GetActiveGIGGoStations();
        Task<GiglgoStationDTO> GetGIGGoStationById(int stationId);
        Task<List<StationDTO>> GetStationsByStationName(string name);
        Task<List<StationDTO>> GetStationsByUserCountry();
        Task<List<StationDTO>> GetStationsByCountry(int countryId);
    }
}
