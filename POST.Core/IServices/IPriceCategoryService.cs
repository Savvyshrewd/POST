﻿using POST.Core.DTO.PaymentTransactions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace POST.Core.IServices
{
    public interface IPriceCategoryService : IServiceDependencyMarker
    {
        Task<IEnumerable<PriceCategoryDTO>> GetPriceCategorys();
        Task<PriceCategoryDTO> GetPriceCategoryById(int priceCategoryId);
        Task<object> AddPriceCategory(PriceCategoryDTO priceCategory);
        Task UpdatePriceCategory(int priceCategoryId, PriceCategoryDTO priceCategory);
        Task DeletePriceCategory(int priceCategoryId);
        Task<IEnumerable<PriceCategoryDTO>> GetPriceCategoriesByCountry(int countryId);
        Task<IEnumerable<PriceCategoryDTO>> GetPriceCategoriesBothCountries(int countryId);
    }
}
