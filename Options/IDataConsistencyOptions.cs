namespace PriceRectifier.Options
{
    internal interface IDataConsistencyOptions
    {
        bool CheckPositivePrice { get; }
        bool CheckOhlcvPositiveVolume { get; }
        bool CheckOhlcvPositiveOrZeroVolume { get; }
        bool CheckOhlcvPriceConsistency { get; }
    }
}
