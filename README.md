# UiPath FX Expense Converter

Converts expense-claim lines to EUR (or another currency) using ECB reference rates from the public [Frankfurter API](https://frankfurter.dev).

Built for **Exercise 4: REST API Integration with Coding Agents**. The workflows, helper classes and tests are generated with **UiPath Autopilot** (coding agent) in Studio Desktop from `docs/solution-design.md`, then reviewed and tested by hand.

## How it works

```text
expenses.csv → Main.xaml → ConvertExpenseClaim.cs → GetExchangeRate (FX.Rates library) → Frankfurter API
                                    ↓
                       expenses_converted.csv
```

| Component | Type | What it does |
| --- | --- | --- |
| `Main.xaml` | Low-code workflow (entry point) | Holds the run settings and invokes the coded workflow |
| `ConvertExpenseClaim.cs` | C# coded workflow | Reads the CSV, validates each line, gets rates (cached per currency and date), converts, rounds, writes the result with a total |
| `ExpenseLine.cs`, `ExpenseCalculator.cs`, `ExpenseCsv.cs` | Code source files | Data model, validation, calculation and CSV handling |
| `ConvertExpenseClaimTests.cs` | Coded test case | Tests rounding, validation and totals |
| `WorkflowCoverageTests.xaml` | Test case | Covers `Main.xaml` without calling the API |
| `GetExchangeRate` | Activity from the **FX.Rates** library | Calls the Frankfurter API with validation, retries, error handling and logging |

## Requirements

- UiPath Studio 26.10 or later (Windows project, C# expressions)
- The **FX.Rates** library package (FX.Rates.Library.v2, version 1.0.3 or later) installed from the Orchestrator tenant feed. Library code: [Fx.Rate-UiPath-Library-v2](https://github.com/Viktoriia-Bazylevska/Fx.Rate-UiPath-Library-v2)
- Internet access to `api.frankfurter.dev`. No API key is needed.

## How to run

1. Clone the repo and open `project.json` in UiPath Studio.
2. Check that FX.Rates is installed: **Manage Packages** → your Orchestrator tenant feed.
3. Run `Main.xaml`. Default arguments:

| Argument | Default |
| --- | --- |
| `in_InputPath` | `Data/Input/expenses.csv` |
| `in_OutputPath` | `Data/Output/expenses_converted.csv` |
| `in_TargetCurrency` | `EUR` |

4. Open `Data/Output/expenses_converted.csv`.

**Run outside Studio:** publish with Studio's **Publish** button to your Personal Workspace (Orchestrator creates the process automatically), then start **ExpenseClaimConverter** from UiPath Assistant. Expected: 5 converted, 3 failed, TOTAL 919.91 EUR.

## Input and output

**Input** (`Data/Input/expenses.csv`): `LineId, ExpenseDate (yyyy-MM-dd), Description, Amount, Currency`

**Output:** the input columns plus `Rate, RateDate, ConvertedAmount, Status, Message`, and a final total row.

- `Status` is `Converted` or `Failed`. A failed line does not stop the run.
- Amounts use `decimal` and are rounded to 2 places (`MidpointRounding.AwayFromZero`); the total sums the rounded converted lines.

The sample input has 8 made-up lines: 5 convert, 3 fail on purpose (unsupported currency, negative amount, future date). Expected TOTAL: 919.91 EUR.

## Error handling and logging

- Input validation before any API call (3-letter currency codes, valid dates, positive amounts)
- Try Catch around the HTTP call; Retry Scope (3 attempts, 2 seconds apart, 10-second timeout) for timeouts, connection errors and HTTP 5xx only
- Clear messages: "Invalid input", "Currency or date not supported", "Rate service unavailable, try again later"
- Log Message at Info, Warning and Error levels; full URLs, responses and data rows are never logged

## Repository contents

| Path | Contents |
| --- | --- |
| `AGENTS.md` | Standing rules Autopilot reads at the start of every session |
| `docs/solution-design.md` | Design spec: components, contracts, rules, conventions, test cases with expected results |
| `docs/prompts.md` | The Autopilot prompts used to build the project |
| `docs/build-log.md` | Time, prompts and manual corrections per step |
| `docs/findings.md` | Exercise 4 evaluation: answers to the key questions |
| `Data/Input/` | Sample test data (made up) |
| `test-data/` | Sample Frankfurter API response and edge-case files |
| `docs/screenshots/` | Test runs and logs |

## Notes

- **Authentication:** not applicable. Frankfurter is a public, keyless API, and this repo contains no secrets.
- Rates are ECB reference rates, not bank or card rates.
- This is a proof of concept. Review it before using it against live systems.

## Author

Viktoriia Bazylevska
