using Domain.Transactions;

namespace Domain.Accounts;

public sealed class FinancialAccount
{
    public Guid Id { get; }
    public string Name { get; }
    public AccountType Type { get; }
    public CurrencyCode Currency { get; }
    public DateTimeOffset CreatedAt { get; }

    public FinancialAccount(
        Guid id,
        string name,
        AccountType type,
        CurrencyCode currency = CurrencyCode.ZAR,
        DateTimeOffset? createdAt = null)
    {
        if (id == Guid.Empty)
            throw new ArgumentException(
                "Account ID cannot be empty.", nameof(id));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "Account name is required.", nameof(name));

        if (!Enum.IsDefined(type))
            throw new ArgumentOutOfRangeException(nameof(type));

        if (!Enum.IsDefined(currency))
            throw new ArgumentOutOfRangeException(nameof(currency));

        Id = id;
        Name = name.Trim();
        Type = type;
        Currency = currency;
        CreatedAt = createdAt ?? DateTimeOffset.UtcNow;
    }
}
