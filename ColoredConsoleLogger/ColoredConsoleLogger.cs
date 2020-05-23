using System;
using Microsoft.Extensions.Logging;

namespace PriceRectifier.ColoredConsoleLogger
{
    public class ColoredConsoleLogger : ILogger
    {
        private readonly string name;

        public ColoredConsoleLogger(string name)
        {
            this.name = name;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return true; //logLevel == config.LogLevel;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            var color = Console.ForegroundColor;
            Console.ForegroundColor = LogLevelToColor(logLevel);
            Console.WriteLine($"{LogLevelToText(logLevel)} - {name}[{eventId.Id}] - {formatter(state, exception)}");
            if (exception != null)
            {
                Console.WriteLine($"{exception.ToString()}");
            }
            Console.ForegroundColor = color;
        }

        private ConsoleColor LogLevelToColor(LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.Critical => ConsoleColor.Red,
                LogLevel.Error => ConsoleColor.DarkRed,
                LogLevel.Warning => ConsoleColor.DarkYellow,
                LogLevel.Information => ConsoleColor.DarkGreen,
                LogLevel.Debug => ConsoleColor.DarkGray,
                LogLevel.Trace => ConsoleColor.DarkBlue,
                _ => ConsoleColor.DarkGray
            };
        }

        private string LogLevelToText(LogLevel logLevel)
        {
            return logLevel switch
            {
                LogLevel.Critical => "crt",
                LogLevel.Error => "err",
                LogLevel.Warning => "wrn",
                LogLevel.Information => "inf",
                LogLevel.Debug => "dbg",
                LogLevel.Trace => "trc",
                _ => "???"
            };
        }
    }
}
