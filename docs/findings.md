# Findings: REST API Integration with Coding Agents

**Project:** ExpenseClaimConverter, with the FX.Rates.Library.v2 library
**Coding agent:** UiPath Autopilot in Studio Desktop (model GPT-5.6 Luna, effort Extra high)
**Build date:** 19 November 2026
**Author:** Viktoriia Bazylevska

## 1. Summary

Autopilot built a working REST API integration from a written design: a reusable library that calls the public Frankfurter API, a C# coded workflow that converts an expense claim, and two test cases. The end-to-end run gives the expected result (5 converted, 3 failed on purpose, TOTAL 919.91 EUR), all six edge cases behave as designed after one fix, every test passes, and Workflow Analyzer shows 0 errors.

The build took about 3 hours 40 minutes of hands-on time, plus about 2 hours of preparation. Autopilot wrote all the workflows, code and tests. A person was still needed to review its plans against the design, catch one gap the automated review missed, decide which bug-hunt findings to fix, and do the platform steps (library export, publishing, Git set-up).

## 2. What was built

| Component | Type | Result |
| --- | --- | --- |
| `GetExchangeRate.xaml` in **FX.Rates.Library.v2** | Low-code workflow, published as a library | Calls Frankfurter with the HTTP Request activity; input validation, Retry Scope (timeouts, connection errors, 5xx), clear error messages, logging |
| `Main.xaml` | Low-code entry point | Holds the default arguments and invokes the coded workflow |
| `ConvertExpenseClaim.cs` | C# coded workflow | Reads the CSV, validates each line, gets rates from the library with a cache, converts with `decimal`, writes the output and a TOTAL row |
| `ExpenseLine.cs`, `ExpenseCalculator.cs`, `ExpenseCsv.cs` | Code source files | Data model, validation and calculation, CSV reading and writing |
| `ConvertExpenseClaimTests.cs` | Coded test case | Checks of rounding, validation, totals and both decimal-comma forms (E4), without calling the API |
| `test-data/edge-cases/` | Test data | Input files for edge cases E1–E5 (E6 is a missing file on purpose) |
| `WorkflowCoverageTests.xaml` | Low-code test case | Runs `Main.xaml` on a same-currency input, without calling the API |

**Authentication:** not applicable. Frankfurter is a public, keyless API; no credentials are stored or logged.

## 3. Method

1. **Design first.** All rules (contracts, error messages, retry policy, output formats, test cases with expected results) are in `docs/solution-design.md`. Standing rules for the agent are in `AGENTS.md`, which Autopilot reads at the start of every chat.
2. **Short, specific prompts.** Each names the skill (`uipath-rpa` or `uipath-review`) and the exact file, asks for a plan before the build, and ends with a test run as evidence.
3. **Review gates.** A read-only `uipath-review` after the library workflow and after the whole project, then a bug hunt with six edge cases. A person triages every finding (fix now or accept as a limitation) before the fix prompt.
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
| 9 Final review and bug hunt | 95 min | ~5 min | Whole-project review, fixes, style clean-up (30 min); edge cases E1–E6, triage, two fixes, reruns, commit (65 min); project reopened once |
| Follow-up: library 1.0.3 | ~21 min | ~10 min | After the senior review: library clean-up, publish 1.0.3, process updated and re-tested |
| 10 Publish and run | (test ~14 min) | ~3 min | Published to Personal Workspace, run from UiPath Assistant; the Autopilot publish was a separate test |
| **Total (Steps 1–9)** | **~3 h 10 min** | **~30 min** | **≈ 3 h 40 min hands-on, plus ~2 h preparation** |

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
| 9 Bug hunt (9.2, 9.3) | Created E1–E5 inputs, ran all six cases: 5 of 6 OK. E4 bug found: an unquoted `45,50` shifted the fields and gave "Invalid currency". Fixed: a line with the wrong number of fields is Failed "Invalid line: expected 5 fields, found N"; quoted `"45,50"` gives "Invalid amount". Removed 3 unused template packages (Excel, Mail, UIAutomation). Afterwards E1–E6, both tests and the sample all pass | Autopilot asked how to test E6 (answered: a path that does not exist). A person triaged its 3 bugs: fixed 2, accepted the relative-path risk as a limitation, and decided the E4 behaviour the design had not stated. Studio could not reload the project after the package change, so it was reopened by hand and a "continue" prompt sent | 4 |
| Follow-up: library 1.0.3 (after review) | Removed 5 unused packages from the library (Excel, Mail, UIAutomation, Testing, CodedWorkflows), set the entry point to GetExchangeRate.xaml, removed a stale assembly reference; the three smoke-test cases (USD 0.89367, EUR 1, XYZ unsupported) gave the expected outputs. After the update to 1.0.3: both process tests and the sample pass (5 / 3 / 919.91 EUR, 5 lookups) | The review found that Export as Library had copied the process project's packages and files into the library. Files deleted and README/AGENTS.md replaced by hand. Autopilot got stuck closing Studio sessions after the package change (second time; stopped by hand). Its push stopped correctly because the remote was not named `origin`; committed from Studio's Source Control panel. Version 1.0.3 published by hand | 5 |
| 10 Publish (test) | With the uipath-rpa and uipath-platform skills: read-only preflight, then packed and published version 1.0.2 to Personal Workspace on the first try; Orchestrator created the process automatically | Two 403 permission errors on the training tenant (listing workspaces and sessions), accepted as a limitation. Job start from Orchestrator cancelled (no unattended robot needed); the process was run from UiPath Assistant and gave the expected result. Publishing by hand takes ~3 min against ~14 min with Autopilot | 2 (test) |
| Commits | 7 commits pushed and verified on GitHub; `.storage/` and the run results (with local paths) added to `.gitignore` | Each commit's file list read before moving on | 7 |

**Prompts in total: about 35**, plus 2 for the optional publishing test: 22 for building and reviewing (12 planned plus 10 extra fixes, answers or questions) and 13 for Git (6 of them to get the first commit working).

## 6. Quality

| Check | Result |
| --- | --- |
| Workflow Analyzer | 0 errors; 1 accepted warning (ST-USG-034, Automation Hub URL) |
| Build and pack | Succeeded (`ExpenseClaimConverter.1.0.0.nupkg`) |
| Library lookup cases | 7 of 7 passed |
| End-to-end run | 8 lines, 5 converted, 3 failed as designed, TOTAL 919.91 EUR, 5 rate lookups |
| Coded test case | All checks passed, including both E4 forms |
| Coverage test case | 2 of 2 checks passed, no API call |
| Edge cases E1–E6 | 6 of 6 as designed after one fix (E4) |
| Library dependencies | FX.Rates.Library.v2 1.0.3 depends only on UiPath.System.Activities and UiPath.WebAPI.Activities |
| Published package | Version 1.0.2 in Personal Workspace; run from UiPath Assistant: 5 converted, 3 failed, TOTAL 919.91 EUR |
| Packages | Only those in the design: System, WebAPI, Testing, CodedWorkflows and the FX library |
| Security | No credentials, tokens or tenant URLs in the code or the repo |
| Annotations | On every activity and argument, as required |

**Known limitations, not fixed:**

| ID | Limitation |
| --- | --- |
| M-003 | The output file is not re-read and checked after writing |
| — | Relative input and output paths are resolved from the process's working directory, not explicitly from the project folder. No run showed a problem (Studio and the Robot start in the project folder); accepted as a limitation |
| — | The 5xx retry path is verified by review only |
| — | The library still has one Analyzer warning (ST-MRD-009, deep nesting) |
| — | The output CSV is not protected against formula injection (a description starting with `=`, `+`, `-` or `@`) |
| — | One unclosed quote in the input stops the whole run instead of failing one line |
| L-001 | Descriptions containing line breaks are not supported |
| L-002, L-005 | Some test assertions are text matches; there is no automated test for a failed rate lookup |

## 7. Answers to the key questions

| Question | Answer | Evidence |
| --- | --- | --- |
| **Can Coding Agents accelerate API development?** | **Yes** | About 3 h 40 min hands-on plus ~2 h preparation, against an estimated 11–18 h for a junior developer by hand. Autopilot wrote every workflow, code file and test |
| **What level of quality is produced?** | **Good, after review** | 0 errors, all tests pass, correct end-to-end result. The first whole-project review rated it "Needs Improvement"; the High findings were fixed in one prompt. The bug hunt found one real bug (E4); after the fix all six edge cases pass. A few accepted limitations remain before production |
| **How much manual correction is required?** | **Moderate** | About 10 extra build prompts and 9 corrections to one plan. The most important catch was made by a person (missing 5xx retry), and a person decided which bug-hunt findings to fix. Platform steps (Git Init, library export and publish) stayed manual. Git through Autopilot took 6 prompts to set up, then worked with one prompt per commit |
| **Can the generated solution be reused?** | **Yes** | `GetExchangeRate` is published as `FX.Rates.Library.v2` in the tenant feed and called from C# through the generated service. Any project can install it and use the activity from the Activities panel. The process itself is published and runs from UiPath Assistant, with the library resolved from the feed; the design, `AGENTS.md` and prompts can be reused as a template for the next API integration |

## 8. Lessons learned

1. **Write rules in the design, not in prompts.** Gaps in Autopilot's plan were rules the design had not stated; once written down, the next plan covers them.
2. **Check the plan line by line.** Asking the plan to map every design rule and edge case makes gaps visible before any code exists.
3. **Trust the run, not the "done" message.** The review missed the 5xx retry; Autopilot reported a successful push as failed and gave the wrong C# name once. Each was caught by checking the actual result.
4. **Keep standing rules short and general.** A rule like "do not install packages" blocked normal work; specific error fixes belong in a prompt, not in `AGENTS.md`.
5. **Start a new chat after each big step.** One chat reached 81% of Autopilot's context during Steps 5–6.
6. **Triage before you fix.** Not every finding is a bug: the bug hunt reported three, one was real, one was a clean-up, and one was a risk no run had shown. Deciding which to fix is a person's job.
7. **Platform steps: Autopilot can, a person is faster.** In v1 Autopilot's publish failed; in v2, with the platform skills, it published first time, but Studio's Publish button took about 3 minutes against 14.
8. **Export as Library copies too much.** The new library inherited the process project's packages and files, and every consumer would inherit those packages. Check the library's `project.json` before the first publish.
9. **A package change means a project reopen.** Twice Autopilot could not reload Studio after removing packages; a person had to close and reopen the project.
10. **Studio quirks are not agent errors.** "Execution aborted" (ST-ANA-003/009) on completed runs, a cached old file version, and the "modified externally" banner all needed a person to recognise them.

## 9. Senior review (after the build)

A senior-developer review of both public repositories, read-only, after the build.

| # | Finding | Status |
| --- | --- | --- |
| 1 | The library listed 5 unused packages that every consumer would inherit | Fixed in 1.0.3 |
| 2 | The library entry point was `NewActivity.xaml`, which does not exist | Fixed in 1.0.3 |
| 3 | The library repository contained the process project's files and `.storage/` | Fixed in 1.0.3 |
| 4 | The library README had the wrong C# name, the wrong package name and no version history | Fixed in 1.0.3 |
| 5 | The library version in the repository did not match the feed | Fixed: both at 1.0.3 |
| 6 | `docs/build-log.md` was still the empty template | Open: to fill in |
| 7 | The output CSV is not protected against formula injection | Open: known limitation |
| 8 | One unclosed quote stops the whole run | Open: known limitation |
| 9 | The test case has `TODO remove` markers and does not delete its temp files | Open: cosmetic |
| 10 | All checks are in one test case; no test for a failed rate lookup | Open: known limitation (L-002, L-005) |

**Overall:** good for a proof of concept: a clear architecture, strict parsing and decimal maths, edge cases designed and tested, UiPath naming, annotations and logging conventions followed. Not production-ready until items 7–8 and the accepted limitations are addressed, and configuration (API base URL, timeout) moves to Orchestrator assets.

## 10. Repository

| Commit | Message |
| --- | --- |
| `965a5c5` | Initial project structure |
| `ff42613` | Add GetExchangeRate workflow |
| `7798600` | Review fixes for GetExchangeRate |
| `e2d5399` | Use FX.Rates library |
| `24d46da` | Add coded workflow, Main and tests |
| `0e99a6a` | Final review fixes |
| `86ac0ff` | Bug hunt fixes (E1–E6) |
| `d22ce74` | Use FX.Rates.Library.v2 1.0.3 |

**Library repository** ([Fx.Rate-UiPath-Library-v2](https://github.com/Viktoriia-Bazylevska/Fx.Rate-UiPath-Library-v2)): 1.0.1 first release (Export as Library); 1.0.2 grouped 22 Assigns into 6 Multiple Assigns; 1.0.3 (`406164f`) removed unused dependencies, fixed the entry point and added a README with arguments, behaviour and version history. No behaviour change in 1.0.2 or 1.0.3.
