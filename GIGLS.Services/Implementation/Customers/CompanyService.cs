using System;
using System.Threading.Tasks;
using GIGLS.Core.DTO.Customers;
using GIGLS.Core.IServices.Customers;
using GIGLS.Core;
using System.Collections.Generic;
using System.Linq;
using GIGL.GIGLS.Core.Domain;
using GIGLS.Core.Enums;
using GIGLS.Infrastructure;
using AutoMapper;
using GIGLS.Core.DTO.Wallet;
using GIGLS.Core.IServices.Wallet;
using GIGLS.Core.IServices.Utility;
using GIGLS.Core.IServices.User;

namespace GIGLS.Services.Implementation.Customers
{
    public class CompanyService : ICompanyService
    {
        private readonly IWalletService _walletService;
        private readonly INumberGeneratorMonitorService _numberGeneratorMonitorService;
        private readonly IPasswordGenerator _passwordGenerator;
        private readonly IUserService _userService;

        private readonly IUnitOfWork _uow;

        public CompanyService(INumberGeneratorMonitorService numberGeneratorMonitorService,
            IWalletService walletService, IPasswordGenerator passwordGenerator, IUserService userService, IUnitOfWork uow)
        {
            _walletService = walletService;
            _numberGeneratorMonitorService = numberGeneratorMonitorService;
            _passwordGenerator = passwordGenerator;
            _userService = userService;
            _uow = uow;
            MapperConfig.Initialize();
        }

        public async Task<CompanyDTO> AddCompany(CompanyDTO company)
        {
            try
            {
                if (await _uow.Company.ExistAsync(c => c.Name.ToLower() == company.Name.Trim().ToLower()))
                {
                    throw new GenericException($"{company.Name} Already Exist");
                }

                var newCompany = Mapper.Map<Company>(company);

                //generate customer code
                if (newCompany.CompanyType == CompanyType.Corporate)
                {
                    var customerCode = await _numberGeneratorMonitorService.GenerateNextNumber(
                        NumberGeneratorType.CustomerCodeCorporate);
                    newCompany.CustomerCode = customerCode;
                }
                else
                {
                    var customerCode = await _numberGeneratorMonitorService.GenerateNextNumber(
                        NumberGeneratorType.CustomerCodeEcommerce);
                    newCompany.CustomerCode = customerCode;
                }

                _uow.Company.Add(newCompany);

                if (company.ContactPersons.Any())
                {
                    foreach (CompanyContactPersonDTO personDto in company.ContactPersons)
                    {
                        var person = Mapper.Map<CompanyContactPerson>(personDto);
                        person.CompanyId = newCompany.CompanyId;
                        _uow.CompanyContactPerson.Add(person);
                    }
                }

                _uow.Complete();

                // add customer to a wallet
                await _walletService.AddWallet(new WalletDTO
                {
                    CustomerId = newCompany.CompanyId,
                    CustomerType = CustomerType.Company
                });


                // add to user table for login
                try
                {
                    var password = await _passwordGenerator.Generate();
                    var result = await _userService.AddUser(new Core.DTO.User.UserDTO()
                    {
                        ConfirmPassword = password,
                        Department = newCompany.CompanyType.ToString(),
                        DateCreated = DateTime.Now,
                        Designation = newCompany.CompanyType.ToString(),
                        Email = newCompany.Email,
                        FirstName = newCompany.Name,
                        LastName = newCompany.Name,
                        Organisation = newCompany.CompanyType.ToString(),
                        Password = password,
                        PhoneNumber = newCompany.PhoneNumber,
                        UserType = UserType.Regular,
                        Username = newCompany.CustomerCode,
                        UserChannelCode = newCompany.CustomerCode,
                        UserChannelPassword = password,
                        UserChannelType = UserChannelType.Customer
                    });
                }
                catch (Exception ex)
                {
                    // do nothing
                }

                return Mapper.Map<CompanyDTO>(newCompany);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task DeleteCompany(int companyId)
        {
            try
            {
                var company = await _uow.Company.GetAsync(companyId);
                if (company == null)
                {
                    throw new GenericException("Company information does not exist");
                }
                _uow.Company.Remove(company);
                _uow.Complete();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task<List<CompanyDTO>> GetCompanies()
        {
            return _uow.Company.GetCompanies();
        }

        public async Task<CompanyDTO> GetCompanyById(int companyId)
        {
            try
            {
                var company = await _uow.Company.GetAsync(companyId);
                if (company == null)
                {
                    throw new GenericException("Company information does not exist");
                }

                CompanyDTO companyDto = Mapper.Map<CompanyDTO>(company);

                var contactPersons = _uow.CompanyContactPerson.Find(x => x.CompanyId == companyDto.CompanyId).ToList();

                if (contactPersons.Any())
                {
                    companyDto.ContactPersons = contactPersons.Select(p => new CompanyContactPersonDTO
                    {
                        CompanyContactPersonId = p.CompanyContactPersonId,
                        Designation = p.Designation,
                        Email = p.Email,
                        FirstName = p.FirstName,
                        LastName = p.LastName,
                        PhoneNumber = p.PhoneNumber,
                        DateCreated = p.DateCreated,
                        DateModified = p.DateModified,
                        CompanyId = p.CompanyId
                    }).ToList();
                }

                return companyDto;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task UpdateCompany(int companyId, CompanyDTO companyDto)
        {
            try
            {
                var company = await _uow.Company.GetAsync(companyId);
                if (company == null || companyId != companyDto.CompanyId)
                {
                    throw new GenericException("Company information does not exist");
                }
                company.Name = companyDto.Name;
                company.PhoneNumber = companyDto.PhoneNumber;
                company.Address = companyDto.Address;
                company.City = companyDto.City;
                company.CompanyType = companyDto.CompanyType;
                company.Discount = companyDto.Discount;
                company.Email = companyDto.Email;
                company.Industry = companyDto.Industry;
                company.CompanyStatus = companyDto.CompanyStatus;
                company.State = companyDto.State;
                company.SettlementPeriod = companyDto.SettlementPeriod;

                if (companyDto.ContactPersons.Any())
                {
                    foreach (CompanyContactPersonDTO personDto in companyDto.ContactPersons)
                    {
                        var person = await _uow.CompanyContactPerson.GetAsync(personDto.CompanyContactPersonId);
                        person.FirstName = personDto.FirstName;
                        person.LastName = personDto.LastName;
                        person.Email = personDto.Email;
                        person.Designation = personDto.Designation;
                        person.PhoneNumber = personDto.PhoneNumber;
                        person.CompanyId = personDto.CompanyId;
                    }
                }

                _uow.Complete();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task UpdateCompanyStatus(int companyId, CompanyStatus status)
        {
            try
            {
                var company = await _uow.Company.GetAsync(companyId);
                if (company == null)
                {
                    throw new GenericException("Company information does not exist");
                }
                company.CompanyStatus = status;
                _uow.Complete();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<CompanyDTO>> GetCompanies(CompanyType companyType, CustomerSearchOption searchOption)
        {
            return await _uow.Company.GetCompanies(companyType, searchOption);
        }
    }
}