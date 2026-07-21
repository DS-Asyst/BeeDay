# LevelUp Roadmap

## Completed foundation

- Solution organization
- Domain cleanup
- Application use cases
- JSON persistence, backup, logs, and health check
- Feature-based Blazor frontend
- Frontend state management
- Initial Design System and component library

## Next priorities

### Component library expansion

- Fields, selects, cards, badges, toasts, skeleton loading
- Reduce remaining repeated markup and CSS contracts

### UX and accessibility

- Focus management for dialogs
- Keyboard navigation
- Screen-reader labels and live regions
- Responsive behavior
- Loading and operation feedback

### Frontend tests

- bUnit tests for editors, dashboard cards, confirmation dialogs, and state-driven rendering

### Specialized services

Create narrower frontend services only where state classes or `LevelUpWebService` become too broad. Avoid creating one service per entity without a concrete responsibility.
