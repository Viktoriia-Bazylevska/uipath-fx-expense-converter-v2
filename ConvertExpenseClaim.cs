using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using UiPath.CodedWorkflows;

namespace ExpenseClaimConverter
{
    /// <summary>Reads, validates, converts, and writes one expense claim.</summary>
    public class ConvertExpenseClaim : CodedWorkflow
    {
        /// <summary>Executes the expense conversion process using the supplied paths and target currency.</summary>
        [Workflow]
        public void Execute(string in_InputPath, string in_OutputPath, string in_TargetCurrency)
        {
            int convertedCount = 0;
            int failedCount = 0;
            int rateLookupCount = 0;
            try
            {
                Log("ConvertExpenseClaim started", LogLevel.Info);
                string inputPath = Path.GetFullPath(in_InputPath);
                string outputPath = Path.GetFullPath(in_OutputPath);
                if (!ExpenseCalculator.TryValidateCurrency(in_TargetCurrency, out string targetCurrency, out string targetError))
                {
                    throw new InvalidDataException("Invalid target currency: " + targetError);
                }

                IReadOnlyList<ExpenseLine> lines = ExpenseCsv.Read(inputPath);
                if (lines.Count == 0)
                {
                    Log("No expense lines", LogLevel.Warn);
                }

                Dictionary<string, RateLookup> rateCache = new(StringComparer.Ordinal);
                foreach (ExpenseLine line in lines)
                {
                    ProcessLine(line, targetCurrency, rateCache, ref rateLookupCount);
                    if (string.Equals(line.Status, "Converted", StringComparison.Ordinal))
                    {
                        convertedCount++;
                    }
                    else
                    {
                        failedCount++;
                    }
                }

                decimal total = ExpenseCalculator.CalculateTotal(lines);
                Log("Output file will be overwritten: " + Path.GetFileName(outputPath), LogLevel.Info);
                ExpenseCsv.Write(outputPath, lines, total, targetCurrency);
                Log(string.Format(CultureInfo.InvariantCulture, "ConvertExpenseClaim ended; LinesRead={0}; Converted={1}; Failed={2}; Total={3:0.00}; RateLookups={4}", lines.Count, convertedCount, failedCount, total, rateLookupCount), LogLevel.Info);
            }
            catch (Exception exception)
            {
                Log("Operation=ConvertExpenseClaim; ExceptionType=" + exception.GetType().Name + "; Message=" + exception.Message, LogLevel.Error);
                throw;
            }
        }

        /// <summary>Processes one line and keeps line-level business failures non-fatal.</summary>
        private void ProcessLine(ExpenseLine line, string targetCurrency, Dictionary<string, RateLookup> rateCache, ref int rateLookupCount)
        {
            if (string.Equals(line.Status, "Failed", StringComparison.Ordinal) && !string.IsNullOrEmpty(line.Message))
            {
                FailLine(line, line.Message);
                return;
            }

            line.Status = "Failed";
            line.Message = string.Empty;
            if (!ExpenseCalculator.TryParseAmount(line.AmountText, out decimal amount, out string amountError))
            {
                FailLine(line, amountError);
                return;
            }
            if (!ExpenseCalculator.TryValidateCurrency(line.Currency, out string currency, out string currencyError))
            {
                FailLine(line, currencyError);
                return;
            }
            if (!ExpenseCalculator.TryParseExpenseDate(line.ExpenseDateText, out DateTime expenseDate, out string dateError))
            {
                FailLine(line, dateError);
                return;
            }
            if (expenseDate.Date > DateTime.Today)
            {
                FailLine(line, "Invalid date: cannot be in the future");
                return;
            }

            RateLookup lookup;
            if (string.Equals(currency, targetCurrency, StringComparison.Ordinal))
            {
                lookup = new(true, 1m, expenseDate, string.Empty);
            }
            else
            {
                lookup = GetCachedRate(currency, targetCurrency, expenseDate, rateCache, ref rateLookupCount);
            }
            if (!lookup.Success)
            {
                FailLine(line, lookup.ErrorMessage);
                return;
            }

            line.Currency = currency;
            line.Rate = lookup.Rate;
            line.RateDate = lookup.RateDate;
            line.ConvertedAmount = ExpenseCalculator.CalculateConvertedAmount(amount, lookup.Rate);
            line.Status = "Converted";
            line.Message = string.Empty;
        }

        /// <summary>Returns a cached rate or invokes the installed FX.Rates library once for the key.</summary>
        private RateLookup GetCachedRate(string currency, string targetCurrency, DateTime expenseDate, Dictionary<string, RateLookup> rateCache, ref int rateLookupCount)
        {
            string cacheKey = currency + "|" + expenseDate.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
            if (rateCache.TryGetValue(cacheKey, out RateLookup cached))
            {
                return cached;
            }

            rateLookupCount++;
            var result = fXRatesLibraryv2.GetExchangeRate(
                in_BaseCurrency: currency,
                in_QuoteCurrency: targetCurrency,
                in_RateDate: expenseDate,
                isolated: false);
            var lookup = new RateLookup(result.Item1, result.Item2, result.Item3, result.Item4);
            rateCache[cacheKey] = lookup;
            return lookup;
        }

        /// <summary>Marks a line as failed and logs only its identifier and concise reason.</summary>
        private void FailLine(ExpenseLine line, string message)
        {
            line.Status = "Failed";
            line.Message = message ?? "Rate lookup failed";
            line.Rate = 0m;
            line.RateDate = null;
            line.ConvertedAmount = null;
            Log("Line " + line.LineId + " failed: " + line.Message, LogLevel.Warn);
        }

        private sealed class RateLookup
        {
            internal RateLookup(bool success, decimal rate, DateTime rateDate, string errorMessage)
            {
                Success = success;
                Rate = rate;
                RateDate = rateDate;
                ErrorMessage = errorMessage ?? string.Empty;
            }

            internal bool Success { get; }
            internal decimal Rate { get; }
            internal DateTime RateDate { get; }
            internal string ErrorMessage { get; }
        }
    }
}
