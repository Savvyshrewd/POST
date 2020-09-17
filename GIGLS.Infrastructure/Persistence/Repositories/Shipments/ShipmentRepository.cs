﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GIGL.GIGLS.Core.Domain;
using GIGLS.Core.DTO.Shipments;
using GIGLS.Core.IRepositories.Shipments;
using GIGLS.Infrastructure.Persistence;
using GIGLS.Infrastructure.Persistence.Repository;
using System.Linq;
using GIGLS.CORE.DTO.Report;
using GIGLS.Core.Enums;
using GIGLS.CORE.Enums;
using GIGLS.Core.DTO.Zone;
using GIGLS.Core.DTO.ServiceCentres;
using GIGLS.CORE.DTO.Shipments;
using GIGLS.Core.DTO.Account;

namespace GIGLS.INFRASTRUCTURE.Persistence.Repositories.Shipments
{
    public class ShipmentRepository : Repository<Shipment, GIGLSContext>, IShipmentRepository
    {
        private GIGLSContext _context;
        public ShipmentRepository(GIGLSContext context) : base(context)
        {
            _context = context;
        }

        public Task<List<ShipmentDTO>> GetShipments(int[] serviceCentreIds)
        {
            var shipment = _context.Shipment.AsQueryable();
            if (serviceCentreIds.Length > 0)
            {
                shipment = _context.Shipment.Where(s => serviceCentreIds.Contains(s.DepartureServiceCentreId));
            }

            //filter by cancelled shipments
            shipment = shipment.Where(s => s.IsCancelled == false);


            List<ShipmentDTO> shipmentDto = (from r in shipment
                                             select new ShipmentDTO()
                                             {
                                                 ShipmentId = r.ShipmentId,
                                                 Waybill = r.Waybill,
                                                 CustomerId = r.CustomerId,
                                                 CustomerType = r.CustomerType,
                                                 ActualDateOfArrival = r.ActualDateOfArrival,
                                                 //ActualReceiverName = r.ActualReceiverName,
                                                 //ActualreceiverPhone = r.ActualreceiverPhone,
                                                 //Comments = r.Comments,
                                                 DateCreated = r.DateCreated,
                                                 DateModified = r.DateModified,
                                                 DeliveryOptionId = r.DeliveryOptionId,
                                                 DeliveryOption = new DeliveryOptionDTO
                                                 {
                                                     Code = r.DeliveryOption.Code,
                                                     Description = r.DeliveryOption.Description
                                                 },
                                                 DeliveryTime = r.DeliveryTime,
                                                 DepartureServiceCentreId = r.DepartureServiceCentreId,
                                                 DepartureServiceCentre = Context.ServiceCentre.Where(c => c.ServiceCentreId == r.DepartureServiceCentreId).Select(x => new ServiceCentreDTO
                                                 {
                                                     Code = x.Code,
                                                     Name = x.Name
                                                 }).FirstOrDefault(),
                                                 DestinationServiceCentreId = r.DestinationServiceCentreId,
                                                 DestinationServiceCentre = Context.ServiceCentre.Where(c => c.ServiceCentreId == r.DestinationServiceCentreId).Select(x => new ServiceCentreDTO
                                                 {
                                                     Code = x.Code,
                                                     Name = x.Name
                                                 }).FirstOrDefault(),
                                                 ExpectedDateOfArrival = r.ExpectedDateOfArrival,
                                                 //GroupWaybill = r.GroupWaybill,
                                                 //IdentificationType = r.IdentificationType,
                                                 //IndentificationUrl = r.IndentificationUrl,
                                                 //IsDomestic = r.IsDomestic,
                                                 PaymentStatus = r.PaymentStatus,
                                                 ReceiverAddress = r.ReceiverAddress,
                                                 ReceiverCity = r.ReceiverCity,
                                                 ReceiverCountry = r.ReceiverCountry,
                                                 ReceiverEmail = r.ReceiverEmail,
                                                 ReceiverName = r.ReceiverName,
                                                 ReceiverPhoneNumber = r.ReceiverPhoneNumber,
                                                 ReceiverState = r.ReceiverState,
                                                 SealNumber = r.SealNumber,
                                                 UserId = r.UserId,
                                                 Value = r.Value,
                                                 GrandTotal = r.GrandTotal,
                                                 AppliedDiscount = r.AppliedDiscount,
                                                 DiscountValue = r.DiscountValue,
                                                 ShipmentPackagePrice = r.ShipmentPackagePrice,
                                                 ShipmentPickupPrice = r.ShipmentPickupPrice,
                                                 CompanyType = r.CompanyType,
                                                 CustomerCode = r.CustomerCode,
                                                 Description = r.Description,
                                                 SenderAddress = r.SenderAddress,
                                                 SenderState = r.SenderState,
                                                 ApproximateItemsWeight = r.ApproximateItemsWeight,
                                                 DepartureCountryId = r.DepartureCountryId,
                                                 DestinationCountryId = r.DestinationCountryId,
                                                 CurrencyRatio = r.CurrencyRatio,

                                                 ShipmentCancel = Context.ShipmentCancel.Where(c => c.Waybill == r.Waybill).Select(x => new ShipmentCancelDTO
                                                 {
                                                     CancelReason = x.CancelReason
                                                 }).FirstOrDefault(),
                                                 //DepartureTerminalName = r.DepartureTerminal.Name,
                                                 //DestinationTerminalName = r.DestinationTerminal.Name       
                                                 //ShipmentItems = Context.ShipmentItem.Where(s => s.ShipmentId == r.ShipmentId).ToList()z
                                             }).ToList();


            return Task.FromResult(shipmentDto.ToList());
        }

        public Tuple<Task<List<ShipmentDTO>>, int> GetShipments(FilterOptionsDto filterOptionsDto, int[] serviceCentreIds)
        {
            try
            {
                //filter by service center
                var shipment = _context.Shipment.AsQueryable();
                if (serviceCentreIds.Length > 0)
                {
                    shipment = _context.Shipment.Where(s => serviceCentreIds.Contains(s.DepartureServiceCentreId));
                }

                //filter by country Id
                if (filterOptionsDto.CountryId > 0)
                {
                    shipment = shipment.Where(s => s.DepartureCountryId == filterOptionsDto.CountryId);
                }

                //filter by cancelled shipments
                shipment = shipment.Where(s => s.IsCancelled == false);

                //filter by Local or International Shipment
                if (filterOptionsDto.IsInternational != null)
                {
                    shipment = shipment.Where(s => s.IsInternational == filterOptionsDto.IsInternational);
                }

                var count = 0;

                List<ShipmentDTO> shipmentDto = new List<ShipmentDTO>();

                //filter
                var filter = filterOptionsDto.filter;
                var filterValue = filterOptionsDto.filterValue;
                if (string.IsNullOrEmpty(filter) || string.IsNullOrEmpty(filterValue))
                {
                    shipmentDto = (from r in shipment
                                   select new ShipmentDTO()
                                   {
                                       ShipmentId = r.ShipmentId,
                                       Waybill = r.Waybill,
                                       CustomerId = r.CustomerId,
                                       CustomerType = r.CustomerType,
                                       ActualDateOfArrival = r.ActualDateOfArrival,
                                       DateCreated = r.DateCreated,
                                       DateModified = r.DateModified,
                                       DeliveryOptionId = r.DeliveryOptionId,
                                       DeliveryOption = new DeliveryOptionDTO
                                       {
                                           Code = r.DeliveryOption.Code,
                                           Description = r.DeliveryOption.Description
                                       },
                                       DeliveryTime = r.DeliveryTime,
                                       DepartureServiceCentreId = r.DepartureServiceCentreId,
                                       DepartureServiceCentre = Context.ServiceCentre.Where(c => c.ServiceCentreId == r.DepartureServiceCentreId).Select(x => new ServiceCentreDTO
                                       {
                                           Code = x.Code,
                                           Name = x.Name
                                       }).FirstOrDefault(),
                                       DestinationServiceCentreId = r.DestinationServiceCentreId,
                                       DestinationServiceCentre = Context.ServiceCentre.Where(c => c.ServiceCentreId == r.DestinationServiceCentreId).Select(x => new ServiceCentreDTO
                                       {
                                           Code = x.Code,
                                           Name = x.Name
                                       }).FirstOrDefault(),
                                       ExpectedDateOfArrival = r.ExpectedDateOfArrival,
                                       PaymentStatus = r.PaymentStatus,
                                       ReceiverAddress = r.ReceiverAddress,
                                       ReceiverCity = r.ReceiverCity,
                                       ReceiverCountry = r.ReceiverCountry,
                                       ReceiverEmail = r.ReceiverEmail,
                                       ReceiverName = r.ReceiverName,
                                       ReceiverPhoneNumber = r.ReceiverPhoneNumber,
                                       ReceiverState = r.ReceiverState,
                                       SealNumber = r.SealNumber,
                                       UserId = r.UserId,
                                       Value = r.Value,
                                       GrandTotal = r.GrandTotal,
                                       AppliedDiscount = r.AppliedDiscount,
                                       DiscountValue = r.DiscountValue,
                                       ShipmentPackagePrice = r.ShipmentPackagePrice,
                                       ShipmentPickupPrice = r.ShipmentPickupPrice,
                                       CompanyType = r.CompanyType,
                                       CustomerCode = r.CustomerCode,
                                       Description = r.Description,
                                       SenderAddress = r.SenderAddress,
                                       SenderState = r.SenderState,
                                       ReprintCounterStatus = r.ReprintCounterStatus,
                                       ApproximateItemsWeight = r.ApproximateItemsWeight,
                                       DepartureCountryId = r.DepartureCountryId,
                                       DestinationCountryId = r.DestinationCountryId,
                                       CurrencyRatio = r.CurrencyRatio,
                                       ShipmentCancel = Context.ShipmentCancel.Where(c => c.Waybill == r.Waybill).Select(x => new ShipmentCancelDTO
                                       {
                                           CancelReason = x.CancelReason
                                       }).FirstOrDefault(),
                                       Invoice = Context.Invoice.Where(c => c.Waybill == r.Waybill).Select(x => new InvoiceDTO
                                       {
                                           InvoiceId = x.InvoiceId,
                                           InvoiceNo = x.InvoiceNo,
                                           Amount = x.Amount,
                                           PaymentStatus = x.PaymentStatus,
                                           PaymentMethod = x.PaymentMethod,
                                           PaymentDate = x.PaymentDate,
                                           Waybill = x.Waybill,
                                           DueDate = x.DueDate,
                                           IsInternational = x.IsInternational
                                       }).FirstOrDefault()
                                   }).OrderByDescending(x => x.DateCreated).Take(10).ToList();

                    count = shipmentDto.Count();

                    return new Tuple<Task<List<ShipmentDTO>>, int>(Task.FromResult(shipmentDto), count);
                }

                shipmentDto = (from r in shipment
                               select new ShipmentDTO()
                               {
                                   ShipmentId = r.ShipmentId,
                                   Waybill = r.Waybill,
                                   CustomerId = r.CustomerId,
                                   CustomerType = r.CustomerType,
                                   ActualDateOfArrival = r.ActualDateOfArrival,
                                   DateCreated = r.DateCreated,
                                   DateModified = r.DateModified,
                                   DeliveryOptionId = r.DeliveryOptionId,
                                   DeliveryOption = new DeliveryOptionDTO
                                   {
                                       Code = r.DeliveryOption.Code,
                                       Description = r.DeliveryOption.Description
                                   },
                                   DeliveryTime = r.DeliveryTime,
                                   DepartureServiceCentreId = r.DepartureServiceCentreId,
                                   DepartureServiceCentre = Context.ServiceCentre.Where(c => c.ServiceCentreId == r.DepartureServiceCentreId).Select(x => new ServiceCentreDTO
                                   {
                                       Code = x.Code,
                                       Name = x.Name
                                   }).FirstOrDefault(),
                                   DestinationServiceCentreId = r.DestinationServiceCentreId,
                                   DestinationServiceCentre = Context.ServiceCentre.Where(c => c.ServiceCentreId == r.DestinationServiceCentreId).Select(x => new ServiceCentreDTO
                                   {
                                       Code = x.Code,
                                       Name = x.Name
                                   }).FirstOrDefault(),
                                   ExpectedDateOfArrival = r.ExpectedDateOfArrival,
                                   PaymentStatus = r.PaymentStatus,
                                   ReceiverAddress = r.ReceiverAddress,
                                   ReceiverCity = r.ReceiverCity,
                                   ReceiverCountry = r.ReceiverCountry,
                                   ReceiverEmail = r.ReceiverEmail,
                                   ReceiverName = r.ReceiverName,
                                   ReceiverPhoneNumber = r.ReceiverPhoneNumber,
                                   ReceiverState = r.ReceiverState,
                                   SealNumber = r.SealNumber,
                                   UserId = r.UserId,
                                   Value = r.Value,
                                   GrandTotal = r.GrandTotal,
                                   AppliedDiscount = r.AppliedDiscount,
                                   DiscountValue = r.DiscountValue,
                                   ShipmentPackagePrice = r.ShipmentPackagePrice,
                                   ShipmentPickupPrice = r.ShipmentPickupPrice,
                                   CompanyType = r.CompanyType,
                                   CustomerCode = r.CustomerCode,
                                   Description = r.Description,
                                   SenderAddress = r.SenderAddress,
                                   SenderState = r.SenderState,
                                   ApproximateItemsWeight = r.ApproximateItemsWeight,
                                   DepartureCountryId = r.DepartureCountryId,
                                   DestinationCountryId = r.DestinationCountryId,
                                   CurrencyRatio = r.CurrencyRatio,
                                   ShipmentCancel = Context.ShipmentCancel.Where(c => c.Waybill == r.Waybill).Select(x => new ShipmentCancelDTO
                                   {
                                       CancelReason = x.CancelReason
                                   }).FirstOrDefault(),
                                   Invoice = Context.Invoice.Where(c => c.Waybill == r.Waybill).Select(x => new InvoiceDTO
                                   {
                                       InvoiceId = x.InvoiceId,
                                       InvoiceNo = x.InvoiceNo,
                                       Amount = x.Amount,
                                       PaymentStatus = x.PaymentStatus,
                                       PaymentMethod = x.PaymentMethod,
                                       PaymentDate = x.PaymentDate,
                                       Waybill = x.Waybill,
                                       DueDate = x.DueDate,
                                       IsInternational = x.IsInternational
                                   }).FirstOrDefault()
                               }).Where(s => (s.Waybill == filterValue || s.GrandTotal.ToString() == filterValue || s.DateCreated.ToString() == filterValue)).ToList();

                //filter
                if (!string.IsNullOrEmpty(filter) && !string.IsNullOrEmpty(filterValue))
                {
                    shipmentDto = shipmentDto.Where(s => (s.GetType().GetProperty(filter).GetValue(s)).ToString().Contains(filterValue)).ToList();
                }

                //sort
                var sortorder = filterOptionsDto.sortorder;
                var sortvalue = filterOptionsDto.sortvalue;

                if (!string.IsNullOrEmpty(sortorder) && !string.IsNullOrEmpty(sortvalue))
                {
                    System.Reflection.PropertyInfo prop = typeof(Shipment).GetProperty(sortvalue);

                    if (sortorder == "0")
                    {
                        shipmentDto = shipmentDto.OrderBy(x => x.GetType().GetProperty(prop.Name).GetValue(x)).ToList();
                    }
                    else
                    {
                        shipmentDto = shipmentDto.OrderByDescending(x => x.GetType().GetProperty(prop.Name).GetValue(x)).ToList();
                    }
                }

                shipmentDto = shipmentDto.OrderByDescending(x => x.DateCreated).Skip(filterOptionsDto.count * (filterOptionsDto.page - 1)).Take(filterOptionsDto.count).ToList();
                count = shipmentDto.Count();
                return new Tuple<Task<List<ShipmentDTO>>, int>(Task.FromResult(shipmentDto), count);
            }
            catch (Exception)
            {
                throw;
            }
        }

        //public Tuple<Task<List<IntlShipmentRequestDTO>>, int> GetIntlTransactionShipmentRequest(FilterOptionsDto filterOptionsDto, int[] serviceCentreIds)
        //{
        //    try
        //    {
        //        //filter by service center
        //        var shipmentRequest = _context.IntlShipmentRequest.AsQueryable();
        //        shipmentRequest = _context.IntlShipmentRequest.Where(s => serviceCentreIds.Contains(s.DepartureServiceCentreId));

        //        var count = 0;

        //        List<IntlShipmentRequestDTO> intlShipmentRequestDTO = new List<IntlShipmentRequestDTO>();

        //        //filter
        //        var filter = filterOptionsDto.filter;
        //        var filterValue = filterOptionsDto.filterValue;
        //        if (string.IsNullOrEmpty(filter) || string.IsNullOrEmpty(filterValue))
        //        {
        //            intlShipmentRequestDTO = (from r in shipmentRequest
        //                                      select new IntlShipmentRequestDTO()
        //                                      {
        //                                          ShipmentId = r.IntlShipmentRequestId,
        //                                          RequestNumber = r.RequestNumber,
        //                                          CustomerId = r.CustomerId,
        //                                          CustomerType = r.CustomerType,
        //                                          DateCreated = r.DateCreated,
        //                                          DateModified = r.DateModified,
        //                                          DeliveryOptionId = r.DeliveryOptionId,
        //                                          DeliveryOption = new DeliveryOptionDTO
        //                                          {
        //                                              Code = r.DeliveryOption.Code,
        //                                              Description = r.DeliveryOption.Description
        //                                          },
        //                                          DepartureServiceCentreId = r.DepartureServiceCentreId,
        //                                          DepartureServiceCentre = Context.ServiceCentre.Where(c => c.ServiceCentreId == r.DepartureServiceCentreId).Select(x => new ServiceCentreDTO
        //                                          {
        //                                              Code = x.Code,
        //                                              Name = x.Name
        //                                          }).FirstOrDefault(),
        //                                          DestinationServiceCentreId = r.DestinationServiceCentreId,
        //                                          DestinationServiceCentre = Context.ServiceCentre.Where(c => c.ServiceCentreId == r.DestinationServiceCentreId).Select(x => new ServiceCentreDTO
        //                                          {
        //                                              Code = x.Code,
        //                                              Name = x.Name
        //                                          }).FirstOrDefault(),
        //                                          ReceiverAddress = r.ReceiverAddress,
        //                                          ReceiverCity = r.ReceiverCity,
        //                                          ReceiverCountry = r.ReceiverCountry,
        //                                          ReceiverEmail = r.ReceiverEmail,
        //                                          ReceiverName = r.ReceiverName,
        //                                          ReceiverPhoneNumber = r.ReceiverPhoneNumber,
        //                                          ReceiverState = r.ReceiverState,
        //                                          UserId = r.UserId,
        //                                          Value = r.Value,
        //                                          GrandTotal = r.GrandTotal,

        //                                          Description = r.Description,
        //                                          SenderAddress = r.SenderAddress,
        //                                          SenderState = r.SenderState,
        //                                          ApproximateItemsWeight = r.ApproximateItemsWeight,
        //                                          DepartureCountryId = r.DepartureCountryId,
        //                                          DestinationCountryId = r.DestinationCountryId,

        //                                      }).OrderByDescending(x => x.DateCreated).Take(10).ToList();

        //            count = intlShipmentRequestDTO.Count(); 

        //            return new Tuple<Task<List<IntlShipmentRequestDTO>>, int>(Task.FromResult(intlShipmentRequestDTO), count); 
        //        }

        //        intlShipmentRequestDTO = (from r in shipmentRequest
        //                                  select new IntlShipmentRequestDTO()
        //                                  {
        //                                      ShipmentId = r.IntlShipmentRequestId,
        //                                      RequestNumber = r.RequestNumber,
        //                                      CustomerId = r.CustomerId,
        //                                      CustomerType = r.CustomerType,
        //                                      DateCreated = r.DateCreated,
        //                                      DateModified = r.DateModified,
        //                                      DeliveryOptionId = r.DeliveryOptionId,
        //                                      DeliveryOption = new DeliveryOptionDTO
        //                                      {
        //                                          Code = r.DeliveryOption.Code,
        //                                          Description = r.DeliveryOption.Description
        //                                      },
        //                                      DepartureServiceCentreId = r.DepartureServiceCentreId,
        //                                      DepartureServiceCentre = Context.ServiceCentre.Where(c => c.ServiceCentreId == r.DepartureServiceCentreId).Select(x => new ServiceCentreDTO
        //                                      {
        //                                          Code = x.Code,
        //                                          Name = x.Name
        //                                      }).FirstOrDefault(),
        //                                      DestinationServiceCentreId = r.DestinationServiceCentreId,
        //                                      DestinationServiceCentre = Context.ServiceCentre.Where(c => c.ServiceCentreId == r.DestinationServiceCentreId).Select(x => new ServiceCentreDTO
        //                                      {
        //                                          Code = x.Code,
        //                                          Name = x.Name
        //                                      }).FirstOrDefault(),
        //                                      PaymentStatus = r.PaymentStatus,
        //                                      ReceiverAddress = r.ReceiverAddress,
        //                                      ReceiverCity = r.ReceiverCity,
        //                                      ReceiverCountry = r.ReceiverCountry,
        //                                      ReceiverEmail = r.ReceiverEmail,
        //                                      ReceiverName = r.ReceiverName,
        //                                      ReceiverPhoneNumber = r.ReceiverPhoneNumber,
        //                                      ReceiverState = r.ReceiverState,
        //                                      UserId = r.UserId,
        //                                      Value = r.Value,
        //                                      GrandTotal = r.GrandTotal,

        //                                      Description = r.Description,
        //                                      SenderAddress = r.SenderAddress,
        //                                      SenderState = r.SenderState,
        //                                      ApproximateItemsWeight = r.ApproximateItemsWeight,
        //                                      DepartureCountryId = r.DepartureCountryId,
        //                                      DestinationCountryId = r.DestinationCountryId,

        //                                  }).Where(s => (s.RequestNumber == filterValue || s.GrandTotal.ToString() == filterValue || s.DateCreated.ToString() == filterValue)).ToList();

        //        //filter
        //        if (!string.IsNullOrEmpty(filter) && !string.IsNullOrEmpty(filterValue))
        //        {
        //            intlShipmentRequestDTO = intlShipmentRequestDTO.Where(s => (s.GetType().GetProperty(filter).GetValue(s)).ToString().Contains(filterValue)).ToList();
        //        }

        //        //sort
        //        var sortorder = filterOptionsDto.sortorder;
        //        var sortvalue = filterOptionsDto.sortvalue;

        //        if (!string.IsNullOrEmpty(sortorder) && !string.IsNullOrEmpty(sortvalue))
        //        {
        //            System.Reflection.PropertyInfo prop = typeof(Shipment).GetProperty(sortvalue);

        //            if (sortorder == "0")
        //            {
        //                intlShipmentRequestDTO = intlShipmentRequestDTO.OrderBy(x => x.GetType().GetProperty(prop.Name).GetValue(x)).ToList();
        //            }
        //            else
        //            {
        //                intlShipmentRequestDTO = intlShipmentRequestDTO.OrderByDescending(x => x.GetType().GetProperty(prop.Name).GetValue(x)).ToList();
        //            }
        //        }

        //        intlShipmentRequestDTO = intlShipmentRequestDTO.OrderByDescending(x => x.DateCreated).Skip(filterOptionsDto.count * (filterOptionsDto.page - 1)).Take(filterOptionsDto.count).ToList();
        //        count = intlShipmentRequestDTO.Count();
        //        return new Tuple<Task<List<IntlShipmentRequestDTO>>, int>(Task.FromResult(intlShipmentRequestDTO), count);
        //    }
        //    catch (Exception)
        //    {
        //        throw;
        //    }
        //}

        public Tuple<Task<List<ShipmentDTO>>, int> GetDestinationShipments(FilterOptionsDto filterOptionsDto, int[] serviceCentreIds)
        {
            try
            {
                //filter by destination service center that is not cancelled
                var shipment = _context.Shipment.AsQueryable().Where(x => x.IsCancelled == false);
                if (serviceCentreIds.Length > 0)
                {
                    shipment = _context.Shipment.Where(s => serviceCentreIds.Contains(s.DestinationServiceCentreId));
                }

                //filter by Local or International Shipment
                if (filterOptionsDto.IsInternational != null)
                {
                    shipment = shipment.Where(s => s.IsInternational == filterOptionsDto.IsInternational);
                }

                var count = shipment.ToList().Count();

                List<ShipmentDTO> shipmentDto = (from r in shipment
                                                 select new ShipmentDTO()
                                                 {
                                                     ShipmentId = r.ShipmentId,
                                                     Waybill = r.Waybill,
                                                     CustomerId = r.CustomerId,
                                                     CustomerType = r.CustomerType,
                                                     ActualDateOfArrival = r.ActualDateOfArrival,
                                                     DateCreated = r.DateCreated,
                                                     DateModified = r.DateModified,
                                                     DeliveryOptionId = r.DeliveryOptionId,
                                                     DeliveryOption = new DeliveryOptionDTO
                                                     {
                                                         Code = r.DeliveryOption.Code,
                                                         Description = r.DeliveryOption.Description
                                                     },
                                                     DeliveryTime = r.DeliveryTime,
                                                     DepartureServiceCentreId = r.DepartureServiceCentreId,
                                                     DepartureServiceCentre = Context.ServiceCentre.Where(c => c.ServiceCentreId == r.DepartureServiceCentreId).Select(x => new ServiceCentreDTO
                                                     {
                                                         Code = x.Code,
                                                         Name = x.Name
                                                     }).FirstOrDefault(),
                                                     DestinationServiceCentreId = r.DestinationServiceCentreId,
                                                     DestinationServiceCentre = Context.ServiceCentre.Where(c => c.ServiceCentreId == r.DestinationServiceCentreId).Select(x => new ServiceCentreDTO
                                                     {
                                                         Code = x.Code,
                                                         Name = x.Name
                                                     }).FirstOrDefault(),

                                                     ExpectedDateOfArrival = r.ExpectedDateOfArrival,
                                                     PaymentStatus = r.PaymentStatus,
                                                     ReceiverAddress = r.ReceiverAddress,
                                                     ReceiverCity = r.ReceiverCity,
                                                     ReceiverCountry = r.ReceiverCountry,
                                                     ReceiverEmail = r.ReceiverEmail,
                                                     ReceiverName = r.ReceiverName,
                                                     ReceiverPhoneNumber = r.ReceiverPhoneNumber,
                                                     ReceiverState = r.ReceiverState,
                                                     SealNumber = r.SealNumber,
                                                     UserId = r.UserId,
                                                     Value = r.Value,
                                                     GrandTotal = r.GrandTotal,
                                                     AppliedDiscount = r.AppliedDiscount,
                                                     DiscountValue = r.DiscountValue,
                                                     ShipmentPackagePrice = r.ShipmentPackagePrice,
                                                     ShipmentPickupPrice = r.ShipmentPickupPrice,
                                                     CompanyType = r.CompanyType,
                                                     CustomerCode = r.CustomerCode,
                                                     Description = r.Description,
                                                     SenderAddress = r.SenderAddress,
                                                     SenderState = r.SenderState,
                                                     ApproximateItemsWeight = r.ApproximateItemsWeight,
                                                     DepartureCountryId = r.DepartureCountryId,
                                                     DestinationCountryId = r.DestinationCountryId,
                                                     CurrencyRatio = r.CurrencyRatio,
                                                 }).ToList();

                //filter
                var filter = filterOptionsDto.filter;
                var filterValue = filterOptionsDto.filterValue;
                if (!string.IsNullOrEmpty(filter) && !string.IsNullOrEmpty(filterValue))
                {
                    shipmentDto = shipmentDto.Where(s => (s.GetType().GetProperty(filter).GetValue(s)).ToString() == filterValue).ToList();
                }

                //sort
                var sortorder = filterOptionsDto.sortorder;
                var sortvalue = filterOptionsDto.sortvalue;

                if (!string.IsNullOrEmpty(sortorder) && !string.IsNullOrEmpty(sortvalue))
                {
                    System.Reflection.PropertyInfo prop = typeof(Shipment).GetProperty(sortvalue);

                    if (sortorder == "0")
                    {
                        shipmentDto = shipmentDto.OrderBy(x => x.GetType().GetProperty(prop.Name).GetValue(x)).ToList();
                    }
                    else
                    {
                        shipmentDto = shipmentDto.OrderByDescending(x => x.GetType().GetProperty(prop.Name).GetValue(x)).ToList();
                    }
                }

                shipmentDto = shipmentDto.OrderByDescending(x => x.DateCreated).Skip(filterOptionsDto.count * (filterOptionsDto.page - 1)).Take(filterOptionsDto.count).ToList();

                return new Tuple<Task<List<ShipmentDTO>>, int>(Task.FromResult(shipmentDto.ToList()), count);
            }
            catch (Exception)
            {
                throw;
            }
        }

        //Get Shipment Detail for list of waybills and filter it by service centre
        public Tuple<Task<List<ShipmentDTO>>, int> GetShipmentDetailByWaybills(FilterOptionsDto filterOptionsDto, int[] serviceCentreIds, List<string> waybills)
        {
            try
            {
                //filter by destination service center that is not cancelled
                var shipment = _context.Shipment.AsQueryable().Where(x => x.IsCancelled == false);

                if (serviceCentreIds.Length > 0)
                {
                    shipment = shipment.Where(s => serviceCentreIds.Contains(s.DestinationServiceCentreId));
                }

                //filter by Local or International Shipment
                if (filterOptionsDto.IsInternational != null)
                {
                    shipment = shipment.Where(s => s.IsInternational == filterOptionsDto.IsInternational);
                }

                if (waybills.Count > 0)
                {
                    shipment = shipment.Where(s => waybills.Contains(s.Waybill));
                }

                var count = shipment.ToList().Count();

                List<ShipmentDTO> shipmentDto = (from r in shipment
                                                 select new ShipmentDTO()
                                                 {
                                                     ShipmentId = r.ShipmentId,
                                                     Waybill = r.Waybill,
                                                     CustomerId = r.CustomerId,
                                                     CustomerType = r.CustomerType,
                                                     ActualDateOfArrival = r.ActualDateOfArrival,
                                                     DateCreated = r.DateCreated,
                                                     DateModified = r.DateModified,
                                                     DeliveryOptionId = r.DeliveryOptionId,
                                                     DeliveryOption = new DeliveryOptionDTO
                                                     {
                                                         Code = r.DeliveryOption.Code,
                                                         Description = r.DeliveryOption.Description
                                                     },
                                                     DeliveryTime = r.DeliveryTime,
                                                     DepartureServiceCentreId = r.DepartureServiceCentreId,
                                                     DepartureServiceCentre = Context.ServiceCentre.Where(c => c.ServiceCentreId == r.DepartureServiceCentreId).Select(x => new ServiceCentreDTO
                                                     {
                                                         Code = x.Code,
                                                         Name = x.Name
                                                     }).FirstOrDefault(),
                                                     DestinationServiceCentreId = r.DestinationServiceCentreId,
                                                     DestinationServiceCentre = Context.ServiceCentre.Where(c => c.ServiceCentreId == r.DestinationServiceCentreId).Select(x => new ServiceCentreDTO
                                                     {
                                                         Code = x.Code,
                                                         Name = x.Name
                                                     }).FirstOrDefault(),

                                                     ExpectedDateOfArrival = r.ExpectedDateOfArrival,
                                                     PaymentStatus = r.PaymentStatus,
                                                     ReceiverAddress = r.ReceiverAddress,
                                                     ReceiverCity = r.ReceiverCity,
                                                     ReceiverCountry = r.ReceiverCountry,
                                                     ReceiverEmail = r.ReceiverEmail,
                                                     ReceiverName = r.ReceiverName,
                                                     ReceiverPhoneNumber = r.ReceiverPhoneNumber,
                                                     ReceiverState = r.ReceiverState,
                                                     SealNumber = r.SealNumber,
                                                     UserId = r.UserId,
                                                     Value = r.Value,
                                                     GrandTotal = r.GrandTotal,
                                                     AppliedDiscount = r.AppliedDiscount,
                                                     DiscountValue = r.DiscountValue,
                                                     ShipmentPackagePrice = r.ShipmentPackagePrice,
                                                     ShipmentPickupPrice = r.ShipmentPickupPrice,
                                                     CompanyType = r.CompanyType,
                                                     CustomerCode = r.CustomerCode,
                                                     Description = r.Description,
                                                     SenderAddress = r.SenderAddress,
                                                     SenderState = r.SenderState,
                                                     ApproximateItemsWeight = r.ApproximateItemsWeight,
                                                     DepartureCountryId = r.DepartureCountryId,
                                                     DestinationCountryId = r.DestinationCountryId,
                                                     CurrencyRatio = r.CurrencyRatio,
                                                 }).ToList();

                //filter
                var filter = filterOptionsDto.filter;
                var filterValue = filterOptionsDto.filterValue;
                if (!string.IsNullOrEmpty(filter) && !string.IsNullOrEmpty(filterValue))
                {
                    shipmentDto = shipmentDto.Where(s => (s.GetType().GetProperty(filter).GetValue(s)).ToString() == filterValue).ToList();
                }

                //sort
                var sortorder = filterOptionsDto.sortorder;
                var sortvalue = filterOptionsDto.sortvalue;

                if (!string.IsNullOrEmpty(sortorder) && !string.IsNullOrEmpty(sortvalue))
                {
                    System.Reflection.PropertyInfo prop = typeof(Shipment).GetProperty(sortvalue);

                    if (sortorder == "0")
                    {
                        shipmentDto = shipmentDto.OrderBy(x => x.GetType().GetProperty(prop.Name).GetValue(x)).ToList();
                    }
                    else
                    {
                        shipmentDto = shipmentDto.OrderByDescending(x => x.GetType().GetProperty(prop.Name).GetValue(x)).ToList();
                    }
                }

                shipmentDto = shipmentDto.OrderByDescending(x => x.DateCreated).Skip(filterOptionsDto.count * (filterOptionsDto.page - 1)).Take(filterOptionsDto.count).ToList();

                return new Tuple<Task<List<ShipmentDTO>>, int>(Task.FromResult(shipmentDto.ToList()), count);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public Task<List<ShipmentDTO>> GetShipments(ShipmentFilterCriteria f_Criteria, int[] serviceCentreIds)
        {
            DateTime StartDate = f_Criteria.StartDate.GetValueOrDefault().Date;
            DateTime EndDate = f_Criteria.EndDate?.Date ?? StartDate;

            //filter by service center
            var shipments = _context.Shipment.AsQueryable();
            if (serviceCentreIds.Length > 0)
            {
                shipments = _context.Shipment.Where(s => serviceCentreIds.Contains(s.DepartureServiceCentreId));
            }
            ////

            //filter by cancelled shipments
            shipments = shipments.Where(s => s.IsCancelled == false);

            //get startDate and endDate
            var queryDate = f_Criteria.getStartDateAndEndDate();
            var startDate = queryDate.Item1;
            var endDate = queryDate.Item2;
            shipments = shipments.Where(x => x.DateCreated >= startDate && x.DateCreated < endDate);

            if (f_Criteria.ServiceCentreId > 0)
            {
                shipments = shipments.Where(x => x.DepartureServiceCentreId == f_Criteria.ServiceCentreId);
            }

            if (f_Criteria.StationId > 0)
            {
                var serviceCentre = Context.ServiceCentre.Where(y => y.StationId == f_Criteria.StationId);
                shipments = shipments.Where(y => serviceCentre.Any(x => y.DepartureServiceCentreId == x.ServiceCentreId));
            }

            if (f_Criteria.StateId > 0)
            {
                var station = Context.Station.Where(s => s.StateId == f_Criteria.StateId);
                var serviceCentre = Context.ServiceCentre.Where(w => station.Any(x => w.StationId == x.StationId));
                shipments = shipments.Where(y => serviceCentre.Any(x => y.DepartureServiceCentreId == x.ServiceCentreId));
            }

            if (!string.IsNullOrWhiteSpace(f_Criteria.UserId))
            {
                shipments = shipments.Where(x => x.UserId == f_Criteria.UserId);
            }

            if (f_Criteria.FilterCustomerType.HasValue && f_Criteria.FilterCustomerType.Equals(FilterCustomerType.IndividualCustomer))
            {
                shipments = shipments.Where(x => x.CustomerType.Equals(CustomerType.IndividualCustomer.ToString()));
            }

            if (f_Criteria.FilterCustomerType.HasValue && !f_Criteria.FilterCustomerType.Equals(FilterCustomerType.IndividualCustomer))
            {
                shipments = shipments.Where(x => x.CustomerType.Equals(CustomerType.Company.ToString()));

                if (f_Criteria.FilterCustomerType.Equals(FilterCustomerType.Corporate))
                {
                    var corporate = Context.Company.Where(x => x.CompanyType == CompanyType.Corporate);
                    shipments = shipments.Where(y => corporate.Any(x => y.CustomerId == x.CompanyId));
                }
                else
                {
                    var ecommerce = Context.Company.Where(x => x.CompanyType == CompanyType.Ecommerce);
                    shipments = shipments.Where(y => ecommerce.Any(x => y.CustomerId == x.CompanyId));
                }
            }

            List<ShipmentDTO> shipmentDto = (from r in shipments
                                             select new ShipmentDTO()
                                             {
                                                 ShipmentId = r.ShipmentId,
                                                 Waybill = r.Waybill,
                                                 CustomerId = r.CustomerId,
                                                 CustomerType = r.CustomerType,
                                                 ActualDateOfArrival = r.ActualDateOfArrival,
                                                 DateCreated = r.DateCreated,
                                                 DateModified = r.DateModified,
                                                 DeliveryOptionId = r.DeliveryOptionId,
                                                 DeliveryOption = new DeliveryOptionDTO
                                                 {
                                                     Code = r.DeliveryOption.Code,
                                                     Description = r.DeliveryOption.Description
                                                 },
                                                 DeliveryTime = r.DeliveryTime,
                                                 DepartureServiceCentreId = r.DepartureServiceCentreId,
                                                 DepartureServiceCentre = Context.ServiceCentre.Where(c => c.ServiceCentreId == r.DepartureServiceCentreId).Select(x => new ServiceCentreDTO
                                                 {
                                                     Code = x.Code,
                                                     Name = x.Name
                                                 }).FirstOrDefault(),

                                                 DestinationServiceCentreId = r.DestinationServiceCentreId,
                                                 DestinationServiceCentre = Context.ServiceCentre.Where(c => c.ServiceCentreId == r.DestinationServiceCentreId).Select(x => new ServiceCentreDTO
                                                 {
                                                     Code = x.Code,
                                                     Name = x.Name
                                                 }).FirstOrDefault(),

                                                 ExpectedDateOfArrival = r.ExpectedDateOfArrival,
                                                 PaymentStatus = r.PaymentStatus,
                                                 ReceiverAddress = r.ReceiverAddress,
                                                 ReceiverCity = r.ReceiverCity,
                                                 ReceiverCountry = r.ReceiverCountry,
                                                 ReceiverEmail = r.ReceiverEmail,
                                                 ReceiverName = r.ReceiverName,
                                                 ReceiverPhoneNumber = r.ReceiverPhoneNumber,
                                                 ReceiverState = r.ReceiverState,
                                                 SealNumber = r.SealNumber,
                                                 UserId = r.UserId,
                                                 Value = r.Value,
                                                 GrandTotal = r.GrandTotal,
                                                 AppliedDiscount = r.AppliedDiscount,
                                                 DiscountValue = r.DiscountValue,
                                                 ShipmentPackagePrice = r.ShipmentPackagePrice,
                                                 ShipmentPickupPrice = r.ShipmentPickupPrice,
                                                 CompanyType = r.CompanyType,
                                                 CustomerCode = r.CustomerCode,
                                                 Description = r.Description,
                                                 SenderAddress = r.SenderAddress,
                                                 SenderState = r.SenderState,
                                                 ApproximateItemsWeight = r.ApproximateItemsWeight,
                                                 DepartureCountryId = r.DepartureCountryId,
                                                 DestinationCountryId = r.DestinationCountryId,
                                                 CurrencyRatio = r.CurrencyRatio,
                                                 ShipmentCancel = Context.ShipmentCancel.Where(c => c.Waybill == r.Waybill).Select(x => new ShipmentCancelDTO
                                                 {
                                                     CancelReason = x.CancelReason
                                                 }).FirstOrDefault()
                                             }).ToList();

            return Task.FromResult(shipmentDto.OrderByDescending(x => x.DateCreated).ToList());
        }

        public Task<List<ShipmentDTO>> GetCustomerShipments(ShipmentFilterCriteria f_Criteria)
        {
            DateTime StartDate = f_Criteria.StartDate.GetValueOrDefault().Date;
            DateTime EndDate = f_Criteria.EndDate?.Date ?? StartDate;

            //filter by service center
            var shipments = _context.Shipment.AsQueryable();

            //filter by cancelled shipments
            shipments = shipments.Where(s => s.IsCancelled == false);

            //If No Date Supply
            if (!f_Criteria.StartDate.HasValue && !f_Criteria.EndDate.HasValue)
            {
                var Today = DateTime.Today;
                var nextDay = DateTime.Today.AddDays(1).Date;
                shipments = shipments.Where(x => x.DateCreated >= Today && x.DateCreated < nextDay);
            }

            if (f_Criteria.StartDate.HasValue && f_Criteria.EndDate.HasValue)
            {
                if (f_Criteria.StartDate.Equals(f_Criteria.EndDate))
                {
                    var nextDay = DateTime.Today.AddDays(1).Date;
                    shipments = shipments.Where(x => x.DateCreated >= StartDate && x.DateCreated < nextDay);
                }
                else
                {
                    var dayAfterEndDate = EndDate.AddDays(1).Date;
                    shipments = shipments.Where(x => x.DateCreated >= StartDate && x.DateCreated < dayAfterEndDate);
                }
            }

            if (f_Criteria.StartDate.HasValue && !f_Criteria.EndDate.HasValue)
            {
                var nextDay = DateTime.Today.AddDays(1).Date;
                shipments = shipments.Where(x => x.DateCreated >= StartDate && x.DateCreated < nextDay);
            }

            if (f_Criteria.EndDate.HasValue && !f_Criteria.StartDate.HasValue)
            {
                var dayAfterEndDate = EndDate.AddDays(1).Date;
                shipments = shipments.Where(x => x.DateCreated < dayAfterEndDate);
            }

            if (f_Criteria.ServiceCentreId > 0)
            {
                shipments = shipments.Where(x => x.DepartureServiceCentreId == f_Criteria.ServiceCentreId);
            }

            if (f_Criteria.StationId > 0)
            {
                var serviceCentre = Context.ServiceCentre.Where(y => y.StationId == f_Criteria.StationId);
                shipments = shipments.Where(y => serviceCentre.Any(x => y.DepartureServiceCentreId == x.ServiceCentreId));
            }

            if (f_Criteria.StateId > 0)
            {
                var station = Context.Station.Where(s => s.StateId == f_Criteria.StateId);
                var serviceCentre = Context.ServiceCentre.Where(w => station.Any(x => w.StationId == x.StationId));
                shipments = shipments.Where(y => serviceCentre.Any(x => y.DepartureServiceCentreId == x.ServiceCentreId));
            }

            if (!string.IsNullOrWhiteSpace(f_Criteria.UserId))
            {
                shipments = shipments.Where(x => x.UserId == f_Criteria.UserId);
            }

            if (f_Criteria.FilterCustomerType.HasValue && f_Criteria.CustomerId > 0)
            {
                if (f_Criteria.FilterCustomerType.Equals(FilterCustomerType.IndividualCustomer))
                {
                    shipments = shipments.Where(x => !x.CustomerType.Equals(CustomerType.Company.ToString()) && x.CustomerId == f_Criteria.CustomerId);
                }
                else
                {
                    shipments = shipments.Where(x => x.CustomerType.Equals(CustomerType.Company.ToString()) && x.CustomerId == f_Criteria.CustomerId);
                }
            }

            List<ShipmentDTO> shipmentDto = (from r in shipments
                                             select new ShipmentDTO()
                                             {
                                                 ShipmentId = r.ShipmentId,
                                                 Waybill = r.Waybill,
                                                 CustomerId = r.CustomerId,
                                                 CustomerType = r.CustomerType,
                                                 ActualDateOfArrival = r.ActualDateOfArrival,
                                                 DateCreated = r.DateCreated,
                                                 DateModified = r.DateModified,
                                                 DeliveryOptionId = r.DeliveryOptionId,
                                                 DeliveryOption = new DeliveryOptionDTO
                                                 {
                                                     Code = r.DeliveryOption.Code,
                                                     Description = r.DeliveryOption.Description
                                                 },
                                                 DeliveryTime = r.DeliveryTime,
                                                 DepartureServiceCentreId = r.DepartureServiceCentreId,
                                                 DepartureServiceCentre = Context.ServiceCentre.Where(c => c.ServiceCentreId == r.DepartureServiceCentreId).Select(x => new ServiceCentreDTO
                                                 {
                                                     Code = x.Code,
                                                     Name = x.Name
                                                 }).FirstOrDefault(),

                                                 DestinationServiceCentreId = r.DestinationServiceCentreId,
                                                 DestinationServiceCentre = Context.ServiceCentre.Where(c => c.ServiceCentreId == r.DestinationServiceCentreId).Select(x => new ServiceCentreDTO
                                                 {
                                                     Code = x.Code,
                                                     Name = x.Name
                                                 }).FirstOrDefault(),

                                                 ExpectedDateOfArrival = r.ExpectedDateOfArrival,
                                                 PaymentStatus = r.PaymentStatus,
                                                 ReceiverAddress = r.ReceiverAddress,
                                                 ReceiverCity = r.ReceiverCity,
                                                 ReceiverCountry = r.ReceiverCountry,
                                                 ReceiverEmail = r.ReceiverEmail,
                                                 ReceiverName = r.ReceiverName,
                                                 ReceiverPhoneNumber = r.ReceiverPhoneNumber,
                                                 ReceiverState = r.ReceiverState,
                                                 SealNumber = r.SealNumber,
                                                 UserId = r.UserId,
                                                 Value = r.Value,
                                                 GrandTotal = r.GrandTotal,
                                                 AppliedDiscount = r.AppliedDiscount,
                                                 DiscountValue = r.DiscountValue,
                                                 ShipmentPackagePrice = r.ShipmentPackagePrice,
                                                 ShipmentPickupPrice = r.ShipmentPickupPrice,
                                                 CompanyType = r.CompanyType,
                                                 CustomerCode = r.CustomerCode,
                                                 Description = r.Description,
                                                 SenderAddress = r.SenderAddress,
                                                 SenderState = r.SenderState,
                                                 ApproximateItemsWeight = r.ApproximateItemsWeight,
                                                 DepartureCountryId = r.DepartureCountryId,
                                                 DestinationCountryId = r.DestinationCountryId,
                                                 CurrencyRatio = r.CurrencyRatio,
                                                 IsCashOnDelivery = r.IsCashOnDelivery,
                                                 CashOnDeliveryAmount = r.CashOnDeliveryAmount,
                                                 ShipmentCancel = Context.ShipmentCancel.Where(c => c.Waybill == r.Waybill).Select(x => new ShipmentCancelDTO
                                                 {
                                                     CancelReason = x.CancelReason
                                                 }).FirstOrDefault(),
                                             }).ToList();

            return Task.FromResult(shipmentDto.OrderByDescending(x => x.DateCreated).ToList());
        }

        public IQueryable<Shipment> ShipmentsAsQueryable()
        {
            var shipments = _context.Shipment.AsQueryable();
            return shipments;
        }

        //Basic shipment details
        public Task<ShipmentDTO> GetBasicShipmentDetail(string waybill)
        {
            var shipment = _context.Shipment.Where(x => x.Waybill == waybill);

            ShipmentDTO shipmentDto = (from r in shipment
                                       select new ShipmentDTO
                                       {
                                           ShipmentId = r.ShipmentId,
                                           Waybill = r.Waybill,
                                           CustomerId = r.CustomerId,
                                           CustomerType = r.CustomerType,
                                           DateCreated = r.DateCreated,
                                           DateModified = r.DateModified,
                                           DeliveryOptionId = r.DeliveryOptionId,
                                           DeliveryOption = new DeliveryOptionDTO
                                           {
                                               Code = r.DeliveryOption.Code,
                                               Description = r.DeliveryOption.Description
                                           },
                                           DepartureServiceCentreId = r.DepartureServiceCentreId,
                                           DepartureServiceCentre = Context.ServiceCentre.Where(c => c.ServiceCentreId == r.DepartureServiceCentreId).Select(x => new ServiceCentreDTO
                                           {
                                               Code = x.Code,
                                               Name = x.Name
                                           }).FirstOrDefault(),
                                           DestinationServiceCentreId = r.DestinationServiceCentreId,
                                           DestinationServiceCentre = Context.ServiceCentre.Where(c => c.ServiceCentreId == r.DestinationServiceCentreId).Select(x => new ServiceCentreDTO
                                           {
                                               Code = x.Code,
                                               Name = x.Name
                                           }).FirstOrDefault(),
                                           PaymentStatus = r.PaymentStatus,
                                           ReceiverAddress = r.ReceiverAddress,
                                           ReceiverCity = r.ReceiverCity,
                                           ReceiverCountry = r.ReceiverCountry,
                                           ReceiverEmail = r.ReceiverEmail,
                                           ReceiverName = r.ReceiverName,
                                           ReceiverPhoneNumber = r.ReceiverPhoneNumber,
                                           ReceiverState = r.ReceiverState,
                                           SealNumber = r.SealNumber,
                                           UserId = r.UserId,
                                           Value = r.Value,
                                           GrandTotal = r.GrandTotal,
                                           AppliedDiscount = r.AppliedDiscount,
                                           DiscountValue = r.DiscountValue,
                                           ShipmentPackagePrice = r.ShipmentPackagePrice,
                                           ShipmentPickupPrice = r.ShipmentPickupPrice,
                                           CompanyType = r.CompanyType,
                                           CustomerCode = r.CustomerCode,
                                           Description = r.Description,
                                           PickupOptions = r.PickupOptions,
                                           IsInternational = r.IsInternational,
                                           Total = r.Total,
                                           CashOnDeliveryAmount = r.CashOnDeliveryAmount,
                                           IsCashOnDelivery = r.IsCashOnDelivery,
                                           SenderAddress = r.SenderAddress,
                                           SenderState = r.SenderState,
                                           ApproximateItemsWeight = r.ApproximateItemsWeight,
                                           DepartureCountryId = r.DepartureCountryId,
                                           DestinationCountryId = r.DestinationCountryId,
                                           CurrencyRatio = r.CurrencyRatio,
                                           ShipmentCancel = Context.ShipmentCancel.Where(c => c.Waybill == r.Waybill).Select(x => new ShipmentCancelDTO
                                           {
                                               CancelReason = x.CancelReason
                                           }).FirstOrDefault(),

                                       }).FirstOrDefault();

            return Task.FromResult(shipmentDto);
        }

        //Get Shipment sales
        public Task<List<InvoiceViewDTO>> GetSalesForServiceCentre(AccountFilterCriteria accountFilterCriteria, int[] serviceCentreIds)
        {
            DateTime StartDate = accountFilterCriteria.StartDate.GetValueOrDefault().Date;
            DateTime EndDate = accountFilterCriteria.EndDate?.Date ?? StartDate;

            // filter by cancelled shipments
            var shipments = _context.Shipment.AsQueryable().Where(s => s.IsCancelled == false && s.IsDeleted == false);

            //filter by service center of the login user
            if (serviceCentreIds.Length > 0)
            {
                shipments = shipments.Where(s => serviceCentreIds.Contains(s.DepartureServiceCentreId));
            }

            //get startDate and endDate
            var queryDate = accountFilterCriteria.getStartDateAndEndDate();
            var startDate = queryDate.Item1;
            var endDate = queryDate.Item2;
            shipments = shipments.Where(x => x.DateCreated >= startDate && x.DateCreated < endDate);

            //filter by country Id
            if (accountFilterCriteria.CountryId > 0)
            {
                shipments = shipments.Where(s => s.DepartureCountryId == accountFilterCriteria.CountryId);
            }

            List<InvoiceViewDTO> result = (from s in shipments
                                           join i in Context.Invoice on s.Waybill equals i.Waybill
                                           join dept in Context.ServiceCentre on s.DepartureServiceCentreId equals dept.ServiceCentreId
                                           join dest in Context.ServiceCentre on s.DestinationServiceCentreId equals dest.ServiceCentreId
                                           join u in Context.Users on s.UserId equals u.Id
                                           select new InvoiceViewDTO
                                           {
                                               Waybill = s.Waybill,
                                               DepartureServiceCentreId = s.DepartureServiceCentreId,
                                               DestinationServiceCentreId = s.DestinationServiceCentreId,
                                               DepartureServiceCentreName = dept.Name,
                                               DestinationServiceCentreName = dest.Name,
                                               Amount = i.Amount,
                                               PaymentMethod = i.PaymentMethod,
                                               PaymentStatus = i.PaymentStatus,
                                               DateCreated = i.DateCreated,
                                               UserName = u.FirstName + " " + u.LastName,
                                               CompanyType = s.CompanyType,
                                               PaymentTypeReference = i.PaymentTypeReference,
                                               ApproximateItemsWeight = s.ApproximateItemsWeight,
                                               Cash = i.Cash,
                                               Transfer = i.Transfer,
                                               Pos = i.Pos
                                           }).ToList();
            var resultDto = result.OrderByDescending(x => x.DateCreated).ToList();
            return Task.FromResult(resultDto);
        }

        public Tuple<Task<List<IntlShipmentRequestDTO>>, int> GetIntlTransactionShipmentRequest(FilterOptionsDto filterOptionsDto, int[] serviceCentreIds)
        {
            throw new NotImplementedException();
        }
    }

    public class IntlShipmentRequestRepository : Repository<IntlShipmentRequest, GIGLSContext>, IIntlShipmentRequestRepository
    {
        private GIGLSContext _context;
        public IntlShipmentRequestRepository(GIGLSContext context) : base(context)
        {
            _context = context;
        }

        public Task<Tuple<List<IntlShipmentRequestDTO>, int>> GetIntlTransactionShipmentRequest(FilterOptionsDto filterOptionsDto, int[] serviceCentreIds)
        {
            try
            {
                //filter by service center
                var shipmentRequest = _context.IntlShipmentRequest.AsQueryable();
                if (serviceCentreIds.Length > 0)
                {
                    shipmentRequest = _context.IntlShipmentRequest.Where(s => s.IsProcessed == false);
                }

                var count = 0;
                List<IntlShipmentRequestDTO> intlShipmentRequestDTO = new List<IntlShipmentRequestDTO>();

                //filter
                var filter = filterOptionsDto.filter;
                var filterValue = filterOptionsDto.filterValue;
                if (string.IsNullOrEmpty(filter) || string.IsNullOrEmpty(filterValue))
                {
                    intlShipmentRequestDTO = (from r in shipmentRequest
                                              select new IntlShipmentRequestDTO()
                                              {
                                                  IntlShipmentRequestId = r.IntlShipmentRequestId,
                                                  RequestNumber = r.RequestNumber,
                                                  CustomerFirstName = r.CustomerFirstName,
                                                  CustomerLastName = r.CustomerLastName,
                                                  CustomerId = r.CustomerId,
                                                  CustomerType = r.CustomerType,
                                                  CustomerCountryId = r.CustomerCountryId,
                                                  CustomerAddress = r.CustomerAddress,
                                                  CustomerEmail = r.CustomerEmail,
                                                  CustomerPhoneNumber = r.CustomerPhoneNumber,
                                                  CustomerCity = r.CustomerCity,
                                                  CustomerState = r.CustomerState,
                                                  DateCreated = r.DateCreated,
                                                  DateModified = r.DateModified,
                                                  DeliveryOptionId = r.DeliveryOptionId,
                                                  DestinationServiceCentreId = r.DestinationServiceCentreId,
                                                  DestinationServiceCentre = Context.ServiceCentre.Where(c => c.ServiceCentreId == r.DestinationServiceCentreId).Select(x => new ServiceCentreDTO
                                                  {
                                                      Code = x.Code,
                                                      Name = x.Name
                                                  }).FirstOrDefault(),
                                                  ReceiverAddress = r.ReceiverAddress,
                                                  ReceiverCity = r.ReceiverCity,
                                                  ReceiverCountry = r.ReceiverCountry,
                                                  ReceiverEmail = r.ReceiverEmail,
                                                  ReceiverName = r.ReceiverName,
                                                  ReceiverPhoneNumber = r.ReceiverPhoneNumber,
                                                  UserId = r.UserId,
                                                  Value = r.Value,
                                                  GrandTotal = r.GrandTotal,
                                                  SenderAddress = r.SenderAddress,
                                                  SenderState = r.SenderState,
                                                  ApproximateItemsWeight = r.ApproximateItemsWeight,
                                                  DestinationCountryId = r.DestinationCountryId,

                                              }).OrderByDescending(x => x.DateCreated).Take(10).ToList();

                    count = intlShipmentRequestDTO.Count();
                    var retVal = new Tuple<List<IntlShipmentRequestDTO>, int>(intlShipmentRequestDTO, count);

                    return Task.FromResult(retVal);
                }

                intlShipmentRequestDTO = (from r in shipmentRequest
                                          select new IntlShipmentRequestDTO()
                                          {
                                              IntlShipmentRequestId = r.IntlShipmentRequestId,
                                              RequestNumber = r.RequestNumber,
                                              CustomerId = r.CustomerId,
                                              CustomerType = r.CustomerType,
                                              DateCreated = r.DateCreated,
                                              DateModified = r.DateModified,
                                              DeliveryOptionId = r.DeliveryOptionId,
                                              DestinationServiceCentreId = r.DestinationServiceCentreId,
                                              DestinationServiceCentre = Context.ServiceCentre.Where(c => c.ServiceCentreId == r.DestinationServiceCentreId).Select(x => new ServiceCentreDTO
                                              {
                                                  Code = x.Code,
                                                  Name = x.Name
                                              }).FirstOrDefault(),
                                              ReceiverAddress = r.ReceiverAddress,
                                              ReceiverCity = r.ReceiverCity,
                                              ReceiverCountry = r.ReceiverCountry,
                                              ReceiverEmail = r.ReceiverEmail,
                                              ReceiverName = r.ReceiverName,
                                              ReceiverPhoneNumber = r.ReceiverPhoneNumber,
                                              UserId = r.UserId,
                                              Value = r.Value,
                                              GrandTotal = r.GrandTotal,
                                              SenderAddress = r.SenderAddress,
                                              SenderState = r.SenderState,
                                              ApproximateItemsWeight = r.ApproximateItemsWeight,
                                              DestinationCountryId = r.DestinationCountryId,

                                          }).Where(s => (s.RequestNumber == filterValue || s.GrandTotal.ToString() == filterValue || s.DateCreated.ToString() == filterValue)).ToList();

                //filter
                if (!string.IsNullOrEmpty(filter) && !string.IsNullOrEmpty(filterValue))
                {
                    intlShipmentRequestDTO = intlShipmentRequestDTO.Where(s => (s.GetType().GetProperty(filter).GetValue(s)).ToString().Contains(filterValue)).ToList();
                }

                //sort
                var sortorder = filterOptionsDto.sortorder;
                var sortvalue = filterOptionsDto.sortvalue;

                if (!string.IsNullOrEmpty(sortorder) && !string.IsNullOrEmpty(sortvalue))
                {
                    System.Reflection.PropertyInfo prop = typeof(Shipment).GetProperty(sortvalue);

                    if (sortorder == "0")
                    {
                        intlShipmentRequestDTO = intlShipmentRequestDTO.OrderBy(x => x.GetType().GetProperty(prop.Name).GetValue(x)).ToList();
                    }
                    else
                    {
                        intlShipmentRequestDTO = intlShipmentRequestDTO.OrderByDescending(x => x.GetType().GetProperty(prop.Name).GetValue(x)).ToList();
                    }
                }

                intlShipmentRequestDTO = intlShipmentRequestDTO.OrderByDescending(x => x.DateCreated).Skip(filterOptionsDto.count * (filterOptionsDto.page - 1)).Take(filterOptionsDto.count).ToList();
                count = intlShipmentRequestDTO.Count();
                var retValue = new Tuple<List<IntlShipmentRequestDTO>, int>(intlShipmentRequestDTO, count);
                return Task.FromResult(retValue);
            }
            catch (Exception)
            {
                throw;
            }
        }


        public Task<List<IntlShipmentRequestDTO>> GetShipments(int[] serviceCentreIds)
        {
            //filter by service center
            var shipmentRequest = _context.IntlShipmentRequest.AsQueryable();
            if (serviceCentreIds.Length > 0)
            {
                shipmentRequest = _context.IntlShipmentRequest.Where(s => s.IsProcessed == true);
            }

            List<IntlShipmentRequestDTO> IntlShipmentRequestDTO = (from r in shipmentRequest
                                                                   select new IntlShipmentRequestDTO()
                                                                   {
                                                                       IntlShipmentRequestId = r.IntlShipmentRequestId,
                                                                       RequestNumber = r.RequestNumber,
                                                                       CustomerId = r.CustomerId,
                                                                       CustomerType = r.CustomerType,
                                                                       DateCreated = r.DateCreated,
                                                                       DateModified = r.DateModified,
                                                                       DeliveryOptionId = r.DeliveryOptionId,
                                                                       DestinationServiceCentreId = r.DestinationServiceCentreId,
                                                                       DestinationServiceCentre = Context.ServiceCentre.Where(c => c.ServiceCentreId == r.DestinationServiceCentreId).Select(x => new ServiceCentreDTO
                                                                       {
                                                                           Code = x.Code,
                                                                           Name = x.Name
                                                                       }).FirstOrDefault(),
                                                                       ReceiverAddress = r.ReceiverAddress,
                                                                       ReceiverCity = r.ReceiverCity,
                                                                       ReceiverCountry = r.ReceiverCountry,
                                                                       ReceiverEmail = r.ReceiverEmail,
                                                                       ReceiverName = r.ReceiverName,
                                                                       ReceiverPhoneNumber = r.ReceiverPhoneNumber,
                                                                       UserId = r.UserId,
                                                                       Value = r.Value,
                                                                       GrandTotal = r.GrandTotal,
                                                                       SenderAddress = r.SenderAddress,
                                                                       SenderState = r.SenderState,
                                                                       ApproximateItemsWeight = r.ApproximateItemsWeight,
                                                                       DestinationCountryId = r.DestinationCountryId,
                                                                   }).ToList();


            return Task.FromResult(IntlShipmentRequestDTO.ToList());
        }

    }
}