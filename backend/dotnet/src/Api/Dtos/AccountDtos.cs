namespace Api.Dtos;

public record CreateAccountRequest(
    Guid UserId,
    string Name,
    string Type,
    string? Currency = "ZAR");

public record FinancialAccountResponse(
    Guid Id,
    Guid UserId,
    string Name,
    string Type,
    string Currency,
    decimal Balance,
    DateTimeOffset CreatedAt);
