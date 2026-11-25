import uuid
from platform_agents.models import SandboxPaymentRequest, SandboxPaymentResponse


class PaymentsAgent:
    """Agent responsible for educational sandbox payment workflows and simulated transactions."""

    @staticmethod
    def simulate_payment(request: SandboxPaymentRequest) -> SandboxPaymentResponse:
        if request.amount <= 0:
            raise ValueError("Payment amount must be greater than zero.")

        if not request.recipient or not request.recipient.strip():
            raise ValueError("Recipient cannot be empty.")

        payment_id = f"sim_{uuid.uuid4().hex[:12]}"

        # Educational messaging based on payment size
        if request.amount > 2000:
            message = (
                f"Simulated payment of R{request.amount:,.2f} to {request.recipient.strip()} recorded. "
                "Notice: For large expenses, consider reviewing your remaining category budget first."
            )
        else:
            message = (
                f"Simulated payment of R{request.amount:,.2f} to {request.recipient.strip()} "
                f"for '{request.description.strip()}' completed successfully in sandbox."
            )

        return SandboxPaymentResponse(
            payment_id=payment_id,
            user_id=request.user_id,
            account_id=request.account_id,
            recipient=request.recipient.strip(),
            amount=round(request.amount, 2),
            status="completed",
            message=message,
        )
