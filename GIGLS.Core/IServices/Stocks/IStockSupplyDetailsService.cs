﻿using POST.Core.DTO.Stocks;
using System.Threading.Tasks;

namespace POST.Core.IServices.Stocks
{
    public interface IStockSupplyDetailsService : IServiceDependencyMarker
    {
        Task<StockSupplyDetailsDTO> GetStockSupplyDetails();
        Task<StockSupplyDetailsDTO> GetStockSupplyDetailById(int stockId);
        Task<object> AddStockSupplyDetail(StockSupplyDetailsDTO stock);
        Task UpdateStockSupplyDetail(int stockId, StockSupplyDetailsDTO stock);
        Task DeleteStockSupplyDetail(int stockId);
    }
}
