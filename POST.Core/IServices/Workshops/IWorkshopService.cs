﻿using POST.Core.DTO.Workshops;
using System.Threading.Tasks;

namespace POST.Core.IServices.Workshops
{
    public interface IWorkshopService : IServiceDependencyMarker
    {
        Task<WorkshopDTO> GetWorkshops();
        Task<WorkshopDTO> GetWorkshopById(int workshopId);
        Task<object> AddWorkshop(WorkshopDTO workshop);
        Task UpdateWorkshop(int workshopId, WorkshopDTO workshop);
        Task DeleteWorkshop(int workshopId);
    }
}
