﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace POST.Core.DTO.Shipments
{
    public class OutstandingPaymentsDTO
    {
        public string Waybill { get; set; }
        public string Departure { get; set; }
        public string Destination { get; set; }
        public decimal Amount { get; set; }
        public double DollarAmount { get; set; }
        public double PoundAmount { get; set; }
        public string CurrencySymbol { get; set; }
        public CountryDTO Country { get; set; }
        public string Description { get; set; }
        public string DollarCurrencySymbol { get; set; }
        public string PoundCurrencySymbol { get; set; }
        public int CountryId { get; set; }
        public string DollarCurrencyCode { get; set; }
        public string PoundCurrencyCode { get; set; }
        public string NairaCurrencyCode { get; set; }
        public string NairaCurrencySymbol { get; set; }
        public double NairaAmount { get; set; }
        public string CediCurrencyCode { get; set; }
        public string CediCurrencySymbol { get; set; }
        public double CediAmount { get; set; }
        public DateTime DateCreated { get; set; }
    }

    public class CurrencyEquivalentDTO
    {
        public decimal Amount { get; set; }
        public int CountryId { get; set; }
    }


}
