# Solution design: ExpenseClaimConverter

**Exercise:** 4, REST API Integration with Coding Agents
**Author:** Viktoriia Bazylevska · November 2026
**Purpose:** the single source of truth for the build. Autopilot reads this file before each task (`AGENTS.md` in the project root tells it to). If this file and a prompt disagree, this file wins.

## 1. Goal

Convert the lines of an expense claim to one target currency (default EUR), using ECB reference rates from the public Frankfurter API. The rate lookup is a reusable component (FX.Rates library) that other processes can install.

## 2. Platform

| Item | Value |
| --- | --- |
| Studio | Studio Desktop STS 26.10 or later, with Autopilot |
| Project type | Process `ExpenseClaimConverter` and library `FX.Rates` |
| Compatibility | Windows |
| Expression language | C# |
| API | `https://api.frankfurter.dev/v2/rates` (GET, public, no authentication) |
| Packages | `UiPath.System.Activities`, `UiPath.WebAPI.Activities` 2.5.2, `UiPath.Testing.Activities`, plus what Studio adds for coded workflows (`UiPath.CodedWorkflows`); Autopilot adds and pins them. FX.Rates is installed by the developer. No other packages without asking |
| Orchestrator | Not used at runtime: no assets, queues or credentials |

## 3. Components

| File | Project | Type | Responsibility |
| --- | --- | --- | --- |
| `GetExchangeRate.xaml` | FX.Rates | Low-code workflow | One rate lookup: validation, HTTP Request, JSON parsing, retries, logging. Never throws |
| `Main.xaml` | Process | Low-code workflow, **entry point** | Holds run settings as In arguments with default values; invokes `ConvertExpenseClaim.cs` |
| `ConvertExpenseClaim.cs` | Process | C# coded workflow | Reads the CSV, validates lines, gets rates through `fXRates.GetExchangeRate(...)` with a cache, converts, writes the output |
| `ExpenseLine.cs` | Process | Code source file | Line model |
| `ExpenseCalculator.cs` | Process | Code source file | Static validation, conversion, rounding and total logic (no I/O, no API) |
| `ExpenseCsv.cs` | Process | Code source file | CSV reading and writing |
| `ConvertExpenseClaimTests.cs` | Process | Coded test case | Tests `ExpenseCalculator` without the API |
| `WorkflowCoverageTests.xaml` | Process | Low-code test case | Covers `Main.xaml` with a small same-currency input (no API call), for rule TA-DBP-002 |

**Flow:** `Data/Input/expenses.csv` → `Main.xaml` → `ConvertExpenseClaim.cs` → `fXRates.GetExchangeRate` → Frankfurter → `Data/Output/expenses_converted.csv`

## 4. Contract: GetExchangeRate

| Argument | Direction | Type | Rule |
| --- | --- | --- | --- |
| `in_BaseCurrency` | In | String | 3 letters; trimmed and upper-cased |
| `in_QuoteCurrency` | In | String | 3 letters; trimmed and upper-cased |
| `in_RateDate` | In | DateTime | Not in the future |
| `out_Success` | Out | Boolean | Set on every path |
| `out_Rate` | Out | Decimal | `1` when base = quote (no API call) |
| `out_RateDate` | Out | DateTime | The date the API returned, not the requested date |
| `out_ErrorMessage` | Out | String | Empty on success |

**Behaviour**

1. Validate inputs. On failure: `out_Success = False`, `out_ErrorMessage = "Invalid input: <field and reason>"`, Warning log, no API call.
2. Base = quote: rate 1, `out_RateDate = in_RateDate`, no API call.
3. `GET {BaseUrl}?date={yyyy-MM-dd}&base={base}&quotes={quote}`. Keep the base URL in one variable. HTTP timeout 10 seconds.
4. Retry Scope around the HTTP call: 3 attempts, 2 seconds apart. Retry **timeouts, connection errors and HTTP 5xx**. Never retry 4xx.
5. Status 404 or 422 → "Currency or date not supported". 5xx, timeout or connection error after retries → "Rate service unavailable, try again later". No rate for the quote in the response → "No rate returned for <quote>".
6. Response shape (array): `[{"date":"2026-11-03","base":"USD","quote":"EUR","rate":0.89367}]`. Parse with invariant culture.

## 5. Contract: Main.xaml and ConvertExpenseClaim.cs

**Arguments** (In, String, on both): `in_InputPath` = `Data/Input/expenses.csv`, `in_OutputPath` = `Data/Output/expenses_converted.csv`, `in_TargetCurrency` = `EUR`. The defaults live in `Main.xaml`. Paths are relative to the project folder.

**Input columns:** `LineId, ExpenseDate, Description, Amount, Currency`

**Output columns:** the input columns + `Rate, RateDate, ConvertedAmount, Status, Message`, then a `TOTAL` row.

**Rules**

1. Dates are parsed only as `yyyy-MM-dd` (`DateTime.TryParseExact`, invariant culture). Amounts are parsed with invariant culture.
2. A line is `Failed` when: Amount is not a number or ≤ 0; Currency is not 3 letters; date invalid or in the future; or `GetExchangeRate` returns `out_Success = False` (Message = its `out_ErrorMessage`). A failed line never stops the run.
3. Currency = target → rate 1, no lookup.
4. Cache lookups (also failed ones) in a dictionary keyed by currency + date: each pair is looked up once.
5. Money is `decimal`, never `double`. `ConvertedAmount = Math.Round(Amount * Rate, 2, MidpointRounding.AwayFromZero)`.
6. `TOTAL` = sum of the rounded converted lines only.
7. The output file is overwritten on each run (Info log). The output folder is created if missing.
8. A missing or malformed input file (wrong header) stops the run with a clear error.
9. The job succeeds even when some lines fail; the failed lines are reported in the output and in a Warning summary log. This is a deliberate decision.

## 6. Conventions (all files)

- Arguments `in_` / `out_` / `io_`; descriptive display names on every activity.
- **Annotations:** every activity has one, not only the workflow. That includes Sequence, Try Catch, Retry Scope, Assign, If, Invoke Workflow and Log Message. 1–2 lines: what it does and why. Every argument has a description. Coded files use XML doc comments on public classes and methods.
- Log Message: Info at start and end of each workflow (with counts), Warning for validation failures and failed lines, Error in every Catch. Never log full URLs, whole responses or whole data rows.
- Every external call inside Try Catch; no empty Catch blocks.
- No Invoke Code; no JavaScript; no hard-coded values except the API base URL and the defaults in `Main.xaml`.
- Mark any temporary or test-only value with `// TODO remove` (or an annotation saying so).
- `requiresUserInteraction` = false in `project.json` (headless process).
- Done means: Workflow Analyzer shows 0 errors and 0 warnings (except ST-USG-034, Automation Hub URL, accepted), the project builds, and a local run passes.

## 7. Not in scope

Authentication (the API is keyless), Orchestrator assets and queues, Maestro, API workflows, screen automation, bank or card rates.

## 8. Test cases and expected results

These dates are in the past, so the ECB rates are fixed and the results repeat on every run.

**GetExchangeRate**

| # | Base | Quote | Date | Expected |
| --- | --- | --- | --- | --- |
| 1 | USD | EUR | 2026-11-03 | Success, rate 0.89367, rate date 2026-11-03 |
| 2 | USD | EUR | 2026-11-07 (Saturday) | Success; Frankfurter returns a rate dated 2026-11-07 itself |
| 3 | EUR | EUR | any past date | Success, rate 1, no HTTP call |
| 4 | usd | eur | 2026-11-03 | Success, same as case 1 |
| 5 | US | EUR | 2026-11-03 | Failed "Invalid input", no HTTP call |
| 6 | XYZ | EUR | 2026-11-03 | Failed "Currency or date not supported", Warning log |
| 7 | USD | EUR | tomorrow | Failed "Invalid input", no HTTP call |

**End to end** (`Data/Input/expenses.csv`, 8 lines): 5 Converted (lines 1–5), 3 Failed (6 XYZ, 7 negative amount, 8 future date), **TOTAL 919.91 EUR**, 5 rate lookups (one per distinct currency + date).

**Edge cases** (files in `test-data/edge-cases/`, passed as `in_InputPath`):

| # | Input | Expected |
| --- | --- | --- |
| E1 | Header only, no lines | Run completes, TOTAL 0.00, Warning "No expense lines" |
| E2 | Wrong header | Run stops with a clear error naming the missing column |
| E3 | Description with a comma in quotes: `"Dinner, client"` | Line parsed correctly and Converted |
| E4 | Amount with a decimal comma: `45,50` | Line Failed "Invalid amount" (never read as 4550) |
| E5 | Lower-case currency: `usd` | Converted, same as `USD` |
| E6 | Input file missing | Run stops with a clear error naming the path |

**Coded test:** rounding 10.005 → 10.01 and 2.344 → 2.34; same currency gives rate 1; negative amount, bad currency and future date rejected; TOTAL sums converted lines only.

## 9. Known pitfalls

- `CultureInfo` needs the `System.Globalization` import (CS0103).
- Asked only for "annotations", Autopilot may annotate workflows and arguments but skip activities. Ask for every activity by type and for a list to check.
- The Studio runner may show "Execution aborted" (ST-ANA-003, ST-ANA-009) on runs that completed. Judge by the output and logs.
- Exporting a library from a project copies every file. Export FX.Rates before the code files exist, and keep only `GetExchangeRate.xaml` in it, otherwise the consumer gets CS0436 duplicate-type warnings.
- Publishing, library export and the FX.Rates install are done by a person in Studio, not by Autopilot. Autopilot runs Git only when asked, after showing the changes for approval.
