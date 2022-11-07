﻿namespace POST.Core.Enums
{
    public enum ShipmentScanStatus
    {
        CRT, //1ST SCAN FOR EVERY SHIPMENT
        TRO, //WHEN YOU TRANSFER SHIPMENT FROM WHERE IT IS CREATED TO OPS CENTER WITHIN OR TO THE HUB
        ARO, //WHEN SHIPMENT ARRIVED THE OPS CENTER OR HUB FOR FURTHER PROCESSING
        DSC, //WHEN SHIPMENT DEPARTS SERVICE CENTER DIRECTLY TO DESTINATION
        DTR, //WHEN SHIPMENT DEPARTS SERVICE CENTER TO THE HUB OR ANOTHER CENTER THAT IS NOT THE FINAL DESTINATION
        AST, //WHEN SHIPMENT ARRIVES THE HUB OR ANOTHER CENTER THAT IS NOT THE FINAL DESTINATION
        DST, //WHEN SHIPMENT IN TRANSIT DEPARTS THE HUB OR ANOTHER CENTER THAT IS NOT THE FINAL DESTINATION
        ARP, //WHEN SHIPMENT ARRIVED THE HUB FOR FURTHER PROCESSING TO ANOTHER TRANSIT CENTER OR FINAL DESTINATION
        APT, // WHEN SHIPMENT IS ALREADY PROCESSED BUT IN TRANSIT THROUGH THE HUB
        DPC, //WHEN SHIPMENT DEPARTS PROCESSING CENTER
        ARF, //WHEN SHIPMENT ARRIVED FINAL DESTINATION
        AD, //DAILY SCAN UNTIL SHIPMENT IS DELIVERED
        OKT, //WHEN SHIPMENT IS DELIVERED AT THE TERMINAL TO THE RECEIVER
        GOP, //SCAN AT SERVICE CENTER WHEN SHIPMENT IS TRANSFERRED TO OPS FOR HOME DELIVERY
        WC, //SCAN BEFORE SHIPMENT IS TAKEN OUT FOR DELIVERY TO RECEIVER
        OKC, //SCAN TO SHOW SHIPMENT HAS BEEN DELIVERED
        SSR,  //SCAN SHIPMENT FOR RETURNS
        SSC,  //SCAN SHIPMENT FOR CANCELLED
        SRR,  //SCAN SHIPMENT FOR REROUTE
        SRC, //SCAN FOR SHIPMENT RECEIVED FROM COURIER
        PRECRT, //1ST SCAN FOR EVERY PRESHIPMENT CREATED
        PRESSC, //SCAN PRESHIPMENT FOR CANCELLED
        AHD, //SHIPMENT ARRIVED FINAL DESTINATION FOR HOME DELIVERY 
        ATD, //WHEN DELIVERY ATTEMPT IS MADE
        ADF,  //SHIPMENT ARRIVED DELIVERY FACILITY TO BE PROCESSED OUT FOR DELIVERY
        MCRT, //SHIPMENT CREATED BY CUSTOMER
        MAPT, //SHIPMENT REQUEST ACCEPTED BY DISPATCH RIDER
        MSHC, //SHIPMENT ENROUTE DELIVERY
        MSVC,  //SHIPMENT ARRIVED SERVICE CENTRE FOR ONWARD PROCESSING
        MAHD, //SHIPMENT DELIVERED
        SMIM, //SHIPMENT MISSED DURING ARRIVAL TRANSIT MANIFEST
        FMS,   //FOUND MISSING SHIPMENT
        MSCC,  //SHIPMENT CANCELLED BY CUSTOMER
        MSCP,   //SHIPMENT CANCELLED BY PARTNER
        DLD,  //DELAYED DELIVERY	
        ACC, //WHEN SHIPMENT ARRIVES COLLATION CENTER
        DCC,  //WHEN SHIPMENT DEPARTS COLLATION CENTER
        MNT, //WHEN MANIFEST IS NOT FOUND IN THE SUPERMANIFEST
        DLP,  //DELAYED PICKUP
        THIRDPARTY, //For 3rd party and gigm captain
        PICKED, // PICKED UP
        AISN, //ARRIVE INTL SHIPMENT TO NIGERIA
        ISFUK, //Item Shipped from UK
        ODMV, //ON THE MOVE
        SRFS, //SHIPMENT RECEIVED FROM THE STORE TO GIG HUB OR CENTRE
        IDH, // International Shipment Depart Houson in Transit
        IDK, // International Shipment Depart UK in Transit
        DUBC, // DELAYED PICK UP BY CUSTOMER
    }
}
