﻿using POST.Core.IServices;
using POST.CORE.DTO.Nav;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace POST.CORE.IServices.Nav
{
    public interface ISubSubNavService : IServiceDependencyMarker
    {
        Task<List<SubSubNavDTO>> GetSubSubNavs();
        Task<SubSubNavDTO> GetSubSubNavById(int subSubNavId);
        Task<object> AddSubSubNav(SubSubNavDTO subSubNav);
        Task UpdateSubSubNav(int subSubNavId, SubSubNavDTO subSubNav);
        Task RemoveSubSubNav(int subSubNavId);
    }
}
