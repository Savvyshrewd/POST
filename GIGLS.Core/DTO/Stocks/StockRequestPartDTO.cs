﻿using POST.Core.DTO.Fleets;
using POST.CORE.DTO;
using System.Collections.Generic;

namespace POST.Core.DTO.Stocks
{
    public class StockRequestPartDTO : BaseDomainDTO
    {
        public StockRequestPartDTO()
        {
            StockRequest = new List<StockRequestDTO>();
            FleetParts = new List<FleetPartDTO>();
        }
        public int StockRequestPartId { get; set; }
        public int Quantity { get; set; }
        public int QuantitySupplied { get; set; }
        public decimal UnitPrice { get; set; }
        public string SerialNumber { get; set; }
        public List<StockRequestDTO> StockRequest { get; set; }
        public List<FleetPartDTO> FleetParts { get; set; }
    }
}
