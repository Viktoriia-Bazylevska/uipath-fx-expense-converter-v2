using System;
using System.Collections.Generic;
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
            List<ExpenseLine> lines = new List<ExpenseLine>
            {
                new ExpenseLine { Status = "Converted", ConvertedAmount = 10.01m },
                new ExpenseLine { Status = "Converted", ConvertedAmount = 2.34m },
                new ExpenseLine { Status = "Failed", ConvertedAmount = null }
            };

            // WHEN: the pure calculator methods process the prepared values.
            decimal total = ExpenseCalculator.CalculateTotal(lines);

            // THEN: every Section 8 coded-test rule is verified without a workflow or API call.
            testing.VerifyAreEqual(10.01m, roundedPositive, "10.005 must round to 10.01.");
            testing.VerifyAreEqual(2.34m, roundedRegular, "2.344 must round to 2.34.");
            testing.VerifyExpression(sameCurrencySuccess && sameCurrencyRate == 1m, "Same currency must return rate 1 without an API call.");
            testing.VerifyExpression(negativeAmountRejected && negativeAmountError == "Invalid amount", "Negative amounts must be rejected.");
            testing.VerifyExpression(invalidCurrencyRejected && invalidCurrencyError == "Invalid currency: must be 3 letters", "Invalid currencies must be rejected.");
            testing.VerifyExpression(futureDateRejected, "Future dates must be rejected.");
            testing.VerifyAreEqual(12.35m, total, "The total must sum converted lines only.");
        }
    }
}
