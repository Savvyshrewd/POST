﻿using GIGL.POST.Core.Repositories;
using POST.Core.DTO.Wallet;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace POST.Core.IRepositories.Wallet
{
    public interface IWalletRepository : IRepository<Domain.Wallet.Wallet>
    {
        Task<IEnumerable<WalletDTO>> GetWalletsAsync();
        IQueryable<Core.Domain.Wallet.Wallet> GetWalletsAsQueryable();
        Task<decimal> GetTotalWalletBalance(int ActiveCountryId);
        Task<List<WalletDTO>> GetOutstaningCorporatePayments();
        Task<WalletBreakdown> GetWalletBreakdown(int activeCountryId);
    }
}
