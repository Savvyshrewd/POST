﻿using POST.Core.DTO.Wallet;
using System.Threading.Tasks;
using POST.Core.DTO.OnlinePayment;
using POST.Core.Domain.Wallet;
using System.Net.Http;
using POST.Core.DTO;
using Newtonsoft.Json.Linq;

namespace POST.Core.IServices.Wallet
{
    public interface IPaystackPaymentService : IServiceDependencyMarker 
    {
        Task<bool> MakePayment(string LiveSecret, WalletPaymentLogDTO wpd);
        Task<bool> VerifyPayment(string reference, string livesecret);  
        Task<PaystackWebhookDTO> VerifyPayment(string reference);
        Task<bool> VerifyAndValidateWallet(PaystackWebhookDTO webhook);
        Task<PaymentResponse> VerifyAndProcessPayment(string referenceCode);
        Task<PaymentResponse> VerifyAndValidateWallet(string referenceCode);
        Task<PaystackWebhookDTO> VerifyPaymentMobile(string reference, string UserId);

        Task VerifyAndValidateMobilePayment(PaystackWebhookDTO webhook);
        Task<PaystackWebhookDTO> VerifyAndValidateMobilePayment(string reference);
        Task<PaystackWebhookDTO> ProcessPaymentForWaybillUsingPin(WaybillPaymentLog waybillPaymentLog, string pin);
        Task<ResponseDTO> VerifyBVN(string bvnNo);
        Task<CreateNubanAccountResponseDTO> CreateUserNubanAccount(CreateNubanAccountDTO nubanAccountDTO);
        Task<JObject> GetNubanAccountProviders();
        Task<NubanCreateCustomerDTO> CreateNubanCustomer(CreateNubanAccountDTO nubanAccountDTO);
        Task CreditCorporateAccount(NubanCustomerResponse customer);
    }
}
