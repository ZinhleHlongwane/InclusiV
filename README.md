# Financial Inclusion Platform

An independently developed AI-enabled financial assistance platform for students and unemployed job seekers.

**Status: foundation scaffold.** This ZIP contains initial service skeletons, documentation, local development configuration and an Azure foundation template. It is not a completed application.

## Product direction
A platform to help people understand financial services, manage limited or irregular income and make informed decisions through coordinated specialist agents.

Planned agents:
1. Financial Wellbeing — spending analysis, budgeting and savings guidance.
2. Insurance Guidance — explain insurance concepts and approved policy information.
3. Payments and Transactions — sandbox payment workflows and transaction status.
4. Financial Research — source-backed research into relevant resources.

## Structure
- `frontend/web` — Next.js, React and TypeScript.
- `backend/dotnet` — ASP.NET Core API.
- `agents` — Python FastAPI agent-service foundation.
- `infra/azure` — initial Azure foundation template.
- `docs` — product, architecture, security and roadmap.

## Prerequisites
.NET 10 SDK, Python 3.11+, Node.js 22+, pnpm, Docker with Compose, and Azure CLI for cloud provisioning.

## Local setup
```bash
cp .env.example .env
docker compose up --build
```
- Frontend: http://localhost:3000
- .NET API: http://localhost:8080/health
- Agent service: http://localhost:8000/health
- PostgreSQL: localhost:5432

The scaffold starts without an LLM key. No model integration is implemented yet. Never commit `.env`.

## Important limitations
Authentication, financial records, calculations, functional AI agents, MCP, A2A and production Azure deployment are not implemented yet. No real banking, payment, insurance or financial-advice functionality is provided.

## Provenance and licensing
This is a new scaffold inspired by the financial-inclusion problem space explored in an earlier hackathon concept. It does not claim authorship over that earlier team's implementation. Review all reused code, assets and dependencies before inclusion. No project-wide open-source licence has been selected; see `docs/LICENSING.md`.
