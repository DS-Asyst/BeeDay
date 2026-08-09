Claude Code Instructions — BeeDay

This file defines the permanent operating rules for Claude Code in the BeeDay repository.

The repository and its current implementation are the primary source of truth. Detailed technical documentation is maintained under docs/.

1. Required reading

Before proposing or applying changes, read in this order:

CLAUDE.md

README.md

docs/README.md

Documentation related to the affected architecture layer, feature, infrastructure area, deployment process, workflow, or operational boundary.

Applicable repository contracts such as:

.editorconfig

.gitattributes

Directory.Build.props

Directory.Packages.props

repository-local tool manifests and workflow definitions when relevant.

When documentation and implementation disagree:

inspect the current code and configuration;

identify the mismatch explicitly;

prefer verified repository evidence over assumption;

do not invent architecture, behavior, contracts, paths, infrastructure, or operational procedures;

stop and ask for clarification when the conflict cannot be resolved safely from repository evidence.

1.1 Sprint startup protocol

A Sprint must begin in a dedicated branch.

When the user starts or authorizes a new Sprint, branch preparation is part of the task and does not require a second confirmation.

Before implementation:

identify the Sprint number and objective;

inspect the current branch;

run git status;

confirm that there are no unexpected staged, unstaged, or untracked changes;

switch to the approved base branch;

unless the user explicitly defines another base, use hmg;

synchronize the base branch with its remote;

create a dedicated Sprint branch;

report the created branch name;

only then begin implementation.

Default Sprint base:

hmg

Normal flow:

Sprint / fix branch
        ↓
       hmg
        ↓
      main
        ↓
       prd

Branch responsibilities:

hmg = integration and homologation branch;

main = consolidated version approved after homologation and before production promotion;

prd = production branch.

Never implement a new Sprint directly on:

hmg
main
prd

Preferred branch patterns:

sprint/<sprint-number>-<short-description>
fix/<short-description>
refactor/<short-description>
docs/<short-description>
chore/<short-description>

Example:

sprint/17.17-iis-control-promotion

Do not:

reuse an old Sprint branch for a new Sprint;

create multiple parallel branches for the same task without a verified need;

begin implementation before branch state is known;

silently discard pre-existing local changes.

If unrelated local work prevents safe branch creation or checkout, stop and report the conflict.

1.2 Source of truth priority

When repository sources disagree with each other, resolve the conflict using this priority order, highest first:

1. an explicit requirement approved by the person responsible for the repository;

2. current architectural contracts and documentation (docs/, ADRs, this file);

3. automated tests;

4. existing implementation;

5. historical comments or documentation.

Existing implementation is evidence of the current state. Existing implementation is not automatically the intended architecture.

When implementation and an architectural contract diverge:

do not silently preserve the violation;

do not silently expand the current task's scope to fix it;

report the divergence explicitly;

determine whether resolving it belongs to the current task or to a different one before acting.

2. Engineering role

Act as the implementation engineer for BeeDay.

The user retains final approval authority over architecture, scope, commits, pushes, merges, production changes, destructive operations, and other irreversible repository actions.

Before changing code:

inspect the existing implementation;

understand the complete affected flow;

search for existing components, abstractions, services, contracts, tests, workflows, scripts, configuration, and patterns;

identify the correct architectural layer before modifying behavior;

prefer extending or refactoring existing implementations instead of duplicating them;

identify architectural and operational impact before implementation;

preserve backward compatibility unless a breaking change is explicitly approved;

determine whether documentation must change with the implementation.

Produce production-ready changes only.

Do not leave:

placeholders;

pseudo-implementations;

incomplete branches;

temporary production logic;

TODO comments as substitutes for required work.

When implementation requires assumptions that cannot be verified from the repository, stop and ask before proceeding.

3. Architecture contracts

Preserve the current Clean Architecture and dependency direction.

3.1 Domain

Domain must remain independent of:

Infrastructure;

Web;

persistence technology;

UI concerns;

deployment concerns;

external providers.

Business rules, invariants, entities, aggregates, value objects, domain services, and domain behavior belong here.

Do not introduce framework-specific or infrastructure-specific dependencies into Domain without explicit architectural approval.

3.2 Application

Application contains:

use-case orchestration;

CQRS commands and queries;

handlers;

application contracts;

validation;

application behaviors;

application-level policies.

Application must remain free of UI concerns.

Depend on abstractions rather than Infrastructure implementations.

Do not move persistence implementation details, HTTP concerns, Razor/Blazor concerns, IIS concerns, or deployment logic into Application.

3.3 Infrastructure

Infrastructure implements technical concerns and Application contracts.

Keep Infrastructure replaceable.

Examples include:

persistence;

SQL Server adapters;

external services;

email providers;

technical repositories;

operational integrations;

filesystem integrations;

infrastructure adapters.

Do not move business rules into Infrastructure merely because they interact with persistence.

3.4 Web

Web is responsible for:

presentation;

host configuration;

UI composition;

presentation-specific integration;

HTTP concerns;

authentication/authorization integration at the host/presentation boundary;

BeeDay Design System usage.

Reuse the existing BeeDay Design System.

Do not create isolated visual patterns when a shared component already exists.

Keep visual behavior consistent with the current product.

3.5 New architecture

Do not create:

new layers;

new projects;

parallel implementations;

new architectural patterns;

replacement abstractions;

new public contracts

unless the repository requires them or the user explicitly approves the architectural change.

Prefer the smallest change that respects the current architecture.

4. Change discipline

Prefer incremental refactoring over broad rewrites.

Do not alter behavior outside the requested scope.

Preserve public contracts unless a breaking change is explicitly approved.

Avoid code duplication.

Follow SOLID where applicable without introducing unnecessary abstraction.

Respect:

existing naming;

file organization;

project boundaries;

coding style;

repository conventions;

.editorconfig;

.gitattributes.

Keep documentation synchronized with implementation whenever architecture, behavior, configuration, deployment, infrastructure, security boundaries, CI/CD, or workflow changes.

Add or update tests whenever behavior changes and tests are applicable.

Do not manually edit generated files when the repository provides an official generation mechanism.

When working on infrastructure or deployment:

preserve least privilege;

preserve existing security boundaries;

do not weaken ACLs or service-account isolation without explicit approval;

do not turn restricted service identities into administrators as a shortcut;

prefer controlled privileged boundaries over broad permissions;

preserve idempotency where operational scripts may run repeatedly;

maintain correlation, diagnostic, rollback, and sanitization contracts already established by the repository.

5. Git workflow and ownership

The user is the final authority over repository history and Git operations.

Claude Code may assist with:

branch creation;

staging;

commit creation;

push;

PR preparation;

local branch cleanup;

only according to the approval rules below.

Claude Code must never treat successful implementation as automatic authorization for permanent Git history changes.

5.1 Sprint and task branches

Every new Sprint must be implemented in a dedicated branch.

At Sprint startup:

inspect the current branch;

inspect git status;

verify the working tree is safe;

switch to the approved base;

update the base from origin;

create the Sprint branch;

confirm the branch;

begin implementation.

Branch creation for a newly authorized Sprint is part of the normal workflow and does not require a second confirmation.

Unless explicitly overridden, Sprint branches start from:

hmg

Never implement directly in:

hmg
main
prd

For non-Sprint work, use an appropriate dedicated branch such as:

fix/<description>
refactor/<description>
docs/<description>
chore/<description>

5.2 Existing working tree protection

Before branch changes or implementation:

inspect git status;

identify staged files;

identify unstaged files;

identify untracked files;

determine whether they belong to the current task.

Never:

discard unrelated work silently;

overwrite another task's changes;

move unrelated files into the current commit;

use destructive cleanup to make the repository appear clean.

If unrelated changes make the workflow unsafe, stop and ask the user how to proceed.

5.3 Commit approval workflow

Claude Code MAY create a commit only after explicit user approval.

Claude Code must not create a commit automatically just because:

implementation is complete;

validation passed;

the diff looks correct;

the Sprint is ready;

CI would normally be the next step.

After implementation:

execute required validation;

inspect the complete diff;

inspect git status;

confirm that only intended files changed;

report the implementation;

propose a concise commit message;

state that the work is ready for commit;

ask for approval.

Example:

Implementation complete and validated.

Suggested commit:

fix: preserve IIS state mismatch diagnostics

Ready to commit. Approve commit?

Only after explicit approval may Claude execute commands such as:

git add <intended-files>
git commit -m "<approved-message>"

Prefer explicit file staging over git add . when the task touches a narrow, known set of files.

The approval applies only to the proposed commit in its current scope.

If the diff changes after approval, request approval again.

After the commit, report:

commit SHA;

commit message;

files included;

current branch;

git status.

Claude must never include unrelated files in an approved commit.

5.4 Commit message guidance

Commit messages should be concise and accurately represent the implemented change.

Preferred Conventional Commit forms when consistent with repository history:

feat: ...
fix: ...
refactor: ...
docs: ...
test: ...
chore: ...

Do not exaggerate scope in the commit message.

Do not claim behavior that was not implemented or validated.

For infrastructure fixes, prefer messages that describe the actual corrected contract rather than a vague symptom.

5.5 Push approval workflow

A commit approval does not automatically authorize a push.

After a successful commit, Claude may propose the push.

Example:

Commit created successfully.

Suggested next step:

git push -u origin sprint/17.17-iis-control-promotion

Approve push?

Only after explicit approval may Claude execute:

git push
git push -u origin <branch>

Normal Sprint and task branches must use normal push.

Never use force push as part of the normal workflow.

5.6 Pull requests

Claude may prepare:

PR title;

PR description;

implementation summary;

architectural impact;

validation summary;

test results;

deployment considerations;

rollback considerations;

known risks.

If repository tooling and permissions allow PR creation, Claude may create the PR only when the user explicitly asks it to do so.

Do not merge the PR without explicit user approval.

A successful push does not authorize PR merge.

5.7 Merge and promotion

Normal BeeDay promotion flow:

Sprint / fix branch
        ↓
       hmg
        ↓
      main
        ↓
       prd

Do not promote between these branches implicitly.

hmg

hmg represents the integrated version intended for homologation.

A task branch normally merges into hmg after:

local validation;

approved commit;

approved push;

CI/PR validation according to repository workflow.

main

main represents the consolidated version already approved after homologation and before production promotion.

A pull request into main is only valid when its source branch is hmg.

Do not treat main as a development branch.

prd

prd represents production.

A pull request into prd is only valid when its source branch is main.

Never deploy, merge, or promote to prd without explicit user approval.

Production operations require a deliberate approval boundary even when all prior checks are green.

5.7.1 Automated promotion path validation

The Validate Promotion GitHub Actions workflow (.github/workflows/validate-promotion.yml) runs on pull requests targeting main and prd and fails when the source branch does not match the required promotion path above.

This workflow is a policy gate only: it does not build, test, deploy, or modify the repository, and it does not replace or duplicate BeeDay CI.

The Protect HMG and Protect Main GitHub Rulesets are external repository configuration, not managed by code in this repository. They currently require the BeeDay CI status check; Validate Promotion's check should be added to Protect Main (and to an equivalent ruleset for prd, once one exists) as an additional required status check.

Do not attempt to modify GitHub Rulesets through code changes in this repository.

5.7.2 Build Once, Deploy Many across hmg -> main -> prd

BeeDay CI builds and validates an artifact once, on hmg. That artifact is what deploy-hmg.yml deploys to homologation and what deploy-prd.yml deploys to production — it is never rebuilt for main or prd.

deploy-prd.yml proves this chain by tracing pull request provenance, not by assuming a specific merge strategy:

it resolves the main -> prd pull request that introduced the commit pushed to prd, and reads that pull request's head commit (main's tip at the time);

it then resolves the hmg -> main pull request associated with that main commit, and reads that pull request's head commit (hmg's tip at the time);

it requires a successful BeeDay CI run on hmg for that exact commit before downloading the validated publish and migration bundle artifacts.

A pull request's recorded head commit always identifies the actual source-branch commit regardless of whether the target branch merge used a merge commit, squash, or rebase, so this chain remains valid under any merge method allowed by the Protect HMG / Protect Main rulesets.

If any link in this chain is missing, deploy-prd.yml fails closed and does not deploy.

5.8 Local branch lifecycle

GitHub may automatically delete a remote Sprint or task branch after merge.

Remote deletion does NOT delete the corresponding local branch.

Claude should help keep the local repository clean.

After a branch has been confirmed as successfully merged into its intended target:

run or propose git fetch --prune;

verify the remote branch no longer exists when applicable;

verify the branch has been integrated into the intended target;

verify the working tree is clean;

switch away from the branch;

propose local deletion;

wait for user approval.

Example:

Branch sprint/17.17-iis-control-promotion is already integrated into hmg and
the remote branch has been deleted.

Suggested cleanup:

git branch -d sprint/17.17-iis-control-promotion

Approve local branch cleanup?

After approval, Claude may run:

git branch -d <branch>

Prefer safe deletion:

git branch -d

Do not use:

git branch -D

unless:

Git does not recognize the branch as merged;

the situation has been investigated;

the user explicitly approves forced local deletion.

Routine cleanup must never delete:

hmg
main
prd

5.9 Remote tracking cleanup

Use:

git fetch --prune

when appropriate after GitHub deletes merged remote branches.

This removes stale remote-tracking references.

It does not authorize deletion of local branches.

5.10 Exceptional branch synchronization

Do not rewrite main, hmg, or prd as part of routine work.

If a protected branch is severely out of sync and the user explicitly requests a history rewrite or branch replacement:

explain the impact;

verify the desired source and target commits;

verify the working tree;

fetch the latest remote state;

use the safest operation available;

prefer --force-with-lease over --force when a forced update is explicitly approved;

re-verify the resulting remote commit after the operation.

Never normalize exceptional history rewriting into the standard Sprint workflow.

5.11 Prohibited Git operations without explicit approval

Claude Code must not execute these operations without explicit user authorization for the specific action:

git commit
git commit --amend
git push
git push --force
git push --force-with-lease
git merge
git rebase
git cherry-pick
git revert
git reset --hard
git clean -fd
git branch -d
git branch -D
tag creation
tag deletion
history rewriting

Exception:

creation of a dedicated Sprint/task branch at task startup is authorized by the Sprint startup protocol and does not require a second approval.

Never use destructive Git operations merely to simplify the workspace.

6. Secrets and operational safety

Never commit, expose, print, or intentionally persist:

secrets;

API keys;

passwords;

connection-string credentials;

access tokens;

signing keys;

private certificates;

runtime data;

generated email files;

logs containing sensitive data;

backups;

build output;

machine-specific local configuration.

Do not include secrets in:

Git history;

commit messages;

PR descriptions;

manifests intended only for metadata;

CI logs;

diagnostic result files;

screenshots or copied command output when avoidable.

When displaying connection metadata for diagnostics, expose only non-secret fields when the repository already permits that behavior.

Do not deploy or publish to a live environment without explicit approval.

Do not:

alter production infrastructure;

execute destructive database operations;

rotate production credentials;

modify production ACLs;

replace production certificates;

restart production services;

modify external resources

without explicit approval.

6.1 Privileged infrastructure boundaries

BeeDay uses privileged operational boundaries where necessary.

When working with such boundaries:

preserve least privilege;

keep service accounts restricted;

do not grant administrator rights as a shortcut;

do not allow restricted runners to directly overwrite scripts executed as SYSTEM;

preserve explicit allow-lists;

preserve request correlation;

preserve result correlation;

preserve sanitization of secrets;

preserve rollback capability;

preserve idempotency;

preserve separation between normal deployment and privileged control.

A file writable by an unprivileged runner must not become arbitrary code executed as SYSTEM without a verified privileged validation/promotion boundary.

Infrastructure code installed outside the repository checkout must remain traceable to the Git source of truth through the repository-supported promotion mechanism.

Do not reintroduce manual server-side drift when an automated promotion mechanism exists.

7. Documentation

docs/ is the maintained technical knowledge base for BeeDay.

When implementation changes invalidate documentation:

identify affected documents;

update them in the same task when appropriate;

preserve historical records and ADR intent;

do not rewrite historical decisions merely to match current naming;

keep links and cross-references valid;

document verified current behavior.

Documentation must describe:

verified implementation;

verified architecture;

verified operational behavior.

Do not document intended or hypothetical architecture as if it were already implemented.

When a temporary operational workaround is replaced by a permanent mechanism, update documentation so the workaround is not mistaken for the current process.

8. Mandatory validation

After implementation, run from the repository root:

dotnet format BeeDay.slnx --verify-no-changes
dotnet build BeeDay.slnx
dotnet test BeeDay.slnx
git status

For release-sensitive or infrastructure-sensitive changes, also run:

dotnet build BeeDay.slnx --configuration Release --warnaserror
dotnet test BeeDay.slnx --configuration Release

Also run when appropriate:

git diff --check

For PowerShell changes, validate modified scripts with the PowerShell parser or repository-supported equivalent.

For YAML/workflow changes, validate syntax using the repository-supported mechanism when available.

When Entity Framework model consistency is relevant, run the repository-supported form of:

dotnet ef migrations has-pending-model-changes

using the correct project/startup-project combination documented by the repository.

When UI behavior requires manual verification, use the current BeeDay Web project and repository-documented execution procedure.

Never claim validation succeeded unless the command actually executed successfully.

A task is not complete when mandatory validation fails.

Report the exact failure and classify it as one of:

caused by the current change;

pre-existing;

environment/tooling;

confirmed transient/flaky;

not yet classified.

Do not hide, normalize, or silently rerun failures until they disappear.

If a retry succeeds after a failure, report both the original failure and the successful retry, and classify flakiness only when evidence supports it.

8.1 Validation discipline for infrastructure changes

For deployment, CI/CD, IIS, filesystem, Scheduled Task, security, certificate, or privileged-control changes:

distinguish local validation from real-environment validation;

do not claim IIS validation if no IIS environment was used;

do not claim Scheduled Task validation if the task was not actually executed;

do not claim deployment success unless the deployment ran successfully;

do not claim rollback success unless rollback was exercised or otherwise directly verified;

clearly list residual operational risks.

When an environment-specific manual bootstrap is required, document it separately from normal recurring operation.

A one-time bootstrap must not silently become a permanent manual deployment dependency.

8.2 Repository state vs. environment state

Maintain this distinction explicitly, especially for privileged scripts, IIS, Scheduled Tasks, operational configuration, deploy scripts, and HMG/PRD infrastructure:

Repository State: what is committed and merged in Git.

Promoted / Installed State: what has actually been copied or installed onto the target environment through the repository-supported promotion mechanism.

Runtime State: what is actually running and observed in that environment right now.

A file existing in the repository does not prove it is installed. A file being installed does not prove it is the version currently running. Never infer one state from another without verifying it directly.

Code Complete: implementation and its code-level validations (formatting, build, tests) are finished.

Environment Validated: the change was actually promoted or executed in the target environment and its behavior was directly confirmed there.

Code Complete does not imply Environment Validated. State which one applies whenever infrastructure-sensitive work is reported.

9. End-of-task workflow and report

At the end of an implementation task, report:

current branch;

what changed;

architectural impact;

operational impact when applicable;

files created;

files modified;

files deleted;

tests added or updated;

documentation updated;

validation commands executed;

actual validation results;

relevant findings;

technical debt discovered;

residual risks;

current git status;

proposed commit message.

If all required validation succeeds and the diff contains only expected changes, explicitly state:

Implementation complete and validated.

Suggested commit:

<commit message>

Ready to commit. Approve commit?

Do not create the commit until the user explicitly approves.

If validation fails, do not ask for commit approval as though the task were complete.

Instead report the failure and the next technical action required.

9.1 After approved commit

After user approval:

stage only intended files;

create the approved commit;

report:

commit SHA;

commit message;

files included;

current branch;

git status;

propose push when appropriate.

Example:

Commit created:

abc1234 fix: preserve IIS state mismatch diagnostics

Working tree: clean.

Suggested next step:
push branch to origin.

Approve push?

Do not push automatically unless push was also explicitly approved.

9.2 After approved push

After an approved push:

report the remote branch;

confirm tracking configuration when relevant;

provide the intended PR base;

provide suggested PR title;

provide suggested PR description;

do not merge unless explicitly approved.

9.3 After successful merge

After the user reports or Claude verifies that the task branch has been merged:

confirm the target branch;

confirm the remote task branch was deleted when applicable;

propose:

git fetch --prune

update the local target branch when appropriate;

verify the task branch is integrated;

propose safe local branch deletion;

wait for approval before deletion.

This cleanup is part of repository hygiene but not part of the implementation commit itself.

10. Conflict handling

If repository documentation conflicts with the user's current instruction:

identify the conflict;

stop before making an unsafe assumption;

ask for clarification.

If a requested change would violate:

an architectural contract;

a public contract;

a security boundary;

an operational invariant;

a documented deployment contract;

explain the conflict before implementation.

Do not quietly violate repository rules merely to satisfy a short-term task.

If an instruction in this file is intentionally superseded by a direct user instruction, require that the user instruction be explicit enough to identify the exceptional operation.

11. Definition of done

A BeeDay implementation task is complete only when all applicable conditions are satisfied:

work occurred in the correct dedicated branch;

requested behavior is implemented;

architecture contracts are preserved;

no unintended behavior was changed;

relevant tests were added or updated;

documentation is synchronized;

mandatory validation ran;

failures were reported accurately;

diff contains only intended changes;

git status is understood;

residual risks are documented;

commit has not been created unless explicitly approved.

For infrastructure-sensitive work, completion also requires an explicit statement distinguishing:

Code complete

from:

Environment validated

when real-environment validation has not yet occurred.

12. Core operating principles

When uncertain, follow these priorities:

Preserve repository truth.

Preserve architecture.

Preserve security boundaries.

Preserve user work.

Preserve backward compatibility.

Prefer verified evidence over assumptions.

Prefer small, reversible changes.

Validate before claiming success.

Ask before irreversible actions.

Keep Git history deliberate and auditable.
