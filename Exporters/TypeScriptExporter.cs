// Uncomment to use ISIN in file- and variable names.
// #define ISIN_IN_NAMES

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using PriceRectifier.Alignment;
using PriceRectifier.Options;
using static System.FormattableString;

namespace PriceRectifier.Exporters
{
    internal static class TypeScriptExporter
    {
        public static async Task ExportToTypeScript(this ToplineInstrument instrument, IOutputOptions outputOptions,
            IAlignedTimeRange alignedTimeRange, CancellationToken cancellationToken)
        {
            DateTime timeFromInclusive = alignedTimeRange.StartDateTime ??
                (instrument.IsOhlcv ? instrument.OhlcvData[0].Time : instrument.ScalarData[0].Time);

            DateTime timeTillInclusive = alignedTimeRange.EndDateTime ??
                (instrument.IsOhlcv ? instrument.OhlcvData.Last().Time : instrument.ScalarData.Last().Time);

            var list = new List<string>();
            instrument.ComposeHeader(outputOptions, list);
            instrument.ComposeData(outputOptions, list, timeFromInclusive, timeTillInclusive);

            string filePath = instrument.ComposeTypeScriptFilePath(outputOptions);
            await File.WriteAllLinesAsync(filePath, list, Encoding.UTF8, cancellationToken);
        }

        private static string ComposeTypeScriptFilePath(this ToplineInstrument instrument, IOutputOptions outputOptions)
        {
            const string dash = "-";

            var symbol = instrument.GetSymbol();
            var mic = instrument.GetMic();
#if ISIN_IN_NAMES
            var isin = instrument.GetIsin();
#endif

            var sb = new StringBuilder("dally");
            if (symbol != null)
            {
                sb.Append(dash);
                sb.Append(symbol.ToLowerInvariant());
            }
#if ISIN_IN_NAMES
            if (isin != null)
            {
                sb.Append(dash);
                sb.Append(isin.ToLowerInvariant());
            }
#endif
            if (mic != null)
            {
                sb.Append(dash);
                sb.Append(mic.ToLowerInvariant());
            }

            sb.Append(dash);
            if (outputOptions.ClosingPriceOnly)
            {
                sb.Append("scalar");
            }
            else
            {
                sb.Append(instrument.IsOhlcv ? "ohlcv" : "scalar");
            }
            sb.Append(".ts");

            return string.Concat(outputOptions.FolderWithTrailingSeparator, sb.ToString());
        }

        private static string ComposeTypeScriptVariableName(this ToplineInstrument instrument, IOutputOptions outputOptions)
        {
            var symbol = instrument.GetSymbol();
            var mic = instrument.GetMic();
#if ISIN_IN_NAMES
            var isin = instrument.GetIsin();
#endif

            var sb = new StringBuilder("dally");
            if (symbol != null)
            {
                symbol = symbol.ToLowerInvariant();
                char c = char.ToUpperInvariant(symbol[0]);
                symbol = string.Concat(c, symbol.Substring(1));
                sb.Append(symbol);
            }
#if ISIN_IN_NAMES
            if (isin != null)
            {
                isin = isin.ToLowerInvariant();
                char c = char.ToUpperInvariant(isin[0]);
                isin = string.Concat(c, isin.Substring(1));
                sb.Append(isin);
            }
#endif
            if (mic != null)
            {
                mic = mic.ToLowerInvariant();
                char c = char.ToUpperInvariant(mic[0]);
                mic = string.Concat(c, mic.Substring(1));
                sb.Append(mic);
            }

            if (outputOptions.ClosingPriceOnly)
            {
                sb.Append("Scalar");
            }
            else
            {
                sb.Append(instrument.IsOhlcv ? "Ohlcv" : "Scalar");
            }
            return sb.ToString();
        }

        private static void ComposeHeader(this ToplineInstrument instrument, IOutputOptions outputOptions, List<string> list)
        {
            list.Add("import { Ohlcv } from '../../../../shared/mbs/data/entities/ohlcv';");
            list.Add(string.Empty);
            list.Add("/**");
            string s = instrument.GetName();
            if (s != null)
            {
                list.Add($" * name: {s}");
                list.Add(" *");
            }
            s = instrument.GetSymbol()?.ToUpperInvariant();
            if (s != null)
            {
                list.Add($" * ticker: {s}");
                list.Add(" *");
            }
            s = instrument.GetIsin()?.ToUpperInvariant();
            if (s != null)
            {
                list.Add($" * ISIN: {s}");
                list.Add(" *");
            }
            s = instrument.GetMic()?.ToUpperInvariant();
            if (s != null)
            {
                list.Add($" * MIC: {s}");
                list.Add(" *");
            }
            s = instrument.GetTyp();
            if (s != null)
            {
                list.Add($" * type: {s}");
                list.Add(" *");
            }
            s = instrument.GetCurrency()?.ToUpperInvariant();
            if (s != null)
            {
                list.Add($" * currency: {s}");
                list.Add(" *");
            }
            s = instrument.GetCfi()?.ToUpperInvariant();
            if (s != null)
            {
                list.Add($" * CFI: {s}");
                list.Add(" *");
            }
            list.Add(" */");

            string variableName = instrument.ComposeTypeScriptVariableName(outputOptions);
            string type = outputOptions.ClosingPriceOnly || !instrument.IsOhlcv ? "Scalar" :  "Ohlcv";
            list.Add($"export const {variableName}: {type} = [");
        }

        private static void ComposeData(this ToplineInstrument instrument, IOutputOptions outputOptions, List<string> list,
            DateTime timeFromInclusive, DateTime timeTillInclusive)
        {
            if (instrument.IsOhlcv)
            {
                if (outputOptions.ClosingPriceOnly)
                {
                    foreach (var e in instrument.OhlcvData)
                    {
                        var t = e.Time;
                        if (t >= timeFromInclusive && t <= timeTillInclusive)
                        {
                            string comma = t == timeTillInclusive ? string.Empty : ",";
                            list.Add(Invariant(
                                $"  {{ time: new Date({t.Year}, {t.Month - 1}, {t.Day}), value: {e.Close} }}{comma}"));
                        }
                    }
                }
                else
                {
                    foreach (var e in instrument.OhlcvData)
                    {
                        var t = e.Time;
                        if (t >= timeFromInclusive && t <= timeTillInclusive)
                        {
                            string comma = t == timeTillInclusive ? string.Empty : ",";
                            list.Add(Invariant(
                                $"  {{ time: new Date({t.Year}, {t.Month - 1}, {t.Day}), open: {e.Open}, high: {e.High}, low: {e.Low}, close: {e.Close}, volume: {e.Volume} }}{comma}"));
                        }
                    }
                }
            }
            else
            {
                foreach (var e in instrument.ScalarData)
                {
                    var t = e.Time;
                    if (t >= timeFromInclusive && t <= timeTillInclusive)
                    {
                        string comma = t == timeTillInclusive ? string.Empty : ",";
                        list.Add(Invariant(
                            $"  {{ time: new Date({t.Year}, {t.Month - 1}, {t.Day}), value: {e.Value} }}{comma}"));
                    }
                }
            }
            list.Add("];");
            list.Add(string.Empty);
        }

        private static string GetSymbol(this ToplineInstrument instrument)
        {
            if (!string.IsNullOrWhiteSpace(instrument.Symbol))
            {
                return instrument.Symbol;
            }

            if (!string.IsNullOrWhiteSpace(instrument.Ticker))
            {
                return instrument.Ticker;
            }

            return null;
        }

        private static string GetIsin(this ToplineInstrument instrument)
        {
            if (!string.IsNullOrWhiteSpace(instrument.Isin))
            {
                return instrument.Isin.ToLowerInvariant();
            }

            if (!string.IsNullOrWhiteSpace(instrument.IsinReconciliation))
            {
                return instrument.IsinReconciliation.ToLowerInvariant();
            }

            return null;
        }

        private static string GetMic(this ToplineInstrument instrument)
        {
            if (string.IsNullOrWhiteSpace(instrument.Mic) || instrument.Mic == "-")
            {
                return null;
            }
            return instrument.Mic;
        }

        private static string GetName(this ToplineInstrument instrument)
        {
            if (!string.IsNullOrWhiteSpace(instrument.Name))
            {
                return instrument.Name;
            }

            if (!string.IsNullOrWhiteSpace(instrument.NameShort))
            {
                return instrument.NameShort;
            }

            return null;
        }

        private static string GetCfi(this ToplineInstrument instrument)
        {
            return !string.IsNullOrWhiteSpace(instrument.Cfi) ? instrument.Cfi : null;
        }

        private static string GetCurrency(this ToplineInstrument instrument)
        {
            return !string.IsNullOrWhiteSpace(instrument.Currency) ? instrument.Currency : null;
        }

        private static string GetTyp(this ToplineInstrument instrument)
        {
            return !string.IsNullOrWhiteSpace(instrument.Type) ? instrument.Type : null;
        }
    }
}
