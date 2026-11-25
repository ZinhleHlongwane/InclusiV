namespace Domain.Calculations;

public sealed class SavingsGoalProgress
{
    public Guid GoalId { get; }
    public string Name { get; }
    public decimal TargetAmount { get; }
    public decimal CurrentAmount { get; }
    public decimal RemainingAmount => Math.Max(0m, TargetAmount - CurrentAmount);
    public decimal PercentageAchieved { get; }
    public DateOnly? TargetDate { get; }
    public decimal? RecommendedMonthlySavings { get; }
    public bool IsCompleted => CurrentAmount >= TargetAmount;

    public SavingsGoalProgress(
        Guid goalId,
        string name,
        decimal targetAmount,
        decimal currentAmount,
        DateOnly? targetDate = null,
        decimal? recommendedMonthlySavings = null)
    {
        if (goalId == Guid.Empty)
            throw new ArgumentException("Goal ID cannot be empty.", nameof(goalId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Goal name cannot be empty.", nameof(name));

        if (targetAmount <= 0)
            throw new ArgumentOutOfRangeException(nameof(targetAmount), "Target amount must be greater than zero.");

        if (currentAmount < 0)
            throw new ArgumentOutOfRangeException(nameof(currentAmount), "Current amount cannot be negative.");

        GoalId = goalId;
        Name = name.Trim();
        TargetAmount = Math.Round(targetAmount, 2, MidpointRounding.AwayFromZero);
        CurrentAmount = Math.Round(currentAmount, 2, MidpointRounding.AwayFromZero);

        var pct = (currentAmount / targetAmount) * 100m;
        PercentageAchieved = Math.Round(pct, 2, MidpointRounding.AwayFromZero);
        TargetDate = targetDate;
        RecommendedMonthlySavings = recommendedMonthlySavings.HasValue
            ? Math.Round(recommendedMonthlySavings.Value, 2, MidpointRounding.AwayFromZero)
            : null;
    }
}
