using System;
using System.Collections.Generic;
using System.Globalization;

namespace ExpenseClaimConverter
{
    /// <summary>Provides deterministic validation, parsing, conversion, and total calculations.</summary>
    public static class ExpenseCalculator
    {
        /// <summary>Parses an invariant-culture decimal amount using only sign and decimal-point styles.</summary>
        public static bool TryParseAmount(string amountText, out decimal amount, out string errorMessage)
        {
            amount = 0m;
            errorMessage = string.Empty;
            const NumberStyles styles = NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign;
            if (!decimal.TryParse(amountText, styles, CultureInfo.InvariantCulture, out amount) || amount <= 0m)
            {
                errorMessage = "Invalid amount";
                return false;
            }
            return true;
        }

        /// <summary>Parses an expense date strictly as yyyy-MM-dd with invariant culture.</summary>
        public static bool TryParseExpenseDate(string dateText, out DateTime expenseDate, out string errorMessage)
        {
            if (!DateTime.TryParseExact(dateText, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out expenseDate))
            {
                errorMessage = "Invalid date: expected yyyy-MM-dd";
                return false;
            }
            errorMessage = string.Empty;
            return true;
        }

        /// <summary>Trims, uppercases, and validates a three-letter currency code.</summary>
        public static bool TryValidateCurrency(string currency, out string normalizedCurrency, out string errorMessage)
        {
            normalizedCurrency = (currency ?? string.Empty).Trim().ToUpperInvariant();
            if (normalizedCurrency.Length != 3 || !IsAsciiLetters(normalizedCurrency))
            {
                errorMessage = "Invalid currency: must be 3 letters";
                return false;
            }
            errorMessage = string.Empty;
            return true;
        }

        /// <summary>Returns rate 1 for a valid same-currency pair without performing any external lookup.</summary>
        public static bool TryGetSameCurrencyRate(string baseCurrency, string quoteCurrency, out decimal rate)
        {
            rate = 0m;
            if (!TryValidateCurrency(baseCurrency, out string normalizedBase, out _) ||
                !TryValidateCurrency(quoteCurrency, out string normalizedQuote, out _) ||
                !string.Equals(normalizedBase, normalizedQuote, StringComparison.Ordinal))
            {
                return false;
            }
            rate = 1m;
            return true;
        }

        /// <summary>Identifies a date later than today so callers can reject future expense lines.</summary>
        public static bool IsFutureDate(DateTime date)
        {
            return date.Date > DateTime.Today;
        }

        /// <summary>Rounds a monetary conversion to two decimals away from zero.</summary>
        public static decimal CalculateConvertedAmount(decimal amount, decimal rate)
        {
            return Math.Round(amount * rate, 2, MidpointRounding.AwayFromZero);
        }

        /// <summary>Sums rounded converted amounts for successful lines only.</summary>
        public static decimal CalculateTotal(IEnumerable<ExpenseLine> lines)
        {
            decimal total = 0m;
            foreach (ExpenseLine line in lines)
            {
                if (line != null && string.Equals(line.Status, "Converted", StringComparison.Ordinal) && line.ConvertedAmount.HasValue)
                {
                    total += line.ConvertedAmount.Value;
                }
            }
            return total;
        }

        private static bool IsAsciiLetters(string value)
        {
            foreach (char character in value)
            {
                if ((character < 'A' || character > 'Z') && (character < 'a' || character > 'z'))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
