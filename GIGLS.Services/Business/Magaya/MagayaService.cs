﻿using AutoMapper;
using GIGL.GIGLS.Core.Domain;
using GIGLS.Core;
using GIGLS.Core.DTO;
using GIGLS.Core.DTO.Customers;
using GIGLS.Core.DTO.ServiceCentres;
using GIGLS.Core.DTO.Shipments;
using GIGLS.Core.Enums;
using GIGLS.Core.IServices.Customers;
using GIGLS.Core.IServices.ServiceCentres;
using GIGLS.Core.IServices.Shipments;
using GIGLS.Core.IServices.User;
using GIGLS.Core.IServices.Utility;
using GIGLS.CORE.DTO.Shipments;
using GIGLS.Infrastructure;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel;
using System.Threading.Tasks;
//using ThirdParty.WebServices;
using ThirdParty.WebServices.Magaya.Business.New;
using ThirdParty.WebServices.Magaya.DTO;
using ThirdParty.WebServices.Magaya.Services;

namespace GIGLS.Services.Business.Magaya.Shipments
{
    public class MagayaService : IMagayaService
    {

        int key = -1;
        int myAccessKey = -1;
        CSSoapServiceSoapClient cs;
        private readonly INumberGeneratorMonitorService _numberGeneratorMonitorService;
        private readonly IUnitOfWork _uow;
        private readonly IUserService _userService;
        private readonly IShipmentService _shipmentService;
        private readonly IServiceCentreService _centreService;
        private readonly IStationService _stationService;
        private readonly IIndividualCustomerService _individualCustomerService;

        public MagayaService(
            INumberGeneratorMonitorService numberGeneratorMonitorService,
            IUnitOfWork uow,
            IUserService userService,
            IShipmentService shipmentService,
            IServiceCentreService centreService,
            IStationService stationService, IIndividualCustomerService individualCustomerController)
        {
            string magayaUri = ConfigurationManager.AppSettings["MagayaUrl"];
            _uow = uow;
            var _webServiceUrl = magayaUri;
            _shipmentService = shipmentService;
            _userService = userService;
            _centreService = centreService;
            _stationService = stationService;
            _individualCustomerService = individualCustomerController;

            var remoteAddress = new System.ServiceModel.EndpointAddress(_webServiceUrl);
            cs = new CSSoapServiceSoapClient(new System.ServiceModel.BasicHttpBinding(), remoteAddress);
            _numberGeneratorMonitorService = numberGeneratorMonitorService;
        }

        //Open Connections
        public bool OpenConnection(out int access_key)
        {
            var magayausername = ConfigurationManager.AppSettings["MagayaUsername"];
            var magayapassword = ConfigurationManager.AppSettings["MagayaPassword"];

            api_session_error result = api_session_error.no_error;
            try
            {
                result = cs.StartSession(magayausername, magayapassword, out key);
                access_key = key;
                return result == api_session_error.no_error;
            }
            catch
            {
                access_key = 0;
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="access_key"></param>
        /// <returns></returns>
        public string CloseConnection(int access_key)
        {
            try
            {
                var result = cs.EndSession(access_key);
                return result.ToString();
            }
            catch
            {
                return null;
            }

        }

        public double CalVolumentricWeight(double volume, double grossWeight)
        {
            var volumetricWeight = volume / 166; // in lb
            return (volumetricWeight > grossWeight) ? volumetricWeight : grossWeight;
        }

        public CurrencyType retCurrencyType()
        {
            return new CurrencyType()
            {
                Code = "USD",
                DecimalPlaces = 2,
                ExchangeRate = 1.00,
                IsHomeCurrency = true,
                Name = "United States Dollar",
                Symbol = "",
                DecimalPlacesSpecified = true,
                ExchangeRateSpecified = true,
                IsHomeCurrencySpecified = true,
            };
        }

        public GUIDItem retNewGuiItem(WarehouseReceipt magayaShipmentDTO)
        {
            return new GUIDItem()
            {
                GUID = Guid.NewGuid().ToString(),
                Number = magayaShipmentDTO.Number,
                Type = TransactionType.WarehouseReceipt
            };
        }

        public MeasurementUnits newMeasurementUnits()
        {
            return new MeasurementUnits()
            {
                LengthUnit = LengthUnitType.@in,
                LengthUnitSpecified = true,
                VolumeUnit = VolumeUnitType.ft3,
                VolumeUnitSpecified = true,
                WeightUnit = WeightUnitType.lb,
                WeightUnitSpecified = true,
                VolumeWeightUnit = VolumeWeightUnitType.vlb,
                VolumeWeightUnitSpecified = true,
                LengthPrecision = 2,
                LengthPrecisionSpecified = true,
                VolumePrecision = 2,
                VolumePrecisionSpecified = true,
                WeightPrecision = 2,
                WeightPrecisionSpecified = true,
                VolumeWeightPrecision = 2,
                VolumeWeightPrecisionSpecified = true,
                AreaPrecision = 2,
                AreaPrecisionSpecified = true,
                VolumeWeightFactor = 166,
                VolumeWeightFactorSpecified = true
            };
        }

        DateTime todaysDate = DateTime.Now;
        double totalPiece = 0.00;
        double totalVolume = 0.00;
        double totalWeight = 0.00;
        double totalVolumeWeight = 0.00;
        Guid guid = Guid.NewGuid();

        private void setMagayaShipmentItems(WarehouseReceipt magayaShipmentDTO)
        {
            for (int i = 0; i < magayaShipmentDTO.Items.Length; i++)
            {
                magayaShipmentDTO.Items[i].GUID = Guid.NewGuid().ToString();
                magayaShipmentDTO.Items[i].Status = ItemStatusType.OnHand;
                magayaShipmentDTO.Items[i].IsSummarized = false;
                magayaShipmentDTO.Items[i].WarehouseReceiptGUID = guid.ToString();
                magayaShipmentDTO.Items[i].PackageName = magayaShipmentDTO.Items[i].Package.Name;
                magayaShipmentDTO.Items[i].LocationCode = magayaShipmentDTO.Items[i].Location.Code;
                magayaShipmentDTO.Items[i].Length = new LenghtValue() { Unit = LengthUnitType.@in, Value = magayaShipmentDTO.Items[i].Length.Value };
                magayaShipmentDTO.Items[i].Width = new LenghtValue() { Unit = LengthUnitType.@in, Value = magayaShipmentDTO.Items[i].Width.Value };
                magayaShipmentDTO.Items[i].Height = new LenghtValue() { Unit = LengthUnitType.@in, Value = magayaShipmentDTO.Items[i].Height.Value };
                magayaShipmentDTO.Items[i].Weight = new WeightValue() { Unit = WeightUnitType.lb, Value = magayaShipmentDTO.Items[i].Length.Value };
                magayaShipmentDTO.Items[i].ContainedPiecesWeightIncluded = true;
                var volume = magayaShipmentDTO.Items[i].Length.Value * magayaShipmentDTO.Items[i].Width.Value * magayaShipmentDTO.Items[i].Height.Value;

                magayaShipmentDTO.Items[i].VolumeWeight = new VolumeWeightValue()
                {
                    Unit = VolumeWeightUnitType.vlb,
                    Value = volume / 166 * Convert.ToDouble(magayaShipmentDTO.Items[i].Pieces)
                };

                magayaShipmentDTO.Items[i].Package = magayaShipmentDTO.Items[i].Package;
                magayaShipmentDTO.Items[i].OutShipmentGUID = guid.ToString();
                magayaShipmentDTO.Items[i].Location = magayaShipmentDTO.Items[i].Location;
                magayaShipmentDTO.Items[i].IncludeInSED = true;
                magayaShipmentDTO.Items[i].IsContainer = false;
                magayaShipmentDTO.Items[i].OutDate = todaysDate;
                magayaShipmentDTO.Items[i].WarehouseReceiptNumber = magayaShipmentDTO.Number;
                magayaShipmentDTO.Items[i].IsPallet = false;
                magayaShipmentDTO.Items[i].IsOverstock = false;
                magayaShipmentDTO.Items[i].NotLoaded = false;
                magayaShipmentDTO.Items[i].EntryDate = todaysDate;

                totalPiece += Convert.ToDouble(magayaShipmentDTO.Items[i].Pieces);
                totalWeight += magayaShipmentDTO.Items[i].PieceWeight.Value * Convert.ToDouble(magayaShipmentDTO.Items[i].Pieces);
                totalVolume += volume;
                totalVolumeWeight += magayaShipmentDTO.Items[i].VolumeWeight.Value;
            }
            magayaShipmentDTO.TotalWeight = new WeightValue() { Unit = WeightUnitType.lb, Value = totalWeight };
            return;
        }

        double totalChargeAmount = 0.00; //total cost of shipment cost
        private void setMagayaShipmentCharges(WarehouseReceipt magayaShipmentDTO)
        {
            var cc = retCurrencyType();
            totalWeight = (totalWeight > totalVolumeWeight) ? totalWeight : totalVolumeWeight;

            for (int i = 0; i < magayaShipmentDTO.Charges.Charge.Length; i++)
            {
                magayaShipmentDTO.Charges.Charge[i].TaxDefinition = null;
                magayaShipmentDTO.Charges.Charge[i].TaxAmount = new MoneyValue() { Value = 0, Currency = "USD" };
                magayaShipmentDTO.Charges.Charge[i].Price = new MoneyValue()
                {
                    Value = (magayaShipmentDTO.Charges.Charge[i].Price != null) ? magayaShipmentDTO.Charges.Charge[i].Price.Value : 0,
                    Currency = "USD"
                };
                magayaShipmentDTO.Charges.Charge[i].Amount = new MoneyValue()
                {
                    Value = (magayaShipmentDTO.Charges.Charge[i].Amount != null) ? magayaShipmentDTO.Charges.Charge[i].Amount.Value : 0.00,
                    Currency = "USD"
                };
                magayaShipmentDTO.Charges.Charge[i].RetentionAmount = new MoneyValue() { Value = 0, Currency = "USD" };
                magayaShipmentDTO.Charges.Charge[i].Entity = magayaShipmentDTO.BillingClient;
                magayaShipmentDTO.Charges.Charge[i].ExchangeRate = 1.00;
                magayaShipmentDTO.Charges.Charge[i].HomeCurrency = cc;
                magayaShipmentDTO.Charges.Charge[i].Currency = cc;

                magayaShipmentDTO.Charges.Charge[i].PriceInCurrency = new MoneyValue()
                {
                    Value = (magayaShipmentDTO.Charges.Charge[i].Price != null) ? magayaShipmentDTO.Charges.Charge[i].Price.Value : 0,
                    Currency = "USD"
                };

                magayaShipmentDTO.Charges.Charge[i].Amount.Value = (magayaShipmentDTO.Charges.Charge[i].FreightChargeInfo.Flags == ChargeFlagsType.Maximum ||
                    magayaShipmentDTO.Charges.Charge[i].FreightChargeInfo.Flags == ChargeFlagsType.Minimum) ?
                    magayaShipmentDTO.Charges.Charge[i].Price.Value : totalWeight * magayaShipmentDTO.Charges.Charge[i].Price.Value;

                totalChargeAmount += magayaShipmentDTO.Charges.Charge[i].Amount.Value;

                magayaShipmentDTO.Charges.Charge[i].AmountInCurrency = new MoneyValue()
                {
                    Value = (magayaShipmentDTO.Charges.Charge[i].Amount != null) ? magayaShipmentDTO.Charges.Charge[i].Amount.Value : 0.00,
                    Currency = "USD"
                };

                magayaShipmentDTO.Charges.Charge[i].TaxAmountInCurrency = new MoneyValue() { Value = 0, Currency = "USD" };
                magayaShipmentDTO.Charges.Charge[i].RetentionAmountInCurrency = new MoneyValue() { Value = 0, Currency = "USD" };
                magayaShipmentDTO.Charges.Charge[i].IsThirdPartyCharge = false;
                magayaShipmentDTO.Charges.Charge[i].Status = ChargeStatusType.Open;
                magayaShipmentDTO.Charges.Charge[i].Notes = "";
                magayaShipmentDTO.Charges.Charge[i].Units = "";
                magayaShipmentDTO.Charges.Charge[i].IsCredit = false;
                magayaShipmentDTO.Charges.Charge[i].IsPrepaid = true;
                magayaShipmentDTO.Charges.Charge[i].Quantity = totalWeight;
                magayaShipmentDTO.Charges.Charge[i].Type = ChargeDesc.Freight;
                magayaShipmentDTO.Charges.Charge[i].ShowInDocuments = true;
                magayaShipmentDTO.Charges.Charge[i].ShowInDocumentsSpecified = true;

                magayaShipmentDTO.Charges.Charge[i].CreatedAt = retNewGuiItem(magayaShipmentDTO);

                magayaShipmentDTO.Charges.Charge[i].FreightChargeInfo = new FreightCharge()
                {
                    Pieces = totalPiece,
                    Weight = new WeightValue() { Unit = WeightUnitType.lb, Value = totalWeight },
                    Volume = new VolumeValue() { Unit = VolumeUnitType.ft3, Value = totalVolume / 1738 },
                    ChargeableWeight = new WeightValue()
                    {
                        Unit = WeightUnitType.lb,
                        Value = (totalWeight > totalVolumeWeight) ? totalWeight : totalVolumeWeight
                    },
                    UseGrossWeight = false,
                    Flags = magayaShipmentDTO.Charges.Charge[i].FreightChargeInfo.Flags,
                    ApplyBy = ApplyByType.Weight,
                    Method = MethodType.Air,
                    MeasurementUnits = magayaShipmentDTO.MeasurementUnits
                };

                magayaShipmentDTO.Charges.Charge[i].Customs = null;
                magayaShipmentDTO.Charges.Charge[i].IsFromSegment = false;
                magayaShipmentDTO.Charges.Charge[i].ModeOfTransportation = magayaShipmentDTO.ModeOfTransportation;
                magayaShipmentDTO.Charges.Charge[i].PointOfOrigin = magayaShipmentDTO.OriginPort;
                magayaShipmentDTO.Charges.Charge[i].PointOfDestination = magayaShipmentDTO.DestinationPort;
            }
            return;
        }

        public async Task<api_session_error> SetTransactions(int access_key, TheWarehouseReceiptCombo mDto)
        {
            var magayaShipmentDTO = mDto.WarehouseReceipt;
            //2. initialize type of shipment and flag
            string type = "WH";

            //int flags = 0x00000800;
            int flags = 0x00000800 | 0x00000001;
            Guid guid = Guid.NewGuid();

            //3. replace some important variables
            magayaShipmentDTO.Type = "WH";
            magayaShipmentDTO.GUID = guid.ToString();
            magayaShipmentDTO.Division = null;
            magayaShipmentDTO.MainCarrier = null;
            magayaShipmentDTO.Customs = null;
            magayaShipmentDTO.TotalPieces = null;
            magayaShipmentDTO.BondedEntryNumber = null;
            magayaShipmentDTO.Carrier = null;
            magayaShipmentDTO.CreatedOn = DateTime.Now;
            magayaShipmentDTO.Charges.UseSequenceOrder = false;

            setMagayaShipmentItems(magayaShipmentDTO);
            magayaShipmentDTO.MeasurementUnits = newMeasurementUnits();
            setMagayaShipmentCharges(magayaShipmentDTO);

            //4. initilize the variables to hold some parameters and return values
            string trans_xml = string.Empty;
            var errval = string.Empty;
            api_session_error result = api_session_error.no_error;

            //5. initialize the serializer object
            Serializer sr = new Serializer();

            //6. serialize object to xml from class warehousereceipt
            var xmlobject = Mapper.Map<WarehouseReceipt>(magayaShipmentDTO);

            try
            {
                //serialize to xml for the magaya request
                trans_xml = sr.Serialize<WarehouseReceipt>(xmlobject);
                string error_code = "";

                //Magaya Request for Shipment Creation
                result = cs.SetTransaction(access_key, type, flags, trans_xml, out error_code);
                //result = api_session_error.no_error;

                if (result == api_session_error.no_error)
                {
                    var shipmentDto = await CreateMagayaShipmentInAgilityAsync(mDto);
                    await _shipmentService.AddShipment(shipmentDto);

                    if (mDto?.IntlShipmentRequest.RequestNumber.Length > 0 )
                    {
                        var request = await _uow.IntlShipmentRequest.GetAsync(s => s.RequestNumber == mDto.IntlShipmentRequest.RequestNumber);
                        request.IsProcessed = true;
                        await _uow.CompleteAsync();
                    }
                }
                else
                {
                    throw new Exception("Error Creating Shipment: ");
                }

                errval = error_code;
            }
            catch (Exception ex)
            {
                errval = ex.Message;
            }
            return result;
        }

        public List<ShipmentItemDTO> getShipmentItems(WarehouseReceipt magayaShipmentDTO)
        {
            var ShipmentItems = new List<ShipmentItemDTO>();

            for (int i = 0; i < magayaShipmentDTO.Items.Length; i++)
            {
                ShipmentItems.Add(
                        new ShipmentItemDTO()
                        {
                            Description = magayaShipmentDTO.Items[i].Description,
                            ShipmentType = ShipmentType.Regular,
                            Weight = magayaShipmentDTO.TotalWeight.Value,
                            Nature = "Normal",
                            Length = magayaShipmentDTO.Items[i].Length.Value,
                            Width = magayaShipmentDTO.Items[i].Width.Value,
                            Height = magayaShipmentDTO.Items[i].Height.Value
                        }
                    );
            };
            return ShipmentItems;
        }

        public List<IntlShipmentRequestItemDTO> getIntlShipmentItems(IntlShipmentRequestDTO intlShipmentDTO)
        {
            var ShipmentItems = new List<IntlShipmentRequestItemDTO>();

            for (int i = 0; i < intlShipmentDTO.ShipmentRequestItems.Count; i++)
            {
                ShipmentItems.Add(
                        new IntlShipmentRequestItemDTO()
                        {
                            Description = intlShipmentDTO.ShipmentRequestItems[i].Description,
                            ShipmentType = ShipmentType.Regular,
                            Weight = intlShipmentDTO.ShipmentRequestItems[i].Weight,
                            Nature = "Normal",
                            Length = intlShipmentDTO.ShipmentRequestItems[i].Length,
                            Width = intlShipmentDTO.ShipmentRequestItems[i].Width,
                            Height = intlShipmentDTO.ShipmentRequestItems[i].Height
                        }
                    );
            };
            return ShipmentItems;
        }

        public CustomerDTO tetCustomerDetails(WarehouseReceipt magayaShipmentDTO)
        {
            CustomerDTO cd = new CustomerDTO();
            var bolVal = magayaShipmentDTO?.ShipperName.Split(' ');
            cd.FirstName = bolVal[0];
            cd.LastName = (bolVal.Length > 1) ? bolVal[1] : bolVal[0];
            cd.Email = magayaShipmentDTO.ShipperAddress.ContactEmail;
            cd.Address = magayaShipmentDTO.ShipperAddress.Street[0];
            cd.PhoneNumber = magayaShipmentDTO.ShipperAddress.ContactPhone;
            cd.City = magayaShipmentDTO.ShipperAddress.City;
            cd.State = magayaShipmentDTO.ShipperAddress.State;
            cd.CustomerType = CustomerType.IndividualCustomer;

            if (magayaShipmentDTO.ShipperAddress.Country != null)
            {
                cd.Country = new CountryDTO()
                {
                    CountryName = magayaShipmentDTO.ShipperAddress.Country.Value,
                    CountryCode = magayaShipmentDTO.ShipperAddress.Country.Code,
                };
            }

            return cd;
        }

        public async Task<ShipmentDTO> CreateMagayaShipmentInAgilityAsync(TheWarehouseReceiptCombo mDto)
        {
            var magayaShipmentDTO = mDto.WarehouseReceipt;
            try
            {
                Serializer sr = new Serializer();
                var currentUserId = await _userService.GetCurrentUserId();
                var currentUser = await _userService.GetUserById(currentUserId);
                var userClaims = await _userService.GetClaimsAsync(currentUserId);

                var scs = await _centreService.GetServiceCentres();

                var destinationSc = scs.Where(s => s.ServiceCentreId == mDto.ServiceCenterId).FirstOrDefault();
                //var spSC = destinationSc.SupperServiceCentreId;

                string[] claimValue = null;
                foreach (var claim in userClaims)
                {
                    if (claim.Type == "Privilege")
                    {
                        claimValue = claim.Value.Split(':');   // format stringName:stringValue
                    }
                }

                var serviceCenter = await _centreService.GetServiceCentreById(int.Parse(claimValue[1]));
                var shipmentItems = getShipmentItems(magayaShipmentDTO);

                var shipmentDTO = new ShipmentDTO();
                shipmentDTO.Waybill = magayaShipmentDTO.Number;
                shipmentDTO.Value = 0;
                shipmentDTO.DeliveryTime = DateTime.Now;
                shipmentDTO.PaymentStatus = PaymentStatus.Paid;
                shipmentDTO.CustomerType = CustomerType.IndividualCustomer.ToString();
                shipmentDTO.CustomerCode = "";

                //Departure and Destination Details
                shipmentDTO.DepartureServiceCentreId = int.Parse(claimValue[1]);
                shipmentDTO.DepartureServiceCentre = serviceCenter;
                shipmentDTO.DestinationServiceCentreId = destinationSc.ServiceCentreId;
                shipmentDTO.DestinationServiceCentre = destinationSc;

                //Receivers Details
                shipmentDTO.ReceiverName = magayaShipmentDTO.ConsigneeName;
                shipmentDTO.ReceiverPhoneNumber = magayaShipmentDTO.Consignee.Phone;
                shipmentDTO.ReceiverEmail = (magayaShipmentDTO.Consignee.Email != " ") ? magayaShipmentDTO.Consignee.Email : magayaShipmentDTO.ConsigneeAddress.ContactEmail;
                shipmentDTO.ReceiverAddress = magayaShipmentDTO.ConsigneeAddress.Street[0];
                shipmentDTO.ReceiverCity = magayaShipmentDTO.ConsigneeAddress.City;
                shipmentDTO.ReceiverState = magayaShipmentDTO.ConsigneeAddress.State;
                shipmentDTO.ReceiverCountry = magayaShipmentDTO.ConsigneeAddress.Country?.Value;

                //Delivery Options
                shipmentDTO.DeliveryOptionId = 1;

                //PickUp Options
                shipmentDTO.PickupOptions = PickupOptions.HOMEDELIVERY;

                //Shipment Items
                shipmentDTO.ShipmentItems = shipmentItems;
                shipmentDTO.ApproximateItemsWeight = magayaShipmentDTO.TotalWeight.Value;
                shipmentDTO.GrandTotal = (decimal)totalChargeAmount;

                //Invoice parameters: Helps generate invoice for ecomnerce customers  by customerType
                shipmentDTO.IsCashOnDelivery = false;
                shipmentDTO.CashOnDeliveryAmount = 0;
                shipmentDTO.ExpectedAmountToCollect = (mDto.ExpectedAmountToCollect == null) ? 0 : decimal.Parse(mDto.ExpectedAmountToCollect);
                shipmentDTO.ActualAmountCollected = (mDto.ActualAmountCollected == null) ? 0 : decimal.Parse(mDto.ActualAmountCollected);

                //General Details comes with role user
                shipmentDTO.UserId = currentUserId;

                shipmentDTO.Customer = new List<CustomerDTO>()
                    {
                         tetCustomerDetails(magayaShipmentDTO)
                    };

                shipmentDTO.CustomerDetails = tetCustomerDetails(magayaShipmentDTO);
                shipmentDTO.IsdeclaredVal = false;
                shipmentDTO.DeclarationOfValueCheck = 0;

                //discount information
                shipmentDTO.AppliedDiscount = 0;
                shipmentDTO.DiscountValue = 0;

                shipmentDTO.Insurance = 0;
                shipmentDTO.Vat = 0;
                shipmentDTO.Total = (decimal)totalChargeAmount;
                shipmentDTO.ShipmentPackagePrice = 0;
                shipmentDTO.ShipmentPickupPrice = 0;

                //from client
                shipmentDTO.vatvalue_display = 0;
                shipmentDTO.InvoiceDiscountValue_display = 0;
                shipmentDTO.offInvoiceDiscountvalue_display = 0;

                //payment method
                shipmentDTO.PaymentMethod = mDto.MagayaPaymentType;

                //ShipmentCollection
                shipmentDTO.ShipmentCollection = new ShipmentCollectionDTO();
                shipmentDTO.IsCancelled = false;
                shipmentDTO.IsInternational = true;
                shipmentDTO.Description = "";

                //Sender's Address - added for the special case of corporate customers
                shipmentDTO.SenderAddress = magayaShipmentDTO.ShipperAddress.Street[0];
                shipmentDTO.SenderState = magayaShipmentDTO.ShipperAddress.State;
                shipmentDTO.IsFromMobile = false;
                shipmentDTO.isInternalShipment = false;

                //Country info
                shipmentDTO.DepartureCountryId = currentUser.UserActiveCountryId;
                shipmentDTO.DestinationCountryId = currentUser.UserActiveCountryId;
                shipmentDTO.ShipmentHash = "";


                //Drop Off
                shipmentDTO.TempCode = "";

                return shipmentDTO;

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IntlShipmentRequestDTO> CreateIntlShipmentRequest(IntlShipmentRequestDTO shipmentDTO)
        {

            var station = await _stationService.GetStationById(shipmentDTO.StationId);
            var customer = await _individualCustomerService.GetCustomerById(shipmentDTO.CustomerId);

            //get the current user info
            var currentUserId = await _userService.GetCurrentUserId();
            shipmentDTO.UserId = currentUserId;

            var destinationServiceCenter = _uow.ServiceCentre.SingleOrDefault(s => s.ServiceCentreId == station.SuperServiceCentreId);

            if (string.IsNullOrEmpty(shipmentDTO.RequestNumber))
            {
                var RequestNumber = await _numberGeneratorMonitorService.GenerateNextNumber(NumberGeneratorType.RequestNumber, destinationServiceCenter.Code);
                shipmentDTO.RequestNumber = RequestNumber;
            }

            var newShipment = await MapIntlShipmentRequest(shipmentDTO);
            newShipment.ReceiverCountry = station.Country;
            newShipment.DestinationServiceCentreId = station.SuperServiceCentreId;
            newShipment.DestinationCountryId = Convert.ToInt32(station.Country);
            newShipment.ReceiverCountry = shipmentDTO.ReceiverCountry;

            var serialNumber = 1;
            foreach (var shipmentItem in newShipment.ShipmentRequestItems)
            {
                shipmentItem.SerialNumber = serialNumber;

                //check for volumetric weight
                if (shipmentItem.IsVolumetric)
                {
                    double volume = (shipmentItem.Length * shipmentItem.Height * shipmentItem.Width) / 5000;
                    double Weight = shipmentItem.Weight > volume ? shipmentItem.Weight : volume;
                    newShipment.ApproximateItemsWeight += Weight;
                    newShipment.GrandTotal += shipmentItem.Price;
                }
                else
                {
                    newShipment.ApproximateItemsWeight += shipmentItem.Weight;
                }

                serialNumber++;
            }

            _uow.IntlShipmentRequest.Add(newShipment);
            await _uow.CompleteAsync();
            return shipmentDTO;
        }

        public async Task<IntlShipmentRequest> MapIntlShipmentRequest(IntlShipmentRequestDTO r)
        {
            var serviceCenters = await _uow.ServiceCentre.GetServiceCentresByStationId(r.StationId);
            var sc = serviceCenters.Where(c => c.ServiceCentreId == r.DestinationServiceCentreId).Select(x => new ServiceCentre()
            {
                Code = x.Code,
                Name = x.Name
            }).FirstOrDefault();

            IntlShipmentRequest varEntity = new IntlShipmentRequest()
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
                PickupOptions = r.PickupOptions,
                DestinationServiceCentreId = r.DestinationServiceCentreId,
                DestinationServiceCentre = sc,
                ReceiverAddress = r.ReceiverAddress,
                ReceiverCity = r.ReceiverCity,
                ReceiverState = r.ReceiverState,
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
                ShipmentRequestItems = r.ShipmentRequestItems.Select(c => new IntlShipmentRequestItem()
                {
                    Description = c.Description,
                    storeName = c.storeName,
                    TrackingId = c.TrackingId,
                    ItemName = c.ItemName,
                    ShipmentType = c.ShipmentType,
                    Weight = c.Weight,
                    Nature = c.Nature,
                    Price = c.Price,
                    Quantity = c.Quantity,
                    SerialNumber = c.SerialNumber,
                    IsVolumetric = c.IsVolumetric,
                    Length = c.Length,
                    Width = c.Width,
                    Height = c.Height
                }).ToList()
            };

            return varEntity;
        }

        public async Task<string> GetMagayaWayBillNumber()
        {
            var mwaybill = await _numberGeneratorMonitorService.GenerateNextNumber(NumberGeneratorType.MagayaWb, "MG");
            return mwaybill;
        }

        //For creating shipment in Magaya
        public string SetEntity(int access_key, EntityDto entitydto)
        {
            //2. initialize type of shipment and flag
            int flags = 0x00000800;

            //3. initilize the variables to hold some parameters and return values
            string entity_xml = string.Empty;
            var errval = string.Empty;

            //4. initialize the serializer object
            Serializer sr = new Serializer();

            //5. serialize object to xml from class warehousereceipt
            var entitydata = new Entity();
            var xmlobject = Mapper.Map<Entity>(entitydto);

            api_session_error result = api_session_error.no_error;

            try
            {
                //serialize to xml for the magaya request
                entity_xml = sr.Serialize<Entity>(xmlobject);

                //trans_xml = sr.ConvertObjectToXMLString(shipmentdata);
                string error_code = "";
                result = cs.SetEntity(access_key, flags, entity_xml, out error_code);
                errval = error_code;
            }
            catch (Exception ex)
            {
                errval = ex.Message;
            }

            return errval;

        }

        public Task<string> SetEntityIntl(CustomerDTO custDTo)
        {
            int access_key;

            //1. Call the open connection to get the session key
            var openconn = OpenConnection(out access_key);

            var entitydto = new EntityDto();
            entitydto.Name = custDTo?.FirstName + " " + custDTo.LastName;
            entitydto.Type = EntityDesc.Client;
            entitydto.CreatedOn = DateTime.Now;

            entitydto.Address = new Address();
            entitydto.Address.Street = new string[2];

            entitydto.Address.Street[0] = custDTo.Address;
            entitydto.Address.City = custDTo.City;
            entitydto.Address.State = custDTo.State;
            entitydto.Address.ZipCode = "";

            entitydto.Address.Country = new Country();
            entitydto.Address.Country.Code = custDTo.Country?.CountryCode;
            entitydto.Address.Country.Value = custDTo.Country?.CountryName;

            entitydto.Address.ContactName = custDTo.FirstName + " " + custDTo.LastName;
            entitydto.Address.ContactPhone = custDTo.PhoneNumber;
            entitydto.Address.ContactEmail = custDTo.Email;

            entitydto.BillingAddress = new Address();
            entitydto.BillingAddress.Street = new string[2];

            entitydto.BillingAddress.ContactName = custDTo.FirstName + " " + custDTo.LastName;
            entitydto.BillingAddress.ContactPhone = custDTo.PhoneNumber;
            entitydto.BillingAddress.ContactEmail = custDTo.Email;

            entitydto.Email = custDTo.Email;
            entitydto.Phone = custDTo.Email;
            entitydto.GUID = Guid.NewGuid().ToString();

            entitydto.CreatedOn = DateTime.Now;
            entitydto.Customs = null;
            entitydto.Division = null;
            entitydto.AgentInfo = null;
            entitydto.CarrierInfo = null;


            //2. initialize type of shipment and flag
            int flags = 0x00000800;

            //3. initilize the variables to hold some parameters and return values
            string entity_xml = string.Empty;
            var errval = string.Empty;

            //4. initialize the serializer object
            Serializer sr = new Serializer();

            //5. serialize object to xml from class warehousereceipt
            var xmlobject = Mapper.Map<Entity>(entitydto);
            var customer = new CustomerDTO();

            api_session_error result = api_session_error.no_error;

            try
            {
                //serialize to xml for the magaya request
                entity_xml = sr.Serialize<Entity>(xmlobject);

                //Add customer in Magaya
                string error_code = "";
                result = cs.SetEntity(access_key, flags, entity_xml, out error_code);

                //result = api_session_error.no_error;                
                errval = error_code;
            }
            catch (Exception ex)
            {
                errval = ex.Message;
            }
            return Task.FromResult(errval);
        }

        public string GetTransactions(int access_key, WarehouseReceipt magayaShipmentDTO)
        {
            //2. initialize type of shipment and flag
            int flags = 0x00000800;

            //3. initilize the variables to hold some parameters and return values
            string entity_xml = string.Empty;
            var errval = string.Empty;

            //4.initialize the serializer object
            Serializer sr = new Serializer();

            //5. serialize object to xml from class warehousereceipt
            var xmlobject = Mapper.Map<WarehouseReceipt>(magayaShipmentDTO);

            api_session_error result = api_session_error.no_error;

            try
            {
                //serialize to xml for the magaya request
                entity_xml = sr.Serialize<WarehouseReceipt>(xmlobject);

                string error_code = "";
                result = cs.SetEntity(access_key, flags, entity_xml, out error_code);
                errval = error_code;
            }
            catch (Exception ex)
            {
                errval = ex.Message;
            }

            return errval;
        }

        public Task<Tuple<List<IntlShipmentRequestDTO>, int>> getIntlShipmentRequests(FilterOptionsDto filterOptionsDto)
        {
            var result = _shipmentService.GetIntlTransactionShipments(filterOptionsDto);
            return result;
        }

        public async Task<IntlShipmentRequestDTO> GetShipmentRequest(string requestNumber)
        {
            try
            {
                var shipment = await _uow.IntlShipmentRequest.GetAsync(x => x.RequestNumber.Equals(requestNumber));

                if (shipment == null)
                {
                    throw new GenericException($"Shipment with request Number: {requestNumber} does not exist", $"{(int)HttpStatusCode.NotFound}");
                }

                return await GetShipmentRequest(shipment.IntlShipmentRequestId);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IntlShipmentRequestDTO> GetShipmentRequest(int shipmentRequestId)
        {
            try
            {
                var shipment = await _uow.IntlShipmentRequest.GetAsync(x => x.IntlShipmentRequestId == shipmentRequestId, "ShipmentRequestItems");
                if (shipment == null)
                {
                    throw new GenericException("Shipment Information does not exist", $"{(int)HttpStatusCode.NotFound}");
                }

                var shipmentDto = Mapper.Map<IntlShipmentRequestDTO>(shipment);

                // get ServiceCentre
                var destinationServiceCentre = await _centreService.GetServiceCentreById(shipment.DestinationServiceCentreId);
                shipmentDto.DestinationServiceCentre = destinationServiceCentre;

                //get CustomerDetails
                if (shipmentDto.CustomerType.Contains("Individual"))
                {
                    shipmentDto.CustomerType = CustomerType.IndividualCustomer.ToString();
                }

                CustomerType customerType = (CustomerType)Enum.Parse(typeof(CustomerType), shipmentDto.CustomerType);

                //Set the Senders AAddress for the Shipment in the CustomerDetails
                shipmentDto.CustomerAddress = shipmentDto.SenderAddress;
                shipmentDto.CustomerState = shipmentDto.SenderState;

                return shipmentDto;
            }
            catch (Exception)
            {
                throw;
            }
        }


        public EntityList GetEntityObect()
        {
            Entity[] items = new Entity[1];
            items[0] = new Entity()
            {
                GUID = Guid.NewGuid().ToString()
            };
            return new EntityList()
            {
                Items = items
            };
        }

        //Get customers, forwarding agents, employees etc 
        public EntityList GetEntities(int access_key, string startwithstring)
        {
            //2. initialize type of entity and flag
            int flags = 0x00000800;

            //3. initilize the variables to hold some parameters and return values
            string entity_xml = string.Empty;
            EntityList errval = null;

            //initialize the serializer object
            Serializer sr = new Serializer();
            api_session_error result = api_session_error.no_error;

            try
            {
                //trans_xml = sr.ConvertObjectToXMLString(shipmentdata);
                string error_code = "";
                result = cs.GetEntities(access_key, flags, startwithstring, out error_code);
                var objectOfXml = sr.Deserialize<EntityList>(error_code);
                errval = objectOfXml;

                if (errval.Items != null)
                {
                    var resObj = errval.Items.ToList();
                    resObj.Add(new Entity()
                    {
                        GUID = Guid.NewGuid().ToString(),
                        Name = startwithstring,
                        CreatedOn = DateTime.Now,
                        CreatedOnSpecified = true,
                        Address = new Address()
                        {
                            Street = new string[1]
                        },
                        BillingAddress = new Address()
                        {
                            Street = new string[1]
                        },
                    }); ;
                    errval.Items = resObj.ToArray();
                }
                else
                {
                    Entity[] itemsVals = new Entity[1];
                    itemsVals[0] = new Entity()
                    {
                        GUID = Guid.NewGuid().ToString(),
                        Name = startwithstring,
                        CreatedOn = DateTime.Now,
                        CreatedOnSpecified = true,
                        Address = new Address()
                        {
                            Street = new string[1]
                        },
                        BillingAddress = new Address()
                        {
                            Street = new string[1]
                        },
                    };

                    errval.Items = itemsVals;
                }

            }
            catch (Exception ex)
            {
            }
            return errval;
        }

        //Get Magaya mode of transportation
        public List<ModeOfTransportation> GetModesOfTransportation()
        {
            //2. initialize type of entity and flag
            int flags = 0x00000800;

            //3. Get the objects for mode of transportation
            var listofModes = new List<ModeOfTransportation>();
            listofModes.Add(new ModeOfTransportation()
            {
                Code = "10",
                Description = "Air, Containerize",
                Method = MethodType.Air
            });

            listofModes.Add(new ModeOfTransportation()
            {
                Code = "13",
                Description = "Ground, Containerized",
                Method = MethodType.Ground
            });

            listofModes.Add(new ModeOfTransportation()
            {
                Code = "17",
                Description = "Rail",
                Method = MethodType.Rail
            });

            listofModes.Add(new ModeOfTransportation()
            {
                Code = "15",
                Description = "Occean, Containerized",
                Method = MethodType.Ocean
            });

            return listofModes;
        }

        //Get Magaya ports called routes in Agility
        public PortList GetPorts()
        {
            //2. initialize type of entity and flag
            int flags = 0x00000800;

            //3. initilize the variables to hold some parameters and return values
            string entity_xml = string.Empty;
            var listofPorts = new PortList();

            //initialize the serializer object
            Serializer sr = new Serializer();
            api_session_error result = api_session_error.no_error;

            try
            {
                string error_code = "";
                result = cs.GetWorkingPorts(out error_code);
                var objectOfXml = sr.Deserialize<PortList>(error_code);
                listofPorts = objectOfXml;
            }
            catch (Exception ex)
            {
            }

            return listofPorts;
        }

        //Get Magaya ports called routes in Agility
        public async Task<List<ServiceCentreDTO>> GetDestinationServiceCenters()
        {
            var result = await _centreService.GetServiceCentres();
            return result.ToList();
        }

        //Get Magaya packages list called  special shipment or so in  Agility
        public PackageList GetPackageList()
        {
            //2. initialize type of entity and flag
            int flags = 0x00000800;

            //3. initilize the variables to hold some parameters and return values
            string entity_xml = string.Empty;
            var listOfPackagss = new PackageList();

            //initialize the serializer object
            Serializer sr = new Serializer();
            api_session_error result = api_session_error.no_error;

            try
            {
                string error_code = "";
                result = cs.GetPackageTypes(out error_code);
                var objectOfXml = sr.Deserialize<PackageList>(error_code);
                listOfPackagss = objectOfXml;
            }
            catch (Exception ex)
            {
            }

            return listOfPackagss;
        }

        public LocationList GetLocations()
        {
            //2. initialize type of entity and flag
            //int flags = 0x00000800;

            //3. initilize the variables to hold some parameters and return values
            string entity_xml = string.Empty;
            var listOfLocations = new LocationList();

            //initialize the serializer object
            Serializer sr = new Serializer();
            api_session_error result = api_session_error.no_error;

            try
            {
                string error_code = "";
                //result = cs.GetPackageTypes(out error_code);
                //var path = Directory.GetCurrentDirectory() + @"\ThirdParty.WebServices\XML\Locations.xml";

                var rootDir = System.Web.HttpContext.Current.Server.MapPath("~");
                var path = rootDir + @"\MagayaDirectory\Locations.xml";

                var xmlInputData = File.ReadAllText(path);
                var objectOfXml = sr.Deserialize<LocationList>(xmlInputData);
                listOfLocations = objectOfXml;
            }
            catch (Exception ex)
            {

            }

            return listOfLocations;
        }

        public ChargeDefinitionList GetChargeDefinitionList(int access_key)
        {
            //2. initialize type of entity and flag
            int flags = 0x00000800;

            //3. initilize the variables to hold some parameters and return values
            string entity_xml = string.Empty;
            var listOfChargeDefinitions = new ChargeDefinitionList();

            //initialize the serializer object
            Serializer sr = new Serializer();
            api_session_error result = api_session_error.no_error;

            try
            {
                string error_code = "";
                result = cs.GetChargeDefinitions(access_key, out error_code);
                var objectOfXml = sr.Deserialize<ChargeDefinitionList>(error_code);
                listOfChargeDefinitions = objectOfXml;
            }
            catch (Exception ex)
            {
            }

            return listOfChargeDefinitions;
        }

        //Get List of Statuses for Items in Magaya
        public List<string> GetItemStatus()
        {
            //3.list of strings init
            var listofitemstatus = new List<string>();

            //convert all element of the enum of string list
            foreach (string s in Enum.GetNames(typeof(ItemStatusType)))
            {
                listofitemstatus.Add(s);
            }

            return listofitemstatus;
        }

        public Description CommodityDescription(string description)
        {
            //1. initilize the variables to hold some parameters and return values
            string entity_xml = string.Empty;
            var listOfDescription = new Description();

            //initialize the serializer object
            Serializer sr = new Serializer();
            api_session_error result = api_session_error.no_error;

            try
            {
                string error_code = "";

                var rootDir = System.Web.HttpContext.Current.Server.MapPath("~");
                var path = rootDir + @"\MagayaDirectory\commoditytypes.xml";

                var xmlInputData = File.ReadAllText(path);
                var objectOfXml = sr.Deserialize<Description>(xmlInputData);

                if (!String.IsNullOrEmpty(description))
                {
                    var Dtype = new DescriptionType();
                    Dtype.Description = description;
                    Dtype.ItemNo = "29";
                    objectOfXml.DescriptionType.Add(Dtype);
                }

                listOfDescription = objectOfXml;
            }
            catch (Exception ex)
            {

            }
            return listOfDescription;
        }


        public TransactionTypes TransactionTypes()
        {
            //1. initilize the variables to hold some parameters and return values
            string entity_xml = string.Empty;
            var listOfTransactionType = new TransactionTypes();

            //initialize the serializer object
            Serializer sr = new Serializer();
            api_session_error result = api_session_error.no_error;

            try
            {
                string error_code = "";

                var rootDir = System.Web.HttpContext.Current.Server.MapPath("~");
                var path = rootDir + @"\MagayaDirectory\transactiontypes.xml";

                var xmlInputData = File.ReadAllText(path);
                var objectOfXml = sr.Deserialize<TransactionTypes>(xmlInputData);
                listOfTransactionType = objectOfXml;
            }
            catch (Exception ex)
            {

            }
            return listOfTransactionType;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="access_key"></param>
        /// <param name="querydto"></param>
        /// <returns></returns>
        public GUIDItemList QueryLog(int access_key, QuerylogDt0 querydto)
        {
            //3. initilize the variables to hold some parameters and return values
            var listOfQuidItems = new GUIDItemList();

            //initialize the serializer object
            Serializer sr = new Serializer();
            api_session_error result = api_session_error.no_error;

            try
            {
                var trans_xml_out = string.Empty;
                result = cs.QueryLog(access_key, querydto.start_date, querydto.end_date, querydto.log_entry_type, querydto.trans_type, querydto.flags, out trans_xml_out);
                var objectOfXml = sr.Deserialize<GUIDItemList>(trans_xml_out);
                listOfQuidItems = objectOfXml;
            }
            catch (Exception ex)
            {
            }

            return listOfQuidItems;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="access_key"></param>
        /// <param name="querydto"></param>
        /// <returns></returns>
        public TransactionResults LargeQueryLog(int access_key, QuerylogDt0 querydto)
        {
            //call query log and retrieve the transactions GUIDs
            var listOfResults = QueryLog(access_key, querydto);

            //initialize the serializer object
            Serializer sr = new Serializer();
            api_session_error result = api_session_error.no_error;

            //2. initilize the variables to hold some parameters and return values
            var listOfWarehousereceipt = new List<WarehouseReceipt>();
            var listOfShipment = new List<MagayaShipment>();
            var listOfInvoice = new List<AccountingItem>();
            var listOfPayment = new List<ThirdParty.WebServices.Magaya.Business.New.PaymentType>();

            //test for null;

            foreach (var list in listOfResults.GUIDItem)
            {
                //get the transaction
                var guid = list.GUID;
                string xmlTransList;

                var resultVal = cs.GetTransaction(access_key, querydto.trans_type, querydto.flags, guid, out xmlTransList);

                if (resultVal == api_session_error.no_error)
                {
                    switch (querydto.trans_type)
                    {
                        case "WH":
                            //listOfWarehousereceipt = sr.Deserialize<WarehouseReceipt>(xmlTransList);
                            listOfWarehousereceipt.Add(sr.Deserialize<WarehouseReceipt>(xmlTransList));
                            break;
                        case "SH":
                            //listOfShipment = sr.Deserialize<ShipmentList>(xmlTransList);
                            listOfShipment.Add(sr.Deserialize<MagayaShipment>(xmlTransList));
                            break;
                        case "IN":
                            //listOfInvoice = sr.Deserialize<InvoiceList>(xmlTransList);
                            listOfInvoice.Add(sr.Deserialize<AccountingItem>(xmlTransList));
                            break;
                        case "PM":
                            //listOfPayment = sr.Deserialize<PaymentList>(xmlTransList);
                            listOfPayment.Add(sr.Deserialize<ThirdParty.WebServices.Magaya.Business.New.PaymentType>(xmlTransList));
                            break;
                        default:
                            break;
                    }
                }


            }

            var tran_result = new TransactionResults()
            {
                warehousereceipt = new WarehouseReceiptList() { WarehouseReceipt = listOfWarehousereceipt.Select(s => s).ToArray() },
                shipmentlist = new ShipmentList() { Items = listOfShipment.Select(s => s).ToArray() },
                invoicelist = new InvoiceList() { Items = listOfInvoice.Select(s => s).ToArray() },
                paymentlist = new PaymentList() { Items = listOfPayment.Select(s => s).ToArray() },
            };
            return tran_result;
        }

        public WarehouseReceiptList GetWarehouseReceiptRangeByDate(int access_key, QuerylogDt0 querydto)
        {
            //2. initilize the variables to hold some parameters and return values
            var listOftransactions = new WarehouseReceiptList();

            //initialize the serializer object
            Serializer sr = new Serializer();
            api_session_error result = api_session_error.no_error;

            try
            {
                var trans_xml_out = string.Empty;
                result = cs.GetTransRangeByDate(access_key, querydto.trans_type, querydto.start_date, querydto.end_date, querydto.flags, out trans_xml_out);

                var objectOfXml = sr.Deserialize<WarehouseReceiptList>(trans_xml_out);
                listOftransactions = objectOfXml;
            }
            catch (Exception ex)
            {
            }

            return listOftransactions;
        }

        public Tuple<WarehouseReceiptList, ShipmentList, InvoiceList, PaymentList> GetFirstTransbyDate(int access_key, QuerylogDt0 querydto, out string
            customcookie, out int more_result)
        {
            //2. initilize the variables to hold some parameters and return values
            var listOfWarehousereceipt = new WarehouseReceiptList();
            var listOfShipment = new ShipmentList();
            var listOfInvoice = new InvoiceList();
            var listOfPayment = new PaymentList();

            //initialize the serializer object
            Serializer sr = new Serializer();
            api_session_error result = api_session_error.no_error;
            querydto.end_date = (String.IsNullOrEmpty(querydto.end_date)) ? DateTime.Now.ToString("yyyy-MM-dd") : querydto.end_date;

            try
            {

                result = cs.GetFirstTransbyDate(access_key, querydto.trans_type, querydto.start_date, querydto.end_date, querydto.flags, querydto.record_quatity,
                    querydto.backwards_order, out string cookies, out more_result);
                string xmlTransList;
                customcookie = cookies;
                var res = GetNextTransByDate(access_key, ref customcookie, out xmlTransList, out more_result);
                var objectOfXml = new Object();

                switch (querydto.trans_type)
                {
                    case "WH":
                        listOfWarehousereceipt = sr.Deserialize<WarehouseReceiptList>(xmlTransList);
                        break;
                    case "SH":
                        listOfShipment = sr.Deserialize<ShipmentList>(xmlTransList);
                        break;
                    case "IN":
                        listOfInvoice = sr.Deserialize<InvoiceList>(xmlTransList);
                        break;
                    case "PM":
                        listOfPayment = sr.Deserialize<PaymentList>(xmlTransList);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                customcookie = "";
                more_result = 0;
            }

            var TupleResult = Tuple.Create<WarehouseReceiptList, ShipmentList, InvoiceList, PaymentList>(listOfWarehousereceipt, listOfShipment, listOfInvoice, listOfPayment);
            return TupleResult;
        }

        public bool GetFirstTransByDate(int access_key, QuerylogDt0 querydto, out string cookie, out int more_results)
        {
            try
            {
                api_session_error result = cs.GetFirstTransbyDate(access_key, querydto.trans_type, querydto.start_date, querydto.end_date, querydto.flags, querydto.record_quatity, querydto.backwards_order, out string cookies, out int more_res);
                cookie = cookies;
                more_results = more_res;
                return result == api_session_error.no_error;

            }
            catch
            {
                cookie = string.Empty;
                more_results = 0;
                return false;
            }
        }

        public bool GetNextTransByDate(int access_key, ref string cookie, out string trans_list_xml, out int more_results)
        {

            BasicHttpBinding binding = new BasicHttpBinding(BasicHttpSecurityMode.None);
            binding.MaxReceivedMessageSize = int.MaxValue;
            binding.MaxBufferSize = int.MaxValue;

            try
            {
                api_session_error result = cs.GetNextTransbyDate(ref cookie, out trans_list_xml, out more_results);
                return result == api_session_error.no_error;
            }
            catch (Exception ex)
            {
                trans_list_xml = null;
                more_results = 0;
                return false;
            }
        }

        public Tuple<WarehouseReceiptList, ShipmentList, InvoiceList, PaymentList> GetNextTransByDate2(int access_key, out int more_results, ref string cookie, string type)
        {
            var listOfWarehousereceipt = new WarehouseReceiptList();
            var listOfShipment = new ShipmentList();
            var listOfInvoice = new InvoiceList();
            var listOfPayment = new PaymentList();

            string xmlTransList;

            Serializer sr = new Serializer();

            try
            {
                //string xmlTransList;
                api_session_error result = cs.GetNextTransbyDate(ref cookie, out xmlTransList, out more_results);

                switch (type)
                {
                    case "WH":
                        listOfWarehousereceipt = sr.Deserialize<WarehouseReceiptList>(xmlTransList);
                        break;
                    case "SH":
                        listOfShipment = sr.Deserialize<ShipmentList>(xmlTransList);
                        break;
                    case "IN":
                        listOfInvoice = sr.Deserialize<InvoiceList>(xmlTransList);
                        break;
                    case "PM":
                        listOfPayment = sr.Deserialize<PaymentList>(xmlTransList);
                        break;
                    default:
                        break;
                }

                var TupleResult = Tuple.Create<WarehouseReceiptList, ShipmentList, InvoiceList, PaymentList>(listOfWarehousereceipt,
                    listOfShipment, listOfInvoice, listOfPayment);
                return TupleResult;
            }
            catch (Exception ex)
            {
                xmlTransList = null;
                var TupleResult = Tuple.Create<WarehouseReceiptList, ShipmentList, InvoiceList, PaymentList>(listOfWarehousereceipt, listOfShipment, listOfInvoice, listOfPayment);
                more_results = 0;
                return TupleResult;
            }
        }

    }


}
