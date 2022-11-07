﻿using System;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Web.Http;
using POST.Core.IServices;
using System.Net.Http;
using POST.Infrastructure;
using POST.Services.Implementation;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using POST.WebApi.Models;
//using Audit.WebApi;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using POST.WebApi.Helper;

namespace POST.WebApi.Controllers
{
    public abstract class BaseWebApiController : ApiController
    {
        private readonly string _controllerName;
        private ModelFactory _modelFactory;
        private ApplicationUserManager _AppUserManager = null;
        private ApplicationRoleManager _AppRoleManager = null;

        protected BaseWebApiController(string controllerName)
        {
            _controllerName = controllerName;
            //Logger = LogManager.GetLogger(controllerName);
        }

        protected ApplicationUserManager AppUserManager
        {
            get
            {
                return _AppUserManager ?? Request.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
        }

        protected ApplicationRoleManager AppRoleManager
        {
            get
            {
                return _AppRoleManager ?? Request.GetOwinContext().GetUserManager<ApplicationRoleManager>();
            }
        }

        protected ModelFactory TheModelFactory
        {
            get
            {
                if (_modelFactory == null)
                {
                    _modelFactory = new ModelFactory(this.Request, this.AppUserManager);
                }
                return _modelFactory;
            }
        }

        private string _databaseForeignKeyErrorMessage;

        protected async Task<IServiceResponse<T>> HandleApiOperationAsync<T>(
            Func<Task<ServiceResponse<T>>> action, [CallerLineNumber] int lineNo = 0,
            [CallerMemberName] string method = "")
        {
            var apiResponse = new ServiceResponse<T>
            {
                Code = $"{(int)HttpStatusCode.OK}",
                ShortDescription = "Operation was successful!"
            };

            try
            {
                if (!ModelState.IsValid)
                    throw new GenericException("There were errors in your input, please correct them and try again.", $"{(int)HttpStatusCode.BadRequest}");

                var methodResponse = await action.Invoke();

                apiResponse.Object = methodResponse.Object;
                apiResponse.Total = methodResponse.Total;
                apiResponse.Cookies = methodResponse.Cookies;
                apiResponse.more_reults = methodResponse.more_reults;
                apiResponse.RefCode = methodResponse.RefCode;
                apiResponse.Shipmentcodref = methodResponse.Shipmentcodref;
                apiResponse.VehicleType = methodResponse.VehicleType;
                apiResponse.ReferrerCode = methodResponse.ReferrerCode;
                apiResponse.AverageRatings = methodResponse.AverageRatings;
                apiResponse.IsVerified = methodResponse.IsVerified;
                apiResponse.PartnerType = methodResponse.PartnerType;
                apiResponse.IsEligible = methodResponse.IsEligible;
                apiResponse.ShortDescription = string.IsNullOrEmpty(methodResponse.ShortDescription)
                    ? apiResponse.ShortDescription
                    : methodResponse.ShortDescription;

            }
            catch (GenericException giglsex) //Error involving form field values
            {
                apiResponse.ShortDescription = giglsex.Message;
                apiResponse.Code = giglsex.ErrorCode;

                if (!ModelState.IsValid)
                {
                    apiResponse.ValidationErrors = ModelState.ToDictionary(
                        m =>
                        {
                            var tokens = m.Key.Split('.');
                            return tokens.Length > 0 ? tokens[tokens.Length - 1] : tokens[0];
                        },
                        m => m.Value.Errors.Select(e => e.Exception?.Message ?? e.ErrorMessage)
                    );
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(apiResponse.Code))
                    {
                        apiResponse.Code = $"{(int)HttpStatusCode.BadRequest}";
                    }
                    List<string> errorList = new List<string>();
                    errorList.Add(giglsex.Message);
                    if(giglsex.InnerException != null ) errorList.Add(giglsex.InnerException?.Message);
                    if(giglsex.InnerException?.InnerException != null) errorList.Add(giglsex.InnerException?.InnerException?.Message);
                    if(giglsex.InnerException?.InnerException?.InnerException != null) errorList.Add(giglsex.InnerException?.InnerException?.InnerException?.Message);
                    apiResponse.ValidationErrors.Add("Error", errorList);
                }
            }
            catch(DbUpdateException duex) when (duex.IsDatabaseFkDeleteException(out _databaseForeignKeyErrorMessage))
            {
                apiResponse.ShortDescription = "You cannot delete this record because it's currently in use.";
                apiResponse.Code = $"{(int)HttpStatusCode.Forbidden}"; 
            }
            catch (DbEntityValidationException devex) // Shouldn't happen but is useful for catching & fixing DB validation errors
            {
                apiResponse.ShortDescription = "A data validation error occurred. Please contact admin for assistance.";
                apiResponse.Code = $"{(int)HttpStatusCode.Forbidden}";
            }
            catch (Exception ex)
            {
                apiResponse.ShortDescription = $"Sorry, we are unable process your request. Please try again or contact support for assistance.";
                apiResponse.Code = $"{(int)HttpStatusCode.InternalServerError}";

                List<string> errorList = new List<string>();
                errorList.Add(ex.Message);
                errorList.Add(ex.StackTrace);
                if(ex.InnerException != null) errorList.Add(ex.InnerException?.Message);
                if(ex.InnerException?.InnerException != null) errorList.Add(ex.InnerException?.InnerException?.Message);
                if(ex.InnerException?.InnerException?.InnerException != null) errorList.Add(ex.InnerException?.InnerException?.InnerException?.Message);
                apiResponse.ValidationErrors.Add("Error", errorList);
            }

            return apiResponse;
        }
              
        protected IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (!result.Succeeded)
            {
                if (result.Errors != null)
                {
                    foreach (string error in result.Errors)
                    {
                        ModelState.AddModelError("", error);
                    }
                }

                if (ModelState.IsValid)
                {
                    // No ModelState errors are available to send, so just return an empty BadRequest.
                    return BadRequest();
                }

                return BadRequest(ModelState);
            }

            return null;
        }
    }
}
