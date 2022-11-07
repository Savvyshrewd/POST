﻿using POST.Core.DTO.Shipments;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace POST.Core.IServices.Shipments
{
    public interface IMissingShipmentService : IServiceDependencyMarker
    {
        Task<List<MissingShipmentDTO>> GetMissingShipments();
        Task<MissingShipmentDTO> GetMissingShipmentById(int missingShipmentId);
        Task<object> AddMissingShipment(MissingShipmentDTO missingShipment);
        Task UpdateMissingShipment(int missingShipmentId, MissingShipmentDTO missingShipment);
        Task DeleteMissingShipment(int missingShipmentId);
    }
}
