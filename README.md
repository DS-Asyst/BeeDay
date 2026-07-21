# 🚀 LevelUp

> A modern productivity application built with **ASP.NET Core**, **Blazor Server** and **Clean Architecture** that transforms your daily habits, tasks and projects into an organized personal management system.

---

## Overview

LevelUp is a modern productivity platform designed to centralize the management of personal activities through a clean, lightweight and maintainable architecture.

Unlike traditional task managers, LevelUp was designed from the beginning to be:

- Clean and modular
- Easy to maintain
- Easy to extend
- Independent of relational databases
- Focused on performance
- Ready for future cloud synchronization

The project currently stores all information in a local JSON database while maintaining a layered architecture that allows future migration to SQL Server, PostgreSQL or any other persistence provider.

---

## Features

### Productivity

- ✅ Habits
- ✅ Tasks
- ✅ To-Dos
- ✅ Projects
- ✅ Dashboard
- ✅ Search
- ✅ User Profile

### Storage

- ✅ JSON persistence
- ✅ Atomic writes
- ✅ Automatic recovery
- ✅ Rotating backups
- ✅ Data validation

### Architecture

- ✅ Clean Architecture
- ✅ Dependency Injection
- ✅ Feature-oriented organization
- ✅ Strong separation of concerns
- ✅ Unit Tests
- ✅ Health Checks

---

# Solution Architecture

```text
                    Browser
                        │
                        ▼
               Blazor Server UI
                        │
                        ▼
                 Application Layer
                        │
                        ▼
                  Domain Layer
                        │
                        ▼
             Infrastructure Layer
                        │
                        ▼
                 LevelUpBD.json
```

---

# Project Structure

```text
LevelUp/

├── src/
│
│   ├── LevelUp.Domain/
│   │
│   ├── LevelUp.Application/
│   │
│   ├── LevelUp.Infrastructure/
│   │
│   └── LevelUp.Web/
│
├── tests/
│
│   ├── LevelUp.Domain.Tests/
│   ├── LevelUp.Application.Tests/
│   └── LevelUp.Infrastructure.Tests/
│
├── .editorconfig
├── .gitignore
├── Directory.Build.props
├── Directory.Packages.props
├── LICENSE
├── README.md
└── LevelUp.slnx
```

---

# Project Layers

## Domain

Responsible for the business rules.

Contains:

- Entities
- Value Objects
- Enums
- Domain Validations
- Domain Services

This layer has **no external dependencies**.

---

## Application

Coordinates the application's use cases.

Contains:

- Use Cases
- Requests
- Responses
- Service Contracts
- Feature Organization

Business rules remain isolated from the presentation layer.

---

## Infrastructure

Responsible for technical concerns.

Contains:

- JSON persistence
- Storage services
- Backup system
- Recovery
- Health checks
- Configuration

The persistence provider can be replaced without affecting the application layer.

---

## Web

Presentation layer built with Blazor Server.

Contains:

- Pages
- Components
- Layouts
- Feature modules
- State Management

The frontend communicates only with the Application layer.

---

# Frontend Organization

The frontend follows a **Feature First** organization.

```text
Components/

Features/

Dashboard/

Habits/

Tasks/

Todos/

Projects/

Profile/

Shared/

Layout/
```

Each feature contains its own:

- Components
- Models
- State
- Services
- UI logic

This keeps the project modular and scalable.

---

# JSON Persistence

The application currently uses a local JSON database.

```
src/LevelUp.Web/Data/LevelUpBD.json
```

Features include:

- Atomic writes
- Backup rotation
- Automatic recovery
- Validation
- Health monitoring

No external database server is required.

---

# Technology Stack

- .NET 10
- ASP.NET Core
- Blazor Server
- C#
- System.Text.Json
- Dependency Injection
- xUnit
- Clean Architecture

---

# Running the Project

Clone the repository.

```bash
git clone https://github.com/tiagoarrigoni/LevelUp.git

cd LevelUp
```

Restore packages.

```bash
dotnet restore
```

Build.

```bash
dotnet build
```

Run tests.

```bash
dotnet test
```

Run the application.

```bash
dotnet run --project src/LevelUp.Web/LevelUp.Web.csproj
```

Local URLs:

```
https://localhost:7245

http://localhost:5059
```

Health endpoint:

```
GET /health
```

---

# Testing

The solution contains independent test projects for each application layer.

```text
tests/

LevelUp.Domain.Tests

LevelUp.Application.Tests

LevelUp.Infrastructure.Tests
```

Run all tests:

```bash
dotnet test
```

---

# Design Principles

The project follows modern software engineering practices.

- Clean Architecture
- SOLID Principles
- Separation of Concerns
- Dependency Injection
- Feature-oriented organization
- Testability
- Maintainability
- Scalability

---

# Roadmap

Current progress:

- ✅ Solution restructuring
- ✅ Domain separation
- ✅ Application refactoring
- ✅ JSON persistence
- ✅ Frontend foundation
- ✅ Frontend state management

Future goals:

- Authentication
- Statistics
- Themes
- Achievements
- Notifications
- Cloud Synchronization
- Docker support
- REST API
- Progressive Web App (PWA)

---

# License

This project is distributed under the MIT License.

---

# Author

Developed by **Tiago Arrigoni**.

GitHub:

https://github.com/tiagoarrigoni
