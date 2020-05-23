namespace PriceRectifier.Options
{
    internal interface IToplineInputOptions
    {
        string ScalarInstrumentsCsvPath { get; }
        string OhlcvInstrumentsCsvPath { get; }
        string ScalarDataCsvPath { get; }
        string OhlcvDataCsvPath { get; }
    }
}
