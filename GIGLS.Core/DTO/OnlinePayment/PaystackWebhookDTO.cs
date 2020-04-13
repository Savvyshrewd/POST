﻿namespace GIGLS.Core.DTO.OnlinePayment
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
    }

    public class Data
    {
        public string Status { get; set; }
        public string Reference { get; set; }
        public decimal Amount { get; set; }
        public string Gateway_Response { get; set; }
        public string Display_Text { get; set; }
        public string Message { get; set; }
    }

    public class PaymentResponse
    {
        public bool Result { get; set; }
        public string Message { get; set; }
        public string GatewayResponse { get; set; }
        public string Status { get; set; }
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

    public class FlutterResponseData
    {
        public FlutterResponseData()
        {
            validateInstructions = new ValidateInstructions();
        }
        public string Status { get; set; }
        public string TXRef { get; set; }
        public decimal Amount { get; set; }
        public string ChargeResponseMessage { get; set; }
        public string ChargeResponseCode { get; set; }
        public string ChargeMessage { get; set; }

        public string PaymentType { get; set; }

        public ValidateInstructions validateInstructions { get; set; }
    }

    public class ValidateInstructions
    {
        public string Instruction { get; set; }
    }
}
