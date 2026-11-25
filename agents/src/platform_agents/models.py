from datetime import datetime, timezone
from typing import Any, Literal
from pydantic import BaseModel, Field


class CashFlowSummaryModel(BaseModel):
    total_income: float = Field(alias="totalIncome", default=0.0)
    total_expenses: float = Field(alias="totalExpenses", default=0.0)
    net_cash_flow: float = Field(alias="netCashFlow", default=0.0)
    currency: str = "ZAR"
    from_date: str | None = Field(alias="fromDate", default=None)
    to_date: str | None = Field(alias="toDate", default=None)

    model_config = {"populate_by_name": True}


class CategorySpendingModel(BaseModel):
    category: str
    total_amount: float = Field(alias="totalAmount", default=0.0)
    percentage_of_total_expenses: float = Field(alias="percentageOfTotalExpenses", default=0.0)
    transaction_count: int = Field(alias="transactionCount", default=0)

    model_config = {"populate_by_name": True}


class BudgetStatusModel(BaseModel):
    budget_id: str = Field(alias="budgetId", default="")
    category: str
    monthly_limit: float = Field(alias="monthlyLimit", default=0.0)
    current_spend: float = Field(alias="currentSpend", default=0.0)
    remaining_budget: float = Field(alias="remainingBudget", default=0.0)
    percentage_used: float = Field(alias="percentageUsed", default=0.0)
    health: Literal["OnTrack", "Warning", "Exceeded"] = "OnTrack"
    is_exceeded: bool = Field(alias="isExceeded", default=False)

    model_config = {"populate_by_name": True}


class SavingsGoalProgressModel(BaseModel):
    goal_id: str = Field(alias="goalId", default="")
    name: str
    target_amount: float = Field(alias="targetAmount", default=0.0)
    current_amount: float = Field(alias="currentAmount", default=0.0)
    remaining_amount: float = Field(alias="remainingAmount", default=0.0)
    percentage_achieved: float = Field(alias="percentageAchieved", default=0.0)
    target_date: str | None = Field(alias="targetDate", default=None)
    recommended_monthly_savings: float | None = Field(alias="recommendedMonthlySavings", default=None)
    is_completed: bool = Field(alias="isCompleted", default=False)

    model_config = {"populate_by_name": True}


class FinancialSummaryModel(BaseModel):
    user_id: str = Field(alias="userId", default="")
    year: int = 2026
    month: int = 11
    total_balance: float = Field(alias="totalBalance", default=0.0)
    cash_flow: CashFlowSummaryModel = Field(alias="cashFlow", default_factory=CashFlowSummaryModel)
    spending_by_category: list[CategorySpendingModel] = Field(alias="spendingByCategory", default_factory=list)
    budget_statuses: list[BudgetStatusModel] = Field(alias="budgetStatuses", default_factory=list)
    savings_goals: list[SavingsGoalProgressModel] = Field(alias="savingsGoals", default_factory=list)
    account_count: int = Field(alias="accountCount", default=0)
    transaction_count: int = Field(alias="transactionCount", default=0)
    has_negative_cash_flow: bool = Field(alias="hasNegativeCashFlow", default=False)
    has_overbudget_alerts: bool = Field(alias="hasOverbudgetAlerts", default=False)
    total_savings: float = Field(alias="totalSavings", default=0.0)

    model_config = {"populate_by_name": True}


class FinancialAdviceRequest(BaseModel):
    user_id: str
    query: str | None = None
    financial_context: FinancialSummaryModel | None = None


class FinancialAdviceResponse(BaseModel):
    agent_id: str = "financial-wellbeing"
    user_id: str
    headline: str
    insights: list[str] = Field(default_factory=list)
    budget_recommendations: list[str] = Field(default_factory=list)
    savings_recommendations: list[str] = Field(default_factory=list)
    actionable_steps: list[str] = Field(default_factory=list)
    vulnerability_alerts: list[str] = Field(default_factory=list)
    timestamp: str = Field(default_factory=lambda: datetime.now(timezone.utc).isoformat())


class SpendingExplanationRequest(BaseModel):
    user_id: str
    category: str | None = None
    financial_context: FinancialSummaryModel | None = None


class SpendingExplanationResponse(BaseModel):
    category: str | None
    headline: str
    explanation: str
    budget_comparison: str | None = None
    recommended_action: str


class SandboxPaymentRequest(BaseModel):
    user_id: str
    account_id: str
    recipient: str
    amount: float
    category: str = "Utilities"
    description: str


class SandboxPaymentResponse(BaseModel):
    payment_id: str
    user_id: str
    account_id: str
    recipient: str
    amount: float
    status: str
    message: str
    timestamp: str = Field(default_factory=lambda: datetime.now(timezone.utc).isoformat())


class ChatMessage(BaseModel):
    role: Literal["user", "assistant", "system"]
    content: str


class ChatRequest(BaseModel):
    user_id: str
    message: str
    history: list[ChatMessage] = Field(default_factory=list)
    financial_context: FinancialSummaryModel | None = None


class ChatResponse(BaseModel):
    agent_id: str
    agent_name: str
    reply: str
    suggestions: list[str] = Field(default_factory=list)
    data: dict[str, Any] | None = None
