# Frontend Core Principles

This document defines **non-negotiable frontend principles** for products that use **TypeScript** and **React**. Adapt folder names to your repository; keep the principles as written unless the team explicitly amends this guideline.

See also: [Backend core principles](./core-principles-backend.md) | [Overview](./core-principles.md)

## Principles

### I. Stack (TypeScript + React)

All frontend code MUST be written in **TypeScript** and **React**. JavaScript-only source files, class components as the default pattern, and alternate UI frameworks are not permitted unless the team explicitly approves an exception and updates this guideline.

**Rationale**: One typed React stack reduces cognitive load, enables shared patterns (hooks, composition), and keeps reviews predictable.

### II. Component Architecture

Frontend structure and component design MUST follow the rules below. Full examples may live in feature plans, ADRs, or agent rules for your project.

**Screens** (`screens/<screen-name>/`):

- One folder per screen in **kebab-case**; the main file `<screen-name>.tsx` exports the screen.
- `components/` inside a screen holds components used **only on that screen**.
- Optional `utils.ts` / `types.ts` for screen-specific helpers and types.
- Simple screens (about 10 lines) MUST NOT add an empty `components/` folder.

**Shared components** (`components/<component-name>/`):

- Used on **2+ screens**, domain-agnostic (data via props), or generic UI primitives.
- Each shared component SHOULD include `*.stories.tsx` when Storybook is in use.
- `components/ui/` holds UI primitives (Box, VStack, Button, and similar).

**Promotion rule**: When a screen-local component is needed elsewhere, move it to `components/`, remove API-specific dependencies, and pass data via props.

**Component design**:

- Components MUST be small and focused (one responsibility).
- Extract when: a fragment has its own state; conditional JSX is about 10 lines or more; list `.map()` items; helpers belong to a JSX fragment.
- Prefer **composition** (children, slots) over prop drilling.
- **Presentational** shared components receive data via props; they MUST NOT fetch by ID.
- **Smart** screens and screen-private components MAY use API or store hooks.
- Hierarchy: Screen (smart) → screen-private → shared (presentational) → UI primitives.

**Naming**:

| Element | Convention | Example |
|---------|------------|---------|
| Screen folder | kebab-case | `screens/event-detail/` |
| Component file | kebab-case | `ride-card.tsx` |
| Exported component | PascalCase | `export function RideCard()` |
| Props type | PascalCase + `Props` | `type RideCardProps` |
| Shared folder | kebab-case | `components/event-card/` |

**Rationale**: Clear boundaries between screen-local and shared UI prevent premature abstraction and keep data flow testable.

### III. Testing

- **Presentational** components: test render variants from props.
- **Smart** screens: integration tests with mocked network or API, not mocked child trees.
- **Shared** `components/`: Storybook stories are expected (`*.stories.tsx`) when Storybook is in use.

Add tests when the feature spec requests them or when verifying non-trivial behavior. Trivial “assert true” tests are forbidden.

**Rationale**: Tests align with component boundaries—props at the leaves, contracts at the boundaries.

## Reference Layout

```text
frontend/
  src/
    screens/<screen-name>/
      <screen-name>.tsx
      components/          # optional, screen-private
      utils.ts             # optional
      types.ts             # optional
    components/            # shared + ui/
    hooks/                 # or api/ for data hooks
```

## Development Workflow

1. **Spec first**: User scenarios and acceptance criteria in the feature spec before UI tasks.
2. **Plan gate**: The implementation plan MUST pass a **Frontend Principles Check** (screen folders, component boundaries, promotion rules).
3. **Tasks**: Include concrete paths (for example `frontend/src/screens/...`).
4. **Reviews**: Verify stack compliance and component promotion rules.

**New screen checklist**:

1. Create `screens/<name>/` and `<name>.tsx`.
2. Add `components/` only when sections warrant extraction.
3. Move business logic to `hooks/` or `api/`.
4. Promote reusable UI to root `components/` when used on 2+ screens.

**Deviations** (extra libraries, alternate UI kit) MUST be documented in the feature plan or ADR before implementation, with rationale and rollback plan.

## Governance

This guideline supersedes ad-hoc frontend conventions in individual pull requests and agent sessions for projects that adopt it.

**Versioning** (semantic versioning for this document):

- **MAJOR**: Removing or redefining a principle (for example, abandoning TypeScript).
- **MINOR**: New principle or materially expanded guidance.
- **PATCH**: Wording clarifications without behavioral change.

**Compliance**: Review against this guideline during planning and before merge. Keep project-specific agent rules and plan templates aligned when this document changes.

**Version**: 1.0.0 | **Ratified**: 2026-05-19 | **Last Amended**: 2026-05-19
