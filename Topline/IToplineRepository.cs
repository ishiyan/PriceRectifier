using System.Threading;
using System.Threading.Tasks;

namespace PriceRectifier.Topline
{
    internal interface IToplineRepository
    {
        Task ImportToplineInstruments(CancellationToken cancellationToken);
        Task ValidateData(CancellationToken cancellationToken);
        Task ExportData(CancellationToken cancellationToken);
    }
}
