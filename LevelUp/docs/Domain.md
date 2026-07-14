# LevelUp Domain

## Overview

The LevelUp domain models real-world productivity using RPG-inspired progression mechanics.

The domain always represents real concepts.

The presentation layer is responsible for transforming those concepts into an RPG experience.

---

# Core Domain

The domain is centered around the Character.

```text
Character
│
├── Habits
├── Quests
├── Projects
├── Experience
├── Attributes
├── Gold
├── Titles
└── Achievements
```

The character evolves by completing activities.

---

# Domain Entities

## Character

Represents the user inside the progression system.

Responsible for:

- experience
- level
- attributes
- unlocked titles
- achievements
- gold

---

## Habit

Represents a recurring activity.

Characteristics

- repeatable
- optional project association (future)
- grants experience
- grants attribute experience

Presentation

Training

---

## Quest

Represents a one-time task.

Characteristics

- completed once
- may belong to a project
- grants experience
- may grant gold
- may unlock achievements

Presentation

Quest

---

## Project

Represents a long-term objective.

Characteristics

- contains zero or more quests
- progress is calculated
- does not grant experience
- unlocks titles
- may unlock achievements

Presentation

Project

---

## Milestone

Represents an important stage inside a project.

Characteristics

- belongs to a project
- groups multiple quests
- represents a boss encounter in the UI

Presentation

Boss

---

# Domain Relationships

```text
Project
│
├── Quest
├── Quest
└── Quest
```

```text
Character

↑

Habit

Quest

Project
```

Habits and Quests improve the Character.

Projects unlock Titles and Achievements.

---

# Progression

Character progression occurs through three independent systems.

## Habit Progression

Habit Completed

↓

Character XP

↓

Attribute XP

↓

Level Check

---

## Quest Progression

Quest Completed

↓

Character XP

↓

Project Progress

↓

Project Completed?

---

## Project Progression

Project Completed

↓

Unlock Title

↓

Unlock Achievement

---

# Project Progress

Project progress is never stored.

It is calculated.

Formula

Completed Quests / Total Quests

Example

2 / 10

↓

20%

If additional quests are linked to the project, the percentage is recalculated automatically.

---

# Domain Events

The following events represent important business facts.

Character Created

Habit Created

Habit Completed

Quest Created

Quest Completed

Project Created

Quest Linked To Project

Project Completed

Achievement Unlocked

Title Unlocked

Game Saved

---

# Business Rules

## Habits

A habit may be completed multiple times.

---

## Quests

A quest may be completed only once.

---

## Projects

A project may exist without quests.

A completed project may become archived.

Progress is always calculated.

Projects do not grant XP.

---

# Future Evolution

Future entities include:

Reward

Statistics

Inventory

Item

Equipment

Shop

Event

Reward will be introduced only when multiple domain concepts require a shared reward model.