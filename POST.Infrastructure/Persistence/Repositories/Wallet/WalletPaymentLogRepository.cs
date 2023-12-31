﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using POST.Core.DTO.Wallet;
using POST.Core.IRepositories.Wallet;
using POST.Infrastructure.Persistence;
using POST.Infrastructure.Persistence.Repository;
using System.Linq;
using AutoMapper;
using POST.Core.Domain.Wallet;
using POST.CORE.DTO.Shipments;
using POST.Core.View;
using POST.CORE.DTO.Report;
using POST.Infrastructure;
using POST.Core.DTO.Report;
using System.Data.SqlClient;
using POST.Core.Domain;
using POST.Core.DTO;

namespace POST.INFRASTRUCTURE.Persistence.Repositories.Wallet
{
    public class WalletPaymentLogRepository : Repository<WalletPaymentLog, GIGLSContext>, IWalletPaymentLogRepository
    {
        private GIGLSContextForView _GIGLSContextForView;
        private GIGLSContext _context;

        public WalletPaymentLogRepository(GIGLSContext context) : base(context)
        {
            _GIGLSContextForView = new GIGLSContextForView();
            _context = context;
        }

        public Task<List<WalletPaymentLogDTO>> GetWalletPaymentLogs()
        {
            try
            {
                var walletPaymentLogs = Context.WalletPaymentLog;

                var walletPaymentLogsDTO = from w in walletPaymentLogs
                                           select new WalletPaymentLogDTO
                                           {
                                               WalletPaymentLogId = w.WalletPaymentLogId,
                                               WalletId = w.WalletId,
                                               Wallet = Context.Wallets.Where(s => s.WalletId == w.WalletId).Select(x => new WalletDTO
                                               {
                                                   WalletId = x.WalletId,
                                                   Balance = x.Balance,
                                                   CustomerCode = x.CustomerCode,
                                                   WalletNumber = x.WalletNumber,
                                                   CustomerType = x.CustomerType,
                                               }).FirstOrDefault(),
                                               Amount = w.Amount,
                                               TransactionStatus = w.TransactionStatus,
                                               UserId = w.UserId,
                                               IsWalletCredited = w.IsWalletCredited,
                                               DateCreated = w.DateCreated,
                                               DateModified = w.DateModified
                                           };
                return Task.FromResult(walletPaymentLogsDTO.OrderBy(x => x.DateCreated).ToList());
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Tuple<Task<List<WalletPaymentLogView>>, int> GetWalletPaymentLogs(FilterOptionsDto filterOptionsDto, string walletNumber)
        {
            try
            {
                var walletPaymentLogDto = new List<WalletPaymentLogDTO>();
                var pageNumber = filterOptionsDto?.page ?? FilterOptionsDto.DefaultPageNumber;
                var pageSize = filterOptionsDto?.count ?? FilterOptionsDto.DefaultCount;

                //build query
                var queryable = GetAllFromWalletPaymentLogView();

                if(walletNumber != null)
                {
                    queryable = queryable.Where(x => x.WalletNumber == walletNumber);
                }

                var filter = filterOptionsDto?.filter ?? null;
                var filterValue = filterOptionsDto?.filterValue ?? null;
                if (!string.IsNullOrWhiteSpace(filter) && !string.IsNullOrWhiteSpace(filterValue))
                {
                    var caseObject = new WalletPaymentLogView();
                    switch (filter)
                    {
                        case nameof(caseObject.WalletNumber):
                            queryable = queryable.Where(s => s.WalletNumber.Contains(filterValue));
                            break;
                        case nameof(caseObject.Reference):
                            queryable = queryable.Where(s => s.Reference.Contains(filterValue));
                            break;
                        case nameof(caseObject.Name):
                            queryable = queryable.Where(s => s.Name.Contains(filterValue) 
                            || s.FirstName.Contains(filterValue));
                            break;
                        case nameof(caseObject.FirstName):
                            queryable = queryable.Where(s => s.Name.Contains(filterValue)
                            || s.FirstName.Contains(filterValue));
                            break;
                    }
                }
                
                //populate the count variable
                var totalCount = queryable.Count();

                //page the query
                queryable = queryable.OrderByDescending(x => x.DateCreated);
                var result = queryable.Skip(pageSize * (pageNumber - 1)).Take(pageSize).ToList();
                return new Tuple<Task<List<WalletPaymentLogView>>, int>(Task.FromResult(result.ToList()), totalCount);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public Tuple<Task<List<WalletPaymentLogDTO>>, int> GetWalletPaymentLogs(FilterOptionsDto filterOptionsDto, int walletId)
        {
            try
            {
                var pageNumber = filterOptionsDto?.page ?? FilterOptionsDto.DefaultPageNumber;
                var pageSize = filterOptionsDto?.count ?? FilterOptionsDto.DefaultCount;

                //build query
                var queryable = Context.WalletPaymentLog.AsQueryable();

                if (walletId > 0)
                {
                    queryable = queryable.Where(x => x.WalletId == walletId);
                }

                var filter = filterOptionsDto?.filter ?? null;
                var filterValue = filterOptionsDto?.filterValue ?? null;
                if (!string.IsNullOrWhiteSpace(filter) && !string.IsNullOrWhiteSpace(filterValue))
                {
                    var caseObject = new WalletPaymentLog();
                    switch (filter)
                    {
                        case nameof(caseObject.Reference):
                            queryable = queryable.Where(s => s.Reference.Contains(filterValue));
                            break;
                    }
                }

                //populate the count variable
                var totalCount = queryable.Count();

                //page the query
                queryable = queryable.OrderByDescending(x => x.DateCreated);
                var result = queryable.Skip(pageSize * (pageNumber - 1)).Take(pageSize).ToList();

                var walletPaymentLogDto = Mapper.Map<List<WalletPaymentLogDTO>>(result);
                if (walletPaymentLogDto.Any())
                {
                    var countryIds = walletPaymentLogDto.Select(x => x.PaymentCountryId).ToList();
                    var countries = _context.Country.AsQueryable();
                    countries = countries.Where(x => countryIds.Contains(x.CountryId));
                    foreach (var item in walletPaymentLogDto)
                    {
                        item.CurrencyCode = countries.FirstOrDefault(x => x.CountryId == item.PaymentCountryId).CurrencyCode;
                        item.CurrencySymbol = countries.FirstOrDefault(x => x.CountryId == item.PaymentCountryId).CurrencySymbol;
                    }
                }
                return new Tuple<Task<List<WalletPaymentLogDTO>>, int>(Task.FromResult(walletPaymentLogDto), totalCount);                
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public IQueryable<WalletPaymentLogView> GetAllFromWalletPaymentLogView()
        {
            var walletPaymentLogViews = _GIGLSContextForView.WalletPaymentLogView.AsQueryable();
            return walletPaymentLogViews;
        }

        public Task<List<WalletPaymentLogView>> GetFromWalletPaymentLogView(DateFilterCriteria filterCriteria)
        {
            //get startDate and endDate
            var queryDate = filterCriteria.getStartDateAndEndDate();
            var startDate = queryDate.Item1;
            var endDate = queryDate.Item2;

            var walletPaymentLogViews = _GIGLSContextForView.WalletPaymentLogView.AsQueryable().Where(s => s.DateCreated >= startDate && s.DateCreated < endDate
                                    && s.UserActiveCountryId == filterCriteria.CountryId).ToList();

            return Task.FromResult(walletPaymentLogViews.OrderByDescending(x => x.DateCreated).ToList());            
        }

        public Task<List<WalletPaymentLogView>> GetFromWalletPaymentLogViewBySearchParameter(string searchItem)
        {
            var walletPaymentLogViews = _GIGLSContextForView.WalletPaymentLogView.AsQueryable();

            if (searchItem != null)
            {
                searchItem = searchItem.ToLower();
                walletPaymentLogViews = walletPaymentLogViews.Where(x => x.PhoneNumber == searchItem || x.Reference == searchItem
                                        || x.CustomerCode == searchItem || x.Email == searchItem);
            }
            else
            {
                throw new GenericException("Kindly enter a search parameter");
            }

            return Task.FromResult(walletPaymentLogViews.OrderByDescending(x => x.DateCreated).ToList());
        }

        public async Task<List<WalletPaymentLogView>> GetWalletPaymentLogBreakdown(DashboardFilterCriteria dashboardFilter)
        {
            var startDate = DateTime.Now;
            var endDate = DateTime.Now;

            //If No Date Supplied
            if (!dashboardFilter.StartDate.HasValue && !dashboardFilter.EndDate.HasValue)
            {
                var OneMonthAgo = DateTime.Now.AddMonths(0);  //One (1) Months ago
                startDate = new DateTime(OneMonthAgo.Year, OneMonthAgo.Month, 1);
                endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            }
            else
            {
                var queryDate = dashboardFilter.getStartDateAndEndDate();
                startDate = queryDate.Item1;
                endDate = queryDate.Item2;
            }


            //declare parameters for the stored procedure
            SqlParameter startDates = new SqlParameter("@StartDate", startDate);
            SqlParameter endDates = new SqlParameter("@EndDate", endDate);
            SqlParameter type = new SqlParameter("@Type", dashboardFilter.ActiveCountryId);


            SqlParameter[] param = new SqlParameter[]
            {
                    startDates,
                    endDates,
                    type
            };

            var summary = await _context.Database.SqlQuery<WalletPaymentLogView>("WalletPaymentLogBreakdown " +
               "@StartDate, @EndDate, @Type",
               param).ToListAsync();

            return await Task.FromResult(summary);
        }

        public async Task LogContentType(LogEntry payload)
        {
            if(payload != null)
            {

                Context.LogEntry.Add(payload);
                await Context.SaveChangesAsync();
            }
        }
    }
}
