﻿using System;
using System.Threading.Tasks;
using GIGLS.Core.DTO.Customers;
using GIGLS.Core.IServices.Customers;
using GIGLS.Core;
using AutoMapper;
using GIGLS.Core.Enums;
using System.Collections.Generic;
using GIGLS.CORE.Enums;

namespace GIGLS.Services.Implementation.Customers
{
    public class CustomerService : ICustomerService
    {
        public IIndividualCustomerService _individualCustomerService { get; set; }
        public ICompanyService _companyService { get; set; }
        public IUnitOfWork _uow { get; set; }

        public CustomerService(IUnitOfWork uow,
            IIndividualCustomerService individualCustomerService, ICompanyService companyService)
        {
            _uow = uow;
            _individualCustomerService = individualCustomerService;
            _companyService = companyService;
            MapperConfig.Initialize();
        }

        public async Task<CustomerDTO> CreateCustomer(CustomerDTO customerDTO)
        {
            try
            {
                // handle Company customers
                if (CustomerType.Company.Equals(customerDTO.CustomerType))
                {
                    var companyId = 0;
                    var CompanyByName = await _uow.Company.FindAsync(c => c.Name.ToLower() == customerDTO.Name.Trim().ToLower() && 
                    c.PhoneNumber == customerDTO.PhoneNumber);
                    foreach (var item in CompanyByName)
                    {
                        companyId = item.CompanyId;
                    }

                    if (companyId > 0)
                    {
                        // update
                        customerDTO.CompanyId = companyId;
                        var companyDTO = Mapper.Map<CompanyDTO>(customerDTO);
                        await _companyService.UpdateCompany(companyId, companyDTO);
                    }
                    else
                    {
                        // create new
                        var companyDTO = Mapper.Map<CompanyDTO>(customerDTO);
                        var createdCustomer = await _companyService.AddCompany(companyDTO);
                        customerDTO = Mapper.Map<CustomerDTO>(companyDTO);
                        customerDTO.CompanyId = createdCustomer.CompanyId;
                    }
                }

                // handle IndividualCustomers
                if (CustomerType.IndividualCustomer.Equals(customerDTO.CustomerType))
                {
                    var individualCustomerId = 0;
                    var individualCustomerByPhone = await _uow.IndividualCustomer.GetAsync(c => c.PhoneNumber == customerDTO.PhoneNumber.Trim());

                    if(individualCustomerByPhone != null)
                    {
                        individualCustomerId = individualCustomerByPhone.IndividualCustomerId;
                    }

                    if (individualCustomerId > 0)
                    {
                        // update
                        customerDTO.IndividualCustomerId = individualCustomerId;
                        var individualCustomerDTO = Mapper.Map<IndividualCustomerDTO>(customerDTO);
                        await _individualCustomerService.UpdateCustomer(individualCustomerId, individualCustomerDTO);
                    }
                    else
                    {
                        // create new
                        var individualCustomerDTO = Mapper.Map<IndividualCustomerDTO>(customerDTO);
                        var createdCustomer = await _individualCustomerService.AddCustomer(individualCustomerDTO);
                        customerDTO.IndividualCustomerId = createdCustomer.IndividualCustomerId;
                    }
                }

                return customerDTO;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<CustomerDTO> GetCustomer(int customerId, CustomerType customerType)
        {
            try
            {
                // handle Company customers
                if (CustomerType.Company.Equals(customerType))
                {
                    var companyDTO = await _companyService.GetCompanyById(customerId);
                    var customerDTO = Mapper.Map<CustomerDTO>(companyDTO);
                    customerDTO.CustomerType = CustomerType.Company;

                    return customerDTO;
                }                
                else
                {
                    // handle IndividualCustomers
                    var individualCustomerDTO = await _individualCustomerService.GetCustomerById(customerId);
                    var customerDTO = Mapper.Map<CustomerDTO>(individualCustomerDTO);
                    customerDTO.CustomerType = CustomerType.IndividualCustomer;

                    return customerDTO;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<CustomerDTO> GetCustomer(string userChannelCode, UserChannelType userChannelType)
        {
            try
            {
                if (UserChannelType.Ecommerce.Equals(userChannelType) || UserChannelType.Corporate.Equals(userChannelType))
                {
                    var companyDTO = await _companyService.GetCompanyByCode(userChannelCode);
                    var customerDTO = Mapper.Map<CustomerDTO>(companyDTO);
                    customerDTO.CustomerType = CustomerType.Company;
                    return customerDTO;
                }
                else if (UserChannelType.IndividualCustomer.Equals(userChannelType))
                {
                    var individualCustomerDTO = await _individualCustomerService.GetCustomerByCode(userChannelCode);
                    var customerDTO = Mapper.Map<CustomerDTO>(individualCustomerDTO);
                    customerDTO.CustomerType = CustomerType.IndividualCustomer;
                    return customerDTO;
                }
                return new CustomerDTO { };
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<CustomerDTO>> GetCustomers(CustomerType customerType)
        {
            try
            {
                // handle Company customers
                if (CustomerType.IndividualCustomer.Equals(customerType))
                {
                    var individualCustomerDTO = await _individualCustomerService.GetIndividualCustomers();
                    var customerDTO = Mapper.Map<List<CustomerDTO>>(individualCustomerDTO);

                    foreach (var item in customerDTO)
                    {
                        item.CustomerType = CustomerType.IndividualCustomer;
                    }
                    return customerDTO;
                }
                else
                {
                    var companyDTO = await _companyService.GetCompanies();
                    var customerDTO = Mapper.Map<List<CustomerDTO>>(companyDTO);

                    foreach (var item in customerDTO)
                    {
                        item.CustomerType = CustomerType.Company;
                    }

                    return customerDTO;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IndividualCustomerDTO> GetCustomerByPhoneNumber(string phoneNumber)
        {
            try
            {
                // handle IndividualCustomers
                var individualCustomerDTO = await _individualCustomerService.GetCustomerByPhoneNumber(phoneNumber);
                return individualCustomerDTO;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<List<CustomerDTO>> SearchForCustomers(CustomerSearchOption searchOption)
        {
            try
            {
                // handle Company customers
                if (FilterCustomerType.IndividualCustomer.Equals(searchOption.CustomerType))
                {
                    var individualCustomerDTO = await _individualCustomerService.GetIndividualCustomers(searchOption.SearchData);
                    var customerDTO = Mapper.Map<List<CustomerDTO>>(individualCustomerDTO);

                    foreach (var item in customerDTO)
                    {
                        item.CustomerType = CustomerType.IndividualCustomer;
                        item.CompanyId = item.IndividualCustomerId;
                    }
                    return customerDTO;
                }
                else
                {
                    CompanyType companyType;

                    if (FilterCustomerType.Corporate.Equals(searchOption.CustomerType))
                    {
                        companyType = CompanyType.Corporate;
                    }
                    else
                    {
                        companyType = CompanyType.Ecommerce;
                    }
                    var companyDTO = await _companyService.GetCompanies(companyType, searchOption);
                    var customerDTO = Mapper.Map<List<CustomerDTO>>(companyDTO);

                    foreach (var item in customerDTO)
                    {
                        item.CustomerType = CustomerType.Company;
                        item.IndividualCustomerId = item.CompanyId;
                    }

                    return customerDTO;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
