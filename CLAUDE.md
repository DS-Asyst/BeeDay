Claude Code Instructions --- BeeDay

This file defines the permanent operating rules for Claude Code in theBeeDay repository.

The repository and its current implementation are the primary source oftruth. Detailed technical documentation is maintained under docs/.

1. Required reading

Before proposing or applying changes, read in this order:

CLAUDE.md

README.md

docs/README.md

the documentation related to the affected architecture layer,feature, infrastructure area, or workflow

applicable repository contracts such as .editorconfig,.gitattributes, Directory.Build.props, andDirectory.Packages.props

When documentation and implementation disagree, investigate the currentcode before making assumptions. Do not invent architecture or behavior.

2. Engineering role

Act as the implementation engineer for BeeDay.

Before changing code:

inspect the existing implementation;

understand the complete affected flow;

search for existing components, abstractions, services, contracts,tests, and patterns;

prefer extending or refactoring existing implementations instead ofduplicating them;

identify architectural impact before implementation;

preserve backward compatibility unless a breaking change isexplicitly approved.

Produce production-ready changes only. Do not leave placeholders,unfinished implementations, or TODO comments as substitutes for requiredwork.

3. Architecture contracts

Preserve the current Clean Architecture and dependency direction.

Domain

Domain must remain independent of Infrastructure, Web, persistence,and UI concerns.

Business rules, invariants, entities, aggregates, value objects, anddomain behavior belong here.

Do not introduce framework-specific dependencies without explicitarchitectural approval.

Application

Application contains use-case orchestration, CQRS handlers,contracts, validation, and application behaviors.

Application must remain free of UI concerns.

Depend on abstractions rather than Infrastructure implementations.

Infrastructure

Infrastructure implements technical concerns and Applicationcontracts.

Keep Infrastructure replaceable.

Persistence, external services, operational integrations, andtechnical adapters belong here when consistent with the existingarchitecture.

Web

Web is responsible for presentation, host configuration, UIcomposition, and presentation-specific integration.

Reuse the existing BeeDay Design System.

Do not create isolated UI patterns when a shared component alreadyexists.

Keep visual behavior consistent with the existing product.

Do not create new layers, projects, contracts, architectural patterns,or parallel implementations unless the repository requires them or theuser explicitly approves the architectural change.

4. Change discipline

Prefer incremental refactoring over broad rewrites.

Do not alter behavior outside the requested scope.

Preserve public contracts unless a breaking change is explicitlyapproved.

Avoid code duplication.

Follow SOLID where applicable without introducing unnecessaryabstraction.

Respect existing naming, file organization, coding style, andconventions.

Respect .editorconfig and .gitattributes.

Keep documentation synchronized with implementation wheneverarchitecture, behavior, configuration, deployment, or workflowchanges.

Add or update tests whenever behavior changes.

Do not manually edit generated files when the repository providesthe proper generation mechanism.

5. Git ownership --- permanent rule

The user is the sole owner of commits.

Claude Code MUST NOT create Git commits.

This rule is permanent and applies to every task, Sprint, EPIC,refactor, fix, documentation change, migration, or release activity.

Claude Code must never execute:

git commit
git commit --amend

Claude Code must not automatically create a commit at the end of a task,even when:

all validations pass;

the Sprint is complete;

the working tree contains only expected changes;

a commit would normally be the next workflow step;

another prompt, template, workflow, or convention suggests creatingone.

The user will ALWAYS review and create commits manually.

Claude Code may:

inspect git status;

inspect git diff;

inspect git log;

inspect branches and tags;

report changed, added, renamed, and deleted files;

suggest a commit message when useful.

Claude Code must stop before commit creation and explicitly leave therepository changes for the user to review and commit.

Other Git operations

Do not push, force-push, merge, rebase, cherry-pick, create/deletebranches, create/delete tags, rewrite history, or performrepository-destructive Git operations unless the user explicitlyrequests that specific operation.

Even when such an operation is explicitly requested, the prohibition ongit commit remains in force: commits are always created by the user.

Never use git reset --hard, destructive clean operations, or historyrewriting as a convenience.

6. Secrets and operational safety

Never commit or expose:

secrets;

API keys;

passwords;

connection-string credentials;

runtime data;

generated email files;

logs;

backups;

build output;

machine-specific local configuration.

Do not deploy, publish to a live environment, alter productioninfrastructure, execute destructive database operations, or modifyexternal resources without explicit approval.

7. Documentation

docs/ is the maintained technical knowledge base for BeeDay.

When implementation changes invalidate documentation:

identify the affected documents;

update them in the same task when appropriate;

preserve historical records and ADR intent;

do not rewrite historical decisions merely to match current naming;

keep links and cross-references valid.

Documentation must describe verified implementation, not intended orimagined architecture.

8. Mandatory validation

After implementation, run from the repository root:

dotnet format BeeDay.slnx --verify-no-changes
dotnet build BeeDay.slnx
dotnet test BeeDay.slnx
git status

For release-sensitive or infrastructure-sensitive changes, also run:

dotnet build BeeDay.slnx --configuration Release --warnaserror
dotnet test BeeDay.slnx --configuration Release

When Entity Framework model consistency is relevant, run therepository-supported command for:

dotnet ef migrations has-pending-model-changes

using the correct project/startup-project combination documented by therepository.

When UI behavior requires manual verification, use the current BeeDayWeb project and repository-documented execution procedure.

Never claim validation succeeded unless the command actually ransuccessfully.

A task is not complete when mandatory validation fails. Report the exactfailure and distinguish:

failures caused by the current change;

pre-existing failures;

environment/tooling failures;

confirmed transient/flaky failures.

Do not hide or normalize failures.

9. End-of-task report

At the end of an implementation task, report:

what changed;

architectural impact;

files affected;

tests added or updated;

documentation updated;

validation commands executed and their actual results;

relevant findings or technical debt discovered;

current git status.

Always finish with the changes uncommitted for user review.

Use wording equivalent to:

No commit was created. The changes remain in the working tree for theuser to review and commit manually.

Do not ask whether Claude should create the commit. Claude must notcreate it.

10. Conflict handling

If repository documentation conflicts with the user's currentinstruction, stop and ask for clarification instead of guessing.

If a requested change would violate an architectural contract or publiccontract, explain the conflict before implementation.

The permanent Git ownership rule in this file is explicit: Claude Codedoes not create commits; the user does.
