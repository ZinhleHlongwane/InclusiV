using Domain.Transactions;

namespace Domain.Calculations;

public sealed class CategorySpending
{
    public TransactionCategory Category { get; }
    public decimal TotalAmount { get; }
    public decimal PercentageOfTotalExpenses { get; }
    public int TransactionCount { get; }

    public CategorySpending(
        TransactionCategory category,
        decimal totalAmount,
        decimal percentageOfTotalExpenses,
        int transactionCount)
    {
        if (totalAmount < 0)
            throw new ArgumentOutOfRangeException(nameof(totalAmount), "Total amount cannot be negative.");

        if (percentageOfTotalExpenses < 0 || percentageOfTotalExpenses > 100)
            throw new ArgumentOutOfRangeException(nameof(percentageOfTotalExpenses), "Percentage must be between 0 and 100.");

        if (transactionCount < 0)
            throw new ArgumentOutOfRangeException(nameof(transactionCount), "Transaction count cannot be negative.");

        Category = category;
        TotalAmount = Math.Round(totalAmount, 2, MidpointRounding.AwayFromZero);
        PercentageOfTotalExpenses = Math.Round(percentageOfTotalExpenses, 2, MidpointRounding.AwayFromZero);
        TransactionCount = transactionCount;
    }
}
