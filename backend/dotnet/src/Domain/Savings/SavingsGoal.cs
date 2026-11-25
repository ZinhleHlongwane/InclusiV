using Domain.Transactions;

namespace Domain.Savings;

public sealed class SavingsGoal
{
    public Guid Id { get; }
    public Guid UserId { get; }
    public string Name { get; private set; }
    public decimal TargetAmount { get; private set; }
    public decimal CurrentAmount { get; private set; }
    public CurrencyCode Currency { get; }
    public DateOnly? TargetDate { get; private set; }
    public DateTimeOffset CreatedAt { get; }

    public SavingsGoal(
        Guid id,
        Guid userId,
        string name,
        decimal targetAmount,
        decimal currentAmount = 0m,
        CurrencyCode currency = CurrencyCode.ZAR,
        DateOnly? targetDate = null,
        DateTimeOffset? createdAt = null)
    {
        if (id == Guid.Empty)
            throw new ArgumentException(
                "Savings goal ID cannot be empty.", nameof(id));

        if (userId == Guid.Empty)
            throw new ArgumentException(
                "User ID cannot be empty.", nameof(userId));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "Goal name is required.", nameof(name));

        if (targetAmount <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(targetAmount), "Target amount must be greater than zero.");

        if (currentAmount < 0)
            throw new ArgumentOutOfRangeException(
                nameof(currentAmount), "Current savings amount cannot be negative.");

        if (!Enum.IsDefined(currency))
            throw new ArgumentOutOfRangeException(nameof(currency));

        Id = id;
        UserId = userId;
        Name = name.Trim();
        TargetAmount = targetAmount;
        CurrentAmount = currentAmount;
        Currency = currency;
        TargetDate = targetDate;
        CreatedAt = createdAt ?? DateTimeOffset.UtcNow;
    }

    public void AddSavings(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(amount), "Deposit amount must be greater than zero.");

        CurrentAmount += amount;
    }

    public void UpdateGoal(string name, decimal targetAmount, DateOnly? targetDate = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException(
                "Goal name is required.", nameof(name));

        if (targetAmount <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(targetAmount), "Target amount must be greater than zero.");

        Name = name.Trim();
        TargetAmount = targetAmount;
        TargetDate = targetDate;
    }
}
