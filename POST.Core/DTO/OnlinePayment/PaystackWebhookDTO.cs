﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace POST.Core.DTO.OnlinePayment
{
    public class PaystackWebhookDTO
    {
        public PaystackWebhookDTO()
        {
            data = new Data();
        }
        public string Event { get; set; }
        public string Message { get; set; }
        public bool Status { get; set; }
        public Data data { get; set; }

        //waybill Grand Total
        public decimal Amount { get; set; }
    }

    public class Data
    {
        public Data()
        {
            Authorization = new Authorization();
        }
        public string Status { get; set; }
        public string Reference { get; set; }
        public string Authorization_url { get; set; }
        public decimal Amount { get; set; }
        public string Gateway_Response { get; set; }
        public string Display_Text { get; set; }
        public string Message { get; set; }
        public Authorization Authorization { get; set; }
    }

    public class Authorization
    {
        [JsonProperty("authorization_code")]
        public string AuthorizationCode { get; set; }

        [JsonProperty("card_type")]
        public string CardType { get; set; }

        [JsonProperty("last4")]
        public string Last4 { get; set; }

        [JsonProperty("exp_month")]
        public string ExpMonth { get; set; }

        [JsonProperty("exp_year")]
        public string ExpYear { get; set; }

        [JsonProperty("bin")]
        public string Bin { get; set; }

        [JsonProperty("bank")]
        public string Bank { get; set; }

        [JsonProperty("channel")]
        public string Channel { get; set; }

        [JsonProperty("reusable")]
        public bool? Reusable { get; set; }

        [JsonProperty("country_code")]
        public string CountryCode { get; set; }
    }

    public class PaymentResponse
    {
        public bool Result { get; set; }
        public string Message { get; set; }
        public string GatewayResponse { get; set; }
        public string Status { get; set; }
        public bool ResponseStatus { get; set; }
    }

    public enum WaybillWalletPaymentType
    {
        Waybill,
        Wallet
    }

    public class FlutterWebhookDTO
    {
        public FlutterWebhookDTO()
        {
            data = new FlutterResponseData();
        }
        public string Message { get; set; }
        public string Status { get; set; }
        public FlutterResponseData data { get; set; }
    }

    public class FlutterTransactionWebhookDTO
    {
        public FlutterTransactionWebhookDTO()
        {
            data = new List<FlutterResponseData>();
        }
        public string Status { get; set; }
        public List<FlutterResponseData> data { get; set; }
    }

    public class FlutterResponseData
    {
        public FlutterResponseData()
        {
            validateInstructions = new ValidateInstructions();
            Card = new Card();
        }
        public string Status { get; set; }
        public int Id { get; set; }
        public int TxId { get; set; }
        public string TX_Ref { get; set; }
        public decimal Amount { get; set; }
        public string ChargeResponseMessage { get; set; }
        public string ChargeResponseCode { get; set; }
        public string ChargeMessage { get; set; }
        public string PaymentType { get; set; }
        public string FlwRef { get; set; }
        public string Acctvalrespcode { get; set; }
        public string Acctvalrespmsg { get; set; }
        public string ChargeCode { get; set; }
        public string Processor_Response { get; set; }
        public ValidateInstructions validateInstructions { get; set; }
        public Card Card { get; set; }
    }

    public class Card
    {
        [JsonProperty("expirymonth")]
        public string ExpiryMonth { get; set; }

        [JsonProperty("expiryyear")]
        public string ExpiryYear { get; set; }

        [JsonProperty("cardBIN")]
        public string CardBIN { get; set; }

        [JsonProperty("last4digits")]
        public string Last4Digits { get; set; }

        [JsonProperty("brand")]
        public string Brand { get; set; }

        [JsonProperty("type")]
        public string CardType { get; set; }

    }

    public class ValidateInstructions
    {
        public string Instruction { get; set; }
    }

    public class USSDWebhook
    {
        public string Status { get; set; }
        public decimal Amount { get; set; }
        public string MSISDN { get; set; }
        public string Order_Reference { get; set; }
        public string Transaction_Ref { get; set; }
    }
    
    public class BonusAddOn
    {
        public string Description { get; set; }
        public decimal Amount { get; set; }
        public bool BonusAdded { get; set; }
    }


    public class CreateNubanAccountDTO
    {
        public int customer { get; set; }
        public string preferred_bank { get; set; }
        public string email { get; set; }
        public string phone { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
    }



    public class CreateNubanAccountResponseDTO
    {
        public CreateNubanAccountResponseDTO()
        {
            data = new NubanDataResponse();
        }
        public string status { get; set; }
        public string message { get; set; }
        public NubanDataResponse data { get; set; }
        public bool succeeded { get; set; }
    }

    public class NubanDataResponse
    {
        public NubanDataResponse()
        {
            bank = new NubanBank();
            assignment = new NubanAssignment();
            customer = new NubanCustomer();
        }
        public NubanAssignment assignment { get; set; }
        public NubanCustomer customer { get; set; }
        public string account_name { get; set; }
        public string account_number { get; set; }
        public string assigned { get; set; }
        public string currency { get; set; }
        public string active { get; set; }
        public NubanBank bank { get; set; }
    }

    public class NubanBank
    {
        public int id { get; set; }
        public string name { get; set; }
        public string slug { get; set; }
    }

    public class NubanCustomer
    {
        public int id { get; set; }
        public string first_name { get; set; }
        public string last_name { get; set; }
        public string email { get; set; }
        public string customer_code { get; set; }
        public string phone { get; set; }
        public string risk_action { get; set; }

    }


    public class NubanAssignment
    {
        public int integration { get; set; }
        public int assignee_id { get; set; }
        public string assignee_type { get; set; }
        public bool expired { get; set; }
        public string account_type { get; set; }
        public DateTime assigned_at { get; set; }
        public string phone { get; set; }
        public string risk_action { get; set; }

    }


    public class NubanCustomerDataResponse
    {
      public string email { get; set; }
      public int integration { get; set; }
      public int id { get; set; }
      public string domain { get; set; }
      public string customer_code { get; set; }
      public bool identified { get; set; }
      public string identifications { get; set; }
      public string createdAt { get; set; }
      public string updatedAt { get; set; }
    }

    public class NubanCreateCustomerDTO
    {
        public NubanCreateCustomerDTO()
        {
            data = new NubanCustomerDataResponse();
        }
        public string status { get; set; }
        public string message { get; set; }
        public NubanCustomerDataResponse data { get; set; }
        public bool succeeded { get; set; }
    }

    public class NubanCustomerResponse
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string CustomerCode { get; set; }
        public string Reference { get; set; }
        public decimal Amount { get; set; }
    }

    public class Payment
    {
        [JsonProperty("MSISDN")]
        public long MSISDN { get; set; }

        [JsonProperty("payerClientName")]
        public string PayerClientName { get; set; }

        [JsonProperty("currencyCode")]
        public string CurrencyCode { get; set; }

        [JsonProperty("amountPaid")]
        public int AmountPaid { get; set; }

        [JsonProperty("cpgTransactionID")]
        public string CpgTransactionID { get; set; }

        [JsonProperty("serviceCode")]
        public string ServiceCode { get; set; }

        [JsonProperty("payerTransactionID")]
        public string PayerTransactionID { get; set; }

        [JsonProperty("hubOverallStatus")]
        public int HubOverallStatus { get; set; }

        [JsonProperty("accountNumber")]
        public string AccountNumber { get; set; }

        [JsonProperty("customerName")]
        public string CustomerName { get; set; }

        [JsonProperty("payerClientCode")]
        public string PayerClientCode { get; set; }

        [JsonProperty("datePaymentReceived")]
        public string DatePaymentReceived { get; set; }
    }

    public class CellulantWebhookDTO
    {
        [JsonProperty("serviceCode")]
        public string ServiceCode { get; set; }

        [JsonProperty("MSISDN")]
        public string MSISDN { get; set; }

        [JsonProperty("originalRequestCurrencyCode")]
        public string OriginalRequestCurrencyCode { get; set; }

        [JsonProperty("originalRequestAmount")]
        public int OriginalRequestAmount { get; set; }

        [JsonProperty("checkoutRequestID")]
        public int CheckoutRequestID { get; set; }

        [JsonProperty("requestCurrencyCode")]
        public string RequestCurrencyCode { get; set; }

        [JsonProperty("requestAmount")]
        public string RequestAmount { get; set; }

        [JsonProperty("accountNumber")]
        public string AccountNumber { get; set; }

        [JsonProperty("requestStatusCode")]
        public int RequestStatusCode { get; set; }

        [JsonProperty("requestStatusDescription")]
        public string RequestStatusDescription { get; set; }

        [JsonProperty("merchantTransactionID")]
        public string MerchantTransactionID { get; set; }

        [JsonProperty("requestDate")]
        public string RequestDate { get; set; }

        [JsonProperty("currencyCode")]
        public string CurrencyCode { get; set; }

        [JsonProperty("amountPaid")]
        public int AmountPaid { get; set; }

        [JsonProperty("serviceChargeAmount")]
        public int ServiceChargeAmount { get; set; }

        [JsonProperty("payments")]
        public List<Payment> Payments { get; set; }

        [JsonProperty("failedPayments")]
        public List<object> FailedPayments { get; set; }
    }

    public class CellulantPaymentResponse
    {
        [JsonProperty("checkoutRequestID")]
        public int CheckoutRequestID { get; set; }

        [JsonProperty("merchantTransactionID")]
        public string MerchantTransactionID { get; set; }

        [JsonProperty("statusCode")]
        public string StatusCode { get; set; }

        [JsonProperty("statusDescription")]
        public string StatusDescription { get; set; }

        [JsonProperty("receiptNumber")]
        public string ReceiptNumber { get; set; }
    }

    #region Korapay
    public class KorapayCustomer
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }
    }

    public class KoarapayInitializeCharge
    {
        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("redirect_url")]
        public string RedirectUrl { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("reference")]
        public string Reference { get; set; }

        [JsonProperty("narration")]
        public string Narration { get; set; }

        [JsonProperty("customer")]
        public KorapayCustomer Customer { get; set; }
    }

    public class KorapayInitializeChargeData
    {
        [JsonProperty("reference")]
        public string Reference { get; set; }

        [JsonProperty("checkout_url")]
        public string CheckoutUrl { get; set; }
    }

    public class KorapayInitializeChargeResponse
    {
        [JsonProperty("status")]
        public bool Status { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("data")]
        public KorapayInitializeChargeData Data { get; set; }
    }

    public class KorapayWebhookData
    {
        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("amount_expected")]
        public decimal AmountExpected { get; set; }

        [JsonProperty("fee")]
        public decimal Fee { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("reference")]
        public string Reference { get; set; }

        [JsonProperty("payment_reference")]
        public string PaymentReference { get; set; }

        [JsonProperty("transaction_status")]
        public string TransactionStatus { get; set; }
    }

    public class KorapayWebhookDTO
    {
        [JsonProperty("event")]
        public string Event { get; set; }

        [JsonProperty("data")]
        public KorapayWebhookData Data { get; set; }
    }

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class KoraPayerBankAccount
    {
        [JsonProperty("account_number")]
        public string AccountNumber { get; set; }

        [JsonProperty("account_name")]
        public string AccountName { get; set; }

        [JsonProperty("bank_name")]
        public string BankName { get; set; }
    }

    public class KorapayQueryData
    {
        [JsonProperty("reference")]
        public string Reference { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("fee")]
        public decimal Fee { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("payer_bank_account")]
        public KoraPayerBankAccount PayerBankAccount { get; set; }
    }

    public class KorapayQueryChargeResponse
    {
        [JsonProperty("status")]
        public bool Status { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("data")]
        public KorapayQueryData Data { get; set; }
    }

    #endregion

    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Credentials
    {
        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("password")]
        public string Password { get; set; }
    }

    public class Packet
    {
        [JsonProperty("serviceCode")]
        public string ServiceCode { get; set; }

        [JsonProperty("MSISDN")]
        public string MSISDN { get; set; }

        [JsonProperty("invoiceNumber")]
        public string InvoiceNumber { get; set; }

        [JsonProperty("accountNumber")]
        public string AccountNumber { get; set; }

        [JsonProperty("payerTransactionID")]
        public string PayerTransactionID { get; set; }

        [JsonProperty("amount")]
        public decimal Amount { get; set; }

        [JsonProperty("hubID")]
        public string HubID { get; set; }

        [JsonProperty("narration")]
        public string Narration { get; set; }

        [JsonProperty("datePaymentReceived")]
        public string DatePaymentReceived { get; set; }

        [JsonProperty("extraData")]
        public string ExtraData { get; set; }

        [JsonProperty("currencyCode")]
        public string CurrencyCode { get; set; }

        [JsonProperty("customerNames")]
        public string CustomerNames { get; set; }

        [JsonProperty("paymentMode")]
        public string PaymentMode { get; set; }
    }

    public class Payload
    {
        public Payload()
        {
            Credentials = new Credentials();
            Packet = new List<Packet>();
        }
        [JsonProperty("credentials")]
        public Credentials Credentials { get; set; }

        [JsonProperty("packet")]
        public List<Packet> Packet { get; set; }
    }

    public class CellulantTransferPayload
    {
        public CellulantTransferPayload()
        {
            Payload = new Payload();
        }
        [JsonProperty("countryCode")]
        public string CountryCode { get; set; }

        [JsonProperty("function")]
        public string Function { get; set; }

        [JsonProperty("payload")]
        public Payload Payload { get; set; }
    }





    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class AuthStatus
    {
        [JsonProperty("authStatusCode")]
        public int AuthStatusCode { get; set; }

        [JsonProperty("authStatusDescription")]
        public string AuthStatusDescription { get; set; }
    }

    public class Result
    {
        [JsonProperty("statusCode")]
        public int StatusCode { get; set; }

        [JsonProperty("statusDescription")]
        public string StatusDescription { get; set; }

        [JsonProperty("payerTransactionID")]
        public string PayerTransactionID { get; set; }

        [JsonProperty("beepTransactionID")]
        public string BeepTransactionID { get; set; }

        [JsonProperty("invoiceNumber")]
        public string InvoiceNumber { get; set; }
    }

    public class CellulantTransferResponsePayload
    {
        [JsonProperty("authStatus")]
        public AuthStatus AuthStatus { get; set; }

        [JsonProperty("results")]
        public List<Result> Results { get; set; }
    }


    public class CellulantTransferDTO
    {
        public string CustomerCode { get; set; }

        public decimal Amount { get; set; }
        public string RefNo { get; set; }
        public string ClientRefNo { get; set; }
        public string Waybill { get; set; }
    }



    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class PacketPushPaymentStatus
    {
        [JsonProperty("statusCode")]
        public string StatusCode { get; set; }

        [JsonProperty("statusDescription")]
        public string StatusDescription { get; set; }

        [JsonProperty("payerTransactionID")]
        public string PayerTransactionID { get; set; }

        [JsonProperty("beepTransactionID")]
        public string BeepTransactionID { get; set; }

        [JsonProperty("receiptNumber")]
        public string ReceiptNumber { get; set; }

        [JsonProperty("receiverNarration")]
        public string ReceiverNarration { get; set; }

        [JsonProperty("function")]
        public string Function { get; set; }

        [JsonProperty("msisdn")]
        public string Msisdn { get; set; }

        [JsonProperty("serviceCode")]
        public string ServiceCode { get; set; }

        [JsonProperty("paymentDate")]
        public string PaymentDate { get; set; }

        [JsonProperty("clientCode")]
        public string ClientCode { get; set; }

        [JsonProperty("extraData")]
        public string ExtraData { get; set; }
    }

    public class PayloadPushPaymentStatus
    {
        [JsonProperty("packet")]
        public PacketPushPaymentStatus Packet { get; set; }

        [JsonProperty("credentials")]
        public Credentials Credentials { get; set; }
    }

    public class PushPaymentStatusRequstPayload
    {
        [JsonProperty("function")]
        public string Function { get; set; }

        [JsonProperty("countryCode")]
        public string CountryCode { get; set; }

        [JsonProperty("payload")]
        public PayloadPushPaymentStatus Payload { get; set; }
    }



    public class PushPaymentStatusResponseResult
    {
        [JsonProperty("statusCode")]
        public int StatusCode { get; set; }

        [JsonProperty("statusDescription")]
        public string StatusDescription { get; set; }

        [JsonProperty("payerTransactionID")]
        public string PayerTransactionID { get; set; }

        [JsonProperty("beepTransactionID")]
        public string BeepTransactionID { get; set; }
    }

    public class CellulantPushPaymentStatusResponse
    {
        public CellulantPushPaymentStatusResponse()
        {
            AuthStatus = new AuthStatus();
            Results = new PushPaymentStatusResponseResult();
        }
        [JsonProperty("authStatus")]
        public AuthStatus AuthStatus { get; set; }

        [JsonProperty("results")]
        public PushPaymentStatusResponseResult Results { get; set; }
    }


    public class ExtraData
    {
        [JsonProperty("callbackUrl")]
        public string callbackUrl { get; set; }

        [JsonProperty("destinationBankCode")]
        public string DestinationBankCode { get; set; }

        [JsonProperty("destinationAccountName")]
        public string DestinationAccountName { get; set; }
        [JsonProperty("destinationAccountNo")]
        public string DestinationAccountNo { get; set; }
        [JsonProperty("destinationBank")]
        public string DestinationBank { get; set; }
    }

    public class ExtraDataCallBack
    {
        [JsonProperty("callbackUrl")]
        public string CallBackUrl { get; set; }

        [JsonProperty("destinationBankCode")]
        public string DestinationBankCode { get; set; }

        [JsonProperty("destinationAccountName")]
        public string DestinationAccountName { get; set; }
        [JsonProperty("destinationAccountNo")]
        public string DestinationAccountNo { get; set; }
        [JsonProperty("destinationBank")]
        public string DestinationBank { get; set; }
        [JsonProperty("hubID")]
        public string HubId { get; set; }
    }


    public class CellulantPaymentLoginDto
    {
        [JsonProperty("grant_type")]
        public string GrantType { get; set; }

        [JsonProperty("client_id")]
        public string ClientId { get; set; }

        [JsonProperty("client_secret")]
        public string ClientSecret { get; set; }
    }

    public class CellulantPaymentLoginResponseDto : CellulantPaymentBaseResponseDto
    {
        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty("access_token")]
        public string AccessToken { get; set; }
    }

    public class CellulantPaymentQueryStatusDto
    {
        [JsonProperty("merchantTransactionID")]
        public string MerchantTransactionID { get; set; }

        [JsonProperty("serviceCode")]
        public string ServiceCode { get; set; }
    }

    public class CellulantPaymentQueryPayment
    {
        [JsonProperty("payerTransactionID")]
        public string PayerTransactionID { get; set; }

        [JsonProperty("MSISDN")]
        public long MSISDN { get; set; }

        [JsonProperty("accountNumber")]
        public string AccountNumber { get; set; }

        [JsonProperty("customerName")]
        public string CustomerName { get; set; }

        [JsonProperty("amountPaid")]
        public int AmountPaid { get; set; }

        [JsonProperty("payerClientCode")]
        public string PayerClientCode { get; set; }

        [JsonProperty("cpgTransactionID")]
        public string CpgTransactionID { get; set; }

        [JsonProperty("paymentDate")]
        public string PaymentDate { get; set; }

        [JsonProperty("clientName")]
        public string ClientName { get; set; }

        [JsonProperty("clientDisplayName")]
        public string ClientDisplayName { get; set; }

        [JsonProperty("currencyCode")]
        public string CurrencyCode { get; set; }

        [JsonProperty("currencyID")]
        public int CurrencyID { get; set; }

        [JsonProperty("paymentID")]
        public int PaymentID { get; set; }

        [JsonProperty("hubOverallStatus")]
        public int HubOverallStatus { get; set; }

        [JsonProperty("clientCategoryID")]
        public int ClientCategoryID { get; set; }

        [JsonProperty("clientCategoryName")]
        public string ClientCategoryName { get; set; }

        [JsonProperty("payerNarration")]
        public string PayerNarration { get; set; }
    }

    public class CellulantPaymentQueryResults
    {
        [JsonProperty("checkoutRequestID")]
        public int CheckoutRequestID { get; set; }

        [JsonProperty("merchantTransactionID")]
        public string MerchantTransactionID { get; set; }

        [JsonProperty("MSISDN")]
        public long MSISDN { get; set; }

        [JsonProperty("accountNumber")]
        public string AccountNumber { get; set; }

        [JsonProperty("requestDate")]
        public string RequestDate { get; set; }

        [JsonProperty("requestStatusCode")]
        public int RequestStatusCode { get; set; }

        [JsonProperty("serviceName")]
        public string ServiceName { get; set; }

        [JsonProperty("serviceCode")]
        public string ServiceCode { get; set; }

        [JsonProperty("requestCurrencyCode")]
        public string RequestCurrencyCode { get; set; }

        [JsonProperty("requestAmount")]
        public int RequestAmount { get; set; }

        [JsonProperty("paymentCurrencyCode")]
        public string PaymentCurrencyCode { get; set; }

        [JsonProperty("amountPaid")]
        public int AmountPaid { get; set; }

        [JsonProperty("shortUrl")]
        public string ShortUrl { get; set; }

        [JsonProperty("redirectTrigger")]
        public string RedirectTrigger { get; set; }

        [JsonProperty("payments")]
        public List<CellulantPaymentQueryPayment> Payments { get; set; }

        [JsonProperty("failedPayments")]
        public List<object> FailedPayments { get; set; }
    }

    public class CellulantPaymentQueryStatusResponseDto : CellulantPaymentBaseResponseDto
    {
        [JsonProperty("status")]
        public CellulantPaymentQueryStatus Status { get; set; }

        [JsonProperty("results")]
        public CellulantPaymentQueryResults Results { get; set; }
    }

    public class CellulantPaymentQueryStatus
    {
        [JsonProperty("statusCode")]
        public int StatusCode { get; set; }

        [JsonProperty("statusDescription")]
        public string StatusDescription { get; set; }
    }

    public class CellulantPaymentAcknowledgeDto
    {
        [JsonProperty("merchantTransactionID")]
        public string MerchantTransactionID { get; set; }

        [JsonProperty("checkoutRequestID")]
        public int CheckoutRequestID { get; set; }

        [JsonProperty("receiptNumber")]
        public string ReceiptNumber { get; set; }

        [JsonProperty("statusCode")]
        public int StatusCode { get; set; }

        [JsonProperty("statusDescription")]
        public string StatusDescription { get; set; }
    }

    public class CellulantPaymentAcknowledgePayment
    {
        [JsonProperty("paymentID")]
        public int PaymentID { get; set; }

        [JsonProperty("checkoutRequestID")]
        public int CheckoutRequestID { get; set; }

        [JsonProperty("cpgTransactionID")]
        public string CpgTransactionID { get; set; }

        [JsonProperty("payerTransactionID")]
        public string PayerTransactionID { get; set; }

        [JsonProperty("MSISDN")]
        public long MSISDN { get; set; }

        [JsonProperty("accountNumber")]
        public string AccountNumber { get; set; }

        [JsonProperty("customerName")]
        public string CustomerName { get; set; }

        [JsonProperty("amountPaid")]
        public int AmountPaid { get; set; }

        [JsonProperty("merchantReceipt")]
        public string MerchantReceipt { get; set; }

        [JsonProperty("payerNarration")]
        public string PayerNarration { get; set; }

        [JsonProperty("receiverNarration")]
        public string ReceiverNarration { get; set; }

        [JsonProperty("hubOverallStatus")]
        public int HubOverallStatus { get; set; }

        [JsonProperty("statusCodeDesc")]
        public string StatusCodeDesc { get; set; }

        [JsonProperty("currencyID")]
        public int CurrencyID { get; set; }

        [JsonProperty("payerClientCode")]
        public string PayerClientCode { get; set; }

        [JsonProperty("payerClientName")]
        public string PayerClientName { get; set; }

        [JsonProperty("payerClientDisplayName")]
        public string PayerClientDisplayName { get; set; }

        [JsonProperty("ownerClientCode")]
        public string OwnerClientCode { get; set; }

        [JsonProperty("ownerClientName")]
        public string OwnerClientName { get; set; }

        [JsonProperty("ownerClientDisplayName")]
        public string OwnerClientDisplayName { get; set; }

        [JsonProperty("merchantTransactionID")]
        public string MerchantTransactionID { get; set; }

        [JsonProperty("convertedAmount")]
        public int ConvertedAmount { get; set; }

        [JsonProperty("totalPayableAmount")]
        public int TotalPayableAmount { get; set; }

        [JsonProperty("datePaymentReceived")]
        public string DatePaymentReceived { get; set; }

        [JsonProperty("datePaymentAcknowledged")]
        public object DatePaymentAcknowledged { get; set; }

        [JsonProperty("dateCreated")]
        public string DateCreated { get; set; }

        [JsonProperty("currencyCode")]
        public string CurrencyCode { get; set; }

        [JsonProperty("countryCode")]
        public string CountryCode { get; set; }
    }

    public class CellulantPaymentAcknowledgeResults
    {
        [JsonProperty("checkoutRequestID")]
        public int CheckoutRequestID { get; set; }

        [JsonProperty("merchantTransactionID")]
        public string MerchantTransactionID { get; set; }

        [JsonProperty("requestStatusCode")]
        public int RequestStatusCode { get; set; }

        [JsonProperty("payments")]
        public List<CellulantPaymentAcknowledgePayment> Payments { get; set; }
    }

    public class CellulantPaymentAcknowledgeResponseDto : CellulantPaymentBaseResponseDto
    {
        [JsonProperty("status")]
        public CellulantPaymentAcknowledgeStatus Status { get; set; }

        [JsonProperty("results")]
        public CellulantPaymentAcknowledgeResults Results { get; set; }
    }

    public class CellulantPaymentAcknowledgeStatus
    {
        [JsonProperty("statusCode")]
        public int StatusCode { get; set; }

        [JsonProperty("statusDescription")]
        public string StatusDescription { get; set; }
    }

    public class CellulantPaymentBaseResponseDto
    {
        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("status_code")]
        public int StatusCode { get; set; }
    }
         
}
