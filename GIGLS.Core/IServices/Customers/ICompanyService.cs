using GIGLS.Core.DTO.Customers;
using GIGLS.Core.Enums;
using GIGLS.CORE.DTO.Report;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GIGLS.Core.IServices.Customers
{
    public interface ICompanyService : IServiceDependencyMarker
    {
        Task<List<CompanyDTO>> GetCompanies();
        Task<List<CompanyDTO>> GetCompaniesWithoutWallet();
        Task<List<CompanyDTO>> GetEcommerceWithoutWallet();
        Task<List<CompanyDTO>> GetCorporateWithoutWallet();

        Task<CompanyDTO> GetCompanyById(int companyId);
        Task<CompanyDTO> GetCompanyByCode(string customerCode);
        Task UpdateCompany(int companyId, CompanyDTO company);
        Task<CompanyDTO> AddCompany(CompanyDTO company);
        Task DeleteCompany(int companyId);
        Task UpdateCompanyStatus(int companyId, CompanyStatus status);
        Task<List<CompanyDTO>> GetCompanies(CompanyType companyType, CustomerSearchOption searchOption);
        Task<EcommerceWalletDTO> GetECommerceWalletById(int companyId);
        Task<List<CompanyDTO>> GetCompanies(BaseFilterCriteria filterCriteria);
        Task<List<CompanyDTO>> GetCompanyByEmail(string email);
    }
}