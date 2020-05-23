using Microsoft.Extensions.Configuration;

namespace PriceRectifier.Options
{
    internal class DataConsistencyOptions : IDataConsistencyOptions
    {
        public bool CheckPositivePrice { get; }
        public bool CheckOhlcvPositiveVolume { get; }
        public bool CheckOhlcvPositiveOrZeroVolume { get; }
        public bool CheckOhlcvPriceConsistency { get; }

        public DataConsistencyOptions(IConfiguration configuration)
        {
            var section = configuration.GetSection("DataConsistency");

            CheckPositivePrice = section.GetValue<bool>("CheckPositivePrice");
            CheckOhlcvPositiveVolume = section.GetValue<bool>("CheckOhlcvPositiveVolume");
            CheckOhlcvPositiveOrZeroVolume = section.GetValue<bool>("CheckOhlcvPositiveOrZeroVolume");
            CheckOhlcvPriceConsistency = section.GetValue<bool>("CheckOhlcvPriceConsistency");
        }
    }
}
