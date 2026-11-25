from fastapi.testclient import TestClient
from platform_agents.agents.wellbeing import FinancialWellbeingAgent
from platform_agents.main import app
from platform_agents.models import (
    BudgetStatusModel,
    CashFlowSummaryModel,
    CategorySpendingModel,
    FinancialSummaryModel,
    SavingsGoalProgressModel,
)

client = TestClient(app)


def create_sample_summary(
    net_cash_flow: float = 500.0,
    total_income: float = 3000.0,
    total_expenses: float = 2500.0,
    is_overbudget: bool = False,
) -> FinancialSummaryModel:
    budget_health = "Exceeded" if is_overbudget else "OnTrack"
    current_spend = 1200.0 if is_overbudget else 800.0

    return FinancialSummaryModel(
        userId="user-123",
        year=2026,
        month=11,
        totalBalance=net_cash_flow,
        cashFlow=CashFlowSummaryModel(
            totalIncome=total_income,
            totalExpenses=total_expenses,
            netCashFlow=net_cash_flow,
        ),
        spendingByCategory=[
            CategorySpendingModel(
                category="Food",
                totalAmount=current_spend,
                percentageOfTotalExpenses=50.0,
                transactionCount=5,
            ),
            CategorySpendingModel(
                category="Transport",
                totalAmount=400.0,
                percentageOfTotalExpenses=20.0,
                transactionCount=4,
            ),
        ],
        budgetStatuses=[
            BudgetStatusModel(
                budgetId="b-1",
                category="Food",
                monthlyLimit=1000.0,
                currentSpend=current_spend,
                remainingBudget=1000.0 - current_spend,
                percentageUsed=(current_spend / 1000.0) * 100.0,
                health=budget_health,
                isExceeded=is_overbudget,
            )
        ],
        savingsGoals=[
            SavingsGoalProgressModel(
                goalId="g-1",
                name="Emergency Fund",
                targetAmount=1000.0,
                currentAmount=250.0,
                remainingAmount=750.0,
                percentageAchieved=25.0,
                targetDate="2027-02-01",
                recommendedMonthlySavings=250.0,
                isCompleted=False,
            )
        ],
        accountCount=1,
        transactionCount=9,
        hasNegativeCashFlow=net_cash_flow < 0,
        hasOverbudgetAlerts=is_overbudget,
        totalSavings=250.0,
    )


def test_advice_with_healthy_cash_flow() -> None:
    summary = create_sample_summary(net_cash_flow=500.0, is_overbudget=False)
    advice = FinancialWellbeingAgent.generate_advice("user-123", summary)

    assert "Positive Cash Flow" in advice.headline
    assert len(advice.vulnerability_alerts) == 0
    assert len(advice.insights) > 0
    assert any("surplus" in insight.lower() for insight in advice.insights)


def test_advice_with_negative_cash_flow() -> None:
    summary = create_sample_summary(
        net_cash_flow=-600.0, total_income=2000.0, total_expenses=2600.0
    )
    advice = FinancialWellbeingAgent.generate_advice("user-123", summary)

    assert "Alert" in advice.headline
    assert len(advice.vulnerability_alerts) > 0
    assert any("negative cash flow" in alert.lower() for alert in advice.vulnerability_alerts)


def test_advice_with_overbudget_category() -> None:
    summary = create_sample_summary(net_cash_flow=100.0, is_overbudget=True)
    advice = FinancialWellbeingAgent.generate_advice("user-123", summary)

    assert len(advice.vulnerability_alerts) > 0
    assert any("over-budget alert" in alert.lower() for alert in advice.vulnerability_alerts)
    assert any("food" in rec.lower() for rec in advice.budget_recommendations)


def test_advice_with_no_history() -> None:
    advice = FinancialWellbeingAgent.generate_advice("new-user", None)

    assert "Welcome" in advice.headline
    assert len(advice.actionable_steps) > 0
    assert len(advice.vulnerability_alerts) == 0


def test_spending_explanation_existing_category() -> None:
    summary = create_sample_summary(is_overbudget=True)
    explanation = FinancialWellbeingAgent.explain_spending("user-123", "Food", summary)

    assert explanation.category == "Food"
    assert "Spending Analysis" in explanation.headline
    assert "1,200.00" in explanation.explanation
    assert explanation.budget_comparison is not None
    assert "Over Budget" in explanation.budget_comparison


def test_spending_explanation_missing_category() -> None:
    summary = create_sample_summary()
    explanation = FinancialWellbeingAgent.explain_spending("user-123", "Entertainment", summary)

    assert explanation.category == "Entertainment"
    assert "No Spending" in explanation.headline


def test_advice_endpoint_with_context() -> None:
    summary = create_sample_summary()
    payload = {
        "user_id": "user-123",
        "query": "How can I improve my savings?",
        "financial_context": summary.model_dump(by_alias=True),
    }

    response = client.post("/v1/agents/financial-wellbeing/advice", json=payload)
    assert response.status_code == 200
    data = response.json()
    assert data["agent_id"] == "financial-wellbeing"
    assert data["user_id"] == "user-123"
    assert len(data["actionable_steps"]) > 0


def test_explain_spending_endpoint_with_context() -> None:
    summary = create_sample_summary()
    payload = {
        "user_id": "user-123",
        "category": "Food",
        "financial_context": summary.model_dump(by_alias=True),
    }

    response = client.post("/v1/agents/financial-wellbeing/explain-spending", json=payload)
    assert response.status_code == 200
    data = response.json()
    assert data["category"] == "Food"
    assert "Analysis" in data["headline"]
