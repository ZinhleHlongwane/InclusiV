# Product Requirements

## Vision
Help students and unemployed job seekers understand financial services, manage limited or irregular income and make informed financial decisions.

## Target users
- Students managing allowances, bursaries, family support, part-time income and study costs.
- Unemployed job seekers managing limited support, occasional income, transport, connectivity, food and job-search expenses.

## Planned capabilities
### Financial wellbeing
Record income and expenses, categorise transactions, view spending summaries, create budgets and savings goals, and receive explanations grounded in recorded data.

### Insurance guidance
Explain common terms, summarise approved policy documents and help users prepare questions for providers. Do not make binding coverage decisions.

### Payments and transactions
Explain payment options, integrate with sandbox providers, show transaction status and require explicit confirmation for consequential actions.

### Financial research
Search approved sources, return source-backed summaries and communicate uncertainty.

## MVP priority
1. User accounts and access control.
2. Income and expense records.
3. Deterministic financial summaries and calculations.
4. Budgets and savings goals.
5. One useful Financial Wellbeing Agent workflow.
6. Secure tool access and traceable agent calls.

## Exclusions
No real bank connections, real money transfers, lending, credit scoring, insurance sales/underwriting or regulated financial advice.

## Acceptance criteria
- Users can access only their own records.
- Financial totals are calculated by deterministic backend logic.
- Invalid amounts and dates are rejected.
- Agent tools enforce user and operation permissions.
- AI distinguishes retrieved facts from suggestions.
- Sensitive actions require explicit confirmation.
- Core logic and security boundaries have automated tests.
