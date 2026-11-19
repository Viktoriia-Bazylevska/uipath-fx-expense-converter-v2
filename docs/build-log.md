# Build log

Fill in as you go: clock time at the start and end of each step, every prompt you send (including fixes), and what you had to correct by hand. This log becomes the intervention log in `docs/findings.md`.

**Started:** ______  **Finished:** ______  **Model:** ______

| Step | Start | End | Prompts sent | What Autopilot did | Manual work or correction needed |
| --- | --- | --- | --- | --- | --- |
| 2–4 Repo, project, files, Git | | | 0 | — | |
| 5 GetExchangeRate: plan, build, 7 cases | | | | | |
| 6 Review gate | | | | | |
| 7 FX.Rates library (manual) | | | | | |
| 8 Coded workflow, Main, tests | | | | | |
| 9 Review, bug hunt, fixes | | | | | |
| 10 README, push | | | 0 | — | |
| **Total** | | | | | |

## Results (Step 11)

| Check | Expected | Actual |
| --- | --- | --- |
| Workflow Analyzer | 0 errors, 0 warnings except ST-USG-034 | |
| GetExchangeRate cases | 7 of 7 | |
| End-to-end run | 5 converted, 3 failed, TOTAL 919.91 EUR, 5 lookups | |
| Coded and coverage tests | All pass | |
| Annotations | Every activity annotated | |
| Review | No Critical or High | |
| Edge cases E1–E6 | All OK after fixes | |
| CS0436 warnings | 0 | |

## Notes

What worked well:

What still needed a person:
