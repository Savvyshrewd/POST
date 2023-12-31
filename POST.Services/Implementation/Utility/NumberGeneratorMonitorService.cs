﻿using POST.Core;
using POST.Core.Domain;
using POST.Core.Enums;
using POST.Core.IServices.Utility;
using POST.Infrastructure;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace POST.Services.Implementation.Utility
{
    public class NumberGeneratorMonitorService : INumberGeneratorMonitorService
    {
        private readonly IUnitOfWork _uow;
        public NumberGeneratorMonitorService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<string> GenerateNextNumber(NumberGeneratorType numberGeneratorType, string serviceCenterCode)
        {
            try
            {
                string numberGenerated = null;
                //1. Get the last numberGenerated for the serviceCenter and numberGeneratorType
                //from the NumberGeneratorMonitor Table
                var monitor = _uow.NumberGeneratorMonitor.SingleOrDefault(x =>
                    x.ServiceCentreCode == serviceCenterCode && x.NumberGeneratorType == numberGeneratorType);

                if (numberGeneratorType == NumberGeneratorType.MagayaWb)
                {
                    monitor = _uow.NumberGeneratorMonitor.SingleOrDefault(x => x.NumberGeneratorType == numberGeneratorType);
                }


                // At this point, monitor can only be null if it's the first time we're
                // creating a number for the service centre and numberGeneratorType. 
                //If that's the case, we assume our numberCode to be "0".
                var numberCode = monitor?.Number ?? "0";

                //2. Increment lastcode to get the next available numberCode by 1
                var number = 0L;
                var numberStr = "";

                if(numberGeneratorType == NumberGeneratorType.MagayaWb)
                {
                    number = long.Parse(numberCode) + 1;
                    numberStr = number.ToString("00");
                }
                else if (numberGeneratorType == NumberGeneratorType.MagayaWbM)
                {
                    number = long.Parse(numberCode) + 1;
                    numberStr = number.ToString("00");
                }
                else if (numberGeneratorType == NumberGeneratorType.MovementManifestNumber)
                {
                    number = long.Parse(numberCode) + 1;
                    numberStr = number.ToString("00000");
                }
                else if (numberGeneratorType == NumberGeneratorType.RequestNumber)
                {
                    number = long.Parse(numberCode) + 1;
                    numberStr = number.ToString("0000");
                }
                else
                {
                    number = long.Parse(numberCode) + 1;
                    numberStr = number.ToString("000000");
                }

                //Add or update the NumberGeneratorMonitor Table for the Service Centre and numberGeneratorType
                if (monitor != null)
                {
                    await UpdateNumberGeneratorMonitor(serviceCenterCode, numberGeneratorType, numberStr);
                }
                else
                {
                    await AddNumberGeneratorMonitor(serviceCenterCode, numberGeneratorType, numberStr);
                }

                //pad the service centre
                var serviceCenter = await _uow.ServiceCentre.GetAsync(s => s.Code == serviceCenterCode);
                var serviceCentreId = (serviceCenter == null) ? 0 : serviceCenter.ServiceCentreId;
                var codeStr = serviceCentreId.ToString("000");

                //Add the numberCode with the serviceCenterCode and numberGeneratorType
                if (numberGeneratorType != NumberGeneratorType.RequestNumber)
                {
                    numberGenerated = ResolvePrefixFromNumberGeneratorType(numberGeneratorType) + codeStr + numberStr;
                }
                else
                {
                    numberGenerated = (ResolvePrefixFromNumberGeneratorType(numberGeneratorType)-9) + codeStr + numberStr;
                }

                if (numberGeneratorType == NumberGeneratorType.MagayaWb)
                {
                    numberGenerated = "AWR-"+ResolvePrefixFromNumberGeneratorType(numberGeneratorType)  + numberStr;
                }else if (numberGeneratorType == NumberGeneratorType.MagayaWbM)
                {
                    numberGenerated = "MWR-" + ResolvePrefixFromNumberGeneratorType(numberGeneratorType) + numberStr;
                }

                if (numberGeneratorType == NumberGeneratorType.RequestNumber)
                {
                    numberGenerated = "REQ-" + numberGenerated;
                }

                if (numberGeneratorType == NumberGeneratorType.CustomerCodeIndividual || numberGeneratorType == NumberGeneratorType.CustomerCodeCorporate ||
                    numberGeneratorType == NumberGeneratorType.CustomerCodeEcommerce   ||  numberGeneratorType == NumberGeneratorType.Wallet ||
                    numberGeneratorType == NumberGeneratorType.Partner || numberGeneratorType == NumberGeneratorType.Employee || 
                    numberGeneratorType == NumberGeneratorType.FleetPartner || numberGeneratorType == NumberGeneratorType.PreShipmentCode 
                   )
                {
                    numberGenerated = ResolvePrefixFromNumberGeneratorTypeForCustomers(numberGeneratorType) + numberStr;
                }

                return numberGenerated;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<string> GenerateNextNumber(NumberGeneratorType numberGeneratorType)
        {
            var defaultServiceCenterCode = "SYS";
            return await GenerateNextNumber(numberGeneratorType, defaultServiceCenterCode);
        }


        private async Task AddNumberGeneratorMonitor(string serviceCenterCode, NumberGeneratorType numberGeneratorType, string number)
        {
            try
            {
                _uow.NumberGeneratorMonitor.Add(new NumberGeneratorMonitor
                {
                    ServiceCentreCode = serviceCenterCode,
                    NumberGeneratorType = numberGeneratorType,
                    Number = number
                });
                await _uow.CompleteAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task UpdateNumberGeneratorMonitor(string serviceCenterCode, NumberGeneratorType numberGeneratorType, string number)
        {
            try
            {
                var monitor = _uow.NumberGeneratorMonitor.SingleOrDefault(x =>
                    x.ServiceCentreCode == serviceCenterCode && x.NumberGeneratorType == numberGeneratorType);

                if (monitor == null)
                {
                    throw new GenericException("Number Generator Monitor failed to update!", $"{(int)HttpStatusCode.NotFound}");
                }
                monitor.Number = number;
                await _uow.CompleteAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }


        private int ResolvePrefixFromNumberGeneratorType(NumberGeneratorType numberGeneratorType)
        {
            switch (numberGeneratorType)
            {
                case NumberGeneratorType.WaybillNumber:
                    {
                        return (int)NumberGeneratorType.WaybillNumber;
                    }
                case NumberGeneratorType.GroupWaybillNumber:
                    {
                        return (int)NumberGeneratorType.GroupWaybillNumber;
                    }
                case NumberGeneratorType.Manifest:
                    {
                        return (int)NumberGeneratorType.Manifest;
                    }
                case NumberGeneratorType.Invoice:
                    {
                        return (int)NumberGeneratorType.Invoice;
                    }
                case NumberGeneratorType.Wallet:
                    {
                        return (int)NumberGeneratorType.Wallet;
                    }
                case NumberGeneratorType.BankProcessingOrderForShipment:
                    {
                        return (int)NumberGeneratorType.BankProcessingOrderForShipment;
                    }
                case NumberGeneratorType.BankProcessingOrderForCOD:
                    {
                        return (int)NumberGeneratorType.BankProcessingOrderForCOD;
                    }
                case NumberGeneratorType.BankProcessingOrderForDemurrage:
                    {
                        return (int)NumberGeneratorType.BankProcessingOrderForDemurrage;
                    }
                case NumberGeneratorType.MagayaWb:
                    {
                        return (int)NumberGeneratorType.MagayaWb;
                    }
                case NumberGeneratorType.SuperManifest:
                    {
                        return (int)NumberGeneratorType.SuperManifest;
                    }
                case NumberGeneratorType.RequestNumber:
                    {
                        return (int)NumberGeneratorType.RequestNumber;
                    }
                case NumberGeneratorType.MovementManifestNumber:
                    {
                        return (int)NumberGeneratorType.MovementManifestNumber;
                    }
                default:
                    {
                        return (int)NumberGeneratorType.WaybillNumber;
                    }
            }
        }

        private string ResolvePrefixFromNumberGeneratorTypeForCustomers(NumberGeneratorType numberGeneratorType)
        {
            switch (numberGeneratorType)
            {
                case NumberGeneratorType.CustomerCodeIndividual:
                    {
                        return "IND";
                    }
                case NumberGeneratorType.CustomerCodeCorporate:
                    {
                        return "ACC";
                    }
                case NumberGeneratorType.CustomerCodeEcommerce:
                    {
                        return "ECO";
                    }
                case NumberGeneratorType.Wallet:
                    {
                        return (int)NumberGeneratorType.Wallet + "";
                    }
                case NumberGeneratorType.Partner:
                    {
                        return "P";
                    }
                case NumberGeneratorType.FleetPartner:
                    {
                        return "EP";
                    }
                case NumberGeneratorType.Employee:
                    {
                        return "EMP";
                    }
                case NumberGeneratorType.PreShipmentCode:
                    {
                        return "PRE";
                    }
                case NumberGeneratorType.RequestNumber:
                    {
                        return "REQ-";
                    }
                default:
                    {
                        return "IND";
                    }
            }
        }

        public async Task<string> GenerateInvoiceRefNoWithDate(NumberGeneratorType numberGeneratorType, string customerCode, DateTime startDate, DateTime endDate)
        {
            var startYear = startDate.Year.ToString().Substring(2);
            var endYear = endDate.Year.ToString().Substring(2);
            var datewithTime = DateTime.Now.ToUniversalTime().ToString("yyyyMMdd\\THHmmssfff");
            var refNo = String.Empty;
            int newRef = 0;
            var customerInvoice = _uow.CustomerInvoice.GetAllAsQueryable().Where(x => x.CustomerCode == customerCode && x.DateCreated >= startDate && x.DateCreated <= endDate).OrderByDescending(x => x.DateCreated).FirstOrDefault();
           // refNo = $"{customerCode.Remove(2)}{startDate.Day}{startYear}{endDate.Day}{endYear}{datewithTime}";
            if (customerInvoice == null)
            {
                refNo = $"{customerCode}{startDate.Month}{refNo.PadLeft(3, '0')}1";
            }
            else
            {
                var lastRef = $"{customerCode}{startDate.Month}";
                var refValue = customerInvoice.InvoiceRefNo.Substring(lastRef.Length);
                var isNumeric = int.TryParse(refValue, out int n);
                if (isNumeric)
                {
                    newRef = Convert.ToInt32(customerInvoice.InvoiceRefNo.Substring(lastRef.Length));
                    newRef = newRef + 1;
                }
                else
                {
                    newRef = Convert.ToInt32(refValue.Last());
                    newRef = newRef + 1;
                }
                refNo = $"{customerCode}{startDate.Month}{newRef.ToString().PadLeft(3, '0')}";
            }
            return refNo;
        }
    }
}
