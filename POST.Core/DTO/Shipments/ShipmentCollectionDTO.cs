﻿using POST.Core.DTO.ServiceCentres;
using POST.Core.DTO.Shipments;
using POST.Core.Enums;
using System;

namespace POST.CORE.DTO.Shipments
{
    public class ShipmentCollectionDTO : BaseDomainDTO
    {
        public string Waybill { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string IndentificationUrl { get; set; }
        public ShipmentScanStatus ShipmentScanStatus { get; set; }
        public string Location { get; set; }

        //IsCashOnDelivery Processing
        public string WalletNumber { get; set; }
        public bool IsCashOnDelivery { get; set; }
        public decimal? CashOnDeliveryAmount { get; set; }
        public string Description { get; set; }
        
        //Demurrage Information
        public DemurrageDTO Demurrage { get; set; }

        //Who processed the collection
        public string UserId { get; set; }

        //original service centres
        public int DepartureServiceCentreId { get; set; }
        public int DestinationServiceCentreId { get; set; }
        public ServiceCentreDTO OriginalDepartureServiceCentre { get; set; }
        public ServiceCentreDTO OriginalDestinationServiceCentre { get; set; }

        public PaymentType PaymentType { get; set; }
        public string PaymentTypeReference { get; set; }

        //boolean to check if release is coming from mobile
        public bool IsComingFromDispatch { get; set; }
       
        public string ReceiverArea { get; set; }
        public string DeliveryAddressImageUrl { get; set; }
        public string DeliveryNumber { get; set; }
        public string ActualDeliveryAddress { get; set; }

    }



    public class ShipmentCollectionForContactDTO : BaseDomainDTO
    {
        public string Waybill { get; set; }
        public string ReceiverName { get; set; }
        public string ReceiverPhoneNumber { get; set; }
        public ShipmentScanStatus ShipmentScanStatus { get; set; }
        public string DepartsureServiceCentre { get; set; }
        public string DestinationServiceCentre { get; set; }
        public int ShipmentContactId { get; set; }
        public string ContactedBy { get; set; }
        public int NoOfContact { get; set; }


    }


    public class ShipmentCollectionDTOForFastTrack
    {
        public string Waybill { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public ShipmentScanStatus ShipmentScanStatus { get; set; }
        public string Location { get; set; }

        //IsCashOnDelivery Processing
        public string WalletNumber { get; set; }
        public bool IsCashOnDelivery { get; set; }
        public decimal? CashOnDeliveryAmount { get; set; }
        public string Description { get; set; }

        //Who processed the collection
        public string UserId { get; set; }

        //original service centres
        public int DepartureServiceCentreId { get; set; }
        public int DestinationServiceCentreId { get; set; }
        public PaymentType PaymentType { get; set; }
        public string PaymentTypeReference { get; set; }

        //boolean to check if release is coming from mobile
        public bool IsComingFromDispatch { get; set; }

        public string ReceiverArea { get; set; }
        public string DeliveryNumber { get; set; }
        //Demurrage Information
        public NewDemurrageDTO Demurrage { get; set; }

    }

    public class ShipmentCollectionDTOForArrived
    {
        public string Waybill { get; set; }
        public string UserId { get; set; }
        public DateTime ShipmentCreatedDate { get; set; }
        public DateTime ShipmentArrivedDate { get; set; }
        public string ReceiverName { get; set; }
        public string ReceiverPhoneNumber { get; set; }
        public int Age { get; set; }
        public int ShipmentStatus { get; set; }
        public string DepartureServiceCentre { get; set; }
        public string DestinationServiceCentre { get; set; }
    }
}
