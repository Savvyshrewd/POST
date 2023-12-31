﻿using POST.Core.DTO;
using POST.Core.DTO.Account;
using POST.Core.DTO.Customers;
using POST.Core.DTO.Dashboard;
using POST.Core.DTO.Haulage;
using POST.Core.DTO.OnlinePayment;
using POST.Core.DTO.PaymentTransactions;
using POST.Core.DTO.ServiceCentres;
using POST.Core.DTO.Shipments;
using POST.Core.DTO.SLA;
using POST.Core.DTO.User;
using POST.Core.DTO.Wallet;
using POST.Core.DTO.Zone;
using POST.Core.IServices;
using POST.Core.IServices.CustomerPortal;
using POST.Core.IServices.Wallet;
using POST.CORE.DTO.Shipments;
using POST.Infrastructure;
using POST.Services.Implementation;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using POST.Core.Domain.BankSettlement;
using POST.Core.DTO.Partnership;
using POST.Core.DTO.Report;
using POST.Core.Enums;
using POST.Core.DTO.MessagingLog;
using POST.Core.DTO.Admin;
using POST.Core.Domain;
using EfeAuthen.Models;
using POST.Core.DTO.Utility;
using POST.Core.DTO.Fleets;
using System.Net;
using POST.Core.DTO.ShipmentScan;
using POST.Core.IServices.Shipments;
using POST.Services.Implementation.Utility;
using POST.Core.DTO.Stores;
using POST.CORE.DTO.Report;
using POST.Core.DTO.DHL;

namespace POST.WebApi.Controllers.CustomerPortal
{
    [Authorize]
    [RoutePrefix("api/portal")]
    public class CustomerPortalController : BaseWebApiController
    {
        private readonly ICustomerPortalService _portalService;
        private readonly IMagayaService _magayaService;

        public CustomerPortalController(ICustomerPortalService portalService, IMagayaService magayaService) : base(nameof(CustomerPortalController))
        {
            _portalService = portalService;
            _magayaService = magayaService;
        }

        [HttpPost]
        [Route("transaction")]
        public async Task<IServiceResponse<List<InvoiceViewDTO>>> GetShipmentTransactions(ShipmentCollectionFilterCriteria f_Criteria)
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

        [HttpPost]
        [Route("AddIntlShipmentTransactions")]
        public async Task<IServiceResponse<object>> AddIntlShipmentTransactions(IntlShipmentRequestDTO TransactionDTO)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _magayaService.CreateIntlShipmentRequest(TransactionDTO);
                return new ServiceResponse<object>
                {
                    Object = result
                };
            });
        }

        [HttpPut]
        [Route("intlshipmentrequest/{requestNumber}")]
        public async Task<IServiceResponse<object>> UpdateIntlShipmentRequest(string requestNumber, IntlShipmentRequestDTO transactionDTO)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _magayaService.UpdateIntlShipmentRequest(requestNumber, transactionDTO);
                return new ServiceResponse<object>
                {
                    Object = result
                };
            });
        }

        [HttpGet]
        [Route("releaseMovementManifest/{movementmanifestcode}/{code}")]
        public async Task<IServiceResponse<bool>> ReleaseMovementManifest(ReleaseMovementManifestDto movementManifestVals)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _portalService.ReleaseMovementManifest(movementManifestVals);
                return new ServiceResponse<bool>
                {
                    Object = result
                };
            });
        }

        [HttpPost]
        [Route("movementmanifestbydate")]
        public async Task<IServiceResponse<IEnumerable<MovementManifestNumberDTO>>> GetAllManifestMovementManifestNumberMappings(DateFilterCriteria dateFilterCriteria)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var MovementmanifestNumberMappings = await _portalService.GetAllManifestMovementManifestNumberMappings(dateFilterCriteria);

                return new ServiceResponse<IEnumerable<MovementManifestNumberDTO>>
                {
                    Object = MovementmanifestNumberMappings
                };
            });
        }

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
        [Route("addwaybillpaymentlog")]
        public async Task<IServiceResponse<object>> AddWaybillPaymentLogFromApp(WaybillPaymentLogDTO walletPaymentLogDTO)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var walletPaymentLog = await _portalService.AddWaybillPaymentLogFromApp(walletPaymentLogDTO);

                return new ServiceResponse<object>
                {
                    Object = walletPaymentLog
                };
            });
        }

        [HttpPost]
        [Route("initiatepaymentusingussd")]
        public async Task<IServiceResponse<USSDResponse>> InitiatePaymentUsingUSSD(WalletPaymentLogDTO walletPaymentLogDTO)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var walletPaymentLog = await _portalService.InitiatePaymentUsingUSSD(walletPaymentLogDTO);

                return new ServiceResponse<USSDResponse>
                {
                    Object = walletPaymentLog
                };
            });
        }

        //[HttpPost]
        //[Route("paywithpaystack")]
        //public async Task<IServiceResponse<object>> PaywithPaystack(WalletPaymentLogDTO paymentinfo)
        //{
        //    return await HandleApiOperationAsync(async () =>
        //    {

        //        //Add wallet payment log
        //        var walletPaymentLog = await _portalService.AddWalletPaymentLog(paymentinfo);

        //        //initialize the secret key from paystack
        //        var testOrLiveSecret = ConfigurationManager.AppSettings["PayStackSecret"];

        //        //Call the paystack class implementation to do the payment
        //        var result = await _paymentService.MakePayment(testOrLiveSecret, paymentinfo);
        //        var updateresult = new object();

        //        if (result)
        //        {
        //            paymentinfo.TransactionStatus = "Success";
        //            updateresult = await _portalService.UpdateWalletPaymentLog(paymentinfo);
        //        }

        //        return new ServiceResponse<object>
        //        {
        //            Object = updateresult
        //        };
        //    });
        //}


        [HttpGet]
        [Route("verifypayment/{referenceCode}")]
        public async Task<IServiceResponse<PaymentResponse>> VerifyAndValidateWallet(string referenceCode)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _portalService.VerifyAndValidatePayment(referenceCode);

                return new ServiceResponse<PaymentResponse>
                {
                    Object = result
                };
            });
        }

        

        [HttpGet]
        [Route("gatewaycode")]
        public async Task<IServiceResponse<GatewayCodeResponse>> GetGatewayCode()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _portalService.GetGatewayCode();
                return new ServiceResponse<GatewayCodeResponse>
                {
                    Object = result
                };
            });
        }

        [HttpGet]
        [Route("walletpaymentlog")]
        public async Task<IServiceResponse<List<WalletPaymentLogDTO>>> GetWalletPaymentLogs([FromUri] FilterOptionsDto filterOptionsDto)
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


        [HttpPost]
        [Route("wallet")]
        public async Task<IServiceResponse<WalletTransactionSummaryDTO>> GetWalletTransactions(PaginationDTO pagination)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var walletTransactionSummary = await _portalService.GetWalletTransactions(pagination);

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
        public async Task<IServiceResponse<InvoiceDTO>> GetInvoiceByWaybill([FromUri] string waybill)
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
        [Route("GetPaidCODByCustomer")]
        public async Task<IServiceResponse<List<CodPayOutList>>> GetPaidCODByCustomer()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var invoice = await _portalService.GetPaidCODByCustomer();

                return new ServiceResponse<List<CodPayOutList>>
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

        [HttpPost]
        [Route("changepassword")]
        public async Task<IServiceResponse<bool>> ChangePassword(ChangePasswordDTO passwordDTO)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _portalService.ChangePassword(passwordDTO);

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

        // [AllowAnonymous]
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


        //[HttpGet]
        //[Route("PreShipments")]
        //public async Task<IServiceResponse<IEnumerable<PreShipmentDTO>>> GetPreShipments([FromUri]FilterOptionsDto filterOptionsDto)
        //{
        //    return await HandleApiOperationAsync(async () =>
        //    {
        //        var preShipments = await _portalService.GetPreShipments(filterOptionsDto);
        //        return new ServiceResponse<IEnumerable<PreShipmentDTO>>
        //        {
        //            Object = preShipments
        //        };
        //    });
        //}

        //[HttpGet]
        //[Route("PreShipments/{waybill}")]
        //public async Task<IServiceResponse<PreShipmentDTO>> GetPreShipment(string waybill)
        //{
        //    return await HandleApiOperationAsync(async () =>
        //    {
        //        var preShipment = await _portalService.GetPreShipment(waybill);
        //        return new ServiceResponse<PreShipmentDTO>
        //        {
        //            Object = preShipment
        //        };
        //    });
        //}

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
        [Route("verifyotp")]
        public async Task<IServiceResponse<JObject>> ValidateOTP(OTPDTO otp)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var userDto = await _portalService.ValidateOTP(otp);
                if (userDto != null && userDto.IsActive == true && otp.isPINCreation == false)
                {
                    string apiBaseUri = ConfigurationManager.AppSettings["WebApiUrl"];
                    string getTokenResponse;

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
                            new KeyValuePair<string, string>("Username", userDto.Username),
                            new KeyValuePair<string, string>("Password", userDto.UserChannelPassword),
                        });

                        //setup login data
                        HttpResponseMessage responseMessage = await client.PostAsync("token", formContent);

                        if (!responseMessage.IsSuccessStatusCode)
                        {
                            throw new GenericException("Incorrect Login Details", $"{(int)HttpStatusCode.Forbidden}");
                        }
                        else
                        {
                            userDto = await _portalService.GenerateReferrerCode(userDto);
                        }

                        //get access token from response body
                        var responseJson = await responseMessage.Content.ReadAsStringAsync();
                        var jObject = JObject.Parse(responseJson);

                        //Get country detail
                        var country = await _portalService.GetUserCountryCode(userDto);
                        var countryJson = JObject.FromObject(country);

                        //jObject.Add(countryJson);
                        jObject.Add(new JProperty("Country", countryJson));

                        getTokenResponse = jObject.GetValue("access_token").ToString();
                        return new ServiceResponse<JObject>
                        {
                            Object = jObject,
                            ReferrerCode = userDto.Referrercode
                        };
                    }
                }
                else if (userDto != null && userDto.IsActive == true && otp.isPINCreation == true)
                {
                    var data = new { IsActive = true };

                    var jObject = JObject.FromObject(data);

                    return new ServiceResponse<JObject>
                    {
                        Code = $"{(int)HttpStatusCode.OK}",
                        ShortDescription = "OTP validation successful",
                        Object = jObject
                    };
                }
                else
                {
                    var data = new { IsActive = false };

                    var jObject = JObject.FromObject(data);

                    return new ServiceResponse<JObject>
                    {
                        Code = $"{(int)HttpStatusCode.BadRequest}",
                        ShortDescription = "User has not been verified",
                        Object = jObject
                    };
                }
            });
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

        //Login for GIGGO App
        [AllowAnonymous]
        [HttpPost]
        [Route("login")]
        public async Task<IServiceResponse<JObject>> Login(MobileLoginDTO logindetail)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var user = await _portalService.CheckDetails(logindetail.UserDetail, logindetail.UserChannelType);

                if (user.RequiresCod == null)
                    user.RequiresCod = false;

                if (user.IsUniqueInstalled == null)
                    user.IsUniqueInstalled = false;

                if (user.IsEligible == null)
                    user.IsEligible = false;


                var vehicle = user.VehicleType;
                var vehicleDetails = user.VehicleDetails;
                var bankName = "";
                var accountName = "";
                var accountNumber = "";
                var partnerType = "";

                if (user.Username != null)
                {
                    user.Username = user.Username.Trim();
                }

                if (logindetail.Password != null)
                {
                    logindetail.Password = logindetail.Password.Trim();
                }

                if (user.UserChannelType == UserChannelType.Employee)
                {
                    throw new GenericException("You are not authorized to login on this platform.", $"{(int)HttpStatusCode.Forbidden}");
                }

                if (user != null && user.IsActive == true)
                {
                    using (var client = new HttpClient())
                    {
                        string apiBaseUri = ConfigurationManager.AppSettings["WebApiUrl"];
                        string getTokenResponse;
                        //setup client
                        client.BaseAddress = new Uri(apiBaseUri);
                        client.DefaultRequestHeaders.Accept.Clear();
                        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                        //setup login data
                        var formContent = new FormUrlEncodedContent(new[]
                        {
                            new KeyValuePair<string, string>("grant_type", "password"),
                            new KeyValuePair<string, string>("Username", user.Username),
                            new KeyValuePair<string, string>("Password", logindetail.Password),
                        });

                        //setup login data
                        HttpResponseMessage responseMessage = await client.PostAsync("token", formContent);

                        if (!responseMessage.IsSuccessStatusCode)
                        {
                            throw new GenericException("Incorrect Login Details");
                        }
                        else
                        {
                            if (logindetail.UserChannelType == UserChannelType.IndividualCustomer.ToString())
                            {
                                var response = await _portalService.CreateCustomer(user.UserChannelCode);
                            }

                            if (logindetail.UserChannelType == UserChannelType.Partner.ToString())
                            {
                                var partner = await _portalService.CreatePartner(user.UserChannelCode);
                                partnerType = partner.PartnerType.ToString();
                                bankName = partner.BankName;
                                accountName = partner.AccountName;
                                accountNumber = partner.AccountNumber;
                                if (partnerType == PartnerType.InternalDeliveryPartner.ToString())
                                {
                                    user.IsVerified = true;
                                    await _portalService.AddWallet(new WalletDTO
                                    {
                                        CustomerId = partner.PartnerId,
                                        CustomerType = CustomerType.Partner,
                                        CustomerCode = user.UserChannelCode,
                                        CompanyType = CustomerType.Partner.ToString()
                                    });
                                }
                                if (logindetail.UserChannelType == UserChannelType.Ecommerce.ToString())
                                {
                                    var response = await _portalService.CreateCompany(user.UserChannelCode);
                                }
                            }

                            //get access token from response body
                            var responseJson = await responseMessage.Content.ReadAsStringAsync();
                            var jObject = JObject.Parse(responseJson);

                            //Get country detail
                            var country = await _portalService.GetUserCountryCode(user);
                            var countryJson = JObject.FromObject(country);

                            //jObject.Add(countryJson);
                            jObject.Add(new JProperty("Country", countryJson));

                            getTokenResponse = jObject.GetValue("access_token").ToString();
                            return new ServiceResponse<JObject>
                            {
                                VehicleType = vehicle,
                                VehicleDetails = vehicleDetails,
                                Object = jObject,
                                ReferrerCode = user.Referrercode,
                                AverageRatings = user.AverageRatings,
                                IsVerified = user.IsVerified,
                                PartnerType = partnerType,
                                IsEligible = (bool)user.IsEligible,
                                BankName = bankName,
                                AccountName = accountName,
                                AccountNumber = accountNumber
                            };
                        }
                    }
                }
                else
                {
                    var data = new { IsActive = false };
                    var jObject = JObject.FromObject(data);

                    return new ServiceResponse<JObject>
                    {
                        Code = $"{(int)HttpStatusCode.BadRequest}",
                        ShortDescription = "User has not been verified",
                        Object = jObject
                    };
                }
            });
        }

        //Login for Customer Portal
        [AllowAnonymous]
        [HttpPost]
        [Route("customerLogin")]
        public async Task<IServiceResponse<JObject>> CustomerLogin(UserloginDetailsModel userLoginModel)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var user = await _portalService.CheckDetailsForCustomerPortal(userLoginModel.username);

                if (user.Username != null)
                {
                    user.Username = user.Username.Trim();
                }

                if (userLoginModel.Password != null)
                {
                    userLoginModel.Password = userLoginModel.Password.Trim();
                }

                if (user.UserChannelType == UserChannelType.Employee)
                {
                    throw new GenericException("You are not authorized to login on this platform.", $"{(int)HttpStatusCode.Forbidden}");
                }

                string apiBaseUri = ConfigurationManager.AppSettings["WebApiUrl"];
                string getTokenResponse;

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
                         new KeyValuePair<string, string>("Password", userLoginModel.Password),
                     });

                    //setup login data
                    HttpResponseMessage responseMessage = await client.PostAsync("token", formContent);

                    if (!responseMessage.IsSuccessStatusCode)
                    {
                        throw new GenericException("Incorrect Login Details");
                    }

                    //get access token from response body
                    var responseJson = await responseMessage.Content.ReadAsStringAsync();
                    var jObject = JObject.Parse(responseJson);

                    //Get country detail
                    var country = await _portalService.GetUserCountryCode(user);
                    var countryJson = JObject.FromObject(country);

                    //jObject.Add(countryJson);
                    jObject.Add(new JProperty("Country", countryJson));
                    getTokenResponse = jObject.GetValue("access_token").ToString();

                    return new ServiceResponse<JObject>
                    {
                        Object = jObject
                    };
                }
            });
        }

        [HttpPost]
        [Route("editprofile")]
        public async Task<IServiceResponse<bool>> EditProfile(UserDTO user)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var registerUser = await _portalService.EditProfile(user);

                return new ServiceResponse<bool>
                {
                    Object = registerUser
                };
            });
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("itemTypes")]
        public async Task<IServiceResponse<List<string>>> GetItemTypes()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var ItemTypes = await _portalService.GetItemTypes();
                return new ServiceResponse<List<string>>
                {
                    Object = ItemTypes,
                };
            });
        }

        [HttpPost]
        [Route("createshipment")]
        public async Task<IServiceResponse<object>> CreateShipment(PreShipmentMobileDTO PreshipmentMobile)
        {
            if (String.IsNullOrEmpty(PreshipmentMobile.ReceiverState))
            {
                PreshipmentMobile.ReceiverState = PreshipmentMobile.ReceiverCountry;
            }
            return await HandleApiOperationAsync(async () =>
            {
                var PreshipMentMobile = await _portalService.AddPreShipmentMobile(PreshipmentMobile);

                return new ServiceResponse<object>
                {
                    Object = PreshipMentMobile
                };
            });
        }

        [HttpPost]
        [Route("createMultipleShipments")]
        public async Task<IServiceResponse<MultipleShipmentOutput>> CreateMultipleShipments(NewPreShipmentMobileDTO PreshipmentMobile)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var preshipMentMobile = await _portalService.AddMultiplePreShipmentMobile(PreshipmentMobile);

                return new ServiceResponse<MultipleShipmentOutput>
                {
                    Object = preshipMentMobile
                };
            });
        }

        [HttpGet]
        [Route("getStation_s")]
        public async Task<IServiceResponse<IEnumerable<StationDTO>>> GetStations()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var stations = await _portalService.GetLocalStations();
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
                var Transactionhistory = await _portalService.GetWalletTransactionsForMobile();
                var preshipments = await _portalService.GetPreShipmentForUser();
                Transactionhistory.Shipments = preshipments;
                return new ServiceResponse<WalletTransactionSummaryDTO>
                {
                    Object = Transactionhistory
                };
            });
        }

        [HttpPost]
        [Route("getwallettransactionandpreshipmenthistory")]
        public async Task<IServiceResponse<ModifiedWalletTransactionSummaryDTO>> GetWalletTransactionAndPreshipmentHistory(ShipmentCollectionFilterCriteria filterCriteria)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var Transactionhistory = await _portalService.GetWalletTransactionsForMobile(filterCriteria);
                return new ServiceResponse<ModifiedWalletTransactionSummaryDTO>
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
                var Price = await _portalService.GetPrice(PreshipmentMobile);

                return new ServiceResponse<MobilePriceDTO>
                {
                    Object = Price,
                };
            });
        }

        [HttpPost]
        [Route("getPriceforMultipleShipments")]
        public async Task<IServiceResponse<MultipleMobilePriceDTO>> GetPriceForMultipleShipments(NewPreShipmentMobileDTO PreshipmentMobile)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var Price = await _portalService.GetPriceForMultipleShipments(PreshipmentMobile);

                return new ServiceResponse<MultipleMobilePriceDTO>
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
                var PreshipMentMobile = await _portalService.GetPreShipmentForUser();
                return new ServiceResponse<List<PreShipmentMobileDTO>>
                {
                    Object = PreshipMentMobile,
                };
            });
        }

        [HttpGet]
        [Route("getwalletbalance")]
        public async Task<IServiceResponse<decimal>> GetWalletBalance()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var wallet = await _portalService.GetWalletBalance();
                return new ServiceResponse<decimal>
                {
                    Object = wallet.Balance
                };
            });
        }

        [HttpGet]
        [Route("getwalletdetails")]
        public async Task<IServiceResponse<WalletDTO>> GetWalletDetails()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var wallet = await _portalService.GetWalletBalanceWithName();
                return new ServiceResponse<WalletDTO>
                {
                    Object = wallet
                };
            });
        }

        [HttpGet]
        [Route("getspecialpackages")]
        public async Task<IServiceResponse<SpecialResultDTO>> GetSpecialPackages()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var packages = await _portalService.GetSpecialPackages();

                return new ServiceResponse<SpecialResultDTO>
                {
                    Object = packages
                };
            });
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("trackshipment/{waybillNumber}")]
        public async Task<IServiceResponse<MobileShipmentTrackingHistoryDTO>> TrackMobileShipment(string waybillNumber)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _portalService.trackShipment(waybillNumber);

                return new ServiceResponse<MobileShipmentTrackingHistoryDTO>
                {
                    Object = result
                };
            });
        }

        [HttpPost]
        [Route("addmobilepickuprequest")]
        public async Task<IServiceResponse<PreShipmentMobileDTO>> AddPickupRequest(MobilePickUpRequestsDTO PickupRequest)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var shipmentItem = await _portalService.AddMobilePickupRequest(PickupRequest);

                return new ServiceResponse<PreShipmentMobileDTO>
                {
                    Object = shipmentItem
                };
            });
        }

        [HttpPost]
        [Route("addmobilepickuprequestmultiple")]
        public async Task<IServiceResponse<List<PreShipmentMobileDTO>>> AddMobilePickupRequestMultipleShipment(MobilePickUpRequestsDTO PickupRequest)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var shipmentItem = await _portalService.AddMobilePickupRequestMultipleShipment(PickupRequest);

                return new ServiceResponse<List<PreShipmentMobileDTO>>
                {
                    Object = shipmentItem
                };
            });
        }

        [HttpGet]
        [Route("getmobilepickuprequests")]
        public async Task<IServiceResponse<List<MobilePickUpRequestsDTO>>> GetPickupRequests()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var PickUpRequests = await _portalService.GetMobilePickupRequest();

                return new ServiceResponse<List<MobilePickUpRequestsDTO>>
                {
                    Object = PickUpRequests
                };
            });
        }

        [HttpPost]
        [Route("updatemobilepickuprequests")]
        public async Task<IServiceResponse<object>> UpdatePickupRequests(MobilePickUpRequestsDTO PickupRequest)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var flag = await _portalService.UpdateMobilePickupRequest(PickupRequest);

                return new ServiceResponse<object>
                {
                    Object = flag
                };
            });
        }

        [HttpPost]
        [Route("updatemobilepickuprequestsbygroup")]
        public async Task<IServiceResponse<object>> UpdatePickupRequestsUsingGroupCode(MobilePickUpRequestsDTO PickupRequest)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var flag = await _portalService.UpdateMobilePickupRequestUsingGroupCode(PickupRequest);

                return new ServiceResponse<object>
                {
                    Object = flag
                };
            });
        }

        [HttpPost]
        [Route("updatemobilepickuprequestsbywaybill")]
        public async Task<IServiceResponse<object>> UpdatePickupRequestsUsingWaybill(MobilePickUpRequestsDTO PickupRequest)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var flag = await _portalService.UpdateMobilePickupRequestUsingWaybill(PickupRequest);

                return new ServiceResponse<object>
                {
                    Object = flag
                };
            });
        }

        [HttpGet]
        [Route("getpreshipmentmobiledetails/{waybillNumber}")]
        public async Task<IServiceResponse<PreShipmentMobileDTO>> GetPreshipmentMobileDetails(string waybillNumber)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var details = await _portalService.GetPreShipmentDetail(waybillNumber);

                return new ServiceResponse<PreShipmentMobileDTO>
                {
                    Object = details
                };
            });
        }

        [HttpGet]
        [Route("getgroupcodewaybilldetails/{groupCode}")]
        public async Task<IServiceResponse<MobileGroupCodeWaybillMappingDTO>> GetWaybillDetailsInGroup(string groupCode)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var details = await _portalService.GetWaybillDetailsInGroup(groupCode);

                return new ServiceResponse<MobileGroupCodeWaybillMappingDTO>
                {
                    Object = details
                };
            });
        }

        [HttpGet]
        [Route("getwaybillsingroupcode/{groupCode}")]
        public async Task<IServiceResponse<MobileGroupCodeWaybillMappingDTO>> GetWaybillsInGroup(string groupCode)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var details = await _portalService.GetWaybillNumbersInGroup(groupCode);

                return new ServiceResponse<MobileGroupCodeWaybillMappingDTO>
                {
                    Object = details
                };
            });
        }


        [HttpPost]
        [Route("updatepreshipmentmobile")]
        public async Task<IServiceResponse<bool>> UpdatePreshipmentMobile(List<PreShipmentItemMobileDTO> preshipment)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var flag = await _portalService.UpdatePreShipmentMobileDetails(preshipment);

                return new ServiceResponse<bool>
                {
                    Object = flag
                };
            });
        }

        [HttpGet]
        [Route("getpreshipmentindispute")]
        public async Task<IServiceResponse<List<PreShipmentMobileDTO>>> GetPreshipmentInDispute()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var shipments = await _portalService.GetDisputePreShipment();

                return new ServiceResponse<List<PreShipmentMobileDTO>>
                {
                    Object = shipments
                };
            });
        }

        [HttpGet]
        [Route("getpartnerwallettransactions")]
        public async Task<IServiceResponse<SummaryTransactionsDTO>> GetPartnerwalletTransactions()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var totalTransactions = await _portalService.GetPartnerWalletTransactions();

                return new ServiceResponse<SummaryTransactionsDTO>
                {
                    Object = totalTransactions
                };
            });
        }

        [HttpPost]
        [Route("resolvedispute")]
        public async Task<object> ResolveDispute(PreShipmentMobileDTO shipment)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var flag = await _portalService.ResolveDisputeForMobile(shipment);

                return new ServiceResponse<object>
                {
                    Object = flag
                };
            });
        }

        [HttpPost]
        [Route("multipleshipmentresolvedispute")]
        public async Task<object> ResolveDisputeForMultipleShipment(PreShipmentMobileDTO shipment)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var flag = await _portalService.ResolveDisputeForMultipleShipment(shipment);

                return new ServiceResponse<object>
                {
                    Object = flag
                };
            });
        }

        [HttpPost]
        [Route("cancelshipment/{waybillNumber}")]
        public async Task<object> CancelShipment(string waybillNumber)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var cancel = new CancelShipmentDTO
                {
                    Waybill = waybillNumber,
                    CancelReason = "None",
                };
                var flag = await _portalService.CancelShipment(cancel.Waybill);

                return new ServiceResponse<object>
                {
                    Object = flag
                };
            });
        }

        [HttpPost]
        [Route("addratings")]
        public async Task<object> Addratings(MobileRatingDTO rating)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var flag = await _portalService.AddRatings(rating);

                return new ServiceResponse<object>
                {
                    Object = flag
                };
            });
        }

        [HttpGet]
        [Route("partnermonthlytransactions")]
        public async Task<IServiceResponse<Partnerdto>> PartnerMonthlyTransactions()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var transactions = await _portalService.GetMonthlyPartnerTransactions();

                return new ServiceResponse<Partnerdto>
                {
                    Object = transactions
                };
            });
        }

        [HttpPost]
        [Route("adddeliverynumber")]
        public async Task<IServiceResponse<bool>> UpdateDeliveryNumber(MobileShipmentNumberDTO detail)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var response = await _portalService.UpdateDeliveryNumber(detail);

                return new ServiceResponse<bool>
                {
                    Object = response
                };
            });
        }

        [HttpPost]
        [Route("deleterecord")]
        public async Task<IServiceResponse<bool>> DeleteRecord(UserDTO user)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var response = await _portalService.deleterecord(user.Email);
                return new ServiceResponse<bool>
                {
                    Object = response
                };
            });
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("forgotpassword")]
        public async Task<IServiceResponse<bool>> ForgotPassword(ForgotPasswordDTO user)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _portalService.ForgotPasswordV2(user);

                return new ServiceResponse<bool>
                {
                    Code = $"{(int)HttpStatusCode.OK}",
                    Object = result
                };
            });
        }

        [HttpPost]
        [Route("verifypartnerdetails")]
        public async Task<IServiceResponse<bool>> VerifyPartnerDetails(PartnerDTO partnerDto)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var response = await _portalService.VerifyPartnerDetails(partnerDto);
                return new ServiceResponse<bool>
                {
                    Object = response
                };
            });
        }

        [HttpPost]
        [Route("saveimages")]
        public async Task<IServiceResponse<string>> LoadImage(ImageDTO images)
        {
            return await HandleApiOperationAsync(async () =>
            {

                var response = await _portalService.LoadImage(images);
                return new ServiceResponse<string>
                {
                    Object = response
                };
            });
        }

        [HttpGet]
        [Route("displayimages")]
        public async Task<IServiceResponse<List<Uri>>> DisplayImages(ImageDTO images)
        {
            return await HandleApiOperationAsync(async () =>
            {

                var response = await _portalService.DisplayImages();
                return new ServiceResponse<List<Uri>>
                {
                    Object = response
                };
            });
        }

        [HttpPost]
        [Route("getallpartnerdetails")]
        public async Task<IServiceResponse<PartnerDTO>> GetAllPartnerDetails(PartnerDTO partner)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var response = await _portalService.GetPartnerDetails(partner.Email);
                return new ServiceResponse<PartnerDTO>
                {
                    Object = response
                };
            });
        }

        [HttpPost]
        [Route("updatereceiverdetails")]
        public async Task<IServiceResponse<bool>> UpdateReceiverDetails(PreShipmentMobileDTO receiver)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var response = await _portalService.UpdateReceiverDetails(receiver);
                return new ServiceResponse<bool>
                {
                    Object = response
                };
            });
        }

        [HttpGet]
        [Route("getpreshipmentmobiledetailsfromdeliverynumber/{deliverynumber}")]
        public async Task<IServiceResponse<PreShipmentSummaryDTO>> GetPreshipmentmobiledetailsfromdeliverynumber(string deliverynumber)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var preshipment = await _portalService.GetShipmentDetailsFromDeliveryNumber(deliverynumber);
                return new ServiceResponse<PreShipmentSummaryDTO>
                {
                    Object = preshipment
                };
            });
        }

        [HttpPost]
        [Route("approveshipment")]
        public async Task<IServiceResponse<bool>> Approveshipment(ApproveShipmentDTO detail)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _portalService.ApproveShipment(detail);
                return new ServiceResponse<bool>
                {
                    Object = result
                };
            });
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("getcountries")]
        public async Task<IServiceResponse<List<NewCountryDTO>>> getcountries()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var countries = await _portalService.GetUpdatedCountries();
                return new ServiceResponse<List<NewCountryDTO>>
                {
                    Object = countries.ToList()
                };
            });
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("getallstations")]
        public async Task<IServiceResponse<Dictionary<string, List<StationDTO>>>> getstations()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var stations = await _portalService.GetAllStations();
                return new ServiceResponse<Dictionary<string, List<StationDTO>>>
                {
                    Object = stations
                };
            });
        }

        [HttpPost]
        [Route("gethaulagepriceformobile")]
        public async Task<IServiceResponse<MobilePriceDTO>> gethaulageprice(HaulagePriceDTO haulage)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var haulagePrice = await _portalService.GetHaulagePrice(haulage);
                return new ServiceResponse<MobilePriceDTO>
                {
                    Object = haulagePrice
                };
            });
        }

        [HttpPost]
        [Route("updatevehicleprofile")]
        public async Task<IServiceResponse<bool>> updatevehicleprofile(UserDTO user)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _portalService.UpdateVehicleProfile(user);
                return new ServiceResponse<bool>
                {
                    Object = result
                };
            });
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("getactivelgas")]
        public async Task<IServiceResponse<IEnumerable<LGADTO>>> GetActiveLGAs()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var lga = await _portalService.GetActiveLGAs();
                return new ServiceResponse<IEnumerable<LGADTO>>
                {
                    Object = lga

                };
            });
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("activehomedeliverylocations")]
        public async Task<IServiceResponse<IEnumerable<LGADTO>>> GetActiveHomeDeliveryLocations()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var locations = await _portalService.GetActiveHomeDeliveryLocations();
                return new ServiceResponse<IEnumerable<LGADTO>>
                {
                    Object = locations
                };
            });
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("websiteData")]
        public async Task<IServiceResponse<AdminReportDTO>> GetWebsiteData()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var data = await _portalService.WebsiteData();
                return new ServiceResponse<AdminReportDTO>
                {
                    Object = data
                };
            });
        }

        [HttpPost]
        [Route("addmobilepickuprequestfortimedoutrequests")]
        public async Task<IServiceResponse<bool>> AddPickupRequestForTimedOutRequest([FromBody] MobilePickUpRequestsDTO PickupRequest)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var response = new ServiceResponse<bool>();
                var request = Request;
                var headers = request.Headers;
                //headers.Add("Content-Type", "application/json");
                if (headers.Contains("api_key"))
                {
                    var key = await _portalService.Decrypt();
                    string token = headers.GetValues("api_key").FirstOrDefault();
                    if (token == key)
                    {
                        var shipmentItem = await _portalService.AddMobilePickupRequest(PickupRequest);
                        response.Object = true;
                    }
                }
                return response;
            });
        }

        [HttpPost]
        [Route("cancelshipmentwithnocharge")]
        public async Task<object> CancelShipmentWithNoCharge(CancelShipmentDTO shipment)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var flag = await _portalService.CancelShipmentWithNoCharge(shipment);

                return new ServiceResponse<object>
                {
                    Object = flag
                };
            });
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("getStations")]
        public async Task<IServiceResponse<List<GiglgoStationDTO>>> GetGostations()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var stations = await _portalService.GetGoStations();

                return new ServiceResponse<List<GiglgoStationDTO>>
                {
                    Object = stations
                };
            });
        }

        [HttpGet]
        [Route("getdeliverynumber/{deliverynumber}")]
        public async Task<IServiceResponse<List<DeliveryNumberDTO>>> GetAllDeliveryNumbers(int deliverynumber)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var preShipment = await _portalService.GetDeliveryNumbers(deliverynumber);
                return new ServiceResponse<List<DeliveryNumberDTO>>
                {
                    Object = preShipment
                };
            });
        }

        [HttpPost]
        [Route("updateGoShipmentStatus")]
        public async Task<IServiceResponse<bool>> UpdateGIGGoShipmentStatus(MobilePickUpRequestsDTO mobilePickUpRequestsDTO)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var status = await _portalService.UpdateGIGGoShipmentStaus(mobilePickUpRequestsDTO);

                return new ServiceResponse<bool>
                {
                    Object = status
                };
            });
        }

        [HttpPost]
        [Route("createdropoff")]
        public async Task<IServiceResponse<bool>> CreateOrUpdateDropOff(PreShipmentDTO preShipmentDTO)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var preshipMentMobile = await _portalService.CreateOrUpdateDropOff(preShipmentDTO);

                return new ServiceResponse<bool>
                {
                    Object = preshipMentMobile
                };
            });
        }

        [HttpPost]
        [Route("updatepickupmanifeststatus")]
        public async Task<IServiceResponse<bool>> UpdatePickupManifestStatus(ManifestStatusDTO manifestStatusDTO)
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _portalService.UpdatePickupManifestStatus(manifestStatusDTO);

                return new ServiceResponse<bool>
                {
                    Object = true
                };
            });
        }

        [HttpGet]
        [Route("waybillsinpickupmanifest/{manifest}")]
        public async Task<IServiceResponse<List<PickupManifestWaybillMappingDTO>>> GetWaybillsInPickupManifest(string manifest)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var waybillNumbersIPickupManifest = await _portalService.GetWaybillsInPickupManifest(manifest);

                return new ServiceResponse<List<PickupManifestWaybillMappingDTO>>
                {
                    Object = waybillNumbersIPickupManifest
                };
            });
        }

        [HttpPost]
        [Route("dropoffs")]
        public async Task<IServiceResponse<List<PreShipmentDTO>>> GetDropOffsOfUser(ShipmentCollectionFilterCriteria filterCriteria)
        {
            return await HandleApiOperationAsync(async () =>
            {
                //set default values if payload is null
                if (filterCriteria == null)
                {
                    filterCriteria = new ShipmentCollectionFilterCriteria
                    {
                        StartDate = null,
                        EndDate = null
                    };
                }

                var dropoffs = await _portalService.GetDropOffsForUser(filterCriteria);

                return new ServiceResponse<List<PreShipmentDTO>>
                {
                    Object = dropoffs
                };
            });
        }

        [HttpGet]
        [Route("dropoffs/{tempcode}")]
        public async Task<IServiceResponse<PreShipmentDTO>> GetDropOffDetail(string tempCode)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var preshipment = await _portalService.GetDropOffDetail(tempCode);
                return new ServiceResponse<PreShipmentDTO>
                {
                    Object = preshipment
                };
            });
        }

        [HttpGet]
        [Route("giggopresentdayshipments")]
        public async Task<IServiceResponse<List<LocationDTO>>> GetPresentDayShipmentLocations()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var preshipment = await _portalService.GetPresentDayShipmentLocations();
                return new ServiceResponse<List<LocationDTO>>
                {
                    Object = preshipment
                };
            });
        }

        [HttpPost]
        [Route("dropoffprice")]
        public async Task<IServiceResponse<MobilePriceDTO>> GetPriceForDropOff(PreShipmentMobileDTO PreshipmentMobile)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var Price = await _portalService.GetPriceForDropOff(PreshipmentMobile);

                return new ServiceResponse<MobilePriceDTO>
                {
                    Object = Price,
                };
            });
        }

        [HttpGet]
        [Route("scanstatus")]
        public async Task<IServiceResponse<IEnumerable<ScanStatusDTO>>> GetScanStatus()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var scanStatus = await _portalService.GetScanStatus();

                return new ServiceResponse<IEnumerable<ScanStatusDTO>>
                {
                    Object = scanStatus
                };
            });
        }

        [HttpPost]
        [Route("scanmultiple")]
        public async Task<IServiceResponse<bool>> ScanMultipleShipment(List<ScanDTO> scanList)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _portalService.ScanMultipleShipment(scanList);

                return new ServiceResponse<bool>
                {
                    Object = result
                };
            });
        }

        [HttpGet]
        [Route("waybillsinmanifestfordispatch")]
        public async Task<IServiceResponse<List<ManifestWaybillMappingDTO>>> GetWaybillsInManifestForDispatch()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var groupWaybillNumbersInManifest = await _portalService.GetWaybillsInManifestForDispatch();

                return new ServiceResponse<List<ManifestWaybillMappingDTO>>
                {
                    Object = groupWaybillNumbersInManifest
                };
            });
        }

        [HttpPut]
        [Route("collected")]
        public async Task<IServiceResponse<bool>> ReleaseShipmentForCollection(ShipmentCollectionDTO shipmentCollection)
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _portalService.ReleaseShipmentForCollectionOnScanner(shipmentCollection);
                return new ServiceResponse<bool>
                {
                    Object = true
                };
            });
        }

        [HttpGet]
        [Route("getlogvisit")]
        public async Task<IServiceResponse<List<LogVisitReasonDTO>>> GetLogVisitReasons()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var logVisitReasons = await _portalService.GetLogVisitReasons();

                return new ServiceResponse<List<LogVisitReasonDTO>>
                {
                    Object = logVisitReasons
                };
            });
        }

        [HttpPost]
        [Route("logwaybillvisit")]
        public async Task<IServiceResponse<object>> AddManifest(ManifestVisitMonitoringDTO manifestVisitMonitoringDTO)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var manifest = await _portalService.AddManifestVisitMonitoring(manifestVisitMonitoringDTO);
                return new ServiceResponse<object>
                {
                    Object = manifest
                };
            });
        }

        [HttpGet]
        [Route("outstandingpayments")]
        public async Task<IServiceResponse<List<OutstandingPaymentsDTO>>> GetOutstandingPayments()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var outstandingPayments = await _portalService.GetOutstandingPayments();
                return new ServiceResponse<List<OutstandingPaymentsDTO>>
                {
                    Object = outstandingPayments
                };
            });
        }

        [HttpGet]
        [Route("paymentforshipment/{waybill}")]
        public async Task<IServiceResponse<bool>> PayForAgilityShipmentFromApp(string waybill)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _portalService.PayForShipment(waybill);
                return new ServiceResponse<bool>
                {
                    Object = result
                };
            });
        }

        [HttpDelete]
        [Route("dropoffs/{tempcode}")]
        public async Task<IServiceResponse<bool>> DeleteDropoff(string tempCode)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var preshipment = await _portalService.DeleteDropOff(tempCode);
                return new ServiceResponse<bool>
                {
                    Object = preshipment
                };
            });
        }

        [HttpPost]
        [Route("getwallettransactions")]
        public async Task<IServiceResponse<List<WalletTransactionDTO>>> GetWalletTransactions(ShipmentAndPreShipmentParamDTO shipmentAndPreShipmentParamDTO)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var Transactionhistory = await _portalService.GetWalletTransactionsForMobilePaginated(shipmentAndPreShipmentParamDTO);
                return new ServiceResponse<List<WalletTransactionDTO>>
                {
                    Object = Transactionhistory
                };
            });
        }

        [HttpPost]
        [Route("getshipments")]
        public async Task<IServiceResponse<List<PreShipmentMobileDTO>>> GetShipments(ShipmentAndPreShipmentParamDTO shipmentAndPreShipmentParamDTO)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var shipmenthistory = await _portalService.GetPreShipmentsAndShipmentsPaginated(shipmentAndPreShipmentParamDTO);
                return new ServiceResponse<List<PreShipmentMobileDTO>>
                {
                    Object = shipmenthistory
                };
            });
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("getactivecountries")]
        public async Task<IServiceResponse<List<NewCountryDTO>>> getactivecountries()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var countries = await _portalService.GetActiveCountries();
                return new ServiceResponse<List<NewCountryDTO>>
                {
                    Object = countries.ToList()
                };
            });
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("getstationsbycountry/{countryId}")]
        public async Task<IServiceResponse<List<StationDTO>>> GetStationsByCountry(int countryId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var stations = await _portalService.GetStationsByCountry(countryId);
                return new ServiceResponse<List<StationDTO>>
                {
                    Object = stations.ToList()
                };
            });
        }

        [HttpPut]
        [Route("profileinternationaluser")]
        public async Task<IServiceResponse<bool>> ProfileInternationalUser(IntertnationalUserProfilerDTO intlUserProfiler)
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _portalService.ProfileInternationalUser(intlUserProfiler);
                return new ServiceResponse<bool>
                {
                    Object = true
                };
            });
        }

        [HttpGet]
        [Route("servicecentresbystation/{stationId}")]
        public async Task<IServiceResponse<List<ServiceCentreDTO>>> GetServiceCentresByStation(int stationId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var centres = await _portalService.GetServiceCentresByStation(stationId);
                return new ServiceResponse<List<ServiceCentreDTO>>
                {
                    Object = centres
                };
            });
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("identificationtypes")]
        public IHttpActionResult GetIdentificationTypes()
        {
            var types = EnumExtensions.GetValues<IdentificationType>();
            types.RemoveAt(3);
            return Ok(types);
        }

        [HttpGet]
        [Route("servicecentresbycountry/{countryId}")]
        public async Task<IServiceResponse<List<ServiceCentreDTO>>> GetServiceCentresBySingleCountry(int countryId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var centres = await _portalService.GetActiveServiceCentres();
                return new ServiceResponse<List<ServiceCentreDTO>>
                {
                    Object = centres
                };
            });
        }

        [HttpGet]
        [Route("storesbycountry/{countryId}")]
        public async Task<IServiceResponse<List<StoreDTO>>> GetStoresByCountry(int countryId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var stores = await _portalService.GetStoresByCountry(countryId);
                return new ServiceResponse<List<StoreDTO>>
                {
                    Object = stores
                };
            });
        }

        [HttpPost]
        [Route("createnotification")]
        public async Task<IServiceResponse<object>> CreateNotification(NotificationDTO notificationDto)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var notification = await _portalService.CreateNotification(notificationDto);

                return new ServiceResponse<object>
                {
                    Object = notification
                };
            });
        }

        [HttpGet]
        [Route("getnotifications/{isRead?}")]
        public async Task<IServiceResponse<IEnumerable<NotificationDTO>>> GetNotifications(bool? isRead = null)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var locations = await _portalService.GetNotifications(isRead);
                return new ServiceResponse<IEnumerable<NotificationDTO>>
                {
                    Object = locations
                };
            });
        }

        [HttpPut]
        [Route("updatenotification/{notificationId:int}")]
        public async Task<IServiceResponse<bool>> UpdateNotificationAsRead(int notificationId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _portalService.UpdateNotificationAsRead(notificationId);

                return new ServiceResponse<bool>
                {
                    Object = true
                };
            });
        }

        [HttpGet]
        [Route("intlshipmentsmessage")]
        public async Task<IServiceResponse<MessageDTO>> GetIntlMessageForApp(int countryId = 0)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var message = await _portalService.GetIntlMessageForApp(countryId);

                return new ServiceResponse<MessageDTO>
                {
                    Object = message
                };
            });
        }

        [HttpPost]
        [Route("intlshipmentrequests")]
        public async Task<IServiceResponse<List<IntlShipmentRequestDTO>>> GetIntlShipmentRequestsForUser(ShipmentCollectionFilterCriteria filterCriteria)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var requests = await _portalService.GetIntlShipmentRequestsForUser(filterCriteria);

                return new ServiceResponse<List<IntlShipmentRequestDTO>>
                {
                    Object = requests
                };
            });
        }


        [HttpGet]
        [Route("consolidatedintlshipments")]
        public async Task<IServiceResponse<List<IntlShipmentRequestDTO>>> GetConsolidateIntlShipments(int countryID = 0)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _magayaService.GetConsolidatedShipmentRequestForUser(countryID);

                return new ServiceResponse<List<IntlShipmentRequestDTO>>
                {
                    Object = result
                };
            });
        }
        [HttpGet]
        [Route("intlshippingcountries")]
        public async Task<IServiceResponse<IEnumerable<CountryDTO>>> GetIntlShippingCountries()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _portalService.GetIntlShipingCountries();

                return new ServiceResponse<IEnumerable<CountryDTO>>
                {
                    Object = result
                };
            });
        }

        [Route("useraddresses")]
        public async Task<IServiceResponse<List<AddressDTO>>> GetTopFiveUserAddresses(bool isIntl = false)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _portalService.GetTopFiveUserAddresses(isIntl);

                return new ServiceResponse<List<AddressDTO>>
                {
                    Object = result
                };
            });
        }

        [HttpPost]
        [Route("upgradetoecommerce")]
        public async Task<IServiceResponse<CompanyDTO>> UpgradeToEcommerce(UpgradeToEcommerce newCompanyDTO)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var company = await _portalService.UpgradeToEcommerce(newCompanyDTO);
                return new ServiceResponse<CompanyDTO>
                {
                    Object = company
                };
            });
        }

        [HttpPost]
        [Route("updateuseractivecountry")]
        public async Task<IServiceResponse<UserActiveCountryDTO>> UpdateUserActiveCountry(UpdateUserActiveCountryDTO userActiveCountry)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var item = await _portalService.UpdateUserActiveCountry(userActiveCountry);
                return new ServiceResponse<UserActiveCountryDTO>
                {
                    Object = item
                };
            });
        }

        [HttpPost]
        [Route("internationalshipmentquote")]
        public async Task<IServiceResponse<List<TotalNetResult>>> InternationalshipmentQuote(InternationalShipmentQuoteDTO quoteDTO)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var shipment = await _portalService.GetInternationalshipmentQuote(quoteDTO);
                return new ServiceResponse<List<TotalNetResult>>
                {
                    Object = shipment
                };
            });
        }

        [HttpPost]
        [Route("internationalshipmentrate")]
        public async Task<IServiceResponse<List<TotalNetResult>>> InternationalshipmentRate(RateInternationalShipmentDTO rateDTO)
        {
            if (String.IsNullOrEmpty(rateDTO.ReceiverState))
            {
                rateDTO.ReceiverState = rateDTO.ReceiverCountry;
            }
            return await HandleApiOperationAsync(async () =>
            {
                var shipment = await _portalService.GetInternationalshipmentRate(rateDTO);
                return new ServiceResponse<List<TotalNetResult>>
                {
                    Object = shipment
                };
            });
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("getintlquickqoute")]
        public async Task<IServiceResponse<QuickQuotePriceDTO>> GetIntlQuickQuote(QuickQuotePriceDTO quickQuotePriceDTO)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var item = await _portalService.GetIntlQuickQuote(quickQuotePriceDTO);
                return new ServiceResponse<QuickQuotePriceDTO>
                {
                    Object = item
                };
            });
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("getcategoriesbycountry/{destcountryId}/{deptcountryId}")]
        public async Task<IServiceResponse<IEnumerable<PriceCategoryDTO>>> GetPriceCategoriesByCountry(int destcountryId, int deptcountryId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var item = await _portalService.GetPriceCategoriesBothCountries(destcountryId, deptcountryId);
                return new ServiceResponse<IEnumerable<PriceCategoryDTO>>
                {
                    Object = item
                };
            });
        }





        [HttpPost]
        [Route("updatecompanyname")]
        public async Task<IServiceResponse<UpdateCompanyNameDTO>> UpdateCompanyName(UpdateCompanyNameDTO updateCompanyNameDTO)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var item = await _portalService.UpdateCompanyName(updateCompanyNameDTO);
                return new ServiceResponse<UpdateCompanyNameDTO>
                {
                    Object = item
                };
            });
        }

        [HttpGet]
        [Route("getagilityandgiggoinvoce/{waybillNumber}")]
        public async Task<IServiceResponse<object>> GetGIGGOAndAgilityShipmentInvoice(string waybillNumber)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var details = await _portalService.GetGIGGOAndAgilityShipmentInvoice(waybillNumber);

                return new ServiceResponse<object>
                {
                    Object = details
                };
            });
        }

        [HttpGet]
        [Route("getwebsitecountrydata")]
        public async Task<IServiceResponse<List<WebsiteCountryDTO>>> GetCoreForWebsite()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var item = await _portalService.GetCoreForWebsite();
                return new ServiceResponse<List<WebsiteCountryDTO>>
                {
                    Object = item
                };
            });
        }

        [HttpPost]
        [Route("optincustomerwhatsappnumber")]
        public async Task<IServiceResponse<bool>> OptinCustomerWhatsappNumber(WhatsappNumberDTO whatsappNumber)
        {
            return await HandleApiOperationAsync(async () =>
            {
                await _portalService.OptInCustomerWhatsappNumber(whatsappNumber);
                return new ServiceResponse<bool>
                {
                    Object = true
                };
            });
        }

        [HttpPost]
        [Route("assignshipment")]
        public async Task<IServiceResponse<AssignedShipmentDTO>> AssignShipmentToPartner(ShipmentAssignmentDTO partnerInfo)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _portalService.AssignShipmentToPartner(partnerInfo);
                return new ServiceResponse<AssignedShipmentDTO>
                {
                    Object = result
                };
            });
        }



        [HttpPost]
        [Route("cancelshipmentwithreason")]
        public async Task<object> CancelShipmentWithReason(CancelShipmentDTO cancelPreShipmentMobile)
        {
            return await HandleApiOperationAsync(async () =>
            {
               
                var flag = await _portalService.CancelShipmentWithReason(cancelPreShipmentMobile);

                return new ServiceResponse<object>
                {
                    Object = flag
                };
            });
        }


        [HttpPost]
        [Route("cancelshipmentwithnochargeandreason")]
        public async Task<object> CancelShipmentWithNoChargeAndReason(CancelShipmentDTO shipment)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var flag = await _portalService.CancelShipmentWithNoChargeAndReason(shipment);

                return new ServiceResponse<object>
                {
                    Object = flag
                };
            });
        }

        [HttpGet]
        [Route("getcorporatecustomer/{customerCode}")]
        public async Task<IServiceResponse<CustomerDTO>> GetCorporateCustomer(string customerCode)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _portalService.GetCorporateCustomer(customerCode);
                return new ServiceResponse<CustomerDTO>
                {
                    Object = result
                };
            });
        }


        [HttpPost]
        [Route("getshipmentprice")]
        public async Task<IServiceResponse<NewPricingDTO>> GetShipmentPrice(CorporateShipmentDTO newShipmentDTO)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var price = await _portalService.GetGrandPriceForShipment(newShipmentDTO);
                return new ServiceResponse<NewPricingDTO>
                {
                    Object = price
                };
            });
        }

        [HttpPost]
        [Route("corporateshipment")]
        public async Task<IServiceResponse<ShipmentDTO>> CreateCorporateShipment(CorporateShipmentDTO corporateShipmentDTO)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _portalService.CreateCorporateShipment(corporateShipmentDTO);
                return new ServiceResponse<ShipmentDTO>
                {
                    Object = result
                };
            });
        }

        [HttpPut]
        [Route("gigxuserdetails")]
        public async Task<IServiceResponse<object>> SaveGIGXUserDetails(GIGXUserDetailDTO userDetails)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _portalService.SaveGIGXUserDetails(userDetails);
                return new ServiceResponse<object>
                {
                    Object = result
                };
            });
        }

        [HttpGet]
        [Route("getgigxuserwalletdetails")]
        public async Task<IServiceResponse<GIGXUserDetailDTO>> GetGIGXUserWalletDetails()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _portalService.GetGIGXUserWalletDetails();
                return new ServiceResponse<GIGXUserDetailDTO>
                {
                    Object = result
                };
            });
        }

        [HttpGet, Route("allcountry")]
        public async Task<IServiceResponse<IEnumerable<CountryDTO>>> GetCountries()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var country = await _portalService.GetCountries();
                return new ServiceResponse<IEnumerable<CountryDTO>>
                {
                    Object = country
                };
            });
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("addtransferdetails")]
        public async Task<IServiceResponse<object>> AddCellulantTransferDetails(TransferDetailsDTO TransferDetailsDTO)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var response = new ServiceResponse<object>();
                var request = Request;
                var headers = request.Headers;
                var result = new object();

                if (headers.Contains("api_key"))
                {
                    //var encrytedkey = await _portalService.EncryptCellulantKey();
                    var key = await _portalService.GetCellulantKey();
                    string apiKey = headers.GetValues("api_key").FirstOrDefault();
                    string token = await _portalService.Decrypt(apiKey);
                    if (token == key)
                    {
                        result = await _portalService.AddCellulantTransferDetails(TransferDetailsDTO);
                        response.Object = result;
                    }
                    else
                    {
                        throw new GenericException("Invalid key", $"{(int)HttpStatusCode.Unauthorized}");
                    }
                }
                else
                {
                    throw new GenericException("Unauthorized", $"{(int)HttpStatusCode.Unauthorized}");
                }
                return response;
            });
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("getpricequote")]
        public async Task<IServiceResponse<MobilePriceDTO>> GetPriceQoute(PreShipmentMobileDTO PreshipmentMobile)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var Price = await _portalService.GetPriceQoute(PreshipmentMobile);
                return new ServiceResponse<MobilePriceDTO>
                {
                    Object = Price,
                };
            });
        }

        [HttpDelete]
        [Route("deleteinboundshipment/{requestNo}")]
        public async Task<IServiceResponse<bool>> DeleteInboundShipment(string requestNo)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var inboundShepment = await _portalService.DeleteInboundShipment(requestNo);
                return new ServiceResponse<bool>
                {
                    Object = inboundShepment
                };
            });
        }

        [HttpGet]
        [AllowAnonymous]
        [Route("getwalletbalance/{customerCode}")]
        public async Task<IServiceResponse<decimal>> GetWalletBalance(string customerCode)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var wallet = await _portalService.GetWalletBalance(customerCode);
                return new ServiceResponse<decimal>
                {
                    Object = wallet.Balance
                };
            });
        }

       
        [HttpPost]
        [Route("createCoupon")]
        public async Task<IServiceResponse<List<string>>> CreateCoupon(CouponManagementDTO couponDto)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var couponCodes = await _portalService.CreateCoupon(couponDto);
                return new ServiceResponse<List<string>>
                {
                    Object = couponCodes
                };
            });
        }

        [HttpGet]
        [Route("getcomputecouponamount/{couponCode}/{amount}")]
        public async Task<IServiceResponse<decimal>> GetComputeCouponAmount(string couponCode, decimal amount)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var computedAmount = await _portalService.GetComputeCouponAmount(couponCode, amount);
                return new ServiceResponse<decimal>
                {
                    Object = computedAmount
                };
            });
        }

        [HttpGet]
        [Route("checkforuserpin")]
        public async Task<IServiceResponse<bool>> CheckIfUserHasPin()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _portalService.CheckIfUserHasPin();
                return new ServiceResponse<bool>
                {
                    Object = result
                };
            });
        }

        [HttpPost]
        [Route("addgiguserpin")]
        public async Task<IServiceResponse<bool>> SaveGIGUserPin(GIGXUserDetailDTO userDetails)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _portalService.SaveGIGUserPin(userDetails);
                return new ServiceResponse<bool>
                {
                    Object = result
                };
            });
        }

        [HttpPost]
        [Route("verifyuserpin")]
        public async Task<IServiceResponse<bool>> VerifyUserPin(GIGXUserDetailDTO userDetails)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _portalService.VerifyUserPin(userDetails);
                return new ServiceResponse<bool>
                {
                    Object = result
                };
            });
        }

        [HttpPut]
        [Route("chargewallet")]
        public async Task<IServiceResponse<ResponseDTO>> ChargeWallet(ChargeWalletDTO responseDTO)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _portalService.ChargeWallet(responseDTO);
                return new ServiceResponse<ResponseDTO>
                {
                    Object = result,
                };
            });
        }

        [HttpPut]
        [Route("{reference}/reversewallet")]
        public async Task<IServiceResponse<ResponseDTO>> ReverseWallet(string reference)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _portalService.ReverseWallet(reference);
                return new ServiceResponse<ResponseDTO>
                {
                    Object = result
                };
            });
        }

        [HttpPut]
        [Route("{emailorcode}/{amount}/billtransactionrefund")]
        public async Task<IServiceResponse<string>> BillTransactionRefund(string emailorcode, decimal amount)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _portalService.BillTransactionRefund(emailorcode, amount);
                return new ServiceResponse<string>
                {
                    Object = result
                };
            });
        }
        [HttpGet]
        [Route("getpaymentmethod")]
        public async Task<IServiceResponse<IEnumerable<PaymentMethodDTO>>> GetPaymentMethod()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var PaymentMethod = await _portalService.GetPaymentMethodByUserActiveCountry();
                return new ServiceResponse<IEnumerable<PaymentMethodDTO>>
                {
                    Object = PaymentMethod,
                };
            });
        }


        [HttpPost]
        [Route("sendservicesms")]
        public async Task<IServiceResponse<bool>> SendServiceSMS(ServiceSMS serviceSMS)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _portalService.SendServiceSMS(serviceSMS);
                return new ServiceResponse<bool>
                {
                    Object = result
                };
            });
        }

        [HttpPut]
        [Route("changeuserpin")]
        public async Task<IServiceResponse<bool>> ChangeUserPin(GIGXUserDetailDTO userDetails)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _portalService.ChangeUserPin(userDetails);
                return new ServiceResponse<bool>
                {
                    Object = result
                };
            });
        }

        [HttpPut]
        [Route("resetuserpin")]
        public async Task<IServiceResponse<bool>> ResetUserPin(GIGXUserDetailDTO userDetails)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _portalService.ResetUserPin(userDetails);
                return new ServiceResponse<bool>
                {
                    Object = result
                };
            });
        }

        [HttpPut]
        [Route("validatebillspayment")]
        public async Task<IServiceResponse<string>> ValidateBillsPaymentRefund(ValidateBillTransactionDTO billTransaction)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _portalService.ValidateBillsPaymentRefund(billTransaction);
                return new ServiceResponse<string>
                {
                    Object = result
                };
            });
        }

        [HttpPost]
        [Route("checkoutencryption")]
        public async Task<IServiceResponse<CellulantResponseDTO>> CheckOutEncryption(CellulantPayloadDTO payload)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _portalService.CheckoutEncryption(payload);

                return new ServiceResponse<CellulantResponseDTO>
                {
                    Object = result
                };
            });
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("verifypayment")]
        public async Task<CellulantPaymentResponse> VerifyAndValidatePayment(CellulantWebhookDTO webhook)
        {
            string contentType = String.Empty;
            var request = Request.GetOwinContext().Request; 

            foreach (var key in request.Headers.Keys)
            {
                contentType += key + "=" + request.Headers[key] + "; ";
            }
            
            var remoteIp = request.RemoteIpAddress;
            var remotePort = request.RemotePort;
            var localIp = request.LocalIpAddress;
            var localPort = request.LocalPort;
            var ipport = $"Remote IP Address: {remoteIp}, Remote Port: {remotePort}, Local IP Address: {localIp}, Local Port: {localPort}";
            var entry = new LogEntryDTO
            {
                DateTime = DateTime.Now.ToString(),
                Logger = contentType,
                Level = $"{(int)HttpStatusCode.OK}",
                MachineName = "CELLULANT",
                CallSite = ipport
            };

            await _portalService.LogContentType(entry);
            return await _portalService.VerifyAndValidatePayment(webhook);
        }

        [HttpPost]
        [Route("currencyequivalent")]
        public async Task<IServiceResponse<OutstandingPaymentsDTO>> GetEquivalentAmountOfActiveCurrency(CurrencyEquivalentDTO currencyEquivalent)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var equivalent = await _portalService.GetEquivalentAmountOfActiveCurrency(currencyEquivalent);
                return new ServiceResponse<OutstandingPaymentsDTO>
                {
                    Object = equivalent
                };
            });
        }

        [HttpGet]
        [Route("getpaymentmethod/{countryid:int}")]
        public async Task<IServiceResponse<IEnumerable<PaymentMethodDTO>>> GetPaymentMethod(int countryid)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var PaymentMethod = await _portalService.GetPaymentMethodByUserActiveCountry(countryid);
                return new ServiceResponse<IEnumerable<PaymentMethodDTO>>
                {
                    Object = PaymentMethod,
                };
            });
        }

        [HttpGet]
        [Route("getgigxuserwalletdetailsnew")]
        public async Task<IServiceResponse<GIGXUserDetailDTO>> GetGIGXUserWalletDetailsNew()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _portalService.GetGIGXUserWalletDetailsNew();
                return new ServiceResponse<GIGXUserDetailDTO>
                {
                    Object = result
                };
            });
        }

        [HttpGet]
        [Route("getgigxuserwalletdetailsbycode/{code}")]
        public async Task<IServiceResponse<GIGXUserDetailDTO>> GetGIGXUserDetailByCodeNew(string code)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _portalService.GetGIGXUserDetailByCodeNew(code);
                return new ServiceResponse<GIGXUserDetailDTO>
                {
                    Object = result
                };
            });
        }

        [HttpPost]
        [Route("createstellasaccount")]
        public async Task<IServiceResponse<StellasResponseDTO>> AddCODWallet(CreateStellaAccountDTO codWalletDTO)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var response = await _portalService.AddCODWallet(codWalletDTO);
                return new ServiceResponse<StellasResponseDTO>
                {
                    Object = response
                };
            });
        }

        [HttpGet]
        [Route("getcustomerstellasaccountbal/{code}")]
        public async Task<IServiceResponse<StellasResponseDTO>> GetStellasAccountBal(string code)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _portalService.GetStellasAccountBal(code);
                return new ServiceResponse<StellasResponseDTO>
                {
                    Object = result
                };
            });
        }


        [HttpPost]
        [Route("allcodshipment")]
        public async Task<IServiceResponse<AllCODShipmentDTO>> GetAllCODShipments(PaginationDTO dto)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var res = await _portalService.GetAllCODShipments(dto);
                return new ServiceResponse<AllCODShipmentDTO>
                {
                    Object = res,
                };
            });
        }

        [HttpPost]
        [Route("generatecheckouturl")]
        public async Task<IServiceResponse<string>> GenerateCheckoutUrlForKorapay(KoarapayInitializeCharge payload)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _portalService.GenerateCheckoutUrlForKorapay(payload);
                return new ServiceResponse<string>
                {
                    Object = result
                };
            });
        }

        [HttpPost]
        [Route("celullanttransfer")]
        public async Task<IServiceResponse<CellulantTransferResponsePayload>> CelullantTransfer(CellulantTransferDTO payload)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _portalService.CelullantTransfer(payload);
                return new ServiceResponse<CellulantTransferResponsePayload>
                {
                    Object = result
                };
            });
        }

        [HttpGet]
        [Route("stellasbanks")]
        public async Task<IServiceResponse<StellasResponseDTO>> GetStellasBank()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _portalService.GetStellasBanks();
                return new ServiceResponse<StellasResponseDTO>
                {
                    Object = result
                };
            });
        }

        [HttpPost]
        [Route("stellaswithdrawal")]
        public async Task<IServiceResponse<StellasResponseDTO>> StellasWithdrawal(StellasTransferDTO payload)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _portalService.StellasTransfer(payload);
                return new ServiceResponse<StellasResponseDTO>
                {
                    Object = result
                };
            });
        }

        [HttpPost]
        [Route("stellasvalidatebankname")]
        public async Task<IServiceResponse<StellasResponseDTO>> StellasValidateBankName(ValidateBankNameDTO payload)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _portalService.StellasValidateBankName(payload);
                return new ServiceResponse<StellasResponseDTO>
                {
                    Object = result
                };
            });
        }

        [HttpGet]
        [Route("checkifuserhascodwallet/{customercode}")]
        public async Task<IServiceResponse<bool>> CheckIfUserHasCODWallet(string customercode)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _portalService.CheckIfUserHasCODWallet(customercode);
                return new ServiceResponse<bool>
                {
                    Object = result
                };
            });
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("validatebvnnumber")]
        public async Task<IServiceResponse<StellasResponseDTO>> ValidateBVNNumber(ValidateCustomerBVN payload)
        {
            return await HandleApiOperationAsync(async () =>
            {
                 var result = await _portalService.ValidateBVNNumber(payload);
                return new ServiceResponse<StellasResponseDTO>
                {
                    Object = result
                };
            });
        }

        [HttpGet]
        [Route("inbundcategory/{countryId}")]
        public async Task<IServiceResponse<IEnumerable<InboundShipmentCategoryDTO>>> GetInboundCategory(int countryId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _portalService.GetInboundCategory(countryId);
                return new ServiceResponse<IEnumerable<InboundShipmentCategoryDTO>>
                {
                    Object = result
                };
            });
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("codtransfer")]
        public async Task<IServiceResponse<GIGGOCODTransferResponseDTO>> CODTransfer(GIGGOCODTransferDTO payload)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _portalService.CODTransfer(payload);
                return new ServiceResponse<GIGGOCODTransferResponseDTO>
                {
                    Object = result
                };
            });
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("getcodtransfer/{waybill}")]
        public async Task<IServiceResponse<GIGGOCODTransferResponseDTO>> GetCODTransfer(string waybill)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _portalService.GetCodTransfer(waybill);
                return new ServiceResponse<GIGGOCODTransferResponseDTO>
                {
                    Object = result
                };
            });
        }

        [HttpPut]
        [Route("deleteaccount")]
        public async Task<IServiceResponse<bool>> DeleteCustomerAccount(DeleteAccountDTO payload)
        {
            return await HandleApiOperationAsync(async () =>
            {
               await _portalService.DeleteCustomerAccount(payload);
                return new ServiceResponse<bool>
                {
                    Object = true
                };
            });
        }

        [HttpPost]
        [Route("codmobilereport")]
        public async Task<IServiceResponse<AllCODShipmentDTO>> GetAllCODShipmentsAgilityReport(PaginationDTO payload)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var cods = await _portalService.GetAllCODShipmentsAgilityReport(payload);
                return new ServiceResponse<AllCODShipmentDTO>
                {
                    Object = cods
                };
            });
        }

        [HttpGet]
        [Route("getstellalogindetails/{customercode}")]
        public async Task<IServiceResponse<LoginDetailsDTO>> GetStellasLoginDetails(string customercode)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _portalService.GetStellasAccountLoginDetails(customercode);
                return new ServiceResponse<LoginDetailsDTO>
                {
                    Object = result
                };
            });
        }

        [HttpPost]
        [Route("transferreversal")]
        public async Task<IServiceResponse<StellasResponseDTO>> StellasTransferReversal(StellasTransferDTO payload)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _portalService.StellasTransferReversal(payload);
                return new ServiceResponse<StellasResponseDTO>
                {
                    Object = result
                };
            });
        }

        [HttpPost]
        [Route("getcodcustomeraccountstatement")]
        public async Task<IServiceResponse<List<CODCustomerAccountStatementDto>>> GetCODCustomerAccountStatement(GetCODCustomerAccountStatementDto accountStatementDto)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _portalService.GetCODCustomerAccountStatement(accountStatementDto);

                return new ServiceResponse<List<CODCustomerAccountStatementDto>>
                {
                    Object = result
                };
            });
        }
    }
}