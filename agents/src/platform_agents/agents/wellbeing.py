from platform_agents.models import (
    FinancialAdviceResponse,
    FinancialSummaryModel,
    SpendingExplanationResponse,
)


class FinancialWellbeingAgent:
    """Empathetic, rule-backed financial wellbeing agent for students and job seekers."""

    @staticmethod
    def generate_advice(
        user_id: str,
        summary: FinancialSummaryModel | None,
        query: str | None = None,
    ) -> FinancialAdviceResponse:
        insights: list[str] = []
        budget_recs: list[str] = []
        savings_recs: list[str] = []
        actionable_steps: list[str] = []
        vulnerability_alerts: list[str] = []

        if not summary:
            return FinancialAdviceResponse(
                user_id=user_id,
                headline="Welcome to InclusiV Financial Wellbeing",
                insights=[
                    "We don't have enough transaction history for your account yet.",
                    "Connecting your account and recording your regular expenses helps unlock personalized guidance.",
                ],
                budget_recs=["Start by setting up budgets for essentials like Food and Transport."],
                savings_recs=["Begin with a small emergency savings target (e.g. R200 - R500)."],
                actionable_steps=[
                    "Add your first income or expense transaction.",
                    "Set a monthly cap on your highest spending category.",
                ],
                vulnerability_alerts=[],
            )

        cash_flow = summary.cash_flow
        net_cash = cash_flow.net_cash_flow
        total_income = cash_flow.total_income
        total_expenses = cash_flow.total_expenses

        # 1. Cash Flow & Burn Rate Analysis
        if net_cash < 0:
            headline = "Financial Alert: Expenses Exceeded Income This Month"
            deficit = abs(net_cash)
            vulnerability_alerts.append(
                f"Negative Cash Flow Alert: You have spent R{deficit:,.2f} more than your recorded income this month."
            )
            insights.append(
                f"Your total expenses (R{total_expenses:,.2f}) are higher than your income (R{total_income:,.2f})."
            )
            actionable_steps.append("Pause non-essential discretionary purchases until your next income cycle.")
        elif net_cash == 0 and total_income > 0:
            headline = "Living Paycheck-to-Paycheck: Exactly Broke Even"
            insights.append("You have spent 100% of your income this month with zero margin for unexpected costs.")
            vulnerability_alerts.append(
                "Zero Buffer Warning: Any emergency expense would create financial stress or debt."
            )
            actionable_steps.append("Try setting aside 5% of your next allowance or stipend right away.")
        else:
            surplus = net_cash
            headline = "Positive Cash Flow: Great Work Maintaining Financial Surplus"
            insights.append(
                f"You have a healthy monthly surplus of R{surplus:,.2f} (Net savings margin: "
                f"{(surplus / total_income * 100 if total_income > 0 else 0):.1f}%)."
            )
            actionable_steps.append("Direct part of your surplus toward your active savings goals.")

        # 2. Budget Health Analysis
        overbudget = [b for b in summary.budget_statuses if b.is_exceeded]
        warning_budgets = [b for b in summary.budget_statuses if b.health == "Warning" and not b.is_exceeded]

        if overbudget:
            for b in overbudget:
                overspend = b.current_spend - b.monthly_limit
                vulnerability_alerts.append(
                    f"Over-Budget Alert: {b.category} spend is at R{b.current_spend:,.2f}, exceeding your "
                    f"R{b.monthly_limit:,.2f} cap by R{overspend:,.2f} ({b.percentage_used:.0f}% used)."
                )
                budget_recs.append(
                    f"Cut back on {b.category} spending for the rest of the month to limit further overrun."
                )

        if warning_budgets:
            for b in warning_budgets:
                budget_recs.append(
                    f"Caution on {b.category}: You have used {b.percentage_used:.1f}% of your R{b.monthly_limit:,.2f} cap "
                    f"(R{b.remaining_budget:,.2f} remaining)."
                )

        if not summary.budget_statuses:
            budget_recs.append(
                "You haven't set any monthly budgets yet. Setting caps on Food, Transport, and Data helps protect your stipend."
            )
            actionable_steps.append("Create your first monthly category budget.")

        # 3. Category Spending Breakdown
        if summary.spending_by_category:
            top_cat = summary.spending_by_category[0]
            insights.append(
                f"Your largest spending category is {top_cat.category} at R{top_cat.total_amount:,.2f} "
                f"({top_cat.percentage_of_total_expenses:.1f}% of all expenses)."
            )

        # 4. Savings Goals & Emergency Buffer
        if summary.savings_goals:
            active_goals = [g for g in summary.savings_goals if not g.is_completed]
            completed_goals = [g for g in summary.savings_goals if g.is_completed]

            if completed_goals:
                insights.append(
                    f"Congratulations! You have completed {len(completed_goals)} savings goal(s)."
                )

            for g in active_goals:
                if g.recommended_monthly_savings:
                    savings_recs.append(
                        f"For '{g.name}': Save R{g.recommended_monthly_savings:,.2f}/month to reach your "
                        f"R{g.target_amount:,.2f} target by {g.target_date} ({g.percentage_achieved:.1f}% achieved)."
                    )
                else:
                    savings_recs.append(
                        f"For '{g.name}': You are at {g.percentage_achieved:.1f}% (R{g.current_amount:,.2f} of R{g.target_amount:,.2f})."
                    )
        else:
            savings_recs.append(
                "You don't have an active savings goal. Building a R500 Emergency Cushion helps protect against transport or study emergencies."
            )
            actionable_steps.append("Create an 'Emergency Buffer' savings goal with a R500 target.")

        # 5. Tailor response to user query if present
        if query:
            q_lower = query.lower()
            if "save" in q_lower or "savings" in q_lower:
                headline = "Savings Optimization Guidance"
                actionable_steps.insert(0, "Automate or deposit your savings immediately upon receiving income.")
            elif "food" in q_lower or "grocery" in q_lower:
                food_cat = next((c for c in summary.spending_by_category if c.category.lower() == "food"), None)
                if food_cat:
                    insights.insert(
                        0,
                        f"Food represents R{food_cat.total_amount:,.2f} ({food_cat.percentage_of_total_expenses:.1f}% of total expenses). "
                        "Meal planning and cooking shared student meals can save 20-30% on food costs."
                    )
            elif "can i afford" in q_lower:
                if net_cash > 0:
                    insights.insert(
                        0,
                        f"You currently have R{net_cash:,.2f} in remaining net monthly margin. Check if this purchase fits within your budget limits."
                    )
                else:
                    insights.insert(
                        0,
                        "Your current monthly deficit indicates you should defer non-critical purchases."
                    )

        return FinancialAdviceResponse(
            user_id=user_id,
            headline=headline,
            insights=insights,
            budget_recommendations=budget_recs,
            savings_recommendations=savings_recs,
            actionable_steps=actionable_steps,
            vulnerability_alerts=vulnerability_alerts,
        )

    @staticmethod
    def explain_spending(
        user_id: str,
        category: str | None,
        summary: FinancialSummaryModel | None,
    ) -> SpendingExplanationResponse:
        if not summary or not summary.spending_by_category:
            return SpendingExplanationResponse(
                category=category,
                headline="No Spending Data Available",
                explanation="There are no recorded expenses for this period.",
                budget_comparison=None,
                recommended_action="Record your recent expenses to view a category breakdown.",
            )

        cat_spend = None
        if category:
            cat_spend = next((c for c in summary.spending_by_category if c.category.lower() == category.lower()), None)
        else:
            cat_spend = summary.spending_by_category[0]

        if not cat_spend:
            return SpendingExplanationResponse(
                category=category,
                headline=f"No Spending in {category}",
                explanation=f"You have not recorded any expenses under {category} for this period.",
                budget_comparison=None,
                recommended_action="If you made payments in this category, log them to track accurately.",
            )

        # Match with budget if exists
        budget = next((b for b in summary.budget_statuses if b.category.lower() == cat_spend.category.lower()), None)

        headline = f"{cat_spend.category} Spending Analysis"
        explanation = (
            f"You spent R{cat_spend.total_amount:,.2f} across {cat_spend.transaction_count} transaction(s) "
            f"in {cat_spend.category}, accounting for {cat_spend.percentage_of_total_expenses:.1f}% of all expenses."
        )

        budget_comp = None
        if budget:
            if budget.is_exceeded:
                budget_comp = (
                    f"Over Budget: You exceeded your R{budget.monthly_limit:,.2f} limit by "
                    f"R{(budget.current_spend - budget.monthly_limit):,.2f} ({budget.percentage_used:.0f}% used)."
                )
                recommended = f"Stop further discretionary {cat_spend.category} spending until next month."
            else:
                budget_comp = (
                    f"Within Budget: You have used {budget.percentage_used:.1f}% of your R{budget.monthly_limit:,.2f} limit. "
                    f"Remaining: R{budget.remaining_budget:,.2f}."
                )
                recommended = f"Keep your daily {cat_spend.category} expenses below R{(budget.remaining_budget / 10):,.2f} to stay on track."
        else:
            budget_comp = "No monthly budget limit has been configured for this category."
            recommended = f"Consider setting a monthly cap for {cat_spend.category} to avoid unplanned spending."

        return SpendingExplanationResponse(
            category=cat_spend.category,
            headline=headline,
            explanation=explanation,
            budget_comparison=budget_comp,
            recommended_action=recommended,
        )
