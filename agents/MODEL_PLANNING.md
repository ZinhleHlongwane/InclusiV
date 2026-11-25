# Model-assisted intent planning

InclusiV can optionally use an Azure OpenAI chat-completions deployment to understand a user's request and produce a structured specialist execution plan.

## Configuration

The model integration is optional. Configure these variables in the repository-root `.env` file:

- `AZURE_AI_ENDPOINT`: Azure OpenAI resource endpoint, for example `https://your-resource.openai.azure.com`
- `AZURE_AI_API_KEY`: API key for that resource
- `AZURE_AI_DEPLOYMENT`: deployed model name/deployment identifier
- `AZURE_AI_API_VERSION`: API version; defaults to `2024-10-21`

The agent service receives these values through Docker Compose. Do not commit real API keys or place them in source files.

If endpoint, key, or deployment is missing, the service uses deterministic keyword routing. It also falls back to deterministic routing when the model request fails, returns invalid JSON, selects an unknown agent, or returns a low-confidence plan.

## Planning flow

1. The orchestrator passes the chat message to `IntentPlanner`.
2. When configured, the planner asks the model to classify intent and return JSON.
3. Pydantic validates the response and the planner checks every intent against the allowlist.
4. Low-confidence, invalid, or failed model plans use the deterministic fallback.
5. The orchestrator executes only the validated specialist intents.
6. If the request is ambiguous, the planner can return one clarification question instead of invoking an agent.

The model does not receive the financial summary, does not have tools, and cannot execute agents or payments. It only proposes intent labels. The orchestrator remains responsible for execution. Payment requests are isolated: a plan that mixes payment intent with other agents is rejected and replaced by deterministic routing.

## Structured plan

A successful model response follows this shape:

```json
{
  "intents": ["budget-planning", "savings-goals"],
  "confidence": 0.92,
  "needs_clarification": false,
  "clarification_question": null,
  "rationale": "The user asks about both spending and savings."
}
```

The API response includes the validated `execution_plan`, including whether it used `model-assisted` planning or the `deterministic-fallback`.

## Privacy and operational notes

When enabled, the user's chat message is sent to the configured Azure OpenAI deployment for intent classification. Do not include account records or financial summaries in this request. The current implementation intentionally sends only the message text. Review your Azure resource's data-handling and retention terms before enabling it with real users.

The integration is optional and has not been verified against a live Azure deployment unless valid resource configuration is supplied. Automated tests use mocked HTTP responses and do not require API credentials.
