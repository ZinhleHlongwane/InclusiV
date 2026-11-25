# MaliNovation Implementation Roadmap

## Stage 0 — Foundation
- [x] Choose the MaliNovation product name and identity.
- [x] Review the existing codebase and record architecture decisions.
- [ ] Confirm reuse permissions and licensing for archived prototype assets.
- [x] Establish the initial domain model and API contracts.

## Stage 1 — Azure foundation
- [ ] Create resource group and budget alerts.
- [ ] Create container registry and Container Apps environment.
- [ ] Configure Key Vault and managed identity.
- [ ] Deploy minimal .NET health endpoint.
- [ ] Verify region, availability and costs before provisioning data services.
- [ ] Configure and verify the optional Azure model deployment.

## Stage 2 — Core application
- [ ] Authentication and profiles.
- [x] Income and expense records.
- [x] Categories and validation.
- [x] Budgets and savings goals.
- [x] Deterministic calculations.
- [ ] Expand unit and integration tests.

## Stage 3 — First agent
- [x] Financial Wellbeing Agent.
- [x] Structured input/output.
- [x] Read-only financial summary access.
- [ ] Secure live model configuration and evaluation scenarios.

## Stage 4 — Agents and orchestration
- [x] Budget Planning Agent.
- [x] Savings & Goals Agent.
- [x] Payments and Transactions sandbox workflow.
- [x] Multi-agent orchestration and partial-failure handling.
- [ ] Insurance Guidance Agent.
- [ ] Financial Research Agent.
- [ ] A2A delegation and interoperability tests.

## Stage 5 — MCP
- [ ] Financial tools MCP server.
- [ ] Approved research tools.
- [ ] Sandbox payment tools.
- [ ] MCP client integration.
- [ ] Permission and consent tests.

## Stage 6 — Security and observability
- [ ] Authentication, authorisation and user-isolation tests.
- [ ] Prompt-injection scenarios.
- [ ] Human approval workflow.
- [ ] Agent evaluation dataset.
- [ ] OpenTelemetry traces.
- [ ] Usage, latency and failure monitoring.

## Stage 7 — Delivery
- [x] GitHub Actions CI foundation.
- [ ] Azure deployment workflow.
- [ ] Operational documentation.
- [ ] Failure and recovery tests.
- [ ] Optional AKS deployment.

A feature is done only when documented, tested, integrated and demonstrable. Listing a protocol or dependency is not evidence it has been implemented.
