from platform_agents.models import (
    FinancialSummaryModel,
    SavingsGoalInsight,
    SavingsGoalsResponse,
)


class SavingsGoalsAgent:
    """Rule-based agent for savings goal progress and planning."""

    @staticmethod
    def review_goals(
        user_id: str,
        summary: FinancialSummaryModel | None,
        query: str | None = None,
    ) -> SavingsGoalsResponse:
        if summary is None:
            return SavingsGoalsResponse(
                user_id=user_id,
                headline="Let's start with a savings goal",
                overview="There isn't enough financial information available to review your savings yet.",
                recommendations=[
                    "Choose a specific goal, such as an emergency fund, study costs or a device.",
                    "Set a target amount that is realistic for your circumstances.",
                    "Record contributions so your progress can be tracked over time.",
                ],
                next_steps=[
                    "Create one savings goal with a target amount.",
                    "Record your first contribution when you are able to save.",
                ],
            )

        goals = summary.savings_goals
        if not goals:
            return SavingsGoalsResponse(
                user_id=user_id,
                headline="You haven't set up a savings goal yet",
                overview="No savings goals were found in your financial summary.",
                recommendations=[
                    "Start with a goal that matters to you, such as an emergency buffer, education costs or transport.",
                    "Choose a target based on what you can realistically set aside after essential expenses.",
                    "You can begin with a small target and increase it as your circumstances change.",
                ],
                next_steps=[
                    "Create your first savings goal and give it a clear target amount.",
                    "Add contributions when you save so the progress stays up to date.",
                ],
            )

        insights: list[SavingsGoalInsight] = []
        recommendations: list[str] = []
        completed_count = 0
        active_count = 0

        for goal in goals:
            insights.append(
                SavingsGoalInsight(
                    goal_id=goal.goal_id,
                    goal_name=goal.name,
                    target_amount=goal.target_amount,
                    current_amount=goal.current_amount,
                    remaining_amount=goal.remaining_amount,
                    percentage_achieved=goal.percentage_achieved,
                    recommended_monthly_savings=goal.recommended_monthly_savings,
                    target_date=goal.target_date,
                    is_completed=goal.is_completed,
                )
            )

            if goal.is_completed:
                completed_count += 1
                recommendations.append(
                    f"You've completed '{goal.name}'. Celebrate the progress and decide whether you want to maintain this fund or set a new target."
                )
                continue

            active_count += 1
            if goal.remaining_amount <= 0:
                recommendations.append(
                    f"'{goal.name}' appears to have reached its target. Review the goal status to make sure it is marked complete."
                )
            elif goal.recommended_monthly_savings and goal.recommended_monthly_savings > 0:
                recommendations.append(
                    f"To stay on track with '{goal.name}', the current estimate is R{goal.recommended_monthly_savings:,.2f} per month. Check that this fits after essential expenses."
                )
            else:
                recommendations.append(
                    f"'{goal.name}' is {goal.percentage_achieved:.1f}% complete, with R{goal.remaining_amount:,.2f} remaining. Consider a contribution amount that is comfortable and sustainable."
                )

        if active_count == 0:
            headline = "All your savings goals are complete"
            next_steps = [
                "Decide whether to create a new goal or keep your completed savings available for its intended purpose."
            ]
        else:
            headline = "Your savings progress"
            next_steps = [
                "Review your essential expenses before committing to the suggested monthly savings amount.",
                "Record each contribution so your goal progress remains accurate.",
            ]

        if completed_count:
            overview = (
                f"You have {len(goals)} savings goal(s): {completed_count} completed "
                f"and {active_count} still active."
            )
        else:
            overview = f"You have {active_count} active savings goal(s) to work towards."

        if query and any(term in query.lower() for term in ("prioritise", "priority", "which first", "where should i start")):
            active_goals = [goal for goal in goals if not goal.is_completed]
            if active_goals:
                smallest_gap = min(active_goals, key=lambda goal: goal.remaining_amount)
                next_steps.insert(
                    0,
                    f"Review '{smallest_gap.name}' first if completing a smaller remaining target would help build momentum; still consider urgency and essential needs.",
                )

        return SavingsGoalsResponse(
            user_id=user_id,
            headline=headline,
            overview=overview,
            insights=insights,
            recommendations=recommendations,
            next_steps=next_steps,
        )
