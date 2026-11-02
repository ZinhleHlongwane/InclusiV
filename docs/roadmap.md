# Implementation Roadmap

## Stage 0 — Foundation
- [ ] Choose product name and identity.
- [ ] Review prior InclusiV code and asset provenance.
- [ ] Confirm reuse permissions and licensing.
- [ ] Finalise domain model and API contracts.
- [ ] Record architecture decisions.

## Stage 1 — Azure foundation
- [ ] Create resource group and budget alerts.
- [ ] Create container registry and Container Apps environment.
- [ ] Configure Key Vault and managed identity.
- [ ] Deploy minimal .NET health endpoint.
- [ ] Verify region, availability and costs before provisioning data services.

## Stage 2 — Core application
- [ ] Authentication and profiles.
- [ ] Income and expense records.
- [ ] Categories and validation.
- [ ] Budgets and savings goals.
- [ ] Deterministic calculations.
- [ ] Unit and integration tests.

## Stage 3 — First agent
- [ ] Financial Wellbeing Agent.
- [ ] Secure model configuration.
- [ ] Structured input/output.
- [ ] Read-only financial tools.
- [ ] Context management and evaluation scenarios.

## Stage 4 — Agents and A2A
- [ ] Insurance Guidance Agent.
- [ ] Payments and Transactions Agent.
- [ ] Financial Research Agent.
- [ ] Agent capability descriptions.
- [ ] A2A delegation, error handling and workflow tests.

## Stage 5 — MCP
- [ ] Financial tools MCP server.
- [ ] Approved research tools.
- [ ] Sandbox payment tools.
- [ ] MCP client integration.
- [ ] Permission and consent tests.

## Stage 6 — Security and observability
- [ ] Prompt-injection scenarios.
- [ ] Authorisation and user-isolation tests.
- [ ] Human approval workflow.
- [ ] Agent evaluation dataset.
- [ ] OpenTelemetry traces.
- [ ] Usage, latency and failure monitoring.

## Stage 7 — Delivery
- [ ] GitHub Actions CI.
- [ ] Azure deployment workflow.
- [ ] Operational documentation.
- [ ] Failure and recovery tests.
- [ ] Optional AKS deployment.

A feature is done only when documented, tested, integrated and demonstrable. Listing a protocol or dependency is not evidence it has been implemented.
