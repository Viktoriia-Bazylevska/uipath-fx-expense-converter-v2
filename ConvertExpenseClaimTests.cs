using System;
using System.Collections.Generic;
using System.IO;
using UiPath.CodedWorkflows;

namespace ExpenseClaimConverter
{
    /// <summary>Verifies the pure expense parsing, validation, same-currency, rounding, and total rules without API access.</summary>
    public class ConvertExpenseClaimTests : CodedWorkflow
    {
        /// <summary>Runs the Section 8 coded calculator checks as Given-When-Then assertions.</summary>
        [TestCase]
        public void Execute()
        {
            // TODO remove: deterministic test-only values and in-memory expense lines.
            // GIVEN: deterministic values and in-memory expense lines only.
            decimal roundedPositive = ExpenseCalculator.CalculateConvertedAmount(10.005m, 1m);
            decimal roundedRegular = ExpenseCalculator.CalculateConvertedAmount(2.344m, 1m);
            bool sameCurrencySuccess = ExpenseCalculator.TryGetSameCurrencyRate("EUR", "EUR", out decimal sameCurrencyRate);
            bool negativeAmountRejected = !ExpenseCalculator.TryParseAmount("-5.00", out _, out string negativeAmountError);
            bool invalidCurrencyRejected = !ExpenseCalculator.TryValidateCurrency("US", out _, out string invalidCurrencyError);
            bool futureDateRejected = ExpenseCalculator.IsFutureDate(DateTime.Today.AddDays(1));
            List<ExpenseLine> lines =
            [
                new() { Status = "Converted", ConvertedAmount = 10.01m },
                new() { Status = "Converted", ConvertedAmount = 2.34m },
                new() { Status = "Failed", ConvertedAmount = null }
            ];

            // GIVEN: temporary CSV fixtures cover both valid quoted and malformed unquoted decimal-comma forms.
            string testDirectory = Path.Combine(Path.GetTempPath(), "ExpenseClaimConverter_E4_" + Guid.NewGuid().ToString("N")); // TODO remove: test-only fixture directory.
            Directory.CreateDirectory(testDirectory);
            string quotedPath = Path.Combine(testDirectory, "quoted.csv"); // TODO remove: test-only fixture path.
            string unquotedPath = Path.Combine(testDirectory, "unquoted.csv"); // TODO remove: test-only fixture path.
            File.WriteAllText(quotedPath, "LineId,ExpenseDate,Description,Amount,Currency" + Environment.NewLine + "4,2026-11-03,Decimal comma amount,\"45,50\",USD" + Environment.NewLine);
            File.WriteAllText(unquotedPath, "LineId,ExpenseDate,Description,Amount,Currency" + Environment.NewLine + "4,2026-11-03,Decimal comma amount,45,50,USD" + Environment.NewLine);
            IReadOnlyList<ExpenseLine> quotedLines = ExpenseCsv.Read(quotedPath);
            IReadOnlyList<ExpenseLine> unquotedLines = ExpenseCsv.Read(unquotedPath);
            bool quotedAmountRejected = quotedLines.Count == 1 && !ExpenseCalculator.TryParseAmount(quotedLines[0].AmountText, out _, out string quotedAmountError) && quotedAmountError == "Invalid amount";
            bool unquotedLineRejected = unquotedLines.Count == 1 && unquotedLines[0].AmountText == string.Empty && unquotedLines[0].Currency == string.Empty && unquotedLines[0].Rate == 0m && !unquotedLines[0].RateDate.HasValue && !unquotedLines[0].ConvertedAmount.HasValue && unquotedLines[0].Status == "Failed" && unquotedLines[0].Message == "Invalid line: expected 5 fields, found 6";

            // WHEN: the pure calculator methods process the prepared values.
            decimal total = ExpenseCalculator.CalculateTotal(lines);

            // THEN: every Section 8 coded-test rule is verified without a workflow or API call.
            testing.VerifyAreEqual(10.01m, roundedPositive, "10.005 must round to 10.01.");
            testing.VerifyAreEqual(2.34m, roundedRegular, "2.344 must round to 2.34.");
            testing.VerifyExpression(sameCurrencySuccess && sameCurrencyRate == 1m, "Same currency must return rate 1 without an API call.");
            testing.VerifyExpression(negativeAmountRejected && negativeAmountError == "Invalid amount", "Negative amounts must be rejected.");
            testing.VerifyExpression(invalidCurrencyRejected && invalidCurrencyError == "Invalid currency: must be 3 letters", "Invalid currencies must be rejected.");
            testing.VerifyExpression(futureDateRejected, "Future dates must be rejected.");
            testing.VerifyExpression(quotedAmountRejected, "A quoted decimal-comma amount must be rejected as Invalid amount.");
            testing.VerifyExpression(unquotedLineRejected, "An unquoted decimal-comma amount must be rejected as an invalid six-field line without shifting fields.");
            testing.VerifyAreEqual(12.35m, total, "The total must sum converted lines only.");
        }
    }
}
