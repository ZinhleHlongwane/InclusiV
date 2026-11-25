namespace Api.Dtos;

public record CreateTransactionRequest(
    Guid AccountId,
    Guid UserId,
    string Type,
    decimal Amount,
    string Description,
    string Category,
    DateOnly TransactionDate,
    string? Currency = "ZAR",
    string? Status = "Completed");

public record UpdateTransactionStatusRequest(
    string Status);

public record FinancialTransactionResponse(
    Guid Id,
    Guid AccountId,
    Guid UserId,
    string Type,
    string Status,
    decimal Amount,
    string Currency,
    string Description,
    string Category,
    DateOnly TransactionDate,
    DateTimeOffset RecordedAt);
