from platform_agents.models import BudgetPlanningResponse, FinancialSummaryModel


class BudgetPlanningAgent:
    """Rule-based agent for practical monthly budget planning."""

    @staticmethod
    def create_plan(
        user_id: str,
        summary: FinancialSummaryModel | None,
        query: str | None = None,
    ) -> BudgetPlanningResponse:
        if summary is None:
            return BudgetPlanningResponse(
                user_id=user_id,
                headline="Let's build your first monthly budget",
                overview="There isn't enough financial activity available to create a personalised plan yet.",
                recommendations=[
                    "Record your income and regular expenses so the plan reflects your actual cash flow.",
                    "Start with essential categories such as Food, Transport, Housing and Data.",
                    "Choose realistic limits that leave room for unexpected expenses.",
                ],
                warnings=[],
                next_steps=[
                    "Add your income and recent expenses.",
                    "Create a monthly budget for one essential spending category.",
                ],
            )

        recommendations: list[str] = []
        warnings: list[str] = []
        next_steps: list[str] = []
        budgets = summary.budget_statuses
        income = summary.cash_flow.total_income
        expenses = summary.cash_flow.total_expenses
        net_cash = summary.cash_flow.net_cash_flow

        if not budgets:
            recommendations.append(
                "No category budgets are configured yet. Start with the categories that account for most of your regular spending."
            )
            next_steps.append("Create your first category budget using your recent spending as a starting point.")

        for budget in budgets:
            remaining = budget.remaining_budget
            if budget.is_exceeded or budget.health == "Exceeded":
                overrun = max(budget.current_spend - budget.monthly_limit, 0)
                warnings.append(
                    f"{budget.category} is over its R{budget.monthly_limit:,.2f} monthly limit by R{overrun:,.2f}."
                )
                recommendations.append(
                    f"Review non-essential {budget.category} purchases and avoid increasing this month's limit just to hide the overrun."
                )
            elif budget.health == "Warning" or budget.percentage_used >= 80:
                recommendations.append(
                    f"{budget.category} has used {budget.percentage_used:.1f}% of its R{budget.monthly_limit:,.2f} limit, with R{remaining:,.2f} remaining. Pace the remaining spend across the month."
                )
            else:
                recommendations.append(
                    f"{budget.category} is currently within its limit, with R{remaining:,.2f} remaining from R{budget.monthly_limit:,.2f}."
                )

        if net_cash < 0:
            warnings.append(
                f"Recorded expenses exceed income by R{abs(net_cash):,.2f}. Avoid setting budget limits that assume money you have not received."
            )
            next_steps.append("Prioritise essential costs and identify one flexible expense to reduce.")
        elif income > 0:
            margin = net_cash / income * 100
            recommendations.append(
                f"Your recorded cash-flow margin is {margin:.1f}% (R{net_cash:,.2f} after R{expenses:,.2f} in expenses). Use this as a guide, not as guaranteed spare cash."
            )
            if net_cash > 0:
                next_steps.append("Decide how much of the remaining cash should be reserved for upcoming essentials before assigning money to optional spending.")
        elif expenses > 0:
            warnings.append("Expenses are recorded, but no income is recorded for this period. Add any income before treating the budget as balanced.")

        if summary.spending_by_category:
            highest = max(summary.spending_by_category, key=lambda item: item.total_amount)
            recommendations.append(
                f"{highest.category} is your largest recorded spending category at R{highest.total_amount:,.2f}. Check whether this reflects essential or flexible spending before changing its limit."
            )

        if not next_steps:
            next_steps.append("Review your category limits after recording your next few transactions.")

        headline = "Your monthly budget review"
        if warnings:
            headline = "Your budget needs attention"
        elif budgets:
            headline = "Your budget is on track"

        if query and any(term in query.lower() for term in ("reduce", "cut", "less")):
            next_steps.insert(0, "Start by reviewing flexible spending in your highest-spend category.")

        return BudgetPlanningResponse(
            user_id=user_id,
            headline=headline,
            overview=(
                f"For {summary.month:02d}/{summary.year}, you have {len(budgets)} category budget(s), "
                f"R{income:,.2f} recorded income and R{expenses:,.2f} recorded expenses."
            ),
            recommendations=recommendations,
            warnings=warnings,
            next_steps=next_steps,
        )
