namespace Domain.Calculations;

public sealed class FinancialSummary
{
    public Guid UserId { get; }
    public int Year { get; }
    public int Month { get; }
    public decimal TotalBalance { get; }
    public CashFlowSummary CashFlow { get; }
    public IReadOnlyList<CategorySpending> SpendingByCategory { get; }
    public IReadOnlyList<BudgetStatus> BudgetStatuses { get; }
    public IReadOnlyList<SavingsGoalProgress> SavingsGoals { get; }
    public int AccountCount { get; }
    public int TransactionCount { get; }

    public bool HasNegativeCashFlow => CashFlow.NetCashFlow < 0;
    public bool HasOverbudgetAlerts => BudgetStatuses.Any(b => b.IsExceeded);
    public decimal TotalSavings => SavingsGoals.Sum(g => g.CurrentAmount);

    public FinancialSummary(
        Guid userId,
        int year,
        int month,
        decimal totalBalance,
        CashFlowSummary cashFlow,
        IReadOnlyList<CategorySpending> spendingByCategory,
        IReadOnlyList<BudgetStatus> budgetStatuses,
        IReadOnlyList<SavingsGoalProgress> savingsGoals,
        int accountCount,
        int transactionCount)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        if (month < 1 || month > 12)
            throw new ArgumentOutOfRangeException(nameof(month), "Month must be between 1 and 12.");

        UserId = userId;
        Year = year;
        Month = month;
        TotalBalance = Math.Round(totalBalance, 2, MidpointRounding.AwayFromZero);
        CashFlow = cashFlow ?? throw new ArgumentNullException(nameof(cashFlow));
        SpendingByCategory = spendingByCategory ?? Array.Empty<CategorySpending>();
        BudgetStatuses = budgetStatuses ?? Array.Empty<BudgetStatus>();
        SavingsGoals = savingsGoals ?? Array.Empty<SavingsGoalProgress>();
        AccountCount = accountCount;
        TransactionCount = transactionCount;
    }
}
