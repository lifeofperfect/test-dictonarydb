# Interview AI Workflow

Use AI as a careful pair-programmer, not as autopilot.

## Opening

Tell the interviewer:

> I will use AI to help clarify requirements, propose tests, and generate candidate code. I will review the output and own the design decisions.

## Starting Prompt

```text
You are assisting me in a senior C# coding interview.

Context:
We are building an in-memory backend service for a metering operator.
The service handles meter readings and consumption.
Requirements are given step by step.
Months are simple business periods, not streaming/time-series windows.
No database or external framework unless explicitly required.
Use C# and xUnit.
Prefer simple DDD: Domain, Application, Infrastructure.
Avoid unnecessary folders and over-engineering.

Your role:
Help me understand requirements, identify assumptions, propose small increments, and suggest tests.
Do not generate large rewrites unless I ask.
When coding, preserve existing behavior and explain trade-offs.

First task:
Explain the likely domain model, the first tests to write, and the smallest implementation step.
```

## Good First Questions

Ask only the questions that affect the immediate code:

- Are meter readings cumulative?
- Is consumption current reading minus previous reading?
- Is month alone enough, or do we need year and month?
- What should happen when the previous reading is missing?
- Should duplicate readings be rejected, overwritten, or treated as corrections?

## Iteration Loop

For each new requirement:

1. Restate the requirement in simple terms.
2. Name the business rule.
3. Add the smallest test.
4. Implement the smallest change.
5. Run the tests.
6. Explain the trade-off.

Use this rule:

```text
AI proposes.
You decide.
Tests prove.
```

## Senior Interview Posture

Good explanation:

> I am keeping the first version intentionally small. I am modelling the business concept first, writing behavior-focused tests, and keeping storage in memory. If the requirements introduce billing, corrections, or customer ownership, I will add those as separate rules rather than guessing upfront.

## Red Flags

Avoid starting with:

- EF Core
- Web API
- MediatR
- CQRS folder explosion
- Generic repository abstractions
- Complex date/time logic
- Features not requested yet
