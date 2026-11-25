from fastapi import FastAPI
from platform_agents.routes import router

app = FastAPI(
    title="Financial Inclusion Platform Agent Service",
    version="0.1.0",
    description="Agent service coordinating specialist financial agents through a central orchestrator.",
)
app.include_router(router)

@app.get("/health", tags=["health"])
async def health() -> dict[str, str]:
    return {"service": "agent-service", "status": "healthy", "stage": "model-assisted-intent-planning"}
