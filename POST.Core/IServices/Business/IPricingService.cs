﻿using POST.Core.DTO.PaymentTransactions;
using POST.Core.DTO.Shipments;
using System.Threading.Tasks;

namespace POST.Core.IServices.Business
{
    public interface IPricingService : IServiceDependencyMarker
    {
        Task<decimal> GetPrice(PricingDTO pricingDto);
        Task<decimal> GetHaulagePrice(HaulagePricingDTO pricingDto);
        Task<decimal> GetEcommerceReturnPrice(PricingDTO pricingDto);
        Task<decimal> GetInternationalPrice(PricingDTO pricingDto);
        Task<ShipmentDTO> GetReroutePrice(ReroutePricingDTO pricingDto);
        Task<decimal> GetMobileRegularPrice(PricingDTO pricingDto);
        Task<decimal> GetMobileEcommercePrice(PricingDTO pricingDto);
        Task<decimal> GetMobileSpecialPrice(PricingDTO pricingDto);
        Task<decimal> GetCountryCurrencyRatio();
        Task<int> GetUserCountryId();
        Task<decimal> GetDropOffRegularPriceForIndividual(PricingDTO pricingDto);
        Task<decimal> GetDropOffSpecialPrice(PricingDTO pricingDto);
        Task<decimal> GetEcommerceDropOffPrice(PricingDTO pricingDto);
        Task<NewPricingDTO> GetGrandPriceForShipment(NewShipmentDTO newShipmentDTO);
        Task<decimal> CalculateCustomerRankPrice(PricingDTO pricingDto, decimal price);
        Task<decimal> GetPriceForUK(UKPricingDTO pricingDto);
        Task<decimal> GetPriceByCategory(UKPricingDTO pricingDto);
        Task<decimal> GetCoporateDiscountedAmount(string customerCode, decimal price);
        Task<decimal> GetPriceByCategoryForUSA(UKPricingDTO pricingDto);
    }
}
