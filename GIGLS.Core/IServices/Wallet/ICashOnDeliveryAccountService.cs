﻿using POST.Core.DTO.Wallet;
using POST.Core.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace POST.Core.IServices.CashOnDeliveryAccount
{
    public interface ICashOnDeliveryAccountService : IServiceDependencyMarker
    {
        Task<IEnumerable<CashOnDeliveryAccountDTO>> GetCashOnDeliveryAccounts();
        Task<CashOnDeliveryAccountDTO> GetCashOnDeliveryAccountById(int cashOnDeliveryAccountId);
        Task<CashOnDeliveryAccountSummaryDTO> GetCashOnDeliveryAccountByWallet(string walletNumber);
        Task<CashOnDeliveryAccountSummaryDTO> GetCashOnDeliveryAccountByStatus(string walletNumber, CODStatus status);
        Task AddCashOnDeliveryAccount(CashOnDeliveryAccountDTO cashOnDeliveryAccountDto);
        Task UpdateCashOnDeliveryAccount(int cashOnDeliveryAccountId, CashOnDeliveryAccountDTO cashOnDeliveryAccountDto);
        Task RemoveCashOnDeliveryAccount(int cashOnDeliveryAccountId);
        Task ProcessCashOnDeliveryPaymentSheet(List<CashOnDeliveryBalanceDTO> data);
        Task ProcessToPending(List<CashOnDeliveryBalanceDTO> data);
        Task<IEnumerable<CashOnDeliveryAccountDTO>> GetCashOnDeliveryAccounts(CODStatus cODStatus);
    }

}
