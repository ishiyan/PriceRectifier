using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace PriceRectifier.Options
{
    internal class ToplineInputOptions : IToplineInputOptions
    {
        public string ScalarInstrumentsCsvPath { get; }
        public string OhlcvInstrumentsCsvPath { get; }
        public string ScalarDataCsvPath { get; }
        public string OhlcvDataCsvPath { get; }

        public ToplineInputOptions(IConfiguration configuration, ILogger<ToplineInputOptions> logger)
        {
            var section = configuration.GetSection("Topline");

            string folderWithTrailingSeparator = section.GetValue<string>("InputFolder");
            ScalarInstrumentsCsvPath = section.GetValue<string>("ScalarInstrumentsCsv");
            OhlcvInstrumentsCsvPath = section.GetValue<string>("OhlcvInstrumentsCsv");
            ScalarDataCsvPath = section.GetValue<string>("ScalarDataCsv");
            OhlcvDataCsvPath = section.GetValue<string>("OhlcvDataCsv");

            if (!Path.EndsInDirectorySeparator(folderWithTrailingSeparator))
            {
                folderWithTrailingSeparator = string.Concat(folderWithTrailingSeparator, Path.DirectorySeparatorChar);
            }

            ScalarInstrumentsCsvPath = string.Concat(folderWithTrailingSeparator, ScalarInstrumentsCsvPath);
            OhlcvInstrumentsCsvPath = string.Concat(folderWithTrailingSeparator, OhlcvInstrumentsCsvPath);
            ScalarDataCsvPath = string.Concat(folderWithTrailingSeparator, ScalarDataCsvPath);
            OhlcvDataCsvPath = string.Concat(folderWithTrailingSeparator, OhlcvDataCsvPath);

            bool success = true;
            if (!File.Exists(ScalarInstrumentsCsvPath))
            {
                success = false;
                logger.LogCritical($"Topline input file \"{ScalarInstrumentsCsvPath}\" does not exist.");
            }

            if (!File.Exists(OhlcvInstrumentsCsvPath))
            {
                success = false;
                logger.LogCritical($"Topline input file \"{OhlcvInstrumentsCsvPath}\" does not exist.");
            }

            if (!File.Exists(ScalarDataCsvPath))
            {
                success = false;
                logger.LogCritical($"Topline input file \"{ScalarDataCsvPath}\" does not exist.");
            }

            if (!File.Exists(OhlcvDataCsvPath))
            {
                success = false;
                logger.LogCritical($"Topline input file \"{OhlcvDataCsvPath}\" does not exist.");
            }

            if (!success)
            {
                throw new IOException("One or more topline input files do not exist.");
            }
        }
    }
}
