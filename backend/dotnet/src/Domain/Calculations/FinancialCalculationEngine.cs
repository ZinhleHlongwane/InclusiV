using Domain.Accounts;
using Domain.Budgets;
using Domain.Savings;
using Domain.Transactions;

namespace Domain.Calculations;

public static class FinancialCalculationEngine
{
    public static decimal CalculateAccountBalance(
        Guid accountId,
        IEnumerable<FinancialTransaction> transactions,
        bool includePending = false)
    {
        if (accountId == Guid.Empty)
            throw new ArgumentException("Account ID cannot be empty.", nameof(accountId));

        ArgumentNullException.ThrowIfNull(transactions);

        decimal balance = 0m;

        foreach (var tx in transactions)
        {
            if (tx.AccountId != accountId)
                continue;

            if (tx.Status == TransactionStatus.Failed)
                continue;

            if (tx.Status == TransactionStatus.Pending && !includePending)
                continue;

            if (tx.Type == TransactionType.Income)
            {
                balance += tx.Amount;
            }
            else if (tx.Type == TransactionType.Expense)
            {
                balance -= tx.Amount;
            }
        }

        return Math.Round(balance, 2, MidpointRounding.AwayFromZero);
    }

    public static decimal CalculateTotalBalance(
        IEnumerable<FinancialAccount> accounts,
        IEnumerable<FinancialTransaction> transactions,
        bool includePending = false)
    {
        ArgumentNullException.ThrowIfNull(accounts);
        ArgumentNullException.ThrowIfNull(transactions);

        var txList = transactions.ToList();
        decimal total = 0m;

        foreach (var acc in accounts)
        {
            total += CalculateAccountBalance(acc.Id, txList, includePending);
        }

        return Math.Round(total, 2, MidpointRounding.AwayFromZero);
    }

    public static CashFlowSummary CalculateCashFlow(
        IEnumerable<FinancialTransaction> transactions,
        DateOnly? fromDate = null,
        DateOnly? toDate = null,
        CurrencyCode currency = CurrencyCode.ZAR)
    {
        ArgumentNullException.ThrowIfNull(transactions);

        decimal income = 0m;
        decimal expenses = 0m;

        foreach (var tx in transactions)
        {
            if (tx.Status != TransactionStatus.Completed)
                continue;

            if (fromDate.HasValue && tx.TransactionDate < fromDate.Value)
                continue;

            if (toDate.HasValue && tx.TransactionDate > toDate.Value)
                continue;

            if (tx.Type == TransactionType.Income)
            {
                income += tx.Amount;
            }
            else if (tx.Type == TransactionType.Expense)
            {
                expenses += tx.Amount;
            }
        }

        return new CashFlowSummary(income, expenses, currency, fromDate, toDate);
    }

    public static IReadOnlyList<CategorySpending> CalculateCategoryBreakdown(
        IEnumerable<FinancialTransaction> transactions,
        DateOnly? fromDate = null,
        DateOnly? toDate = null)
    {
        ArgumentNullException.ThrowIfNull(transactions);

        var filteredExpenses = transactions
            .Where(t => t.Type == TransactionType.Expense && t.Status == TransactionStatus.Completed)
            .Where(t => !fromDate.HasValue || t.TransactionDate >= fromDate.Value)
            .Where(t => !toDate.HasValue || t.TransactionDate <= toDate.Value)
            .ToList();

        decimal totalExpenses = filteredExpenses.Sum(t => t.Amount);

        if (totalExpenses == 0m)
        {
            return Array.Empty<CategorySpending>();
        }

        return filteredExpenses
            .GroupBy(t => t.Category)
            .Select(group =>
            {
                var categorySum = group.Sum(t => t.Amount);
                var percentage = (categorySum / totalExpenses) * 100m;
                return new CategorySpending(group.Key, categorySum, percentage, group.Count());
            })
            .OrderByDescending(c => c.TotalAmount)
            .ToList();
    }

    public static IReadOnlyList<BudgetStatus> CalculateBudgetStatuses(
        IEnumerable<Budget> budgets,
        IEnumerable<FinancialTransaction> transactions,
        int year,
        int month)
    {
        ArgumentNullException.ThrowIfNull(budgets);
        ArgumentNullException.ThrowIfNull(transactions);

        var startDate = new DateOnly(year, month, 1);
        var endDate = new DateOnly(year, month, DateTime.DaysInMonth(year, month));

        var monthExpenses = transactions
            .Where(t => t.Type == TransactionType.Expense && t.Status == TransactionStatus.Completed)
            .Where(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate)
            .ToList();

        var statuses = new List<BudgetStatus>();

        foreach (var budget in budgets)
        {
            var spend = monthExpenses
                .Where(t => t.Category == budget.Category)
                .Sum(t => t.Amount);

            statuses.Add(new BudgetStatus(budget.Id, budget.Category, budget.MonthlyLimit, spend));
        }

        return statuses;
    }

    public static IReadOnlyList<SavingsGoalProgress> CalculateSavingsProgress(
        IEnumerable<SavingsGoal> goals,
        DateOnly asOfDate)
    {
        ArgumentNullException.ThrowIfNull(goals);

        var result = new List<SavingsGoalProgress>();

        foreach (var goal in goals)
        {
            decimal? recommendedMonthly = null;

            if (goal.TargetDate.HasValue && goal.CurrentAmount < goal.TargetAmount)
            {
                var targetDate = goal.TargetDate.Value;
                var months = ((targetDate.Year - asOfDate.Year) * 12) + targetDate.Month - asOfDate.Month;

                if (months > 0)
                {
                    var remaining = goal.TargetAmount - goal.CurrentAmount;
                    recommendedMonthly = remaining / months;
                }
                else
                {
                    // Goal is due this month or past due
                    recommendedMonthly = goal.TargetAmount - goal.CurrentAmount;
                }
            }

            result.Add(new SavingsGoalProgress(
                goal.Id,
                goal.Name,
                goal.TargetAmount,
                goal.CurrentAmount,
                goal.TargetDate,
                recommendedMonthly));
        }

        return result;
    }

    public static FinancialSummary CalculateFinancialSummary(
        Guid userId,
        IEnumerable<FinancialAccount> accounts,
        IEnumerable<FinancialTransaction> transactions,
        IEnumerable<Budget> budgets,
        IEnumerable<SavingsGoal> goals,
        int year,
        int month,
        DateOnly? asOfDate = null)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        var accList = (accounts ?? Array.Empty<FinancialAccount>()).ToList();
        var txList = (transactions ?? Array.Empty<FinancialTransaction>()).ToList();
        var budgetList = (budgets ?? Array.Empty<Budget>()).ToList();
        var goalList = (goals ?? Array.Empty<SavingsGoal>()).ToList();

        var calculationDate = asOfDate ?? new DateOnly(year, month, DateTime.DaysInMonth(year, month));

        var startDate = new DateOnly(year, month, 1);
        var endDate = new DateOnly(year, month, DateTime.DaysInMonth(year, month));

        var totalBalance = CalculateTotalBalance(accList, txList);
        var cashFlow = CalculateCashFlow(txList, startDate, endDate);
        var spendingByCategory = CalculateCategoryBreakdown(txList, startDate, endDate);
        var budgetStatuses = CalculateBudgetStatuses(budgetList, txList, year, month);
        var savingsProgress = CalculateSavingsProgress(goalList, calculationDate);

        var monthTxCount = txList.Count(t => t.TransactionDate >= startDate && t.TransactionDate <= endDate);

        return new FinancialSummary(
            userId,
            year,
            month,
            totalBalance,
            cashFlow,
            spendingByCategory,
            budgetStatuses,
            savingsProgress,
            accList.Count,
            monthTxCount);
    }
}
