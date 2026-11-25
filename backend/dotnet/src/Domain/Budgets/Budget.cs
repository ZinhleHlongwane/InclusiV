using Domain.Transactions;

namespace Domain.Budgets;

public sealed class Budget
{
    public Guid Id { get; }
    public Guid UserId { get; }
    public TransactionCategory Category { get; }
    public decimal MonthlyLimit { get; private set; }
    public CurrencyCode Currency { get; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public Budget(
        Guid id,
        Guid userId,
        TransactionCategory category,
        decimal monthlyLimit,
        CurrencyCode currency = CurrencyCode.ZAR,
        DateTimeOffset? createdAt = null,
        DateTimeOffset? updatedAt = null)
    {
        if (id == Guid.Empty)
            throw new ArgumentException(
                "Budget ID cannot be empty.", nameof(id));

        if (userId == Guid.Empty)
            throw new ArgumentException(
                "User ID cannot be empty.", nameof(userId));

        if (monthlyLimit <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(monthlyLimit), "Monthly budget limit must be greater than zero.");

        if (!Enum.IsDefined(category))
            throw new ArgumentOutOfRangeException(nameof(category));

        if (!Enum.IsDefined(currency))
            throw new ArgumentOutOfRangeException(nameof(currency));

        Id = id;
        UserId = userId;
        Category = category;
        MonthlyLimit = monthlyLimit;
        Currency = currency;
        CreatedAt = createdAt ?? DateTimeOffset.UtcNow;
        UpdatedAt = updatedAt;
    }

    public void UpdateLimit(decimal newLimit, DateTimeOffset? updatedAt = null)
    {
        if (newLimit <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(newLimit), "Monthly budget limit must be greater than zero.");

        MonthlyLimit = newLimit;
        UpdatedAt = updatedAt ?? DateTimeOffset.UtcNow;
    }
}
