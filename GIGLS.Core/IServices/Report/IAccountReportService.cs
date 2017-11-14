﻿using GIGLS.Core.DTO.Account;
using GIGLS.Core.IServices;
using GIGLS.CORE.DTO.Report;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GIGLS.CORE.IServices.Report
{
    public interface IAccountReportService : IServiceDependencyMarker
    {
        Task<List<GeneralLedgerDTO>> GetIncomeReports(AccountFilterCriteria accountFilterCriteria);
        Task<List<GeneralLedgerDTO>> GetExpenditureReports(AccountFilterCriteria accountFilterCriteria);
        Task<List<InvoiceDTO>> GetInvoiceReports(AccountFilterCriteria accountFilterCriteria);
    }
}
