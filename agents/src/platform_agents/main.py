from fastapi import FastAPI
from platform_agents.routes import router

app = FastAPI(
    title="Financial Inclusion Platform Agent Service",
    version="0.1.0",
    description="Foundation service; LLM and protocol capabilities are not implemented yet.",
)
app.include_router(router)

@app.get("/health", tags=["health"])
async def health() -> dict[str, str]:
    return {"service": "agent-service", "status": "healthy", "stage": "foundation"}
