using Domain.Budgets;
using Domain.Transactions;

namespace Domain.Tests;

public class BudgetTests
{
    [Fact]
    public void Constructor_WithValidData_CreatesBudget()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var budget = new Budget(
            id,
            userId,
            TransactionCategory.Food,
            1500m,
            CurrencyCode.ZAR);

        Assert.Equal(id, budget.Id);
        Assert.Equal(userId, budget.UserId);
        Assert.Equal(TransactionCategory.Food, budget.Category);
        Assert.Equal(1500m, budget.MonthlyLimit);
        Assert.Equal(CurrencyCode.ZAR, budget.Currency);
        Assert.Null(budget.UpdatedAt);
    }

    [Fact]
    public void Constructor_WithEmptyId_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new Budget(Guid.Empty, Guid.NewGuid(), TransactionCategory.Transport, 500m));

        Assert.Equal("id", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithEmptyUserId_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new Budget(Guid.NewGuid(), Guid.Empty, TransactionCategory.Transport, 500m));

        Assert.Equal("userId", exception.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-50)]
    public void Constructor_WithInvalidLimit_ThrowsArgumentOutOfRangeException(decimal limit)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new Budget(Guid.NewGuid(), Guid.NewGuid(), TransactionCategory.Education, limit));

        Assert.Equal("monthlyLimit", exception.ParamName);
    }

    [Fact]
    public void UpdateLimit_WithValidAmount_UpdatesLimitAndTimestamp()
    {
        var budget = new Budget(Guid.NewGuid(), Guid.NewGuid(), TransactionCategory.Utilities, 800m);
        var updateTime = DateTimeOffset.UtcNow;

        budget.UpdateLimit(1200m, updateTime);

        Assert.Equal(1200m, budget.MonthlyLimit);
        Assert.Equal(updateTime, budget.UpdatedAt);
    }

    [Fact]
    public void UpdateLimit_WithZeroOrNegative_ThrowsArgumentOutOfRangeException()
    {
        var budget = new Budget(Guid.NewGuid(), Guid.NewGuid(), TransactionCategory.Utilities, 800m);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            budget.UpdateLimit(0m));

        Assert.Equal("newLimit", exception.ParamName);
    }
}
