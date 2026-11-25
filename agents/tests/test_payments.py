from fastapi.testclient import TestClient
from platform_agents.main import app

client = TestClient(app)


def test_simulate_payment_success() -> None:
    payload = {
        "user_id": "user-123",
        "account_id": "acc-456",
        "recipient": "Campus Bookstore",
        "amount": 350.0,
        "category": "Education",
        "description": "Calculus Textbook",
    }
    response = client.post("/v1/agents/payments/simulate", json=payload)
    assert response.status_code == 200
    data = response.json()
    assert data["status"] == "completed"
    assert data["payment_id"].startswith("sim_")
    assert data["amount"] == 350.0
    assert "Campus Bookstore" in data["message"]


def test_simulate_payment_large_amount_notice() -> None:
    payload = {
        "user_id": "user-123",
        "account_id": "acc-456",
        "recipient": "Laptop Store",
        "amount": 4500.0,
        "category": "Equipment",
        "description": "Student Laptop",
    }
    response = client.post("/v1/agents/payments/simulate", json=payload)
    assert response.status_code == 200
    data = response.json()
    assert "large expenses" in data["message"].lower()


def test_simulate_payment_invalid_amount() -> None:
    payload = {
        "user_id": "user-123",
        "account_id": "acc-456",
        "recipient": "Cafe",
        "amount": -50.0,
        "description": "Coffee",
    }
    response = client.post("/v1/agents/payments/simulate", json=payload)
    assert response.status_code == 400
    assert "greater than zero" in response.json()["detail"].lower()


def test_simulate_payment_empty_recipient() -> None:
    payload = {
        "user_id": "user-123",
        "account_id": "acc-456",
        "recipient": "   ",
        "amount": 100.0,
        "description": "Coffee",
    }
    response = client.post("/v1/agents/payments/simulate", json=payload)
    assert response.status_code == 400
    assert "recipient cannot be empty" in response.json()["detail"].lower()
