using System;

namespace PriceRectifier.Options
{
    internal interface IDataAlignmentOptions
    {
        bool FillMissingDatesInTheSet { get; }
        bool SelectLatestStartDateInTheSet { get; }
        bool SelectEarliestEndDateInTheSet { get; }
        DateTime? StartDateInclusive { get; }
        DateTime? EndDateInclusive { get; }
    }
}
