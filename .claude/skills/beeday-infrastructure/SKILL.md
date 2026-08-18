---
name: beeday-infrastructure
description: BeeDay infrastructure workflow for GitHub Actions, CI/CD, deployment scripts, IIS, PowerShell, filesystem permissions, certificates, environment configuration, privileged operations, and HMG/production operational boundaries.
---

# beeday Infrastructure and Operations

Infrastructure work has a higher evidence and safety burden than normal code changes.

## 1. Establish state explicitly

Before proposing operational changes, distinguish:

- **Repository State** — what exists in Git;
- **Installed / Promoted State** — what has actually been provisioned/copied into an environment;
- **Runtime State** — what is actually running and observed now.

Never infer runtime state from repository files alone.

Verify current environment availability from repository documentation and direct environment evidence when access is authorized.

## 2. Preserve privileged boundaries

Maintain:

- least privilege;
- restricted service identities;
- explicit allow-lists;
- request/result correlation;
- secret sanitization;
- rollback capability;
- idempotency;
- separation between normal deployment and privileged control;
- traceability from installed scripts/configuration back to Git source of truth.

Do not solve permission problems by making a restricted identity an administrator.

Do not allow a file writable by an unprivileged runner to become arbitrary code executed under a privileged identity without a verified validation/promotion boundary.

## 3. Deployment changes

For CI/CD or deployment changes:

- inspect existing workflows and scripts first;
- preserve Build Once / Deploy Many or other repository-defined artifact provenance contracts;
- preserve fail-closed behavior;
- preserve rollback behavior;
- preserve environment separation;
- keep secrets out of logs and metadata;
- avoid manual server drift when an automated repository mechanism exists.

Do not claim environment deployment success from local script parsing or unit tests.

## 4. PowerShell

For modified PowerShell:

- parse syntax using the PowerShell AST/parser or repository-supported check;
- preserve Windows PowerShell / PowerShell version compatibility documented by the repository;
- avoid quoting/path behavior that only works in one shell when workflows use another;
- preserve exit-code propagation;
- preserve idempotency and error classification.

## 5. GitHub Actions / YAML

For workflow changes:

- validate YAML syntax;
- inspect triggers, permissions, environments, concurrency, artifact provenance, secrets usage, and failure behavior;
- do not broaden token permissions unnecessarily;
- verify shell differences explicitly;
- avoid silently changing required check names or promotion contracts.

## 6. IIS / server operations

Repository code can be Code Complete without being Environment Validated.

Do not claim:

- IIS state was validated if IIS was not queried/exercised;
- a Scheduled Task works if it was not actually run;
- certificate binding works if it was not inspected in the target environment;
- deployment succeeded if the deployment did not execute;
- rollback succeeded if rollback was not exercised or directly verified.

## 7. Production boundary

Production/environment mutation is Class E.

Even Sprint/Epic autonomy does not authorize:

- production deployment;
- production restart;
- production ACL changes;
- production certificate replacement;
- secret rotation;
- destructive database action;
- destructive external-resource mutation.

Require explicit authorization for the specific operation.

## 8. Infrastructure final report

Include:

- repository state changed;
- installed/runtime state actually verified;
- local validation performed;
- real-environment validation performed;
- rollback status;
- residual operational risks;
- manual bootstrap requirements, if any;
- whether the result is **Code Complete** and/or **Environment Validated**.
