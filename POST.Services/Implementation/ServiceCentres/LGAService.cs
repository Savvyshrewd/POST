﻿using AutoMapper;
using POST.Core;
using POST.Core.Domain;
using POST.Core.DTO.ServiceCentres;
using POST.Core.IServices.ServiceCentres;
using POST.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POST.Services.Implementation.ServiceCentres
{
    public class LGAService : ILGAService
    {
        private readonly IUnitOfWork _uow;

        public LGAService(IUnitOfWork uow)
        {
            _uow = uow;
            MapperConfig.Initialize();
        }

        public async Task<object> AddLGA(LGADTO lgaDto)
        {
            try
            {
                var state = await GetState(lgaDto.StateId);

                var lga = await _uow.LGA.GetAsync(x => x.LGAName.ToLower() == lgaDto.LGAName.ToLower() && x.StateId == lgaDto.StateId);

                if (lga != null)
                {
                    throw new GenericException("LGA Information already exists");
                }
                lgaDto.LGAState = state.StateName;
                var newlga = Mapper.Map<LGA>(lgaDto);
                _uow.LGA.Add(newlga);
                await _uow.CompleteAsync();
                return new { Id = newlga.LGAId };
            }
            catch (Exception)
            {
                throw;
            }
        }


        private async Task<State> GetState(int stateId)
        {
            try
            {
                var state = await _uow.State.GetAsync(stateId);
                if (state == null)
                {
                    throw new GenericException("State does not exist");
                }

                return state;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<LGADTO> GetLGAById(int lgaId)
        {
            try
            {
                var lga = await _uow.LGA.GetAsync(lgaId);
                if (lga == null)
                {
                    throw new GenericException("LGA information does not exist");
                }

                var lgaDto = Mapper.Map<LGADTO>(lga);
                return lgaDto;
            }
            catch (Exception)
            {
                throw;
            }
        }
        public Task<IEnumerable<LGADTO>> GetLGAs()
        {
            var lgas = _uow.LGA.GetAll().OrderBy(x => x.LGAName);
            return Task.FromResult(Mapper.Map<IEnumerable<LGADTO>>(lgas));
        }

        public async Task UpdateLGA(int lgaId, LGADTO lgaDto)
        {
            try
            {
                var state = await GetState(lgaDto.StateId);
                var lga = await _uow.LGA.GetAsync(lgaId);

                //To check if the update already exists
                var lgas = await _uow.LGA.ExistAsync(c => c.LGAName.ToLower() == lgaDto.LGAName.ToLower() && c.StateId == lgaDto.StateId);
                if (lga == null || lgaDto.LGAId != lgaId)
                {
                    throw new GenericException("LGA Information does not exist");
                }
                else if (lgas == true)
                {
                    throw new GenericException("LGA Information already exists");
                }
                lga.LGAName = lgaDto.LGAName;
                lga.LGAState = state.StateName;
                lga.StateId = state.StateId;
                lga.Status = lgaDto.Status;

                _uow.Complete();

            }
            catch (Exception)
            {
                throw;
            }
        }
        public async Task UpdateLGA(int lgaId, bool status)
        {
            try
            {
                var lga = await _uow.LGA.GetAsync(lgaId);
                if (lga == null)
                {
                    throw new GenericException("LGA Information does not exist");
                }
                lga.Status = status;
                _uow.Complete();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task DeleteLGA(int lgaId)
        {
            try
            {
                var lga = await _uow.LGA.GetAsync(lgaId);
                if (lga == null)
                {
                    throw new GenericException("LGA information does not exist");
                }
                _uow.LGA.Remove(lga);
                _uow.Complete();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<LGADTO>> GetActiveLGAs()
        {
            try
            {
                return await _uow.LGA.GetActiveLGAs();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task UpdateHomeDeliveryLocation(int lgaId, bool status)
        {
            try
            {
                var location = await _uow.LGA.GetAsync(lgaId);
                if (location == null)
                {
                    throw new GenericException("LGA Information does not exist");
                }
                location.HomeDeliveryStatus = status;
                _uow.Complete();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<LGADTO>> GetActiveHomeDeliveryLocations()
        {
            try
            {
                return await _uow.LGA.GetActiveHomeDeliveryLocations();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<LGADTO>> GetLGAByState(int stateId)
        {
            try
            {
                var lgas = new List<LGADTO>();
                var items = _uow.LGA.GetAll().Where(x => x.StateId == stateId).ToList();
                if (items.Any())
                {
                    lgas = Mapper.Map<List<LGADTO>>(items);
                }
                return lgas;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> CheckHomeDeliveryAllowed(int lgaID)
        {
            try
            {
                bool allowed = false;
                var item = await _uow.LGA.GetAsync( x => x.LGAId == lgaID);
                if (item != null && item.HomeDeliveryStatus == true)
                {
                    allowed = true;
                }
                return allowed;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
