import asyncio

import httpx

from platform_agents.intent_planner import IntentExecutionPlan, IntentPlanner


def test_deterministic_fallback_works_without_model_configuration() -> None:
    planner = IntentPlanner(endpoint="", api_key="", deployment="")
    plan = asyncio.run(planner.create_plan("Review my budget and savings goals"))

    assert plan.intents == ["budget-planning", "savings-goals"]
    assert plan.planning_mode == "deterministic-fallback"


def test_model_plan_can_select_multiple_specialists() -> None:
    def handler(request: httpx.Request) -> httpx.Response:
        return httpx.Response(
            200,
            json={
                "choices": [
                    {
                        "message": {
                            "content": (
                                '{"intents":["budget-planning","savings-goals"],'
                                '"confidence":0.94,"needs_clarification":false,'
                                '"clarification_question":null,"rationale":"The user asks about both."}'
                            )
                        }
                    }
                ]
            },
        )

    client = httpx.AsyncClient(transport=httpx.MockTransport(handler))
    planner = IntentPlanner(
        endpoint="https://example.test",
        api_key="test-key",
        deployment="test-model",
        http_client=client,
    )
    plan = asyncio.run(planner.create_plan("Can I save more while keeping my spending under control?"))
    asyncio.run(client.aclose())

    assert plan.intents == ["budget-planning", "savings-goals"]
    assert plan.planning_mode == "model-assisted"
    assert plan.confidence == 0.94


def test_invalid_model_intent_uses_deterministic_fallback() -> None:
    def handler(request: httpx.Request) -> httpx.Response:
        return httpx.Response(
            200,
            json={
                "choices": [
                    {
                        "message": {
                            "content": (
                                '{"intents":["delete-my-account"],"confidence":0.99,'
                                '"needs_clarification":false,"rationale":"invalid intent"}'
                            )
                        }
                    }
                ]
            },
        )

    client = httpx.AsyncClient(transport=httpx.MockTransport(handler))
    planner = IntentPlanner(
        endpoint="https://example.test",
        api_key="test-key",
        deployment="test-model",
        http_client=client,
    )
    plan = asyncio.run(planner.create_plan("Please review my budget"))
    asyncio.run(client.aclose())

    assert plan.intents == ["budget-planning"]
    assert plan.planning_mode == "deterministic-fallback"


def test_model_payment_intent_cannot_be_combined_with_other_agents() -> None:
    def handler(request: httpx.Request) -> httpx.Response:
        return httpx.Response(
            200,
            json={
                "choices": [
                    {
                        "message": {
                            "content": (
                                '{"intents":["payments-transactions","budget-planning"],'
                                '"confidence":0.99,"needs_clarification":false,'
                                '"rationale":"invalid mixed payment plan"}'
                            )
                        }
                    }
                ]
            },
        )

    client = httpx.AsyncClient(transport=httpx.MockTransport(handler))
    planner = IntentPlanner(
        endpoint="https://example.test",
        api_key="test-key",
        deployment="test-model",
        http_client=client,
    )
    plan = asyncio.run(planner.create_plan("Simulate a payment and review my budget"))
    asyncio.run(client.aclose())

    assert plan.intents == ["payments-transactions"]
    assert plan.planning_mode == "deterministic-fallback"


def test_low_confidence_model_plan_falls_back() -> None:
    def handler(request: httpx.Request) -> httpx.Response:
        return httpx.Response(
            200,
            json={
                "choices": [
                    {
                        "message": {
                            "content": (
                                '{"intents":["savings-goals"],"confidence":0.2,'
                                '"needs_clarification":false,"rationale":"uncertain"}'
                            )
                        }
                    }
                ]
            },
        )

    client = httpx.AsyncClient(transport=httpx.MockTransport(handler))
    planner = IntentPlanner(
        endpoint="https://example.test",
        api_key="test-key",
        deployment="test-model",
        http_client=client,
    )
    plan = asyncio.run(planner.create_plan("Review my budget"))
    asyncio.run(client.aclose())

    assert plan.intents == ["budget-planning"]
    assert plan.planning_mode == "deterministic-fallback"


def test_model_can_request_clarification() -> None:
    def handler(request: httpx.Request) -> httpx.Response:
        return httpx.Response(
            200,
            json={
                "choices": [
                    {
                        "message": {
                            "content": (
                                '{"intents":["financial-wellbeing"],"confidence":0.8,'
                                '"needs_clarification":true,'
                                '"clarification_question":"Are you asking about a budget or a savings goal?",'
                                '"rationale":"The request is ambiguous."}'
                            )
                        }
                    }
                ]
            },
        )

    client = httpx.AsyncClient(transport=httpx.MockTransport(handler))
    planner = IntentPlanner(
        endpoint="https://example.test",
        api_key="test-key",
        deployment="test-model",
        http_client=client,
    )
    plan = asyncio.run(planner.create_plan("Can you help me with that thing?"))
    asyncio.run(client.aclose())

    assert plan.needs_clarification is True
    assert plan.clarification_question == "Are you asking about a budget or a savings goal?"

def test_model_can_return_clarification_without_selecting_an_agent() -> None:
    def handler(request: httpx.Request) -> httpx.Response:
        return httpx.Response(
            200,
            json={
                "choices": [
                    {
                        "message": {
                            "content": (
                                '{"intents":[],"confidence":0.4,'
                                '"needs_clarification":true,'
                                '"clarification_question":"Do you mean your budget or savings goal?",'
                                '"rationale":"The request is ambiguous."}'
                            )
                        }
                    }
                ]
            },
        )

    client = httpx.AsyncClient(transport=httpx.MockTransport(handler))
    planner = IntentPlanner(
        endpoint="https://example.test",
        api_key="test-key",
        deployment="test-model",
        http_client=client,
    )
    plan = asyncio.run(planner.create_plan("Can you help me with it?"))
    asyncio.run(client.aclose())

    assert plan.needs_clarification is True
    assert plan.intents == []
