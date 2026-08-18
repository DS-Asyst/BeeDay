---
name: beeday-review
description: Mandatory final BeeDay two-pass review and quality verdict. Use on the final diff after implementation and validation, and when reviewing a PR or proposed change.
---

# beeday Final Review

Review the **final diff**, not an earlier intermediate state.

## Pass 1 — Implementation correctness

Inspect every changed file for applicable concerns:

- requested behavior is complete;
- invariants are preserved;
- validation is correct;
- nullability is safe;
- cancellation is propagated where required;
- errors are handled through established contracts;
- authentication/authorization/ownership boundaries are preserved;
- configuration has safe defaults and failure behavior;
- UI accessibility/localization/responsiveness are preserved;
- tests prove behavior rather than private implementation details.

Record findings as `BLOCKER`, `MAJOR`, or `MINOR`.

## Pass 2 — Repository integration

Inspect the complete final diff for:

- Clean Architecture violations;
- duplicated services/components/contracts;
- unnecessary abstractions;
- public contract drift;
- backward-compatibility regressions;
- missing test coverage;
- documentation drift;
- secret exposure;
- generated or binary noise;
- accidental line-ending/formatting changes;
- unrelated files;
- infrastructure/deployment drift;
- rollback or idempotency regressions when relevant.

## Severity

### BLOCKER

Must be fixed before delivery. Examples: security issue, change-caused failing mandatory validation, architecture violation, data-loss risk, incomplete required behavior, unapproved breaking contract.

### MAJOR

Important non-trivial deficiency. Normally fix before delivery unless clearly outside scope and safe to defer with explicit reporting.

### MINOR

Non-blocking improvement that may remain when documented.

## Evaluation matrix

Produce an internal/final matrix for every applicable row:

| Dimension | PASS / FAIL / N/A | Evidence or finding |
|---|---|---|
| Correctness | | |
| Scope | | |
| Architecture | | |
| Reuse | | |
| Security | | |
| Tests | | |
| UI/UX | | |
| Maintainability | | |
| Documentation | | |
| Operations | | |
| Git hygiene | | |

## Final verdict

Use exactly one:

- `PASS` — all applicable gates passed and no blocking findings remain.
- `PASS WITH FINDINGS` — safe to deliver with explicitly documented non-blocking findings.
- `FAIL` — at least one BLOCKER remains or mandatory evidence is incomplete.

Any unresolved BLOCKER forces `FAIL`.
