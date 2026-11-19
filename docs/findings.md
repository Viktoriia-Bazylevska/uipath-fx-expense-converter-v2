# Findings: REST API Integration with Coding Agents

**Project:** ExpenseClaimConverter, with the FX.Rates.Library.v2 library
**Coding agent:** UiPath Autopilot in Studio Desktop (model GPT-5.6 Luna, effort Extra high)
**Build date:** 19 November 2026
**Author:** Viktoriia Bazylevska

## 1. Summary

Autopilot built a working REST API integration from a written design: a reusable library that calls the public Frankfurter API, a C# coded workflow that converts an expense claim, and two test cases. The end-to-end run gives the expected result (5 converted, 3 failed on purpose, TOTAL 919.91 EUR), every test passes, and Workflow Analyzer shows 0 errors.

The build took about 2.5 hours of hands-on time, plus about 2 hours of preparation. Autopilot wrote all the workflows, code and tests. A person was still needed to review its plans against the design, catch one gap the automated review missed, and do the platform steps (library export, publishing, Git set-up).

## 2. What was built

| Component | Type | Result |
| --- | --- | --- |
| `GetExchangeRate.xaml` in **FX.Rates.Library.v2** | Low-code workflow, published as a library | Calls Frankfurter with the HTTP Request activity; input validation, Retry Scope (timeouts, connection errors, 5xx), clear error messages, logging |
| `Main.xaml` | Low-code entry point | Holds the default arguments and invokes the coded workflow |
| `ConvertExpenseClaim.cs` | C# coded workflow | Reads the CSV, validates each line, gets rates from the library with a cache, converts with `decimal`, writes the output and a TOTAL row |
| `ExpenseLine.cs`, `ExpenseCalculator.cs`, `ExpenseCsv.cs` | Code source files | Data model, validation and calculation, CSV reading and writing |
| `ConvertExpenseClaimTests.cs` | Coded test case | 7 checks of rounding, validation and totals, without calling the API |
| `WorkflowCoverageTests.xaml` | Low-code test case | Runs `Main.xaml` on a same-currency input, without calling the API |

**Authentication:** not applicable. Frankfurter is a public, keyless API; no credentials are stored or logged.

## 3. Method

1. **Design first.** All rules (contracts, error messages, retry policy, output formats, test cases with expected results) are in `docs/solution-design.md`. Standing rules for the agent are in `AGENTS.md`, which Autopilot reads at the start of every chat.
2. **Short, specific prompts.** Each names the skill (`uipath-rpa` or `uipath-review`) and the exact file, asks for a plan before the build, and ends with a test run as evidence.
3. **Review gates.** A read-only `uipath-review` after the library workflow and after the whole project; Critical and High findings are fixed before moving on.
4. **Git through Autopilot.** One prompt for the first commit and one reusable prompt for each later commit, with a safety check of the staged files.

## 4. Time

| Step | Coding agent | Manual | What happened |
| --- | --- | --- | --- |
| Preparation | — | ~2 h (estimate) | Design file, `AGENTS.md`, prompts, build guide |
| 1 Prerequisites | — | 5 min | Studio, Autopilot, Git, GitHub |
| 2 GitHub repo | — | 2 min | Empty public repo |
| 3 API and design | — | 10 min | API checked in the browser, sample response saved, design file added |
| 4 Project and Git | 3 min | 1 min | Project created; first commit and push through Autopilot |
| 5 GetExchangeRate | 20 min | — | Plan, build, 7 lookup cases |
| 6 Review gate | 30 min | — | Review, fixes, 5xx retry added |
| 7 Library | 10 min | ~5 min | Export as Library, publish, install; switch to the library |
| 8 Coded workflow and tests | 30 min | — | Plan, corrected and approved; build; 2 test cases |
| 9 Final review | 30 min | — | Whole-project review, fixes, style clean-up |
| **Total (Steps 1–9)** | **~2 h 5 min** | **~25 min** | **≈ 2.5 h hands-on, plus ~2 h preparation** |

The coding-agent time includes waiting while Autopilot builds, runs and checks. The elapsed time on the day was longer because of breaks and set-up questions.

**Estimate for building it by hand without a coding agent:** 11–18 hours for a junior developer, 6–8 hours for a mid-level developer (the same scope: library workflow, coded workflow, CSV handling, tests, review fixes).

## 5. Intervention log

| Step | What Autopilot did | Manual work or correction needed | Prompts |
| --- | --- | --- | --- |
| 4 Git set-up | Ran `git init` checks, added the remote, completed `.gitignore`, committed and pushed | Git Init in Studio first. Two rule conflicts: the first `AGENTS.md` forbade Git, so the rule was changed. `CLAUDE.md` and `.tmh/` were excluded from the repo. Autopilot reported the first push as failed although it worked (PowerShell treats Git's normal messages as errors), so a check with `git log origin/main` was added to the prompt | 6 |
| 5 GetExchangeRate | Planned (asked to re-plan once), built the workflow with annotations, ran all 7 lookup cases: **7 of 7 passed** | `UiPath.WebAPI.Activities` was blocked by a "do not install packages" rule, so it was installed on request and the rule changed. Runtime errors `CS0103 Regex / CultureInfo`: Autopilot used full type names; the project had to be reopened to clear Studio's cached version | 6 |
| 6 Review gate | Review found 2 High and 3 Medium issues; fixed them (retry count, annotations, status routing, nesting warning) | **A person caught a gap the review missed:** HTTP 5xx errors were not retried, against the design. A targeted prompt added it. The 5xx path is checked by review, not by a test (Frankfurter cannot return 5xx on demand) | 4 |
| 7 Library | Confirmed the dependency, deleted the local copy, Analyzer clean | Export as Library and publishing done in Studio (`FX.Rates.Library.v2` 1.0.1). Autopilot reported the C# name as `fXRates`; the generated name is `fXRatesLibraryv2`, which Autopilot found and used in Step 8 | 1 |
| 7 Library refactor | Grouped 22 Assign activities into 6 Multiple Assigns, no behaviour change; 3 cases pass | First attempt stalled; it worked on the second try. One Analyzer warning remains in the library (ST-MRD-009, deep nesting) | 1–2 |
| 8 Coded workflow | Plan with 5 files and 13 [DEFAULT] / [SME REVIEW] assumptions; built all files; end-to-end run **5 converted, 3 failed, TOTAL 919.91 EUR, 5 lookups**; tests **7 of 7** and **2 of 2** passed | The plan was reviewed against the design and corrected in 9 points before approval: strict date parsing, decimal point only (so `45,50` is rejected), a short `Execute()`, header-only file, output formats, TOTAL row, logging, exception types. One XAML validation error in the coverage test, fixed by Autopilot | 3 |
| 9 Final review | Review rated the project "Needs Improvement": 1 Critical, 2 High, 7 Medium, 5 Low. Fixed the High findings and 4 Medium ones, confirmed no unused packages, packed the project; applied 24 code-style suggestions | The Critical finding (missing `.cs.json` metadata) was a false alarm: the project packs and tests are discovered. Each finding was triaged by a person before the fix prompt | 3 |
| Commits | 6 commits pushed and verified on GitHub | Each commit's file list read before moving on | 5 |

**Prompts in total: about 29.** 18 for building and reviewing (10 planned plus 8 extra fixes or questions) and 11 for Git (6 of them to get the first commit working).

## 6. Quality

| Check | Result |
| --- | --- |
| Workflow Analyzer | 0 errors; 1 accepted warning (ST-USG-034, Automation Hub URL) |
| Build and pack | Succeeded (`ExpenseClaimConverter.1.0.0.nupkg`) |
| Library lookup cases | 7 of 7 passed |
| End-to-end run | 8 lines, 5 converted, 3 failed as designed, TOTAL 919.91 EUR, 5 rate lookups |
| Coded test case | 7 of 7 checks passed |
| Coverage test case | 2 of 2 checks passed, no API call |
| Security | No credentials, tokens or tenant URLs in the code or the repo |
| Annotations | On every activity and argument, as required |

**Known limitations, not fixed:**

| ID | Limitation |
| --- | --- |
| M-003 | The output file is not re-read and checked after writing |
| M-005 | Extra CSV columns are accepted and ignored; the design does not define this |
| M-006 | Edge cases E1–E6 (empty file, wrong header, comma in a description, decimal comma, lower-case codes, missing file) are designed but not yet run |
| — | The 5xx retry path is verified by review only |
| — | The library still has one Analyzer warning (ST-MRD-009, deep nesting) |
| L-001 | Descriptions containing line breaks are not supported |
| L-002, L-005 | Some test assertions are text matches; there is no automated test for a failed rate lookup |
| — | A runtime statistics file (`.storage/`) was committed; it should be added to `.gitignore` |

## 7. Answers to the key questions

| Question | Answer | Evidence |
| --- | --- | --- |
| **Can Coding Agents accelerate API development?** | **Yes** | About 2.5 h hands-on plus ~2 h preparation, against an estimated 11–18 h for a junior developer by hand. Autopilot wrote every workflow, code file and test |
| **What level of quality is produced?** | **Good, after review** | 0 errors, all tests pass, correct end-to-end result. The first whole-project review rated it "Needs Improvement"; the High findings were fixed in one prompt. Not production-ready until the edge cases run |
| **How much manual correction is required?** | **Moderate** | About 8 extra build prompts and 9 corrections to one plan. The most important catch was made by a person (missing 5xx retry). Platform steps (Git Init, library export and publish) stayed manual. Git through Autopilot took 6 prompts to set up, then worked with one prompt per commit |
| **Can the generated solution be reused?** | **Yes** | `GetExchangeRate` is published as `FX.Rates.Library.v2` in the tenant feed and called from C# through the generated service. Any project can install it and use the activity from the Activities panel; the design, `AGENTS.md` and prompts can be reused as a template for the next API integration |

## 8. Lessons learned

1. **Write rules in the design, not in prompts.** Gaps in Autopilot's plan were rules the design had not stated; once written down, the next plan covers them.
2. **Check the plan line by line.** Asking the plan to map every design rule and edge case makes gaps visible before any code exists.
3. **Trust the run, not the "done" message.** The review missed the 5xx retry; Autopilot reported a successful push as failed and gave the wrong C# name once. Each was caught by checking the actual result.
4. **Keep standing rules short and general.** A rule like "do not install packages" blocked normal work; specific error fixes belong in a prompt, not in `AGENTS.md`.
5. **Start a new chat after each big step.** One chat reached 81% of Autopilot's context during Steps 5–6.
6. **Studio quirks are not agent errors.** "Execution aborted" (ST-ANA-003/009) on completed runs, a cached old file version, and the "modified externally" banner all needed a person to recognise them.

## 9. Repository

| Commit | Message |
| --- | --- |
| `965a5c5` | Initial project structure |
| `ff42613` | Add GetExchangeRate workflow |
| `7798600` | Review fixes for GetExchangeRate |
| `e2d5399` | Use FX.Rates library |
| `24d46da` | Add coded workflow, Main and tests |
| `0e99a6a` | Final review fixes |
