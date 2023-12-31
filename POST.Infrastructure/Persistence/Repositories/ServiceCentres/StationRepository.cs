﻿using System.Collections.Generic;
using System.Threading.Tasks;
using POST.Core.Domain;
using POST.Core.IRepositories.ServiceCentres;
using POST.Infrastructure.Persistence;
using POST.Infrastructure.Persistence.Repository;
using System.Linq;
using POST.Core.DTO.ServiceCentres;
using System;
using POST.Core.DTO;

namespace POST.INFRASTRUCTURE.Persistence.Repositories.ServiceCentres
{
    public class StationRepository : Repository<Station, GIGLSContext>, IStationRepository
    {
        public StationRepository(GIGLSContext context) : base(context)
        {
        }

        public Task<List<Station>> GetStationsAsync()
        {
            return Task.FromResult(Context.Station.Include("State").ToList());
        }

        public Task<List<StationDTO>> GetLocalStations(int[] countryIds)
        {
            try
            {
                var stations = Context.Station;
                var stationDto = from s in stations
                                 join st in Context.State on s.StateId equals st.StateId
                                 join c in Context.Country on st.CountryId equals c.CountryId
                                 where countryIds.Contains(c.CountryId)
                                select new StationDTO
                                {
                                    StationId = s.StationId,
                                    StationName = s.StationName,
                                    StationCode = s.StationCode,
                                    StateId = s.StateId,
                                    StateName = st.StateName,
                                    Country = c.CountryName,
                                    DateCreated = s.DateCreated,
                                    DateModified = s.DateModified,
                                    CountryDTO = new CountryDTO
                                    {
                                        CountryId = c.CountryId,
                                        CountryCode = c.CountryCode,
                                        CountryName = c.CountryName
                                    },
                                    SuperServiceCentreId = s.SuperServiceCentreId,
                                    SuperServiceCentreDTO = Context.ServiceCentre.Where(c => c.ServiceCentreId == s.SuperServiceCentreId).Select(x => new ServiceCentreDTO
                                    {
                                        Code = x.Code,
                                        Name = x.Name
                                    }).FirstOrDefault()
                                };
                return Task.FromResult(stationDto.OrderBy(x => x.StationName).ToList());
            }
            catch (Exception)
            {
                throw;
            }
        }


        public Task<List<StationDTO>> GetLocalStationsWithoutSuperServiceCentre(int[] countryIds)
        {
            try
            {
                var stations = Context.Station;
                var stationDto = from s in stations
                                 join st in Context.State on s.StateId equals st.StateId
                                 join c in Context.Country on st.CountryId equals c.CountryId
                                 where countryIds.Contains(c.CountryId)
                                 select new StationDTO
                                 {
                                     StationId = s.StationId,
                                     StationName = s.StationName,
                                     StationCode = s.StationCode,
                                     StateId = s.StateId,
                                     StateName = st.StateName,
                                     Country = c.CountryName,
                                     CountryDTO = new CountryDTO
                                     {
                                         CountryId = c.CountryId,
                                         CountryCode = c.CountryCode,
                                         CountryName = c.CountryName
                                     },
                                     SuperServiceCentreId = s.SuperServiceCentreId
                                 };
                return Task.FromResult(stationDto.OrderBy(x => x.StationName).ToList());
            }
            catch (Exception)
            {
                throw;
            }
        }
        public Task<List<StationDTO>> GetInternationalStations()
        {
            try
            {
                var stations = Context.Station;
                var stationDto = from s in stations
                                 join st in Context.State on s.StateId equals st.StateId
                                 join c in Context.Country on st.CountryId equals c.CountryId
                                 //where c.CountryName != "Nigeria"
                                 where c.CountryId == 1
                                 select new StationDTO
                                 {
                                     StationId = s.StationId,
                                     StationName = s.StationName,
                                     StationCode = s.StationCode,
                                     StateId = s.StateId,
                                     StateName = st.StateName,
                                     Country = c.CountryName,
                                     DateCreated = s.DateCreated,
                                     DateModified = s.DateModified,
                                     CountryDTO = new CountryDTO
                                    {
                                        CountryId = c.CountryId,
                                        CountryCode = c.CountryCode,
                                        CountryName = c.CountryName
                                    },
                                     SuperServiceCentreId = s.SuperServiceCentreId,
                                     SuperServiceCentreDTO = Context.ServiceCentre.Where(c => c.ServiceCentreId == s.SuperServiceCentreId).Select(x => new ServiceCentreDTO
                                     {
                                         Code = x.Code,
                                         Name = x.Name
                                     }).FirstOrDefault()
                                 };
                return Task.FromResult(stationDto.OrderBy(x => x.StationName).ToList());
            }
            catch (Exception)
            {
                throw;
            }
        }
        public Task<List<Station>> GetAllStationsAsync()
        {
            return Task.FromResult(Context.Station.ToList());
        }

        public Task<List<StationDTO>> GetActiveGIGGoStations()
        {
            try
            {
                var stations = Context.Station.AsQueryable().Where(x => x.GIGGoActive == true);

                var stationDto = from s in stations
                                 join st in Context.State on s.StateId equals st.StateId
                                 join c in Context.Country on st.CountryId equals c.CountryId
                                 select new StationDTO
                                 {
                                     StationId = s.StationId,
                                     StationName = s.StationName,
                                     StationCode = s.StationCode,
                                     StateId = s.StateId,
                                     StateName = st.StateName,
                                     Country = c.CountryName,
                                     DateCreated = s.DateCreated,
                                     DateModified = s.DateModified,
                                     SuperServiceCentreId = s.SuperServiceCentreId,
                                 };
                return Task.FromResult(stationDto.OrderBy(x => x.StationName).ToList());
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task<List<StationDTO>> GetStationsByCountry(int countryId)
        {
            try
            {
                var stations = Context.Station;
                var stationDto = from s in stations
                                 join st in Context.State on s.StateId equals st.StateId
                                 join c in Context.Country on st.CountryId equals c.CountryId
                                 where st.CountryId == countryId && c.IsActive == true
                                 select new StationDTO
                                 {
                                     StationId = s.StationId,
                                     StationName = s.StationName,
                                     StationCode = s.StationCode,
                                     StateId = s.StateId,
                                     StateName = st.StateName,
                                     Country = c.CountryName,
                                 };
                return Task.FromResult(stationDto.OrderBy(x => x.StationName).ToList());
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task<List<ServiceCentreDTO>> GetServiceCentresByStation(int stationId)
        {
            try
            {
                var svrcentres = Context.ServiceCentre;
                var svrcentreDto = from s in svrcentres
                                 join st in Context.Station on s.StationId equals st.StationId
                                 where s.StationId == stationId && s.IsActive == true
                                 select new ServiceCentreDTO
                                 {
                                     StationId = s.StationId,
                                     Name = s.Name,
                                     Address = s.Address,
                                     City = s.City,
                                     Code = s.Code
                                 };
                return Task.FromResult(svrcentreDto.OrderBy(x => x.Name).ToList());
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task<List<StationDTO>> GetStationsByUserCountry(int[] countryIds)
        {
            try
            {
                var stations = Context.Station;
                var stationDto = from s in stations
                                 join st in Context.State on s.StateId equals st.StateId
                                 join c in Context.Country on st.CountryId equals c.CountryId
                                 where countryIds.Contains(c.CountryId)
                                 select new StationDTO
                                 {
                                     StationId = s.StationId,
                                     StationName = s.StationName,
                                     StationCode = s.StationCode,
                                     StateId = s.StateId,
                                     StateName = st.StateName,
                                     Country = c.CountryName
                                 };
                return Task.FromResult(stationDto.OrderBy(x => x.StationName).ToList());
            }
            catch (Exception)
            {
                throw;
            }
        }



    }
}
