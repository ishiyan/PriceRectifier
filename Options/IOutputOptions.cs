namespace PriceRectifier.Options
{
    internal interface IOutputOptions
    {
        string FolderWithTrailingSeparator { get; }
        bool ClosingPriceOnly { get; }
    }
}
