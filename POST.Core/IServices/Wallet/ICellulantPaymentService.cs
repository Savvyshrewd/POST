﻿using POST.Core.DTO;
using POST.Core.DTO.OnlinePayment;
using POST.CORE.DTO.Report;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POST.Core.IServices.Wallet
{
    public interface ICellulantPaymentService : IServiceDependencyMarker
    {
        Task<string> GetCellulantKey();
        Task<string> DecryptKey(string encrytedKey);
        Task<bool> AddCellulantTransferDetails(TransferDetailsDTO transferDetailsDTO);
        Task<TransferDetailsDTO> GetAllTransferDetails(string reference);
        Task<List<TransferDetailsDTO>> GetTransferDetails(BaseFilterCriteria baseFilter);
        Task<List<TransferDetailsDTO>> GetTransferDetailsByAccountNumber(BaseFilterCriteria baseFilter);
        Task<CellulantResponseDTO> CheckoutEncryption(CellulantPayloadDTO payload);
        Task<CellulantPaymentResponse> VerifyAndValidatePayment(CellulantWebhookDTO webhook);
        Task<CellulantPaymentResponse> VerifyAndValidatePaymentForWebhook(CellulantWebhookDTO webhook);
        Task<CellulantTransferResponsePayload> Transfer(CellulantTransferPayload payload);
        Task<bool> CODCallBack(CODCallBackDTO cod);
        Task<CellulantTransferResponsePayload> CelullantTransfer(CellulantTransferDTO transferDTO);
        Task<CellulantPushPaymentStatusResponse> UpdateCODShipmentOnCallBack(PushPaymentStatusRequstPayload payload);
        Task<bool> GetTransferStatus(string craccount);
        Task<CODPaymentResponse> GetCODPaymentReceivedStatus(string craccount);
        Task<bool> UpdateCODShipmentOnCallBackStellas(CODCallBackDTO cod);
        Task<GenerateAccountDTO> GenerateAccountNumberCellulant(GenerateAccountPayloadDTO payload);
        Task<PaystackWebhookDTO> VerifyAndValidateMobilePayment(string reference);
    }
}
