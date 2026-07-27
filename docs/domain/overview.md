# Domain Model Overview

## Root persisted state

`LevelUpData` represents the persisted application state and contains users and their related product data. Persistence-specific compatibility behavior is separated into the `LevelUpData.Persistence` partial definition.

## Main entities

- `User`: identity, account, preferences, active state, character ownership, and tokens;
- `Character`: character identity, class, avatar/onboarding state, and experience;
- `Activity`: shared base for Habit, RecurringTask, Todo, and Project;
- `Project`: project state and relationships to work items;
- `Wallet`, `Transaction`, and `InventoryTag`: inventory state;
- `UserToken`: confirmation and password-reset token state.

## Main value objects

- `EmailAddress`
- `UserName`
- `CharacterNickname`
- `ActivityTitle`
- `ActivityDescription`
- `ProjectColor`

## Domain events

- `ApplicationActionDomainEvent`
- `ExperienceGrantedDomainEvent`
- `CharacterLeveledUpDomainEvent`

## Enumerations

Domain enums define supported character classes, activity attributes, habit settings, task recurrence, project status, transaction types, identity settings, token types, and experience source/reward classifications.
