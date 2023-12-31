﻿using System;
using System.Threading.Tasks;
using POST.Core.DTO.Customers;
using POST.Core.IServices.Customers;
using System.Collections.Generic;
using POST.Core;
using GIGL.POST.Core.Domain;
using POST.Infrastructure;
using AutoMapper;
using System.Linq;
using POST.Core.IServices.Wallet;
using POST.Core.DTO.Wallet;
using POST.Core.Enums;
using POST.Core.IServices.Utility;
using POST.Core.IServices.User;

namespace POST.Services.Implementation.Customers
{
    public class IndividualCustomerService : IIndividualCustomerService
    {
        private readonly IWalletService _walletService;
        private readonly INumberGeneratorMonitorService _numberGeneratorMonitorService;
        private readonly IUnitOfWork _uow;
        private readonly IPasswordGenerator _passwordGenerator;
        private readonly IUserService _userService;

        public IndividualCustomerService(INumberGeneratorMonitorService numberGeneratorMonitorService,
            IWalletService walletService, IPasswordGenerator passwordGenerator, IUserService userService, IUnitOfWork uow)
        {
            _walletService = walletService;
            _numberGeneratorMonitorService = numberGeneratorMonitorService;
            _passwordGenerator = passwordGenerator;
            _userService = userService;
            _uow = uow;
            MapperConfig.Initialize();
        }

        public async Task<IndividualCustomerDTO> AddCustomer(IndividualCustomerDTO customer)
        {
            try
            {
                if (await _uow.IndividualCustomer.ExistAsync(c => c.PhoneNumber == customer.PhoneNumber.Trim()))
                {
                    throw new GenericException($"Individual Customer Phone Number {customer.PhoneNumber } Already Exist");
                }

                //if (await _uow.IndividualCustomer.ExistAsync(c => c.Email == customer.Email.Trim()))
                //{
                //    throw new GenericException($"Individual Customer Email {customer.Email } Already Exist");
                //}

                var newCustomer = Mapper.Map<IndividualCustomer>(customer);

                //added to add customers from Mobile
                if (customer.IsFromMobile != true)
                {
                    //get the SC Agent priviledge country
                    var countryIds = await _userService.GetPriviledgeCountryIds();
                    if (countryIds.Length > 0)
                    {
                        newCustomer.UserActiveCountryId = countryIds[0];
                    }
                }

                //generate customer code
                var customerCode = await _numberGeneratorMonitorService.GenerateNextNumber(NumberGeneratorType.CustomerCodeIndividual);
                newCustomer.CustomerCode = customerCode;

                //added this line of code for mobile registration
                if(customer.IsFromMobile ==true)
                {
                    newCustomer.IsRegisteredFromMobile = true;
                }

                _uow.IndividualCustomer.Add(newCustomer);

                await _uow.CompleteAsync();

                // add customer to a wallet
                await _walletService.AddWallet(new WalletDTO
                {
                    CustomerId = newCustomer.IndividualCustomerId,
                    CustomerType = CustomerType.IndividualCustomer,
                    CustomerCode = newCustomer.CustomerCode,
                    CompanyType = CustomerType.IndividualCustomer.ToString()
                });

                // add to user table for login
                //1. If userEmail is null, use CustomerCode
                if(String.IsNullOrEmpty(newCustomer.Email))
                {
                    newCustomer.Email = newCustomer.CustomerCode;
                }

                //create login for the user
                //try
                //{
                //    var password = "";
                //    if (newCustomer.Password == null)
                //    {
                //        password = await _passwordGenerator.Generate();
                //    }
                //    else
                //    {
                //        password = newCustomer.Password;
                //    }
                //    var result = await _userService.AddUser(new Core.DTO.User.UserDTO()
                //    {
                //        ConfirmPassword = password,
                //        Department = CustomerType.IndividualCustomer.ToString(),
                //        DateCreated = DateTime.Now,
                //        Designation = CustomerType.IndividualCustomer.ToString(),
                //        Email=newCustomer.Email,
                //        FirstName = newCustomer.FirstName,
                //        LastName = newCustomer.LastName,
                //        Organisation = CustomerType.IndividualCustomer.ToString(),
                //        Password = password,
                //        PhoneNumber = newCustomer.PhoneNumber,
                //        UserType = UserType.Regular,
                //        Username = newCustomer.CustomerCode,
                //        UserChannelCode = newCustomer.CustomerCode,
                //        UserChannelPassword = password,
                //        UserChannelType = UserChannelType.IndividualCustomer,
                //        PasswordExpireDate = DateTime.Now,
                //        UserActiveCountryId = newCustomer.UserActiveCountryId
                //    });
                //}
                //catch (Exception)
                //{
                //    // do nothing
                //}
                
                return Mapper.Map<IndividualCustomerDTO>(newCustomer);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task DeleteCustomer(int customerId)
        {
            try
            {
                //Delete user, wallet and customer table
                var customer = await _uow.IndividualCustomer.GetAsync(customerId);
                if (customer == null)
                {
                    throw new GenericException("Individual Customer Inforamtion does not exist");
                }
                _uow.IndividualCustomer.Remove(customer);

                var wallet = await _uow.Wallet.GetAsync(x => x.CustomerCode == customer.CustomerCode);
                if (wallet != null)
                {
                    _uow.Wallet.Remove(wallet);
                }

                var user = await _uow.User.GetUserByChannelCode(customer.CustomerCode);
                if (user != null)
                {
                    await _uow.User.Remove(user.Id);
                }

                _uow.Complete();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IndividualCustomerDTO> GetCustomerById(int customerId)
        {
            try
            {
                var customer = await _uow.IndividualCustomer.GetAsync(customerId);
                if (customer == null)
                {
                    throw new GenericException("Individual Customer information does not exist");
                }

                IndividualCustomerDTO individual = Mapper.Map<IndividualCustomerDTO>(customer);

                //get all countries and set the country name
                var userCountry = await _uow.Country.GetAsync(individual.UserActiveCountryId);
                individual.UserActiveCountryName = userCountry?.CountryName;

                return individual;
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
                var customer = await _uow.IndividualCustomer.GetAsync(x => x.PhoneNumber.Contains(phoneNumber));
                IndividualCustomerDTO individual = Mapper.Map<IndividualCustomerDTO>(customer);

                if(individual != null)
                {
                    //get all countries and set the country name
                    var userCountry = await _uow.Country.GetAsync(individual.UserActiveCountryId);
                    individual.UserActiveCountryName = userCountry?.CountryName;
                }

                return individual;
            }
            catch (Exception)
            {
                throw;
            }
        }


        public async Task<List<IndividualCustomerDTO>> GetIndividualCustomers()
        {
            try
            {
                List<IndividualCustomerDTO> customers = _uow.IndividualCustomer.GetAll().Select(s => new IndividualCustomerDTO
                {
                    IndividualCustomerId = s.IndividualCustomerId,
                    FirstName = s.FirstName,
                    LastName = s.LastName,
                    Email = s.Email,
                    Address = s.Address,
                    City = s.City,
                    Gender = s.Gender,
                    PictureUrl = s.PictureUrl,
                    PhoneNumber = s.PhoneNumber,
                    State = s.State,
                    DateModified = s.DateModified,
                    DateCreated = s.DateCreated,
                    CustomerCode = s.CustomerCode,
                    UserActiveCountryId = s.UserActiveCountryId
                }).ToList();

                return await Task.FromResult(customers);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task UpdateCustomer(int customerId, IndividualCustomerDTO customerDto)
        {
            var customer = await _uow.IndividualCustomer.GetAsync(customerId);
            if (customer == null || customerDto.IndividualCustomerId != customerId)
            {
                throw new GenericException("Individual Customer information does not exist");
            }
            
            customer.FirstName = customerDto.FirstName;
            customer.LastName = customerDto.LastName;
            customer.Email = customerDto.Email;
            customer.Address = customerDto.Address;
            customer.City = customerDto.City;
            customer.Gender = customerDto.Gender;
            customer.PictureUrl = customerDto.PictureUrl;
            //work on the picture later
            customer.PhoneNumber = customerDto.PhoneNumber;
            customer.State = customerDto.State;
            customer.Password = customerDto.Password;
            customer.UserActiveCountryId = customerDto.UserActiveCountryId;
            
            //var user = await _userService.GetUserByChannelCode(customer.CustomerCode);
            //user.FirstName = customerDto.FirstName;
            //user.LastName = customerDto.LastName;
            //user.PhoneNumber = customerDto.PhoneNumber;
            //user.Email = customerDto.Email;
            //user.PictureUrl = customerDto.PictureUrl;
            //user.UserActiveCountryId = customerDto.UserActiveCountryId;
            //await _userService.UpdateUser(user.Id, user);

            await _uow.CompleteAsync();           
        }

        public async Task<List<IndividualCustomerDTO>> GetIndividualCustomers(string searchData)
        {
            return await _uow.IndividualCustomer.GetIndividualCustomers(searchData);
        }

        public async Task<IndividualCustomerDTO> GetCustomerByCode(string customerCode)
        {
            try
            {
                if (String.IsNullOrEmpty(customerCode))
                {
                    throw new GenericException("Invalid Individual Customer Code");
                }
                var customer = await _uow.IndividualCustomer.GetAsync(x => x.CustomerCode.ToLower() == customerCode.ToLower());
                if (customer == null)
                {
                    return new IndividualCustomerDTO { };
                }
                IndividualCustomerDTO individual = Mapper.Map<IndividualCustomerDTO>(customer);
                return individual;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<object> GetByCode(string customerCode)
        {
            try
            {
                if (String.IsNullOrEmpty(customerCode))
                {
                    throw new GenericException("Invalid Customer Code");
                }
                //check individual table
                var customer = await _uow.IndividualCustomer.GetAsync(x => x.CustomerCode.ToLower() == customerCode.ToLower());
                if (customer != null)
                {
                    IndividualCustomerDTO individual = Mapper.Map<IndividualCustomerDTO>(customer);
                    return individual;
                }
                else
                {
                    var company = await _uow.Company.GetAsync(x => x.CustomerCode.ToLower() == customerCode.ToLower());
                    if (company != null)
                    {
                        CompanyDTO companydto = Mapper.Map<CompanyDTO>(company);
                        return companydto;
                    }
                }
                return null;
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
