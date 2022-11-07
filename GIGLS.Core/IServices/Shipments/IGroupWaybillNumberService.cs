﻿using POST.Core.Domain;
using POST.Core.DTO.Shipments;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace POST.Core.IServices.Shipments
{
    public interface IGroupWaybillNumberService : IServiceDependencyMarker
    {
        Task<IEnumerable<GroupWaybillNumberDTO>> GetAllGroupWayBillNumbers();
        Task<List<GroupWaybillNumberDTO>> GetActiveGroupWayBillNumbers();
        Task<List<GroupWaybillNumberDTO>> GetDeliverGroupWayBillNumbers();
        Task<GroupWaybillNumberDTO> GetGroupWayBillNumberById(int groupwaybillId);
        Task<GroupWaybillNumberDTO> GetGroupWayBillNumberById(string groupwaybillNumber);
        Task<string> GenerateGroupWaybillNumber(GroupWaybillNumberDTO groupWaybillNumberDTO);
        Task<string> GenerateGroupWaybillNumber(string serviceCentreCode);
        Task UpdateGroupWaybillNumber(int groupwaybillId);
        Task UpdateGroupWaybillNumber(string groupwaybillNumber);
        Task RemoveGroupWaybillNumber(int groupwaybillId);
        Task RemoveGroupWaybillNumber(string groupwaybillId);
        Task<GroupWaybillNumber> GetGroupWayBillNumberForScan(string groupwaybillNumber);
        Task ChangeDepartureServiceInGroupWaybill(int serviceCentreId, string groupWaybillNumber, bool hasManifest = false);
    }
}
