from fastapi.testclient import TestClient
from platform_agents.main import app

client = TestClient(app)


def test_health_endpoint() -> None:
    response = client.get("/health")
    assert response.status_code == 200
    data = response.json()
    assert data["status"] == "healthy"
    assert data["stage"] == "wellbeing-and-payments"


def test_agent_catalog_contains_active_and_planned_agents() -> None:
    response = client.get("/v1/agents")
    assert response.status_code == 200
    agents = response.json()["agents"]
    assert len(agents) == 4

    agent_dict = {a["id"]: a for a in agents}
    assert agent_dict["financial-wellbeing"]["status"] == "active"
    assert agent_dict["payments-transactions"]["status"] == "active"
    assert agent_dict["insurance-guidance"]["status"] == "planned"
    assert agent_dict["financial-research"]["status"] == "planned"
