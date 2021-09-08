﻿using AutoMapper;
using GIGLS.Core.Domain;
using GIGLS.Core.DTO;
using GIGLS.Core.DTO.Customers;
using GIGLS.Core.DTO.Report;
using GIGLS.Core.DTO.Shipments;
using GIGLS.Core.Enums;
using GIGLS.Core.IRepositories.Shipments;
using GIGLS.Infrastructure.Persistence.Repository;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;

namespace GIGLS.Infrastructure.Persistence.Repositories.Shipments
{
    public class PreShipmentMobileRepository : Repository<PreShipmentMobile, GIGLSContext>, IPreShipmentMobileRepository
    {
        private GIGLSContext _context;
        public PreShipmentMobileRepository(GIGLSContext context) : base(context)
        {
            _context = context;
        }

        public Task<List<PreShipmentMobileDTO>> GetPreShipmentsForMobile()
        {
            try
            {
                var preShipmentMobile = _context.PresShipmentMobile.AsQueryable();

                List<PreShipmentMobileDTO> preShipmentDto = new List<PreShipmentMobileDTO>();

                preShipmentDto = (from r in preShipmentMobile
                                  select new PreShipmentMobileDTO()
                                  {
                                      PreShipmentMobileId = r.PreShipmentMobileId,
                                      Waybill = r.Waybill,
                                      CustomerType = r.CustomerType,
                                      ActualDateOfArrival = r.ActualDateOfArrival,
                                      DateCreated = r.DateCreated,
                                      DateModified = r.DateModified,
                                      ExpectedDateOfArrival = r.ExpectedDateOfArrival,
                                      ReceiverAddress = r.ReceiverAddress,
                                      SenderAddress = r.SenderAddress,
                                      ReceiverCity = r.ReceiverCity,
                                      ReceiverCountry = r.ReceiverCountry,
                                      ReceiverEmail = r.ReceiverEmail,
                                      ReceiverName = r.ReceiverName,
                                      ReceiverPhoneNumber = r.ReceiverPhoneNumber,
                                      ReceiverState = r.ReceiverState,
                                      SenderName = r.SenderName,
                                      UserId = r.UserId,
                                      Value = r.Value,
                                      GrandTotal = r.GrandTotal,
                                      DeliveryPrice = r.DeliveryPrice,
                                      CalculatedTotal = r.CalculatedTotal,
                                      InsuranceValue = r.InsuranceValue,
                                      DiscountValue = r.DiscountValue,
                                      CompanyType = r.CompanyType,
                                      CustomerCode = r.CustomerCode
                                  }).OrderByDescending(x => x.DateCreated).Take(20).ToList();

                return Task.FromResult(preShipmentDto.OrderByDescending(x => x.DateCreated).ToList());
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task<PreShipmentMobileDTO> GetBasicPreShipmentMobileDetail(string waybill)
        {
            var preShipment = _context.PresShipmentMobile.Where(x => x.Waybill == waybill);

            PreShipmentMobileDTO preShipmentDto = (from r in preShipment
                                                   select new PreShipmentMobileDTO
                                                   {
                                                       PreShipmentMobileId = r.PreShipmentMobileId,
                                                       Waybill = r.Waybill,
                                                       CustomerType = r.CustomerType,
                                                       ActualDateOfArrival = r.ActualDateOfArrival,
                                                       DateCreated = r.DateCreated,
                                                       DateModified = r.DateModified,
                                                       ExpectedDateOfArrival = r.ExpectedDateOfArrival,
                                                       ReceiverAddress = r.ReceiverAddress,
                                                       SenderAddress = r.SenderAddress,
                                                       ReceiverCity = r.ReceiverCity,
                                                       ReceiverCountry = r.ReceiverCountry,
                                                       ReceiverEmail = r.ReceiverEmail,
                                                       ReceiverName = r.ReceiverName,
                                                       ReceiverPhoneNumber = r.ReceiverPhoneNumber,
                                                       ReceiverState = r.ReceiverState,
                                                       SenderName = r.SenderName,
                                                       UserId = r.UserId,
                                                       Value = r.Value,
                                                       GrandTotal = r.GrandTotal,
                                                       DeliveryPrice = r.DeliveryPrice,
                                                       CalculatedTotal = r.CalculatedTotal,
                                                       InsuranceValue = r.InsuranceValue,
                                                       DiscountValue = r.DiscountValue,
                                                       CompanyType = r.CompanyType,
                                                       CustomerCode = r.CustomerCode,
                                                       ShipmentPackagePrice = r.ShipmentPackagePrice,
                                                       Total = r.Total,
                                                       CashOnDeliveryAmount = r.CashOnDeliveryAmount,
                                                       IsCashOnDelivery = r.IsCashOnDelivery,
                                                       WaybillImageUrl = r.WaybillImageUrl,
                                                   }).FirstOrDefault();

            return Task.FromResult(preShipmentDto);
        }

        public IQueryable<PreShipmentMobileDTO> GetPreShipmentForUser(string userChannelCode)
        {
            var preShipments =  Context.PresShipmentMobile.AsQueryable().Where(s => s.CustomerCode == userChannelCode);

            var shipmentDto = (from r in preShipments
                               join co in _context.Country on r.CountryId equals co.CountryId
                               select new PreShipmentMobileDTO()
                               {
                                   PreShipmentMobileId = r.PreShipmentMobileId,
                                   Waybill = r.Waybill,
                                   DateCreated = r.DateCreated,
                                   DateModified = r.DateModified,
                                   ReceiverAddress = r.ReceiverAddress,
                                   SenderAddress = r.SenderAddress,
                                   ReceiverName = r.ReceiverName,
                                   ReceiverPhoneNumber = r.ReceiverPhoneNumber,
                                   shipmentstatus = r.shipmentstatus,
                                   GrandTotal = r.GrandTotal,
                                   CurrencySymbol = co.CurrencySymbol,
                                   IsDelivered = r.IsDelivered,
                                   IsBatchPickUp = r.IsBatchPickUp,
                                   IsCancelled = r.IsCancelled,
                                   ZoneMapping = r.ZoneMapping,
                                   PreShipmentItems = _context.PresShipmentItemMobile.Where(d => d.PreShipmentMobileId == r.PreShipmentMobileId)
                                      .Select(y => new PreShipmentItemMobileDTO
                                      {
                                          PreShipmentItemMobileId = y.PreShipmentItemMobileId,
                                          Description = y.Description
                                      }).ToList()
                               });

            return shipmentDto;
        }

        public IQueryable<PreShipmentMobileDTO> GetShipments(string userChannelCode)
        {
            var preShipments = Context.Shipment.AsQueryable().Where(x => x.CustomerCode == userChannelCode && x.IsCancelled == false);

            var shipmentDto = (from r in preShipments
                               join co in _context.Country on r.DepartureCountryId equals co.CountryId
                               select new PreShipmentMobileDTO()
                               {
                                   PreShipmentMobileId = r.ShipmentId,
                                   Waybill = r.Waybill,
                                   DateCreated = r.DateCreated,
                                   DateModified = r.DateModified,
                                   ReceiverAddress = r.ReceiverAddress,
                                   ReceiverName = r.ReceiverName,
                                   ReceiverPhoneNumber = r.ReceiverPhoneNumber,
                                   SenderAddress = r.SenderAddress,
                                   GrandTotal = r.GrandTotal,
                                   shipmentstatus = "Shipment",
                                   CurrencySymbol = co.CurrencySymbol,
                                   CustomerType = r.CustomerType,
                                   CustomerId = r.CustomerId,
                                   PreShipmentItems = _context.ShipmentItem.Where(d => d.ShipmentId == r.ShipmentId)
                                      .Select(y => new PreShipmentItemMobileDTO
                                      {
                                          PreShipmentItemMobileId = y.ShipmentItemId,
                                          Description = y.Description
                                      }).ToList()
                               });

            return shipmentDto;
        }

        //Get Shipments that have not been paid for by user
        public async Task<List<OutstandingPaymentsDTO>> GetAllOutstandingShipmentsForUser(string userChannelCode)
        {
            var shipments = _context.Shipment.AsQueryable().Where(s => s.CustomerCode == userChannelCode && s.IsCancelled == false);
            var usCountry = _context.Country.AsQueryable().Where(s => s.CountryId == 207).FirstOrDefault();
            var ukCountry = _context.Country.AsQueryable().Where(s => s.CountryId == 62).FirstOrDefault();
            var naijaCountry = _context.Country.AsQueryable().Where(s => s.CountryId == 1).FirstOrDefault();


            var result = (from s in shipments
                          join i in Context.Invoice on s.Waybill equals i.Waybill
                          join dept in Context.ServiceCentre on s.DepartureServiceCentreId equals dept.ServiceCentreId
                          join dest in Context.ServiceCentre on s.DestinationServiceCentreId equals dest.ServiceCentreId
                          where i.PaymentStatus == Core.Enums.PaymentStatus.Pending
                          select new OutstandingPaymentsDTO()
                          {
                              Waybill = s.Waybill,
                              Departure = dept.Name,
                              Destination = dest.Name,
                              Amount = i.Amount,
                              Country = _context.Country.Where(c => c.CountryId == i.CountryId).Select(x => new CountryDTO
                              {
                                  CurrencySymbol = x.CurrencySymbol,
                                  CountryCode = x.CountryCode,
                                  CurrencyCode = x.CurrencyCode,
                                  CountryName = x.CountryName,
                              }).FirstOrDefault(),
                              CountryId = i.CountryId,
                              CurrencySymbol = Context.Country.Where(c => c.CountryId == i.CountryId).Select(x => x.CurrencySymbol).FirstOrDefault(),
                              Description = s.Description,
                              DateCreated = s.DateCreated
                          }).ToList();


            if (result.Any())
            {
               result = await GetEquivalentAmountOfActiveCurrencies(result);
            }
            return result.OrderByDescending(x => x.DateCreated).ToList();
        }

        private async  Task<decimal> GetDiscountForInternationalShipmentBasedOnRank(Rank rank, int countryId)
        {
            decimal percentage = 0.00M;

            if (rank == Rank.Class)
            {
                var globalProperty =  _context.GlobalProperty.AsQueryable().Where(s => s.Key == GlobalPropertyType.InternationalRankClassDiscount.ToString() && s.CountryId == countryId).FirstOrDefault();
                if (globalProperty != null)
                {
                    percentage = Convert.ToDecimal(globalProperty.Value);
                }
            }
            else
            {
                var globalProperty = _context.GlobalProperty.AsQueryable().Where(s => s.Key == GlobalPropertyType.InternationalBasicClassDiscount.ToString() && s.CountryId == countryId).FirstOrDefault();
                if (globalProperty != null)
                {
                    percentage = Convert.ToDecimal(globalProperty.Value);
                }
            }

            decimal discount = ((100M - percentage) / 100M);
            return discount;
        }

        public async Task<List<PreShipmentMobileReportDTO>> GetPreShipments(MobileShipmentFilterCriteria accountFilterCriteria)
        {
            try
            {
                //DateTime StartDate = accountFilterCriteria.StartDate.GetValueOrDefault().Date;
                //DateTime EndDate = accountFilterCriteria.EndDate?.Date ?? StartDate;

                //get startDate and endDate
                var queryDate = accountFilterCriteria.getStartDateAndEndDate();
                var StartDate = queryDate.Item1;
                var EndDate = queryDate.Item2;

                //declare parameters for the stored procedure
                SqlParameter startDate = new SqlParameter("@StartDate", StartDate);
                SqlParameter endDate = new SqlParameter("@EndDate", EndDate);
                SqlParameter departureStationId = new SqlParameter("@DepartureStationId", accountFilterCriteria.DepartureStationId);
                SqlParameter destinationStationId = new SqlParameter("@DestinationStationId", accountFilterCriteria.DestinationStationId);
                SqlParameter countryId = new SqlParameter("@CountryId", accountFilterCriteria.CountryId);
                SqlParameter vehicleType = new SqlParameter("@VehicleType", (object)accountFilterCriteria.VehicleType ?? DBNull.Value);
                SqlParameter companyType = new SqlParameter("@CompanyType", (object)accountFilterCriteria.CompanyType ?? DBNull.Value);
                SqlParameter shipmentstatus = new SqlParameter("@Shipmentstatus", (object)accountFilterCriteria.Shipmentstatus ?? DBNull.Value);

                SqlParameter[] param = new SqlParameter[]
                {
                startDate,
                endDate,
                departureStationId,
                destinationStationId,
                countryId,
                vehicleType,
                companyType,
                shipmentstatus
                };

                //var listCreated = new List<PreShipmentMobileReportDTO>();

                var listCreated = await _context.Database.SqlQuery<PreShipmentMobileReportDTO>("GIGGOReporting " +
                   "@StartDate, @EndDate, @DepartureStationId, @DestinationStationId, @CountryId, @VehicleType, @CompanyType, @Shipmentstatus",
                   param).ToListAsync();

                return await Task.FromResult(listCreated);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public IQueryable<PreShipmentMobileDTO> GetBatchedPreShipmentForUser(string userChannelCode)
        {
            var preShipments = Context.PresShipmentMobile.AsQueryable().Where(s => s.CustomerCode == userChannelCode && s.IsBatchPickUp == true);

            var shipmentDto = (from r in preShipments
                               join co in _context.Country on r.CountryId equals co.CountryId
                               select new PreShipmentMobileDTO()
                               {
                                   PreShipmentMobileId = r.PreShipmentMobileId,
                                   Waybill = r.Waybill,
                                   DateCreated = r.DateCreated,
                                   DateModified = r.DateModified,
                                   ReceiverAddress = r.ReceiverAddress,
                                   SenderAddress = r.SenderAddress,
                                   ReceiverName = r.ReceiverName,
                                   ReceiverPhoneNumber = r.ReceiverPhoneNumber,
                                   shipmentstatus = r.shipmentstatus,
                                   GrandTotal = r.GrandTotal,
                                   CurrencySymbol = co.CurrencySymbol,
                                   IsDelivered = r.IsDelivered,
                                   IsBatchPickUp = r.IsBatchPickUp,
                                   IsCancelled = r.IsCancelled
                               });

            return shipmentDto;
        }

        public IQueryable<PreShipmentMobileDTO> GetAllBatchedPreShipment()
        {
            var preShipments = Context.PresShipmentMobile.AsQueryable().Where(s => s.IsBatchPickUp == true);

            var shipmentDto = (from r in preShipments
                               join co in _context.Country on r.CountryId equals co.CountryId
                               select new PreShipmentMobileDTO()
                               {
                                   PreShipmentMobileId = r.PreShipmentMobileId,
                                   Waybill = r.Waybill,
                                   DateCreated = r.DateCreated,
                                   DateModified = r.DateModified,
                                   ReceiverAddress = r.ReceiverAddress,
                                   SenderAddress = r.SenderAddress,
                                   ReceiverName = r.ReceiverName,
                                   ReceiverPhoneNumber = r.ReceiverPhoneNumber,
                                   shipmentstatus = r.shipmentstatus,
                                   CustomerCode = r.CustomerCode
                               });

            return shipmentDto;
        }



        public async Task<List<AddressDTO>> GetTopFiveUserAddresses(string userID,  bool isIntl)
        {
            var preShipments = Context.PresShipmentMobile.AsQueryable().Where(s => s.UserId == userID).OrderByDescending(x => x.DateCreated).GroupBy(x => x.ReceiverAddress).Take(5);
            if (isIntl)
            {
                var preShipmentsIntl = Context.PresShipmentMobile.AsQueryable().Where(s => s.UserId == userID && s.IsInternationalShipment).OrderByDescending(x => x.DateCreated).GroupBy(x => x.ReceiverAddress);
                preShipments = preShipmentsIntl;
            }

            var address = (from r in preShipments
                           select new AddressDTO()
                           {
                               ReceiverAddress = r.Key,
                               ReceiverName = r.FirstOrDefault().ReceiverName,
                               ReceiverStationName = Context.Station.FirstOrDefault(x => x.StationId == r.FirstOrDefault().ReceiverStationId).StationName,
                               ReceiverLat = Context.Location.FirstOrDefault(x => x.LocationId == r.FirstOrDefault().ReceiverLocation.LocationId).Latitude,
                               ReceiverLng = Context.Location.FirstOrDefault(x => x.LocationId == r.FirstOrDefault().ReceiverLocation.LocationId).Longitude,
                               ReceiverLGA = Context.Location.FirstOrDefault(x => x.LocationId == r.FirstOrDefault().ReceiverLocation.LocationId).LGA,
                               SenderAddress = r.FirstOrDefault().SenderAddress,
                               SenderName = r.FirstOrDefault().SenderName,
                               DateCreated = r.FirstOrDefault().DateCreated,
                               SenderStationName = Context.Station.FirstOrDefault(x => x.StationId == r.FirstOrDefault().SenderStationId).StationName,
                               SenderLat = Context.Location.FirstOrDefault(x => x.LocationId == r.FirstOrDefault().SenderLocation.LocationId).Latitude,
                               SenderLng = Context.Location.FirstOrDefault(x => x.LocationId == r.FirstOrDefault().SenderLocation.LocationId).Longitude,
                               SenderLGA = Context.Location.FirstOrDefault(x => x.LocationId == r.FirstOrDefault().SenderLocation.LocationId).LGA,
                               SenderPhoneNumber = r.FirstOrDefault().SenderPhoneNumber,
                               ReceiverCity = r.FirstOrDefault().ReceiverCity,
                               ReceiverCountry = r.FirstOrDefault().ReceiverCountry,
                               ReceiverCountryCode = r.FirstOrDefault().ReceiverCountryCode,
                               ReceiverEmail = r.FirstOrDefault().ReceiverEmail,
                               ReceiverPhoneNumber = r.FirstOrDefault().ReceiverPhoneNumber,
                               ReceiverPostalCode = r.FirstOrDefault().ReceiverPostalCode,
                               ReceiverStateOrProvinceCode = r.FirstOrDefault().ReceiverStateOrProvinceCode,
                               ReceiverStationId = r.FirstOrDefault().SenderStationId,
                               DestinationCountryId = r.FirstOrDefault().DestinationCountryId ,
                               ReceiverState = r.FirstOrDefault().ReceiverState
                           }).ToList();

            return address;
        }

        private async Task<List<OutstandingPaymentsDTO>> GetEquivalentAmountOfActiveCurrencies(List<OutstandingPaymentsDTO> result)
        {
            var usCountry = _context.Country.AsQueryable().Where(s => s.CountryId == 207).FirstOrDefault();
            var ukCountry = _context.Country.AsQueryable().Where(s => s.CountryId == 62).FirstOrDefault();
            var naijaCountry = _context.Country.AsQueryable().Where(s => s.CountryId == 1).FirstOrDefault();
            var destCountries = new List<CountryRouteZoneMap>();
            var countryIds = result.Select(x => x.CountryId);
            destCountries.AddRange(Context.CountryRouteZoneMap.Where(x => countryIds.Contains(x.DepartureId)));
            destCountries.AddRange(Context.CountryRouteZoneMap.Where(x => countryIds.Contains(x.DestinationId)));
            foreach (var i in result)
            {

                if (i.CountryId == 1)
                {
                    var usdestCountry = destCountries.Where(c => c.DepartureId == usCountry.CountryId && c.DestinationId == i.CountryId).FirstOrDefault();
                    var ukdestCountry = destCountries.Where(c => c.DepartureId == ukCountry.CountryId && c.DestinationId == i.CountryId).FirstOrDefault();
                    if (usdestCountry != null)
                    {
                        i.DollarCurrencySymbol = usCountry.CurrencySymbol;
                        i.DollarAmount = Math.Round((usdestCountry.Rate * (double)i.Amount), 2);
                        i.DollarCurrencyCode = usCountry.CurrencyCode;
                    }
                    if (ukdestCountry != null)
                    {
                        i.PoundCurrencySymbol = ukCountry.CurrencySymbol;
                        i.PoundCurrencyCode = ukCountry.CurrencyCode;
                        i.PoundAmount = Math.Round((ukdestCountry.Rate * (double)i.Amount), 2);
                    }
                    i.NairaCurrencyCode = naijaCountry.CurrencyCode;
                    i.NairaCurrencySymbol = naijaCountry.CurrencySymbol;
                    i.NairaAmount = (double)i.Amount;

                }
                else if (i.CountryId == 62)
                {
                    var naijadestCountry = destCountries.Where(c => c.DepartureId == naijaCountry.CountryId && c.DestinationId == i.CountryId).FirstOrDefault();
                    if (naijadestCountry != null)
                    {
                        i.NairaCurrencyCode = i.CountryId == 1 ? naijaCountry.CurrencyCode : naijaCountry.CurrencyCode;
                        i.NairaCurrencySymbol = i.CountryId == 1 ? naijaCountry.CurrencySymbol : naijaCountry.CurrencySymbol;
                        i.NairaAmount = i.CountryId == 1 ? (double)i.Amount : Math.Round((naijadestCountry.Rate * (double)i.Amount), 2);
                    }

                    var usdestCountry = destCountries.Where(c => c.DepartureId == usCountry.CountryId && c.DestinationId == i.CountryId).FirstOrDefault();
                    if (usdestCountry != null)
                    {
                        i.DollarCurrencySymbol = usCountry.CurrencySymbol;
                        i.DollarAmount = Math.Round((usdestCountry.Rate * (double)i.Amount), 2);
                        i.DollarCurrencyCode = usCountry.CurrencyCode;
                    }
                    i.PoundCurrencyCode = ukCountry.CurrencyCode;
                    i.PoundCurrencySymbol = ukCountry.CurrencySymbol;
                    i.PoundAmount = (double)i.Amount;
                }
                else if (i.CountryId == 207)
                {
                    var naijadestCountry = destCountries.Where(c => c.DepartureId == naijaCountry.CountryId && c.DestinationId == i.CountryId).FirstOrDefault();
                    if (naijaCountry != null)
                    {
                        i.NairaCurrencyCode = i.CountryId == 1 ? naijaCountry.CurrencyCode : naijaCountry.CurrencyCode;
                        i.NairaCurrencySymbol = i.CountryId == 1 ? naijaCountry.CurrencySymbol : naijaCountry.CurrencySymbol;
                        i.NairaAmount = i.CountryId == 1 ? (double)i.Amount : Math.Round((naijadestCountry.Rate * (double)i.Amount), 2);
                    }

                    var ukdestCountry = destCountries.Where(c => c.DepartureId == ukCountry.CountryId && c.DestinationId == i.CountryId).FirstOrDefault();
                    if (ukdestCountry != null)
                    {
                        i.PoundCurrencySymbol = ukCountry.CurrencySymbol;
                        i.PoundCurrencyCode = ukCountry.CurrencyCode;
                        i.PoundAmount = Math.Round((ukdestCountry.Rate * (double)i.Amount), 2);
                    }
                    i.DollarCurrencyCode = usCountry.CurrencyCode;
                    i.DollarCurrencySymbol = usCountry.CurrencySymbol;
                    i.DollarAmount = (double)i.Amount;
                }
            }
            return result;
        }


        public async Task<PreShipmentMobileDTO> GetPreshipmentMobileByWaybill(string waybill)
        {
            var preShipments = Context.PresShipmentMobile.AsQueryable().Where(s => s.Waybill == waybill);

            var preShipmentDTO = (from r in preShipments
                                  select new PreShipmentMobileDTO()
                                  {
                                      ReceiverAddress = r.ReceiverAddress,
                                      ReceiverName = r.ReceiverName,
                                      ReceiverStationName = Context.Station.FirstOrDefault(x => x.StationId == r.ReceiverStationId).StationName,
                                      ReceiverLat = Context.Location.FirstOrDefault(x => x.LocationId == r.ReceiverLocation.LocationId).Latitude,
                                      ReceiverLng = Context.Location.FirstOrDefault(x => x.LocationId == r.ReceiverLocation.LocationId).Longitude,
                                      SenderAddress = r.SenderAddress,
                                      SenderName = r.SenderName,
                                      DateCreated = r.DateCreated,
                                      SenderStationName = Context.Station.FirstOrDefault(x => x.StationId == r.SenderStationId).StationName,
                                      SenderLat = Context.Location.FirstOrDefault(x => x.LocationId == r.SenderLocation.LocationId).Latitude,
                                      SenderLng = Context.Location.FirstOrDefault(x => x.LocationId == r.SenderLocation.LocationId).Longitude,
                                      PreShipmentMobileId = r.PreShipmentMobileId,
                                      Waybill = r.Waybill,
                                      DateModified = r.DateModified,
                                      ReceiverPhoneNumber = r.ReceiverPhoneNumber,
                                      shipmentstatus = r.shipmentstatus,
                                      GrandTotal = r.GrandTotal,
                                      IsDelivered = r.IsDelivered,
                                      IsBatchPickUp = r.IsBatchPickUp,
                                      IsCancelled = r.IsCancelled,
                                      CustomerCode = r.CustomerCode,
                                      VehicleType = r.VehicleType,
                                      ZoneMapping = r.ZoneMapping,
                                      SenderLocality = r.SenderLocality,
                                      PreShipmentItems = _context.PresShipmentItemMobile.Where(d => d.PreShipmentMobileId == r.PreShipmentMobileId)
                                             .Select(y => new PreShipmentItemMobileDTO
                                             {
                                                 PreShipmentItemMobileId = y.PreShipmentItemMobileId,
                                                 Description = y.Description
                                             }).ToList()
                                  }).ToList();

            return preShipmentDTO.FirstOrDefault();

        }

        public async Task<CustomerDTO> GetBotUserWithPhoneNo(string phonenumber)
        {
            CustomerDTO customerDTO = new CustomerDTO();
            var IndUser = Context.IndividualCustomer.AsQueryable().Where(s => s.PhoneNumber.Contains(phonenumber)).FirstOrDefault();
            if (IndUser != null)
            {
                customerDTO = Mapper.Map<CustomerDTO>(IndUser);
                customerDTO.CustomerType = CustomerType.IndividualCustomer;
            }
            else
            {
                var ComUser = Context.Company.AsQueryable().Where(s => s.PhoneNumber.Contains(phonenumber)).FirstOrDefault();
                if (ComUser != null)
                {
                    customerDTO = Mapper.Map<CustomerDTO>(ComUser);
                    customerDTO.CustomerType = CustomerType.Company;
                }
            }
            return customerDTO;
        }
    }
}