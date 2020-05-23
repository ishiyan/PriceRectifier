using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Microsoft.Extensions.Configuration;

namespace PriceRectifier.Options
{
    internal class OutputOptions : IOutputOptions
    {
        public string FolderWithTrailingSeparator { get; }
        public bool ClosingPriceOnly { get; }

        public OutputOptions(IConfiguration configuration)
        {
            var section = configuration.GetSection("Output");

            FolderWithTrailingSeparator = section.GetValue<string>("Folder");
            ClosingPriceOnly = section.GetValue<bool>("ClosingPriceOnly");

            if (!Path.EndsInDirectorySeparator(FolderWithTrailingSeparator))
            {
                FolderWithTrailingSeparator = string.Concat(FolderWithTrailingSeparator, Path.DirectorySeparatorChar);
            }
        }
    }
}
