﻿using GIGL.POST.Core.Repositories;
using POST.Core.Domain.Archived;
using POST.Core.DTO.Account;
using POST.Core.DTO.Report;
using POST.Core.Enums;
using POST.Core.View;
using POST.Core.View.AdminReportView;
using POST.Core.View.Archived;
using POST.CORE.DTO.Report;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace POST.Core.IRepositories.Archived
{
    public interface IInvoiceArchiveRepository : IRepository<Invoice_Archive>
    {
        Task<List<InvoiceDTO>> GetInvoicesAsync(int[] serviceCentreIds);
        Task<List<InvoiceDTO>> GetInvoicesAsync(AccountFilterCriteria accountFilterCriteria, int[] serviceCentreIds);
        Task<List<InvoiceViewDTO>> GetInvoicesFromViewAsync(AccountFilterCriteria accountFilterCriteria, int[] serviceCentreIds);
        Task<List<InvoiceViewDTO>> GetInvoicesFromViewAsyncFromSP(AccountFilterCriteria accountFilterCriteria, int[] serviceCentreIds);
        IQueryable<InvoiceArchiveView> GetAllFromInvoiceView();
        
        IQueryable<InvoiceArchiveView> GetInvoicesForReminderAsync();
        IQueryable<InvoiceView> GetAllFromInvoiceAndShipments();
        IQueryable<InvoiceView> GetAllInvoiceShipments();
        IQueryable<InvoiceView> GetCustomerTransactions();
        IQueryable<InvoiceView> GetCustomerInvoices();
        Task<List<InvoiceViewDTO>> GetInvoicesFromViewWithDeliveryTimeAsyncFromSP(AccountFilterCriteria accountFilterCriteria, int[] serviceCentreIds);
        Task<List<InvoiceMonitorDTO>> GetShipmentMonitorSetSP(AccountFilterCriteria accountFilterCriteria, int[] serviceCentreIds);
        Task<List<InvoiceMonitorDTO>> GetShipmentMonitorSetSPExpected(AccountFilterCriteria accountFilterCriteria, int[] serviceCentreIds);
        Task<List<InvoiceViewDTOUNGROUPED>> GetShipmentMonitorSetSP_NotGrouped(AccountFilterCriteria accountFilterCriteria, int[] serviceCentreIds);
        Task<List<InvoiceViewDTOUNGROUPED>> GetShipmentMonitorSetSP_NotGroupedx(AccountFilterCriteria accountFilterCriteria, int[] serviceCentreIds);
        Task<List<InvoiceMonitorDTO>> GetShipmentWaitingForCollection(AccountFilterCriteria accountFilterCriteria, int[] serviceCentreIds);
        Task<List<InvoiceViewDTOUNGROUPED>> GetShipmentWaitingForCollection_NotGrouped(AccountFilterCriteria accountFilterCriteria, int[] serviceCentreIds);

        //Admin Report 
        IQueryable<InvoiceView> GetAllFromInvoiceAndShipments(ShipmentCollectionFilterCriteria filterCriteria);
        
    }
}
