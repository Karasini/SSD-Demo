# Backend Core Principles

This document defines **non-negotiable backend principles** for products that use **.NET** with **PostgreSQL** and **Entity Framework Core**. Adapt folder names to your repository; keep the principles as written unless the team explicitly amends this guideline.

See also: [Frontend core principles](./core-principles-frontend.md) | [Overview](./core-principles.md)

## Principles

### I. Stack (.NET + Vertical Slices)

Backend code MUST use the **current .NET LTS** version aligned with the solution, organized as **vertical slices** under `Features/`. Layered “god” folders (for example, all controllers in one tree unrelated to features) MUST NOT replace slice-local Endpoint, Handler, DTOs, Commands, Queries, Validators, and Exceptions.

**Rationale**: Vertical slices keep use cases cohesive, reviewable, and independently evolvable.

### II. Persistence (PostgreSQL + EF Core)

The system of record MUST be **PostgreSQL**, accessed via **Entity Framework Core**. Repositories and `DbContext` live in `Infrastructure/Persistence/`; domain entities and value objects remain in `Domain/`. Schema changes MUST use EF Core migrations (or approved SQL migration scripts under `Infrastructure/Persistence/Migrations/`), not ad-hoc manual production edits.

**Rationale**: One relational store with a first-class ORM supports auditability, reporting, and common .NET team skills.

### III. Testing

- Unit tests without HTTP.
- Integration or API tests with `WebApplicationFactory`.
- Test utilities in dedicated test projects.

Add tests when the feature spec requests them or when verifying non-trivial behavior. Trivial “assert true” tests are forbidden.

**Rationale**: Tests align with slice boundaries—domain and handlers in unit tests, HTTP contracts in integration tests.

## Reference Layout

```text
src/<SolutionName>/
  <ApiHost>/                         # API host project
    Program.cs
    EndpointsConfiguration.cs        # HTTP endpoint registration
    ConsumersConfiguration.cs        # message consumers (if used)
    Common/                          # shared primitives, validation codes
    Domain/                          # entities, value objects
    Events/                          # integration or domain event contracts
    Infrastructure/
      Persistence/                   # EF Core DbContext, repositories, migrations
      DI/                            # marker interfaces (IHandler, ICommand, IQuery, …)
    Features/                        # vertical slices
      <FeatureName>/
        Endpoint.cs
        Handler.cs
        Validator.cs
        Dtos/ | Commands/ | Queries/ | Exceptions/
      ProcessingPipelines/           # optional multi-step pipelines
        <Pipeline>/<Step>/…
  <ApiHost>.Tests/
  <ApiHost>.IntegrationTests/
  <ApiHost>.Tests.BuildingBlocks/    # optional shared test helpers
docs/
scripts/
docker/                              # local infra as needed
```

## Placement Rules

- New HTTP features → `Features/<FeatureName>/`; register in `EndpointsConfiguration.cs`.
- Pipeline steps → `Features/ProcessingPipelines/<Pipeline>/<Step>/`.
- Repositories → `Infrastructure/Persistence/<Area>/` with interfaces and EF implementations.
- Pub/Sub or queue consumers → beside the feature Handler; register in `ConsumersConfiguration.cs`.
- Constants → one class per file in `Common/` or feature `Constants/`.
- Handlers orchestrate commands and queries; persistence stays in Infrastructure.

**Deviations** (extra projects, libraries, or stores) MUST be documented in the feature plan or ADR before implementation, with rationale and rollback plan.

## Development Workflow

1. **Spec first**: User scenarios and acceptance criteria in the feature spec before backend tasks.
2. **Plan gate**: The implementation plan MUST pass a **Backend Principles Check** (slice placement, PostgreSQL and EF Core for persistence, endpoint registration).
3. **Tasks**: Include concrete paths (for example `Features/<FeatureName>/...`).
4. **Reviews**: Verify stack compliance and slice boundaries.

## Governance

This guideline supersedes ad-hoc backend conventions in individual pull requests and agent sessions for projects that adopt it.

**Versioning** (semantic versioning for this document):

- **MAJOR**: Removing or redefining a principle (for example, abandoning vertical slices).
- **MINOR**: New principle or materially expanded guidance (for example, a new observability rule).
- **PATCH**: Wording clarifications without behavioral change.

**Compliance**: Review against this guideline during planning and before merge. Keep project-specific agent rules and plan templates aligned when this document changes.

**Version**: 1.0.0 | **Ratified**: 2026-05-19 | **Last Amended**: 2026-05-19
