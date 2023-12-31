﻿using POST.Core.DTO.Stocks;
using System.Threading.Tasks;

namespace POST.Core.IServices.Stocks
{
    public interface IStockRequestPartService : IServiceDependencyMarker
    {
        Task<StockRequestPartDTO> GetStockRequestParts();
        Task<StockRequestPartDTO> GetStockRequestPartById(int stockId);
        Task<object> AddStockRequestPart(StockRequestPartDTO stock);
        Task UpdateStockRequestPart(int stockId, StockRequestPartDTO stock);
        Task DeleteStockRequestPart(int stockId);
    }
}
