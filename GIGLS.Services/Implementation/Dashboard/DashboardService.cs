﻿using GIGLS.Core;
using GIGLS.Core.DTO.Dashboard;
using GIGLS.Core.IServices.Dashboard;
using GIGLS.Core.IServices.ServiceCentres;
using GIGLS.Core.IServices.Shipments;
using GIGLS.Core.IServices.User;
using System.Threading.Tasks;
using System.Linq;
using GIGLS.Core.Enums;
using GIGLS.Core.IServices.Customers;
using System;
using GIGLS.Infrastructure;
using System.Collections.Generic;
using GIGLS.Core.View;
using GIGLS.Core.DTO.Report;

namespace GIGLS.Services.Implementation.Dashboard
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _uow;
        private IShipmentService _shipmentService;
        private IUserService _userService;
        private IShipmentTrackingService _shipmentTrackingService;
        private IServiceCentreService _serviceCenterService;
        private IStationService _stationService;
        private IIndividualCustomerService _individualCustomerService;
        private ICompanyService _companyService;
        private ICustomerService _customerService;

        public DashboardService(IUnitOfWork uow,
            IShipmentService shipmentService, IUserService userService,
            IShipmentTrackingService shipmentTrackingService,
            IServiceCentreService serviceCenterService,
            IStationService stationService,
            IIndividualCustomerService individualCustomerService,
            ICompanyService companyService,
            ICustomerService customerService
            )
        {
            _uow = uow;
            _shipmentService = shipmentService;
            _userService = userService;
            _shipmentTrackingService = shipmentTrackingService;
            _serviceCenterService = serviceCenterService;
            _stationService = stationService;
            _individualCustomerService = individualCustomerService;
            _companyService = companyService;
            _customerService = customerService;
        }

        public async Task<DashboardDTO> GetDashboard()
        {
            var dashboardDTO = new DashboardDTO();

            // get current user
            try
            {
                var currentUserId = await _userService.GetCurrentUserId();
                var currentUser = await _userService.GetUserById(currentUserId);
                var userClaims = await _userService.GetClaimsAsync(currentUserId);

                string[] claimValue = null;
                foreach (var claim in userClaims)
                {
                    if (claim.Type == "Privilege")
                    {
                        claimValue = claim.Value.Split(':');   // format stringName:stringValue
                    }
                }
                if (claimValue == null)
                {
                    throw new GenericException($"User {currentUser.Username} does not have a priviledge claim.");
                }


                if (claimValue[0] == "Public")
                {
                    dashboardDTO = new DashboardDTO()
                    {
                        Public = "Public"
                    };
                }
                else if (claimValue[0] == "Global")
                {
                    dashboardDTO = await GetDashboardForGlobal();
                }
                else if (claimValue[0] == "Station")
                {
                    dashboardDTO = await GetDashboardForStation(int.Parse(claimValue[1]));
                }
                else if (claimValue[0] == "ServiceCentre")
                {
                    dashboardDTO = await GetDashboardForServiceCentre(int.Parse(claimValue[1]));
                }
                else
                {
                    throw new GenericException($"User {currentUser.Username} does not have a priviledge claim.");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return dashboardDTO;
        }
        
        private async Task<DashboardDTO> GetDashboardForServiceCentreOld(int serviceCenterId)
        {
            var dashboardDTO = new DashboardDTO();

            int[] serviceCenterIds = new int[] { serviceCenterId };
            // get the service centre
            var serviceCentre = await _serviceCenterService.GetServiceCentreById(serviceCenterId);
            var allShipmentsQueryable = _uow.Invoice.GetAllFromInvoiceView();
            var serviceCentreShipments = allShipmentsQueryable.Where(s => serviceCenterId == s.DepartureServiceCentreId);

            //set for TargetAmount and TargetOrder
            dashboardDTO.TargetOrder = serviceCentre.TargetOrder;
            dashboardDTO.TargetAmount = serviceCentre.TargetAmount;


            // get shipment delivered - global
            //var shipmentTrackings = _uow.ShipmentTracking.GetAllAsQueryable();
            //var shipmentsDelivered = shipmentTrackings.Where(s => s.Status == ShipmentScanStatus.ARF.ToString());
            //var shipmentsDeliveredByServiceCenter =
            //    serviceCentreShipments.Where(s => shipmentsDelivered.Select(d => d.Waybill).Contains(s.Waybill));


            // get shipment ordered
            var shipmentsOrderedByServiceCenter = serviceCentreShipments.ToList().AsQueryable();
            dashboardDTO.ShipmentsOrderedByServiceCenter = shipmentsOrderedByServiceCenter;

            var totalCustomers = GetTotalCutomersCount(shipmentsOrderedByServiceCenter);

            // set properties
            dashboardDTO.ServiceCentre = serviceCentre;
            //dashboardDTO.TotalShipmentDelivered = shipmentsDeliveredByServiceCenter.Count();
            dashboardDTO.TotalShipmentOrdered = shipmentsOrderedByServiceCenter.Count();
            dashboardDTO.TotalCustomers = totalCustomers;

            // MostRecentOrder
            var mostRecentOrder =
                shipmentsOrderedByServiceCenter.
                OrderByDescending(s => s.DateCreated).Take(5).ToList();

            dashboardDTO.MostRecentOrder = (from s in mostRecentOrder
                                            select new ShipmentOrderDTO()
                                            {
                                                //// customer
                                                Customer = string.Format($"{s.CustomerId}:{s.CustomerType}"),
                                                //Customer = _customerService.GetCustomer(
                                                //    s.CustomerId,
                                                //    (CustomerType)Enum.Parse(typeof(CustomerType), s.CustomerType)).
                                                //    Result.FirstName,
                                                //price
                                                Price = s.GrandTotal,
                                                //waybill
                                                Waybill = s.Waybill,
                                                //status
                                                //Status = shipmentTrackings.
                                                //    Where(a => a.Waybill == s.Waybill).
                                                //    OrderByDescending(b => b.DateCreated).
                                                //    First().Status,
                                                //date
                                                Date = s.DateCreated
                                            }).ToList();

            // populate customer
            await PopulateCustomer(dashboardDTO);

            // populate graph data
            await PopulateGraphData(dashboardDTO);

            // reset the dashboardDTO.ShipmentsOrderedByServiceCenter
            dashboardDTO.ShipmentsOrderedByServiceCenter = null;

            return dashboardDTO;
        }
        
        private async Task<DashboardDTO> GetDashboardForServiceCentre(int serviceCenterId)
        {
            int currentYear = DateTime.Now.Year;
            var dashboardDTO = new DashboardDTO();

            int[] serviceCenterIds = new int[] { serviceCenterId };
            // get the service centre
            var serviceCentre = await _serviceCenterService.GetServiceCentreById(serviceCenterId);

            //set for TargetAmount and TargetOrder
            dashboardDTO.TargetOrder = serviceCentre.TargetOrder;
            dashboardDTO.TargetAmount = serviceCentre.TargetAmount;

            var allShipmentsQueryable = _uow.Invoice.GetAllFromInvoiceAndShipments().Where(s => s.DateCreated.Year == currentYear);
            var serviceCentreShipments = allShipmentsQueryable.Where(s => serviceCenterId == s.DepartureServiceCentreId);
                        
            // get shipment ordered
            var shipmentsOrderedByServiceCenter = serviceCentreShipments.ToList().AsQueryable();
            dashboardDTO.ShipmentsOrderedByServiceCenter = shipmentsOrderedByServiceCenter;
            
            // set properties
            dashboardDTO.ServiceCentre = serviceCentre;
            dashboardDTO.TotalShipmentDelivered = _uow.Invoice.GetAllFromInvoiceAndShipments().Where(s => s.IsShipmentCollected == true && s.DateCreated.Year == currentYear && serviceCenterId == s.DepartureServiceCentreId).Count();
            dashboardDTO.TotalShipmentOrdered = shipmentsOrderedByServiceCenter.Count();

            //get customer 
            int accountCustomer = _uow.Company.GetAllAsQueryable().Where(c => c.DateCreated.Year == currentYear).Count();
            int individualCustomer = _uow.IndividualCustomer.GetAllAsQueryable().Where(i => i.DateCreated.Year == currentYear).Count();
            dashboardDTO.TotalCustomers = accountCustomer + individualCustomer;

            // MostRecentOrder
            dashboardDTO.MostRecentOrder = new List<ShipmentOrderDTO> { };

            // populate customer
            //await PopulateCustomer(dashboardDTO);

            // populate graph data
            await PopulateGraphData(dashboardDTO);

            // reset the dashboardDTO.ShipmentsOrderedByServiceCenter
            dashboardDTO.ShipmentsOrderedByServiceCenter = null;

            return dashboardDTO;
        }
        
        private async Task<DashboardDTO> GetDashboardForStationOld(int stationId)
        {
            var dashboardDTO = new DashboardDTO();

            // get the service centre
            var serviceCentres = await _serviceCenterService.GetServiceCentres();
            var serviceCenterIds = serviceCentres.Where(s => s.StationId == stationId).Select(s => s.ServiceCentreId).ToArray();
            var allShipments = _uow.Invoice.GetAllFromInvoiceView();
            var serviceCentreShipments = allShipments.Where(s => serviceCenterIds.Contains(s.DepartureServiceCentreId));


            //set for TargetAmount and TargetOrder
            dashboardDTO.TargetOrder = serviceCentres.Where(s => s.StationId == stationId).Sum(s => s.TargetOrder);
            dashboardDTO.TargetAmount = serviceCentres.Where(s => s.StationId == stationId).Sum(s => s.TargetAmount);

            // get shipment delivered - global
            var shipmentTrackings = _uow.ShipmentTracking.GetAllAsQueryable();
            //var shipmentsDelivered = shipmentTrackings.Where(s => s.Status == ShipmentScanStatus.ARF.ToString());
            //var shipmentsDeliveredByServiceCenter =
            //    serviceCentreShipments.Where(s => shipmentsDelivered.Select(d => d.Waybill).Contains(s.Waybill));


            // get shipment ordered
            var shipmentsOrderedByServiceCenter = serviceCentreShipments.ToList().AsQueryable();
            dashboardDTO.ShipmentsOrderedByServiceCenter = shipmentsOrderedByServiceCenter;

            // get all customers - individual and company
            var totalCustomers = GetTotalCutomersCount(shipmentsOrderedByServiceCenter);

            // set properties
            //dashboardDTO.ServiceCentre = serviceCentreDTO;
            var stationDTO = await _stationService.GetStationById(stationId);
            dashboardDTO.Station = stationDTO;
            //dashboardDTO.TotalShipmentDelivered = shipmentsDeliveredByServiceCenter.Count();
            dashboardDTO.TotalShipmentOrdered = shipmentsOrderedByServiceCenter.Count();
            dashboardDTO.TotalCustomers = totalCustomers;

            // MostRecentOrder
            var mostRecentOrder =
                shipmentsOrderedByServiceCenter.
                OrderByDescending(s => s.DateCreated).Take(5).ToList();

            dashboardDTO.MostRecentOrder = (from s in mostRecentOrder
                                            select new ShipmentOrderDTO()
                                            {
                                                //// customer
                                                Customer = string.Format($"{s.CustomerId}:{s.CustomerType}"),
                                                //Customer = _customerService.GetCustomer(
                                                //    s.CustomerId,
                                                //    (CustomerType)Enum.Parse(typeof(CustomerType), s.CustomerType)).
                                                //    Result.FirstName,
                                                //price
                                                Price = s.GrandTotal,
                                                //waybill
                                                Waybill = s.Waybill,
                                                //status
                                                //Status = shipmentTrackings.
                                                //    Where(a => a.Waybill == s.Waybill).
                                                //    OrderByDescending(b => b.DateCreated).
                                                //    First().Status,
                                                //date
                                                Date = s.DateCreated
                                            }).ToList();

            // populate customer
            await PopulateCustomer(dashboardDTO);

            // populate graph data
            await PopulateGraphData(dashboardDTO);

            // reset the dashboardDTO.ShipmentsOrderedByServiceCenter
            dashboardDTO.ShipmentsOrderedByServiceCenter = null;

            return dashboardDTO;
        }
        
        private async Task<DashboardDTO> GetDashboardForStation(int stationId)
        {
            int currentYear = DateTime.Now.Year;
            var dashboardDTO = new DashboardDTO();
            
            //get the station
            var stationDTO = await _stationService.GetStationById(stationId);
            dashboardDTO.Station = stationDTO;

            // get the service centre
            var serviceCentres = await _serviceCenterService.GetServiceCentresByStationId(stationId);
            int[] serviceCenterIds = serviceCentres.Select(s => s.ServiceCentreId).ToArray();
            
            var allShipments = _uow.Invoice.GetAllFromInvoiceAndShipments().Where(s => s.DateCreated.Year == currentYear);
            var serviceCentreShipments = allShipments.Where(s => serviceCenterIds.Contains(s.DepartureServiceCentreId));
            
            //set for TargetAmount and TargetOrder
            dashboardDTO.TargetOrder = serviceCentres.Where(s => s.StationId == stationId).Sum(s => s.TargetOrder);
            dashboardDTO.TargetAmount = serviceCentres.Where(s => s.StationId == stationId).Sum(s => s.TargetAmount);
            
            // get shipment ordered
            var shipmentsOrderedByServiceCenter = serviceCentreShipments.ToList().AsQueryable();
            dashboardDTO.ShipmentsOrderedByServiceCenter = shipmentsOrderedByServiceCenter;
            
            // set properties
            dashboardDTO.TotalShipmentDelivered = _uow.Invoice.GetAllFromInvoiceAndShipments().Where(s => s.IsShipmentCollected == true && s.DateCreated.Year == currentYear && serviceCenterIds.Contains(s.DepartureServiceCentreId)).Count();            
            dashboardDTO.TotalShipmentOrdered = shipmentsOrderedByServiceCenter.Count();

            //customers
            int accountCustomer = _uow.Company.GetAllAsQueryable().Where(c => c.DateCreated.Year == currentYear).Count();
            int individualCustomer = _uow.IndividualCustomer.GetAllAsQueryable().Where(i => i.DateCreated.Year == currentYear).Count();
            dashboardDTO.TotalCustomers = accountCustomer + individualCustomer;

            // MostRecentOrder
            dashboardDTO.MostRecentOrder = new List<ShipmentOrderDTO> { };

            // populate customer
            //await PopulateCustomer(dashboardDTO);

            // populate graph data
            await PopulateGraphData(dashboardDTO);

            // reset the dashboardDTO.ShipmentsOrderedByServiceCenter
            dashboardDTO.ShipmentsOrderedByServiceCenter = null;

            return dashboardDTO;
        }
        
        private async Task<DashboardDTO> GetDashboardForGlobalOld()
        {
            var dashboardDTO = new DashboardDTO();

            int[] serviceCenterIds = { };   // empty array
            var serviceCentreShipmentsQueryable = _uow.Invoice.GetAllFromInvoiceView();

            //set for TargetAmount and TargetOrder
            var serviceCentres = await _serviceCenterService.GetServiceCentres();
            dashboardDTO.TargetOrder = serviceCentres.Sum(s => s.TargetOrder);
            dashboardDTO.TargetAmount = serviceCentres.Sum(s => s.TargetAmount);


            // get shipment delivered
            var shipmentTrackings = _uow.ShipmentTracking.GetAllAsQueryable();
            var shipmentsDelivered = shipmentTrackings.Where(s => s.Status == ShipmentScanStatus.ARF.ToString());

            // get shipment ordered
            var shipmentsOrderedByServiceCenter = serviceCentreShipmentsQueryable.ToList().AsQueryable();
            dashboardDTO.ShipmentsOrderedByServiceCenter = shipmentsOrderedByServiceCenter;

            // get all customers - individual and company
            var totalCustomers = GetTotalCutomersCount(shipmentsOrderedByServiceCenter);


            // set properties
            //dashboardDTO.ServiceCentre = serviceCentreDTO;
            dashboardDTO.TotalShipmentDelivered = shipmentsDelivered.Count();
            dashboardDTO.TotalShipmentOrdered = shipmentsOrderedByServiceCenter.Count();
            dashboardDTO.TotalCustomers = totalCustomers;

            // MostRecentOrder
            var mostRecentOrder =
                serviceCentreShipmentsQueryable.
                OrderByDescending(s => s.DateCreated).Take(5).ToList();

            dashboardDTO.MostRecentOrder = (from s in mostRecentOrder
                                            select new ShipmentOrderDTO()
                                            {
                                                //// customer
                                                Customer = string.Format($"{s.CustomerId}:{s.CustomerType}"),
                                                //Customer = _customerService.GetCustomer(
                                                //    s.CustomerId,
                                                //    (CustomerType)Enum.Parse(typeof(CustomerType), s.CustomerType)).
                                                //    Result.FirstName,
                                                //price
                                                Price = s.GrandTotal,
                                                //waybill
                                                Waybill = s.Waybill,
                                                //status
                                                //Status = shipmentTrackings.
                                                //    Where(a => a.Waybill == s.Waybill).
                                                //    OrderByDescending(b => b.DateCreated).
                                                //    First().Status,
                                                //date
                                                Date = s.DateCreated
                                            }).ToList();

            // populate customer
            await PopulateCustomer(dashboardDTO);

            // populate graph data
            await PopulateGraphData(dashboardDTO);

            // reset the dashboardDTO.ShipmentsOrderedByServiceCenter
            dashboardDTO.ShipmentsOrderedByServiceCenter = null;

            return dashboardDTO;
        }
        
        private async Task<DashboardDTO> GetDashboardForGlobal()
        {
            int currentYear = DateTime.Now.Year;
            var dashboardDTO = new DashboardDTO();

            int[] serviceCenterIds = { };   // empty array
            var serviceCentreShipmentsQueryable = _uow.Invoice.GetAllFromInvoiceAndShipments().Where(s => s.DateCreated.Year == currentYear);

            //set for TargetAmount and TargetOrder
            var serviceCentres = await _serviceCenterService.GetServiceCentres();
            dashboardDTO.TargetOrder = serviceCentres.Sum(s => s.TargetOrder);
            dashboardDTO.TargetAmount = serviceCentres.Sum(s => s.TargetAmount);

            // get shipment ordered
            var shipmentsOrderedByServiceCenter = serviceCentreShipmentsQueryable.ToList().AsQueryable();
            dashboardDTO.ShipmentsOrderedByServiceCenter = shipmentsOrderedByServiceCenter;
            
            // set properties
            dashboardDTO.TotalShipmentDelivered = _uow.Invoice.GetAllFromInvoiceAndShipments().Where(s => s.IsShipmentCollected == true && s.DateCreated.Year == currentYear).Count();
            dashboardDTO.TotalShipmentOrdered = shipmentsOrderedByServiceCenter.Count();
            
            //customers
            int accountCustomer = _uow.Company.GetAllAsQueryable().Where(c => c.DateCreated.Year == currentYear).Count();
            int individualCustomer = _uow.IndividualCustomer.GetAllAsQueryable().Where(i => i.DateCreated.Year == currentYear).Count();
            dashboardDTO.TotalCustomers = accountCustomer + individualCustomer;

            // MostRecentOrder
            dashboardDTO.MostRecentOrder = new List<ShipmentOrderDTO> { };
            
            // populate customer
            //await PopulateCustomer(dashboardDTO);

            // populate graph data
            await PopulateGraphData(dashboardDTO);

            // reset the dashboardDTO.ShipmentsOrderedByServiceCenter
            dashboardDTO.ShipmentsOrderedByServiceCenter = null;

            return dashboardDTO;
        }

        private async Task PopulateGraphData(DashboardDTO dashboardDTO)
        {
            var graphDataList = new List<GraphDataDTO>();
            var shipmentsOrderedByServiceCenter = dashboardDTO.ShipmentsOrderedByServiceCenter;
            int currentYear = DateTime.Now.Year;
            int currentMonth = DateTime.Now.Month;

            ////Only to solve last year report 
            //int currentYear = DateTime.Now.Year - 1;
            //int currentMonth = DateTime.Now.AddMonths(-1).Month;

            // filter shipments by current year
            var thisYearShipments = shipmentsOrderedByServiceCenter.Where(
                s => s.DateCreated.Year == currentYear);

            // fill GraphDataDTO by month
            for (int month = 1; month <= 12; month++)
            {
                var thisMonthShipments = thisYearShipments.Where(
                    s => s.DateCreated.Month == month);

                var graphData = new GraphDataDTO
                {
                    CalculationDay = 1,
                    ShipmentMonth = month,
                    ShipmentYear = currentYear,
                    TotalShipmentByMonth = thisMonthShipments.Count(),
                    TotalSalesByMonth = (from a in thisMonthShipments
                                         select a.GrandTotal).DefaultIfEmpty(0).Sum()
                };
                graphDataList.Add(graphData);

                // set the current month graphData
                if (currentMonth == month)
                {
                    dashboardDTO.CurrentMonthGraphData = graphData;
                }                
            }

            dashboardDTO.GraphData = graphDataList;

            await Task.FromResult(0);
        }
        
        private int GetTotalCutomersCount(IQueryable<InvoiceView> shipmentsOrderedByServiceCenter)
        {
            var count = (from shipment in shipmentsOrderedByServiceCenter select shipment).
                GroupBy(g => new { g.CustomerType, g.CustomerId }).Count();
            
            return count;
        }

        private async Task PopulateCustomer(DashboardDTO dashboardDTO)
        {
            foreach (var order in dashboardDTO.MostRecentOrder)
            {
                string[] custArray = order.Customer.Split(':');

                if (string.IsNullOrEmpty(custArray[0]) || string.IsNullOrEmpty(custArray[1]))
                {
                    order.Customer = "Anonymous";
                }
                else
                {
                    var customerId = int.Parse(custArray[0]);
                    var customerType = CustomerType.Company;

                    if (CustomerType.IndividualCustomer.ToString().Contains(custArray[1]))
                    {
                        customerType = CustomerType.IndividualCustomer;
                    }

                    try
                    {
                        var customer = await _customerService.GetCustomer(
                            customerId, customerType);

                        if (customerType == CustomerType.IndividualCustomer)
                        {
                            order.Customer = string.Format($"{customer.FirstName} {customer.LastName}"); ;
                        }
                        else
                        {
                            order.Customer = customer.Name;
                        }
                    }
                    catch (Exception)
                    {
                        order.Customer = "Anonymous";
                    }
                }
            }
        }

        public async Task<int[]> GetCurrentUserServiceCenters()
        {
            int[] serviceCenterIds = { };   //empty array
            // get current user
            try
            {
                var currentUserId = await _userService.GetCurrentUserId();
                var currentUser = await _userService.GetUserById(currentUserId);
                var userClaims = await _userService.GetClaimsAsync(currentUserId);

                string[] claimValue = null;
                foreach (var claim in userClaims)
                {
                    if (claim.Type == "Privilege")
                    {
                        claimValue = claim.Value.Split(':');   // format stringName:stringValue
                    }
                }
                if (claimValue == null)
                {
                    throw new GenericException($"User {currentUser.Username} does not have a priviledge claim.");
                }

                if (claimValue[0] == "Global")
                {
                    serviceCenterIds = new int[] { };
                }
                else if (claimValue[0] == "Station")
                {
                    var stationId = int.Parse(claimValue[1]);
                    var serviceCentres = await _serviceCenterService.GetServiceCentres();
                    serviceCenterIds = serviceCentres.Where(s => s.StationId == stationId).Select(s => s.ServiceCentreId).ToArray();
                }
                else if (claimValue[0] == "ServiceCentre")
                {
                    int serviceCenterId = int.Parse(claimValue[1]);
                    serviceCenterIds = new int[] { serviceCenterId };
                }
                else
                {
                    throw new GenericException($"User {currentUser.Username} does not have a priviledge claim.");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return serviceCenterIds;
        }
        
        public async Task<DashboardDTO> GetDashboard(DashboardFilterCriteria dashboardFilterCriteria)
        {
            var dashboardDTO = new DashboardDTO();

            // get current user
            try
            {
                var currentUserId = await _userService.GetCurrentUserId();
                var currentUser = await _userService.GetUserById(currentUserId);
                var userClaims = await _userService.GetClaimsAsync(currentUserId);

                string[] claimValue = null;
                foreach (var claim in userClaims)
                {
                    if (claim.Type == "Privilege")
                    {
                        claimValue = claim.Value.Split(':');   // format stringName:stringValue
                    }
                }
                if (claimValue == null)
                {
                    throw new GenericException($"User {currentUser.Username} does not have a priviledge claim.");
                }


                if (claimValue[0] == "Public")
                {
                    dashboardDTO = new DashboardDTO()
                    {
                        Public = "Public"
                    };
                }
                else if (claimValue[0] == "Global")
                {
                    dashboardDTO = await GetDashboardForGlobal(dashboardFilterCriteria);
                }
                else if (claimValue[0] == "Station")
                {
                    dashboardDTO = await GetDashboardForStation(int.Parse(claimValue[1]), dashboardFilterCriteria);
                }
                else if (claimValue[0] == "ServiceCentre")
                {
                    dashboardDTO = await GetDashboardForServiceCentre(int.Parse(claimValue[1]), dashboardFilterCriteria);
                }
                else
                {
                    throw new GenericException($"User {currentUser.Username} does not have a priviledge claim.");
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return dashboardDTO;
        }

        private async Task<DashboardDTO> GetDashboardForServiceCentre(int serviceCenterId, DashboardFilterCriteria dashboardFilterCriteria)
        {
            var dashboardDTO = new DashboardDTO();

            int[] serviceCenterIds = new int[] { serviceCenterId };
            // get the service centre
            var serviceCentre = await _serviceCenterService.GetServiceCentreById(serviceCenterId);

            //set for TargetAmount and TargetOrder
            dashboardDTO.TargetOrder = serviceCentre.TargetOrder;
            dashboardDTO.TargetAmount = serviceCentre.TargetAmount;

            //get startDate and endDate
            var queryDate = dashboardFilterCriteria.getStartDateAndEndDate();
            var startDate = queryDate.Item1;
            var endDate = queryDate.Item2;

            var allShipmentsQueryable = _uow.Invoice.GetAllFromInvoiceAndShipments().Where(s => s.DateCreated >= startDate && s.DateCreated < endDate);
            var serviceCentreShipments = allShipmentsQueryable.Where(s => serviceCenterId == s.DepartureServiceCentreId);

            // get shipment ordered
            var shipmentsOrderedByServiceCenter = serviceCentreShipments.ToList().AsQueryable();
            dashboardDTO.ShipmentsOrderedByServiceCenter = shipmentsOrderedByServiceCenter;

            // set properties
            dashboardDTO.ServiceCentre = serviceCentre;
            dashboardDTO.TotalShipmentDelivered = _uow.Invoice.GetAllFromInvoiceAndShipments().Where(s => s.IsShipmentCollected == true && serviceCenterId == s.DepartureServiceCentreId && s.DateCreated >= startDate && s.DateCreated < endDate).Count();
            dashboardDTO.TotalShipmentOrdered = shipmentsOrderedByServiceCenter.Count();

            //get customer 
            int accountCustomer = _uow.Company.GetAllAsQueryable().Where(c => c.DateCreated >= startDate && c.DateCreated < endDate).Count();
            int individualCustomer = _uow.IndividualCustomer.GetAllAsQueryable().Where(i => i.DateCreated >= startDate && i.DateCreated < endDate).Count();
            dashboardDTO.TotalCustomers = accountCustomer + individualCustomer;

            // MostRecentOrder
            dashboardDTO.MostRecentOrder = new List<ShipmentOrderDTO> { };

            // populate graph data
            await PopulateGraphDataByDate(dashboardDTO);

            // reset the dashboardDTO.ShipmentsOrderedByServiceCenter
            dashboardDTO.ShipmentsOrderedByServiceCenter = null;

            return dashboardDTO;
        }

        private async Task<DashboardDTO> GetDashboardForStation(int stationId, DashboardFilterCriteria dashboardFilterCriteria)
        {
            var dashboardDTO = new DashboardDTO();

            //get the station
            var stationDTO = await _stationService.GetStationById(stationId);
            dashboardDTO.Station = stationDTO;

            // get the service centre
            var serviceCentres = await _serviceCenterService.GetServiceCentresByStationId(stationId);
            int[] serviceCenterIds = serviceCentres.Select(s => s.ServiceCentreId).ToArray();

            //get startDate and endDate
            var queryDate = dashboardFilterCriteria.getStartDateAndEndDate();
            var startDate = queryDate.Item1;
            var endDate = queryDate.Item2;

            var allShipments = _uow.Invoice.GetAllFromInvoiceAndShipments().Where(s => s.DateCreated >= startDate && s.DateCreated < endDate);
            var serviceCentreShipments = allShipments.Where(s => serviceCenterIds.Contains(s.DepartureServiceCentreId));

            //set for TargetAmount and TargetOrder
            dashboardDTO.TargetOrder = serviceCentres.Where(s => s.StationId == stationId).Sum(s => s.TargetOrder);
            dashboardDTO.TargetAmount = serviceCentres.Where(s => s.StationId == stationId).Sum(s => s.TargetAmount);

            // get shipment ordered
            var shipmentsOrderedByServiceCenter = serviceCentreShipments.ToList().AsQueryable();
            dashboardDTO.ShipmentsOrderedByServiceCenter = shipmentsOrderedByServiceCenter;

            // set properties
            dashboardDTO.TotalShipmentDelivered = _uow.Invoice.GetAllFromInvoiceAndShipments().Where(s => s.IsShipmentCollected == true && s.DateCreated >= startDate && s.DateCreated < endDate && serviceCenterIds.Contains(s.DepartureServiceCentreId)).Count();
            dashboardDTO.TotalShipmentOrdered = shipmentsOrderedByServiceCenter.Count();

            //customers
            int accountCustomer = _uow.Company.GetAllAsQueryable().Where(s => s.DateCreated >= startDate && s.DateCreated < endDate).Count();
            int individualCustomer = _uow.IndividualCustomer.GetAllAsQueryable().Where(i => i.DateCreated >= startDate && i.DateCreated < endDate).Count();
            dashboardDTO.TotalCustomers = accountCustomer + individualCustomer;

            // MostRecentOrder
            dashboardDTO.MostRecentOrder = new List<ShipmentOrderDTO> { };

            // populate customer
            //await PopulateCustomer(dashboardDTO);

            // populate graph data
            await PopulateGraphDataByDate(dashboardDTO);

            // reset the dashboardDTO.ShipmentsOrderedByServiceCenter
            dashboardDTO.ShipmentsOrderedByServiceCenter = null;

            return dashboardDTO;
        }

        private async Task<DashboardDTO> GetDashboardForGlobal(DashboardFilterCriteria dashboardFilterCriteria)
        {
            var dashboardDTO = new DashboardDTO();

            //get startDate and endDate
            var queryDate = dashboardFilterCriteria.getStartDateAndEndDate();
            var startDate = queryDate.Item1;
            var endDate = queryDate.Item2;

            int[] serviceCenterIds = { };   // empty array
            var serviceCentreShipmentsQueryable = _uow.Invoice.GetAllFromInvoiceAndShipments().Where(s => s.DateCreated >= startDate && s.DateCreated < endDate);

            //set for TargetAmount and TargetOrder
            var serviceCentres = await _serviceCenterService.GetServiceCentres();
            dashboardDTO.TargetOrder = serviceCentres.Sum(s => s.TargetOrder);
            dashboardDTO.TargetAmount = serviceCentres.Sum(s => s.TargetAmount);

            // get shipment ordered
            var shipmentsOrderedByServiceCenter = serviceCentreShipmentsQueryable.ToList().AsQueryable();
            dashboardDTO.ShipmentsOrderedByServiceCenter = shipmentsOrderedByServiceCenter;

            // set properties
            dashboardDTO.TotalShipmentDelivered = _uow.Invoice.GetAllFromInvoiceAndShipments().Where(s => s.IsShipmentCollected == true && s.DateCreated >= startDate && s.DateCreated < endDate).Count();
            dashboardDTO.TotalShipmentOrdered = shipmentsOrderedByServiceCenter.Count();

            //customers
            int accountCustomer = _uow.Company.GetAllAsQueryable().Where(c => c.DateCreated >= startDate && c.DateCreated < endDate).Count();
            int individualCustomer = _uow.IndividualCustomer.GetAllAsQueryable().Where(i => i.DateCreated >= startDate && i.DateCreated < endDate).Count();
            dashboardDTO.TotalCustomers = accountCustomer + individualCustomer;

            // MostRecentOrder
            dashboardDTO.MostRecentOrder = new List<ShipmentOrderDTO> { };

            // populate customer
            //await PopulateCustomer(dashboardDTO);

            // populate graph data
            await PopulateGraphDataByDate(dashboardDTO);

            // reset the dashboardDTO.ShipmentsOrderedByServiceCenter
            dashboardDTO.ShipmentsOrderedByServiceCenter = null;

            return dashboardDTO;
        }

        private async Task PopulateGraphDataByDate(DashboardDTO dashboardDTO)
        {
            var graphDataList = new List<GraphDataDTO>();
            var shipmentsOrderedByServiceCenter = dashboardDTO.ShipmentsOrderedByServiceCenter;
            int currentMonth = DateTime.Now.Month;

            //use this date as the next year of when we launched Agility to cater for
            //month we have not launch agility that will be empty
            int year = 2018;
            
            // fill GraphDataDTO by month
            for (int month = 1; month <= 12; month++)
            {
                var thisMonthShipments = shipmentsOrderedByServiceCenter.Where(
                    s => s.DateCreated.Month == month);
                
                var firstDataToGetYear = thisMonthShipments.FirstOrDefault();
                if(firstDataToGetYear != null)
                {
                    year = firstDataToGetYear.DateCreated.Year;
                }
                else if (dashboardDTO.ServiceCentre != null && firstDataToGetYear == null)
                {
                    year = dashboardDTO.ServiceCentre.DateCreated.Year;
                }
                else if (dashboardDTO.Station != null && dashboardDTO.ServiceCentre == null && firstDataToGetYear == null)
                {
                    year = dashboardDTO.Station.DateCreated.Year;
                }

                var graphData = new GraphDataDTO
                {
                    CalculationDay = 1,
                    ShipmentMonth = month,
                    ShipmentYear = year,
                    TotalShipmentByMonth = thisMonthShipments.Count(),
                    TotalSalesByMonth = (from a in thisMonthShipments
                                         select a.GrandTotal).DefaultIfEmpty(0).Sum()
                };
                graphDataList.Add(graphData);

                // set the current month graphData
                if (currentMonth == month)
                {
                    dashboardDTO.CurrentMonthGraphData = graphData;
                }
            }

            dashboardDTO.GraphData = graphDataList;

            await Task.FromResult(0);
        }
    }
}