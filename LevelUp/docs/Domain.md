# LevelUp Domain

## Core Model

LevelUp models real-world productivity with RPG-inspired presentation.

```text
Character
├── Attributes
├── Habits
├── Quests
└── Projects
    └── Quests
```

## Character

Represents user progression, including experience, level and attributes.

## Habit

Represents a repeatable activity. The UI presents a Habit as Training. Completing a habit may grant character and attribute experience.

## Quest

Represents a one-time task.

Rules:

- may be independent or associated with one Project;
- follows `Created → Active → Completed` lifecycle;
- may be archived;
- stores creation, activation, completion and archive timestamps;
- can be edited while not archived;
- completion contributes to project progress.

## Project

Represents a long-term objective composed of zero or more quests.

Rules:

- follows `Created → Active → Completed` lifecycle;
- may be archived;
- stores an unlocked title as its current reward metadata;
- progress is derived and never persisted;
- archived quests do not participate in progress;
- a project with at least one valid quest completes automatically when every valid quest is completed;
- only an active project can complete automatically.

## Project Progress

```text
Completed non-archived quests / Total non-archived quests
```

Examples:

- 0 valid quests → 0%;
- 1 of 4 completed → 25%;
- 4 of 4 completed → 100% and automatic completion for an active project.

## Associations

Quests store an optional `ProjectId` instead of a direct object reference. This keeps JSON persistence simple and avoids circular serialization graphs.

## Domain and Presentation

The domain uses real-world terminology. RPG metaphors belong to the UI. For example, Habit remains the domain entity while the UI calls it Training.


## Phase 4 hardening rules

- Archived projects and quests cannot be edited.
- Archived quests cannot be assigned to another project.
- Project deletion may remove quest associations, including archived quests, to preserve referential integrity.
- Quests may only be assigned to projects in Created or Active status.
- Project progress ignores archived quests.


## Milestones

A Milestone is an ordered, optional Project chapter. It always belongs to a Project, may contain Quests, may define a quest-count requirement, and may expose reward metadata. Only one Milestone can be active per Project.

## Boss Encounters

A Boss Encounter is an optional challenge linked to one Milestone. Completing the Milestone requirements unlocks the Boss. Defeating it completes the Milestone; a final Boss may complete the Project. Phase 4 intentionally does not introduce a full combat engine.
