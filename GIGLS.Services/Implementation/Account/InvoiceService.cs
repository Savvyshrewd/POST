﻿using AutoMapper;
using GIGLS.Core;
using GIGLS.Core.Domain;
using GIGLS.Core.DTO.Account;
using GIGLS.Core.DTO.PaymentTransactions;
using GIGLS.Core.Enums;
using GIGLS.Core.IServices.Account;
using GIGLS.Core.IServices.Customers;
using GIGLS.Core.IServices.Shipments;
using GIGLS.Core.IServices.User;
using GIGLS.Core.IServices.Utility;
using GIGLS.Infrastructure;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GIGLS.Services.Implementation.Account
{
    public class InvoiceService : IInvoiceService
    {
        private readonly IUnitOfWork _uow;
        private INumberGeneratorMonitorService _service { get; set; }
        private IShipmentService _shipmentService { get; set; }
        private ICustomerService _customerService { get; set; }
        private readonly IUserService _userService;

        public InvoiceService(IUnitOfWork uow, INumberGeneratorMonitorService service,
            IShipmentService shipmentService, ICustomerService customerService,
            IUserService userService)
        {
            _uow = uow;
            _service = service;
            _shipmentService = shipmentService;
            _customerService = customerService;
            _userService = userService;
            MapperConfig.Initialize();
        }

        public async Task<IEnumerable<InvoiceDTO>> GetInvoices()
        {
            var serviceCenterIds = await _userService.GetPriviledgeServiceCenters();
            var invoices = await _uow.Invoice.GetInvoicesAsync(serviceCenterIds);
            return invoices.ToList().OrderByDescending(x => x.DateCreated);
        }

        public async Task<InvoiceDTO> GetInvoiceById(int invoiceId)
        {
            var invoice = await _uow.Invoice.GetAsync(invoiceId);

            if (invoice == null)
            {
                throw new GenericException("Invoice does not exists");
            }

            var invoiceDTO = Mapper.Map<InvoiceDTO>(invoice);

            // get Shipment
            var waybill = invoiceDTO.Waybill;
            invoiceDTO.Shipment = await _shipmentService.GetShipment(waybill);

            // get Customer
            invoiceDTO.Customer = invoiceDTO.Shipment.CustomerDetails;

            //get wallet number
            if (invoiceDTO.Customer.CustomerType == CustomerType.Company)
            {
                var wallet = await _uow.Wallet.GetAsync(
                    s => s.CustomerId == invoiceDTO.Customer.CompanyId &&
                    s.CustomerType == CustomerType.Company);
                invoiceDTO.Customer.WalletNumber = wallet?.WalletNumber;
            }
            else
            {
                var wallet = await _uow.Wallet.GetAsync(
                    s => s.CustomerId == invoiceDTO.Customer.IndividualCustomerId &&
                    s.CustomerType == CustomerType.IndividualCustomer);
                invoiceDTO.Customer.WalletNumber = wallet?.WalletNumber;
            }

            ///// Partial Payments, if invoice status is pending
            if (invoiceDTO.PaymentStatus == PaymentStatus.Pending)
            {
                var partialTransactionsForWaybill = await _uow.PaymentPartialTransaction.FindAsync(x => x.Waybill.Equals(waybill));
                invoiceDTO.PaymentPartialTransaction = new PaymentPartialTransactionProcessDTO()
                {
                    Waybill = waybill,
                    PaymentPartialTransactions = Mapper.Map<List<PaymentPartialTransactionDTO>>(partialTransactionsForWaybill)
                };
            }

            return invoiceDTO;
        }

        public async Task<InvoiceDTO> GetInvoiceByWaybill(string waybl)
        {
            //var invoices = await GetInvoices();
            var invoice = await _uow.Invoice.GetAsync(e => e.Waybill == waybl);

            if (invoice == null)
            {
                throw new GenericException("Invoice does not exists");
            }

            return await GetInvoiceById(invoice.InvoiceId);
        }

        public async Task<object> AddInvoice(InvoiceDTO invoiceDto)
        {
            var newInvoice = Mapper.Map<Invoice>(invoiceDto);
            newInvoice.InvoiceNo = await _service.GenerateNextNumber(NumberGeneratorType.Invoice);

            _uow.Invoice.Add(newInvoice);
            await _uow.CompleteAsync();
            return new { id = newInvoice.InvoiceId };
        }

        public async Task UpdateInvoice(int invoiceId, InvoiceDTO invoiceDto)
        {
            var invoice = await _uow.Invoice.GetAsync(invoiceId);

            if (invoice == null)
            {
                throw new GenericException("Invoice does not exists");
            }

            invoice.InvoiceId = invoiceId;
            invoice.InvoiceNo = invoiceDto.InvoiceNo;
            invoice.PaymentDate = invoiceDto.PaymentDate;
            invoice.PaymentMethod = invoiceDto.PaymentMethod;
            invoice.PaymentStatus = invoiceDto.PaymentStatus;
            invoice.Waybill = invoiceDto.Waybill;

            await _uow.CompleteAsync();
        }

        public async Task RemoveInvoice(int invoiceId)
        {
            var invoice = await _uow.Invoice.GetAsync(invoiceId);

            if (invoice == null)
            {
                throw new GenericException("Invoice does not exists");
            }
            _uow.Invoice.Remove(invoice);
            await _uow.CompleteAsync();
        }
    }
}
