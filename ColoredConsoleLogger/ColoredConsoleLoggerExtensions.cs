using System;
using Microsoft.Extensions.Logging;

namespace PriceRectifier.ColoredConsoleLogger
{
    public static class ColoredConsoleLoggerExtensions
    {
        public static ILoggingBuilder AddColoredConsoleLogger(this ILoggingBuilder loggingBuilder)
        {
            //var config = new ColoredConsoleLoggerConfiguration();
            return loggingBuilder.AddProvider(new ColoredConsoleLoggerProvider(/*config*/));
        }

        /*public static ILoggingBuilder AddColoredConsoleLogger(this ILoggingBuilder loggingBuilder, ColoredConsoleLoggerConfiguration config)
        {
            return loggingBuilder.AddProvider(new ColoredConsoleLoggerProvider(config));
        }

        public static ILoggingBuilder AddColoredConsoleLogger(this ILoggingBuilder loggingBuilder, Action<ColoredConsoleLoggerConfiguration> configure)
        {
            var config = new ColoredConsoleLoggerConfiguration();
            configure(config);
            return loggingBuilder.AddProvider(new ColoredConsoleLoggerProvider(config));
        }*/

        /*
        public static ILoggerFactory AddColoredConsoleLogger(this ILoggerFactory loggerFactory, ColoredConsoleLoggerConfiguration config)
        {
            loggerFactory.AddProvider(new ColoredConsoleLoggerProvider(config));
            return loggerFactory;
        }

        public static ILoggerFactory AddColoredConsoleLogger(this ILoggerFactory loggerFactory)
        {
            var config = new ColoredConsoleLoggerConfiguration();
            return loggerFactory.AddColoredConsoleLogger(config);
        }

        public static ILoggerFactory AddColoredConsoleLogger(this ILoggerFactory loggerFactory, Action<ColoredConsoleLoggerConfiguration> configure)
        {
            var config = new ColoredConsoleLoggerConfiguration();
            configure(config);
            return loggerFactory.AddColoredConsoleLogger(config);
        }*/
    }
}
