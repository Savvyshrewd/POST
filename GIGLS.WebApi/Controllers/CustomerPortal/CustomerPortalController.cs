﻿using GIGLS.Core.DTO;
using GIGLS.Core.DTO.Account;
using GIGLS.Core.DTO.Customers;
using GIGLS.Core.DTO.Dashboard;
using GIGLS.Core.DTO.Haulage;
using GIGLS.Core.DTO.OnlinePayment;
using GIGLS.Core.DTO.PaymentTransactions;
using GIGLS.Core.DTO.ServiceCentres;
using GIGLS.Core.DTO.Shipments;
using GIGLS.Core.DTO.SLA;
using GIGLS.Core.DTO.User;
using GIGLS.Core.DTO.Wallet;
using GIGLS.Core.DTO.Zone;
using GIGLS.Core.IServices;
using GIGLS.Core.IServices.Shipments;
using GIGLS.Core.IServices.CustomerPortal;
using GIGLS.Core.IServices.User;
using GIGLS.Core.IServices.Wallet;
using GIGLS.CORE.DTO.Report;
using GIGLS.CORE.DTO.Shipments;
using GIGLS.Infrastructure;
using GIGLS.Services.Implementation;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Http;
using GIGLS.Core;
using GIGLS.Core.IServices.ServiceCentres;
using GIGLS.Core.IServices.Business;

namespace GIGLS.WebApi.Controllers.CustomerPortal
{
    [Authorize]
    [RoutePrefix("api/portal")]
    public class CustomerPortalController : BaseWebApiController
    {
        //private readonly IUnitOfWork _uow;
        private readonly ICustomerPortalService _portalService;
        private readonly IPaystackPaymentService _paymentService;
        private readonly IOTPService _otpService;
        private readonly IUserService _userService;
        private readonly IPreShipmentMobileService _preshipmentmobileService;
        private readonly IStationService _stationService;
        private readonly IWalletService _walletService;
        private readonly IWalletTransactionService _walletTransactionService;


        public CustomerPortalController(ICustomerPortalService portalService, IPaystackPaymentService paymentService, IOTPService otpService,
            IUserService userService, IPreShipmentMobileService preshipmentmobileService, IStationService stationService, IWalletService walletService, IWalletTransactionService walletTransactionService) : base(nameof(CustomerPortalController))
        {
            // _uow = uow;
            _userService = userService;
            _otpService = otpService;
            _portalService = portalService;
            _paymentService = paymentService;
            _preshipmentmobileService = preshipmentmobileService;
            _stationService = stationService;
            _walletService = walletService;
            _walletTransactionService = walletTransactionService;
        }


        [HttpPost]
        [Route("transaction")]
        public async Task<IServiceResponse<List<InvoiceViewDTO>>> GetShipmentTransactions(ShipmentFilterCriteria f_Criteria)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var invoices = await _portalService.GetShipmentTransactions(f_Criteria);

                return new ServiceResponse<List<InvoiceViewDTO>>
                {
                    Object = invoices
                };
            });
        }

        //[Authorize]
        [HttpPut]
        [Route("wallet/{walletId:int}")]
        public async Task<IServiceResponse<object>> UpdateWallet(int walletId, WalletTransactionDTO walletTransactionDTO)
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _portalService.UpdateWallet(walletId, walletTransactionDTO);
                return new ServiceResponse<object>
                {
                    Object = true
                };
            });
        }


        [HttpPost]
        [Route("addwalletpaymentlog")]
        public async Task<IServiceResponse<object>> AddWalletPaymentLog(WalletPaymentLogDTO walletPaymentLogDTO)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var walletPaymentLog = await _portalService.AddWalletPaymentLog(walletPaymentLogDTO);

                return new ServiceResponse<object>
                {
                    Object = walletPaymentLog
                };
            });
        }


        [HttpPost]
        [Route("paywithpaystack")]
        public async Task<IServiceResponse<object>> PaywithPaystack(WalletPaymentLogDTO paymentinfo)
        {
            return await HandleApiOperationAsync(async () =>
            {

                //Add wallet payment log
                var walletPaymentLog = await _portalService.AddWalletPaymentLog(paymentinfo);

                //initialize the secret key from paystack
                var testOrLiveSecret = ConfigurationManager.AppSettings["PayStackSecret"];

                //Call the paystack class implementation to do the payment
                var result = await _paymentService.MakePayment(testOrLiveSecret, paymentinfo);
                var updateresult = new object();

                if (result)
                {
                    paymentinfo.TransactionStatus = "Success";
                    updateresult = await _portalService.UpdateWalletPaymentLog(paymentinfo);
                }

                return new ServiceResponse<object>
                {
                    Object = updateresult
                };
            });
        }


        [HttpGet]
        [Route("verifypayment/{referenceCode}")]
        public async Task<IServiceResponse<PaymentResponse>> VerifyAndValidateWallet(string referenceCode)
        {
            //return await HandleApiOperationAsync(async () =>
            //{
            var result = await _paymentService.VerifyAndValidateWallet(referenceCode);

            return new ServiceResponse<PaymentResponse>
            {
                Object = result
            };
            //});
        }


        [HttpGet]
        [Route("walletpaymentlog")]
        public async Task<IServiceResponse<List<WalletPaymentLogDTO>>> GetWalletPaymentLogs([FromUri]FilterOptionsDto filterOptionsDto)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var walletTuple = await _portalService.GetWalletPaymentLogs(filterOptionsDto);
                return new ServiceResponse<List<WalletPaymentLogDTO>>
                {
                    Object = await walletTuple.Item1,
                    Total = walletTuple.Item2
                };
            });
        }

        [HttpPut]
        [Route("updatewalletpaymentlog")]
        public async Task<IServiceResponse<object>> UpdateWalletPaymentLog(WalletPaymentLogDTO walletPaymentLogDTO)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var walletPaymentLog = await _portalService.UpdateWalletPaymentLog(walletPaymentLogDTO);

                return new ServiceResponse<object>
                {
                    Object = walletPaymentLog
                };
            });
        }


        [HttpGet]
        [Route("wallet")]
        public async Task<IServiceResponse<WalletTransactionSummaryDTO>> GetWalletTransactions()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var walletTransactionSummary = await _portalService.GetWalletTransactions();

                return new ServiceResponse<WalletTransactionSummaryDTO>
                {
                    Object = walletTransactionSummary
                };
            });
        }

        [HttpGet]
        [Route("invoice")]
        public async Task<IServiceResponse<IEnumerable<InvoiceViewDTO>>> GetInvoices()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var invoice = await _portalService.GetInvoices();

                return new ServiceResponse<IEnumerable<InvoiceViewDTO>>
                {
                    Object = invoice
                };
            });
        }

        [HttpGet]
        [Route("bywaybill/{waybill}")]
        public async Task<IServiceResponse<InvoiceDTO>> GetInvoiceByWaybill([FromUri]  string waybill)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var invoice = await _portalService.GetInvoiceByWaybill(waybill);

                return new ServiceResponse<InvoiceDTO>
                {
                    Object = invoice
                };
            });
        }

        [HttpGet]
        [Route("{waybillNumber}")]
        public async Task<IServiceResponse<IEnumerable<ShipmentTrackingDTO>>> TrackShipment(string waybillNumber)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _portalService.TrackShipment(waybillNumber);

                return new ServiceResponse<IEnumerable<ShipmentTrackingDTO>>
                {
                    Object = result
                };
            });
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("public/{waybillNumber}")]
        public async Task<IServiceResponse<IEnumerable<ShipmentTrackingDTO>>> PublicTrackShipment(string waybillNumber)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _portalService.PublicTrackShipment(waybillNumber);

                return new ServiceResponse<IEnumerable<ShipmentTrackingDTO>>
                {
                    Object = result
                };
            });
        }

        [HttpGet]
        [Route("cod")]
        public async Task<IServiceResponse<CashOnDeliveryAccountSummaryDTO>> GetCashOnDeliveryAccount()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _portalService.GetCashOnDeliveryAccount();

                return new ServiceResponse<CashOnDeliveryAccountSummaryDTO>
                {
                    Object = result
                };
            });
        }

        [HttpGet]
        [Route("partialPaymentTransaction/{waybill}")]
        public async Task<IServiceResponse<IEnumerable<PaymentPartialTransactionDTO>>> GetPartialPaymentTransaction(string waybill)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var payment = await _portalService.GetPartialPaymentTransaction(waybill);

                return new ServiceResponse<IEnumerable<PaymentPartialTransactionDTO>>
                {
                    Object = payment
                };
            });
        }


        [HttpGet]
        [Route("dashboard")]
        public async Task<IServiceResponse<DashboardDTO>> GetDashboard()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var dashboard = await _portalService.GetDashboard();

                return new ServiceResponse<DashboardDTO>
                {
                    Object = dashboard
                };
            });
        }


        [HttpGet]
        [Route("state")]
        public async Task<IServiceResponse<IEnumerable<StateDTO>>> GetStates(int pageSize = 10, int page = 1)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var state = await _portalService.GetStates(pageSize, page);
                var total = _portalService.GetStatesTotal();

                return new ServiceResponse<IEnumerable<StateDTO>>
                {
                    Total = total,
                    Object = state
                };
            });
        }


        [HttpGet]
        [Route("localservicecentre")]
        public async Task<IServiceResponse<IEnumerable<ServiceCentreDTO>>> GetLocalServiceCentres()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var centres = await _portalService.GetLocalServiceCentres();
                return new ServiceResponse<IEnumerable<ServiceCentreDTO>>
                {
                    Object = centres
                };
            });
        }


        [HttpGet]
        [Route("deliveryoption")]
        public async Task<IServiceResponse<IEnumerable<DeliveryOptionDTO>>> GetDeliveryOptions()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var delivery = await _portalService.GetDeliveryOptions();

                return new ServiceResponse<IEnumerable<DeliveryOptionDTO>>
                {
                    Object = delivery
                };
            });
        }

        [HttpGet]
        [Route("specialdomesticpackage")]
        public async Task<IServiceResponse<IEnumerable<SpecialDomesticPackageDTO>>> GetSpecialDomesticPackages()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var packages = await _portalService.GetSpecialDomesticPackages();

                return new ServiceResponse<IEnumerable<SpecialDomesticPackageDTO>>
                {
                    Object = packages
                };
            });
        }

        [HttpGet]
        [Route("haulage")]
        public async Task<IServiceResponse<IEnumerable<HaulageDTO>>> GetHaulages()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var haulage = await _portalService.GetHaulages();

                return new ServiceResponse<IEnumerable<HaulageDTO>>
                {
                    Object = haulage
                };
            });
        }

        [HttpGet]
        [Route("vat")]
        public async Task<IServiceResponse<VATDTO>> GetVATs()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var vat = await _portalService.GetVATs();
                return new ServiceResponse<VATDTO>
                {
                    Object = vat.FirstOrDefault()
                };
            });
        }

        [HttpGet]
        [Route("insurance")]
        public async Task<IServiceResponse<InsuranceDTO>> GetInsurances()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var insurance = await _portalService.GetInsurances();
                return new ServiceResponse<InsuranceDTO>
                {
                    Object = insurance.FirstOrDefault()
                };
            });
        }

        [HttpGet]
        [Route("{departure:int}/{destination:int}")]
        public async Task<IServiceResponse<DomesticRouteZoneMapDTO>> GetZone(int departure, int destination)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var zone = await _portalService.GetZone(departure, destination);

                return new ServiceResponse<DomesticRouteZoneMapDTO>
                {
                    Object = zone
                };
            });
        }

        [HttpPost]
        [Route("price")]
        public async Task<IServiceResponse<decimal>> GetPrice(PricingDTO pricingDto)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var price = await _portalService.GetPrice(pricingDto);

                return new ServiceResponse<decimal>
                {
                    Object = price
                };
            });
        }

        [HttpPost]
        [Route("haulageprice")]
        public async Task<IServiceResponse<decimal>> GetHaulagePrice(HaulagePricingDTO haulagePricingDto)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var price = await _portalService.GetHaulagePrice(haulagePricingDto);

                return new ServiceResponse<decimal>
                {
                    Object = price
                };
            });
        }

        [HttpGet]
        [Route("user/{userId}")]
        public async Task<IServiceResponse<CustomerDTO>> GetUser(string userId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var user = await _portalService.GetCustomer(userId);
                return new ServiceResponse<CustomerDTO>
                {
                    Object = user
                };
            });
        }

        [HttpPut]
        [Route("changepassword/{userid}/{currentPassword}/{newPassword}")]
        public async Task<IServiceResponse<bool>> ChangePassword(string userid, string currentPassword, string newPassword)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _portalService.ChangePassword(userid, currentPassword, newPassword);

                if (!result.Succeeded)
                {
                    throw new GenericException("Operation could not complete successfully");
                }

                return new ServiceResponse<bool>
                {
                    Object = true
                };
            });
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("register")]
        public async Task<IServiceResponse<UserDTO>> Register(UserDTO user)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var registerUser = await _portalService.Register(user);
                return new ServiceResponse<UserDTO>
                {
                    Object = registerUser
                };
            });
        }


        [HttpGet]
        [Route("PreShipments")]
        public async Task<IServiceResponse<IEnumerable<PreShipmentDTO>>> GetPreShipments([FromUri]FilterOptionsDto filterOptionsDto)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var preShipments = await _portalService.GetPreShipments(filterOptionsDto);
                return new ServiceResponse<IEnumerable<PreShipmentDTO>>
                {
                    Object = preShipments
                };
            });
        }

        [HttpGet]
        [Route("PreShipments/{waybill}")]
        public async Task<IServiceResponse<PreShipmentDTO>> GetPreShipment(string waybill)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var preShipment = await _portalService.GetPreShipment(waybill);
                return new ServiceResponse<PreShipmentDTO>
                {
                    Object = preShipment
                };
            });
        }

        [HttpGet]
        [Route("sla")]
        public async Task<IServiceResponse<SLADTO>> GetSLA()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var sla = await _portalService.GetSLA();

                return new ServiceResponse<SLADTO>
                {
                    Object = sla
                };
            });
        }

        [HttpGet]
        [Route("sla/{slaId:int}")]
        public async Task<IServiceResponse<object>> SignSLA(int slaId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var sla = await _portalService.SignSLA(slaId);

                return new ServiceResponse<object>
                {
                    Object = sla
                };
            });
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("signup")]
        public async Task<IServiceResponse<SignResponseDTO>> SignUp(UserDTO user)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var signUp = await _portalService.SignUp(user);

                return new ServiceResponse<SignResponseDTO>
                {
                    Object = signUp
                };
            });
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("validateotp/{OTP}")]
        public async Task<IServiceResponse<JObject>> IsOTPValid(int OTP)
        {
            var Otp = await _otpService.IsOTPValid(OTP);
            if (Otp != null && Otp.IsActive == true)
            {
                string apiBaseUri = ConfigurationManager.AppSettings["WebApiUrl"];
                string getTokenResponse;

                return await HandleApiOperationAsync(async () =>
                {
                    using (var client = new HttpClient())
                    {
                        //setup client
                        client.BaseAddress = new Uri(apiBaseUri);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        //setup login data
                        var formContent = new FormUrlEncodedContent(new[]
                            {
                         new KeyValuePair<string, string>("grant_type", "password"),
                         new KeyValuePair<string, string>("Username", Otp.Username),
                         new KeyValuePair<string, string>("Password", Otp.UserChannelPassword),
                         });

                        //setup login data
                        HttpResponseMessage responseMessage = client.PostAsync("token", formContent).Result;

                        if (!responseMessage.IsSuccessStatusCode)
                        {
                            throw new GenericException("Operation could not complete login successfully:");
                        }

                        //get access token from response body
                        var responseJson = await responseMessage.Content.ReadAsStringAsync();
                        var jObject = JObject.Parse(responseJson);

                        getTokenResponse = jObject.GetValue("access_token").ToString();

                        return new ServiceResponse<JObject>
                        {
                            Object = jObject
                        };
                    }
                });
            }
            else
            {
                var jObject = JObject.FromObject(Otp);
                return new ServiceResponse<JObject>
                {
                    ShortDescription = "User has not been verified",
                    Object = jObject

                };
            }
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("resendotp")]
        public async Task<IServiceResponse<SignResponseDTO>> ResendOTP(UserDTO user)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var resendOTP = await _portalService.ResendOTP(user);

                return new ServiceResponse<SignResponseDTO>
                {
                    Object = resendOTP
                };
            });
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("login/{UserDetail}/{Password}")]
        public async Task<IServiceResponse<JObject>> Login(string UserDetail, string Password)
        {

            var user = await _otpService.CheckDetails(UserDetail);
            if (user.Username != null)
            {
                user.Username = user.Username.Trim();
            }

            if (Password != null)
            {
                Password = Password.Trim();
            }
            if (user != null && user.IsActive == true)
            {
                string apiBaseUri = ConfigurationManager.AppSettings["WebApiUrl"];
                string getTokenResponse;

                return await HandleApiOperationAsync(async () =>
                {
                    using (var client = new HttpClient())
                    {
                        //setup client
                        client.BaseAddress = new Uri(apiBaseUri);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        //setup login data
                        var formContent = new FormUrlEncodedContent(new[]
                            {
                         new KeyValuePair<string, string>("grant_type", "password"),
                         new KeyValuePair<string, string>("Username", user.Username),
                         new KeyValuePair<string, string>("Password", Password),
                         });

                        //setup login data
                        HttpResponseMessage responseMessage = client.PostAsync("token", formContent).Result;

                        if (!responseMessage.IsSuccessStatusCode)
                        {
                            throw new GenericException("Operation could not complete login successfully:");
                        }

                        //get access token from response body
                        var responseJson = await responseMessage.Content.ReadAsStringAsync();
                        var jObject = JObject.Parse(responseJson);

                        getTokenResponse = jObject.GetValue("access_token").ToString();

                        return new ServiceResponse<JObject>
                        {
                            Object = jObject
                        };
                    }
                });
            }
            else
            {
                var jObject = JObject.FromObject(user);
                return new ServiceResponse<JObject>
                {
                    ShortDescription = "User has not been verified",
                    Object = jObject
                };
            }
        }

        [HttpPost]
        [Route("editprofile")]
        public async Task<IServiceResponse<bool>> EditProfile(UserDTO user)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var registerUser = await _portalService.Register(user);

                return new ServiceResponse<bool>
                {
                    Object = true
                };
            });
        }

        [HttpGet]
        [Route("itemTypes")]
        public async Task<IServiceResponse<List<string>>> GetItemTypes()
        {
            var ItemTypes = _portalService.GetItemTypes();
            return new ServiceResponse<List<string>>
            {
                Object = ItemTypes,
            };
        }

        [HttpPost]
        [Route("createshipment")]
        public async Task<IServiceResponse<object>> CreateShipment(PreShipmentMobileDTO PreshipmentMobile)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var PreshipMentMobile = await _preshipmentmobileService.AddPreShipmentMobile(PreshipmentMobile);

                return new ServiceResponse<object>
                {
                    Object = PreshipMentMobile
                };
            });
        }

        [HttpGet]
        [Route("getStations")]
        public async Task<IServiceResponse<IEnumerable<StationDTO>>> GetStations()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var stations = await _stationService.GetLocalStations();
                return new ServiceResponse<IEnumerable<StationDTO>>
                {
                    Object = stations,
                };
            });
        }

        [HttpGet]
        [Route("getwallettransactionandpreshipmenthistory")]
        public async Task<IServiceResponse<WalletTransactionSummaryDTO>> GetWalletTransactionAndPreshipmentHistory()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var Transactionhistory = await _walletTransactionService.GetWalletTransactionsForMobile();
                var preshipments = await _preshipmentmobileService.GetPreShipmentForUser();
                Transactionhistory.Shipments = preshipments;
                return new ServiceResponse<WalletTransactionSummaryDTO>
                {
                    Object = Transactionhistory
                };
            });
        }

        [HttpPost]
        [Route("getprice")]
        public async Task<IServiceResponse<MobilePriceDTO>> GetPrice(PreShipmentMobileDTO PreshipmentMobile)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var Price = await _preshipmentmobileService.GetPrice(PreshipmentMobile);

                return new ServiceResponse<MobilePriceDTO>
                {
                    Object = Price,
                };
            });
        }

        [HttpGet]
        [Route("getpreshipmenthistory")]
        public async Task<IServiceResponse<List<PreShipmentMobileDTO>>> GetPreshipmentHistory()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var PreshipMentMobile = await _preshipmentmobileService.GetPreShipmentForUser();
                return new ServiceResponse<List<PreShipmentMobileDTO>>
                {
                    Object = PreshipMentMobile,
                };
            });
        }

        //Should be discard 
        [HttpGet]
        [Route("verifypaystackpayment/{reference}/{UserId}")]
        public async Task<IServiceResponse<PaystackWebhookDTO>> VerifyMobilePayment(string reference, string UserId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var walletPaymentLog = await _paymentService.VerifyPaymentMobile(reference, UserId);
                return new ServiceResponse<PaystackWebhookDTO>
                {

                    Object = walletPaymentLog
                };
            });
        }

        [HttpGet]
        [Route("getwalletbalance")]
        public async Task<IServiceResponse<decimal>> GetWalletBalance()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var wallet = await _walletService.GetWalletBalance();
                return new ServiceResponse<decimal>
                {
                    Object = wallet.Balance
                };
            });
        }
    }
}