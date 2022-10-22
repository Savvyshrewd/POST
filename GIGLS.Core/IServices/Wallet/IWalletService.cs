﻿using POST.Core.Domain.Wallet;
using POST.Core.DTO;
using POST.Core.DTO.Wallet;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace POST.Core.IServices.Wallet
{
    public interface IWalletService : IServiceDependencyMarker
    {
        Task<IEnumerable<WalletDTO>> GetWallets();
        Task<WalletDTO> GetWalletById(int walletId);
        Task<Domain.Wallet.Wallet> GetWalletById(string walletNumber);
        Task AddWallet(WalletDTO wallet);
        Task UpdateWallet(int walletId, WalletTransactionDTO walletTransactionDTO, bool hasServiceCentre = true);
        Task RemoveWallet(int walletId);
        Task<WalletNumber> GenerateNextValidWalletNumber();
        Task<WalletDTO> GetSystemWallet();
        Task<List<WalletDTO>> SearchForWallets(WalletSearchOption searchOption);
        Task<WalletDTO> GetWalletBalance();
        IQueryable<Core.Domain.Wallet.Wallet> GetWalletAsQueryableService();
        Task<WalletDTO> GetWalletBalance(string userChannelCode);
        Task<WalletDTO> GetWalletBalanceWithName();
        Task<List<WalletDTO>> GetOutstaningCorporatePayments();
        Task<ResponseDTO> ChargeWallet(ChargeWalletDTO chargeWalletDTO);
        Task<List<WalletDTO>> GetUserWallets(WalletSearchOption searchOption);
        Task<bool> ChargeUserWallet(WalletDTO walletDTO);
        Task TopUpWallet(int walletId, WalletTransactionDTO walletTransactionDTO, bool hasServiceCentre = true);
        Task<ResponseDTO> ReverseWallet(string reference);
        Task<object> ProcessBulkWalletUpload(string path);
    }

}
