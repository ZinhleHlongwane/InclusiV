from fastapi.testclient import TestClient
from platform_agents.main import app

client = TestClient(app)


def test_chat_financial_wellbeing_routing() -> None:
    payload = {
        "user_id": "user-123",
        "message": "How can I start an emergency fund on a student stipend?",
    }
    response = client.post("/v1/chat", json=payload)
    assert response.status_code == 200
    data = response.json()
    assert data["agent_id"] == "financial-wellbeing"
    assert "Financial Wellbeing Agent" in data["agent_name"]
    assert len(data["suggestions"]) > 0
    assert len(data["reply"]) > 0


def test_chat_payments_routing() -> None:
    payload = {
        "user_id": "user-123",
        "message": "Can I simulate payment for my textbook?",
    }
    response = client.post("/v1/chat", json=payload)
    assert response.status_code == 200
    data = response.json()
    assert data["agent_id"] == "payments-transactions"
    assert "Payments and Transactions Agent" in data["agent_name"]
    assert "simulate sandbox payments" in data["reply"].lower()
    assert any("simulate" in s.lower() for s in data["suggestions"])
