using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PriceRectifier.Alignment;
using PriceRectifier.Entities;
using PriceRectifier.Validators;
using PriceRectifier.Exporters;
using PriceRectifier.Options;

namespace PriceRectifier.Topline
{
    internal class ToplineRepository : IToplineRepository
    {
        public static List<ToplineInstrument> ToplineInstruments { get; } = new List<ToplineInstrument>();

        private readonly ILogger<ToplineRepository> logger;
        private readonly IToplineInputOptions toplineInputOptions;
        private readonly IDataConsistencyOptions dataConsistencyOptions;
        private readonly IOutputOptions outputOptions;
        private readonly IAlignedTimeRange alignedTimeRange;

        public ToplineRepository(ILogger<ToplineRepository> logger, IToplineInputOptions toplineInputOptions,
            IDataConsistencyOptions dataConsistencyOptions, IOutputOptions outputOptions, IAlignedTimeRange alignedTimeRange)
        {
            this.logger = logger;
            this.toplineInputOptions = toplineInputOptions;
            this.dataConsistencyOptions = dataConsistencyOptions;
            this.outputOptions = outputOptions;
            this.alignedTimeRange = alignedTimeRange;
        }

        public async Task ImportToplineInstruments(CancellationToken cancellationToken)
        {
            logger.LogInformation("Importing topline instruments ...");

            await ImportToplineInstruments(toplineInputOptions.ScalarInstrumentsCsvPath, toplineInputOptions.ScalarDataCsvPath,
                true, cancellationToken);
            await ImportToplineInstruments(toplineInputOptions.OhlcvInstrumentsCsvPath, toplineInputOptions.OhlcvDataCsvPath,
                false, cancellationToken);

            logger.LogInformation($"Imported {ToplineInstruments.Count} topline instruments.");

        }

        public async Task ValidateData(CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                logger.LogInformation($"Verifying data for {ToplineInstruments.Count} topline instruments ...");
                bool success = true;
                foreach (var instrument in ToplineInstruments)
                {
                    var status = instrument.IsOhlcv ?
                        VerifyInstrumentData(instrument.OhlcvData, instrument.Name) :
                        VerifyInstrumentData(instrument.ScalarData, instrument.Name);
                    if (success)
                    {
                        success = status;
                    }
                }

                string result = success ? "succeeded" : "failed";
                logger.LogInformation($"Verifying data for {ToplineInstruments.Count} topline instruments {result}.");

            }, cancellationToken);
        }

        public async Task ExportData(CancellationToken cancellationToken)
        {
            logger.LogInformation($"Exporting topline instruments ...");
            foreach (var instrument in ToplineInstruments)
            {
                if (alignedTimeRange.EndDateTime.HasValue && instrument.StartDateInclusive > alignedTimeRange.EndDateTime.Value ||
                    alignedTimeRange.StartDateTime.HasValue && instrument.EndDateInclusive < alignedTimeRange.StartDateTime.Value)
                {
                    logger.LogInformation($"Skipping instrument \"{instrument.Name}\"");
                }
                else
                {
                    logger.LogInformation($"Exporting instrument \"{instrument.Name}\"");
                    await instrument.ExportToTypeScript(outputOptions, alignedTimeRange, cancellationToken);
                }
            }

            logger.LogInformation($"Exported topline instruments.");
        }

        private async Task ImportToplineInstruments(string pathToIndexCsv, string pathToDataCsv, bool isScalar, CancellationToken cancellationToken)
        {
            var instrumentLines = await File.ReadAllLinesAsync(pathToIndexCsv, cancellationToken);
            var dataLines = await File.ReadAllLinesAsync(pathToDataCsv, cancellationToken);

            foreach (var line in instrumentLines)
            {
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                {
                    continue;
                }

                var instrument = new ToplineInstrument {IsOhlcv = !isScalar};

                var elements = line.Split(';');

                // ReSharper disable CommentTypo
                // #  0         1                  2                3           4           5         6              7          8
                // # "FONDSID";"FST_FONDSTYPE_NR";"VAL_VALUTACODE";"FONDSNAAM";"NAAM_KORT";"SYMBOOL";"SYMBOOL_LONG";"CFI_CODE";"ISIN";
                // #  9                   10       11            12      13    14          15    16                17             18             19     
                // # "RECONCILIATIEISIN";"TICKER";"SYMBOLINDEX";"CUISP";"RIC";"BLOOMBERG";"MIC";"EXCANGE_SEGMENT";"EOD_DATETIME";"EXCANGE_NAME";"EXCANGE_TIMEZONE"
                // ReSharper restore CommentTypo

                logger.LogInformation($"started parsing instrument {Strip(elements[3])}");
                instrument.Fondsid = int.Parse(Strip(elements[0]));
                instrument.Type = ConvertToplineInstrumentType(int.Parse(Strip(elements[1])));
                instrument.Currency = Strip(elements[2]);
                instrument.Name = Strip(elements[3]);
                instrument.NameShort = Strip(elements[4]);
                instrument.Symbol = Strip(elements[5]);
                instrument.SymbolLong = Strip(elements[6]);
                instrument.Cfi = Strip(elements[7]);
                instrument.Isin = Strip(elements[8]);
                instrument.IsinReconciliation = Strip(elements[9]);
                instrument.Ticker = Strip(elements[10]);
                instrument.SymbolIndex = Strip(elements[11]);
                instrument.Cuisp = Strip(elements[12]);
                instrument.Ric = Strip(elements[13]);
                instrument.Bloomberg = Strip(elements[14]);
                instrument.Mic = Strip(elements[15]);
                instrument.ExchangeSegment = Strip(elements[16]);
                instrument.ExchangeSegmentEodTime = ExtractTime(Strip(elements[17]));
                instrument.ExchangeName = Strip(elements[18]);
                instrument.ExchangeTimeZone = Strip(elements[19]);

                if (instrument.IsinReconciliation.Length > 0)
                {
                    logger.LogWarning($"instrument {instrument.NameShort} has ISIN {instrument.Isin} and ISIN rec {instrument.IsinReconciliation}");
                }

                // Assumption: all data lines are sorted per instrument ascending by time.
                var rows = dataLines.Where(r => r.StartsWith(elements[0]));
                if (isScalar)
                {
                    foreach (var row in rows)
                    {
                        var components = row.Split(';');
                        var scalar = new Scalar
                        {
                            Time = ConvertDate(Strip(components[1])), Value = ConvertDouble(Strip(components[2]))
                        };
                        instrument.ScalarData.Add(scalar);
                    }

                    instrument.StartDateInclusive = instrument.ScalarData.First().Time;
                    instrument.EndDateInclusive = instrument.ScalarData.Last().Time;

                    if (instrument.StartDateInclusive >= new DateTime(2016, 1, 1))
                    {
                        logger.LogWarning($"{instrument.Name}:  start date {instrument.StartDateInclusive} >= 2016-01-01");
                    }
                    if (instrument.EndDateInclusive <= new DateTime(2020, 4, 6))
                    {
                        logger.LogWarning($"{instrument.Name}:  end date {instrument.EndDateInclusive} <= 2020-04-06");
                    }
                }
                else
                {
                    foreach (var row in rows)
                    {
                        var components = row.Split(';');
                        var ohlcv = new Ohlcv
                        {
                            Time = ConvertDate(Strip(components[1])),
                            Open = ConvertDouble(Strip(components[2])),
                            High = ConvertDouble(Strip(components[3])),
                            Low = ConvertDouble(Strip(components[4])),
                            Close = ConvertDouble(Strip(components[5])),
                            Volume = ConvertDouble(Strip(components[6]))
                        };
                        instrument.OhlcvData.Add(ohlcv);
                    }

                    instrument.StartDateInclusive = instrument.OhlcvData.First().Time;
                    instrument.EndDateInclusive = instrument.OhlcvData.Last().Time;

                    if (instrument.StartDateInclusive >= new DateTime(2016, 1, 1))
                    {
                        logger.LogWarning($"{instrument.Name}:  start date {instrument.StartDateInclusive} >= 2016-01-01");
                    }
                    if (instrument.EndDateInclusive <= new DateTime(2020, 4, 6))
                    {
                        logger.LogWarning($"{instrument.Name}:  end date {instrument.EndDateInclusive} <= 2020-04-06");
                    }
                }

                ToplineInstruments.Add(instrument);
            }
        }

        private string Strip(string str)
        {
            return str.Trim('"').Trim(' ');
        }

        private string ExtractTime(string dateAndTime)
        {
            if (string.IsNullOrWhiteSpace(dateAndTime))
            {
                return string.Empty;
            }

            //                             111111111
            //                   0123456789012345678
            // Expected format: "2020-04-08 23:00:00".
            return dateAndTime.Substring(11);
        }

        private DateTime ConvertDate(string date)
        {
            // Assumption: date is formatted as "1991-12-31".

            DateTime.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime dateTime);
            return dateTime;
        }

        private double ConvertDouble(string value)
        {
            // Assumption: double is formatted as "100.00000".
            return double.Parse(value, CultureInfo.InvariantCulture);
        }

        private string ConvertToplineInstrumentType(int type)
        {
            return type switch
            {
                116 => "Share, Toronto",
                292 => "Bonds Luxembourg",
                215 => "Bond, OTC",
                750 => "Discounter",
                814 => "SRD re-investment dividend (cash)",
                293 => "BOTS",
                128 => "Share, Oslo",
                2910 => "Bond futures",
                2920 => "Currency futures",
                2930 => "Commodities futures",
                166 => "Turbo's and Speeders",
                // ReSharper disable once StringLiteralTypo
                117 => "Share, Fundsettle",
                1200 => "Basket",
                126 => "Share, Copenhagen",
                127 => "Share, Stockholm",
                171 => "Trackers",
                802 => "optional dividend (cash)",
                803 => "optional dividend (stock)",
                5100 => "SRD Class",
                9100 => "SRD Series",
                310 => "SRD Cash dividend",
                294 => "Index Linked Bonds",
                1060 => "Inflation Index",
                804 => "re-investment dividend (cash)",
                805 => "re-investment dividend (stock)",
                610 => "SRD stock dividend",
                100 => "Shares",
                114 => "Share, London",
                115 => "Share, Zurich",
                // ReSharper disable once StringLiteralTypo
                106 => "Asas",
                108 => "Share, New York",
                109 => "Share, Nasdaq",
                111 => "Share issue",
                113 => "Share, Amex",
                120 => "Euro zone shares",
                125 => "Foreign share on AEX",
                130 => "Depositary receipts",
                135 => "Foreign depositary receipt on AEX",
                140 => "Cum.Pref. depositary receipt",
                145 => "Conv.Cum.Pref. profit-sharing depositary receipt.",
                146 => "Conv.Cum.Pref. depositary receipt",
                150 => "Non-convertible depositary receipt",
                155 => "Depositary receipt for founder's certificate",
                160 => "Preference share",
                165 => "Preference profit-sharing share",
                170 => "Preference depositary receipt",
                180 => "Conv.Pref. shares",
                185 => "Conv.Cum.Pref. share",
                190 => "Cum.Pref. share",
                191 => "Profit-sharing certificates",
                192 => "Debt replacement certificate",
                195 => "Founder's certificate",
                197 => "Investment fund",
                198 => "Foreign investment fund",
                199 => "Fund (not listed)",
                200 => "Nominal bonds",
                210 => "State bond",
                211 => "Bond issue",
                240 => "Floating Rate Notes",
                250 => "Capital bond",
                260 => "Bank, savings or mortgage bond",
                270 => "Convertible Bond",
                275 => "Reversed Conv. Bond",
                285 => "Foreign bond on AEX",
                290 => "Strips -Interest-",
                291 => "Strip -Nominal-",
                300 => "Dividend",
                400 => "Coupons",
                500 => "Subscription rights",
                590 => "Scrip",
                600 => "Stock dividends",
                660 => "Subscription right",
                700 => "Warrants",
                704 => "Put Warrants",
                705 => "Call Warrants",
                710 => "Falcons",
                720 => "Fascons",
                1000 => "Index",
                1050 => "Currency",
                2100 => "Share options",
                2150 => "Special Products",
                2200 => "Bond options",
                9000 => "Futures",
                3000 => "Options on Index",
                3050 => "Options on currency ",
                2900 => "Index Futures",
                261 => "mortgage bonds",
                262 => "savings certificates",
                801 => "optional dividend - oud",
                4050 => "Option series",
                706 => "Constant Leverage Products",
                707 => "Mini Future",
                709 => "Capped Bonus Certificate",
                711 => "Uncapped Bonus Certificate",
                717 => "Double Knock-out Warrant",
                712 => "Uncapped Capital Protection Certificate",
                713 => "Capped Capital Protection Certificate",
                714 => "Uncapped Out-performance Certificate",
                715 => "Capped Out-performance Certificate",
                716 => "Express Certificate",
                708 => "Tracker Certificate",
                _ => "???"
            };
        }

        private bool VerifyInstrumentData(IEnumerable<Scalar> list, string name)
        {
            list = list.ToList();
            bool success = true;

            if (dataConsistencyOptions.CheckPositivePrice)
            {
                success = list.ValidatePositivePrice(name, logger);
            }

            return success;
        }

        private bool VerifyInstrumentData(IEnumerable<Ohlcv> list, string name)
        {
            list = list.ToList();
            bool success = true;

            if (dataConsistencyOptions.CheckPositivePrice)
            {
                success = list.ValidatePositivePrice(name, logger);
            }

            if (dataConsistencyOptions.CheckOhlcvPositiveVolume)
            {
                bool status = list.ValidatePositiveVolume(name, logger);
                if (success)
                {
                    success = status;
                }
            }

            if (dataConsistencyOptions.CheckOhlcvPositiveOrZeroVolume)
            {
                bool status = list.ValidatePositiveOrZeroVolume(name, logger);
                if (success)
                {
                    success = status;
                }
            }

            if (dataConsistencyOptions.CheckOhlcvPriceConsistency)
            {
                bool status = list.ValidateOhlcConsistency(name, logger);
                if (success)
                {
                    success = status;
                }
            }

            return success;
        }
    }
}
