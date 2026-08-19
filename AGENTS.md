# AI Collaboration Instructions

## Context

This is a C# coding interview exercise for a metering operator.

The task is to build an in-memory backend service around meter readings and consumption. Requirements are expected to arrive step by step. The goal is working software, clear data modelling, and safe adaptation as requirements evolve.

This is not a time-series task:

- Months are simple business billing periods.
- Do not introduce streams, windowing, schedulers, or date-time complexity unless explicitly required.
- Use year and month through `BillingPeriod` when a real period is needed.

## Stack

- Language: C#
- Test framework: xUnit
- Storage: in-memory collections
- Current solution: `MeteringInterview.sln`

Do not add EF Core, Web API controllers, MediatR, AutoMapper, databases, or other frameworks unless the requirement explicitly asks for them.

## Project Shape

The project is intentionally small and DDD-inspired:

```text
src/Metering/
  Domain/
  Application/
  Infrastructure/

tests/Metering.Tests/
```

Treat `Metering` as the current business boundary. Avoid extra folders such as `Domain/Models`, `Domain/Services`, or `Infrastructure/InMemory` until the codebase is large enough to justify them.

## Domain Rules

- A meter reading is cumulative.
- A reading belongs to `MeterId + BillingPeriod`.
- `BillingPeriod` contains `Year + Month`.
- Consumption is derived from readings.
- Monthly consumption is `current reading - previous period reading`.
- A reading cannot go backwards for the same meter across consecutive periods.
- Use `decimal` for kWh and billing-style values.
- Use `HashSet<string>` for customer meters to prevent duplicate meter registration and double-counting.

## AI Working Rules

Before changing code:

1. Read the current files.
2. Identify the smallest change required by the new requirement.
3. Preserve existing public behavior unless the requirement says otherwise.
4. Add or update tests for every behavior change.
5. Run `dotnet test` before considering the work complete.

When requirements are ambiguous:

- State the assumption clearly.
- Prefer the simplest business rule that satisfies the current requirement.
- Ask a question only when choosing silently would be risky.

## Avoid

Avoid implementing future requirements before they are asked for:

- Reading corrections
- Estimated readings
- Invoice snapshots
- VAT or standing charges
- Meter replacement
- Move-in or move-out periods
- Thread-safe repositories
- Database persistence

The interview rewards adapting code as requirements evolve, not predicting everything upfront.

## Useful Prompt Pattern

```text
New requirement:
[paste requirement]

Given the existing code, propose the smallest change.
List the domain impact, application impact, infrastructure impact, and tests.
Do not rewrite the whole project.
```
