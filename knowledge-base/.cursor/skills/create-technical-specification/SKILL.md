---
name: create-technical-specification
description: Drafts technical-specification.md for a feature by analyzing overview/usecases/specification-ux-ui, applying knowledge-base technical-specification rules plus optional backend/frontend .cursor/rules from the workspace, and producing simple English (B2), developer-focused implementation HOW (endpoints, payloads/contracts as JSON, integrations, error modes, optional diagrams). Use when the user asks to create or update technical specification for a specific feature folder.
---

# Create Technical Specification

## Purpose

Generate a `technical-specification.md` that lets developers implement the feature without guessing. The document must focus on the implementation **HOW**, integration points, and concrete contracts.

## Required inputs (from workspace)

1. A **feature folder** containing these files (source of truth for requirements):
   - `overview.md`
   - `usecases.md`
   - `specification-ux-ui.md`
2. Optional but recommended: **implementation codebases** in the same workspace (backend, frontend, or other services) so the agent can read `.cursor/rules` and mirror existing patterns.

If the user only has the knowledge-base repo, still draft the spec from the three requirement files; ask for codebase paths or rules when integration details are unclear.

## Step-by-step workflow

1. **Identify the feature scope**
   - Ask for the **feature folder path** and where `technical-specification.md` should live (usually the same folder).
   - Verify the three requirement files exist.
   - If any file is missing, ask the user to provide it or confirm the correct folder.

2. **Analyze requirements in the feature folder**
   - Read `overview.md`: goals, non-goals, actors/roles, main flows, assumptions.
   - Read `usecases.md`: steps, decision points, error conditions, dependencies.
   - Read `specification-ux-ui.md`: UI states and data needs that map to API contracts.

3. **Discover implementation context (when codebases are available)**
   - Ask which repos matter for this feature (e.g. API service, web app, worker) if not obvious from the workspace.
   - For each relevant repo, run a short **pattern discovery** pass (no source code in the spec output):
     - Find 1–3 similar features/modules and mirror: endpoint style, contract/DTO naming, error response shape, messaging/event patterns, auth boundaries, AI integration shape (if applicable).
     - For client apps: how APIs are consumed (hooks, caching, offline behavior) at the **contract/behavior** level only.
   - If implementation repos are not in the workspace, ask the user to add folders via **Add Folder to Workspace**, or to point to rule paths / example features.

4. **Apply documentation and engineering rules**
   - **Knowledge-base** (this repo): `.cursor/rules/technical-specification-documents.mdc` and `.cursor/rules/english-simple-b2-documents.mdc`.
   - **Implementation repos** (when present): scan `.cursor/rules/**/*.mdc` (or project-documented rule paths) and follow conventions that affect the design (architecture layers, CQRS, observability, messaging, API style, security).
   - From `technical-specification-documents.mdc`, in particular:
     - **Simple English (B2)** for all spec content
     - Optimize for developers, solution architects, and QA
     - Focus on HOW and integration points; be concrete and concise
     - Choose **exactly one** pattern per document:
       - Pattern 1: Flow-Based Solution
       - Pattern 2: Solution Analysis & Selection
       - Pattern 3: Single Solution
     - Allowed examples: JSON payloads, HTTP endpoints with parameters and response shapes, contracts as JSON
     - JSON: use fenced ` ```json ` blocks with valid JSON for params, request/response, and contracts
     - **Must NOT include**: problem description (use `overview.md`), user stories (use `*userstory*.md`), time estimates/roadmaps, database schemas unless critical to the approach
     - **No example source code** (C#/TS/Python/SQL/etc.) unless the user explicitly asks

5. **Choose the spec pattern (exactly one)**
   - Multi-step flows → Pattern 1.
   - Multiple approaches to compare → Pattern 2 (pros/cons + recommendation).
   - Straightforward single approach → Pattern 3.
   - If ambiguous, propose two options and ask the user which pattern to use.

6. **Design the implementation (developer-facing)**
   - Document integration points explicitly:
     - HTTP: method, URL, parameters, request/response shapes.
     - Messaging/events: consumer/producer, payload contracts, ordering/delivery expectations when relevant.
   - Respect architecture constraints stated in project rules (e.g. Clean Architecture, CQRS, DDD boundaries): orchestration in application/feature layer, integrations in infrastructure, separate reads from writes where the project expects it.
   - Include JSON blocks for request/response, DTOs, events, and non-obvious state transitions.
   - Include failure modes and **caller-visible** behavior: validation, auth, upstream failures, partial failures; map to the project's standard error format at the spec level (not code).

7. **Challenge the solution and ask for user decisions**
   - For boundary decisions, propose alternatives (max 2–3) with short pros/cons (Pattern 2 preferred, or a brief inline comparison).
   - Consult the user before finalizing when the project does not already dictate:
     - message ordering keys (if messaging is used)
     - consistency (idempotency, retries, conflicts)
     - AI inputs/outputs (if AI is involved)

8. **Draft output and request approval**
   - Produce the full `technical-specification.md` draft as Markdown.
   - Ask the user to confirm: pattern/structure, endpoints/contracts, and whether to write or update the file.

## Output constraints (must obey)

- **Language:** simple English (B2) — see `english-simple-b2-documents.mdc`.
- No example source code unless the user explicitly asks.
- Allowed: JSON in fenced ` ```json ` blocks; endpoint and integration contracts in prose + JSON.
- Do not include problem description, user stories, or time estimates/roadmaps.
- No marketing fluff; every section should add implementable detail.
- Allowed: Mermaid diagrams when helpful (sequence, flow).
- Do not repeat `overview.md` as a “problem” section; treat it as source of truth.

## Early clarification questions (ask when needed)

Ask only what is needed to avoid wrong contracts:

- Feature scope: which user actions and which systems are in or out?
- Auth: admin-only vs user-scoped? Which roles?
- Sync vs async: main flow HTTP, messaging, or both?
- Ordering: if message ordering matters, what is the ordering key? (User should confirm.)
- Idempotency/retries: what identifiers make reprocessing safe?
- Data contracts: existing DTO/event names or new ones?
- Errors: which failures are retryable vs terminal for the caller?
- Client: what must the UI show when offline or degraded (if relevant)?

## Definition of done

- Exactly one approved pattern (Flow-Based / Solution Analysis & Selection / Single Solution).
- Concrete HOW with explicit integration points.
- Payloads/contracts in valid JSON blocks where applicable.
- Key failure modes with caller-visible behavior.
- Architecture boundaries match project rules (when repos/rules were available).
- Follows `technical-specification-documents.mdc` (English B2, no example code, no marketing fluff).
