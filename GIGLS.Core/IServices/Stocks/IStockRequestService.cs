﻿using POST.Core.DTO.Stocks;
using System.Threading.Tasks;

namespace POST.Core.IServices.Stocks
{
    public interface IStockRequestService : IServiceDependencyMarker
    {
        Task<StockRequestDTO> GetStockRequests();
        Task<StockRequestDTO> GetStockRequestById(int stockId);
        Task<object> AddStockRequest(StockRequestDTO stock);
        Task UpdateStockRequest(int stockId, StockRequestDTO stock);
        Task DeleteStockRequest(int stockId);
    }
}
