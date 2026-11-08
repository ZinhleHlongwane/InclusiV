from fastapi import APIRouter

router = APIRouter(prefix="/v1", tags=["agents"])

AGENTS = [
    {"id": "financial-wellbeing", "name": "Financial Wellbeing Agent", "status": "planned",
     "responsibility": "Spending explanations, budgeting and savings guidance."},
    {"id": "insurance-guidance", "name": "Insurance Guidance Agent", "status": "planned",
     "responsibility": "Explain insurance terminology and approved policy information."},
    {"id": "payments-transactions", "name": "Payments and Transactions Agent", "status": "planned",
     "responsibility": "Sandbox payment workflows and transaction status."},
    {"id": "financial-research", "name": "Financial Research Agent", "status": "planned",
     "responsibility": "Source-backed research into relevant financial resources."},
]

@router.get("/agents")
async def list_agents() -> dict[str, list[dict[str, str]]]:
    return {"agents": AGENTS}
