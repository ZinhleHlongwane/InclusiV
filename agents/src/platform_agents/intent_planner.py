import logging
import os
from typing import Any

import httpx
from pydantic import BaseModel, Field, ValidationError

logger = logging.getLogger(__name__)


class IntentExecutionPlan(BaseModel):
    """Validated, tool-free plan describing which specialist agents to invoke."""

    intents: list[str] = Field(min_length=1)
    confidence: float = Field(ge=0.0, le=1.0)
    needs_clarification: bool = False
    clarification_question: str | None = None
    rationale: str = ""
    planning_mode: str = "model-assisted"


class IntentPlanner:
    """Use Azure OpenAI for intent planning, with deterministic fallback.

    The model only classifies user intent. It cannot call agents, access
    financial records, or execute actions. The orchestrator remains in charge
    of validating and executing the returned plan.
    """

    ALLOWED_INTENTS = {
        "payments-transactions",
        "budget-planning",
        "savings-goals",
        "financial-wellbeing",
    }
    MIN_CONFIDENCE = 0.65

    def __init__(
        self,
        *,
        endpoint: str | None = None,
        api_key: str | None = None,
        deployment: str | None = None,
        api_version: str | None = None,
        http_client: Any | None = None,
    ) -> None:
        self.endpoint = (endpoint if endpoint is not None else os.getenv("AZURE_AI_ENDPOINT", "")).rstrip("/")
        self.api_key = api_key if api_key is not None else os.getenv("AZURE_AI_API_KEY", "")
        self.deployment = deployment if deployment is not None else os.getenv("AZURE_AI_DEPLOYMENT", "")
        self.api_version = api_version or os.getenv("AZURE_AI_API_VERSION", "2024-10-21")
        self.http_client = http_client

    @property
    def model_enabled(self) -> bool:
        return bool(self.endpoint and self.api_key and self.deployment)

    @staticmethod
    def deterministic_plan(message: str) -> IntentExecutionPlan:
        """Fallback routing that does not require model credentials or network."""
        text = message.lower()
        payment_terms = (
            "simulate payment", "simulate a payment", "make a payment",
            "pay ", "send money", "transfer money",
        )
        if any(term in text for term in payment_terms):
            intents = ["payments-transactions"]
        else:
            intents: list[str] = []
            budget_terms = (
                "budget", "spending limit", "category limit",
                "manage my spending", "overspending", "over budget",
            )
            savings_terms = (
                "savings goal", "saving goal", "savings progress",
                "track my savings", "my savings goals", "saving for", "reach my goal",
            )
            wellbeing_terms = (
                "financial wellbeing", "financial health", "cash flow",
                "financial situation", "financial overview", "spending breakdown",
                "can i afford", "affordability",
            )
            if any(term in text for term in budget_terms):
                intents.append("budget-planning")
            if any(term in text for term in savings_terms):
                intents.append("savings-goals")
            if any(term in text for term in wellbeing_terms):
                intents.append("financial-wellbeing")
            if not intents:
                intents = ["financial-wellbeing"]

        return IntentExecutionPlan(
            intents=intents,
            confidence=0.5,
            rationale="Deterministic keyword routing fallback.",
            planning_mode="deterministic-fallback",
        )

    async def create_plan(self, message: str) -> IntentExecutionPlan:
        fallback = self.deterministic_plan(message)
        if not self.model_enabled:
            return fallback

        url = (
            f"{self.endpoint}/openai/deployments/{self.deployment}"
            f"/chat/completions?api-version={self.api_version}"
        )
        payload = {
            "temperature": 0,
            "response_format": {"type": "json_object"},
            "messages": [
                {
                    "role": "system",
                    "content": (
                        "You are an intent classifier for a financial inclusion assistant. "
                        "Return only a JSON object with keys: intents (array), confidence "
                        "(0 to 1), needs_clarification (boolean), clarification_question "
                        "(string or null), rationale (short string). Allowed intents are "
                        "payments-transactions, budget-planning, savings-goals, and "
                        "financial-wellbeing. Select every relevant specialist. If a request "
                        "asks to initiate or simulate a payment, return only "
                        "payments-transactions. Do not provide financial advice, invent data, "
                        "or request tools. If the user's meaning is genuinely unclear, set "
                        "needs_clarification true and ask one concise question."
                    ),
                },
                {"role": "user", "content": message},
            ],
        }

        try:
            if self.http_client is None:
                async with httpx.AsyncClient(timeout=8.0) as client:
                    response = await client.post(
                        url,
                        headers={"api-key": self.api_key, "Content-Type": "application/json"},
                        json=payload,
                    )
            else:
                response = await self.http_client.post(
                    url,
                    headers={"api-key": self.api_key, "Content-Type": "application/json"},
                    json=payload,
                    timeout=8.0,
                )
            response.raise_for_status()
            content = response.json()["choices"][0]["message"]["content"]
            raw_plan = IntentExecutionPlan.model_validate_json(content)
            return self._validate_plan(raw_plan, fallback)
        except (httpx.HTTPError, KeyError, IndexError, TypeError, ValueError, ValidationError):
            logger.warning("Model-assisted intent planning failed; using deterministic fallback.", exc_info=True)
            return fallback
        except Exception:
            logger.exception("Unexpected intent-planning error; using deterministic fallback.")
            return fallback

    def _validate_plan(
        self,
        plan: IntentExecutionPlan,
        fallback: IntentExecutionPlan,
    ) -> IntentExecutionPlan:
        if plan.needs_clarification:
            if not plan.clarification_question:
                return fallback
            return plan

        if not plan.intents or any(intent not in self.ALLOWED_INTENTS for intent in plan.intents):
            return fallback

        # A payment request must never be combined with other specialist work.
        if "payments-transactions" in plan.intents:
            if len(plan.intents) != 1:
                return fallback
            return plan

        # Deduplicate while retaining the model's intended execution order.
        plan.intents = list(dict.fromkeys(plan.intents))
        if plan.confidence < self.MIN_CONFIDENCE:
            return fallback
        return plan
