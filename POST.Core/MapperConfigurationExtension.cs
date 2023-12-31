﻿using AutoMapper;
using POST.Core.DTO.Zone;
using GIGL.POST.Core.Domain;
using POST.Core.Domain;
using POST.Core.DTO.ServiceCentres;
using POST.Core.DTO.User;
using POST.Core.DTO;
using POST.Core.DTO.PaymentTransactions;
using POST.Core.Domain.Partnership;
using POST.Core.DTO.Partnership;
using POST.Core.DTO.Fleets;
using POST.Core.DTO.Customers;
using POST.Core.DTO.Shipments;
using POST.Core.DTO.Account;
using POST.Core.DTO.Client;
using POST.CORE.Domain;
using POST.CORE.DTO.Nav;
using POST.CORE.DTO.Shipments;
using POST.Core.DTO.Haulage;
using POST.Core.Domain.Wallet;
using POST.Core.DTO.Wallet;
using POST.Core.DTO.MessagingLog;
using POST.Core.Domain.MessagingLog;
using POST.Core.Domain.ShipmentScan;
using POST.Core.DTO.ShipmentScan;
using POST.Core.Domain.Utility;
using POST.Core.DTO.Utility;
using POST.Core.View;
using POST.Core.Domain.Devices;
using POST.Core.DTO.Devices;
using POST.Core.Domain.BankSettlement;
using POST.Core.DTO.BankSettlement;
using POST.Core.DTO.InternationalShipmentDetails;
using POST.Core.DTO.SLA;
using POST.Core.Domain.SLA;
using POST.Core.DTO.Expenses;
using POST.Core.Domain.Expenses;
using ThirdParty.WebServices.Magaya.DTO;
using ThirdParty.WebServices.Magaya.Business;
using ThirdParty.WebServices;
using ThirdParty.WebServices.Magaya.Business.New;
using ThirdParty.WebServices.Business;
using POST.Core.Domain.DHL;
using POST.Core.DTO.DHL;
using POST.Core.Domain.Archived;
using POST.Core.DTO.Stores;
using Country = POST.Core.Domain.Country;
using POST.Core.DTO.Captains;

namespace POST.Core
{
    public static class MapperConfigurationExtension
    {
        public static void ServiceMapConfigurations(this IRuntimeMapper mapper) { }
    }

    public class MapperConfig
    {
        private static bool isInit = false;
        public static void Initialize()
        {
            if (isInit)
            {
                return;
            }

            Mapper.Initialize(config =>
            {

                config.CreateMap<DomesticRouteZoneMapDTO, DomesticRouteZoneMap>();
                config.CreateMap<DomesticRouteZoneMap, DomesticRouteZoneMapDTO>();

                config.CreateMap<CountryRouteZoneMapDTO, CountryRouteZoneMap>();
                config.CreateMap<CountryRouteZoneMap, CountryRouteZoneMapDTO>();

                config.CreateMap<ZoneDTO, Zone>();
                config.CreateMap<Zone, ZoneDTO>();

                config.CreateMap<StationDTO, Station>();
                config.CreateMap<Station, StationDTO>();

                config.CreateMap<User, UserDTO>();
                config.CreateMap<UserDTO, User>();

                config.CreateMap<AppRole, RoleDTO>();
                config.CreateMap<RoleDTO, AppRole>();

                config.CreateMap<State, StateDTO>();
                config.CreateMap<StateDTO, State>();

                config.CreateMap<ServiceCentre, ServiceCentreDTO>();
                config.CreateMap<ServiceCentreDTO, ServiceCentre>();
                
                config.CreateMap<SpecialDomesticPackageDTO, SpecialDomesticPackage>();
                config.CreateMap<SpecialDomesticPackage, SpecialDomesticPackageDTO>();

                config.CreateMap<WeightLimit, WeightLimitDTO>();
                config.CreateMap<WeightLimitDTO, WeightLimit>();

                config.CreateMap<WeightLimitPrice, WeightLimitPriceDTO>();
                config.CreateMap<WeightLimitPriceDTO, WeightLimitPrice>();

                config.CreateMap<PaymentTransaction, PaymentTransactionDTO>();
                config.CreateMap<PaymentTransactionDTO, PaymentTransaction>();

                config.CreateMap<PaymentPartialTransaction, PaymentPartialTransactionDTO>();
                config.CreateMap<PaymentPartialTransactionDTO, PaymentPartialTransaction>();

                config.CreateMap<Partner, PartnerDTO>();
                config.CreateMap<PartnerDTO, Partner>();

                config.CreateMap<PartnerApplication, PartnerApplicationDTO>();
                config.CreateMap<PartnerApplicationDTO, PartnerApplication>();

                config.CreateMap<FleetMake, FleetMakeDTO>();
                config.CreateMap<FleetMakeDTO, FleetMake>();

                config.CreateMap<FleetModel, FleetModelDTO>();
                config.CreateMap<FleetModelDTO, FleetModel>();

                config.CreateMap<Fleet, FleetDTO>();
                config.CreateMap<FleetDTO, Fleet>();

                config.CreateMap<IndividualCustomer, IndividualCustomerDTO>();
                config.CreateMap<IndividualCustomerDTO, IndividualCustomer>();

                config.CreateMap<Shipment, ShipmentDTO>();
                config.CreateMap<ShipmentDTO, Shipment>();

                config.CreateMap<IntlShipmentRequest, IntlShipmentRequestDTO>();
                config.CreateMap<IntlShipmentRequestDTO, IntlShipmentRequest>();

                config.CreateMap<IntlShipmentRequestItem, IntlShipmentRequestItemDTO>();
                config.CreateMap<IntlShipmentRequestItemDTO, IntlShipmentRequestItem>();

                config.CreateMap<WaybillNumber, WaybillNumberDTO>();
                config.CreateMap<WaybillNumberDTO, WaybillNumber>();

                config.CreateMap<GroupWaybillNumber, GroupWaybillNumberDTO>();
                config.CreateMap<GroupWaybillNumberDTO, GroupWaybillNumber>();

                config.CreateMap<MovementManifestNumber, MovementManifestNumberDTO>();
                config.CreateMap<MovementManifestNumberDTO, MovementManifestNumber>(); //IMovementManifestNumberMappingRepository

                config.CreateMap<MovementManifestNumberMapping, MovementManifestNumberMappingDTO>();
                config.CreateMap<MovementManifestNumberMappingDTO, MovementManifestNumberMapping>();

                config.CreateMap<Company, CompanyDTO>();
                config.CreateMap<CompanyDTO, Company>();

                config.CreateMap<CompanyContactPerson, CompanyContactPersonDTO>();
                config.CreateMap<CompanyContactPersonDTO, CompanyContactPerson>();

                config.CreateMap<Manifest, ManifestDTO>();
                config.CreateMap<ManifestDTO, Manifest>();

                config.CreateMap<GeneralLedger, GeneralLedgerDTO>();
                config.CreateMap<GeneralLedgerDTO, GeneralLedger>();

                config.CreateMap<CustomerDTO, CompanyDTO>();
                config.CreateMap<CompanyDTO, CustomerDTO>();

                config.CreateMap<CustomerDTO, IndividualCustomerDTO>();
                config.CreateMap<IndividualCustomerDTO, CustomerDTO>();

                config.CreateMap<ClientNode, ClientNodeDTO>();
                config.CreateMap<ClientNodeDTO, ClientNode>();

                config.CreateMap<Invoice, InvoiceDTO>();
                config.CreateMap<InvoiceDTO, Invoice>();

                config.CreateMap<InvoiceView, InvoiceViewDTO>();
                config.CreateMap<InvoiceViewDTO, InvoiceView>();

                config.CreateMap<VAT, VATDTO>();
                config.CreateMap<VATDTO, VAT>();

                config.CreateMap<Insurance, InsuranceDTO>();
                config.CreateMap<InsuranceDTO, Insurance>();

                config.CreateMap<InvoiceShipment, InvoiceShipmentDTO>();
                config.CreateMap<InvoiceShipmentDTO, InvoiceShipment>();

                config.CreateMap<DeliveryOption, DeliveryOptionDTO>();
                config.CreateMap<DeliveryOptionDTO, DeliveryOption>();

                config.CreateMap<ShipmentItem, ShipmentItemDTO>();
                config.CreateMap<ShipmentItemDTO, ShipmentItem>();

                config.CreateMap<ShipmentCollection, ShipmentCollectionDTO>();
                config.CreateMap<ShipmentCollectionDTO, ShipmentCollection>();

                config.CreateMap<ShipmentReturn, ShipmentReturnDTO>();
                config.CreateMap<ShipmentReturnDTO, ShipmentReturn>();

                config.CreateMap<MainNav, MainNavDTO>();
                config.CreateMap<MainNavDTO, MainNav>();

                config.CreateMap<SubNav, SubNavDTO>();
                config.CreateMap<SubNavDTO, SubNav>();

                config.CreateMap<SubSubNav, SubSubNavDTO>();
                config.CreateMap<SubSubNavDTO, SubSubNav>();

                config.CreateMap<Message, MessageDTO>();
                config.CreateMap<MessageDTO, Message>();

                config.CreateMap<Haulage, HaulageDTO>();
                config.CreateMap<HaulageDTO, Haulage>();

                config.CreateMap<HaulageDistanceMapping, HaulageDistanceMappingDTO>();
                config.CreateMap<HaulageDistanceMappingDTO, HaulageDistanceMapping>();

                config.CreateMap<HaulageDistanceMappingPrice, HaulageDistanceMappingPriceDTO>();
                config.CreateMap<HaulageDistanceMappingPriceDTO, HaulageDistanceMappingPrice>();

                config.CreateMap<PackingList, PackingListDTO>();
                config.CreateMap<PackingListDTO, PackingList>();

                config.CreateMap<Wallet, WalletDTO>();
                config.CreateMap<WalletDTO, Wallet>();

                config.CreateMap<WalletTransaction, WalletTransactionDTO>();
                config.CreateMap<WalletTransactionDTO, WalletTransaction>();

                config.CreateMap<CashOnDeliveryBalance, CashOnDeliveryBalanceDTO>();
                config.CreateMap<CashOnDeliveryBalanceDTO, CashOnDeliveryBalance>();

                config.CreateMap<CashOnDeliveryAccount, CashOnDeliveryAccountDTO>();
                config.CreateMap<CashOnDeliveryAccountDTO, CashOnDeliveryAccount>();

                config.CreateMap<CashOnDeliveryRegisterAccount, CashOnDeliveryRegisterAccountDTO>();
                config.CreateMap<CashOnDeliveryRegisterAccountDTO, CashOnDeliveryRegisterAccount>();

                config.CreateMap<DemurrageRegisterAccount, DemurrageRegisterAccountDTO>();
                config.CreateMap<DemurrageRegisterAccountDTO, DemurrageRegisterAccount>();

                config.CreateMap<Demurrage, DemurrageDTO>();
                config.CreateMap<DemurrageDTO, Demurrage>();

                config.CreateMap<Dispatch, DispatchDTO>();
                config.CreateMap<DispatchDTO, DispatchDTO>();

                config.CreateMap<MovementDispatch, MovementDispatchDTO>();
                config.CreateMap<MovementDispatchDTO, MovementDispatch>();

                config.CreateMap<DispatchActivity, DispatchActivityDTO>();
                config.CreateMap<DispatchActivityDTO, DispatchActivity>();

                config.CreateMap<EmailSendLog, EmailSendLogDTO>();
                config.CreateMap<EmailSendLogDTO, EmailSendLog>();

                config.CreateMap<SmsSendLog, SmsSendLogDTO>();
                config.CreateMap<SmsSendLogDTO, SmsSendLog>();

                config.CreateMap<ScanStatus, ScanStatusDTO>();
                config.CreateMap<ScanStatusDTO, ScanStatus>();

                config.CreateMap<GlobalProperty, GlobalPropertyDTO>();
                config.CreateMap<GlobalPropertyDTO, GlobalProperty>();

                config.CreateMap<MissingShipment, MissingShipmentDTO>();
                config.CreateMap<MissingShipmentDTO, MissingShipment>();

                config.CreateMap<ShipmentCancel, ShipmentCancelDTO>();
                config.CreateMap<ShipmentCancelDTO, ShipmentCancel>();

                config.CreateMap<POST.Core.Domain.Country, CountryDTO>();
                config.CreateMap<CountryDTO, POST.Core.Domain.Country>(); 

                config.CreateMap<POST.Core.Domain.Country, NewCountryDTO>();
                config.CreateMap<NewCountryDTO, POST.Core.Domain.Country>();

                config.CreateMap<ShipmentReroute, ShipmentRerouteDTO>();
                config.CreateMap<ShipmentRerouteDTO, ShipmentReroute>();

                config.CreateMap<ShipmentPackagePrice, ShipmentPackagePriceDTO>();
                config.CreateMap<ShipmentPackagePriceDTO, ShipmentPackagePrice>();

                config.CreateMap<ManifestVisitMonitoring, ManifestVisitMonitoringDTO>();
                config.CreateMap<ManifestVisitMonitoringDTO, ManifestVisitMonitoring>();

                config.CreateMap<ManifestWaybillMapping, ManifestWaybillMappingDTO>();
                config.CreateMap<ManifestWaybillMappingDTO, ManifestWaybillMapping>();

                config.CreateMap<Device, DeviceDTO>();
                config.CreateMap<DeviceDTO, Device>();

                config.CreateMap<DeviceManagement, DeviceManagementDTO>();
                config.CreateMap<DeviceManagementDTO, DeviceManagement>();

                config.CreateMap<ShipmentDeliveryOptionMapping, ShipmentDeliveryOptionMappingDTO>();
                config.CreateMap<ShipmentDeliveryOptionMappingDTO, ShipmentDeliveryOptionMapping>();

                config.CreateMap<CODSettlementSheet, CODSettlementSheetDTO>();
                config.CreateMap<CODSettlementSheetDTO, CODSettlementSheet>();

                config.CreateMap<PreShipmentItem, PreShipmentItemDTO>();
                config.CreateMap<PreShipmentItemDTO, PreShipmentItem>();

                config.CreateMap<PreShipment, PreShipmentDTO>();
                config.CreateMap<PreShipmentDTO, PreShipment>();

                config.CreateMap<PreShipmentManifestMapping, PreShipmentManifestMappingDTO>();
                config.CreateMap<PreShipmentManifestMappingDTO, PreShipmentManifestMapping>();

                config.CreateMap<PreShipmentDTO, ThirdPartyPreShipmentDTO>();
                config.CreateMap<ThirdPartyPreShipmentDTO, PreShipmentDTO>();

                config.CreateMap<PreShipmentItemDTO, ThirdPartyPreShipmentItemDTO>();
                config.CreateMap<ThirdPartyPreShipmentItemDTO, PreShipmentItemDTO>();

                config.CreateMap<CustomerView, CustomerDTO>();
                config.CreateMap<CustomerDTO, CustomerView>();

                config.CreateMap<Domain.Notification, NotificationDTO>();
                config.CreateMap<NotificationDTO, Domain.Notification>();

                config.CreateMap<LogVisitReason, LogVisitReasonDTO>();
                config.CreateMap<LogVisitReasonDTO, LogVisitReason>();

                config.CreateMap<WalletPaymentLog, WalletPaymentLogDTO>();
                config.CreateMap<WalletPaymentLogDTO, WalletPaymentLog>();

                config.CreateMap<OverdueShipment, OverdueShipmentDTO>();
                config.CreateMap<OverdueShipmentDTO, OverdueShipment>();

                config.CreateMap<InternationalRequestReceiver, InternationalRequestReceiverDTO>();
                config.CreateMap<InternationalRequestReceiverDTO, InternationalRequestReceiver>();

                config.CreateMap<InternationalRequestReceiverItem, InternationalRequestReceiverItemDTO>();
                config.CreateMap<InternationalRequestReceiverItemDTO, InternationalRequestReceiverItem>();

                config.CreateMap<SLA, SLADTO>();
                config.CreateMap<SLADTO, SLA>();

                config.CreateMap<SLASignedUser, SLASignedUserDTO>();
                config.CreateMap<SLASignedUserDTO, SLASignedUser>();

                config.CreateMap<ExpenseType, ExpenseTypeDTO>();
                config.CreateMap<ExpenseTypeDTO, ExpenseType>();

                config.CreateMap<Expenditure, ExpenditureDTO>();
                config.CreateMap<ExpenditureDTO, Expenditure>();

                config.CreateMap<OTP, OTPDTO>();
                config.CreateMap<OTPDTO, OTP>();

                config.CreateMap<PreShipmentMobile, PreShipmentMobileDTO>();
                config.CreateMap<PreShipmentMobileDTO, PreShipmentMobile>();

                config.CreateMap<PreShipmentItemMobile, PreShipmentItemMobileDTO>();
                config.CreateMap<PreShipmentItemMobileDTO, PreShipmentItemMobile>();

                config.CreateMap<Location, LocationDTO>();
                config.CreateMap<LocationDTO, Location>();


                config.CreateMap<UserLoginEmail, UserLoginEmailDTO>();
                config.CreateMap<UserLoginEmailDTO, UserLoginEmail>();

                config.CreateMap<MobilePickUpRequests, MobilePickUpRequestsDTO>();
                config.CreateMap<MobilePickUpRequestsDTO, MobilePickUpRequests>();

                config.CreateMap<Region, RegionDTO>();
                config.CreateMap<RegionDTO, Region>();

                config.CreateMap<RegionServiceCentreMapping, RegionServiceCentreMappingDTO>();
                config.CreateMap<RegionServiceCentreMappingDTO, RegionServiceCentreMapping>();

                config.CreateMap<MobileScanStatus, MobileScanStatusDTO>();
                config.CreateMap<MobileScanStatusDTO, MobileScanStatus>();

                config.CreateMap<HUBManifestWaybillMapping, HUBManifestWaybillMappingDTO>();
                config.CreateMap<HUBManifestWaybillMappingDTO, HUBManifestWaybillMapping>();

                config.CreateMap<CodPayOutList, NewInvoiceViewDTO>();
                config.CreateMap<NewInvoiceViewDTO, CodPayOutList>();

                config.CreateMap<Category, CategoryDTO>();
                config.CreateMap<CategoryDTO, Category>();

                config.CreateMap<SubCategory, SubCategoryDTO>();
                config.CreateMap<SubCategoryDTO, SubCategory>();

                config.CreateMap<PartnerTransactions, PartnerTransactionsDTO>();
                config.CreateMap<PartnerTransactionsDTO, PartnerTransactions>();

                config.CreateMap<MobileRating, MobileRatingDTO>();
                config.CreateMap<MobileRatingDTO, MobileRating>();

                config.CreateMap<ReferrerCode, ReferrerCodeDTO>();
                config.CreateMap<ReferrerCodeDTO, ReferrerCode>();

                config.CreateMap<DeliveryNumber, DeliveryNumberDTO>();
                config.CreateMap<DeliveryNumberDTO, DeliveryNumber>();

                config.CreateMap<VehicleType,VehicleTypeDTO>();
                config.CreateMap<VehicleTypeDTO, VehicleType>();

                config.CreateMap<PickupManifest, PickupManifestDTO>();
                config.CreateMap<PickupManifestDTO, PickupManifest>();

                config.CreateMap<PickupManifestWaybillMapping, PickupManifestWaybillMappingDTO>();
                config.CreateMap<PickupManifestWaybillMappingDTO, PickupManifestWaybillMapping>();

                config.CreateMap<RiderDelivery, RiderDeliveryDTO>();
                config.CreateMap<RiderDeliveryDTO, RiderDelivery>();

                config.CreateMap<DeliveryLocation, DeliveryLocationDTO>();
                config.CreateMap<DeliveryLocationDTO, DeliveryLocation>();

                config.CreateMap<LGA, LGADTO>();
                config.CreateMap<LGADTO, LGA>();

                config.CreateMap<WaybillPaymentLog, WaybillPaymentLogDTO>();
                config.CreateMap<WaybillPaymentLogDTO, WaybillPaymentLog>();

                config.CreateMap<Bank, BankDTO>();
                config.CreateMap<BankDTO, Bank>();
                
                config.CreateMap<ActivationCampaignEmail, ActivationCampaignEmailDTO>();
                config.CreateMap<ActivationCampaignEmailDTO, ActivationCampaignEmail>();

                config.CreateMap<Shipment, PreShipmentMobileDTO>();
                config.CreateMap<PreShipmentMobileDTO, Shipment>();
                config.CreateMap<PreShipmentItemMobileDTO, ShipmentItem>();
                config.CreateMap<ShipmentItem, PreShipmentItemMobileDTO>();

                config.CreateMap<GiglgoStation, GiglgoStationDTO>();
                config.CreateMap<GiglgoStationDTO, GiglgoStation>();

                config.CreateMap<WarehouseReceipt, MagayaShipmentDto>();
                config.CreateMap<MagayaShipmentDto, WarehouseReceipt>();

                config.CreateMap<Entity, EntityDto>();
                config.CreateMap<EntityDto, Entity>();

                config.CreateMap<FleetPartner, FleetPartnerDTO>();
                config.CreateMap<FleetPartnerDTO, FleetPartner>();

                config.CreateMap<MobileGroupCodeWaybillMapping, MobileGroupCodeWaybillMappingDTO>();
                config.CreateMap<MobileGroupCodeWaybillMappingDTO, MobileGroupCodeWaybillMapping>();

                config.CreateMap<PartnerPayout, PartnerPayoutDTO>();
                config.CreateMap<PartnerPayoutDTO, PartnerPayout>();

                config.CreateMap<CreatePreShipmentMobileDTO, PreShipmentMobileDTO>();
                config.CreateMap<PreShipmentMobileDTO, CreatePreShipmentMobileDTO>();

                config.CreateMap<EcommerceAgreement, EcommerceAgreementDTO>();
                config.CreateMap<EcommerceAgreementDTO, EcommerceAgreement>();

                config.CreateMap<ShipmentDTO, PreShipmentMobileFromAgilityDTO>();
                config.CreateMap<PreShipmentMobileFromAgilityDTO, ShipmentDTO>();

                config.CreateMap<PreShipmentMobileDTO, PreShipmentMobileFromAgilityDTO>();
                config.CreateMap<PreShipmentMobileFromAgilityDTO, PreShipmentMobileDTO>();

                config.CreateMap<FinancialReport, FinancialReportDTO>();
                config.CreateMap<FinancialReportDTO, FinancialReport>();

                config.CreateMap<IntlShipmentRequestDTL, IntlShipmentRequestDTO>();
                config.CreateMap<IntlShipmentRequestDTO, IntlShipmentRequestDTL>();
                config.CreateMap<ShipmentDTO, NewShipmentDTO>();
                config.CreateMap<NewShipmentDTO, ShipmentDTO>();

                config.CreateMap<InternationalShipmentDTO, ShipmentDTO>();

                config.CreateMap<InternationalShipmentWaybill, InternationalShipmentWaybillDTO>();
                config.CreateMap<InternationalShipmentWaybillDTO, InternationalShipmentWaybill>();

                config.CreateMap<PriceCategory, PriceCategoryDTO>();
                config.CreateMap<PriceCategoryDTO, PriceCategory>();

                config.CreateMap<Store, StoreDTO>();
                config.CreateMap<StoreDTO, Store>();

                config.CreateMap<CaptainBonusByZoneMaping, CaptainBonusByZoneMapingDTO>();
                config.CreateMap<CaptainBonusByZoneMapingDTO, CaptainBonusByZoneMaping>();

                config.CreateMap<CustomerInvoice, CustomerInvoiceDTO>();
                config.CreateMap<CustomerInvoiceDTO, CustomerInvoice>();

                config.CreateMap<InternationalShipmentQuoteDTO, InternationalShipmentDTO>();
                config.CreateMap<InternationalShipmentDTO, InternationalShipmentQuoteDTO>();

                config.CreateMap<RateInternationalShipmentDTO, InternationalShipmentDTO>();
                config.CreateMap<InternationalShipmentDTO, RateInternationalShipmentDTO>();

                config.CreateMap<CreateInternationalShipmentDTO, InternationalShipmentDTO>();
                config.CreateMap<InternationalShipmentDTO, CreateInternationalShipmentDTO>();

                config.CreateMap<RateShipmentItem, ShipmentItemDTO>();
                config.CreateMap<ShipmentItemDTO, RateShipmentItem>();

                config.CreateMap<QuoteShipmentItem, ShipmentItemDTO>();
                config.CreateMap<ShipmentItemDTO, QuoteShipmentItem>();

                config.CreateMap<PreShipmentItemMobile, ShipmentItemDTO>();
                config.CreateMap<ShipmentItemDTO, PreShipmentItemMobile>();

                config.CreateMap<PreShipmentMobile, InternationalShipmentDTO>();
                config.CreateMap<InternationalShipmentDTO, PreShipmentMobile>();

                config.CreateMap<IndividualCustomer, CustomerDTO>();
                config.CreateMap<CustomerDTO, IndividualCustomer>();

                config.CreateMap<Company, CustomerDTO>();
                config.CreateMap<CustomerDTO, Company>();

                config.CreateMap<TransferDetails, TransferDetailsDTO>();
                config.CreateMap<TransferDetailsDTO, TransferDetails>();

                config.CreateMap<InternationalCargoManifestDetail, InternationalCargoManifestDetailDTO>();
                config.CreateMap<InternationalCargoManifestDetailDTO, InternationalCargoManifestDetail>();

                config.CreateMap<InternationalCargoManifest, InternationalCargoManifestDTO>();
                config.CreateMap<InternationalCargoManifestDTO, InternationalCargoManifest>();

                //Archived Marking
                config.CreateMap<Shipment_Archive, ShipmentDTO>();
                config.CreateMap<ShipmentItem_Archive, ShipmentItem>();
                config.CreateMap<ShipmentItem_Archive, ShipmentItemDTO>();
                config.CreateMap<GeneralLedger_Archive, GeneralLedgerDTO>();
                config.CreateMap<Invoice_Archive, InvoiceDTO>();
                config.CreateMap<TransitWaybillNumber_Archive, TransitWaybillNumberDTO>();

                config.CreateMap<ShipmentItemDTO, PreShipmentItemMobileDTO>();
                config.CreateMap<PreShipmentItemMobileDTO, ShipmentItemDTO>();

                config.CreateMap<InternationalShipmentDTO, PreShipmentMobileDTO>();
                config.CreateMap<PreShipmentMobileDTO, InternationalShipmentDTO>();

                config.CreateMap<ShipmentDTO, PreShipmentMobile>();
                config.CreateMap<PreShipmentMobile, ShipmentDTO>();

                config.CreateMap<CancelledShipmentDTO, PreShipmentMobile>();
                config.CreateMap<PreShipmentMobile, CancelledShipmentDTO>();

                config.CreateMap<UnidentifiedItemsForInternationalShippingDTO, UnidentifiedItemsForInternationalShipping>();
                config.CreateMap<UnidentifiedItemsForInternationalShipping, UnidentifiedItemsForInternationalShippingDTO>();

                config.CreateMap<CODWalletDTO, CODWallet>();
                config.CreateMap<CODWallet, CODWalletDTO>();

                config.CreateMap<LogEntryDTO, LogEntry>();
                config.CreateMap<LogEntry, LogEntryDTO>();

                config.CreateMap<InboundCategory, InboundCategoryDTO>();
                config.CreateMap<InboundCategoryDTO, InboundCategory>();

                config.CreateMap<GIGGOCODTransfer, GIGGOCODTransferDTO>();
                config.CreateMap<GIGGOCODTransferDTO, GIGGOCODTransfer>();

                config.CreateMap<GIGGOCODTransferResponseDTO, GIGGOCODTransfer>();
                config.CreateMap<GIGGOCODTransfer, GIGGOCODTransferResponseDTO>();

                config.CreateMap<InboundShipmentCategoryDTO, InboundShipmentCategory>();
                config.CreateMap<InboundShipmentCategory, InboundShipmentCategoryDTO>();
                config.CreateMap<ShipmentCategory, InboundShipmentCategoryDTO>();
                config.CreateMap<InboundShipmentCategoryDTO, ShipmentCategory>();
                config.CreateMap<InboundShipmentCategoryDTO, Country>();
                config.CreateMap<Country, InboundShipmentCategoryDTO>();

                // Fleet Job Card
                config.CreateMap<FleetJobCardDto, FleetJobCard>();
                config.CreateMap<FleetJobCard, FleetJobCardDto>();

                config.CreateMap<FleetTripDTO, FleetTrip>();
                config.CreateMap<FleetTrip, FleetTripDTO>();
                config.CreateMap<FleetPartnerTransactionDTO, FleetPartnerTransaction>();
                config.CreateMap<FleetPartnerTransaction, FleetPartnerTransactionDTO>();

                config.CreateMap<AzapayTransferDetailsDTO, TransferDetails>();
                config.CreateMap<TransferDetails, AzapayTransferDetailsDTO>();
            });

            isInit = true;
        }
    }
}
