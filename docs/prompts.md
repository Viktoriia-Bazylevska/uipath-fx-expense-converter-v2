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
| 7.1 | 2 | Switch to the library |
| 7.2 | 2 | Clean up the library project (library chat) |
| 8.1 | 2 | Plan the process part |
| 8.2 | 2 | Build and run |
| 8.3 | 2 | Tests |
| 9.1 | 2 | Whole-project review (read-only) |
| 9.2 | 2 | Bug hunt with edge cases (no changes) |
| 9.3 | 2 | Fix the bugs you chose, one at a time |

## 4.1 Initial commit and push (Step 4)

```text
Make the initial Git commit and push for this project in one go. This message is my OK to commit and push, as long as every check below passes.

Repository URL: <your repository URL from Step 2>
Branch: main

1. Check: is this folder a Git repository, which branch is active, and which remotes are set?
   - If it is not a repository: run git init and create the branch main.
   - If origin is missing: add origin with the repository URL above.
   - If origin exists but points to a different URL: stop and tell me; do not change it.
2. Make sure .gitignore contains these lines, and add any that are missing: .local/, .objects/, .screenshots/, *.nupkg, Data/Output/, CLAUDE.md, .tmh/, .storage/, test-data/edge-cases/results/
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
3. Push to the configured remote (check with git remote -v; do not add or rename remotes). After the push, check the result with git status and git log <remote>/<branch> -1. Report the push as successful only if it shows the new commit; otherwise stop and show me the error.
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
The plan is approved. First, if UiPath.WebAPI.Activities 2.5.2 (docs/solution-design.md section 2) is missing from project.json, add it with that pinned version and restore the packages. Then build GetExchangeRate.xaml in this project. Group consecutive Assign activities into one Multiple Assign with a clear display name (for example "Initialize outputs", "Normalize and validate inputs") instead of a separate Assign per variable. After that:
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

## Manual: Export as Library (Step 7)

In Studio: Export as Library → tick only `GetExchangeRate.xaml` → keep Publish and install, Alter your workflows, Delete replaced workflows → publish to the Tenant feed with a package name and version.

## 7.1 Switch to the library (new chat, send 5.0 first)

```text
The library <package name> <version> has been exported, published and installed with Studio's Export as Library. Check the project and change only what is needed:
1. Confirm <package name> is a dependency in project.json with the version pinned to [<version>].
2. If a local GetExchangeRate.xaml still exists in this project, delete it. Check that nothing in the project still invokes a local GetExchangeRate.xaml.
3. Confirm that the GetExchangeRate activity from the library is available, and show me how to call it from C#: the exact call Studio generated for this package, its in arguments and its output arguments (out_Success, out_Rate, out_RateDate, out_ErrorMessage).
4. Run Workflow Analyzer and show the result.
Do not create any other files.
```

## 7.2 Clean up the library project (in the library project, new chat)

First delete the copied `Data/` and `docs/` files in the library folder and add `FX.Rates/README.md` and `FX.Rates/AGENTS.md` from this kit. After the prompt, publish the next version from Studio and update it in the process project.

```text
Use the uipath-rpa skill. Read AGENTS.md first. This is the <package name> library project; its public contract is GetExchangeRate.xaml and the Arguments and Behaviour sections of README.md. Do not change arguments, behaviour or messages.
Clean up the project settings only:
1. In project.json, set "main" to GetExchangeRate.xaml.
2. Confirm that no workflow or code file uses UiPath.Excel.Activities, UiPath.Mail.Activities, UiPath.UIAutomation.Activities, UiPath.Testing.Activities or UiPath.CodedWorkflows. Remove those that are unused, including stale assembly references in the XAML. Keep UiPath.System.Activities and UiPath.WebAPI.Activities. If Studio needs one of the others, keep it and tell me why. This message is my OK for this package change only.
3. Set requiresUserInteraction to false.
4. Validate GetExchangeRate.xaml and build. If Studio cannot reload the project, stop and tell me; I will reopen it.
5. Run GetExchangeRate.xaml three times with input arguments and show the outputs:
   - USD to EUR, 2026-11-03: Success True, Rate 0.89367
   - EUR to EUR, 2026-11-03: Success True, Rate 1, no API call
   - XYZ to EUR, 2026-11-03: Success False, "Currency or date not supported"
6. Show the final dependencies in project.json and a table: change, result.
Do not publish and do not run Git.
```

## 8.1 Plan the process part (new chat, send 5.0 first)

```text
Plan the process part exactly as in docs/solution-design.md sections 3, 5 and 6: Main.xaml (entry point with the default arguments), ConvertExpenseClaim.cs, ExpenseLine.cs, ExpenseCalculator.cs and ExpenseCsv.cs. Do not create anything yet.
Get rates only through the GetExchangeRate activity from the library installed in Step 7 (<package name>), using the C# call confirmed in Step 7. No local copy of GetExchangeRate and no direct API call.
Show me:
1. The files, classes and methods, and the order of work.
2. How each rule in section 5 (rules 1-13) is implemented, one line per rule.
3. How the edge cases E1-E6 in section 8 will behave.
4. Every remaining assumption, marked [DEFAULT] or [SME REVIEW]. Do not invent rules that are not in the design; ask instead.
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
Use the uipath-rpa skill. Look for bugs without changing any workflow or code.
1. Create the input files for edge cases E1–E5 from docs/solution-design.md section 8 under test-data/edge-cases/. For E6 do not create a file: pass a path that does not exist.
2. Run Main.xaml once per case, passing the path as in_InputPath. Save run output only under test-data/edge-cases/results/ (Git ignores it).
3. For each case show: expected (from the design), actual result, key log lines, and OK or BUG. Judge by the output file and the logs: "Execution aborted" (ST-ANA-003, ST-ANA-009) on a completed run is not a failure.
4. Check the packages in project.json against the design's package list.
5. List every bug with its cause, its file and a proposed fix. Do not fix anything yet.
```

Before 9.3: compare each bug with the design, decide fix or accept, and write the decision in docs/build-log.md.

## 9.3 Fix the bugs you chose (fill in the placeholders)

```text
Use the uipath-rpa skill. Fix only the bugs below, one at a time. Do not change docs/ or AGENTS.md.
1. <file>: <the bug> — <the behaviour you want, with the exact message>.
2. <file>: <the bug> — <the behaviour you want>.
Do not fix: <the bug> — I accept it as a known limitation.
After each fix, validate the changed files and build. Where a bug can be tested without the API, add a check for it to ConvertExpenseClaimTests.cs.
Then run E1–E6, ConvertExpenseClaimTests, WorkflowCoverageTests and the original sample (must still give 5 converted, 3 failed, TOTAL 919.91 EUR). Show a final table: bug, fix, edge-case result, sample result.
```

If Studio could not reload the project, reopen it and send:

```text
I reopened the project in Studio. Continue from where you stopped: validate the changed files, build, then run E1–E6, ConvertExpenseClaimTests, WorkflowCoverageTests and the original sample. Show the final table.
```

## Appendix: publish to Orchestrator with Autopilot (tested, optional)

Tested once: it worked first time (version 1.0.2 to Personal Workspace, process created automatically) but took about 14 minutes and 2 prompts; Studio's **Publish** button takes about 3 minutes. `AGENTS.md` keeps "Do not publish"; these prompts give the OK for one version only. Start a new chat with Prompt 5.0 first. On a tenant without an unattended robot, skip the job and run the process from UiPath Assistant.

### P.1 Preflight and publish plan (no changes)

```text
Use the uipath-rpa skill for packing and publishing, and the uipath-platform skill for everything in Orchestrator (folders, packages, libraries, processes, jobs). If a step fails, use the uipath-troubleshoot skill to find the cause.

This is a one-time test of publishing this project to Orchestrator. AGENTS.md says "Do not publish": for this task only, publishing is allowed after my OK in Part B. Do not change any workflow, code file, docs/, AGENTS.md or Git. Do not write tenant names, URLs, IDs or user names into any project file.

Part A: preflight, read-only. Do not pack, publish or start anything yet.
1. Check that the uip CLI is signed in to Orchestrator. Show only the tenant name, not URLs or IDs.
2. Find my Personal Workspace folder, and tell me whether it has a robot (machine and user) available to run an unattended or attended job.
3. Check that the library this project depends on (FX.Rates.Library.v2, the version in project.json) exists in the tenant library feed. If the version in project.json is not in the feed, stop and tell me.
4. Check project.json: name, version, project type (process), entry point Main.xaml, and the dependencies. Confirm that only the packages from the design are listed.
5. Check that Data/Input/expenses.csv will be inside the package, and explain where Main.xaml will read the input and write the output when it runs as a job (the paths are relative to the project folder).
6. Validate and build the project, and check that the Workflow Analyzer shows 0 errors.
7. Propose: the package version (the next free one), where to publish it (Personal Workspace or tenant feed, and why), the process name, and the job to run (Main.xaml with its default arguments).
Then stop and wait for my OK.
```

### P.2 Publish (after checking Part A)

```text
Before Part B, one check, read-only: list the existing versions of ExpenseClaimConverter in my Personal Workspace feed with their publish dates, and tell me whether a process with that name already exists in Personal Workspace and which package version it uses. Then continue with Part B only if no existing process would be changed; otherwise stop and tell me.

OK for Part B with these changes. This message is my OK to publish this one package version.
1. Pack and publish version <next free version> to my Personal Workspace feed. Show the package name, version and destination.
2. Check whether the upload created or updated a process automatically. Create the process only if none exists. Do not change other processes.
3. Start one job of Main.xaml with its default arguments on the available Development runtime. If the job is refused, or stays Pending for more than 2 minutes, stop the job so nothing is left in the queue, and report it. Then stop: I will run the process myself from UiPath Assistant. Do not try other robots, machines, licences or folder settings.
4. If the job runs: show the job state, the job logs (start, the Warning summary, the end line with counts and total) and the total. Expected: 5 converted, 3 failed, TOTAL 919.91 EUR.
5. If any step fails, stop at that step. Show the exact error, use uipath-troubleshoot to explain the cause, and propose a fix. Accept permission errors (403) as a known limitation; do not try to work around them.
6. Finish with a table: step, command or action, result, and time taken.
```
