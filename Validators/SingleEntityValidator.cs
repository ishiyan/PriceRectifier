using System.Collections.Generic;
using System.Globalization;
using Microsoft.Extensions.Logging;
using PriceRectifier.Entities;

namespace PriceRectifier.Validators
{
    internal static class SingleEntityValidator
    {
        public static bool ValidatePositivePrice(this IEnumerable<Scalar> list, string name, ILogger logger)
        {
            bool success = true;
            foreach (var element in list)
            {
                if (element.Value <= 0)
                {
                    success = false;
                    logger.LogWarning($"instrument \"{name}\": value {element.Value} is not positive at date {ToShortDate(element)}");
                }
            }

            return success;
        }

        public static bool ValidatePositivePrice(this IEnumerable<Ohlcv> list, string name, ILogger logger)
        {
            bool success = true;
            foreach (var element in list)
            {
                if (element.Open <= 0)
                {
                    success = false;
                    logger.LogWarning($"instrument \"{name}\": opening price {element.Open} is not positive at date {ToShortDate(element)}");
                }
                if (element.High <= 0)
                {
                    success = false;
                    logger.LogWarning($"instrument \"{name}\": highest price {element.High} is not positive at date {ToShortDate(element)}");
                }
                if (element.Low <= 0)
                {
                    success = false;
                    logger.LogWarning($"instrument \"{name}\": lowest price {element.Low} is not positive at date {ToShortDate(element)}");
                }
                if (element.Close <= 0)
                {
                    success = false;
                    logger.LogWarning($"instrument \"{name}\": closing price {element.Close} is not positive at date {ToShortDate(element)}");
                }
                if (element.Volume < 0)
                {
                    success = false;
                    logger.LogWarning($"instrument \"{name}\": volume {element.Volume} is negative at date {ToShortDate(element)}");
                }
            }

            return success;
        }

        public static bool ValidatePositiveVolume(this IEnumerable<Ohlcv> list, string name, ILogger logger)
        {
            bool success = true;
            foreach (var element in list)
            {
                if (element.Volume <= 0)
                {
                    success = false;
                    logger.LogWarning($"instrument \"{name}\": volume {element.Volume} is not positive at date {ToShortDate(element)}");
                }
            }

            return success;
        }

        public static bool ValidatePositiveOrZeroVolume(this IEnumerable<Ohlcv> list, string name, ILogger logger)
        {
            bool success = true;
            foreach (var element in list)
            {
                if (element.Volume < 0)
                {
                    success = false;
                    logger.LogWarning($"instrument \"{name}\": volume {element.Volume} is negative at date {ToShortDate(element)}");
                }
            }

            return success;
        }

        public static bool ValidateOhlcConsistency(this IEnumerable<Ohlcv> list, string name, ILogger logger)
        {
            bool success = true;
            foreach (var element in list)
            {
                var o = element.Open;
                var h = element.High;
                var l = element.Low;
                var c = element.Close;

                if (h < o)
                {
                    success = false;
                    logger.LogWarning($"instrument \"{name}\": highest price {h} is less than opening price {o} at date {ToShortDate(element)}");
                }
                if (h < l)
                {
                    success = false;
                    logger.LogWarning($"instrument \"{name}\": highest price {h} is less than lowest price {l} at date {ToShortDate(element)}");
                }
                if (h < c)
                {
                    success = false;
                    logger.LogWarning($"instrument \"{name}\": highest price {h} is less than closing price {c} at date {ToShortDate(element)}");
                }
                if (l > o)
                {
                    success = false;
                    logger.LogWarning($"instrument \"{name}\": lowest price {h} is greater than opening price {o} at date {ToShortDate(element)}");
                }
                if (l > c)
                {
                    success = false;
                    logger.LogWarning($"instrument \"{name}\": lowest price {h} is greater than closing price {c} at date {ToShortDate(element)}");
                }
            }

            return success;
        }

        private static string ToShortDate(Scalar element)
        {
            return element.Time.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }

        private static string ToShortDate(Ohlcv element)
        {
            return element.Time.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        }
    }
}
