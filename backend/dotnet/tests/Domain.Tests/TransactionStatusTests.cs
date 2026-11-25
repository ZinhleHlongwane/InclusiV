using Domain.Transactions;

namespace Domain.Tests;

public class TransactionStatusTests
{
    [Fact]
    public void Constructor_WithoutStatus_DefaultsToCompleted()
    {
        var transaction = CreateTransaction();

        Assert.Equal(TransactionStatus.Completed, transaction.Status);
    }

    [Theory]
    [InlineData(TransactionStatus.Scheduled)]
    [InlineData(TransactionStatus.Pending)]
    [InlineData(TransactionStatus.Completed)]
    [InlineData(TransactionStatus.Failed)]
    public void Constructor_WithValidStatus_StoresStatus(
        TransactionStatus status)
    {
        var transaction = CreateTransaction(status: status);

        Assert.Equal(status, transaction.Status);
    }

    [Fact]
    public void Constructor_WithInvalidStatus_ThrowsArgumentOutOfRangeException()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreateTransaction(status: (TransactionStatus)999));

        Assert.Equal("status", exception.ParamName);
    }

    private static FinancialTransaction CreateTransaction(
        TransactionStatus status = TransactionStatus.Completed)
    {
        return new FinancialTransaction(
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            TransactionType.Expense,
            250m,
            "Test transaction",
            TransactionCategory.Other,
            new DateOnly(2026, 11, 16),
            status: status);
    }
}
