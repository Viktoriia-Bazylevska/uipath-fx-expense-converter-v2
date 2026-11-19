# Project conventions: ExpenseClaimConverter

- Use the `uipath-rpa` skill for automation work and the `uipath-review` skill for reviews.
- Read `docs/solution-design.md` before every task. It is the source of truth for components, contracts, rules, conventions and test cases. If a prompt and the design disagree, ask.
- Windows project, C# expressions. Pin package versions in `project.json`; do not add or upgrade packages without asking.
- Add an annotation to every activity (including Sequence, Try Catch, Retry Scope, Assign, If, Invoke Workflow and Log Message), every argument and every workflow: 1–2 lines on what it does and why.
- Run Workflow Analyzer and fix every error and warning (ST-USG-034 may stay) before saying a task is done. Then run the workflow and show the result.
- Do not publish or install packages. The developer does these in Studio.
- Run Git commands only when the developer asks in a prompt. Before any commit, show `git status` and the files to be committed, and wait for the developer's OK. Use the exact commit message given, with no co-author or other trailer lines. Never force-push, and never create or change a remote unless asked. If a Git command fails, stop and show the error.
- Do not change files in `docs/`, `Data/Input/` or `AGENTS.md` unless asked.
- Mark any temporary or test-only value with `TODO remove`.
