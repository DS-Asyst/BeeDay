# Domain

The Domain project owns LevelUp business state and invariants.

## Documents

- [Domain model overview](overview.md)
- [Users and identity](users-and-identity.md)
- [Activities and projects](activities-and-projects.md)
- [Experience](experience.md)
- [Wallet](wallet.md)

## Principles

- Entities protect their own valid state.
- Value objects validate domain-specific values.
- Domain events represent meaningful completed state changes.
- Application handlers coordinate use cases but do not replace entity invariants.
- Infrastructure serialization must preserve domain meaning without moving rules into storage code.
