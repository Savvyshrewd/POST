﻿using GIGLS.Core.IServices;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;

namespace GIGLS.Services.Implementation
{
    public class ServiceResponse<TResponse> : IServiceResponse<TResponse>
    {
        public ServiceResponse(TResponse response) : this()
        {
            Object = response;
        }

        public ServiceResponse()
        {
            ValidationErrors = new Dictionary<string, IEnumerable<string>>();
        }

        public string Code { get; set; }
        public string ShortDescription { get; set; }
        public TResponse Object { get; set; }
        public int Total { get; set; }

        public Dictionary<string, IEnumerable<string>> ValidationErrors { get; set; }
    }
}
