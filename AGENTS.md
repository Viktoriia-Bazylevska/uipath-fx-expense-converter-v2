# Project conventions: ExpenseClaimConverter

- Use the `uipath-rpa` skill for automation work and the `uipath-review` skill for reviews.
- Read `docs/solution-design.md` before every task. It is the source of truth for components, contracts, rules, conventions and test cases. If a prompt and the design disagree, ask.
- Windows project, C# expressions.
- Add an annotation to every activity (including Sequence, Try Catch, Retry Scope, Assign, If, Invoke Workflow and Log Message), every argument and every workflow: 1–2 lines on what it does and why.
- Run Workflow Analyzer and fix every error and warning (ST-USG-034 may stay) before saying a task is done. Then run the workflow and show the result.
- Packages: add only those listed in `docs/solution-design.md` section 2, plus what Studio needs for coded workflows and test cases (such as `UiPath.CodedWorkflows`), and pin every version in `project.json`. Ask before adding or upgrading any other package. The developer installs FX.Rates from the tenant feed.
- Do not publish. The developer does this in Studio.
- Run Git commands only when the developer asks in a prompt. Before any commit, check the staged files; if the prompt does not already give the OK, show `git status` and wait for it. Never commit `Data/Output/` files, `.nupkg` packages or credentials. Use the exact commit message given, with no co-author or other trailer lines. Never force-push, and never create or change a remote unless asked. If a Git command fails, stop and show the error.
- Do not change files in `docs/`, `Data/Input/` or `AGENTS.md` unless asked.
- Mark any temporary or test-only value with `TODO remove`.
