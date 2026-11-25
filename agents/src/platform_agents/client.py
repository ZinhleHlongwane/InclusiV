import os
from typing import Any
import httpx
from platform_agents.models import FinancialSummaryModel


class BackendApiClient:
    def __init__(self, base_url: str | None = None, timeout: float = 5.0) -> None:
        self.base_url = (base_url or os.getenv("API_BASE_URL", "http://localhost:8080")).rstrip("/")
        self.timeout = timeout

    async def get_financial_summary(
        self, user_id: str, year: int | None = None, month: int | None = None
    ) -> FinancialSummaryModel | None:
        params: dict[str, Any] = {}
        if year is not None:
            params["year"] = year
        if month is not None:
            params["month"] = month

        url = f"{self.base_url}/api/financial-summary/user/{user_id}"
        try:
            async with httpx.AsyncClient(timeout=self.timeout) as client:
                response = await client.get(url, params=params)
                if response.status_code == 200:
                    return FinancialSummaryModel.model_validate(response.json())
                return None
        except Exception:
            return None
