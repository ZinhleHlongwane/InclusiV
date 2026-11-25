using Domain.Transactions;

namespace Domain.Tests;

public class FinancialTransactionTests
{
    [Fact]
    public void Constructor_WithValidIncome_CreatesTransaction()
    {
        var id = Guid.NewGuid();
        var accountId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var date = new DateOnly(2026, 11, 16);

        var transaction = new FinancialTransaction(
            id,
            accountId,
            userId,
            TransactionType.Income,
            5000m,
            "Monthly allowance",
            TransactionCategory.Other,
            date);

        Assert.Equal(id, transaction.Id);
        Assert.Equal(accountId, transaction.AccountId);
        Assert.Equal(userId, transaction.UserId);
        Assert.Equal(TransactionType.Income, transaction.Type);
        Assert.Equal(5000m, transaction.Amount);
        Assert.Equal(CurrencyCode.ZAR, transaction.Currency);
        Assert.Equal("Monthly allowance", transaction.Description);
        Assert.Equal(TransactionCategory.Other, transaction.Category);
        Assert.Equal(date, transaction.TransactionDate);
    }

    [Fact]
    public void Constructor_WithValidExpense_CreatesTransaction()
    {
        var accountId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var transaction = new FinancialTransaction(
            Guid.NewGuid(),
            accountId,
            userId,
            TransactionType.Expense,
            250m,
            "Groceries",
            TransactionCategory.Food,
            new DateOnly(2026, 11, 16));

        Assert.Equal(accountId, transaction.AccountId);
        Assert.Equal(userId, transaction.UserId);
        Assert.Equal(TransactionType.Expense, transaction.Type);
        Assert.Equal(250m, transaction.Amount);
        Assert.Equal(TransactionCategory.Food, transaction.Category);
        Assert.Equal(CurrencyCode.ZAR, transaction.Currency);
    }

    [Fact]
    public void Constructor_WithExplicitZarCurrency_StoresCurrency()
    {
        var transaction = CreateTransaction(currency: CurrencyCode.ZAR);

        Assert.Equal(CurrencyCode.ZAR, transaction.Currency);
    }

    [Fact]
    public void Constructor_WithInvalidCurrency_ThrowsArgumentOutOfRangeException()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreateTransaction(currency: (CurrencyCode)999));

        Assert.Equal("currency", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithFractionalAmount_PreservesExactDecimalValue()
    {
        var transaction = CreateTransaction(amount: 125.50m);

        Assert.Equal(125.50m, transaction.Amount);
    }

    [Fact]
    public void Constructor_WithSmallFractionalAmount_PreservesExactDecimalValue()
    {
        var transaction = CreateTransaction(amount: 0.01m);

        Assert.Equal(0.01m, transaction.Amount);
    }

    [Fact]
    public void Constructor_WithEmptyId_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            CreateTransaction(id: Guid.Empty));

        Assert.Equal("id", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithEmptyAccountId_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            CreateTransaction(accountId: Guid.Empty));

        Assert.Equal("accountId", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithEmptyUserId_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            CreateTransaction(userId: Guid.Empty));

        Assert.Equal("userId", exception.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void Constructor_WithInvalidAmount_ThrowsArgumentOutOfRangeException(
        decimal amount)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreateTransaction(amount: amount));

        Assert.Equal("amount", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithBlankDescription_ThrowsArgumentException(
        string description)
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            CreateTransaction(description: description));

        Assert.Equal("description", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithInvalidType_ThrowsArgumentOutOfRangeException()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreateTransaction(type: (TransactionType)999));

        Assert.Equal("type", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithInvalidCategory_ThrowsArgumentOutOfRangeException()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreateTransaction(category: (TransactionCategory)999));

        Assert.Equal("category", exception.ParamName);
    }

    [Fact]
    public void Constructor_TrimsDescription()
    {
        var transaction = CreateTransaction(
            description: "  Transport  ");

        Assert.Equal("Transport", transaction.Description);
    }

    [Fact]
    public void Constructor_WithExplicitRecordedAt_PreservesTimestamp()
    {
        var recordedAt = new DateTimeOffset(
            2026, 11, 16, 10, 30, 0, TimeSpan.FromHours(2));

        var transaction = CreateTransaction(recordedAt: recordedAt);

        Assert.Equal(recordedAt, transaction.RecordedAt);
    }

    [Fact]
    public void Constructor_WithoutRecordedAt_UsesUtcTimestamp()
    {
        var beforeCreation = DateTimeOffset.UtcNow;

        var transaction = CreateTransaction();

        var afterCreation = DateTimeOffset.UtcNow;

        Assert.Equal(TimeSpan.Zero, transaction.RecordedAt.Offset);
        Assert.InRange(
            transaction.RecordedAt,
            beforeCreation,
            afterCreation);
    }

    [Fact]
    public void Constructor_WithHistoricalTransactionDate_AcceptsTransaction()
    {
        var historicalDate = new DateOnly(2020, 1, 15);

        var transaction = new FinancialTransaction(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            TransactionType.Expense,
            150m,
            "Historical purchase",
            TransactionCategory.Food,
            historicalDate);

        Assert.Equal(historicalDate, transaction.TransactionDate);
    }

    [Fact]
    public void Constructor_WithFutureTransactionDate_AcceptsTransaction()
    {
        var futureDate = new DateOnly(2030, 5, 20);

        var transaction = new FinancialTransaction(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            TransactionType.Expense,
            300m,
            "Scheduled payment",
            TransactionCategory.Utilities,
            futureDate);

        Assert.Equal(futureDate, transaction.TransactionDate);
    }

    [Fact]
    public void UpdateStatus_WithValidStatus_UpdatesStatus()
    {
        var transaction = CreateTransaction();

        transaction.UpdateStatus(TransactionStatus.Pending);

        Assert.Equal(TransactionStatus.Pending, transaction.Status);
    }

    [Fact]
    public void UpdateStatus_WithInvalidStatus_ThrowsArgumentOutOfRangeException()
    {
        var transaction = CreateTransaction();

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            transaction.UpdateStatus((TransactionStatus)999));

        Assert.Equal("newStatus", exception.ParamName);
    }

    private static FinancialTransaction CreateTransaction(
        Guid? id = null,
        Guid? accountId = null,
        Guid? userId = null,
        TransactionType type = TransactionType.Expense,
        decimal amount = 100m,
        string description = "Test transaction",
        TransactionCategory category = TransactionCategory.Other,
        CurrencyCode currency = CurrencyCode.ZAR,
        DateTimeOffset? recordedAt = null)
    {
        return new FinancialTransaction(
            id ?? Guid.NewGuid(),
            accountId ?? Guid.NewGuid(),
            userId ?? Guid.NewGuid(),
            type,
            amount,
            description,
            category,
            new DateOnly(2026, 11, 16),
            currency,
            recordedAt);
    }
}
