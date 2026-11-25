namespace Api.Dtos;

public record CreateBudgetRequest(
    Guid UserId,
    string Category,
    decimal MonthlyLimit,
    string? Currency = "ZAR");

public record UpdateBudgetLimitRequest(
    decimal MonthlyLimit);

public record BudgetResponse(
    Guid Id,
    Guid UserId,
    string Category,
    decimal MonthlyLimit,
    string Currency,
    decimal CurrentSpend,
    decimal RemainingBudget,
    decimal PercentageUsed,
    string Health,
    bool IsExceeded,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
