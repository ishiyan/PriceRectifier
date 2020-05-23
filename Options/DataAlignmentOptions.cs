using System;
using System.Globalization;
using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace PriceRectifier.Options
{
    internal class DataAlignmentOptions : IDataAlignmentOptions
    {
        public bool FillMissingDatesInTheSet { get; }
        public bool SelectLatestStartDateInTheSet { get; }
        public bool SelectEarliestEndDateInTheSet { get; }
        public DateTime? StartDateInclusive { get; }
        public DateTime? EndDateInclusive { get; }

        public DataAlignmentOptions(IConfiguration configuration, ILogger<DataAlignmentOptions> logger)
        {
            var section = configuration.GetSection("DataAlignment");

            FillMissingDatesInTheSet = section.GetValue<bool>("FillMissingDatesInTheSet");
            SelectLatestStartDateInTheSet = section.GetValue<bool>("SelectLatestStartDateInTheSet");
            SelectEarliestEndDateInTheSet = section.GetValue<bool>("SelectEarliestEndDateInTheSet");
            string startDateInclusive = section.GetValue<string>("StartDateInclusive");
            string endDateInclusive = section.GetValue<string>("EndDateInclusive");

            bool success = true;
            if (!string.IsNullOrEmpty(startDateInclusive))
            {
                var converted = ConvertToDateTime(startDateInclusive);
                if (converted.HasValue)
                {
                    StartDateInclusive = converted;
                }
                else
                {
                    success = false;
                    logger.LogCritical($"Data alignment StartDateInclusive \"{startDateInclusive}\" has invalid date-time format.");
                }
            }
            if (!string.IsNullOrEmpty(endDateInclusive))
            {
                var converted = ConvertToDateTime(endDateInclusive);
                if (converted.HasValue)
                {
                    EndDateInclusive = converted;
                }
                else
                {
                    success = false;
                    logger.LogCritical($"Data alignment EndDateInclusive \"{endDateInclusive}\" has invalid date-time format.");
                }
            }
            if (!success)
            {
                throw new IOException("One or more data alignment dates have invalid date-time format.");
            }
        }

        private static DateTime? ConvertToDateTime(string date)
        {
            return DateTime.TryParseExact(date,
                new[] {"yyyy-MM-dd", "yyyy-MM-dd hh:mm:ss", "yyyy-MM-ddThh:mm:ss" },
                CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime) ? dateTime : (DateTime?)null;
        }
    }
}
