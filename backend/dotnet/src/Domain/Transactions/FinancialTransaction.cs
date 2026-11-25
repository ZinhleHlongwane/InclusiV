namespace Domain.Transactions;

public sealed class FinancialTransaction
{
    public Guid Id { get; }
    public Guid AccountId { get; }
    public Guid UserId { get; }
    public TransactionType Type { get; }
    public TransactionStatus Status { get; }
    public decimal Amount { get; }
    public CurrencyCode Currency { get; }
    public string Description { get; }
    public TransactionCategory Category { get; }
    public DateOnly TransactionDate { get; }
    public DateTimeOffset RecordedAt { get; }

    public FinancialTransaction(
        Guid id,
        Guid accountId,
        Guid userId,
        TransactionType type,
        decimal amount,
        string description,
        TransactionCategory category,
        DateOnly transactionDate,
        CurrencyCode currency = CurrencyCode.ZAR,
        DateTimeOffset? recordedAt = null,
        TransactionStatus status = TransactionStatus.Completed)
    {
        if (id == Guid.Empty)
            throw new ArgumentException(
                "Transaction ID cannot be empty.", nameof(id));

        if (accountId == Guid.Empty)
            throw new ArgumentException(
                "Account ID cannot be empty.", nameof(accountId));

        if (userId == Guid.Empty)
            throw new ArgumentException(
                "User ID cannot be empty.", nameof(userId));

        if (amount <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(amount), "Transaction amount must be greater than zero.");

        if (string.IsNullOrWhiteSpace(description))
            throw new ArgumentException(
                "Description is required.", nameof(description));

        if (!Enum.IsDefined(type))
            throw new ArgumentOutOfRangeException(nameof(type));

        if (!Enum.IsDefined(currency))
            throw new ArgumentOutOfRangeException(nameof(currency));

        if (!Enum.IsDefined(status))
            throw new ArgumentOutOfRangeException(nameof(status));

        if (!Enum.IsDefined(category))
            throw new ArgumentOutOfRangeException(nameof(category));

        Id = id;
        AccountId = accountId;
        UserId = userId;
        Type = type;
        Status = status;
        Amount = amount;
        Currency = currency;
        Description = description.Trim();
        Category = category;
        TransactionDate = transactionDate;
        RecordedAt = recordedAt ?? DateTimeOffset.UtcNow;
    }
}
