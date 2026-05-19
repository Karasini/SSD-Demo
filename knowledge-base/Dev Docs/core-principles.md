# Core Principles (Overview)

This folder holds **non-negotiable engineering principles** for full-stack products. Principles are split by stack so frontend and backend teams can adopt and version them independently.

| Document | Scope |
|----------|--------|
| [core-principles-frontend.md](./core-principles-frontend.md) | TypeScript, React, component architecture, frontend layout and tests |
| [core-principles-backend.md](./core-principles-backend.md) | .NET vertical slices, PostgreSQL, EF Core, backend layout and tests |

Adapt folder names to your repository. Keep each document’s principles as written unless the team explicitly amends that guideline.

## Cross-Stack Workflow

These steps apply to features that touch both stacks:

1. **Spec first**: User scenarios and acceptance criteria in the feature spec before implementation tasks.
2. **Plan gate**: The implementation plan MUST pass both a **Frontend Principles Check** and a **Backend Principles Check** (see the linked documents).
3. **Tasks**: Include concrete paths for each stack (for example `frontend/src/screens/...`, `Features/...`).
4. **Reviews**: Verify compliance for every stack touched by the change.
5. **Amendments**: Changes to a guideline require team agreement, a version bump on that document, and sync of dependent templates or agent rules.

**Version**: 1.0.0 | **Ratified**: 2026-05-19 | **Last Amended**: 2026-05-19
