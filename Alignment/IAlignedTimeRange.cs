using System;
using System.Threading;
using System.Threading.Tasks;

namespace PriceRectifier.Alignment
{
    internal interface IAlignedTimeRange
    {
        DateTime? StartDateTime { get; }
        DateTime? EndDateTime { get; }

        Task Align(CancellationToken cancellationToken);
    }
}
