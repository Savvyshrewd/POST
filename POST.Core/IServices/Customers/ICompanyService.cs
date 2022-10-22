using POST.Core.DTO;
using POST.Core.DTO.Customers;
using POST.Core.DTO.OnlinePayment;
using POST.Core.DTO.Report;
using POST.Core.DTO.User;
using POST.Core.Enums;
using POST.CORE.DTO.Report;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace POST.Core.IServices.Customers
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
        Task<List<CompanyDTO>> GetCompanyByEmail(string email, Rank? rank);
        Task<string> AddCountryCodeToPhoneNumber(string phoneNumber, int countryId);
        Task<List<EcommerceAgreementDTO>> GetPendingEcommerceRequest(BaseFilterCriteria filterCriteria);
        Task<EcommerceAgreementDTO> GetCustomerPendingRequestsById(int companyId);
        Task<List<CompanyDTO>> GetCompaniesBy(List<string> codes);
        Task<ResponseDTO> UnboardUser(NewCompanyDTO company);
        Task<ResponseDTO> UpdateUserRank(UserValidationDTO userValidationDTO);
        Task SendMessageToNewSignUps(object obj);
        Task<List<CompanyDTO>> GetClassCustomers(ShipmentCollectionFilterCriteria filterCriteria);
        Task<CompanyDTO> UpgradeToEcommerce(UpgradeToEcommerce newCompanyDTO);
        Task<List<CompanyDTO>> GetAssignedCustomers(BaseFilterCriteria filterCriteria);
        Task<CreateNubanAccountResponseDTO> CraeteNubanAccount(CreateNubanAccountDTO nubanAccount);
        Task<JObject> GetNubanProviders();
        Task<List<CompanyDTO>> GetAssignedCustomersByCustomerRepEmail(BaseFilterCriteria filterCriteria);
        Task<List<CompanyDTO>> GetCompaniesByEmailOrCode(string searchParams);
        Task<ResponseDTO> AddSubscriptionToCustomer(string customercode);
        Task<ResponseDTO> UpdateUserRankForAlpha(string merchantEmail);
        Task<CompanyDTO> GetCompanyDetailsByEmail(string email);
        Task DeleteCustomerAccount(string customercode);
    }
}