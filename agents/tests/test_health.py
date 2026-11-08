from fastapi.testclient import TestClient
from platform_agents.main import app

client = TestClient(app)

def test_health_endpoint() -> None:
    response = client.get("/health")
    assert response.status_code == 200
    assert response.json()["status"] == "healthy"

def test_agent_catalog_is_explicitly_planned() -> None:
    response = client.get("/v1/agents")
    assert response.status_code == 200
    agents = response.json()["agents"]
    assert len(agents) == 4
    assert all(agent["status"] == "planned" for agent in agents)
