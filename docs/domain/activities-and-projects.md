# Activities and Projects

`Activity` is the shared abstraction for Habits, recurring Tasks, To-Dos, and Projects.

## Shared activity data

Activities use validated titles and descriptions and may carry an optional semantic attribute:

- Strength
- Dexterity
- Intelligence
- Wisdom
- Vitality
- Charisma

A missing attribute is valid. Attributes are organizational metadata only: they do not grant XP, alter rewards, change levels, or modify the experience curve.

## Habits

Habits support direction, difficulty, reset-counter behavior, counters, and domain rules validated by `Habit` and the related enums.

## Recurring Tasks

Recurring Tasks model repeat behavior through `TaskRepeat` and application-managed completion operations.

## To-Dos

To-Dos represent finite work and can participate in project organization.

## Projects

Projects carry a validated color, status, activity data, and progress relationships. Project status is represented by `ProjectStatus`.

## Ordering

Manual activity ordering is a supported product behavior. Search, filtering, and optional attribute sorting must not silently overwrite the authoritative manual order.
