# Coding Standards

Repository-level rules are defined by `.editorconfig`, `.gitattributes`, `Directory.Build.props`, and `Directory.Packages.props`.

## C# and .NET

- Target .NET 10.
- Keep nullable reference types enabled.
- Respect the latest configured analyzers and code-style enforcement.
- Prefer explicit, testable behavior over hidden side effects.
- Use domain-specific types and validated value objects where they improve invariants.
- Keep asynchronous I/O cancellable and propagate cancellation tokens.
- Do not duplicate package versions in individual projects; use central package management.

## Layering

- Domain contains business rules.
- Application contains use cases and contracts.
- Infrastructure contains technical implementations.
- Web contains HTTP and presentation concerns.

## Blazor and CSS

- Keep feature components under their feature directory.
- Reuse Design System components before creating new visual primitives.
- Keep component-specific styles in colocated `.razor.css` files when appropriate.
- Preserve keyboard interaction, focus visibility, semantic labels, and reduced-motion behavior.
- Do not embed functional SVG directly in feature components; use the Pixel Icon System.

## Files and line endings

`.gitattributes` standardizes repository text files as CRLF, with shell scripts kept as LF. Binary assets must remain marked binary. Review line-ending-only diffs before committing.

## Documentation

Documentation is written in English, describes current behavior, and is updated in the same change as the code it governs.
