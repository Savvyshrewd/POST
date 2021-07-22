﻿using GIGLS.Core.IServices;
using GIGLS.Core.DTO.Customers;
using GIGLS.Core.IServices.Customers;
using GIGLS.Services.Implementation;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web.Http;
using GIGLS.Core.Enums;
using GIGLS.WebApi.Filters;
using GIGLS.CORE.DTO.Report;
using GIGLS.Core.DTO.Report;
using GIGLS.Core.DTO.OnlinePayment;
using Newtonsoft.Json.Linq;

namespace GIGLS.WebApi.Controllers.Customers
{
    [Authorize(Roles = "Admin, ViewAdmin")]
    [RoutePrefix("api/company")]
    public class CompanyController : BaseWebApiController
    {
        private readonly ICompanyService _service;
        private readonly ICountryService _countryService;

        public CompanyController(ICompanyService service, ICountryService countryService) : base(nameof(CompanyController))
        {
            _service = service;
            _countryService = countryService;
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("")]
        public async Task<IServiceResponse<IEnumerable<CompanyDTO>>> GetCompanies()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var companies = await _service.GetCompanies();
                return new ServiceResponse<IEnumerable<CompanyDTO>>
                {
                    Object = companies
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("{searchParams}/emailorcode")]
        public async Task<IServiceResponse<IEnumerable<CompanyDTO>>> GetCompaniesByEmailOrCode(string searchParams)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var companies = await _service.GetCompaniesByEmailOrCode(searchParams);
                return new ServiceResponse<IEnumerable<CompanyDTO>>
                {
                    Object = companies
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("search")]
        public async Task<IServiceResponse<IEnumerable<CompanyDTO>>> GetCompanies(BaseFilterCriteria filterCriteria)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var companies = await _service.GetCompanies(filterCriteria);
                return new ServiceResponse<IEnumerable<CompanyDTO>>
                {
                    Object = companies
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("withoutwallet")]
        public async Task<IServiceResponse<IEnumerable<CompanyDTO>>> GetCompaniesWithoutWallet()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var companies = await _service.GetCompaniesWithoutWallet();
                return new ServiceResponse<IEnumerable<CompanyDTO>>
                {
                    Object = companies
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("ecommerce")]
        public async Task<IServiceResponse<IEnumerable<CompanyDTO>>> GetEcommerceWithoutWallet()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var companies = await _service.GetEcommerceWithoutWallet();
                return new ServiceResponse<IEnumerable<CompanyDTO>>
                {
                    Object = companies
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("corporate")]
        public async Task<IServiceResponse<IEnumerable<CompanyDTO>>> GetCorporateWithoutWallet()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var companies = await _service.GetCorporateWithoutWallet();
                return new ServiceResponse<IEnumerable<CompanyDTO>>
                {
                    Object = companies
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("{companyId:int}/getwallet")]
        public async Task<IServiceResponse<EcommerceWalletDTO>> GettWalletDetailsForCompany(int companyId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var company = await _service.GetECommerceWalletById(companyId);
                return new ServiceResponse<EcommerceWalletDTO>
                {
                    Object = company
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Create")]
        [HttpPost]
        [Route("")]
        public async Task<IServiceResponse<object>> AddCompany(CompanyDTO companyDto)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var company = await _service.AddCompany(companyDto);
                return new ServiceResponse<object>
                {
                    Object = company
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("{companyId:int}")]
        public async Task<IServiceResponse<CompanyDTO>> GetCompany(int companyId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var company = await _service.GetCompanyById(companyId);

                return new ServiceResponse<CompanyDTO>
                {
                    Object = company
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Delete")]
        [HttpDelete]
        [Route("{companyId:int}")]
        public async Task<IServiceResponse<bool>> DeleteCustomer(int companyId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _service.DeleteCompany(companyId);
                return new ServiceResponse<bool>
                {
                    Object = true
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "Update")]
        [HttpPut]
        [Route("{companyId:int}")]
        public async Task<IServiceResponse<bool>> UpdateCustomer(int companyId, CompanyDTO companyDto)
        {

            return await HandleApiOperationAsync(async () =>
           {
               await _service.UpdateCompany(companyId, companyDto);
               return new ServiceResponse<bool>
               {
                   Object = true
               };
           });
        }

        [GIGLSActivityAuthorize(Activity = "Update")]
        [HttpPut]
        [Route("{companyId:int}/status/{status}")]
        public async Task<IServiceResponse<bool>> UpdateCustomerStatus(int companyId, CompanyStatus status)
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _service.UpdateCompanyStatus(companyId, status);

                return new ServiceResponse<bool>
                {
                    Object = true
                };

            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("searchByEmailCode")]
        public async Task<IServiceResponse<IEnumerable<CompanyDTO>>> GetCompanyByEmail(CompanySearchDTO searchDTO)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var company = await _service.GetCompanyByEmail(searchDTO.searchItem, searchDTO.rank);

                return new ServiceResponse<IEnumerable<CompanyDTO>>
                {
                    Object = company
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("pendingrequests")]
        public async Task<IServiceResponse<IEnumerable<EcommerceAgreementDTO>>> GetPendingEcommerceRequest(BaseFilterCriteria filterCriteria)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var companies = await _service.GetPendingEcommerceRequest(filterCriteria);
                return new ServiceResponse<IEnumerable<EcommerceAgreementDTO>>
                {
                    Object = companies
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("{companyId:int}/pendingrequests")]
        public async Task<IServiceResponse<EcommerceAgreementDTO>> GetPendingEcommerceRequestById(int companyId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var companies = await _service.GetCustomerPendingRequestsById(companyId);
                return new ServiceResponse<EcommerceAgreementDTO>
                {
                    Object = companies
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("classcustomers")]
        public async Task<IServiceResponse<IEnumerable<CompanyDTO>>> GetClassCustomers(ShipmentCollectionFilterCriteria filterCriteria)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var companies = await _service.GetClassCustomers(filterCriteria);
                return new ServiceResponse<IEnumerable<CompanyDTO>>
                {
                    Object = companies
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("assignedcustomers")]
        public async Task<IServiceResponse<IEnumerable<CompanyDTO>>> GetAssignedCustomers(BaseFilterCriteria filterCriteria)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var companies = await _service.GetAssignedCustomers(filterCriteria);
                return new ServiceResponse<IEnumerable<CompanyDTO>>
                {
                    Object = companies
                };
            });
        }


        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("nubanproviders")]
        public async Task<IServiceResponse<JObject>> GetNubanProviders()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var providers = await _service.GetNubanProviders();
                return new ServiceResponse<JObject>
                {
                    Object = providers
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("assignedcustomersbycustomerrep")]
        public async Task<IServiceResponse<IEnumerable<CompanyDTO>>> GetAssignedCustomersByCustomerRep(BaseFilterCriteria filterCriteria)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var companies = await _service.GetAssignedCustomersByCustomerRepEmail(filterCriteria);
                return new ServiceResponse<IEnumerable<CompanyDTO>>
                {
                    Object = companies
                };
            });
        }

    }
}
