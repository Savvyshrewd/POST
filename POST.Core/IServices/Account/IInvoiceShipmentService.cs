﻿using POST.Core.IServices;
using POST.Core.DTO.Account;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POST.Core.IServices.Account
{
    public interface IInvoiceShipmentService : IServiceDependencyMarker
    {
        Task<IEnumerable<InvoiceShipmentDTO>> GetInvoiceShipments();
        Task<InvoiceShipmentDTO> GetInvoiceShipmentById(int invoiceShipmentId);
        Task<object> AddInvoiceShipment(InvoiceShipmentDTO invoiceShipment);
        Task UpdateInvoiceShipment(int invoiceShipmentId, InvoiceShipmentDTO invoiceShipment);
        Task RemoveInvoiceShipment(int invoiceShipmentId);
    }
}
