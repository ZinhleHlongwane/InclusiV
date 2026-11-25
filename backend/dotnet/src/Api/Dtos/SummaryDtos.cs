namespace Api.Dtos;

public record CashFlowSummaryDto(
    decimal TotalIncome,
    decimal TotalExpenses,
    decimal NetCashFlow,
    string Currency,
    DateOnly? FromDate,
    DateOnly? ToDate);

public record CategorySpendingDto(
    string Category,
    decimal TotalAmount,
    decimal PercentageOfTotalExpenses,
    int TransactionCount);

public record BudgetStatusDto(
    Guid BudgetId,
    string Category,
    decimal MonthlyLimit,
    decimal CurrentSpend,
    decimal RemainingBudget,
    decimal PercentageUsed,
    string Health,
    bool IsExceeded);

public record SavingsGoalProgressDto(
    Guid GoalId,
    string Name,
    decimal TargetAmount,
    decimal CurrentAmount,
    decimal RemainingAmount,
    decimal PercentageAchieved,
    DateOnly? TargetDate,
    decimal? RecommendedMonthlySavings,
    bool IsCompleted);

public record FinancialSummaryResponse(
    Guid UserId,
    int Year,
    int Month,
    decimal TotalBalance,
    CashFlowSummaryDto CashFlow,
    IReadOnlyList<CategorySpendingDto> SpendingByCategory,
    IReadOnlyList<BudgetStatusDto> BudgetStatuses,
    IReadOnlyList<SavingsGoalProgressDto> SavingsGoals,
    int AccountCount,
    int TransactionCount,
    bool HasNegativeCashFlow,
    bool HasOverbudgetAlerts,
    decimal TotalSavings);
