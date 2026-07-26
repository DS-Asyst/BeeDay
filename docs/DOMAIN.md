# Domain

## Core Entities

- `User`
- `Character`
- `Habit`
- `RecurringTask`
- `Project`
- `Todo`
- `Wallet`
- `Transaction`
- `InventoryTag`
- `UserToken`

## Value Objects

The domain uses value objects for validated concepts such as email addresses, user names, character nicknames, activity titles, activity descriptions, and project colors.

## Rules

- Domain entities enforce their own invariants.
- Invalid state transitions raise domain exceptions.
- Domain events communicate relevant state changes without coupling entities to infrastructure.
- Persistence-specific serialization concerns are isolated from business behavior.
- Future modules should extend the domain only after their invariants and ownership boundaries are defined.
