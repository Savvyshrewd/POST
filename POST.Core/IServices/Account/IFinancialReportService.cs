﻿using POST.Core.DTO.Account;
using System.Threading.Tasks;

namespace POST.Core.IServices.Account
{
    public interface IFinancialReportService : IServiceDependencyMarker
    {
        Task<object> AddReport(FinancialReportDTO financialReportDTO);
    }
}
