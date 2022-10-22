﻿using POST.Core.DTO.BankSettlement;
using POST.Core.DTO.Wallet;
using POST.Core.Enums;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace POST.Core.IServices.BankSettlement
{
    public interface ICODSettlementSheetService : IServiceDependencyMarker
    {
        Task<IEnumerable<CODSettlementSheetDTO>> GetCODSettlementSheets();
        Task<CODSettlementSheetDTO> GetCODSettlementSheetById(int codSettlementSheetId);
        Task<object> AddCODSettlementSheet(CODSettlementSheetDTO codSettlementSheetDto);
        Task UpdateCODSettlementSheet(int codSettlementSheetId, CODSettlementSheetDTO codSettlementSheetDto);
        Task DeleteCODSettlementSheet(int codSettlementSheetId);
        Task UpdateMultipleStatusCODSettlementSheet(List<string> WaybillNumbers);
        Task<IEnumerable<CODSettlementSheetDTO>> GetUnbankedCODShipmentSettlement();
    }
}
