# User Interface

LevelUp.Web is a Blazor Server application organized by feature under `Components/Features` and by reusable primitives under `Components/DesignSystem`.

## Feature Areas

- Account
- Authentication and identity
- Character creation and onboarding
- Dashboard
- Habits
- Recurring tasks
- Todos
- Projects
- Inventory

## Design Principles

- Pixel-art-inspired visual language
- Reusable components for buttons, cards, forms, icons, feedback, modals, and text
- Consistent loading, empty, error, and disabled states
- Responsive layouts
- Accessible labels and keyboard behavior
- Feature state isolated from persistence details

## Typography

- **Jersey 15**: branding, primary headings, card titles, large numeric indicators, and decorative RPG elements
- **Jersey 25**: buttons, inputs, labels, body copy, menus, tables, dialogs, messages, navigation, and actions

### Canonical Scale

| Token | Size | Intended use |
|---|---:|---|
| `--levelup-font-size-xs` | `.75rem` | Metadata and compact supporting text |
| `--levelup-font-size-sm` | `.85rem` | Labels and secondary text |
| `--levelup-font-size-md` | `.95rem` | Regular interface text |
| `--levelup-font-size-base` | `1rem` | Body text |
| `--levelup-font-size-lg` | `1.125rem` | Emphasized text |
| `--levelup-font-size-xl` | `1.5rem` | Card and section headings |
| `--levelup-font-size-2xl` | `1.8rem` | Page headings |
| `--levelup-font-size-3xl` | `2.2rem` | Display headings |

Textual controls should not be smaller than `.75rem`.

## Integration

The presentation layer accesses application behavior through registered services and MediatR handlers. JSON persistence remains an Infrastructure responsibility.

## Run

```bash
dotnet run --project src/LevelUp.Web/LevelUp.Web.csproj
```
