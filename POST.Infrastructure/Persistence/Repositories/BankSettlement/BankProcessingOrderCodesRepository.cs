﻿using GIGL.POST.Core.Domain;
using POST.Core.Domain.BankSettlement;
using POST.Core.DTO.BankSettlement;
using POST.Core.DTO.Report;
using POST.Core.DTO.ServiceCentres;
using POST.Core.Enums;
using POST.Core.IRepositories.BankSettlement;
using POST.Infrastructure.Persistence;
using POST.Infrastructure.Persistence.Repository;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace POST.INFRASTRUCTURE.Persistence.Repositories.BankSettlement
{
    public class BankProcessingOrderCodesRepository : Repository<BankProcessingOrderCodes, GIGLSContext>, IBankProcessingOrderCodesRepository
    {
        public BankProcessingOrderCodesRepository(GIGLSContext context) : base(context)
        {
        }

        public Task<List<BankProcessingOrderCodesDTO>> GetBankOrderProcessingCode(DepositType type)
        {
            var processingorderCodes = Context.BankProcessingOrderCodes.AsQueryable();
            processingorderCodes = processingorderCodes.Where(s => s.DepositType == type);
            var processingcodes = (from processingorderCode in processingorderCodes
                                  select new BankProcessingOrderCodesDTO
                                  {
                                      CodeId = processingorderCode.CodeId,
                                      Code = processingorderCode.Code,
                                      DateAndTimeOfDeposit = processingorderCode.DateAndTimeOfDeposit,
                                      DepositType = processingorderCode.DepositType,
                                      TotalAmount = processingorderCode.TotalAmount,
                                      UserId = processingorderCode.UserId,
                                      Status = processingorderCode.Status,
                                      ServiceCenter = processingorderCode.ServiceCenter,
                                      ScName = processingorderCode.ScName,
                                      FullName = processingorderCode.FullName,
                                      VerifiedBy = processingorderCode.VerifiedBy,
                                      BankName = processingorderCode.BankName,
                                      DateCreated = processingorderCode.DateCreated,
                                      DateModified = processingorderCode.DateModified
                                  }).ToList();

            return Task.FromResult(processingcodes.OrderByDescending(s => s.DateAndTimeOfDeposit).ToList());
        }

        public Task<BankProcessingOrderCodesDTO> GetBankOrderProcessingCodeV2(DepositType type, string refcode)
        {
            var processingorderCodes = Context.BankProcessingOrderCodes.AsQueryable().Where(s => s.DepositType == type && s.Code == refcode);
           
            var processingcodes = (from processingorderCode in processingorderCodes
                                   select new BankProcessingOrderCodesDTO
                                   {
                                       CodeId = processingorderCode.CodeId,
                                       Code = processingorderCode.Code,
                                       DateAndTimeOfDeposit = processingorderCode.DateAndTimeOfDeposit,
                                       DepositType = processingorderCode.DepositType,
                                       TotalAmount = processingorderCode.TotalAmount,
                                       UserId = processingorderCode.UserId,
                                       Status = processingorderCode.Status,
                                       ServiceCenter = processingorderCode.ServiceCenter,
                                       ScName = processingorderCode.ScName,
                                       FullName = processingorderCode.FullName,
                                       VerifiedBy = processingorderCode.VerifiedBy,
                                       BankName = processingorderCode.BankName,
                                       DateCreated = processingorderCode.DateCreated,
                                       DateModified = processingorderCode.DateModified
                                   }).FirstOrDefault();

            return Task.FromResult(processingcodes);
        }

        public Task<List<BankProcessingOrderCodesDTO>> GetBankOrderProcessingCodeByServiceCenter(DepositType type, BankDepositFilterCriteria dateFilterCriteria, ServiceCentreDTO[] sc)  
        {
            //get startDate and endDate
            var queryDate = dateFilterCriteria.getStartDateAndEndDate();
            var startDate = queryDate.Item1;
            var endDate = queryDate.Item2;
            var servicecenterid = sc[0].ServiceCentreId;
            var processingorderCodes = Context.BankProcessingOrderCodes.Where(s => s.DateCreated >= startDate && s.DateCreated < endDate && s.DepositType == type && s.ServiceCenter== servicecenterid).AsQueryable();

            var processingcodes = (from processingorderCode in processingorderCodes
                                   select new BankProcessingOrderCodesDTO
                                   {
                                       CodeId = processingorderCode.CodeId,
                                       Code = processingorderCode.Code,
                                       DateAndTimeOfDeposit = processingorderCode.DateAndTimeOfDeposit,
                                       DepositType = processingorderCode.DepositType,
                                       TotalAmount = processingorderCode.TotalAmount,
                                       UserId = processingorderCode.UserId,
                                       Status = processingorderCode.Status,
                                       ServiceCenter = processingorderCode.ServiceCenter,
                                       ScName = processingorderCode.ScName,
                                       FullName = processingorderCode.FullName,
                                       VerifiedBy = processingorderCode.VerifiedBy,
                                       BankName = processingorderCode.BankName,
                                       DateCreated = processingorderCode.DateCreated,
                                       DateModified = processingorderCode.DateModified
                                   }).ToList();

            return Task.FromResult(processingcodes.OrderByDescending(s => s.DateAndTimeOfDeposit).ToList());
        }

        //gets the deposits for the date range
        public Task<List<BankProcessingOrderCodesDTO>> GetBankOrderProcessingCodeByDate(DepositType type, BankDepositFilterCriteria dateFilterCriteria)
        {
            //get startDate and endDate
            var queryDate = dateFilterCriteria.getStartDateAndEndDate();
            var startDate = queryDate.Item1;
            var endDate = queryDate.Item2;                                  

            var processingorderCodes = Context.BankProcessingOrderCodes.Where(s => s.DateCreated >= startDate && s.DateCreated < endDate && s.DepositType == type).AsQueryable();                       

            
            var processingcodes = (from processingorderCode in processingorderCodes
                                  select new BankProcessingOrderCodesDTO
                                  {
                                      CodeId = processingorderCode.CodeId,
                                      Code = processingorderCode.Code,
                                      DateAndTimeOfDeposit = processingorderCode.DateAndTimeOfDeposit,
                                      DepositType = processingorderCode.DepositType,
                                      TotalAmount = processingorderCode.TotalAmount,
                                      UserId = processingorderCode.UserId,
                                      Status = processingorderCode.Status,
                                      ServiceCenter = processingorderCode.ServiceCenter,
                                      ScName = processingorderCode.ScName,
                                      FullName = processingorderCode.FullName,
                                      VerifiedBy = processingorderCode.VerifiedBy,
                                      BankName = processingorderCode.BankName,
                                      DateCreated = processingorderCode.DateCreated,
                                      DateModified = processingorderCode.DateModified
                                  }).ToList();

            return Task.FromResult(processingcodes.OrderByDescending(s => s.DateAndTimeOfDeposit).ToList());
        }

        //gets the regional deposits for the date range
        public Task<List<BankProcessingOrderCodesDTO>> GetBankOrderProcessingCodeByDate(DepositType type, BankDepositFilterCriteria dateFilterCriteria, int[] serviceCenters)
        {
            //get startDate and endDate
            var queryDate = dateFilterCriteria.getStartDateAndEndDate();
            var startDate = queryDate.Item1;
            var endDate = queryDate.Item2;

            var processingorderCodes = Context.BankProcessingOrderCodes.AsQueryable().Where(s => s.DateCreated >= startDate && s.DateCreated < endDate && s.DepositType == type);

            //filter by service center of the login user
            if (serviceCenters.Length > 0)
            {
                processingorderCodes = processingorderCodes.Where(s => serviceCenters.Contains(s.ServiceCenter));
            }

            var processingcodes = (from processingorderCode in processingorderCodes
                                  select new BankProcessingOrderCodesDTO
                                  {
                                      CodeId = processingorderCode.CodeId,
                                      Code = processingorderCode.Code,
                                      DateAndTimeOfDeposit = processingorderCode.DateAndTimeOfDeposit,
                                      DepositType = processingorderCode.DepositType,
                                      TotalAmount = processingorderCode.TotalAmount,
                                      UserId = processingorderCode.UserId,
                                      Status = processingorderCode.Status,
                                      ServiceCenter = processingorderCode.ServiceCenter,
                                      ScName = processingorderCode.ScName,
                                      FullName = processingorderCode.FullName,
                                      VerifiedBy = processingorderCode.VerifiedBy,
                                      BankName = processingorderCode.BankName,
                                      DateCreated = processingorderCode.DateCreated,
                                      DateModified = processingorderCode.DateModified
                                  }).ToList();

            return Task.FromResult(processingcodes.OrderByDescending(s => s.DateAndTimeOfDeposit).ToList());
        }

        public IQueryable<BankProcessingOrderCodesDTO> GetBankOrderProcessingCodeAsQueryable()
        {
            var processingorderCodes = Context.BankProcessingOrderCodes.AsQueryable();
            var processingcodes = from processingorderCode in processingorderCodes
                                  select new BankProcessingOrderCodesDTO
                                  {
                                      CodeId = processingorderCode.CodeId,
                                      Code = processingorderCode.Code,
                                      DateAndTimeOfDeposit = processingorderCode.DateAndTimeOfDeposit,
                                      DepositType = processingorderCode.DepositType,
                                      TotalAmount = processingorderCode.TotalAmount,
                                      UserId = processingorderCode.UserId,
                                      Status = processingorderCode.Status,
                                      ServiceCenter = processingorderCode.ServiceCenter,
                                      BankName = processingorderCode.BankName,
                                      DateCreated = processingorderCode.DateCreated,
                                      DateModified = processingorderCode.DateModified
                                  };
            return processingcodes.OrderByDescending(s => s.DateAndTimeOfDeposit);
        }

        public Task<List<BankProcessingOrderCodesDTO>> GetProcessingOrderCodebyRefCode(string refcode)
        {
            var processingorderCodes = Context.BankProcessingOrderCodes.AsQueryable();
            processingorderCodes = processingorderCodes.Where(s => s.Code == refcode);

            var codorder = from processingorderCode in processingorderCodes
                           select new BankProcessingOrderCodesDTO
                           {
                               CodeId = processingorderCode.CodeId,
                               Code = processingorderCode.Code,
                               DateAndTimeOfDeposit = processingorderCode.DateAndTimeOfDeposit,
                               DepositType = processingorderCode.DepositType,
                               TotalAmount = processingorderCode.TotalAmount,
                               UserId = processingorderCode.UserId,
                               Status = processingorderCode.Status,
                               ServiceCenter = processingorderCode.ServiceCenter,
                               BankName = processingorderCode.BankName,
                               DateCreated = processingorderCode.DateCreated,
                               DateModified = processingorderCode.DateModified
                           };
            return Task.FromResult(codorder.ToList());
        }

        public Task<Shipment> GetShipmentByWaybill(string waybill)
        {
            var shipment = Context.Shipment.Where(x => x.Waybill == waybill).FirstOrDefault();
            return Task.FromResult(shipment);
        }
    }
}
