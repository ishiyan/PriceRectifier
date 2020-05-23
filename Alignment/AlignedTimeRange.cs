using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using PriceRectifier.Entities;
using PriceRectifier.Options;
using PriceRectifier.Topline;

namespace PriceRectifier.Alignment
{
    internal class AlignedTimeRange : IAlignedTimeRange
    {
        private readonly ILogger<AlignedTimeRange> logger;
        private readonly IDataAlignmentOptions dataAlignmentOptions;

        public DateTime? StartDateTime { get; private set; }
        public DateTime? EndDateTime { get; private set; }

        public AlignedTimeRange(ILogger<AlignedTimeRange> logger, IDataAlignmentOptions dataAlignmentOptions)
        {
            this.logger = logger;
            this.dataAlignmentOptions = dataAlignmentOptions;
        }

        public async Task Align(CancellationToken cancellationToken)
        {
            await Task.Run(() =>
            {
                logger.LogInformation("Aligning time range ...");

                if (dataAlignmentOptions.StartDateInclusive.HasValue)
                {
                    StartDateTime = dataAlignmentOptions.StartDateInclusive.Value;
                }
                else if (dataAlignmentOptions.SelectLatestStartDateInTheSet)
                {
                    /*var max = DateTime.MinValue;
                    foreach (var i in ToplineRepository.ToplineInstruments)
                    {
                        if (max < i.StartDateInclusive)
                        {
                            max = i.StartDateInclusive;
                        }
                    }*/
                    StartDateTime = ToplineRepository.ToplineInstruments.Max(i => i.StartDateInclusive);
                    /*if (max != StartDateTime)
                    {
                        logger.LogInformation($"Error calculating max start: {max} versus {StartDateTime}");
                    }*/
                }

                if (dataAlignmentOptions.EndDateInclusive.HasValue)
                {
                    EndDateTime = dataAlignmentOptions.EndDateInclusive.Value;
                }
                else if (dataAlignmentOptions.SelectEarliestEndDateInTheSet)
                {
                    /*var min = DateTime.MaxValue;
                    foreach (var i in ToplineRepository.ToplineInstruments)
                    {
                        if (min > i.EndDateInclusive)
                        {
                            min = i.EndDateInclusive;
                        }
                    }*/
                    EndDateTime = ToplineRepository.ToplineInstruments.Min(i => i.EndDateInclusive);
                    /*if (min != EndDateTime)
                    {
                        logger.LogInformation($"Error calculating min end: {min} versus {EndDateTime}");
                    }*/
                }

                if (StartDateTime.HasValue && EndDateTime.HasValue && StartDateTime.Value > EndDateTime.Value)
                {
                    logger.LogWarning($"StartDateTime {StartDateTime} is greater than EndDateTime {EndDateTime}, setting StartDateTime to be equal EndDateTime");
                    StartDateTime = EndDateTime.Value;
                }
                logger.LogInformation($"Aligned time range (inclusive): from {StartDateTime} till {EndDateTime}");

                if (dataAlignmentOptions.FillMissingDatesInTheSet)
                {
                    logger.LogInformation("Combining dates ...");
                    var timeList = new List<DateTime>();
                    foreach (var instrument in ToplineRepository.ToplineInstruments)
                    {
                        if (instrument.IsOhlcv)
                        {
                            timeList = timeList.Union(instrument.OhlcvData
                                .Where(e => e.Time >= StartDateTime && e.Time <= EndDateTime)
                                .Select(e => e.Time).ToList()).ToList();
                        }
                        else
                        {
                            timeList = timeList.Union(instrument.ScalarData
                                .Where(e => e.Time >= StartDateTime && e.Time <= EndDateTime)
                                .Select(e => e.Time).ToList()).ToList();
                        }
                    }
                    logger.LogInformation("Combining dates finished");

                    logger.LogInformation("Filling missing dates ...");
                    foreach (var instrument in ToplineRepository.ToplineInstruments)
                    {
                        if (instrument.IsOhlcv)
                        {
                            var data = instrument.OhlcvData;
                            foreach (var t in timeList)
                            {
                                while (true)
                                {
                                    int index = data.FindIndex(e => e.Time >= t);
                                    if (index >= 0 && data[index].Time > t)
                                    {
                                        if (index == 0)
                                        {
                                            logger.LogWarning($"instrument \"{instrument.Name}\": cloning ohlcv index {index} with time {data[index].Time} to index {index} time {t}");
                                            data.Insert(index, Clone(data[index], t));
                                        }
                                        else
                                        {
                                            logger.LogWarning($"instrument \"{instrument.Name}\": cloning ohlcv index {index - 1} with time {data[index - 1].Time} to index {index} time {t}");
                                            data.Insert(index, Clone(data[index - 1], t));
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            var data = instrument.ScalarData;
                            foreach (var t in timeList)
                            {
                                while (true)
                                {
                                    int index = data.FindIndex(e => e.Time >= t);
                                    if (index >= 0 && data[index].Time > t)
                                    {
                                        if (index == 0)
                                        {
                                            logger.LogWarning($"instrument \"{instrument.Name}\": cloning ohlcv index {index} with time {data[index].Time} to index {index} time {t}");
                                            data.Insert(index, Clone(data[index], t));
                                        }
                                        else
                                        {
                                            logger.LogWarning($"instrument \"{instrument.Name}\": cloning ohlcv index {index - 1} with time {data[index - 1].Time} to index {index} time {t}");
                                            data.Insert(index, Clone(data[index - 1], t));
                                        }
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                            }
                        }
                    }
                    logger.LogInformation("Filling missing dates finished");
                }

                logger.LogInformation($"Aligned time range for {ToplineRepository.ToplineInstruments.Count} instruments.");
            }, cancellationToken);
        }

        private Ohlcv Clone(Ohlcv ohlcv, DateTime dateTime)
        {
            return new Ohlcv
            {
                Time = dateTime,
                Open = ohlcv.Open,
                High = ohlcv.High,
                Low = ohlcv.Low,
                Close = ohlcv.Close,
                Volume = ohlcv.Volume
            };
        }

        private Scalar Clone(Scalar scalar, DateTime dateTime)
        {
            return new Scalar
            {
                Time = dateTime,
                Value = scalar.Value
            };
        }
    }
}
