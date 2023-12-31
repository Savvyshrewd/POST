﻿using EfeAuthen.Models;
using POST.Core.DTO;
using POST.Core.DTO.Account;
using POST.Core.DTO.Customers;
using POST.Core.DTO.OnlinePayment;
using POST.Core.DTO.Partnership;
using POST.Core.DTO.Report;
using POST.Core.DTO.ServiceCentres;
using POST.Core.DTO.Shipments;
using POST.Core.DTO.User;
using POST.Core.DTO.Wallet;
using POST.Core.IServices;
using POST.Core.IServices.ThirdPartyAPI;
using POST.CORE.DTO.Report;
using POST.Infrastructure;
using POST.Services.Implementation;
using POST.WebApi.Filters;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;

namespace POST.WebApi.Controllers.ThirdPartyAPI
{
    [Authorize(Roles = "ThirdParty")]
    [RoutePrefix("api/thirdparty")]
    public class ThirdPartyAPIController : BaseWebApiController
    {
        private readonly IThirdPartyAPIService _thirdPartyAPIService;

        public ThirdPartyAPIController(IThirdPartyAPIService portalService) : base(nameof(ThirdPartyAPIController))
        {
            _thirdPartyAPIService = portalService;
        }

        /// <summary>
        /// This api is used to get the price for Shipment Items
        /// </summary>
        /// <param name="thirdPartyPricingDto"></param>
        /// <returns></returns>
        //[ThirdPartyActivityAuthorize(Activity = "View")]
        //[HttpPost]
        //[Route("previousprice")]
        //public async Task<IServiceResponse<decimal>> GetPrice(ThirdPartyPricingDTO thirdPartyPricingDto)
        //{
        //    return await HandleApiOperationAsync(async () =>
        //    {
        //        var price = await _thirdPartyAPIService.GetPrice2(thirdPartyPricingDto);

        //        return new ServiceResponse<decimal>
        //        {
        //            Object = price
        //        };
        //    });
        //}
        [ThirdPartyActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("price")]
        public async Task<IServiceResponse<MobilePriceDTO>> GetPrice(PreShipmentMobileDTO preshipmentMobile)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var PreshipMentMobile = await _thirdPartyAPIService.GetPrice(preshipmentMobile);
                return new ServiceResponse<MobilePriceDTO>
                {
                    Object = PreshipMentMobile
                };
            });
        }

        //Capture Shipment API
        /// <summary>
        /// This api is used to register a shipment
        /// </summary>
        /// <param name="thirdPartyPreShipmentDTO"></param>
        /// <returns></returns>
        //[ThirdPartyActivityAuthorize(Activity = "Create")]
        //[HttpPost]
        //[Route("previouscaptureshipment")]
        //public async Task<IServiceResponse<bool>> AddPreShipment(ThirdPartyPreShipmentDTO thirdPartyPreShipmentDTO)
        //{
        //    return await HandleApiOperationAsync(async () =>
        //    {
        //        var shipment = await _thirdPartyAPIService.AddPreShipment(thirdPartyPreShipmentDTO);
        //        return new ServiceResponse<bool>
        //        {
        //            Object = true
        //        };
        //    });
        //}

        [ThirdPartyActivityAuthorize(Activity = "Create")]
        [HttpPost]
        [Route("captureshipment")]
        public async Task<IServiceResponse<object>> CreateShipment(CreatePreShipmentMobileDTO preshipmentMobile)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _thirdPartyAPIService.CreatePreShipment(preshipmentMobile);
                return new ServiceResponse<object>
                {
                    Object = result
                };
            });
        }

        /// <summary>
        /// This api is used to track all shipments
        /// </summary>
        /// <param name="waybillNumber"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpGet]
        [Route("TrackShipmentPublic/{waybillNumber}")]
        public async Task<IServiceResponse<IEnumerable<ShipmentTrackingDTO>>> PublicTrackShipment(string waybillNumber)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _thirdPartyAPIService.PublicTrackShipment(waybillNumber);

                return new ServiceResponse<IEnumerable<ShipmentTrackingDTO>>
                {
                    Object = result
                };
            });
        }

        /// <summary>
        /// This api is used to get all local stations
        /// </summary>
        /// <returns></returns>
        [ThirdPartyActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("localStations")]
        public async Task<IServiceResponse<IEnumerable<StationDTO>>> GetLocalStations()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var stations = await _thirdPartyAPIService.GetLocalStations();

                return new ServiceResponse<IEnumerable<StationDTO>>
                {
                    Object = stations
                };
            });
        }

        /// <summary>
        /// This api is used to get all international stations
        /// </summary>
        /// <returns></returns>
        [ThirdPartyActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("InternationalStations")]
        public async Task<IServiceResponse<IEnumerable<StationDTO>>> GetInternationalStations()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var stations = await _thirdPartyAPIService.GetInternationalStations();

                return new ServiceResponse<IEnumerable<StationDTO>>
                {
                    Object = stations
                };
            });
        }

        /// <summary>
        /// This api is used to login and acquire token for subsequent calls
        /// </summary>
        /// <description>This api is used to login and acquire token for subsequent calls</description>
        /// <param name="userLoginModel"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        [Route("login")]
        public async Task<IServiceResponse<JObject>> Login(UserloginDetailsModel userLoginModel)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var user = await _thirdPartyAPIService.CheckDetailsForLogin(userLoginModel.username);

                //trim
                if (user.Username != null)
                {
                    user.Username = user.Username.Trim();
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

                    getTokenResponse = jObject.GetValue("access_token").ToString();

                    return new ServiceResponse<JObject>
                    {
                        Object = jObject
                    };
                }
            });
        }


        /// <summary>
        /// This api is used to get shipments created by user
        /// </summary>
        /// <description>This api is used to get shipments by user</description>
        /// <returns></returns>

        [ThirdPartyActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("PickUpRequests")]
        public async Task<IServiceResponse<List<InvoiceViewDTO>>> GetShipmentTransactions(ShipmentCollectionFilterCriteria f_Criteria)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var preshipMentMobile = await _thirdPartyAPIService.GetShipmentTransactions(f_Criteria);
                return new ServiceResponse<List<InvoiceViewDTO>>
                {
                    Object = preshipMentMobile
                };
            });
        }

        /// <summary>
        /// This api is used to track shipments created by the user
        /// </summary>
        /// <param name="waybillNumber"></param>
        /// <returns></returns>
        [ThirdPartyActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("TrackAllShipment/{waybillNumber}")]
        public async Task<IServiceResponse<MobileShipmentTrackingHistoryDTO>> TrackMobileShipment(string waybillNumber)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _thirdPartyAPIService.TrackShipment(waybillNumber);

                return new ServiceResponse<MobileShipmentTrackingHistoryDTO>
                {
                    Object = result
                };
            });
        }

        /// <summary>
        /// This api is used to get the active lgas for GiG Go shipments 
        /// </summary>
        /// <returns></returns>
        [ThirdPartyActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("getactivelgas")]
        public async Task<IServiceResponse<IEnumerable<LGADTO>>> GetActiveLGAs()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _thirdPartyAPIService.GetActiveLGAs();

                return new ServiceResponse<IEnumerable<LGADTO>>
                {
                    Object = result
                };
            });
        }

        /// <summary>
        /// This api is used to get the active home delivery locations  
        /// </summary>
        /// <returns></returns>
        [ThirdPartyActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("activehomedeliverylocations")]
        public async Task<IServiceResponse<IEnumerable<LGADTO>>> GetActiveHomeDeliveryLocations()
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _thirdPartyAPIService.GetActiveHomeDeliveryLocations();
                return new ServiceResponse<IEnumerable<LGADTO>>
                {
                    Object = result
                };
            });
        }

        /// <summary>
        /// This api is used to get the details about a shipment 
        /// </summary>
        /// <param name="waybillNumber"></param>
        /// <returns></returns>
        [ThirdPartyActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("preshipmentmobile/{waybillNumber}")]
        public async Task<IServiceResponse<PreShipmentMobileDTO>> GetPreShipmentMobileByWaybill(string waybillNumber)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _thirdPartyAPIService.GetPreShipmentMobileByWaybill(waybillNumber);

                return new ServiceResponse<PreShipmentMobileDTO>
                {
                    Object = result
                };
            });
        }

        /// <summary>
        /// This api is used to get the lists of manifests belonging to a service center 
        /// </summary>
        /// <returns></returns>
        [ThirdPartyActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("manifests")]
        public async Task<IServiceResponse<IEnumerable<ManifestGroupWaybillNumberMappingDTO>>> GetManifestsInServiceCenter(DateFilterCriteria dateFilterCriteria)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _thirdPartyAPIService.GetManifestsInServiceCenter(dateFilterCriteria);

                return new ServiceResponse<IEnumerable<ManifestGroupWaybillNumberMappingDTO>>
                {
                    Object = result
                };
            });
        }

        /// <summary>
        /// This api is used to get the lists of group waybills and waybills assigned to a manifest 
        /// </summary>
        /// <param name="manifestCode"></param>
        /// <returns></returns>
        [ThirdPartyActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("manifests/{manifestCode}")]
        public async Task<IServiceResponse<List<GroupWaybillAndWaybillDTO>>> GetGroupWaybillDataInManifest(string manifestCode)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _thirdPartyAPIService.GetGroupWaybillDataInManifest(manifestCode);

                return new ServiceResponse<List<GroupWaybillAndWaybillDTO>>
                {
                    Object = result
                };
            });
        }

        /// <summary>
        /// This api is used to give "Shipped from the UK" Scan to all the waybills in a manifests
        /// </summary>
        /// <param name="manifestCode"></param>
        /// <returns></returns>
        [ThirdPartyActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("scan/{manifestCode}")]
        public async Task<IServiceResponse<bool>> ScanShipment(string manifestCode)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _thirdPartyAPIService.ItemShippedFromUKScan(manifestCode);

                return new ServiceResponse<bool>
                {
                    Object = result
                };
            });
        }

        [ThirdPartyActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("getaddressdetails")]
        public async Task<IServiceResponse<GoogleAddressDTO>> GetGoogleAddressDetails(GoogleAddressDTO location)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _thirdPartyAPIService.GetGoogleAddressDetails(location);

                return new ServiceResponse<GoogleAddressDTO>
                {
                    Object = result
                };
            });
        }

        /// <summary>
        /// This api is used to get a list of service centres by station id 
        /// </summary>
        /// <returns></returns>
        [ThirdPartyActivityAuthorize(Activity = "View")]
        [HttpGet]
        [Route("servicecentresbystation/{stationId}")]
        public async Task<IServiceResponse<List<ServiceCentreDTO>>> GetServiceCentresByStation(int stationId)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var centres = await _thirdPartyAPIService.GetServiceCentresByStation(stationId);
                return new ServiceResponse<List<ServiceCentreDTO>>
                {
                    Object = centres
                };
            });
        }

        /// <summary>
        /// This api is used to get user detail 
        /// </summary>
        /// <returns></returns>
        [ThirdPartyActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("userdetail")]
        public async Task<IServiceResponse<UserDTO>> GetUserDetail(UserValidationFor3rdParty user)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _thirdPartyAPIService.CheckUserPhoneNo(user);

                return new ServiceResponse<UserDTO>
                {
                    Object = result
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
                    var key = await _thirdPartyAPIService.GetCellulantKey();
                    string apiKey = headers.GetValues("api_key").FirstOrDefault();
                    string token = await _thirdPartyAPIService.Decrypt(apiKey);
                    if (token == key)
                    {
                        result = await _thirdPartyAPIService.AddCellulantTransferDetails(TransferDetailsDTO);
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


        /// <summary>
        /// This api is used to create multiple shipments by one receiver 
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("createmultipleshipment")]
        public async Task<IServiceResponse<object>> CreateMultipleShipment(PreShipmentMobileMultiMerchantDTO preshipmentMobile)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var shipments = await _thirdPartyAPIService.AddMultiplePreShipmentMobile(preshipmentMobile);

                return new ServiceResponse<object>
                {
                    Object = shipments
                };
            });
        }

        /// <summary>
        /// This api is used to get price for multiple shipment creation
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        [Route("getpriceformultiplemerchantshipment")]
        public async Task<IServiceResponse<MultiMerchantMobilePriceDTO>> GetPriceMultipleMobileShipment(PreShipmentMobileMultiMerchantDTO preshipmentMobile)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var shipments = await _thirdPartyAPIService.GetPriceMultipleMobileShipment(preshipmentMobile);

                return new ServiceResponse<MultiMerchantMobilePriceDTO>
                {
                    Object = shipments
                };
            });
        }

        /// <summary>
        /// This api is used to charge a customer's wallet
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        [Route("chargewallet")]
        public async Task<IServiceResponse<ResponseDTO>> ChargeWallet(ChargeWalletDTO responseDTO)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _thirdPartyAPIService.ChargeWallet(responseDTO);
                return new ServiceResponse<ResponseDTO>
                {
                    Object = result,
                };
            });
        }

      
        [HttpPut]
        [Route("updatemerchantsubscription/{merchantcode}")]
        public async Task<IServiceResponse<ResponseDTO>> UpdateMerchantClassSubscriptionForAlpha(string merchantcode)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var result = await _thirdPartyAPIService.UpdateUserRankForAlpha(merchantcode);

                return new ServiceResponse<ResponseDTO>
                {
                    Object = result
                };
            });
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("codcallback")]
        public async Task<IServiceResponse<bool>> CODCallBack(CODCallBackDTO cod)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var response = new ServiceResponse<bool>();
                var request = Request;
                var headers = request.Headers;
                var result = false;

                if (headers.Contains("api_key"))
                {
                    var key = await _thirdPartyAPIService.GetCellulantKey();
                    string apiKey = headers.GetValues("api_key").FirstOrDefault();
                    string token = await _thirdPartyAPIService.Decrypt(apiKey);
                    if (token == key)
                    {
                        result = await _thirdPartyAPIService.CODCallBack(cod);
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
        [Route("updateshipmentcallback")]
        public async Task<object> UpdateCODShipmentOnCallBack(PushPaymentStatusRequstPayload payload)
        {
            //return await HandleApiOperationAsync(async () =>
            //{
            //    var result = await _thirdPartyAPIService.UpdateCODShipmentOnCallBack(payload);

            //    return new ServiceResponse<object>
            //    {
            //        Object = result
            //    };
            //});
            var result = await _thirdPartyAPIService.UpdateCODShipmentOnCallBack(payload);
            return result;
        }


        [AllowAnonymous]
        [HttpPost]
        [Route("updateshipmentcallbackstellas")]
        public async Task<IServiceResponse<bool>> UpdateCODShipmentOnCallBackStellas(CODCallBackDTO cod)
        {
            var result = await _thirdPartyAPIService.UpdateCODShipmentOnCallBackStellas(cod);
            return new ServiceResponse<bool>
            {
                Object = result
            };
        }

        /// <summary>
        /// This api is used to cancel shipment
        /// </summary>
        /// <returns></returns>
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
                var flag = await _thirdPartyAPIService.CancelShipment(cancel.Waybill);

                return new ServiceResponse<object>
                {
                    Object = flag
                };
            });
        }

        /// <summary>
        /// This api is used to get company details
        /// </summary>
        /// <returns></returns>
        [ThirdPartyActivityAuthorize(Activity = "View")]
        [HttpPost]
        [Route("getcompanydetails")]
        public async Task<IServiceResponse<CompanyDTO>> GetCompanyDetailsByEmail(CompanySearchDTO searchDTO)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var company = await _thirdPartyAPIService.GetCompanyDetailsByEmail(searchDTO.email);

                return new ServiceResponse<CompanyDTO>
                {
                    Object = company
                };
            });
        }

        /// <summary>
        /// This api is used to cancel shipment (v2)
        /// </summary>
        /// <returns></returns>
        [ThirdPartyActivityAuthorize(Activity = "Create")]
        [HttpPost]
        [Route("cancelshipment")]
        public async Task<IServiceResponse<object>> CancelShipmentV2(CancelShipmentDTO payload)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var flag = await _thirdPartyAPIService.CancelShipment(payload.Waybill);

                return new ServiceResponse<object>
                {
                    Object = flag
                };
            });
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("addazapaytransferdetails")]
        public async Task<IServiceResponse<object>> AddAzapayTransferDetails(AzapayTransferDetailsDTO TransferDetailsDTO)
        {
            return await HandleApiOperationAsync(async () =>
            {
                var response = new ServiceResponse<object>();
                var request = Request;
                var headers = request.Headers;
                var result = new object();

                if (headers.Contains("api_key"))
                {
                    var key = await _thirdPartyAPIService.GetCellulantKey();
                    string apiKey = headers.GetValues("api_key").FirstOrDefault();
                    string token = await _thirdPartyAPIService.Decrypt(apiKey);
                    if (token == key)
                    {
                        result = await _thirdPartyAPIService.AddAzaPayTransferDetails(TransferDetailsDTO);
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
    }
}