using Domain.Accounts;
using Domain.Budgets;
using Domain.Calculations;
using Domain.Savings;
using Domain.Transactions;

namespace Domain.Tests.Calculations;

public class FinancialCalculationEngineTests
{
    [Fact]
    public void CalculateAccountBalance_WithMixedTransactions_ReturnsCorrectBalance()
    {
        var accountId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var transactions = new List<FinancialTransaction>
        {
            new(Guid.NewGuid(), accountId, userId, TransactionType.Income, 1000m, "Deposit", TransactionCategory.Other, new DateOnly(2026, 11, 1)),
            new(Guid.NewGuid(), accountId, userId, TransactionType.Expense, 250.50m, "Groceries", TransactionCategory.Food, new DateOnly(2026, 11, 2)),
            new(Guid.NewGuid(), accountId, userId, TransactionType.Expense, 49.50m, "Transport", TransactionCategory.Transport, new DateOnly(2026, 11, 3))
        };

        var balance = FinancialCalculationEngine.CalculateAccountBalance(accountId, transactions);

        Assert.Equal(700.00m, balance);
    }

    [Fact]
    public void CalculateAccountBalance_IgnoresFailedTransactions()
    {
        var accountId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var transactions = new List<FinancialTransaction>
        {
            new(Guid.NewGuid(), accountId, userId, TransactionType.Income, 1000m, "Deposit", TransactionCategory.Other, new DateOnly(2026, 11, 1)),
            new(Guid.NewGuid(), accountId, userId, TransactionType.Expense, 500m, "Failed tx", TransactionCategory.Other, new DateOnly(2026, 11, 2), status: TransactionStatus.Failed)
        };

        var balance = FinancialCalculationEngine.CalculateAccountBalance(accountId, transactions);

        Assert.Equal(1000m, balance);
    }

    [Fact]
    public void CalculateAccountBalance_HandlesPendingTransactionsCorrectly()
    {
        var accountId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var transactions = new List<FinancialTransaction>
        {
            new(Guid.NewGuid(), accountId, userId, TransactionType.Income, 1000m, "Deposit", TransactionCategory.Other, new DateOnly(2026, 11, 1)),
            new(Guid.NewGuid(), accountId, userId, TransactionType.Expense, 200m, "Pending card", TransactionCategory.Food, new DateOnly(2026, 11, 2), status: TransactionStatus.Pending)
        };

        var settledBalance = FinancialCalculationEngine.CalculateAccountBalance(accountId, transactions, includePending: false);
        var availableBalance = FinancialCalculationEngine.CalculateAccountBalance(accountId, transactions, includePending: true);

        Assert.Equal(1000m, settledBalance);
        Assert.Equal(800m, availableBalance);
    }

    [Fact]
    public void CalculateCashFlow_CalculatesTotalIncomeExpensesAndNet()
    {
        var accountId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var transactions = new List<FinancialTransaction>
        {
            new(Guid.NewGuid(), accountId, userId, TransactionType.Income, 3500m, "Stipend", TransactionCategory.Other, new DateOnly(2026, 11, 5)),
            new(Guid.NewGuid(), accountId, userId, TransactionType.Income, 500m, "Tutoring", TransactionCategory.Other, new DateOnly(2026, 11, 15)),
            new(Guid.NewGuid(), accountId, userId, TransactionType.Expense, 1200m, "Rent", TransactionCategory.Housing, new DateOnly(2026, 11, 6)),
            new(Guid.NewGuid(), accountId, userId, TransactionType.Expense, 800m, "Food", TransactionCategory.Food, new DateOnly(2026, 11, 10))
        };

        var cashFlow = FinancialCalculationEngine.CalculateCashFlow(transactions);

        Assert.Equal(4000m, cashFlow.TotalIncome);
        Assert.Equal(2000m, cashFlow.TotalExpenses);
        Assert.Equal(2000m, cashFlow.NetCashFlow);
    }

    [Fact]
    public void CalculateCashFlow_FiltersByDateRange()
    {
        var accountId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var transactions = new List<FinancialTransaction>
        {
            new(Guid.NewGuid(), accountId, userId, TransactionType.Income, 1000m, "October Income", TransactionCategory.Other, new DateOnly(2026, 10, 31)),
            new(Guid.NewGuid(), accountId, userId, TransactionType.Income, 2000m, "November Income", TransactionCategory.Other, new DateOnly(2026, 11, 5)),
            new(Guid.NewGuid(), accountId, userId, TransactionType.Expense, 500m, "November Expense", TransactionCategory.Food, new DateOnly(2026, 11, 10))
        };

        var cashFlow = FinancialCalculationEngine.CalculateCashFlow(
            transactions,
            fromDate: new DateOnly(2026, 11, 1),
            toDate: new DateOnly(2026, 11, 30));

        Assert.Equal(2000m, cashFlow.TotalIncome);
        Assert.Equal(500m, cashFlow.TotalExpenses);
        Assert.Equal(1500m, cashFlow.NetCashFlow);
    }

    [Fact]
    public void CalculateCategoryBreakdown_CalculatesAmountsAndPercentages()
    {
        var accountId = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var transactions = new List<FinancialTransaction>
        {
            new(Guid.NewGuid(), accountId, userId, TransactionType.Expense, 750m, "Groceries", TransactionCategory.Food, new DateOnly(2026, 11, 10)),
            new(Guid.NewGuid(), accountId, userId, TransactionType.Expense, 250m, "Bus ticket", TransactionCategory.Transport, new DateOnly(2026, 11, 12))
        };

        var breakdown = FinancialCalculationEngine.CalculateCategoryBreakdown(transactions);

        Assert.Equal(2, breakdown.Count);
        Assert.Equal(TransactionCategory.Food, breakdown[0].Category);
        Assert.Equal(750m, breakdown[0].TotalAmount);
        Assert.Equal(75.00m, breakdown[0].PercentageOfTotalExpenses);

        Assert.Equal(TransactionCategory.Transport, breakdown[1].Category);
        Assert.Equal(250m, breakdown[1].TotalAmount);
        Assert.Equal(25.00m, breakdown[1].PercentageOfTotalExpenses);
    }

    [Fact]
    public void CalculateCategoryBreakdown_WithZeroExpenses_ReturnsEmptyList()
    {
        var transactions = new List<FinancialTransaction>();

        var breakdown = FinancialCalculationEngine.CalculateCategoryBreakdown(transactions);

        Assert.Empty(breakdown);
    }

    [Fact]
    public void CalculateBudgetStatuses_CalculatesSpendAndIdentifiesWarningAndExceeded()
    {
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();

        var budgets = new List<Budget>
        {
            new(Guid.NewGuid(), userId, TransactionCategory.Food, 1000m),
            new(Guid.NewGuid(), userId, TransactionCategory.Transport, 500m),
            new(Guid.NewGuid(), userId, TransactionCategory.Entertainment, 300m)
        };

        var transactions = new List<FinancialTransaction>
        {
            // Food: 850 / 1000 = 85% (Warning)
            new(Guid.NewGuid(), accountId, userId, TransactionType.Expense, 850m, "Food", TransactionCategory.Food, new DateOnly(2026, 11, 10)),
            // Transport: 550 / 500 = 110% (Exceeded)
            new(Guid.NewGuid(), accountId, userId, TransactionType.Expense, 550m, "Transport", TransactionCategory.Transport, new DateOnly(2026, 11, 15)),
            // Entertainment: 100 / 300 = 33.33% (OnTrack)
            new(Guid.NewGuid(), accountId, userId, TransactionType.Expense, 100m, "Movies", TransactionCategory.Entertainment, new DateOnly(2026, 11, 20))
        };

        var statuses = FinancialCalculationEngine.CalculateBudgetStatuses(budgets, transactions, 2026, 11);

        var foodStatus = statuses.First(s => s.Category == TransactionCategory.Food);
        Assert.Equal(850m, foodStatus.CurrentSpend);
        Assert.Equal(150m, foodStatus.RemainingBudget);
        Assert.Equal(85.00m, foodStatus.PercentageUsed);
        Assert.Equal(BudgetHealth.Warning, foodStatus.Health);
        Assert.False(foodStatus.IsExceeded);

        var transportStatus = statuses.First(s => s.Category == TransactionCategory.Transport);
        Assert.Equal(550m, transportStatus.CurrentSpend);
        Assert.Equal(-50m, transportStatus.RemainingBudget);
        Assert.Equal(110.00m, transportStatus.PercentageUsed);
        Assert.Equal(BudgetHealth.Exceeded, transportStatus.Health);
        Assert.True(transportStatus.IsExceeded);

        var entStatus = statuses.First(s => s.Category == TransactionCategory.Entertainment);
        Assert.Equal(100m, entStatus.CurrentSpend);
        Assert.Equal(200m, entStatus.RemainingBudget);
        Assert.Equal(33.33m, entStatus.PercentageUsed);
        Assert.Equal(BudgetHealth.OnTrack, entStatus.Health);
        Assert.False(entStatus.IsExceeded);
    }

    [Fact]
    public void CalculateSavingsProgress_CalculatesPercentageAndRecommendedMonthlySavings()
    {
        var userId = Guid.NewGuid();
        var asOfDate = new DateOnly(2026, 11, 15);

        var goals = new List<SavingsGoal>
        {
            // Target: 2000, Current: 500, TargetDate: 2027-02-15 (3 months remaining) -> (1500 / 3) = 500/month
            new(Guid.NewGuid(), userId, "Laptop Fund", 2000m, 500m, CurrencyCode.ZAR, new DateOnly(2027, 2, 15)),
            // Already completed
            new(Guid.NewGuid(), userId, "Course Fee", 1000m, 1000m, CurrencyCode.ZAR, new DateOnly(2027, 1, 1))
        };

        var progress = FinancialCalculationEngine.CalculateSavingsProgress(goals, asOfDate);

        var laptop = progress.First(p => p.Name == "Laptop Fund");
        Assert.Equal(25.00m, laptop.PercentageAchieved);
        Assert.Equal(1500m, laptop.RemainingAmount);
        Assert.Equal(500.00m, laptop.RecommendedMonthlySavings);
        Assert.False(laptop.IsCompleted);

        var course = progress.First(p => p.Name == "Course Fee");
        Assert.Equal(100.00m, course.PercentageAchieved);
        Assert.Equal(0m, course.RemainingAmount);
        Assert.Null(course.RecommendedMonthlySavings);
        Assert.True(course.IsCompleted);
    }

    [Fact]
    public void CalculateFinancialSummary_AggregatesAllMetricsAndFlagsCorrectly()
    {
        var userId = Guid.NewGuid();
        var accountId = Guid.NewGuid();

        var accounts = new List<FinancialAccount>
        {
            new(accountId, userId, "Student Account", AccountType.BankAccount)
        };

        var transactions = new List<FinancialTransaction>
        {
            new(Guid.NewGuid(), accountId, userId, TransactionType.Income, 2000m, "Stipend", TransactionCategory.Other, new DateOnly(2026, 11, 1)),
            new(Guid.NewGuid(), accountId, userId, TransactionType.Expense, 2500m, "Textbooks", TransactionCategory.Education, new DateOnly(2026, 11, 5))
        };

        var budgets = new List<Budget>
        {
            new(Guid.NewGuid(), userId, TransactionCategory.Education, 2000m)
        };

        var goals = new List<SavingsGoal>
        {
            new(Guid.NewGuid(), userId, "Emergency", 1000m, 200m)
        };

        var summary = FinancialCalculationEngine.CalculateFinancialSummary(
            userId, accounts, transactions, budgets, goals, 2026, 11);

        Assert.Equal(userId, summary.UserId);
        Assert.Equal(-500.00m, summary.TotalBalance);
        Assert.Equal(2000.00m, summary.CashFlow.TotalIncome);
        Assert.Equal(2500.00m, summary.CashFlow.TotalExpenses);
        Assert.Equal(-500.00m, summary.CashFlow.NetCashFlow);
        Assert.True(summary.HasNegativeCashFlow);
        Assert.True(summary.HasOverbudgetAlerts);
        Assert.Equal(200m, summary.TotalSavings);
        Assert.Equal(1, summary.AccountCount);
        Assert.Equal(2, summary.TransactionCount);
    }
}
