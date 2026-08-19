# Decisions

This file records the current domain and design decisions so future changes have context.

## Current Decisions

- The current bounded context is `Metering`.
- The code uses a small DDD-inspired structure: `Domain`, `Application`, and `Infrastructure`.
- Storage is in-memory dictionaries.
- Meter readings are cumulative values.
- A meter reading is identified by `MeterId + BillingPeriod`.
- `BillingPeriod` contains `Year + Month`.
- Month-only methods are preserved for compatibility and use a default year internally.
- Consumption is derived, not stored.
- Meter consumption is calculated from the current reading minus the previous period reading.
- Period consumption is calculated from a start reading and an end reading.
- Monthly consumption can be returned as a `ConsumptionResult` when callers need status instead of exceptions.
- Duplicate readings for the same meter and billing period are rejected.
- Reading corrections are explicit and replace an existing reading only when one already exists.
- Customer consumption is the sum of consumption across that customer's registered meters.
- Customer meters use a `HashSet<string>` to avoid duplicate registration and double-counting.
- Invoice total is currently simple: `consumption * unit price`.

## Intentional Non-Decisions

These are not implemented yet because no requirement needs them:

- Measured vs estimated readings
- Invoice idempotency
- Invoice snapshot lines
- VAT, standing charges, discounts, or regulated fees
- Meter replacement
- Customer move-in or move-out dates
- Thread-safe repository access
- Database persistence

## Guiding Principle

Implement the smallest behavior that satisfies the current requirement. Add complexity only when a requirement creates real pressure for it.
