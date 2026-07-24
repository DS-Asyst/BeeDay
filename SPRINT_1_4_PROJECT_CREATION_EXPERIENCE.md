# Sprint 1.4 — New Project Creation Experience

## Implemented flow

Project → Open Project → Add To-Dos → Execute → Automatic completion.

## Changes

- Projects open a dedicated planning workspace from the dashboard card.
- To-Dos are created inside the open Project and inherit its ProjectId automatically.
- The standalone add button was removed from the To-Dos dashboard column.
- Project status is derived automatically:
  - Planned: no To-Dos.
  - In Progress: at least one pending To-Do.
  - Completed: all To-Dos completed.
- Manual Project completion remains prohibited by the domain.
- The workspace shows progress, status, To-Dos, completion actions and direct editing.
