from enum import StrEnum
from typing import Callable

from platform_agents.agents.budget_planning import BudgetPlanningAgent
from platform_agents.agents.savings_goals import SavingsGoalsAgent
from platform_agents.agents.wellbeing import FinancialWellbeingAgent
from platform_agents.client import BackendApiClient
from platform_agents.models import ChatRequest, ChatResponse, FinancialSummaryModel
from platform_agents.intent_planner import IntentExecutionPlan, IntentPlanner


class AgentIntent(StrEnum):
    PAYMENTS = "payments-transactions"
    BUDGET = "budget-planning"
    SAVINGS = "savings-goals"
    WELLBEING = "financial-wellbeing"
    BUDGET_AND_SAVINGS = "budget-and-savings"


class AgentOrchestrator:
    """Plan and coordinate one or more specialist agents for a chat request.

    Routing rules are deterministic and deliberately separated from execution.
    A future model-assisted planner can replace classify_intents without
    changing the specialist-agent response contracts.
    """

    BUDGET_TERMS = (
        "budget", "spending limit", "category limit", "manage my spending",
        "overspending", "over budget",
    )
    SAVINGS_TERMS = (
        "savings goal", "saving goal", "savings progress", "track my savings",
        "my savings goals", "saving for", "reach my goal",
    )
    WELLBEING_TERMS = (
        "financial wellbeing", "financial health", "cash flow", "financial situation",
        "financial overview", "spending breakdown", "can i afford", "affordability",
    )
    PAYMENT_TERMS = (
        "simulate payment", "simulate a payment", "make a payment", "pay ", "send money", "transfer money",
    )

    def __init__(
        self,
        backend_client: BackendApiClient | None = None,
        intent_planner: IntentPlanner | None = None,
    ) -> None:
        self.backend_client = backend_client or BackendApiClient()
        self.intent_planner = intent_planner or IntentPlanner()

    @classmethod
    def classify_intents(cls, message: str) -> list[AgentIntent]:
        """Return every specialist intent identified in a message."""
        text = message.lower()

        # Payment language is handled separately so a financial review request
        # never accidentally triggers a payment workflow.
        if any(term in text for term in cls.PAYMENT_TERMS):
            return [AgentIntent.PAYMENTS]

        intents: list[AgentIntent] = []
        if any(term in text for term in cls.BUDGET_TERMS):
            intents.append(AgentIntent.BUDGET)
        if any(term in text for term in cls.SAVINGS_TERMS):
            intents.append(AgentIntent.SAVINGS)
        if any(term in text for term in cls.WELLBEING_TERMS):
            intents.append(AgentIntent.WELLBEING)

        return intents or [AgentIntent.WELLBEING]

    @classmethod
    def classify_intent(cls, message: str) -> AgentIntent:
        """Compatibility helper for callers that expect one intent value."""
        intents = cls.classify_intents(message)
        if intents == [AgentIntent.BUDGET, AgentIntent.SAVINGS]:
            return AgentIntent.BUDGET_AND_SAVINGS
        return intents[0]

    async def _get_summary(self, request: ChatRequest) -> FinancialSummaryModel | None:
        if request.financial_context is not None:
            return request.financial_context
        return await self.backend_client.get_financial_summary(request.user_id)

    @staticmethod
    def _format_budget(plan) -> str:
        parts = [plan.headline, plan.overview]
        if plan.warnings:
            parts.extend(["Warnings:", *(f"- {item}" for item in plan.warnings)])
        if plan.recommendations:
            parts.extend(["Recommendations:", *(f"- {item}" for item in plan.recommendations[:3])])
        if plan.next_steps:
            parts.extend(["Next step:", f"- {plan.next_steps[0]}"])
        return "\n".join(parts)

    @staticmethod
    def _format_savings(review) -> str:
        parts = [review.headline, review.overview]
        if review.recommendations:
            parts.extend(["Recommendations:", *(f"- {item}" for item in review.recommendations[:3])])
        if review.next_steps:
            parts.extend(["Next step:", f"- {review.next_steps[0]}"])
        return "\n".join(parts)

    @staticmethod
    def _format_wellbeing(advice) -> str:
        parts = [advice.headline]
        if advice.vulnerability_alerts:
            parts.extend(["Important Alerts:", *(f"- {item}" for item in advice.vulnerability_alerts)])
        if advice.insights:
            parts.extend(["Key Insights:", *(f"- {item}" for item in advice.insights[:2])])
        if advice.actionable_steps:
            parts.extend(["Recommended Next Step:", f"- {advice.actionable_steps[0]}"])
        return "\n".join(parts)

    async def handle(self, request: ChatRequest) -> ChatResponse:
        execution_plan = await self.intent_planner.create_plan(request.message)
        if execution_plan.needs_clarification:
            return ChatResponse(
                agent_id="agent-orchestrator",
                agent_name="InclusiV Agent Orchestrator",
                reply=execution_plan.clarification_question or "Could you clarify what kind of financial help you need?",
                suggestions=[
                    "Review my monthly budget",
                    "Review my savings goals",
                    "Give me an overall financial health review",
                ],
                data={"execution_plan": execution_plan.model_dump()},
            )

        intents = [AgentIntent(intent) for intent in execution_plan.intents]

        if intents == [AgentIntent.PAYMENTS]:
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
                data={"action": "simulate_payment_prompt", "execution_plan": execution_plan.model_dump()},
            )

        summary = await self._get_summary(request)
        # Each specialist is registered once here. Adding a specialist means
        # adding its intent and executor, not another nested routing branch.
        executors: dict[AgentIntent, Callable] = {
            AgentIntent.BUDGET: lambda: BudgetPlanningAgent.create_plan(
                user_id=request.user_id, summary=summary, query=request.message
            ),
            AgentIntent.SAVINGS: lambda: SavingsGoalsAgent.review_goals(
                user_id=request.user_id, summary=summary, query=request.message
            ),
            AgentIntent.WELLBEING: lambda: FinancialWellbeingAgent.generate_advice(
                user_id=request.user_id, summary=summary, query=request.message
            ),
        }
        names = {
            AgentIntent.BUDGET: "Budget Planning Agent",
            AgentIntent.SAVINGS: "Savings & Goals Agent",
            AgentIntent.WELLBEING: "Financial Wellbeing Agent",
        }
        formatters = {
            AgentIntent.BUDGET: self._format_budget,
            AgentIntent.SAVINGS: self._format_savings,
            AgentIntent.WELLBEING: self._format_wellbeing,
        }

        results: list[tuple[AgentIntent, object]] = []
        failures: list[str] = []
        for intent in intents:
            try:
                results.append((intent, executors[intent]()))
            except Exception:
                # Keep successful specialist findings if another specialist
                # fails; expose the failure without leaking internal details.
                failures.append(names[intent])

        if not results:
            return ChatResponse(
                agent_id="agent-orchestrator",
                agent_name="InclusiV Agent Orchestrator",
                reply="I couldn't complete the requested financial review just now. Please try again.",
                suggestions=["Try your request again", "Review my financial wellbeing"],
                data={"delegated_agents": [], "failed_agents": failures},
            )

        if len(results) == 1 and not failures:
            intent, result = results[0]
            return ChatResponse(
                agent_id=result.agent_id,
                agent_name=names[intent],
                reply=formatters[intent](result),
                suggestions=self._suggestions(intent),
                data={**result.model_dump(), "execution_plan": execution_plan.model_dump()},
            )

        reply_parts = ["InclusiV coordinated the following specialist reviews."]
        delegated_agents: list[str] = []
        agent_results: dict[str, dict] = {}
        for intent, result in results:
            delegated_agents.append(result.agent_id)
            agent_results[result.agent_id] = result.model_dump()
            reply_parts.extend([f"## {names[intent]}", formatters[intent](result)])

        if failures:
            reply_parts.append(
                "I couldn't complete these specialist reviews: " + ", ".join(failures) + "."
            )

        return ChatResponse(
            agent_id="agent-orchestrator",
            agent_name="InclusiV Agent Orchestrator",
            reply="\n\n".join(reply_parts),
            suggestions=[
                "Review my monthly budget",
                "Review my savings progress",
                "Give me an overall financial health review",
            ],
            data={
                "orchestrator": "agent-orchestrator",
                "delegated_agents": delegated_agents,
                "results": agent_results,
                "failed_agents": failures,
                "execution_plan": execution_plan.model_dump(),
            },
        )

    @staticmethod
    def _suggestions(intent: AgentIntent) -> list[str]:
        if intent == AgentIntent.BUDGET:
            return ["Review my monthly budget", "How can I reduce my spending?", "Explain my spending breakdown"]
        if intent == AgentIntent.SAVINGS:
            return ["Review my savings progress", "Which savings goal should I prioritise?", "How much should I save each month?"]
        return ["Explain my spending breakdown", "How can I save more this month?", "Check my budget limits"]
