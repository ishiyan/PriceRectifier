using System;
using System.Collections.Generic;
using PriceRectifier.Entities;

namespace PriceRectifier
{
    internal class ToplineInstrument
    {
        public int Fondsid { get; set; }
        public string Type { get; set; }
        public string Currency { get; set; }
        public string Symbol { get; set; }
        public string SymbolLong { get; set; }
        public string Name { get; set; }
        public string NameShort { get; set; }
        public string Cfi { get; set; }
        /// <summary>
        /// International Securities Identification Number (ISO 6166).
        /// </summary>
        public string Isin { get; set; }
        /// <summary>
        /// Real Isin. Added in case the ISIN code contains the "trading ISIN".
        /// </summary>
        public string IsinReconciliation { get; set; }
        public string Ticker { get; set; }
        /// <summary>
        /// Identifier used by EuroNext from Optiq3 onward.
        /// </summary>
        public string SymbolIndex { get; set; }
        /// <summary>
        /// CUSIP (Committee on Uniform Securities Identification Procedures) Number.
        /// </summary>
        public string Cuisp { get; set; }
        /// <summary>
        /// Reuters Instrument Code.
        /// </summary>
        public string Ric { get; set; }
        /// <summary>
        /// Bloomberg Ticker.
        /// </summary>
        public string Bloomberg { get; set; }
        public string Mic { get; set; }
        public string ExchangeSegment { get; set; }
        public string ExchangeSegmentEodTime { get; set; }
        public string ExchangeName { get; set; }
        public string ExchangeTimeZone { get; set; }

        public bool IsOhlcv { get; set; }
        public DateTime StartDateInclusive { get; set; }
        public DateTime EndDateInclusive { get; set; }
        public List<Ohlcv> OhlcvData { get; } = new List<Ohlcv>();
        public List<Scalar> ScalarData { get; } = new List<Scalar>();
    }
}