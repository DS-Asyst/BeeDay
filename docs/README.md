# LevelUp Documentation

This directory is the maintained source of truth for the current LevelUp implementation. Historical sprint notes are intentionally excluded; Git preserves implementation history.

## Sections

### [Architecture](architecture/README.md)

System boundaries, project responsibilities, dependency rules, persistence, authentication, and production hosting.

### [Development](development/README.md)

Local setup, coding standards, testing, Git workflow, configuration, CI, and deployment operations.

### [Design System](design-system/README.md)

Visual foundations, shared components, typography, icons, accessibility, and interaction rules.

### [Domain](domain/README.md)

Business concepts and invariants for users, characters, activities, projects, inventory, and experience progression.

### [AI collaboration](ai/README.md)

Shared context and operating rules for ChatGPT, Claude Code, and future coding agents.

## Documentation policy

- Describe the application as it exists now.
- Update documentation in the same change as affected behavior.
- Avoid duplicating the same rule in multiple files; link to the owning document.
- Use English for repository documentation, technical identifiers, branches, and commits.
- Treat code and automated tests as the final authority when documentation conflicts with implementation; resolve the inconsistency before completing the task.
