﻿using POST.Core.DTO.Account;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace POST.Core.IServices.Account
{
    public interface IVATService : IServiceDependencyMarker
    {
        Task<IEnumerable<VATDTO>> GetVATs();
        Task<VATDTO> GetVATById(int vatId);
        Task<object> AddVAT(VATDTO vat);
        Task UpdateVAT(int vatId, VATDTO vat);
        Task RemoveVAT(int vatId);
        Task<VATDTO> GetVATByCountry();
    }
}
