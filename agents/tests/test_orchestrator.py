import asyncio

from platform_agents.agents.budget_planning import BudgetPlanningAgent
from platform_agents.models import (
    ChatRequest,
    FinancialSummaryModel,
    SavingsGoalProgressModel,
)
from platform_agents.orchestrator import AgentIntent, AgentOrchestrator


def sample_summary() -> FinancialSummaryModel:
    return FinancialSummaryModel(
        userId="user-123",
        year=2026,
        month=11,
        savingsGoals=[
            SavingsGoalProgressModel(
                goalId="goal-1",
                name="Emergency Fund",
                targetAmount=1000.0,
                currentAmount=250.0,
                remainingAmount=750.0,
                percentageAchieved=25.0,
                recommendedMonthlySavings=125.0,
                isCompleted=False,
            )
        ],
    )


def test_intent_classifier_selects_specialist_agents() -> None:
    assert AgentOrchestrator.classify_intents("Please help me plan my budget") == [AgentIntent.BUDGET]
    assert AgentOrchestrator.classify_intents("Can you review my savings goals?") == [AgentIntent.SAVINGS]
    assert AgentOrchestrator.classify_intents("Can I simulate payment for groceries?") == [AgentIntent.PAYMENTS]
    assert AgentOrchestrator.classify_intents("How can I improve my financial wellbeing?") == [AgentIntent.WELLBEING]


def test_intent_classifier_detects_multiple_specialists() -> None:
    intents = AgentOrchestrator.classify_intents(
        "Review my budget and tell me how my savings goals and financial health are progressing"
    )
    assert intents == [AgentIntent.BUDGET, AgentIntent.SAVINGS, AgentIntent.WELLBEING]


def test_payment_intent_is_kept_separate_from_other_financial_reviews() -> None:
    intents = AgentOrchestrator.classify_intents(
        "Simulate a payment and review my budget and savings goals"
    )
    assert intents == [AgentIntent.PAYMENTS]


def test_orchestrator_delegates_compound_request_to_multiple_agents() -> None:
    response = asyncio.run(
        AgentOrchestrator().handle(
            ChatRequest(
                user_id="user-123",
                message="Review my budget and savings goals",
                financial_context=sample_summary(),
            )
        )
    )

    assert response.agent_id == "agent-orchestrator"
    assert response.agent_name == "MaliNovation Agent Orchestrator"
    assert response.data is not None
    assert response.data["delegated_agents"] == ["budget-planning", "savings-goals"]
    assert "budget-planning" in response.data["results"]
    assert "savings-goals" in response.data["results"]


def test_orchestrator_can_coordinate_three_specialists() -> None:
    response = asyncio.run(
        AgentOrchestrator().handle(
            ChatRequest(
                user_id="user-123",
                message="Review my budget, savings goals and overall financial health",
                financial_context=sample_summary(),
            )
        )
    )
    assert response.agent_id == "agent-orchestrator"
    assert response.data is not None
    assert response.data["delegated_agents"] == [
        "budget-planning",
        "savings-goals",
        "financial-wellbeing",
    ]


def test_orchestrator_routes_single_budget_request_to_budget_agent() -> None:
    response = asyncio.run(
        AgentOrchestrator().handle(
            ChatRequest(
                user_id="user-123",
                message="Help me plan my budget",
                financial_context=sample_summary(),
            )
        )
    )
    assert response.agent_id == "budget-planning"


def test_orchestrator_routes_single_savings_request_to_savings_agent() -> None:
    response = asyncio.run(
        AgentOrchestrator().handle(
            ChatRequest(
                user_id="user-123",
                message="Review my savings goals",
                financial_context=sample_summary(),
            )
        )
    )
    assert response.agent_id == "savings-goals"


def test_orchestrator_keeps_successful_agent_result_if_another_fails(monkeypatch) -> None:
    def fail_plan(*args, **kwargs):
        raise RuntimeError("simulated specialist failure")

    monkeypatch.setattr(BudgetPlanningAgent, "create_plan", fail_plan)
    response = asyncio.run(
        AgentOrchestrator().handle(
            ChatRequest(
                user_id="user-123",
                message="Review my budget and savings goals",
                financial_context=sample_summary(),
            )
        )
    )

    assert response.agent_id == "agent-orchestrator"
    assert response.data is not None
    assert response.data["delegated_agents"] == ["savings-goals"]
    assert response.data["failed_agents"] == ["Budget Planning Agent"]


def test_orchestrator_returns_clarification_without_running_agents() -> None:
    class ClarifyingPlanner:
        async def create_plan(self, message: str):
            from platform_agents.intent_planner import IntentExecutionPlan

            return IntentExecutionPlan(
                intents=[],
                confidence=0.4,
                needs_clarification=True,
                clarification_question="Are you asking about your budget or savings?",
                rationale="The request is ambiguous.",
            )

    response = asyncio.run(
        AgentOrchestrator(intent_planner=ClarifyingPlanner()).handle(
            ChatRequest(
                user_id="user-123",
                message="Can you help me with it?",
                financial_context=sample_summary(),
            )
        )
    )

    assert response.agent_id == "agent-orchestrator"
    assert response.reply == "Are you asking about your budget or savings?"
    assert response.data is not None
    assert response.data["execution_plan"]["needs_clarification"] is True
