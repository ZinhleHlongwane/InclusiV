# ADR 0001: Hybrid .NET Core and Python Agent Services

- Status: Accepted as initial direction
- Date: 2026-11-16

## Context
The target AI Engineer role emphasises C#/.NET backend engineering and agent development. Python is also a practical ecosystem for agent services and model integrations.

## Decision
Use ASP.NET Core as the trusted application backend and Python/FastAPI for the initial agent service, with explicit API contracts.

## Consequences
- Demonstrates C#/.NET backend development.
- Enables Python agent experimentation.
- Requires explicit service-to-service trust.
- Adds operational complexity.
- Agent services must not bypass backend authorisation or hold unrestricted database credentials.
