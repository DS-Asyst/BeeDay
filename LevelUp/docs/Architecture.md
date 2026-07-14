# LevelUp Architecture

## Overview

LevelUp is a console-based productivity RPG developed with C# and .NET.

The system uses real-world terminology in its domain model and RPG terminology in its presentation layer.

The current application is organized around four primary areas:

```text
LevelUp/
├── Domain/
│   ├── Attributes/
│   ├── Character/
│   ├── Habits/
│   ├── Projects/
│   └── GameData.cs
│
├── Services/
│   ├── Character/
│   ├── Habits/
│   ├── Persistence/
│   └── Projects/
│
├── UI/
├── Data/
└── docs/