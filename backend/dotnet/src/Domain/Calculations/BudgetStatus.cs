using Domain.Transactions;

namespace Domain.Calculations;

public enum BudgetHealth
{
    OnTrack,
    Warning,
    Exceeded
}

public sealed class BudgetStatus
{
    public Guid BudgetId { get; }
    public TransactionCategory Category { get; }
    public decimal MonthlyLimit { get; }
    public decimal CurrentSpend { get; }
    public decimal RemainingBudget => MonthlyLimit - CurrentSpend;
    public decimal PercentageUsed { get; }
    public bool IsExceeded => CurrentSpend > MonthlyLimit;
    public BudgetHealth Health { get; }

    public BudgetStatus(
        Guid budgetId,
        TransactionCategory category,
        decimal monthlyLimit,
        decimal currentSpend)
    {
        if (budgetId == Guid.Empty)
            throw new ArgumentException("Budget ID cannot be empty.", nameof(budgetId));

        if (monthlyLimit <= 0)
            throw new ArgumentOutOfRangeException(nameof(monthlyLimit), "Monthly limit must be greater than zero.");

        if (currentSpend < 0)
            throw new ArgumentOutOfRangeException(nameof(currentSpend), "Current spend cannot be negative.");

        BudgetId = budgetId;
        Category = category;
        MonthlyLimit = Math.Round(monthlyLimit, 2, MidpointRounding.AwayFromZero);
        CurrentSpend = Math.Round(currentSpend, 2, MidpointRounding.AwayFromZero);

        var pct = (currentSpend / monthlyLimit) * 100m;
        PercentageUsed = Math.Round(pct, 2, MidpointRounding.AwayFromZero);

        if (currentSpend > monthlyLimit)
        {
            Health = BudgetHealth.Exceeded;
        }
        else if (PercentageUsed >= 80m)
        {
            Health = BudgetHealth.Warning;
        }
        else
        {
            Health = BudgetHealth.OnTrack;
        }
    }
}
