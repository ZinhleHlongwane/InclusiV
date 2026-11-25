from fastapi.testclient import TestClient

from platform_agents.agents.savings_goals import SavingsGoalsAgent
from platform_agents.main import app
from platform_agents.models import FinancialSummaryModel, SavingsGoalProgressModel

client = TestClient(app)


def sample_summary() -> FinancialSummaryModel:
    return FinancialSummaryModel(
        userId="user-123",
        year=2026,
        month=11,
        savingsGoals=[
            SavingsGoalProgressModel(
                goalId="goal-1",
                name="Emergency Fund",
                targetAmount=5000.0,
                currentAmount=2000.0,
                remainingAmount=3000.0,
                percentageAchieved=40.0,
                targetDate="2027-06-30",
                recommendedMonthlySavings=500.0,
                isCompleted=False,
            ),
            SavingsGoalProgressModel(
                goalId="goal-2",
                name="Laptop",
                targetAmount=10000.0,
                currentAmount=10000.0,
                remainingAmount=0.0,
                percentageAchieved=100.0,
                targetDate=None,
                recommendedMonthlySavings=None,
                isCompleted=True,
            ),
        ],
    )


def test_review_without_financial_history() -> None:
    review = SavingsGoalsAgent.review_goals("new-user", None)
    assert review.agent_id == "savings-goals"
    assert "start with a savings goal" in review.headline.lower()
    assert review.next_steps


def test_review_without_existing_goals() -> None:
    review = SavingsGoalsAgent.review_goals("user-123", FinancialSummaryModel(userId="user-123"))
    assert "haven't set up" in review.headline.lower()
    assert any("first savings goal" in step.lower() for step in review.next_steps)


def test_review_reports_progress_and_completed_goals() -> None:
    review = SavingsGoalsAgent.review_goals("user-123", sample_summary())
    assert len(review.insights) == 2
    emergency = next(item for item in review.insights if item.goal_name == "Emergency Fund")
    assert emergency.percentage_achieved == 40.0
    assert emergency.remaining_amount == 3000.0
    assert any("R500.00 per month" in item for item in review.recommendations)
    assert any("Laptop" in item and "completed" in item for item in review.recommendations)
    assert "1 completed" in review.overview


def test_savings_goals_endpoint_accepts_financial_context() -> None:
    response = client.post(
        "/v1/agents/savings-goals/review",
        json={
            "user_id": "user-123",
            "financial_context": sample_summary().model_dump(by_alias=True),
        },
    )
    assert response.status_code == 200
    assert response.json()["agent_id"] == "savings-goals"


def test_chat_routes_savings_questions_to_savings_agent() -> None:
    response = client.post(
        "/v1/chat",
        json={
            "user_id": "user-123",
            "message": "Can you review my savings goals?",
            "financial_context": sample_summary().model_dump(by_alias=True),
        },
    )
    assert response.status_code == 200
    data = response.json()
    assert data["agent_id"] == "savings-goals"
    assert data["agent_name"] == "Savings & Goals Agent"
    assert data["data"]["agent_id"] == "savings-goals"


def test_agents_directory_lists_savings_agent_as_active() -> None:
    response = client.get("/v1/agents")
    assert response.status_code == 200
    agents = response.json()["agents"]
    savings_agent = next(agent for agent in agents if agent["id"] == "savings-goals")
    assert savings_agent["status"] == "active"
