# Activity Attributes

## Purpose

Activity attributes provide optional semantic classification for Habits, Tasks, To-Dos, and Projects. They improve organization and prepare the application for attribute-based filtering.

## Supported values

- Strength
- Dexterity
- Intelligence
- Wisdom
- Vitality
- Charisma

`None` is represented by a null value and remains valid.

## Domain rules

- The attribute belongs to the shared `Activity` abstraction.
- Only values defined by `ActivityAttribute` are accepted.
- Selection is optional during creation and editing.
- The value is persisted with the activity in JSON storage.
- Existing data remains compatible because missing attributes deserialize as null.

## Explicit non-goals

Activity attributes do not grant XP, change levels, modify rewards, affect the experience curve, or alter character progression. They are organizational metadata only.

## Affected layers

- Domain: nullable `Activity.Attribute` and validated mutation.
- Application: request contracts, validators, and handlers.
- Web: editor models and create/edit selectors.
- Infrastructure: existing JSON serialization persists the new property without a schema migration.
- Tests: domain, validation, handler, and persistence coverage.
