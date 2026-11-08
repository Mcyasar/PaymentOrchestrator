# Multi-Agent Engineering & Orchestration Protocol (`AGENTS.md`)

## 1. Operating Philosophy & Boundaries
All agents operate under strict specialization. English is mandatory for all code, commit messages, issue descriptions, PR summaries, and inline documentation.

### Core Principles
* **Architecture:** Domain-Driven Design (DDD) with Vertical Slice Architecture (VSA). No horizontal layered abstractions (`Controller -> Service -> Repository`). Features are self-contained vertical slices.
* **Methodology:** Lean Kanban with strict Test-Driven Development (TDD).
* **Multi-Agent Role Segregation:** Testing and implementation must never be performed by the same agent context on the same turn. This prevents tests from being modified to fit an incomplete or flawed implementation.
* **Human-in-the-Loop Authority Gate:** No feature branch creation or coding may occur without an explicit human sign-off moving the task to `Ready for Dev`.

---

## 2. Agent Roles & File Access Locks

┌────────────────────────┐         ┌──────────────────────────────┐         ┌────────────────────────┐
│  Architect / PM Agent  │         │      Tester Agent (QA)       │         │ Developer Agent (Dev)  │
│  - Backlog Specs       │ ───►    │   - Red Phase Only           │ ───►    │ - Green & Refactor     │
│  - Scope Definition    │         │   - Path: tests/** (Stubs*)  │         │ - Path: src/**         │
│  - Path: .github/**    │         │   - LOCKED: src/** Impl      │         │ - LOCKED: tests/**     │
└────────────────────────┘         └──────────────────────────────┘         └────────────────────────┘
*Note: QA is permitted to declare signature stubs (`throw new NotImplementedException()`) in `src/**` strictly to ensure compilation.

### Role Matrix & Permissions

| Role | Permitted File Paths | Strictly Locked / Forbidden Paths | Authorized State Transitions |
| :--- | :--- | :--- | :--- |
| **Architect / PM Agent** | `.github/**`, `AGENTS.md`, Issue drafts | `src/**`, `tests/**` | `Backlog` ➔ `Ready for Human Review` |
| **Tester Agent (Red)** | `tests/**`, `src/**` (Stubs only*) | `src/**` (Implementation logic locked) | `Ready for Dev` ➔ `In Progress - Red Phase` |
| **Developer Agent (Green)** | `src/**` | `tests/**` (Write-locked) | `In Progress - Red Phase` ➔ `In Progress - Green Phase` ➔ `In Review` |

---

## 3. Project Board Automation & Configuration

All project board item transitions are driven programmatically using the settings defined in `.github/project-config.json`.

### Configuration Source
Before executing any card move, read `.github/project-config.json`:
* `projectId`: The GitHub Projects v2 Node ID (`PVT_...`).
* `statusFieldId`: The Single Select Field ID for "Status" (`PVTSSF_...`).
* `columns`: Mapping of column names to their respective `Option ID` strings.

### Board Transitions & Column Mapping

| Column on Board | Trigger / Responsibility | Next Step |
| :--- | :--- | :--- |
| `Backlog` | Created by Agent during task decomposition | Move to `Ready for Human Review` |
| `Ready for Human Review` | Agent completes issue specification | **HALT.** Wait for human approval |
| `Ready for Dev` | **HUMAN ONLY** sets this status | Agent picks up issue for TDD |
| `In Progress - Red Phase` | Agent starts work; writes failing test | Commit failing test |
| `In Progress - Green Phase` | Agent writes minimal implementation | All tests pass cleanly |
| `In Review` | Agent completes refactoring & opens PR | Human reviews code |
| `Done` | Human / CI merges PR into `main` | Task completed |

### MCP Tool Execution Pattern
When moving a task across columns, use the GitHub MCP tool `update_project_item_field` (or the equivalent GraphQL mutation) with parameters extracted directly from `.github/project-config.json`:
* `projectId`: `<project-config.json -> projectId>`
* `itemId`: `<project item id>`
* `fieldId`: `<project-config.json -> statusFieldId>`
* `value`: `<project-config.json -> columns[TargetColumn]>`

---

## 4. Step-by-Step TDD & Delivery Protocol

### Step 1: Issue Breakdown & Proposal (Architect / PM Agent)
1. Read .github/project-config.json to extract project IDs.
2. Deconstruct requirements into atomic, single-slice specifications using the Issue Specification Template.
3. Create the issue on GitHub.
4. Add the issue to the project board and transition status to Ready for Human Review.
5. HALT. Do not touch source code, test directories, or create branches.

### Step 2: Red Phase (Tester Agent)
* Pre-condition: Issue status is Ready for Dev.
* Lock Constraint: YOU ARE FORBIDDEN FROM IMPLEMENTING FEATURE LOGIC IN `src/`. (Declaring empty signature stubs throwing `NotImplementedException` strictly to satisfy compilation is permitted).
1. Switch to/create branch: feature/<issue-number>-<short-slug>.
2. Move project board status to In Progress - Red Phase using the configured Option ID.
3. Write failing unit and integration tests under tests/ based on the Gherkin scenarios in the issue.
4. Run dotnet test.
5. Verification Requirement: Tests must fail strictly due to missing feature behavior or assertions. Compilation or syntax errors are not valid Red Phase failures.
6. Commit: test: add failing tests for #<issue-number> (<issue-title>).
7. HALT. Do not attempt to make the tests pass. Hand off to the Developer Agent.

### Step 3: Green Phase (Developer Agent)
* Pre-condition: Project board is in In Progress - Red Phase, and failing tests are committed.
* Lock Constraint: YOU ARE FORBIDDEN FROM MODIFYING ANY FILE IN tests/.
* Anti-Tampering Rule: Any modification, softening of assertions, or deletion of files inside tests/ will result in automatic PR rejection by the CI gatekeeper.
1. Implement the minimal domain and application logic solely inside src/PaymentOrchestrator.Features/<SliceName>/ or src/PaymentOrchestrator.Domain/.
2. Run `dotnet test`. Ensure 100% of the test suite passes.
3. Move the project item to `In Progress - Green Phase`.
4. Commit: `feat: implement #<issue-number> (<issue-title>)`.

### Step 4: Refactor Phase & Pull Request (Developer Agent)
* Lock Constraint: tests/ remains write-locked. All tests must remain green without assertion changes.
1. Refactor the code for domain clarity, allocation efficiency, and formatting without altering external behaviors or breaking tests.
2. Run `dotnet format --verify-no-changes` and `dotnet test`.
3. Move the project item to `In Review`.
4. Submit a Pull Request targeting `main` referencing the issue (`Closes #<issue-number>`) using the **Pull Request Template**.

---

## 5. Architecture & Engineering Rules

### Solution Hierarchy
```text
src/
├── PaymentOrchestrator.Api/              # Entry point, Middleware, OpenTelemetry, Host configuration
│
├── PaymentOrchestrator.Domain/           # Shared Aggregates, Value Objects, Domain Errors
│   ├── Common/                           # Money, Currency, Result<T>, Error
│   └── Payments/                         # PaymentAggregate, PaymentStatus, TransactionId
│
├── PaymentOrchestrator.Features/         # Autonomous Vertical Slices
│   ├── AuthorizePayment/                 # Slice: Endpoint, Command, Handler, Validator
│   ├── CapturePayment/
│   └── Idempotency/                      # Distributed idempotency contracts & gates
│
├── PaymentOrchestrator.Infrastructure/   # Gateway Adapters & Cross-Cutting Concerns
│   ├── Gateways/
│   │   ├── Stripe/                       # Stripe Anti-Corruption Layer (ACL)
│   │   └── MasterCard/                   # MasterCard MPGS ACL
│   ├── Persistence/                      # EF Core configurations, Outbox table
│   └── Resilience/                       # Polly v8 resilience pipelines
│
tests/
├── PaymentOrchestrator.UnitTests/        # Domain rules, Value Object invariants
└── PaymentOrchestrator.IntegrationTests/ # End-to-end slice tests with Testcontainers
```

### Engineering Standards
* **Purity of Domain:** The `PaymentOrchestrator.Domain` project must have zero references to ASP.NET Core, EF Core, or external gateway SDKs (Stripe/Mastercard).

* **Anti-Corruption Layer (ACL):** External vendor payloads, error schemas, and response formats must never leak into feature handlers or the domain. Map them into unified internal representations inside `Infrastructure/Gateways/`.

* **HTTP REPR Pattern:** Use Minimal APIs within each slice rather than monolithic controllers.

* **Integration Testing:** Real dependencies (PostgreSQL, Redis) must run via Testcontainers.DotNet in integration tests. Do not create complex mock graphs for internal business logic.

---

## 6. Templates
### Issue Specification Template
#### 1. Objective & Slice
- **Feature Slice:** `Features/<SliceName>`
- **Overview:** [Brief 2-3 sentence technical description]

#### 2. Boundaries & Touched Components
- **Domain:** `Domain/Payments/...`
- **Infrastructure:** `Infrastructure/...`

#### 3. Acceptance Criteria (Gherkin)
```gherkin
Scenario: [Descriptive Scenario Name]
  Given [Preconditions]
  When [Action/Endpoint call]
  Then [Expected Outcome & Invariants]
```

#### 4. Planned Test Strategy
Unit Test: [Domain invariants to test]
Integration Test: [Endpoint / Testcontainers scenario to test]

#### 5. Explicitly Out of Scope
[Items deliberately excluded from this atomic slice]

### Pull Request Template
## Summary
Closes #<issue-number>
- **Slice:** `Features/<SliceName>`
- **TDD Verification:** Red -> Green -> Refactor cycle completed.

## Verification Checklist
- [x] Failing test committed first
- [x] All tests passing (`dotnet test`)
- [x] Code formatting verified (`dotnet format`)
- [x] No domain leaks or unapproved package references
