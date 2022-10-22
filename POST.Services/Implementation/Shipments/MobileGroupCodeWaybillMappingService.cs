﻿using POST.Core;
using POST.Core.DTO.Shipments;
using POST.Core.IServices.Shipments;
using POST.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace POST.Services.Implementation.Shipments
{
    public class MobileGroupCodeWaybillMappingService : IMobileGroupCodeWaybillMappingService
    {
        private readonly IUnitOfWork _uow;
        private readonly IPreShipmentMobileService _preShipmentMobileService;

        public MobileGroupCodeWaybillMappingService(IUnitOfWork uow, IPreShipmentMobileService preShipmentMobileService)
        {
            _uow = uow;
            _preShipmentMobileService = preShipmentMobileService;
            MapperConfig.Initialize();
        }

        //Get WayBillDetails In Group
        public async Task<MobileGroupCodeWaybillMappingDTO> GetWaybillDetailsInGroup (string groupCodeNumber)
        {
            try
            {
                var groupList = await _uow.MobileGroupCodeWaybillMapping.FindAsync(x => x.GroupCodeNumber == groupCodeNumber);
                
                if(groupList != null)
                {
                    var preShipmentList = new List<PreShipmentMobileDTO>();

                    foreach (var item in groupList)
                    {
                        var preShipmentDTO = await _preShipmentMobileService.GetPreShipmentDetail(item.WaybillNumber);
                        preShipmentList.Add(preShipmentDTO);
                    }

                    var groupCodeWaybillMappingDTO = new MobileGroupCodeWaybillMappingDTO
                    {
                        GroupCodeNumber = groupCodeNumber,
                        DateMapped = groupList.Select(x => x.DateMapped).FirstOrDefault(),
                        PreShipmentMobile = preShipmentList,
                    };

                    return groupCodeWaybillMappingDTO;
                }
                else
                {
                    throw new GenericException($"No GroupCode exists for this : {groupCodeNumber}", $"{(int)HttpStatusCode.NotFound}");
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        //Get Only WayBillNumbers In Group
        public async Task<MobileGroupCodeWaybillMappingDTO> GetWaybillNumbersInGroup(string groupCodeNumber)
        {
            try
            {
                var groupList = await _uow.MobileGroupCodeWaybillMapping.FindAsync(x => x.GroupCodeNumber == groupCodeNumber);
                
                if (groupList != null)
                {
                    var waybillList = new List<string>();

                    foreach (var item in groupList)
                    {
                        waybillList.Add(item.WaybillNumber);
                    }
                    var groupCodeWaybillMappingDTO = new MobileGroupCodeWaybillMappingDTO
                    {
                        GroupCodeNumber = groupCodeNumber,
                        DateMapped = groupList.Select(x => x.DateMapped).FirstOrDefault(),
                        WaybillNumbers = waybillList
                    };
                    return groupCodeWaybillMappingDTO;
                }
                else
                {
                    throw new GenericException($"No GroupCode exists for this : {groupCodeNumber}", $"{(int)HttpStatusCode.NotFound}");
                }
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
