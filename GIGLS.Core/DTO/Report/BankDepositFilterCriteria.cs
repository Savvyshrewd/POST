﻿using System;

namespace POST.Core.DTO.Report
{
    public class BankDepositFilterCriteria
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int ServiceCentreId { get; set; }
        public int CountryId { get; set; }
        public int StationId { get; set; }

        /// <summary>
        /// Get the Start Date and End Date for query to the database
        /// </summary>
        /// <param name="collectionFilterCriteria"></param>
        /// <returns></returns>
        public Tuple<DateTime, DateTime> getStartDateAndEndDate()
        {
            BankDepositFilterCriteria collectionFilterCriteria = this;

            var startDate = DateTime.Now;
            var endDate = DateTime.Now;

            //If No Date Supplied, select the last 7 days
            if (!collectionFilterCriteria.StartDate.HasValue && !collectionFilterCriteria.EndDate.HasValue)
            {
                startDate = DateTime.Today.AddDays(-6);
                endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            }

            //StartDate has value and EndDate has Value 
            if (collectionFilterCriteria.StartDate.HasValue && collectionFilterCriteria.EndDate.HasValue)
            {
                var tempStartDate = ((DateTime)collectionFilterCriteria.StartDate);
                startDate = new DateTime(tempStartDate.Year, tempStartDate.Month, tempStartDate.Day);

                var tempEndDate = ((DateTime)collectionFilterCriteria.EndDate);
                endDate = new DateTime(tempEndDate.Year, tempEndDate.Month, tempEndDate.Day);
            }

            //StartDate has value and EndDate has no Value
            if (collectionFilterCriteria.StartDate.HasValue && !collectionFilterCriteria.EndDate.HasValue)
            {
                var tempStartDate = ((DateTime)collectionFilterCriteria.StartDate);
                startDate = new DateTime(tempStartDate.Year, tempStartDate.Month, tempStartDate.Day);

                endDate = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day);
            }

            //StartDate has no value and EndDate has Value
            if (collectionFilterCriteria.EndDate.HasValue && !collectionFilterCriteria.StartDate.HasValue)
            {
                startDate = new DateTime(2000, DateTime.Now.Month, DateTime.Now.Day);

                var tempEndDate = ((DateTime)collectionFilterCriteria.EndDate);
                endDate = new DateTime(tempEndDate.Year, tempEndDate.Month, tempEndDate.Day);
            }

            return new Tuple<DateTime, DateTime>(startDate, endDate.AddDays(1));
        }
    }
}
