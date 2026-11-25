from fastapi import APIRouter, HTTPException
from platform_agents.agents.budget_planning import BudgetPlanningAgent
from platform_agents.agents.payments import PaymentsAgent
from platform_agents.agents.wellbeing import FinancialWellbeingAgent
from platform_agents.client import BackendApiClient
from platform_agents.models import (
    BudgetPlanningRequest,
    BudgetPlanningResponse,
    ChatRequest,
    ChatResponse,
    FinancialAdviceRequest,
    FinancialAdviceResponse,
    SandboxPaymentRequest,
    SandboxPaymentResponse,
    SpendingExplanationRequest,
    SpendingExplanationResponse,
)

router = APIRouter(prefix="/v1", tags=["agents"])

AGENTS = [
    {
        "id": "financial-wellbeing",
        "name": "Financial Wellbeing Agent",
        "status": "active",
        "responsibility": "Spending explanations, budgeting, vulnerability alerts and savings guidance.",
    },
    {
        "id": "budget-planning",
        "name": "Budget Planning Agent",
        "status": "active",
        "responsibility": "Monthly budget reviews, category spending limits and practical spending recommendations.",
    },
    {
        "id": "payments-transactions",
        "name": "Payments and Transactions Agent",
        "status": "active",
        "responsibility": "Sandbox payment workflows, transaction verification and spending threshold checks.",
    },
    {
        "id": "insurance-guidance",
        "name": "Insurance Guidance Agent",
        "status": "planned",
        "responsibility": "Explain insurance terminology and approved policy information.",
    },
    {
        "id": "financial-research",
        "name": "Financial Research Agent",
        "status": "planned",
        "responsibility": "Source-backed research into relevant financial resources.",
    },
]

backend_client = BackendApiClient()


@router.get("/agents")
async def list_agents() -> dict[str, list[dict[str, str]]]:
    return {"agents": AGENTS}


@router.post("/agents/financial-wellbeing/advice", response_model=FinancialAdviceResponse)
async def get_financial_advice(request: FinancialAdviceRequest) -> FinancialAdviceResponse:
    summary = request.financial_context
    if summary is None:
        summary = await backend_client.get_financial_summary(request.user_id)

    return FinancialWellbeingAgent.generate_advice(
        user_id=request.user_id,
        summary=summary,
        query=request.query,
    )


@router.post("/agents/financial-wellbeing/explain-spending", response_model=SpendingExplanationResponse)
async def explain_spending(request: SpendingExplanationRequest) -> SpendingExplanationResponse:
    summary = request.financial_context
    if summary is None:
        summary = await backend_client.get_financial_summary(request.user_id)

    return FinancialWellbeingAgent.explain_spending(
        user_id=request.user_id,
        category=request.category,
        summary=summary,
    )


@router.post("/agents/budget-planning/plan", response_model=BudgetPlanningResponse)
async def create_budget_plan(request: BudgetPlanningRequest) -> BudgetPlanningResponse:
    summary = request.financial_context
    if summary is None:
        summary = await backend_client.get_financial_summary(request.user_id)

    return BudgetPlanningAgent.create_plan(
        user_id=request.user_id,
        summary=summary,
        query=request.query,
    )


@router.post("/agents/payments/simulate", response_model=SandboxPaymentResponse)
async def simulate_payment(request: SandboxPaymentRequest) -> SandboxPaymentResponse:
    try:
        return PaymentsAgent.simulate_payment(request)
    except ValueError as ex:
        raise HTTPException(status_code=400, detail=str(ex)) from ex


@router.post("/chat", response_model=ChatResponse)
async def chat(request: ChatRequest) -> ChatResponse:
    msg_lower = request.message.lower()

    # Route payment simulator queries
    if any(keyword in msg_lower for keyword in ["pay ", "send money", "transfer", "simulate payment"]):
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

    # Route financial wellbeing queries
    summary = request.financial_context
    if summary is None:
        summary = await backend_client.get_financial_summary(request.user_id)

    advice = FinancialWellbeingAgent.generate_advice(
        user_id=request.user_id,
        summary=summary,
        query=request.message,
    )

    # Format empathetic conversational reply
    reply_parts = [advice.headline + "\n"]

    if advice.vulnerability_alerts:
        reply_parts.append("Important Alerts:")
        for alert in advice.vulnerability_alerts:
            reply_parts.append(f"- {alert}")
        reply_parts.append("")

    if advice.insights:
        reply_parts.append("Key Insights:")
        for insight in advice.insights[:2]:
            reply_parts.append(f"- {insight}")
        reply_parts.append("")

    if advice.actionable_steps:
        reply_parts.append("Recommended Next Step:")
        reply_parts.append(f"- {advice.actionable_steps[0]}")

    suggestions = [
        "Explain my spending breakdown",
        "How can I save more this month?",
        "Check my budget limits",
    ]

    return ChatResponse(
        agent_id="financial-wellbeing",
        agent_name="Financial Wellbeing Agent",
        reply="\n".join(reply_parts),
        suggestions=suggestions,
        data=advice.model_dump(),
    )
