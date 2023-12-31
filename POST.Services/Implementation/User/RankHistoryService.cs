﻿using System;
using System.Threading.Tasks;
using POST.Core;
using POST.Core.DTO.Stores;
using POST.Core.DTO.User;
using POST.Core.IRepositories.User;
using POST.Core.IServices.Stores;
using POST.Core.IServices.User;

namespace POST.Services.Implementation.User
{
    public class RankHistoryService : IRankHistoryService
    {

        private readonly IUserService _userService;
        private readonly IUnitOfWork _uow;

        public RankHistoryService(IUserService userService,IUnitOfWork uow)
        {
            _userService = userService;
            _uow = uow;
            MapperConfig.Initialize();
        }

        public async Task AddRankHistory(RankHistoryDTO history)
        {

            _uow.RankHistory.Add(new Core.Domain.RankHistory 
            {
                RankHistoryId = history.RankHistoryId,
                CustomerName = history.CustomerName,
                CustomerCode = history.CustomerCode,
                RankType = history.RankType,
                DateCreated = DateTime.Now,
                DateModified = DateTime.Now
            });
            await _uow.CompleteAsync();
        }
    }
}
