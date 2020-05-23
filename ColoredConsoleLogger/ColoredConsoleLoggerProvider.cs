using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace PriceRectifier.ColoredConsoleLogger
{
    public class ColoredConsoleLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, ColoredConsoleLogger> loggers = new ConcurrentDictionary<string, ColoredConsoleLogger>();

        public ILogger CreateLogger(string categoryName)
        {
            return loggers.GetOrAdd(categoryName, name => new ColoredConsoleLogger(name));
        }

        public void Dispose()
        {
            loggers.Clear();
        }
    }
}
