namespace Api.Dtos;

public record CreateSavingsGoalRequest(
    Guid UserId,
    string Name,
    decimal TargetAmount,
    decimal? CurrentAmount = 0m,
    string? Currency = "ZAR",
    DateOnly? TargetDate = null);

public record ContributeSavingsRequest(
    decimal Amount);

public record UpdateSavingsGoalRequest(
    string Name,
    decimal TargetAmount,
    DateOnly? TargetDate);

public record SavingsGoalResponse(
    Guid Id,
    Guid UserId,
    string Name,
    decimal TargetAmount,
    decimal CurrentAmount,
    decimal RemainingAmount,
    decimal PercentageAchieved,
    string Currency,
    DateOnly? TargetDate,
    decimal? RecommendedMonthlySavings,
    bool IsCompleted,
    DateTimeOffset CreatedAt);
