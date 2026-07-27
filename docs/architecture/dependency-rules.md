# Dependency and Ownership Rules

## Domain

Domain may contain pure business logic only. It must not reference Application, Infrastructure, ASP.NET Core, Blazor, file systems, email providers, or HTTP concerns.

Put here:

- invariants and state transitions;
- entities, value objects, enums, and domain exceptions;
- domain events;
- experience and level calculations.

## Application

Application depends on Domain and owns use-case sequencing.

Put here:

- commands, queries, requests, responses, validators, and handlers;
- contracts for persistence and technical services;
- authorization checks based on the current-user abstraction;
- centralized experience reward orchestration;
- cross-cutting pipeline behavior.

Application must not know concrete JSON, IIS, Resend, or Blazor types.

## Infrastructure

Infrastructure depends on Application and provides technical implementations.

Put here:

- JSON storage and backup mechanics;
- PBKDF2 password hashing;
- email senders and token protection;
- caching, auditing, clocks, queues, hosted workers, and health dependencies.

Do not place feature business rules in Infrastructure.

## Web

Web is the composition root and may reference all production projects.

Put here:

- routes, HTTP endpoints, cookies, antiforgery, middleware, and redirects;
- Blazor components, UI state, presentation formatting, and interaction behavior;
- dependency-injection composition and configuration validation;
- health response formatting and static assets.

Web components must not instantiate or depend on concrete JSON persistence classes.

## Change guidance

- A new invariant starts in Domain.
- A new user action becomes an Application use case.
- A new external implementation belongs in Infrastructure behind an Application contract.
- A new visual behavior belongs in Web and should reuse the Design System.
- Cross-layer shortcuts require an explicit architectural decision and documentation update.
