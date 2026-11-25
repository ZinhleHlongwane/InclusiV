from fastapi import APIRouter, HTTPException
from platform_agents.agents.budget_planning import BudgetPlanningAgent
from platform_agents.agents.payments import PaymentsAgent
from platform_agents.agents.savings_goals import SavingsGoalsAgent
from platform_agents.agents.wellbeing import FinancialWellbeingAgent
from platform_agents.client import BackendApiClient
from platform_agents.models import (
    BudgetPlanningRequest,
    BudgetPlanningResponse,
    ChatRequest,
    ChatResponse,
    FinancialAdviceRequest,
    FinancialAdviceResponse,
    SavingsGoalsRequest,
    SavingsGoalsResponse,
    SandboxPaymentRequest,
    SandboxPaymentResponse,
    SpendingExplanationRequest,
    SpendingExplanationResponse,
)

router = APIRouter(prefix="/v1", tags=["agents"])

AGENTS = [
    {
        "id": "agent-orchestrator",
        "name": "InclusiV Agent Orchestrator",
        "status": "active",
        "responsibility": "Builds validated specialist execution plans using optional model-assisted intent understanding with deterministic fallback.",
    },
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
        "id": "savings-goals",
        "name": "Savings & Goals Agent",
        "status": "active",
        "responsibility": "Savings goal progress, target tracking and realistic contribution guidance.",
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


@router.post("/agents/savings-goals/review", response_model=SavingsGoalsResponse)
async def review_savings_goals(request: SavingsGoalsRequest) -> SavingsGoalsResponse:
    summary = request.financial_context
    if summary is None:
        summary = await backend_client.get_financial_summary(request.user_id)

    return SavingsGoalsAgent.review_goals(
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
    from platform_agents.orchestrator import AgentOrchestrator

    orchestrator = AgentOrchestrator(backend_client=backend_client)
    return await orchestrator.handle(request)
