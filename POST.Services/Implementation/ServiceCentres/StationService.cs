﻿using POST.Core;
using POST.Core.Domain;
using POST.Core.DTO.ServiceCentres;
using POST.Core.IServices;
using POST.Core.IServices.ServiceCentres;
using POST.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using POST.Core.IServices.User;
using POST.Core.DTO;

namespace POST.Services.Implementation.ServiceCentres
{
    public class StationService : IStationService
    {
        private readonly IUnitOfWork _uow;
        private IStateService _stateService;
        private readonly IUserService _userService;

        public StationService(IUnitOfWork uow, IStateService stateService, IUserService userService)
        {
            _uow = uow;
            _stateService = stateService;
            _userService = userService;
            MapperConfig.Initialize();
        }

        public async Task<object> AddStation(StationDTO station)
        {
            await _stateService.GetStateById(station.StateId);

            station.StationName = station.StationName.Trim();
            var stationCode = station.StationCode.ToLower().Trim();

            if (await _uow.Station.ExistAsync(v => v.StationCode.ToLower() == stationCode))
            {
                throw new GenericException($"{station.StationName} STATION INFORMATION ALREADY EXIST");
            }

            var newStation = new Station
            {
                StationName = station.StationName,
                StationCode = station.StationCode,
                StateId = station.StateId,
                SuperServiceCentreId = station.SuperServiceCentreId,
                StationPickupPrice = station.StationPickupPrice
            };

            _uow.Station.Add(newStation);
            await _uow.CompleteAsync();
            return new { id = newStation.StationId };
        }

        public async Task DeleteStation(int stationId)
        {
            var station = await _uow.Station.GetAsync(stationId);

            if (station == null)
            {
                throw new GenericException("STATION INFORMATION DOES NOT EXIST");
            }
            _uow.Station.Remove(station);
            await _uow.CompleteAsync();
        }

        public async Task<StationDTO> GetStationById(int stationId)
        {
            var station = await _uow.Station.GetAsync(s => s.StationId == stationId, "State");

            if (station == null)
            {
                throw new GenericException("STATION INFORMATION DOES NOT EXIST");
            }

            var serviceCentre = await _uow.ServiceCentre.GetAsync(s => s.ServiceCentreId == station.SuperServiceCentreId);
            return new StationDTO
            {
                StationId = station.StationId,
                StationName = station.StationName,
                StationCode = station.StationCode,
                StateId = station.StateId,
                StateName = station.State.StateName,
                DateCreated = station.DateCreated,
                DateModified = station.DateModified,
                Country = station.State.CountryId.ToString(),
                SuperServiceCentreId = station.SuperServiceCentreId,
                StationPickupPrice = station.StationPickupPrice,
                SuperServiceCentreDTO = new ServiceCentreDTO()
                {
                    Code = serviceCentre?.Code,
                    Name = serviceCentre?.Name
                }
            };
        }

        public async Task UpdateStation(int stationId, StationDTO stationDto)
        {
            await _stateService.GetStateById(stationDto.StateId);

            var station = await _uow.Station.GetAsync(stationDto.StationId);

            if (station == null || stationId != stationDto.StationId)
            {
                throw new GenericException("STATION INFORMATION DOES NOT EXIST");
            }

            station.StationName = stationDto.StationName.Trim();
            station.StationCode = stationDto.StationCode.Trim();
            station.StateId = stationDto.StateId;
            station.SuperServiceCentreId = stationDto.SuperServiceCentreId;
            station.StationPickupPrice = stationDto.StationPickupPrice;
            station.GIGGoActive = stationDto.GIGGoActive;

            await _uow.CompleteAsync();
        }

        public async Task UpdateGIGGoStationStatus(int stationId, bool status)
        {
            try
            {
                var station = await _uow.Station.GetAsync(stationId);
                if (station == null)
                {
                    throw new GenericException("LGA Information does not exist");
                }
                station.GIGGoActive = status;
                _uow.Complete();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<StationDTO>> GetStationsByStationName(string name)
        {
            List<StationDTO> stationDto = new List<StationDTO>();
           //var st = _uow.Station.GetAll().Where(s => s.StationName == name).FirstOrDefault();
            var st = await _uow.Station.GetAsync(s => s.StationName == name);

            stationDto.Add(new StationDTO
            {
                StationId = st.StationId,
                StationCode = st.StationCode,
                StationName = st.StationName,
                StateId = st.StateId,
                StateName = st.State.StateName,
                SuperServiceCentreId = st.SuperServiceCentreId,
                StationPickupPrice = st.StationPickupPrice,
                GIGGoActive = st.GIGGoActive
            });
            return stationDto;
        }

        public async Task<IEnumerable<StationDTO>> GetStations()
        {
            List<StationDTO> stationDto = new List<StationDTO>();
            var stations = await _uow.Station.GetStationsAsync();

            foreach (var st in stations)
            {
                ServiceCentreDTO superServiceCentreDTO = null;

                if(st.SuperServiceCentreId > 0)
                {
                    var serviceCentre = await _uow.ServiceCentre.GetAsync(s => s.ServiceCentreId == st.SuperServiceCentreId);
                    if (serviceCentre != null)
                    {
                        superServiceCentreDTO = new ServiceCentreDTO()
                        {
                            Code = serviceCentre?.Code,
                            Name = serviceCentre?.Name
                        };
                    }
                }

                stationDto.Add(new StationDTO
                {
                    StationId = st.StationId,
                    StationCode = st.StationCode,
                    StationName = st.StationName,
                    StateId = st.StateId,
                    StateName = st.State.StateName,
                    SuperServiceCentreId = st.SuperServiceCentreId,
                    StationPickupPrice = st.StationPickupPrice,
                    SuperServiceCentreDTO = superServiceCentreDTO,
                    GIGGoActive = st.GIGGoActive
                });
            }
            return stationDto.OrderBy(x => x.StationName).ToList();
        }

        public async Task<List<StationDTO>> GetLocalStations()
        {
            var countryIds = await _userService.GetPriviledgeCountryIds();
            return await _uow.Station.GetLocalStations(countryIds);
        }

        public async Task<List<StationDTO>> GetInternationalStations()
        {
            return await _uow.Station.GetInternationalStations();
        }

        public async Task<List<StationDTO>> GetActiveGIGGoStations()
        {
            return await _uow.Station.GetActiveGIGGoStations();
        }

        public async Task<GiglgoStationDTO> GetGIGGoStationById(int stationId)
        {
            return await _uow.GiglgoStation.GetGoStationsById(stationId);
        }

        public async Task<List<StationDTO>> GetStationsByUserCountry()
        {
            var countryIds = await _userService.GetPriviledgeCountryIds();
            return await _uow.Station.GetStationsByUserCountry(countryIds);
        }

        public async Task<List<StationDTO>> GetStationsByCountry(int countryId)
        {
            return await _uow.Station.GetStationsByCountry(countryId);
        }
    }
}
