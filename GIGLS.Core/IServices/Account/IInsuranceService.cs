﻿using POST.Core.DTO.Account;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace POST.Core.IServices.Account
{
    public interface IInsuranceService : IServiceDependencyMarker
    {
        Task<IEnumerable<InsuranceDTO>> GetInsurances();
        Task<InsuranceDTO> GetInsuranceById(int insuranceId);
        Task<object> AddInsurance(InsuranceDTO insurance);
        Task UpdateInsurance(int insuranceId, InsuranceDTO insurance);
        Task RemoveInsurance(int insuranceId);
        Task<InsuranceDTO> GetInsuranceByCountry();
        Task<decimal> GetInsuranceValueByCountry();
    }
}
