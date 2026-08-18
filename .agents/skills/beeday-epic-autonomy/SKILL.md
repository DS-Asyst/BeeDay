---
name: beeday-epic-autonomy
description: "Execute an explicitly autonomous BeeDay Epic containing multiple Sprints. Use only when the user grants AUTONOMY: EPIC or unmistakably authorizes Codex to implement, validate, commit, push, and create PRs for all defined Sprints without waiting between them."
---

# BeeDay Autonomous Epic Execution

This Skill is intentionally high-autonomy but bounded.

It grants Class C delivery capability for authorized Sprint work. It does not authorize merge, destructive Git, history rewriting, or environment/production mutation.

## 1. Confirm Epic contract

Before Sprint 1, identify from the user's Epic package/prompt and repository evidence:

- Epic identifier and objective;
- Sprint sequence;
- each Sprint's scope and acceptance criteria;
- explicit exclusions;
- initial branch base;
- required validation;
- PR target policy;
- dependencies between Sprints.

Do not invent missing Sprint requirements.

If the Epic definition is sufficiently complete to execute safely, do not ask for repetitive approvals.

## 2. Default branch chaining

Unless the Epic explicitly specifies another strategy:

```text
approved initial base (normally hmg)
  └─ Sprint 1 local branch
      └─ Sprint 2 local branch
          └─ Sprint 3 local branch
              └─ ...
```

Rules:

1. Create Sprint 1 from the approved initial base.
2. After Sprint N is implemented, reviewed, validated, committed, pushed, and its PR is created, keep the completed local Sprint N branch.
3. Create Sprint N+1 from the **completed local Sprint N branch**.
4. Do not return to `hmg` to create Sprint N+1 unless the Epic explicitly requires that strategy.
5. Never reuse one branch for two Sprints.
6. Never combine two Sprint scopes into one commit merely because their branches are chained.
7. Report that PRs are stacked by branch ancestry and should be reviewed in Sprint order.

Branch ancestry does not change normal PR promotion policy. Unless the Epic or repository explicitly says otherwise, Sprint PRs still target the normal integration branch (`hmg`).

## 3. Per-Sprint execution loop

For each Sprint:

1. verify current branch and working tree;
2. verify previous Sprint delivery state;
3. create the next Sprint branch from the required local predecessor;
4. load/use relevant engineering, architecture, UI/UX, infrastructure Skills;
5. implement only the current Sprint;
6. add/update tests;
7. update required documentation;
8. run mandatory quality validation;
9. perform two-pass review;
10. resolve BLOCKER findings;
11. inspect final diff and Git status;
12. stage intended files;
13. commit;
14. push;
15. create PR;
16. record branch, SHA, PR, quality verdict, and validation result;
17. proceed immediately to the next Sprint unless a defined stop condition is reached.

Do not wait for the previous PR to be merged before creating the next chained local Sprint branch unless the Epic explicitly requires that wait.

## 4. No repetitive approval prompts

During active Epic autonomy, do not ask the user to approve:

- normal Sprint branch creation;
- Sprint commit;
- normal Sprint push;
- Sprint PR creation;
- continuation to the next defined Sprint.

Those actions are already authorized within the Epic boundary.

## 5. Actions still forbidden without specific approval

Epic autonomy does not authorize:

- PR merge;
- `git merge` for promotion;
- rebase;
- cherry-pick;
- reset/history replacement;
- force push;
- forced branch deletion;
- production deployment;
- production database mutation;
- production IIS/service mutation;
- ACL/certificate/secret changes;
- destructive external-resource operations.

## 6. Epic stop conditions

Stop and report when:

- requirements materially conflict;
- repository architecture/security contracts cannot be reconciled safely;
- unrelated working-tree changes create risk;
- Class D or E action becomes necessary;
- credentials/permissions required for an authorized Class C action are unavailable;
- a breaking public contract needs owner approval;
- repository evidence is insufficient to determine a safe next action.

Ordinary build/test/format/review failures caused by the current Sprint should be fixed autonomously rather than treated as owner blockers.

## 7. Epic final report

After the last Sprint, provide a compact ledger:

| Sprint | Branch | Commit | PR | Validation | Verdict |
|---|---|---|---|---|---|

Also report:

- final local branch;
- outstanding PR order/dependencies;
- residual risks;
- any environment validation still required;
- any deferred technical debt discovered during the Epic.

## 8. Codex approval handling

The BeeDay Codex configuration intentionally keeps the workspace sandbox instead of using broad full access. Protected `.git` writes and network delivery may therefore create escalation requests.

During `AUTONOMY: EPIC`, Auto-review may approve eligible Class C escalations because commit, normal push, PR creation, and chained Sprint branch preparation are already authorized by the Epic contract. It must not be treated as authorization for Class D or Class E operations.

If Auto-review denies an action, do not attempt policy circumvention. Use a materially safer path or stop when the denied action is required.
