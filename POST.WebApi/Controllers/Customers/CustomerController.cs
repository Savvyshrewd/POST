﻿using POST.Core.IServices;
using POST.Services.Implementation;
using System.Threading.Tasks;
using System.Web.Http;
using POST.Core.IServices.Customers;
using POST.Core.DTO.Customers;
using POST.Core.Enums;
using POST.WebApi.Filters;
using System.Collections.Generic;
using POST.Core.IServices.Wallet;
using POST.Core.DTO.Wallet;
using POST.Core.DTO.Shipments;
using POST.Core.Domain;
using POST.Core.DTO.User;
using POST.CORE.DTO.Report;
using POST.Core.DTO.Account;

namespace POST.WebApi.Controllers
{
    [Authorize(Roles = "Shipment, ViewAdmin")]
    [RoutePrefix("api/Customer")]
    public class CustomerController : BaseWebApiController
    {
        private readonly ICustomerService _service;
        private readonly IWalletTransactionService _walletTransactionService;
        private readonly IWalletService _walletService;

        public CustomerController(ICustomerService service, IWalletTransactionService walletTransactionService, IWalletService walletService) : base(nameof(CustomerController))
        {
            _service = service;
            _walletTransactionService = walletTransactionService;
            _walletService = walletService;
        }

        [GIGLSActivityAuthorize(Activity = "Create")]
        [HttpPost]
        [Route("")]
        public async Task<IServiceResponse<CustomerDTO>> CreateCustomer(CustomerDTO customer)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var customerObj = await _service.CreateCustomer(customer);
                return new ServiceResponse<CustomerDTO>
                {
                    Object = customerObj
                };
            });

        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("")]
        public async Task<IServiceResponse<CustomerDTO>> GetCustomer(int customerId, CustomerType customerType)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var customerObj = await _service.GetCustomer(customerId, customerType);

                return new ServiceResponse<CustomerDTO>
                {
                    Object = customerObj
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("{phonenumber}/phonenumber")]
        public async Task<IServiceResponse<IndividualCustomerDTO>> GetCustomerByPhoneNumber(string phonenumber)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var customerObj = await _service.GetCustomerByPhoneNumber(phonenumber);

                return new ServiceResponse<IndividualCustomerDTO>
                {
                    Object = customerObj
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("{customerType}")]
        public async Task<IServiceResponse<List<CustomerDTO>>> GetCustomers(CustomerType customerType)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var customerObj = await _service.GetCustomers(customerType);

                return new ServiceResponse<List<CustomerDTO>>
                {
                    Object = customerObj
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("searchcustomers")]
        public async Task<IServiceResponse<List<CustomerDTO>>> SearchForCustomers(CustomerSearchOption searchOption)
        {
            return await HandleApiOperationAsync(async () =>
            {

                var customersObj = await _service.SearchForCustomers(searchOption);

                return new ServiceResponse<List<CustomerDTO>>
                {
                    Object = customersObj
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("searchwallets")]
        public async Task<IServiceResponse<List<WalletDTO>>> SearchForWallets(WalletSearchOption searchOption)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var walletsObj = await _walletService.SearchForWallets(searchOption);

                return new ServiceResponse<List<WalletDTO>>
                {
                    Object = walletsObj
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("summary/{walletId:int}")]
        public async Task<IServiceResponse<WalletTransactionSummaryDTO>> GetWalletTransactionByWalletId(int walletId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var walletTransactionSummary = await _walletTransactionService.GetWalletTransactionByWalletId(walletId);

                return new ServiceResponse<WalletTransactionSummaryDTO>
                {
                    Object = walletTransactionSummary
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("code/{code}")]
        public async Task<IServiceResponse<IndividualCustomerDTO>> GetCustomerByCode(string code)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var customerObj = await _service.GetCustomerByCode(code);

                return new ServiceResponse<IndividualCustomerDTO>
                {
                    Object = customerObj
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("deliveryno/{deliveryNo}")]
        public async Task<IServiceResponse<DeliveryNumberDTO>> GetDeliveryNoByWaybill(string deliveryNo)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var item = await _service.GetDeliveryNoByWaybill(deliveryNo);

                return new ServiceResponse<DeliveryNumberDTO>
                {
                    Object = item
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("shipmentactivities/{waybill}")]
        public async Task<IServiceResponse<List<ShipmentActivityDTO>>> GetShipmentActivities(string waybill)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var activities = await _service.GetShipmentActivities(waybill);

                return new ServiceResponse<List<ShipmentActivityDTO>>
                {
                    Object = activities
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("intlcustomers/{email}/")]
        public async Task<IServiceResponse<UserDTO>> GetCustomer(string email)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var customerObj = await _service.GetInternationalUser(email);

                return new ServiceResponse<UserDTO>
                {
                    Object = customerObj
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpPut]
        [Route("deactivateintlcustomer/{email}/")]
        public async Task<IServiceResponse<bool>> ActivateUser(string email)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _service.DeactivateInternationalUser(email);

                return new ServiceResponse<bool>
                {
                    Object = true
                };
            });

        }


        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("getbycode/{code}")]
        public async Task<IServiceResponse<object>> GetByCode(string code)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var customerObj = await _service.GetByCode(code);

                return new ServiceResponse<object>
                {
                    Object = customerObj
                };
            });
        }

        [GIGLSActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("coporatetransactions")]
        public async Task<IServiceResponse<List<InvoiceViewDTO>>> GetCoporateTransactions(DateFilterForDropOff searchOption)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var transactions = await _service.GetCoporateMonthlyTransaction(searchOption);

                return new ServiceResponse<List<InvoiceViewDTO>>
                {
                    Object = transactions
                };
            });
        }


    }
}
