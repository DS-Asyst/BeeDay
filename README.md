# 🎮 LevelUp

Transform your real life into an RPG.


## About

LevelUp is a console RPG built with C# and .NET.

The objective is to transform personal development into an RPG experience, where every completed training grants experience, levels, attribute progression and, in future versions, quests, bosses, gold and achievements.

The project is also used as a study project to practice Clean Code, Object-Oriented Programming, Git, GitHub and Software Architecture.


## Current Features

- Character creation
- Character progression
- Experience system
- Attribute system
- Training management
- JSON persistence
- Input validation
- Modular console UI


## Project Structure

```text
LevelUp/

├── Data/
│
├── Models/
│   ├── AttributeProgress
│   ├── AttributeType
│   ├── Character
│   ├── GameData
│   ├── Habit
│   ├── ILevelProgress
│   └── PlayerAttributes
│
├── Services/
│   ├── AttributeService
│   ├── CharacterService
│   ├── HabitsService
│   ├── ProgressionService
│   └── SaveService
│
├── UI/
│   ├── BossScreen
│   ├── CharacterCreationScreen
│   ├── CharacterScreen
│   ├── ConsoleHelper
│   ├── GoldScreen
│   ├── InputReader
│   ├── MainMenuScreen
│   ├── QuestScreen
│   └── TrainingScreen
│
└── Program.cs
```


## Technologies

- C#
- .NET 10
- System.Text.Json
- Git
- GitHub


## Running

Clone the repository

```bash
git clone https://github.com/tiagoarrigoni/LevelUp.git
```

Enter the project

```bash
cd LevelUp
```

Run

```bash
dotnet run
```


## Roadmap

### Phase 1
- [x] Character creation
- [x] Character progression
- [x] JSON Save
- [x] Training System
- [x] Console UI Refactoring

### Phase 2
- [ ] Rename Habit → Training
- [ ] Edit trainings
- [ ] Delete trainings
- [ ] Training categories
- [ ] Daily streak

### Phase 3
- [ ] Quest System
- [ ] Boss System
- [ ] Gold System
- [ ] Inventory

### Phase 4
- [ ] SQLite
- [ ] ASP.NET Core API
- [ ] Blazor UI


## Architecture

```text
Program
        │
        ▼
MainMenuScreen
        │
        ├── CharacterScreen
        ├── TrainingScreen
        ├── QuestScreen
        ├── BossScreen
        └── GoldScreen

                │
                ▼

Services

        │
        ▼

Models

        │
        ▼

JSON Persistence
```

## Git Workflow

develop

↓

feature/*

↓

develop

↓

main


## Author

Developed by Tiago Arrigoni