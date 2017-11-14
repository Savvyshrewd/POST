﻿using GIGLS.Core.DTO.PaymentTransactions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GIGLS.Core.IServices.PaymentTransactions
{
    public interface IPaymentTransactionService : IServiceDependencyMarker
    {
        Task<IEnumerable<PaymentTransactionDTO>> GetPaymentTransactions();
        Task<PaymentTransactionDTO> GetPaymentTransactionById(string waybill);
        Task<object> AddPaymentTransaction(PaymentTransactionDTO paymentTransaction);
        Task UpdatePaymentTransaction(string waybill, PaymentTransactionDTO paymentTransaction);
        Task RemovePaymentTransaction(string waybill);
    }
}
