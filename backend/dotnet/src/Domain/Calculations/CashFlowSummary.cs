using Domain.Transactions;

namespace Domain.Calculations;

public sealed class CashFlowSummary
{
    public decimal TotalIncome { get; }
    public decimal TotalExpenses { get; }
    public decimal NetCashFlow => TotalIncome - TotalExpenses;
    public CurrencyCode Currency { get; }
    public DateOnly? FromDate { get; }
    public DateOnly? ToDate { get; }

    public CashFlowSummary(
        decimal totalIncome,
        decimal totalExpenses,
        CurrencyCode currency = CurrencyCode.ZAR,
        DateOnly? fromDate = null,
        DateOnly? toDate = null)
    {
        if (totalIncome < 0)
            throw new ArgumentOutOfRangeException(nameof(totalIncome), "Total income cannot be negative.");

        if (totalExpenses < 0)
            throw new ArgumentOutOfRangeException(nameof(totalExpenses), "Total expenses cannot be negative.");

        TotalIncome = Math.Round(totalIncome, 2, MidpointRounding.AwayFromZero);
        TotalExpenses = Math.Round(totalExpenses, 2, MidpointRounding.AwayFromZero);
        Currency = currency;
        FromDate = fromDate;
        ToDate = toDate;
    }
}
