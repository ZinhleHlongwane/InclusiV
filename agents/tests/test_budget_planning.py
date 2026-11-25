from fastapi.testclient import TestClient

from platform_agents.agents.budget_planning import BudgetPlanningAgent
from platform_agents.main import app
from platform_agents.models import (
    BudgetStatusModel,
    CashFlowSummaryModel,
    CategorySpendingModel,
    FinancialSummaryModel,
)

client = TestClient(app)


def sample_summary(overbudget: bool = False) -> FinancialSummaryModel:
    spend = 1200.0 if overbudget else 650.0
    limit = 1000.0
    return FinancialSummaryModel(
        userId="user-123",
        year=2026,
        month=11,
        cashFlow=CashFlowSummaryModel(
            totalIncome=3000.0,
            totalExpenses=1800.0,
            netCashFlow=1200.0,
        ),
        spendingByCategory=[
            CategorySpendingModel(
                category="Food",
                totalAmount=spend,
                percentageOfTotalExpenses=60.0,
                transactionCount=8,
            ),
            CategorySpendingModel(
                category="Transport",
                totalAmount=300.0,
                percentageOfTotalExpenses=20.0,
                transactionCount=5,
            ),
        ],
        budgetStatuses=[
            BudgetStatusModel(
                budgetId="budget-1",
                category="Food",
                monthlyLimit=limit,
                currentSpend=spend,
                remainingBudget=limit - spend,
                percentageUsed=spend / limit * 100,
                health="Exceeded" if overbudget else "OnTrack",
                isExceeded=overbudget,
            )
        ],
    )


def test_plan_without_financial_history() -> None:
    plan = BudgetPlanningAgent.create_plan("new-user", None)
    assert plan.agent_id == "budget-planning"
    assert "first monthly budget" in plan.headline.lower()
    assert len(plan.next_steps) > 0


def test_plan_uses_budget_and_spending_data() -> None:
    plan = BudgetPlanningAgent.create_plan("user-123", sample_summary())
    assert plan.user_id == "user-123"
    assert "1 category budget" in plan.overview
    assert any("Food" in item for item in plan.recommendations)
    assert any("cash-flow margin" in item for item in plan.recommendations)


def test_plan_warns_when_category_is_over_budget() -> None:
    plan = BudgetPlanningAgent.create_plan("user-123", sample_summary(overbudget=True))
    assert plan.warnings
    assert any("Food" in warning and "200.00" in warning for warning in plan.warnings)
    assert "needs attention" in plan.headline.lower()


def test_budget_planning_endpoint_accepts_financial_context() -> None:
    payload = {
        "user_id": "user-123",
        "financial_context": sample_summary().model_dump(by_alias=True),
    }
    response = client.post("/v1/agents/budget-planning/plan", json=payload)
    assert response.status_code == 200
    assert response.json()["agent_id"] == "budget-planning"


def test_chat_routes_budget_requests_to_budget_agent() -> None:
    payload = {
        "user_id": "user-123",
        "message": "Can you help me plan my budget?",
        "financial_context": sample_summary().model_dump(by_alias=True),
    }
    response = client.post("/v1/chat", json=payload)
    assert response.status_code == 200
    data = response.json()
    assert data["agent_id"] == "budget-planning"
    assert data["agent_name"] == "Budget Planning Agent"
    assert data["data"]["agent_id"] == "budget-planning"


def test_agents_directory_lists_budget_planning_as_active() -> None:
    response = client.get("/v1/agents")
    assert response.status_code == 200
    agents = response.json()["agents"]
    budget_agent = next(agent for agent in agents if agent["id"] == "budget-planning")
    assert budget_agent["status"] == "active"
