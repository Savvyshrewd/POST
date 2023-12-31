﻿using POST.Core.DTO.Account;
using POST.Core.DTO.Expenses;
using POST.Core.DTO.Report;
using POST.Core.IServices;
using POST.Core.IServices.Account;
using POST.Services.Implementation;
using POST.WebApi.Filters;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;

namespace POST.WebApi.Controllers.Account
{
    [Authorize(Roles = "Account, Shipment")]
    [RoutePrefix("api/expenditure")]
    public class ExpenditureController : BaseWebApiController
    {
        private readonly IExpenditureService _expenditureService;
        public ExpenditureController(IExpenditureService expenditureService) : base(nameof(ExpenditureController))
        {
            _expenditureService = expenditureService;
        }


        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("expense")]
        public async Task<IServiceResponse<IEnumerable<ExpenditureDTO>>> GetAllExpenditure(ExpenditureFilterCriteria expenditureFilterCriteria)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var expenditures = await _expenditureService.GetExpenditures(expenditureFilterCriteria);

                return new ServiceResponse<IEnumerable<ExpenditureDTO>>
                {
                    Object = expenditures
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Create")]
        [HttpPost]
        [Route("")]
        public async Task<IServiceResponse<object>> AddExpenditure(ExpenditureDTO expenditureDto)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var expenditure = await _expenditureService.AddExpenditure(expenditureDto);

                return new ServiceResponse<object>
                {
                    Object = expenditure
                };
            });
        }

        //This code is not used again to generate expenditure report
        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("")]
        public async Task<IServiceResponse<IEnumerable<GeneralLedgerDTO>>> GetAllExpenditure()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var expenditure = await _expenditureService.GetExpenditures();

                return new ServiceResponse<IEnumerable<GeneralLedgerDTO>>
                {
                    Object = expenditure
                };
            });
        }

        //This code is not used again to add expenditure
        [GIGLSActivityAuthorize(Activity = "Create")]
        [HttpPost]
        [Route("generalledger")]
        public async Task<IServiceResponse<object>> AddExpenditure(GeneralLedgerDTO generalledgerDto)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var generalledger = await _expenditureService.AddExpenditure(generalledgerDto);

                return new ServiceResponse<object>
                {
                    Object = generalledger
                };
            });
        }

    }
}
