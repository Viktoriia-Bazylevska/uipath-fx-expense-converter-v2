# Autopilot prompts

These prompts build ExpenseClaimConverter and the FX.Rates library from `docs/solution-design.md`. Record any extra prompt you send (fixes, follow-ups) in `docs/build-log.md`.

How they are written: the skill is named on the first line, one task per chat, the exact file is named, a plan comes before the build, and every build ends with a run as evidence. Platform steps (library export, publishing, the FX.Rates install) are done by hand in Studio; Git runs through Autopilot only when you ask; the prompt itself gives the OK, and Autopilot stops if a check fails.

| # | Chat | Purpose |
| --- | --- | --- |
| 4.1 | 1 | Initial Git commit and push |
| 4.2 | 1 and 2 | Commit and push after each step |
| 5.0 | 1 and 2 | Start of chat: read the design |
| 5.1 | 1 | Plan GetExchangeRate |
| 5.2 | 1 | Build, analyze, run 7 cases |
| 6.1 | 1 | Review (read-only) |
| 6.2 | 1 | Targeted fix, only if needed |
| 7.1 | 1 | Switch to the FX.Rates library |
| 8.1 | 2 | Plan the process part |
| 8.2 | 2 | Build and run |
| 8.3 | 2 | Tests |
| 9.1 | 2 | Whole-project review (read-only) |
| 9.2 | 2 | Bug hunt with edge cases (no changes) |
| 9.3 | 2 | Fix the bugs one at a time |

## 4.1 Initial commit and push (Step 4)

```text
Make the initial Git commit and push for this project in one go. This message is my OK to commit and push, as long as every check below passes.

Repository URL: <your repository URL from Step 2>
Branch: main

1. Check: is this folder a Git repository, which branch is active, and which remotes are set?
   - If it is not a repository: run git init and create the branch main.
   - If origin is missing: add origin with the repository URL above.
   - If origin exists but points to a different URL: stop and tell me; do not change it.
2. Make sure .gitignore contains these lines, and add any that are missing: .local/, .objects/, .screenshots/, *.nupkg, Data/Output/, CLAUDE.md, .tmh/
3. Stage all files. Check that no Data/Output/ files, .nupkg packages, credentials, tokens or tenant URLs are staged. The author name in docs/ is intended; keep it. If anything else looks wrong, stop and tell me before committing.
4. If the checks are clean: commit with exactly this message: "Initial project structure".
5. Push to origin main and set it as the upstream branch. After the push, check the result with git status and git log origin/main -1. Report the push as successful only if origin/main shows the new commit; otherwise (for example, sign-in is needed or the remote is not empty) stop and show me the error.
6. Show me the list of committed files, the commit hash, the branch, the remote URL and the result of the push.
```

## 4.2 Commit and push after each step (Steps 5–10)

```text
Commit and push the current changes in one go. This message is my OK, as long as the check below passes.
1. Stage all changes. Check that no Data/Output/ files, .nupkg packages, credentials, tokens or tenant URLs are staged. If any are, stop and tell me before committing.
2. Commit with exactly this message: "<message>".
3. Push to origin main. After the push, check the result with git status and git log origin/main -1. Report the push as successful only if origin/main shows the new commit; otherwise stop and show me the error.
4. Show me the list of committed files, the commit hash and the result of the push.
```

## 5.0 Start of every chat

```text
Use the uipath-rpa skill.
Follow AGENTS.md and read docs/solution-design.md in this project. The design is the source of truth: components, contracts, rules, conventions and test cases. Follow section 6 (Conventions) in every change.
Do not publish; add only the packages listed in the design. Run Git only when I ask. Do not change files in docs/ unless I ask.
Reply with a 5-line summary of what you understood, and nothing else.
```

## 5.1 Plan GetExchangeRate

```text
Plan GetExchangeRate.xaml exactly as in docs/solution-design.md section 4. Do not create anything yet.
List: the activities in order with their display names, the variables, the packages and versions, how the JSON response is parsed, and which errors the Retry Scope retries.
```

## 5.2 Build, analyze, test

```text
The plan is approved. First, if UiPath.WebAPI.Activities 2.5.2 (docs/solution-design.md section 2) is missing from project.json, add it with that pinned version and restore the packages. Then build GetExchangeRate.xaml in this project, and after that:
1. Add an annotation to EVERY activity (including Sequence, Try Catch, Retry Scope, Assign, If and Log Message), 1-2 lines on what it does and why, plus a description on every argument. Then list every activity display name with its annotation, so I can check that none is missing.
2. Run Workflow Analyzer and fix every error and warning (ST-USG-034 may stay).
3. Run the workflow once for each of the 7 GetExchangeRate test cases in docs/solution-design.md section 8.
Show a table: case, expected, actual output arguments, log level and message, pass or fail. If a case fails, explain why before changing anything.
```

## 6.1 Review (read-only)

```text
Use the uipath-review skill.
Review GetExchangeRate.xaml against docs/solution-design.md sections 4 and 6. Report each finding as Critical, High, Medium or Low, with the activity name and a one-line fix. Do not change anything.
```

## 6.2 Targeted fix (only if needed)

```text
Fix only these findings in GetExchangeRate.xaml: <paste the findings to fix>. Do not change anything else.
Then run Workflow Analyzer and re-run test cases 1, 5 and 6 from docs/solution-design.md section 8, and show the results.
```

## Manual: export and publish FX.Rates

Export As → Library `FX.Rates`; keep only `GetExchangeRate.xaml` and `project.json`; Analyze; publish to the Tenant feed as 1.0.3; install FX.Rates 1.0.3 in ExpenseClaimConverter.

## 7.1 Switch to the library

```text
FX.Rates 1.0.3 is now installed. Delete the local GetExchangeRate.xaml from this project.
Confirm that the GetExchangeRate activity from FX.Rates is available, and show me how to call it from C# with fXRates.GetExchangeRate(...), including its output arguments. Do not create any other files.
```

## 8.1 Plan the process part (new chat, send 5.0 first)

```text
Plan the process part exactly as in docs/solution-design.md sections 3, 5 and 6: Main.xaml (entry point with the default arguments), ConvertExpenseClaim.cs, ExpenseLine.cs, ExpenseCalculator.cs and ExpenseCsv.cs.
Get rates only through fXRates.GetExchangeRate(...) from the FX.Rates package. Do not add a local copy of GetExchangeRate and do not call the API directly. Do not create anything yet.
Show the files, classes and methods, the order of work, and every assumption you are making.
```

## 8.2 Build and run

```text
The plan is approved. First add any package from docs/solution-design.md section 2 that the files need and project.json is missing (for example UiPath.Testing.Activities or UiPath.CodedWorkflows), with pinned versions, and restore the packages. Then build the files, set Main.xaml as the entry point in project.json and set requiresUserInteraction to false.
Add an annotation to EVERY activity in Main.xaml and a description on every argument, and XML doc comments to the public classes and methods. List every activity in Main.xaml with its annotation.
Run Workflow Analyzer and fix every error and warning (ST-USG-034 may stay). Then run Main.xaml with its default arguments and show: lines read, converted, failed, the TOTAL, the number of rate lookups, and the first 3 output rows.
```

Expected: 8 lines read, 5 converted, 3 failed, TOTAL 919.91 EUR, 5 lookups.

## 8.3 Tests

```text
Create the two test cases listed in docs/solution-design.md section 3: ConvertExpenseClaimTests.cs (coded test case with the checks under "Coded test" in section 8) and WorkflowCoverageTests.xaml. Neither may call the API.
Run both and show the result of every verification.
```

## 9.1 Whole-project review (read-only)

```text
Use the uipath-review skill.
Review the whole project against docs/solution-design.md: security, error handling, logging, naming, activity and API choices, test coverage, and anything that could fail silently. Report Critical, High, Medium and Low findings. Do not change anything.
```

## 9.2 Bug hunt (no changes yet)

```text
Use the uipath-rpa skill.
Look for bugs without changing any workflow or code yet. Create the 6 edge-case input files from docs/solution-design.md section 8 ("Edge cases") under test-data/edge-cases/, then run Main.xaml once per file, passing its path as in_InputPath.
For each case show: expected (from the design), actual result, log lines, and OK or BUG. Then list every bug with its cause and a proposed fix. Do not fix anything yet.
```

## 9.3 Fix the bugs

```text
Fix the bugs you listed, one at a time. After each fix, re-run its edge case and the original sample: the sample must still give 5 converted, 3 failed, TOTAL 919.91 EUR.
Where a bug can be tested without the API, add a check for it to ConvertExpenseClaimTests.cs and run the test case. Show a final table: bug, fix, edge case result, sample result.
```
