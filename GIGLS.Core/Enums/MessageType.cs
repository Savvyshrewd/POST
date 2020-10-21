﻿namespace GIGLS.Core.Enums
{
    public enum MessageType
    {
        ShipmentCreation,
        UserAccountCreation,
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
        AHD, //WHEN SHIPMENT ARRIVED FINAL DESTINATION FOR HOME DELIVERY
        PreShipmentCreation,
        CRH, //SHIPMENT CREATION SCAN FOR HOME DELIVERY - EMAIL
        ISE, //SHIPMENT CREATION SCAN FOR INTERNAIONAL SHIPMENT - EMAIL
        USER_LOGIN, //USER LOGIN
        IEMAIL, //FOR DUE INVOICES FOR CORPORATE CLIENTS
        WEMAIL, //WALLET BALANCES AT 10K AND 5K FOR CORPORATE CLIENTS
        SSC_Email,  //Message Type for use in sending email
        MATD,  //Message for Attempted Delivery
        PEmail, //Message Type for Forgot Password
        MRB, //Mesage For Referral Bonus
        SMIM, //SHIPMENT MISSED DURING ARRIVAL TRANSIT MANIFEST
        FMS,  //FOUND MISSING SHIPMENT
        OTP, //Message for One Time Password
        UDM, //Message for shipment that has exceeded delivery time
        WEBPICKUP, //Message for website mail for Schedlue Pickup
        WEBQUOTE, //Message for website mail for Request Quote
        APPREPORT, //Mail sent containing Issues reported by GIGGo Users
        HOUSTON,   //Create Message to handle CRT for Hoston Shipment
        FPEmail, //Message Type for Fleet Partner Login Access
        CEMAIL, // Account creation mail,
        MCS, //Message for Mobile Create Shipment
        ENM, //ECOMMERCE NOTIFICATION  MESSAGE FOR GIGGO CUSTOMER REGISTRATION
        DLD,  //DELAYED DELIVERY	
        MMCS, //Message for Multiple Mobile Create Shipment
        ARFS, // WHEN GIGL STORE SHIPMENT ARRIVES FINAL DESTINATION
        DBDO, // When discrepancy has been identified during confirmation of bank deposit
        SRMEmail, // Message for Regional Managers when Store Keeper sends shipment to their region
        INTLPEMAIL,  //INTERNATIONAL SHIPMENT PROCESS EMAIL
        RMCS, //Message for Receiver Delivery Code
        REQMAIL, //International Message for customer
        REQSCA //International Message for Service centre
    } 
}
