from enum import StrEnum

from platform_agents.agents.budget_planning import BudgetPlanningAgent
from platform_agents.agents.savings_goals import SavingsGoalsAgent
from platform_agents.agents.wellbeing import FinancialWellbeingAgent
from platform_agents.client import BackendApiClient
from platform_agents.models import ChatRequest, ChatResponse, FinancialSummaryModel


class AgentIntent(StrEnum):
    PAYMENTS = "payments-transactions"
    BUDGET = "budget-planning"
    SAVINGS = "savings-goals"
    WELLBEING = "financial-wellbeing"
    BUDGET_AND_SAVINGS = "budget-and-savings"


class AgentOrchestrator:
    """Coordinates specialist agents for incoming chat requests.

    Intent classification is deterministic for now. This keeps routing
    predictable and testable while leaving a clear seam for a future
    model-assisted planner.
    """

    def __init__(self, backend_client: BackendApiClient | None = None) -> None:
        self.backend_client = backend_client or BackendApiClient()

    @staticmethod
    def classify_intent(message: str) -> AgentIntent:
        text = message.lower()

        payment_terms = ("simulate payment", "make a payment", "pay ", "send money", "transfer money")
        if any(term in text for term in payment_terms):
            return AgentIntent.PAYMENTS

        budget_terms = ("budget", "spending limit", "category limit", "manage my spending")
        savings_terms = ("savings goal", "saving goal", "savings progress", "track my savings", "my savings goals", "saving for", "reach my goal")

        has_budget_intent = any(term in text for term in budget_terms)
        has_savings_intent = any(term in text for term in savings_terms)

        if has_budget_intent and has_savings_intent:
            return AgentIntent.BUDGET_AND_SAVINGS
        if has_budget_intent:
            return AgentIntent.BUDGET
        if has_savings_intent:
            return AgentIntent.SAVINGS
        return AgentIntent.WELLBEING

    async def _get_summary(
        self,
        request: ChatRequest,
    ) -> FinancialSummaryModel | None:
        if request.financial_context is not None:
            return request.financial_context
        return await self.backend_client.get_financial_summary(request.user_id)

    async def handle(self, request: ChatRequest) -> ChatResponse:
        intent = self.classify_intent(request.message)

        if intent == AgentIntent.PAYMENTS:
            return ChatResponse(
                agent_id="payments-transactions",
                agent_name="Payments and Transactions Agent",
                reply=(
                    "I can help you simulate sandbox payments to test your transactions safely. "
                    "You can specify a recipient and an amount to preview how it will affect your category budget."
                ),
                suggestions=[
                    "Simulate R250 payment for Groceries",
                    "Simulate R50 payment for Transport",
                    "Review remaining category budgets",
                ],
                data={"action": "simulate_payment_prompt"},
            )

        summary = await self._get_summary(request)

        if intent == AgentIntent.BUDGET:
            plan = BudgetPlanningAgent.create_plan(
                user_id=request.user_id,
                summary=summary,
                query=request.message,
            )
            reply_parts = [plan.headline, plan.overview]
            if plan.warnings:
                reply_parts.extend(["Warnings:", *(f"- {item}" for item in plan.warnings)])
            if plan.recommendations:
                reply_parts.extend(["Recommendations:", *(f"- {item}" for item in plan.recommendations[:3])])
            if plan.next_steps:
                reply_parts.extend(["Next step:", f"- {plan.next_steps[0]}"])
            return ChatResponse(
                agent_id=plan.agent_id,
                agent_name="Budget Planning Agent",
                reply="\\n".join(reply_parts),
                suggestions=[
                    "Review my monthly budget",
                    "How can I reduce my spending?",
                    "Explain my spending breakdown",
                ],
                data=plan.model_dump(),
            )

        if intent == AgentIntent.SAVINGS:
            review = SavingsGoalsAgent.review_goals(
                user_id=request.user_id,
                summary=summary,
                query=request.message,
            )
            reply_parts = [review.headline, review.overview]
            if review.recommendations:
                reply_parts.extend(["Recommendations:", *(f"- {item}" for item in review.recommendations[:3])])
            if review.next_steps:
                reply_parts.extend(["Next step:", f"- {review.next_steps[0]}"])
            return ChatResponse(
                agent_id=review.agent_id,
                agent_name="Savings & Goals Agent",
                reply="\\n".join(reply_parts),
                suggestions=[
                    "Review my savings progress",
                    "Which savings goal should I prioritise?",
                    "How much should I save each month?",
                ],
                data=review.model_dump(),
            )

        if intent == AgentIntent.BUDGET_AND_SAVINGS:
            plan = BudgetPlanningAgent.create_plan(
                user_id=request.user_id,
                summary=summary,
                query=request.message,
            )
            review = SavingsGoalsAgent.review_goals(
                user_id=request.user_id,
                summary=summary,
                query=request.message,
            )
            reply_parts = [
                "Here's a combined review from your Budget Planning and Savings & Goals agents.",
                plan.headline,
                plan.overview,
            ]
            if plan.warnings:
                reply_parts.extend(["Budget warnings:", *(f"- {item}" for item in plan.warnings)])
            if plan.recommendations:
                reply_parts.extend(["Budget recommendations:", *(f"- {item}" for item in plan.recommendations[:2])])
            reply_parts.extend([review.headline, review.overview])
            if review.recommendations:
                reply_parts.extend(["Savings recommendations:", *(f"- {item}" for item in review.recommendations[:2])])
            return ChatResponse(
                agent_id="agent-orchestrator",
                agent_name="InclusiV Agent Orchestrator",
                reply="\\n".join(reply_parts),
                suggestions=[
                    "How should I balance saving and spending?",
                    "Review my monthly budget",
                    "Review my savings progress",
                ],
                data={
                    "orchestrator": "agent-orchestrator",
                    "delegated_agents": [plan.agent_id, review.agent_id],
                    "budget": plan.model_dump(),
                    "savings": review.model_dump(),
                },
            )

        advice = FinancialWellbeingAgent.generate_advice(
            user_id=request.user_id,
            summary=summary,
            query=request.message,
        )
        reply_parts = [advice.headline + "\\n"]
        if advice.vulnerability_alerts:
            reply_parts.extend(["Important Alerts:", *(f"- {item}" for item in advice.vulnerability_alerts), ""])
        if advice.insights:
            reply_parts.extend(["Key Insights:", *(f"- {item}" for item in advice.insights[:2]), ""])
        if advice.actionable_steps:
            reply_parts.extend(["Recommended Next Step:", f"- {advice.actionable_steps[0]}"])

        return ChatResponse(
            agent_id=advice.agent_id,
            agent_name="Financial Wellbeing Agent",
            reply="\\n".join(reply_parts),
            suggestions=[
                "Explain my spending breakdown",
                "How can I save more this month?",
                "Check my budget limits",
            ],
            data=advice.model_dump(),
        )
