﻿using POST.Core.IServices.PaymentTransactions;
using System.Collections.Generic;
using System.Threading.Tasks;
using POST.Core.DTO.PaymentTransactions;
using POST.Core;
using AutoMapper;
using POST.Infrastructure;
using POST.Core.Domain;
using POST.Core.Enums;
using System;
using POST.Core.IServices.User;
using POST.Core.IServices.Wallet;
using POST.Core.Domain.Wallet;
using POST.Core.IMessageService;
using System.Security.Cryptography;
using System.Text;
using POST.Core.DTO.Account;
using POST.Core.IServices.Account;
using GIGL.POST.Core.Domain;
using System.Linq;
using POST.Core.IServices.Utility;

namespace POST.Services.Implementation.PaymentTransactions
{
    public class PaymentPartialTransactionService : IPaymentPartialTransactionService
    {
        private readonly IUnitOfWork _uow;
        private readonly IUserService _userService;
        private readonly IWalletService _walletService;
        private readonly IMessageSenderService _messageSenderService;
        private readonly IFinancialReportService _financialReportService;
        private readonly INumberGeneratorMonitorService _numberGeneratorMonitorService;
        private readonly IAutoManifestAndGroupingService _autoManifestAndGroupingService;

        public PaymentPartialTransactionService(IUnitOfWork uow, IUserService userService, IWalletService walletService, 
            IMessageSenderService messageSenderService, IFinancialReportService financialReportService, INumberGeneratorMonitorService numberGeneratorMonitorService, IAutoManifestAndGroupingService autoManifestAndGroupingService)
        {
            _uow = uow;
            _userService = userService;
            _walletService = walletService;
            _messageSenderService = messageSenderService;
            _financialReportService = financialReportService;
            _numberGeneratorMonitorService = numberGeneratorMonitorService;
            _autoManifestAndGroupingService = autoManifestAndGroupingService;
            MapperConfig.Initialize();
        }

        //used for transaction, hence private
        private async Task<object> AddPaymentPartialTransaction(PaymentPartialTransactionDTO paymentPartialTransaction)
        {
            if (paymentPartialTransaction == null)
                throw new GenericException("Null Input");

            var payment = Mapper.Map<PaymentPartialTransaction>(paymentPartialTransaction);
            _uow.PaymentPartialTransaction.Add(payment);
            return await Task.FromResult(new { Id = payment.PaymentPartialTransactionId });
        }

        public async Task<IEnumerable<PaymentPartialTransactionDTO>> GetPaymentPartialTransactionById(string waybill)
        {
            var transaction = await _uow.PaymentPartialTransaction.FindAsync(x => x.Waybill.Equals(waybill));
            return Mapper.Map<IEnumerable<PaymentPartialTransactionDTO>>(transaction);
        }

        public Task<IEnumerable<PaymentPartialTransactionDTO>> GetPaymentPartialTransactions()
        {
            return Task.FromResult(Mapper.Map<IEnumerable<PaymentPartialTransactionDTO>>(_uow.PaymentPartialTransaction.GetAll()));
        }

        public async Task RemovePaymentPartialTransaction(string waybill)
        {
            var transaction = await _uow.PaymentPartialTransaction.GetAsync(x => x.Waybill.Equals(waybill));

            if (transaction == null)
            {
                throw new GenericException("Payment Partial Transaction does not exist");
            }
            _uow.PaymentPartialTransaction.Remove(transaction);
            await _uow.CompleteAsync();
        }

        public async Task UpdatePaymentPartialTransaction(string waybill, PaymentPartialTransactionDTO paymentPartialTransaction)
        {
            if (paymentPartialTransaction == null)
                throw new GenericException("Null Input");

            var payment = await _uow.PaymentPartialTransaction.GetAsync(x => x.Waybill.Equals(waybill));
            if (payment == null)
                throw new GenericException($"No Payment Partial Transaction exist for {waybill} waybill");

            payment.TransactionCode = paymentPartialTransaction.TransactionCode;
            payment.PaymentStatus = paymentPartialTransaction.PaymentStatus;
            payment.PaymentType = paymentPartialTransaction.PaymentType;
            await _uow.CompleteAsync();
        }

        private async Task ConfirmSingleTransferDetails(string transactionCode, decimal amount)
        {
            /* 
             1. The reference code exists and it is the same as what is entered
             2. We will check that the amount that the reference code is being validated for, has not been validated before
             3. We will check that the amount cellulant says it has collected is the same amount user claimed to have paid.
             */
            var transferDetails = _uow.TransferDetails.GetAllAsQueryable()
                                                        .Where(x => x.PaymentReference.ToLower() == transactionCode.ToLower()).FirstOrDefault();

            if (transferDetails == null)
                throw new GenericException($"Transfer details does not exist for {transactionCode}");

            if (!transferDetails.IsVerified)
            {
                //Block if amount transfered is less than shipment amount
                if (Convert.ToDecimal(transferDetails.Amount) < Convert.ToDecimal(amount))
                    throw new GenericException($"Transaction amount in reference code is less than processed amount");

                transferDetails.IsVerified = true;
                await _uow.CompleteAsync();
            }
            else
            {
                throw new GenericException($"This transaction reference has been used already. Provide a valid transaction reference");
            }
        }
        public async Task<bool> ProcessPaymentPartialTransaction(PaymentPartialTransactionProcessDTO paymentPartialTransactionProcessDTO)
        {
            var result = false;

            if (paymentPartialTransactionProcessDTO == null)
                throw new GenericException("Null Input");

            decimal cash = 0;
            decimal pos = 0;
            decimal transfer = 0;
            string cashType = null;
            string posType = null;
            string transferType = null;

            foreach (var item in paymentPartialTransactionProcessDTO.PaymentPartialTransactions)
            {
                if (item.PaymentType == PaymentType.Cash)
                {
                    cash += item.Amount;
                    cashType = item.PaymentType.ToString();
                }
                else if (item.PaymentType == PaymentType.Pos)
                {
                    pos += item.Amount;
                    posType = item.PaymentType.ToString();
                }
                else if (item.PaymentType == PaymentType.Transfer)
                {
                    await ConfirmSingleTransferDetails(item.TransactionCode, item.Amount);
                    transfer += item.Amount;
                    transferType = item.PaymentType.ToString();
                }
            }

            // get the current user info
            var currentUserId = await _userService.GetCurrentUserId();

            //get Ledger and Invoice
            var waybill = paymentPartialTransactionProcessDTO.Waybill;
            var generalLedgerEntity = await _uow.GeneralLedger.GetAsync(s => s.Waybill == waybill);
            var invoiceEntity = await _uow.Invoice.GetAsync(s => s.Waybill == waybill);
            var shipment = await _uow.Shipment.GetAsync(s => s.Waybill == waybill);

            //get the GrandTotal Amount to be paid
            var grandTotal = invoiceEntity.Amount;

            //get total amount already paid
            decimal totalAmountAlreadyPaid = 0;
            var partialTransactionsForWaybill = await _uow.PaymentPartialTransaction.FindAsync(x => x.Waybill.Equals(waybill));
            foreach (var item in partialTransactionsForWaybill)
            {
                totalAmountAlreadyPaid += item.Amount;
            }

            //get total amount customer is paying
            decimal totalAmountPaid = 0;
            foreach (var paymentPartialTransaction in paymentPartialTransactionProcessDTO.PaymentPartialTransactions)
            {
                //settlement by wallet
                if (paymentPartialTransaction.PaymentType == PaymentType.Wallet)
                {
                    //I used transaction code to represent wallet number when process for wallet
                    var wallet = await _walletService.GetWalletById(paymentPartialTransaction.TransactionCode);

                    //deduct the price for the wallet and update wallet transaction table
                    if (wallet.Balance < paymentPartialTransaction.Amount)
                    {
                        throw new GenericException("Insufficient Balance in the Wallet");
                    }

                    wallet.Balance = wallet.Balance - paymentPartialTransaction.Amount;

                    var serviceCenterIds = await _userService.GetPriviledgeServiceCenters();

                    var newWalletTransaction = new WalletTransaction
                    {
                        WalletId = wallet.WalletId,
                        Amount = paymentPartialTransaction.Amount,
                        DateOfEntry = DateTime.Now,
                        ServiceCentreId = serviceCenterIds[0],
                        UserId = currentUserId,
                        CreditDebitType = CreditDebitType.Debit,
                        PaymentType = PaymentType.Wallet,
                        Waybill = waybill,
                        Description = generalLedgerEntity.Description
                    };

                    _uow.WalletTransaction.Add(newWalletTransaction);
                }

                // create paymentPartial
                paymentPartialTransaction.Waybill = waybill;
                paymentPartialTransaction.UserId = currentUserId;
                paymentPartialTransaction.PaymentStatus = PaymentStatus.Paid;
                var paymentTransactionId = await AddPaymentPartialTransaction(paymentPartialTransaction);


                totalAmountPaid += paymentPartialTransaction.Amount;
            }
            // get the balance
            decimal balanceAmount = (grandTotal - totalAmountAlreadyPaid) - totalAmountPaid;

            //if customer over pays, throw an exception
            if (balanceAmount < 0)
            {
                throw new GenericException($"The amount of {totalAmountPaid} you are trying to pay is greater than {grandTotal - totalAmountAlreadyPaid} balance you are to pay.");
            }

            /////2.  When payment is complete and balance is 0
            if (balanceAmount == 0)
            {
                foreach (var item in partialTransactionsForWaybill)
                {
                    if (item.PaymentType == PaymentType.Cash)
                    {
                        cash += item.Amount;
                        cashType = item.PaymentType.ToString();
                    }
                    else if (item.PaymentType == PaymentType.Pos)
                    {
                        pos += item.Amount;
                        posType = item.PaymentType.ToString();
                    }
                    else if (item.PaymentType == PaymentType.Transfer)
                    {
                        transfer += item.Amount;
                        transferType = item.PaymentType.ToString();
                    }
                }

                // update GeneralLedger
                generalLedgerEntity.IsDeferred = false;
                generalLedgerEntity.PaymentType = PaymentType.Partial;
                //generalLedgerEntity.PaymentTypeReference = paymentPartialTransaction.TransactionCode;

                //update invoice
                invoiceEntity.PaymentDate = DateTime.Now;
                //invoiceEntity.PaymentMethod = PaymentType.Partial.ToString();
                invoiceEntity.PaymentMethod = PaymentType.Partial.ToString() + " - " + (cashType != null ? cashType + "(" + cash + ") " : "") +
                                                                      (posType != null ? posType + "(" + pos + ") " : "") +
                                                                      (transferType != null ? transferType + "(" + transfer + ") " : "");
                invoiceEntity.Cash = cash;
                invoiceEntity.Transfer = transfer;
                invoiceEntity.Pos = pos;
                invoiceEntity.PaymentStatus = PaymentStatus.Paid;
            }

            await _uow.CompleteAsync();
            result = true;

            /////2.  When payment is complete and balance is 0, send sms to customer
            if (balanceAmount == 0)
            {
                if (shipment != null)
                {
                    //Add to Financial Reports
                    var financialReport = new FinancialReportDTO
                    {
                        Source = ReportSource.Agility,
                        Waybill = shipment.Waybill,
                        PartnerEarnings = 0.0M,
                        GrandTotal = invoiceEntity.Amount,
                        Earnings = invoiceEntity.Amount,
                        Demurrage = 0.00M,
                        CountryId = invoiceEntity.CountryId
                    };

                    await _financialReportService.AddReport(financialReport);

                    //QR Code
                    var deliveryNumber = await _uow.DeliveryNumber.GetAsync(s => s.Waybill == shipment.Waybill);

                    //send sms to the customer
                    var smsData = new Core.DTO.Shipments.ShipmentTrackingDTO
                    {
                        Waybill = invoiceEntity.Waybill,
                        QRCode = deliveryNumber.SenderCode
                    };

                    if (shipment.DepartureServiceCentreId == 309)
                    {
                        await _messageSenderService.SendMessage(MessageType.HOUSTON, EmailSmsType.SMS, smsData);
                        await _messageSenderService.SendMessage(MessageType.CRT, EmailSmsType.Email, smsData);
                    }
                    else
                    {
                        await _messageSenderService.SendMessage(MessageType.CRT, EmailSmsType.All, smsData);
                    }
                }
            }
            if (result)
            {
                //grouping and manifesting shipment
                if (!shipment.IsGIGGOExtension)
                {
                    if (shipment.IsBulky)
                    {
                        await _autoManifestAndGroupingService.MappingWaybillNumberToGroupForBulk(shipment.Waybill);
                    }
                    else
                    {
                        await _autoManifestAndGroupingService.MappingWaybillNumberToGroup(shipment.Waybill);
                    }
                }
            }

            return result;
        }

        private async Task<DeliveryNumberDTO> GenerateDeliveryNumber(int value, string waybill)
        {
            int maxSize = 6;
            char[] chars = new char[54];
            string a;
            a = "abcdefghjkmnpqrstuvwxyzABCDEFGHJKMNPQRSTUVWXYZ23456789";
            chars = a.ToCharArray();
            int size = maxSize;
            byte[] data = new byte[1];
            RNGCryptoServiceProvider crypto = new RNGCryptoServiceProvider();
            crypto.GetNonZeroBytes(data);
            size = maxSize;
            data = new byte[size];
            crypto.GetNonZeroBytes(data);
            StringBuilder result = new StringBuilder(size);
            foreach (byte b in data)
            { result.Append(chars[b % (chars.Length - 1)]); }
            var strippedText = result.ToString();
            var number = new DeliveryNumber
            {
                SenderCode = "DN" + strippedText.ToUpper(),
                IsUsed = false,
                Waybill = waybill
            };
            var deliverynumberDTO = Mapper.Map<DeliveryNumberDTO>(number);
            _uow.DeliveryNumber.Add(number);
            await _uow.CompleteAsync();
            return await Task.FromResult(deliverynumberDTO);
        }

    }
}