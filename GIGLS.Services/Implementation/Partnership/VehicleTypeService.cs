﻿using POST.Core;
using POST.Core.IServices.Partnership;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POST.Services.Implementation.Partnership
{
    public class VehicleTypeService: IVehicleTypeService
    {
        private readonly IUnitOfWork _uow;
        public VehicleTypeService(IUnitOfWork uow)
        {
            _uow = uow;
            MapperConfig.Initialize();
        }
    }
}
