﻿using POST.Core.DTO;
using POST.Core.IServices;
using System.Threading.Tasks;

namespace POST.Core.IMessage
{
    public interface IEmailService : IServiceDependencyMarker
    {
        Task<string> SendAsync(MessageDTO message);
        Task<string> SendEcommerceRegistrationNotificationAsync(MessageDTO message);
        Task<string> SendPaymentNotificationAsync(MessageDTO message);
        Task<string> SendCustomerRegistrationMails(MessageDTO message);
        Task<string> SendOverseasShipmentMails(MessageDTO message);
        Task<string> SendEmailIntlShipmentAsync(MessageDTO message);
        Task<string> SendEmailShipmentARFAsync(MessageDTO message);
        Task<string> SendEmailEcommerceCustomerRepAsync(MessageDTO message);
        Task<string> SendEmailShipmentCreationAsync(MessageDTO message);
        Task<string> SendEmailShipmentARFHomeDeliveryAsync(MessageDTO message);
        Task<string> SendEmailShipmentARFTerminalPickupAsync(MessageDTO message);
        Task<string> SendEmailClassCustomerShipmentCreationAsync(MessageDTO message);
        Task<string> SendConfigCorporateSignUpMessage(MessageDTO message);
        Task<string> SendConfigCorporateNubanAccMessage(MessageDTO message);
        Task<string> SendEmailForReceivedItem(MessageDTO message);
        Task<string> ConfigSendGridMonthlyCorporateTransactions(MessageDTO message);
        Task<string> SendEmailForService(MessageDTO message);
        Task<string> SendEmailForStellaLoginDetails(MessageDTO message);
        Task<string> SendEmailOpenJobCardAsync(MessageDTO message);
        Task<string> SendEmailFleetDisputeMessageAsync(MessageDTO message);
        Task<string> SendEmailCloseJobCardAsync(MessageDTO message);
        Task<string> SendEmailForCODReport(MessageDTO message);
    }
}
