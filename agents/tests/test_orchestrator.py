import asyncio

from platform_agents.models import (
    ChatRequest,
    FinancialSummaryModel,
    SavingsGoalProgressModel,
)
from platform_agents.orchestrator import AgentIntent, AgentOrchestrator


def test_intent_classifier_selects_specialist_agents() -> None:
    assert AgentOrchestrator.classify_intent("Please help me plan my budget") == AgentIntent.BUDGET
    assert AgentOrchestrator.classify_intent("Can you review my savings goals?") == AgentIntent.SAVINGS
    assert AgentOrchestrator.classify_intent("Can I simulate payment for groceries?") == AgentIntent.PAYMENTS
    assert AgentOrchestrator.classify_intent("How can I improve my financial wellbeing?") == AgentIntent.WELLBEING


def test_intent_classifier_detects_compound_budget_and_savings_request() -> None:
    intent = AgentOrchestrator.classify_intent(
        "Review my budget and tell me how my savings goals are progressing"
    )
    assert intent == AgentIntent.BUDGET_AND_SAVINGS


def test_orchestrator_delegates_compound_request_to_two_agents() -> None:
    summary = FinancialSummaryModel(
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
    orchestrator = AgentOrchestrator()
    response = asyncio.run(
        orchestrator.handle(
            ChatRequest(
                user_id="user-123",
                message="Review my budget and savings goals",
                financial_context=summary,
            )
        )
    )

    assert response.agent_id == "agent-orchestrator"
    assert response.agent_name == "InclusiV Agent Orchestrator"
    assert response.data is not None
    assert response.data["delegated_agents"] == ["budget-planning", "savings-goals"]
    assert "budget" in response.data
    assert "savings" in response.data


def test_orchestrator_routes_budget_request_to_budget_agent() -> None:
    response = __import__("asyncio").run(
        AgentOrchestrator().handle(
            ChatRequest(
                user_id="user-123",
                message="Help me plan my budget",
                financial_context=FinancialSummaryModel(userId="user-123"),
            )
        )
    )
    assert response.agent_id == "budget-planning"


def test_orchestrator_routes_savings_request_to_savings_agent() -> None:
    response = __import__("asyncio").run(
        AgentOrchestrator().handle(
            ChatRequest(
                user_id="user-123",
                message="Review my savings goals",
                financial_context=FinancialSummaryModel(userId="user-123"),
            )
        )
    )
    assert response.agent_id == "savings-goals"
