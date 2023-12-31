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
using POST.Core.IServices.Utility;
using GIGL.POST.Core.Domain;
using System.Linq;
using POST.Core.IServices.Zone;
using POST.Core.IMessageService;
using System.Text;
using System.Security.Cryptography;
using POST.Core.DTO.Account;
using POST.Core.IServices.Account;
using System.Net;
using POST.Core.DTO.Shipments;
using POST.Core.DTO.Customers;
using POST.Core.DTO.ServiceCentres;
using POST.Core.DTO.Wallet;
using POST.Core.IServices.Node;
using POST.Core.DTO;
using POST.Core.IServices.Customers;

namespace POST.Services.Implementation.PaymentTransactions
{
    public class PaymentTransactionService : IPaymentTransactionService
    {
        private readonly IUnitOfWork _uow;
        private readonly IUserService _userService;
        private readonly IWalletService _walletService;
        private readonly IGlobalPropertyService _globalPropertyService;
        private readonly ICountryRouteZoneMapService _countryRouteZoneMapService;
        private readonly IMessageSenderService _messageSenderService;
        private readonly IFinancialReportService _financialReportService;
        private readonly INodeService _nodeService;
        private readonly INumberGeneratorMonitorService _numberGeneratorMonitorService;
        private readonly IAutoManifestAndGroupingService _autoManifestAndGroupingService;
        private readonly IAzapayPaymentService _azapayService;


        public PaymentTransactionService(IUnitOfWork uow, IUserService userService, IWalletService walletService,
            IGlobalPropertyService globalPropertyService, ICountryRouteZoneMapService countryRouteZoneMapService,
            IMessageSenderService messageSenderService, IFinancialReportService financialReportService, INodeService nodeService, 
            INumberGeneratorMonitorService numberGeneratorMonitorService, IAutoManifestAndGroupingService autoManifestAndGroupingService,
            IAzapayPaymentService azapayService)
        {
            _uow = uow;
            _userService = userService;
            _walletService = walletService;
            _globalPropertyService = globalPropertyService;
            _countryRouteZoneMapService = countryRouteZoneMapService;
            _messageSenderService = messageSenderService;
            _financialReportService = financialReportService;
            _nodeService = nodeService;
            _numberGeneratorMonitorService = numberGeneratorMonitorService;
            _autoManifestAndGroupingService = autoManifestAndGroupingService;
            _azapayService = azapayService;

            MapperConfig.Initialize();
        }

        //used for transaction, hence private
        private async Task<object> AddPaymentTransaction(PaymentTransactionDTO paymentTransaction)
        {
            if (paymentTransaction == null)
                throw new GenericException("Null Input");

            var transactionExist = await _uow.PaymentTransaction.ExistAsync(x => x.Waybill.Equals(paymentTransaction.Waybill));

            if (transactionExist == true)
                throw new GenericException($"Payment Transaction for {paymentTransaction.Waybill} already exist");

            var payment = Mapper.Map<PaymentTransaction>(paymentTransaction);
            _uow.PaymentTransaction.Add(payment);
            //await _uow.CompleteAsync();
            return new { Id = payment.PaymentTransactionId };
        }

        public async Task<PaymentTransactionDTO> GetPaymentTransactionById(string waybill)
        {
            var transaction = await _uow.PaymentTransaction.GetAsync(x => x.Waybill.Equals(waybill));
            return Mapper.Map<PaymentTransactionDTO>(transaction);
        }

        public Task<IEnumerable<PaymentTransactionDTO>> GetPaymentTransactions()
        {
            return Task.FromResult(Mapper.Map<IEnumerable<PaymentTransactionDTO>>(_uow.PaymentTransaction.GetAll()));
        }

        public async Task RemovePaymentTransaction(string waybill)
        {
            var transaction = await _uow.PaymentTransaction.GetAsync(x => x.Waybill.Equals(waybill));

            if (transaction == null)
            {
                throw new GenericException("Payment Transaction does not exist");
            }
            _uow.PaymentTransaction.Remove(transaction);
            await _uow.CompleteAsync();
        }

        public async Task UpdatePaymentTransaction(string waybill, PaymentTransactionDTO paymentTransaction)
        {
            if (paymentTransaction == null)
                throw new GenericException("Null Input");

            var payment = await _uow.PaymentTransaction.GetAsync(x => x.Waybill.Equals(waybill));
            if (payment == null)
                throw new GenericException($"No Payment Transaction exist for {waybill} waybill");

            payment.TransactionCode = paymentTransaction.TransactionCode;
            payment.PaymentStatus = paymentTransaction.PaymentStatus;
            payment.PaymentTypes = paymentTransaction.PaymentType;
            await _uow.CompleteAsync();
        }

        public async Task<bool> ProcessPaymentTransaction(PaymentTransactionDTO paymentTransaction)
        {
            var result = false;
            var returnWaybill = await _uow.ShipmentReturn.GetAsync(x => x.WaybillNew == paymentTransaction.Waybill);

            if (returnWaybill != null)
            {
                result = await ProcessReturnPaymentTransaction(paymentTransaction);
            }
            else
            {
                //Check if payment type is trnafer
                if (paymentTransaction.PaymentType == PaymentType.Transfer)
                {
                    var shipment = await _uow.Shipment.GetAsync(s => s.Waybill == paymentTransaction.Waybill);
                    if (shipment == null)
                        throw new GenericException($"Shipment with waybill {paymentTransaction.Waybill} does not exist");

                    //Use new transfer payment method for nigeria else normal transfer payment method for others
                    if (shipment.DepartureCountryId == 1)
                    {
                        //Check transfer processing partner used
                        if(paymentTransaction.ProcessingPartner == ProcessingPartnerType.Cellulant)
                        {
                            result = await ProcessNewPaymentTransactionForTransfer(paymentTransaction);
                        }
                        else
                        {
                            result = await ProcessNewPaymentTransactionForAzapayTransfer(paymentTransaction);
                        }
                    }
                    else
                    {
                        result = await ProcessNewPaymentTransaction(paymentTransaction);
                    }
                }
                else
                {
                    result = await ProcessNewPaymentTransaction(paymentTransaction);
                }
            }

            return result;
        }

        public async Task<bool> ProcessNewPaymentTransactionForTransfer(PaymentTransactionDTO paymentTransaction)
        {
            await ConfirmShipmentTransferDetails(paymentTransaction);
            return true;
        }

        public async Task<bool> ProcessNewPaymentTransactionForAzapayTransfer(PaymentTransactionDTO paymentTransaction)
        {
            await ConfirmShipmentTransferDetailsForAzapay(paymentTransaction);
            return true;
        }

        public async Task<bool> ProcessNewPaymentTransaction(PaymentTransactionDTO paymentTransaction)
        {
            var result = false;

            if (paymentTransaction == null)
                throw new GenericException("Null Input");

            // get the current user info
            var currentUserId = await _userService.GetCurrentUserId();
            paymentTransaction.UserId = currentUserId;

            //get Ledger, Invoice, shipment
            var generalLedgerEntity = await _uow.GeneralLedger.GetAsync(s => s.Waybill == paymentTransaction.Waybill);
            var invoiceEntity = await _uow.Invoice.GetAsync(s => s.Waybill == paymentTransaction.Waybill);
            var shipment = await _uow.Shipment.GetAsync(s => s.Waybill == paymentTransaction.Waybill);

            //all account customers payment should be settle by wallet automatically
            //settlement by wallet
            if (paymentTransaction.PaymentType == PaymentType.Wallet)
            {
                paymentTransaction.TransactionCode = shipment.CustomerCode;
                if (paymentTransaction.IsNotOwner)
                {
                    paymentTransaction.UserId = paymentTransaction.CustomerUserId;
                    paymentTransaction.TransactionCode = paymentTransaction.CustomerCode;
                    await ProcessWalletPaymentForShipment(paymentTransaction, shipment, invoiceEntity, generalLedgerEntity, paymentTransaction.UserId);
                }
                else
                {
                    await ProcessWalletTransaction(paymentTransaction, shipment, invoiceEntity, generalLedgerEntity, currentUserId);
                }
            }

            // create payment
            paymentTransaction.PaymentStatus = PaymentStatus.Paid;
            var paymentTransactionId = await AddPaymentTransaction(paymentTransaction);

            // update GeneralLedger
            generalLedgerEntity.IsDeferred = false;
            generalLedgerEntity.PaymentType = paymentTransaction.PaymentType;
            generalLedgerEntity.PaymentTypeReference = paymentTransaction.TransactionCode;

            //update invoice
            invoiceEntity.PaymentDate = DateTime.Now;
            invoiceEntity.PaymentMethod = paymentTransaction.PaymentType.ToString();

            await BreakdownPayments(invoiceEntity, paymentTransaction);

            invoiceEntity.PaymentStatus = paymentTransaction.PaymentStatus;
            invoiceEntity.PaymentTypeReference = paymentTransaction.TransactionCode;
            await _uow.CompleteAsync();

            //QR Code
            var deliveryNumber = await _uow.DeliveryNumber.GetAsync(s => s.Waybill == shipment.Waybill);

            //send sms to the customer
            var smsData = new ShipmentTrackingDTO
            {
                Waybill = shipment.Waybill,
                QRCode = deliveryNumber.SenderCode
            };

            if (shipment.DepartureCountryId == 1)
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
            }
            else
            {
                var countryRateConversion = await _countryRouteZoneMapService.GetZone(shipment.DestinationCountryId, shipment.DepartureCountryId);
                double amountToDebitDouble = (double)invoiceEntity.Amount * countryRateConversion.Rate;
                var amountToDebit = (decimal)Math.Round(amountToDebitDouble, 2);

                //Add to Financial Reports
                var financialReport = new FinancialReportDTO
                {
                    Source = ReportSource.Intl,
                    Waybill = shipment.Waybill,
                    PartnerEarnings = 0.0M,
                    GrandTotal = amountToDebit,
                    Earnings = amountToDebit,
                    Demurrage = 0.00M,
                    ConversionRate = countryRateConversion.Rate,
                    CountryId = shipment.DestinationCountryId
                };
                await _financialReportService.AddReport(financialReport);
            }

            //Send Email to Sender when Payment for International Shipment has being made
            if (invoiceEntity.IsInternational == true)
            {
                var shipmentDTO = Mapper.Map<ShipmentDTO>(shipment);
                await _messageSenderService.SendOverseasPaymentConfirmationMails(shipmentDTO);
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
                return true;
            }

            var shipmentObjDTO = Mapper.Map<ShipmentDTO>(shipment);
            if (shipment.DepartureServiceCentreId == 309)
            {
                await _messageSenderService.SendMessage(MessageType.HOUSTON, EmailSmsType.SMS, smsData);
                //Commented this out 15/06/2021 to implement new email 
                //await _messageSenderService.SendMessage(MessageType.CRT, EmailSmsType.Email, smsData);
                await _messageSenderService.SendEmailToCustomerForShipmentCreation(shipmentObjDTO);
            }
            else
            {

                //if (paymentTransaction.IsNotOwner)
                //{
                //  //TODO: SEND PAYMENT NOTIFICATION FOR ALREADY CREATED SHIPMENT 
                //}
                //Commented this out 15/06/2021 to implement new email
                //await _messageSenderService.SendMessage(MessageType.CRT, EmailSmsType.All, smsData);
                //sperated the previous implementation into sms / email
                //else
                //{
                //    await _messageSenderService.SendMessage(MessageType.CRT, EmailSmsType.SMS, smsData);
                //    await _messageSenderService.SendEmailToCustomerForShipmentCreation(shipmentObjDTO); 
                //}

                //Set sms template for Ghana or other coutry
                var currentInfo = await _userService.GetUserById(currentUserId);
                if (currentInfo.UserActiveCountryId == 1)
                {
                    if (shipmentObjDTO.ExpressDelivery)
                    {
                        await _messageSenderService.SendMessage(MessageType.CRTGF, EmailSmsType.SMS, smsData);
                    }
                    else
                    {
                        await _messageSenderService.SendMessage(MessageType.CRT, EmailSmsType.SMS, smsData);
                    }

                }
                else
                {
                    await _messageSenderService.SendMessage(MessageType.CRTGH, EmailSmsType.SMS, smsData);
                }
                await _messageSenderService.SendEmailToCustomerForShipmentCreation(shipmentObjDTO);
            }

            //grouping and manifesting shipment

            if (!shipment.IsGIGGOExtension)
            {
                if (!paymentTransaction.IsNotOwner)
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
            result = true;
            return result;
        }

        private async Task ConfirmShipmentTransferDetails(PaymentTransactionDTO paymentTransaction)
        {
            /* 
             1. The reference code exists and it is the same as what is entered
             2. We will check that the waybill that the reference code is being validated for, has not been validated before
             3. We will check that the amount cellulant says it has collected is the same amount for the cost of the waybill.
             */
            var transferDetails = _uow.TransferDetails.GetAllAsQueryable()
                                                        .Where(x => x.PaymentReference.ToLower() == paymentTransaction.TransactionCode.ToLower()).FirstOrDefault();

            if (transferDetails == null)
                throw new GenericException($"Transfer details does not exist for {paymentTransaction.TransactionCode}");

            if (!transferDetails.IsVerified)
            {
                var transactionAmount = 0.00m;
                transactionAmount = Convert.ToDecimal(transferDetails.Amount);
                if (paymentTransaction.TransactionCodes != null && paymentTransaction.TransactionCodes.Count > 0)
                {
                    List<TransferDetails> detailsList = new List<TransferDetails>();
                    foreach (var item in paymentTransaction.TransactionCodes)
                    {
                        if(string.IsNullOrEmpty( item.TransactionCode))
                            throw new GenericException($"Transaction code is required");

                        var transactionDetails = await _uow.TransferDetails.GetAsync(s => s.PaymentReference == item.TransactionCode && s.IsVerified == false);
                        if (transactionDetails != null)
                        {
                            detailsList.Add(transactionDetails);
                            transactionAmount += Convert.ToDecimal(transactionDetails.Amount);
                        }
                        else
                        {
                            throw new GenericException($"Transfer details does not exist for {item.TransactionCode}");
                        }
                    }

                    //Get waybill
                    //Get shipment amount

                    var shipment = await _uow.Shipment.GetAsync(s => s.Waybill == paymentTransaction.Waybill);
                    if (shipment == null)
                        throw new GenericException($"Shipment with waybill {paymentTransaction.Waybill} does not exist");

                    //Block if amount transfered is less than shipment amount
                    if (transactionAmount < shipment.GrandTotal)
                        throw new GenericException($"Transferred amount is less than shipment amount");

                    var paymentStatus = _uow.Invoice.GetAllAsQueryable().Where(s => s.Waybill == paymentTransaction.Waybill).FirstOrDefault().PaymentStatus;

                    //process shioments that have not been paid for
                    if (paymentStatus != PaymentStatus.Paid)
                    {
                        paymentTransaction.Waybill = paymentTransaction.Waybill;
                        await ProcessNewPaymentTransaction(paymentTransaction);

                        transferDetails.IsVerified = true;

                        if(detailsList.Count > 0)
                        {
                            foreach(var item in detailsList)
                            {
                                item.IsVerified = true;
                            }
                        }
                        await _uow.CompleteAsync();
                    }
                }
                else
                {
                    //Get the sum of all waybill(s)
                    var shipmentsTotal = 0.00m;
                    foreach (var item in paymentTransaction.Waybills)
                    {
                        var shipment = await _uow.Shipment.GetAsync(s => s.Waybill == item.Waybill);
                        if (shipment != null)
                        {
                            shipmentsTotal += shipment.GrandTotal;
                        }
                        else
                        {
                            throw new GenericException($"Shipment with waybill {item.Waybill} does not exist");
                        }
                    }

                    //Block if amount transfered is less than shipment amount
                    if (Convert.ToDecimal(transferDetails.Amount) < shipmentsTotal)
                        throw new GenericException($"Transferred amount is less than shipment amount");

                    //Get sum of all shipment that have been processed with the transaction code
                    var sumOfVerifiedShipments = _uow.Invoice.GetAllAsQueryable()
                                                     .Where(s => s.PaymentTypeReference.ToLower() == paymentTransaction.TransactionCode.ToLower() && s.PaymentStatus == PaymentStatus.Paid)
                                                     .Select(x => x.Amount).DefaultIfEmpty(0).Sum();

                    shipmentsTotal += sumOfVerifiedShipments;

                    //Block if amount transfered is less than shipment amount
                    if (Convert.ToDecimal(transferDetails.Amount) < shipmentsTotal)
                        throw new GenericException($"Transaction amount in reference code is less than processed shipment amount");

                    //Process each waybill as paid
                    foreach (var item in paymentTransaction.Waybills)
                    {
                        var paymentStatus = _uow.Invoice.GetAllAsQueryable().Where(s => s.Waybill == item.Waybill).FirstOrDefault().PaymentStatus;

                        //paymentStatuss.PaymentTypeReference
                        if (paymentStatus != PaymentStatus.Paid)
                        {
                            paymentTransaction.Waybill = item.Waybill;
                            await ProcessNewPaymentTransaction(paymentTransaction);
                        }
                    }

                    //Change transfer details to true if total shipments equals transfer amount
                    if (shipmentsTotal == Convert.ToDecimal(transferDetails.Amount))
                    {
                        transferDetails.IsVerified = true;
                        await _uow.CompleteAsync();
                    }
                }
            }
            else
            {
                throw new GenericException($"This transaction reference has been used already. Provide a valid transaction reference");
            }
        }

        private async Task ConfirmShipmentTransferDetailsForAzapay(PaymentTransactionDTO paymentTransaction)
        {
            /* 
             1. Validate the transfer with azapay
             2. We will check that the waybill that the reference code is being validated for, has not been validated before
             3. We will check that the amount azapay says it has collected is the same amount for the cost of the waybill.
             */
            var transferDetails = await _azapayService.ValidateTimedAccountRequest(paymentTransaction.AccountNumber);

            if (transferDetails == null)
                throw new GenericException($"Transfer details does not exist for {paymentTransaction.AccountNumber}");

            if(transferDetails.Data.Status.ToLower() != "confirmed")
                throw new GenericException($"Unable to confirm transfer made to {paymentTransaction.AccountNumber}");

            var shipment = await _uow.Shipment.GetAsync(s => s.Waybill == paymentTransaction.Waybill);
            if (shipment == null)
                throw new GenericException($"Shipment with waybill {paymentTransaction.Waybill} does not exist");

            //Block if amount transfered is less than shipment amount
            if (Convert.ToDecimal( transferDetails.Data.Amount) < shipment.GrandTotal)
                throw new GenericException($"Transferred amount is less than shipment amount");

            var paymentStatus = _uow.Invoice.GetAllAsQueryable().Where(s => s.Waybill == paymentTransaction.Waybill).FirstOrDefault().PaymentStatus;

            //process shioments that have not been paid for
            if (paymentStatus != PaymentStatus.Paid)
            {
                await ProcessNewPaymentTransaction(paymentTransaction);
            }
        }

        private async Task ProcessWalletTransaction(PaymentTransactionDTO paymentTransaction, Shipment shipment, Invoice invoiceEntity, GeneralLedger generalLedgerEntity, string currentUserId)
        {
            //I used transaction code to represent wallet number when processing for wallet
            var wallet = await _walletService.GetWalletById(paymentTransaction.TransactionCode);

            decimal amountToDebit = invoiceEntity.Amount;

            amountToDebit = await GetActualAmountToDebit(shipment, amountToDebit);

            //Additions for Ecommerce customers (Max wallet negative payment limit)
            //var shipment = _uow.Shipment.SingleOrDefault(s => s.Waybill == paymentTransaction.Waybill);
            if (shipment != null && CompanyType.Ecommerce.ToString() == shipment.CompanyType && !paymentTransaction.FromApp)
            {
                //Gets the customer wallet limit for ecommerce
                decimal ecommerceNegativeWalletLimit = await GetEcommerceWalletLimit(shipment);

                //deduct the price for the wallet and update wallet transaction table
                if (wallet.Balance - amountToDebit < (Math.Abs(ecommerceNegativeWalletLimit) * (-1)))
                {
                    throw new GenericException(" Shipment successfully created, however payment could not be processed for ecommerce customer due to insufficient wallet balance ");
                }
            }

            //check if user has sufficient balance for only individual customers
            if (shipment != null && shipment.CustomerType.Contains("Individual"))
            {
                if (wallet.Balance < amountToDebit)
                {
                    throw new GenericException("Insufficient wallet balance ");
                }
            }

            //for other customers
            //deduct the price for the wallet and update wallet transaction table
            //--Update April 25, 2019: Corporate customers should be debited from wallet
            if (shipment != null && CompanyType.Client.ToString() == shipment.CompanyType)
            {
                if (wallet.Balance < amountToDebit)
                {
                    throw new GenericException("Shipment successfully created, however payment could not be processed for customer due to insufficient wallet balance ");
                }
            }

            if (shipment != null && paymentTransaction.FromApp == true)
            {
                if (wallet.Balance < amountToDebit)
                {
                    throw new GenericException("Shipment successfully created, however payment could not be processed for customer due to insufficient wallet balance ");
                }
            }

            int[] serviceCenterIds = { };

            if (!paymentTransaction.FromApp)
            {
                serviceCenterIds = await _userService.GetPriviledgeServiceCenters();
            }
            else
            {
                var gigGOServiceCenter = await _userService.GetGIGGOServiceCentre();
                serviceCenterIds = new int[] { gigGOServiceCenter.ServiceCentreId };
            }

            var newWalletTransaction = new WalletTransaction
            {
                WalletId = wallet.WalletId,
                Amount = amountToDebit,
                DateOfEntry = DateTime.Now,
                ServiceCentreId = serviceCenterIds[0],
                UserId = currentUserId,
                CreditDebitType = CreditDebitType.Debit,
                PaymentType = PaymentType.Wallet,
                Waybill = paymentTransaction.Waybill,
                Description = generalLedgerEntity.Description
            };
            //get the balance after transaction
            if (newWalletTransaction.CreditDebitType == CreditDebitType.Credit)
            {
                newWalletTransaction.BalanceAfterTransaction = wallet.Balance + newWalletTransaction.Amount;
            }
            else
            {
                newWalletTransaction.BalanceAfterTransaction = wallet.Balance - newWalletTransaction.Amount;
            }
            wallet.Balance = wallet.Balance - amountToDebit;

            _uow.WalletTransaction.Add(newWalletTransaction);
        }


        private async Task BreakdownPayments(Invoice invoiceEntity, PaymentTransactionDTO paymentTransaction)
        {
            if (paymentTransaction.PaymentType == PaymentType.Cash)
            {
                invoiceEntity.Cash = invoiceEntity.Amount;
            }
            else if (paymentTransaction.PaymentType == PaymentType.Transfer)
            {
                invoiceEntity.Transfer = invoiceEntity.Amount;
            }
            else if (paymentTransaction.PaymentType == PaymentType.Pos)
            {
                invoiceEntity.Pos = invoiceEntity.Amount;
            }
        }

        public async Task<bool> ProcessReturnPaymentTransaction(PaymentTransactionDTO paymentTransaction)
        {
            var result = false;

            if (paymentTransaction == null)
                throw new GenericException("Null Input");

            // get the current user info
            var currentUserId = await _userService.GetCurrentUserId();

            //get Ledger, Invoice, shipment
            var generalLedgerEntity = await _uow.GeneralLedger.GetAsync(s => s.Waybill == paymentTransaction.Waybill);
            var invoiceEntity = await _uow.Invoice.GetAsync(s => s.Waybill == paymentTransaction.Waybill);
            var shipment = _uow.Shipment.SingleOrDefault(s => s.Waybill == paymentTransaction.Waybill);

            //all account customers payment should be settle by wallet automatically
            //settlement by wallet
            if (shipment.CustomerType == CustomerType.Company.ToString() || paymentTransaction.PaymentType == PaymentType.Wallet)
            {
                //I used transaction code to represent wallet number when processing for wallet
                if (string.IsNullOrWhiteSpace(paymentTransaction.TransactionCode))
                {
                    paymentTransaction.TransactionCode = shipment.CustomerCode;
                }
                var wallet = await _walletService.GetWalletById(paymentTransaction.TransactionCode);

                //Additions for Ecommerce customers (Max wallet negative payment limit)
                //var shipment = _uow.Shipment.SingleOrDefault(s => s.Waybill == paymentTransaction.Waybill);
                //if (shipment != null && CompanyType.Ecommerce.ToString() == shipment.CompanyType)
                //{
                //    //Gets the customer wallet limit for ecommerce
                //    decimal ecommerceNegativeWalletLimit = await GetEcommerceWalletLimit(shipment);

                //    //deduct the price for the wallet and update wallet transaction table
                //    if (wallet.Balance - invoiceEntity.Amount < (System.Math.Abs(ecommerceNegativeWalletLimit) * (-1)))
                //    {
                //        throw new GenericException("Ecommerce Customer. Insufficient Balance in the Wallet");
                //    }
                //}


                decimal amountToDebit = invoiceEntity.Amount;

                amountToDebit = await GetActualAmountToDebit(shipment, amountToDebit);

                //for other customers
                //deduct the price for the wallet and update wallet transaction table
                if (shipment != null && CompanyType.Ecommerce.ToString() != shipment.CompanyType)
                {
                    if (wallet.Balance < amountToDebit)
                    {
                        throw new GenericException("Insufficient Balance in the Wallet");
                    }
                }

                var serviceCenterIds = await _userService.GetPriviledgeServiceCenters();

                var newWalletTransaction = new WalletTransaction
                {
                    WalletId = wallet.WalletId,
                    Amount = amountToDebit,
                    DateOfEntry = DateTime.Now,
                    ServiceCentreId = serviceCenterIds[0],
                    UserId = currentUserId,
                    CreditDebitType = CreditDebitType.Debit,
                    PaymentType = PaymentType.Wallet,
                    Waybill = paymentTransaction.Waybill,
                    Description = generalLedgerEntity.Description
                };

                if (newWalletTransaction.CreditDebitType == CreditDebitType.Credit)
                {
                    newWalletTransaction.BalanceAfterTransaction = wallet.Balance + newWalletTransaction.Amount;
                }
                else
                {
                    newWalletTransaction.BalanceAfterTransaction = wallet.Balance - newWalletTransaction.Amount;
                }

                wallet.Balance = wallet.Balance - amountToDebit;

                _uow.WalletTransaction.Add(newWalletTransaction);
            }

            // create payment
            paymentTransaction.UserId = currentUserId;
            paymentTransaction.PaymentStatus = PaymentStatus.Paid;
            var paymentTransactionId = await AddPaymentTransaction(paymentTransaction);

            // update GeneralLedger
            generalLedgerEntity.IsDeferred = false;
            generalLedgerEntity.PaymentType = paymentTransaction.PaymentType;
            generalLedgerEntity.PaymentTypeReference = paymentTransaction.TransactionCode;

            //update invoice
            invoiceEntity.PaymentDate = DateTime.Now;
            invoiceEntity.PaymentMethod = paymentTransaction.PaymentType.ToString();
            await BreakdownPayments(invoiceEntity, paymentTransaction);
            invoiceEntity.PaymentStatus = paymentTransaction.PaymentStatus;
            invoiceEntity.PaymentTypeReference = paymentTransaction.TransactionCode;
            await _uow.CompleteAsync();

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
                Waybill = shipment.Waybill,
                QRCode = deliveryNumber.SenderCode
            };

            var shipmentObjDTO = Mapper.Map<ShipmentDTO>(shipment);
            if (shipment.DepartureServiceCentreId == 309)
            {
                await _messageSenderService.SendMessage(MessageType.HOUSTON, EmailSmsType.SMS, smsData);
                //Commented this out 15/06/2021 to implement new email 
                //await _messageSenderService.SendMessage(MessageType.CRT, EmailSmsType.Email, smsData);
                await _messageSenderService.SendEmailToCustomerForShipmentCreation(shipmentObjDTO);
            }
            else
            {
                //Commented this out 15/06/2021 to implement new email
                //await _messageSenderService.SendMessage(MessageType.CRT, EmailSmsType.All, smsData);

                //sperated the previous implementation into sms / email
                await _messageSenderService.SendMessage(MessageType.CRT, EmailSmsType.SMS, smsData);
                await _messageSenderService.SendEmailToCustomerForShipmentCreation(shipmentObjDTO);
            }

            result = true;
            return result;

        }

        private async Task<decimal> GetEcommerceWalletLimit(Shipment shipment)
        {
            decimal ecommerceNegativeWalletLimit = 0;

            //Get the Customer Wallet Limit Category
            var companyObj = await _uow.Company.GetAsync(x => x.CustomerCode.ToLower() == shipment.CustomerCode.ToLower());
            var customerWalletLimitCategory = companyObj.CustomerCategory;

            var userActiveCountryId = await _userService.GetUserActiveCountryId();

            switch (customerWalletLimitCategory)
            {
                case CustomerCategory.Gold:
                    {
                        //get max negati ve wallet limit from GlobalProperty
                        var ecommerceNegativeWalletLimitObj = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.EcommerceGoldNegativeWalletLimit, userActiveCountryId);
                        ecommerceNegativeWalletLimit = decimal.Parse(ecommerceNegativeWalletLimitObj.Value);
                        break;
                    }
                case CustomerCategory.Premium:
                    {
                        //get max negati ve wallet limit from GlobalProperty
                        var ecommerceNegativeWalletLimitObj = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.EcommercePremiumNegativeWalletLimit, userActiveCountryId);
                        ecommerceNegativeWalletLimit = decimal.Parse(ecommerceNegativeWalletLimitObj.Value);
                        break;
                    }
                case CustomerCategory.Normal:
                    {
                        //get max negati ve wallet limit from GlobalProperty
                        var ecommerceNegativeWalletLimitObj = await _globalPropertyService.GetGlobalProperty(GlobalPropertyType.EcommerceNegativeWalletLimit, userActiveCountryId);
                        ecommerceNegativeWalletLimit = decimal.Parse(ecommerceNegativeWalletLimitObj.Value);
                        break;
                    }
                default:
                    break;
            }

            return ecommerceNegativeWalletLimit;
        }

        private async Task<decimal> GetActualAmountToDebit(Shipment shipment, decimal amountToDebit)
        {
            //1. Get Customer Country detail
            int customerCountryId = 0;
            Rank rank = Rank.Basic;

            if (UserChannelType.Ecommerce.ToString() == shipment.CompanyType || UserChannelType.Corporate.ToString() == shipment.CompanyType)
            {
                //customerCountryId = _uow.Company.GetAllAsQueryable()
                //    .Where(x => x.CustomerCode.ToLower() == shipment.CustomerCode.ToLower()).Select(x => x.UserActiveCountryId).FirstOrDefault();

                var customer = _uow.Company.GetAllAsQueryable().Where(x => x.CustomerCode.ToLower() == shipment.CustomerCode.ToLower()).FirstOrDefault();
                if (customer != null)
                {
                    customerCountryId = customer.UserActiveCountryId;
                    rank = customer.Rank;
                }
            }
            else
            {
                customerCountryId = _uow.IndividualCustomer.GetAllAsQueryable().Where(x => x.CustomerCode.ToLower() == shipment.CustomerCode.ToLower()).Select(x => x.UserActiveCountryId).FirstOrDefault();
            }

            //check if the customer country is same as the country in the user table
            var user = await _uow.User.GetUserByChannelCode(shipment.CustomerCode);
            if (user != null)
            {
                if (user.UserActiveCountryId != customerCountryId)
                {
                    throw new GenericException($"Payment Failed for waybill {shipment.Waybill}, Contact Customer Care", $"{(int)HttpStatusCode.Forbidden}");
                }
            }

            //2. If the customer country !== Departure Country, Convert the payment
            if (customerCountryId != shipment.DepartureCountryId)
            {
                var countryRateConversion = await _countryRouteZoneMapService.GetZone(customerCountryId, shipment.DepartureCountryId);

                double amountToDebitDouble = (double)amountToDebit * countryRateConversion.Rate;

                amountToDebit = (decimal)Math.Round(amountToDebitDouble, 2);
            }

            //if the shipment is International Shipment & Payment was initiated before the shipment get to Nigeria
            //5% discount should be give to the customer
            if (shipment.IsInternational)
            {
                //check if the shipment has a scan of AISN in Tracking Table, 
                //bool isPresent = await _uow.ShipmentTracking.ExistAsync(x => x.Waybill == shipment.Waybill 
                //&& x.Status == ShipmentScanStatus.AISN.ToString());

                //if (!isPresent)
                //{
                //    //amountToDebit = amountToDebit * 0.95m;
                //    var discount = GetDiscountForInternationalShipmentBasedOnRank(rank);
                //    amountToDebit = amountToDebit * discount;
                //}           

                if (UserChannelType.Ecommerce.ToString() == shipment.CompanyType)
                {
                    var discount = await GetDiscountForInternationalShipmentBasedOnRank(rank, customerCountryId);
                    amountToDebit = amountToDebit * discount;
                }
            }
            return amountToDebit;
        }

        private async Task<decimal> GetDiscountForInternationalShipmentBasedOnRank(Rank rank, int countryId)
        {
            decimal percentage = 0.00M;

            if (rank == Rank.Class)
            {
                var globalProperty = await _uow.GlobalProperty.GetAsync(s => s.Key == GlobalPropertyType.InternationalRankClassDiscount.ToString() && s.CountryId == countryId);
                if (globalProperty != null)
                {
                    percentage = Convert.ToDecimal(globalProperty.Value);
                }
            }
            else
            {
                var globalProperty = await _uow.GlobalProperty.GetAsync(s => s.Key == GlobalPropertyType.InternationalBasicClassDiscount.ToString() && s.CountryId == countryId);
                if (globalProperty != null)
                {
                    percentage = Convert.ToDecimal(globalProperty.Value);
                }
            }

            decimal discount = ((100M - percentage) / 100M);
            return discount;
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


        public async Task<bool> ProcessPaymentTransactionGIGGO(PaymentTransactionDTO paymentTransaction)
        {
            var result = false;

            //check if waybill is from BOT
            var preshipment = await _uow.PreShipmentMobile.GetPreshipmentMobileByWaybill(paymentTransaction.Waybill);
            if (preshipment != null)
            {
                //CHECK IF IS BOT USER
                var customer = await _uow.Company.GetAsync(x => x.CustomerCode == preshipment.CustomerCode);
                if (customer != null && customer.TransactionType == WalletTransactionType.BOT)
                {
                    var nodePayload = new CreateShipmentNodeDTO()
                    {
                        waybillNumber = preshipment.Waybill,
                        customerId = preshipment.CustomerCode,
                        locality = preshipment.SenderLocality,
                        receiverAddress = preshipment.ReceiverAddress,
                        vehicleType = preshipment.VehicleType,
                        value = preshipment.Value,
                        zone = preshipment.ZoneMapping,
                        senderAddress = preshipment.SenderAddress,
                        receiverLocation = new NodeLocationDTO()
                        {
                            lng = preshipment.ReceiverLng,
                            lat = preshipment.ReceiverLat
                        },
                        senderLocation = new NodeLocationDTO()
                        {
                            lng = preshipment.SenderLng,
                            lat = preshipment.SenderLat
                        }
                    };
                    await _nodeService.CreateShipment(nodePayload);
                    var shipmentToUpdate = await _uow.PreShipmentMobile.GetAsync(x => x.Waybill == paymentTransaction.Waybill);
                    if (shipmentToUpdate != null)
                    {
                        //Update shipment to shipment created
                        shipmentToUpdate.shipmentstatus = "Shipment created";
                        var userId = await _userService.GetCurrentUserId();

                        var user = await _uow.PreShipmentMobile.GetBotUserWithPhoneNo(shipmentToUpdate.SenderPhoneNumber);
                        if (String.IsNullOrEmpty(user.CustomerCode))
                        {
                            //create this user first
                            var newCustomer = new CustomerDTO()
                            {
                                PhoneNumber = shipmentToUpdate.SenderPhoneNumber,
                                FirstName = shipmentToUpdate.SenderName,
                                LastName = shipmentToUpdate.SenderName,
                                UserActiveCountryId = 1,
                                City = shipmentToUpdate.SenderLocality,
                                CustomerType = CustomerType.IndividualCustomer,
                                Address = shipmentToUpdate.SenderAddress,
                                ReturnAddress = shipmentToUpdate.SenderAddress,
                                Gender = Gender.Male,
                                CustomerCode = await _numberGeneratorMonitorService.GenerateNextNumber(NumberGeneratorType.CustomerCodeIndividual)
                            };
                            var newIndCustomer = Mapper.Map<IndividualCustomer>(newCustomer);
                            _uow.IndividualCustomer.Add(newIndCustomer);
                            await _uow.CompleteAsync();
                            user = newCustomer;
                        }
                        if (!String.IsNullOrEmpty(user.CustomerCode))
                        {
                            var regUser = await _uow.User.GetUserByChannelCode(user.CustomerCode);
                            if (regUser != null)
                            {
                                userId = regUser.Id;
                            }
                            //Pin Generation 
                            var message = new MobileShipmentCreationMessageDTO
                            {
                                SenderPhoneNumber = shipmentToUpdate.SenderPhoneNumber,
                                WaybillNumber = shipmentToUpdate.Waybill
                            };
                            var number = await _globalPropertyService.GenerateDeliveryCode();
                            var deliveryNumber = new DeliveryNumber
                            {
                                SenderCode = number,
                                IsUsed = false,
                                Waybill = shipmentToUpdate.Waybill
                            };
                            _uow.DeliveryNumber.Add(deliveryNumber);
                            message.QRCode = deliveryNumber.SenderCode;

                            if (user.CustomerType == CustomerType.IndividualCustomer)
                            {
                                var indCust = await _uow.IndividualCustomer.GetAsync(x => x.CustomerCode == user.CustomerCode);
                                if (indCust != null)
                                {
                                    shipmentToUpdate.CustomerCode = user.CustomerCode;
                                    shipmentToUpdate.CustomerType = CustomerType.IndividualCustomer.ToString();
                                    shipmentToUpdate.CompanyType = CompanyType.Client.ToString();
                                    shipmentToUpdate.UserId = userId;
                                    shipmentToUpdate.SenderPhoneNumber = indCust.PhoneNumber;
                                    message.SenderName = indCust.FirstName + " " + indCust.LastName;
                                }
                            }
                            else
                            {
                                var compCust = await _uow.Company.GetAsync(x => x.CustomerCode == user.CustomerCode);
                                if (compCust != null)
                                {
                                    shipmentToUpdate.CustomerCode = user.CustomerCode;
                                    shipmentToUpdate.CustomerType = CustomerType.IndividualCustomer.ToString();
                                    shipmentToUpdate.CompanyType = compCust.CompanyType.ToString();
                                    shipmentToUpdate.UserId = userId;
                                    shipmentToUpdate.SenderPhoneNumber = compCust.PhoneNumber;
                                    message.SenderName = compCust.Name;
                                }
                            }
                            await _messageSenderService.SendMessage(MessageType.MCS, EmailSmsType.SMS, message);
                        }
                    }
                }
                await _uow.CompleteAsync();
            }
            result = true;
            return result;
        }


        private async Task<bool> ProcessWalletPaymentForShipment(PaymentTransactionDTO paymentTransaction, Shipment shipment, Invoice invoiceEntity, GeneralLedger generalLedgerEntity, string currentUserId)
        {
            //I used transaction code to represent wallet number when processing for wallet
            var wallet = await _walletService.GetWalletById(paymentTransaction.TransactionCode);

            decimal amountToDebit = invoiceEntity.Amount;

            amountToDebit = await GetActualAmountToDebitForNotOwner(shipment, amountToDebit, paymentTransaction);

            //Additions for Ecommerce customers (Max wallet negative payment limit)
            //var shipment = _uow.Shipment.SingleOrDefault(s => s.Waybill == paymentTransaction.Waybill);
            if (shipment != null && CompanyType.Ecommerce.ToString() == shipment.CompanyType && !paymentTransaction.FromApp)
            {
                //Gets the customer wallet limit for ecommerce
                decimal ecommerceNegativeWalletLimit = await GetEcommerceWalletLimit(shipment);

                //deduct the price for the wallet and update wallet transaction table
                if (wallet.Balance - amountToDebit < (Math.Abs(ecommerceNegativeWalletLimit) * (-1)))
                {
                    throw new GenericException("Payment could not be processed for customer due to insufficient wallet balance");
                }
            }

            //for other customers
            //deduct the price for the wallet and update wallet transaction table
            //--Update April 25, 2019: Corporate customers should be debited from wallet
            if (shipment != null && CompanyType.Client.ToString() == shipment.CompanyType)
            {
                if (wallet.Balance < amountToDebit)
                {
                    throw new GenericException("Payment could not be processed for customer due to insufficient wallet balance ");
                }
            }

            if (shipment != null && paymentTransaction.FromApp == true)
            {
                if (wallet.Balance < amountToDebit)
                {
                    throw new GenericException("Payment could not be processed for customer due to insufficient wallet balance ");
                }
            }

            int[] serviceCenterIds = { };

            if (!paymentTransaction.FromApp)
            {
                int centerIds = await _userService.GetPriviledgeServiceCenter();
                serviceCenterIds = new int[] { centerIds };
            }
            else
            {
                var gigGOServiceCenter = await _userService.GetGIGGOServiceCentre();
                serviceCenterIds = new int[] { gigGOServiceCenter.ServiceCentreId };
            }

            var newWalletTransaction = new WalletTransaction
            {
                WalletId = wallet.WalletId,
                Amount = amountToDebit,
                DateOfEntry = DateTime.Now,
                ServiceCentreId = serviceCenterIds[0],
                UserId = currentUserId,
                CreditDebitType = CreditDebitType.Debit,
                PaymentType = PaymentType.Wallet,
                Waybill = paymentTransaction.Waybill,
                Description = generalLedgerEntity.Description
            };
            //get the balance after transaction
            if (newWalletTransaction.CreditDebitType == CreditDebitType.Credit)
            {
                newWalletTransaction.BalanceAfterTransaction = wallet.Balance + newWalletTransaction.Amount;
            }
            else
            {
                newWalletTransaction.BalanceAfterTransaction = wallet.Balance - newWalletTransaction.Amount;
            }
            wallet.Balance = wallet.Balance - amountToDebit;

            _uow.WalletTransaction.Add(newWalletTransaction);
            return true;
        }

        private async Task<decimal> GetActualAmountToDebitForNotOwner(Shipment shipment, decimal amountToDebit, PaymentTransactionDTO paymentTransaction)
        {
            //1. Get Customer Country detail
            int customerCountryId = 0;
            Rank rank = Rank.Basic;
            var user = await _uow.User.GetUserByChannelCode(paymentTransaction.CustomerCode);
            if (user != null)
            {
                if (UserChannelType.Ecommerce == user.UserChannelType || UserChannelType.Corporate == user.UserChannelType)
                {
                    var customer = _uow.Company.GetAllAsQueryable().Where(x => x.CustomerCode.ToLower() == paymentTransaction.CustomerCode.ToLower()).FirstOrDefault();
                    if (customer != null)
                    {
                        customerCountryId = customer.UserActiveCountryId;
                        rank = customer.Rank;
                    }
                }
                else
                {
                    customerCountryId = _uow.IndividualCustomer.GetAllAsQueryable().Where(x => x.CustomerCode.ToLower() == paymentTransaction.CustomerCode.ToLower()).Select(x => x.UserActiveCountryId).FirstOrDefault();
                }

                //check if the customer country is same as the country in the user table
                if (user.UserActiveCountryId != customerCountryId)
                {
                    throw new GenericException($"Payment Failed for waybill {shipment.Waybill}, Contact Customer Care", $"{(int)HttpStatusCode.Forbidden}");
                }

                //2. If the customer country !== Departure Country, Convert the payment
                if (customerCountryId != shipment.DepartureCountryId)
                {
                    var countryRateConversion = await _countryRouteZoneMapService.GetZone(customerCountryId, shipment.DepartureCountryId);

                    double amountToDebitDouble = (double)amountToDebit * countryRateConversion.Rate;

                    amountToDebit = (decimal)Math.Round(amountToDebitDouble, 2);
                }

                //3. if the shipment is International Shipment & Payment was initiated before the shipment get to Nigeria
                //5% discount should be give to the customer
                if (shipment.IsInternational)
                {
                    if (UserChannelType.Ecommerce.ToString() == shipment.CompanyType)
                    {
                        var discount = await GetDiscountForInternationalShipmentBasedOnRank(rank, customerCountryId);
                        amountToDebit = amountToDebit * discount;
                    }
                }
            }

            return amountToDebit;
        }




    }
}