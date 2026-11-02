# Architecture Overview

## Style
Begin with a modular ASP.NET Core application and a separately hosted Python agent service. Avoid premature microservice decomposition. Split agents into independently deployed services when A2A interoperability or isolation justifies it.

## Components
- **Next.js frontend:** user interface; no database credentials or privileged model secrets.
- **ASP.NET Core:** authentication, authorisation, profiles, financial records, deterministic calculations, API contracts and trusted tool endpoints.
- **Python FastAPI agent service:** orchestration, specialist agent capabilities, model integration, context management, MCP and A2A integrations.
- **PostgreSQL:** system of record. Consider vector search only when approved-document retrieval is implemented.

## Agents
1. Financial Wellbeing: spending explanations, budgeting and savings guidance.
2. Insurance Guidance: insurance terms and approved policy information.
3. Payments and Transactions: sandbox workflows and transaction status.
4. Financial Research: source-backed financial resources.

The orchestrator routes and composes. It does not replace backend validation.

## Protocols
- REST: browser to core backend.
- MCP: agents access explicitly exposed tools and resources.
- A2A: independent agents exchange tasks and results.
- Database access: through trusted backend services or narrowly scoped tools, never unrestricted agent credentials.

## Azure target
Azure Container Registry, Azure Container Apps, Azure Database for PostgreSQL, Key Vault, managed identities, Azure Monitor/Application Insights and an Azure AI Foundry model deployment (subject to regional availability and cost). AKS is a later milestone.

## Trust boundaries
Frontend, user messages and retrieved documents are untrusted. Validate tool arguments server-side. Scope every data access to the authenticated user. Require human confirmation for sensitive actions.
