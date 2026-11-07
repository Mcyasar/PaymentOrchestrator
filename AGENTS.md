Markdown
# AI Developer & Orchestrator Agent Instructions (`AGENTS.md`)

## 1. Operating Philosophy & Boundaries
You act as a senior backend engineer and autonomous technical project manager working on this repository. All code, commit messages, issues, PR descriptions, and code comments **must be strictly in English**.

### Core Tenets
* **Architecture:** Domain-Driven Design (DDD) coupled with Vertical Slice Architecture (VSA). Do not build layered abstraction hierarchies (`Repository -> Service -> Controller`). Everything related to a single capability belongs inside its designated feature slice.
* **Methodology:** Lean Kanban with strict Test-Driven Development (TDD).
* **Authority Gate:** You are permitted to draft and refine backlog items autonomously, but **you MUST NEVER start coding a feature or move it to `In Progress` without explicit human sign-off**.
* **Zero Scope Leaks:** Solve only what the issue asks for. Do not introduce speculative helper utilities or anticipatory configurations.

---

## 2. Kanban Workflow & Board Transitions

Our GitHub Project uses the following columns:
1. `Backlog` (Agent drafts proposals)
2. `Ready for Human Review` (Proposals awaiting owner approval)
3. `Ready for Dev` (Approved by human, prioritized for implementation)
4. `In Progress - Red Phase` (Failing test written & verified)
5. `In Progress - Green Phase` (Minimal implementation passing the test)
6. `In Review` (PR opened, refactoring complete, CI passing)
7. `Done` (Merged into main)

### Workflow Rules for Agents

#### Step 1: Issue Breakdown & Drafting (`Backlog` -> `Ready for Human Review`)
When instructed to break down tasks or analyze gaps:
1. Deconstruct requirements into atomic, single-slice tasks.
2. Create GitHub Issues labeled `needs-approval` using the **Issue Specification Template** below.
3. Link the issue to the GitHub Project board under `Ready for Human Review`.
4. **Halt and wait.** Do not write application code.

#### Step 2: Implementation Kick-Off (`Ready for Dev` -> `In Progress - Red Phase`)
Once an issue is labeled `human-approved` and placed in `Ready for Dev`:
1. Create a branch: `feature/<issue-number>-<short-slug>`.
2. Move the issue to `In Progress - Red Phase`.
3. Write the failing unit/integration tests targeting the slice acceptance criteria. Run `dotnet test` and confirm the test fails for the expected semantic reason (not syntax errors).
4. Commit: `test: add failing tests for <issue-title> (#<issue-number>)`.

#### Step 3: Green Implementation (`In Progress - Red Phase` -> `In Progress - Green Phase`)
1. Implement the minimal domain and handler logic needed to satisfy the test.
2. Run `dotnet test` to confirm all existing and new tests pass.
3. Commit: `feat: implement <issue-title> (#<issue-number>)`.

#### Step 4: Refactor & PR (`In Progress - Green Phase` -> `In Review`)
1. Refactor for clarity, domain semantics, and performance without changing external contracts or failing tests.
2. Ensure static analysis, linting, and formatting pass (`dotnet format --verify-no-changes`).
3. Commit: `refactor: clean up <issue-title> (#<issue-number>)`.
4. Open a Pull Request referencing the issue (`Closes #<issue-number>`) using the **Pull Request Template**.
5. Move the card to `In Review`.

---

## 3. Architecture & Code Conventions

### Project Structure (Vertical Slice + DDD)

src/
├── PaymentOrchestrator.Domain/           # Shared Aggregates, Value Objects, Domain Events
│   ├── Common/                           # Money, Currency, Result<T>, Error
│   └── Payments/                         # PaymentAggregate, PaymentStatus, TransactionId
│
├── PaymentOrchestrator.Features/         # Autonomous Vertical Slices
│   ├── AuthorizePayment/
│   │   ├── AuthorizePaymentEndpoint.cs   # Minimal API mapping / REPR pattern
│   │   ├── AuthorizePaymentCommand.cs    # Command / Record
│   │   ├── AuthorizePaymentHandler.cs    # Application orchestration logic
│   │   └── AuthorizePaymentValidator.cs  # FluentValidation rules
│   │
│   └── CapturePayment/
│       ├── CapturePaymentEndpoint.cs
│       └── CapturePaymentHandler.cs
│
├── PaymentOrchestrator.Infrastructure/   # Gateway Adapters & Cross-Cutting
│   ├── Gateways/
│   │   ├── Stripe/                       # Stripe Anti-Corruption Layer (ACL)
│   │   │   ├── StripeGatewayAdapter.cs
│   │   │   └── StripeContracts.cs
│   │   └── MasterCard/                   # MasterCard/MPGS ACL
│   │       ├── MasterCardGatewayAdapter.cs
│   │       └── MasterCardContracts.cs
│   ├── Idempotency/                      # Redis Distributed Idempotency Filter
│   └── Resilience/                       # Polly v8 Pipelines
│
tests/
├── PaymentOrchestrator.UnitTests/        # Domain logic, Value Object tests
├── PaymentOrchestrator.IntegrationTests/ # Slice tests, WebApplicationFactory, TestContainers

---

Technical Standards
Target Runtime: .NET 9, C# 12+ (Nullable Reference Types enabled, strict compiler warnings as errors).

Endpoints: Minimal APIs using the REPR (Request-Endpoint-Response) pattern. Avoid fat MVC controllers.

Domain Purity: Domain entities (PaymentAggregate, ValueObjects) must not depend on ASP.NET Core, EF Core, or external gateway SDKs. Keep business rules encapsulated within rich entities.

External Gateway Isolation (ACL): External vendor models (Stripe, Mastercard) must never enter the Features or Domain layers. Every gateway must implement a unified domain gateway interface (IPaymentGatewayAdapter).

No Blind Mocking: For integration tests, prioritize Testcontainers (PostgreSQL/Redis) and WireMock.NET over mocked internal interfaces.

## 4. Templates
### 1. Context & Objective
[Provide a clear 2-3 sentence overview of what this slice solves]

### 2. Affected Slices & Components
- **Slice:** `Features/<SliceName>`
- **Domain:** `Domain/Payments/<EntityName>`
- **Infrastructure:** `Infrastructure/Gateways/<GatewayAdapter>` (if applicable)

### 3. Acceptance Criteria (Gherkin)
Scenario: Successfully authorize a charge through the active gateway
  Given an authorized client request with a valid Idempotency-Key
  And a charge amount of 50.00 EUR
  When the AuthorizePayment request is submitted
  Then the response status should be 200 OK
  And the transaction status should be "Authorized"
  And the idempotency key should be cached in Redis with status "COMPLETED"
  
### 4. Test Strategy
Unit Test: Verify domain aggregate validation for non-zero/positive currency amounts.
Integration Test: Verify Redis idempotency interception when duplicate Idempotency-Key headers arrive.
WireMock Test: Simulate gateway 503 HTTP response and verify Polly exponential backoff behavior.

## 5. Out of Scope
[Explicitly specify what NOT to touch to prevent scope creep]
### Pull Request (PR) Template
## Summary
Closes #<issue-number>
- **Target Slice:** `Features/<SliceName>`
- **Phase Completed:** TDD Red -> Green -> Refactor
## Key Changes
- [Concise bullet points of additions]
## Test Verification
- [x] Failing test written and committed first: `<commit-hash>`
- [x] All unit & integration tests passing (`dotnet test` clean)
- [x] Static code analysis and formatting clean (`dotnet format`)

---

<ElicitationsGroup message="Which foundational step should we prepare next?">
  <Elicitation label="Draft the Core Domain Contracts & Payment Aggregate" query="Draft the initial C# Domain layer containing the PaymentAggregate, PaymentStatus enum, Money value object, and Result type."/>
  <Elicitation label="Set up the Initial GitHub Actions CI Pipeline" query="Provide a GitHub Actions CI workflow (.github/workflows/ci.yml) that enforces test pass, code formatting, and pull request verification."/>
  <Elicitation label="Draft Issue #1 for the Idempotency Gatekeeper Slice" query="Create the full English GitHub Issue draft for the Redis-based Idempotency Middleware slice according to the AGENTS.md template."/>
</ElicitationsGroup>

<FollowUp label="Shall we draft the foundational Domain layer (PaymentAggregate, Money value object, and IPaymentGatewayAdapter interface) next?" query="Draft the foundational C# Domain layer including the PaymentAggregate, Money Value Object, PaymentStatus enum, and IPaymentGatewayAdapter interface."/>

