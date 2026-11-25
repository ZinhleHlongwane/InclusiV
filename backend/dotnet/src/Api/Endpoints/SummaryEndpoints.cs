using Api.Data;
using Api.Dtos;
using Domain.Calculations;
using Microsoft.EntityFrameworkCore;

namespace Api.Endpoints;

public static class SummaryEndpoints
{
    public static RouteGroupBuilder MapSummaryEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/financial-summary").WithTags("FinancialSummary");

        group.MapGet("/user/{userId:guid}", async (
            Guid userId,
            int? year,
            int? month,
            InclusivDbContext db) =>
        {
            var user = await db.UserProfiles.FindAsync(userId);
            if (user is null)
                return Results.NotFound(new { error = "User profile not found." });

            var now = DateTimeOffset.UtcNow;
            var targetYear = year ?? now.Year;
            var targetMonth = month ?? now.Month;

            if (targetMonth < 1 || targetMonth > 12)
                return Results.BadRequest(new { error = "Month must be between 1 and 12." });

            var accounts = await db.Accounts
                .Where(a => a.UserId == userId)
                .ToListAsync();

            var transactions = await db.Transactions
                .Where(t => t.UserId == userId)
                .ToListAsync();

            var budgets = await db.Budgets
                .Where(b => b.UserId == userId)
                .ToListAsync();

            var savingsGoals = await db.SavingsGoals
                .Where(g => g.UserId == userId)
                .ToListAsync();

            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            var summary = FinancialCalculationEngine.CalculateFinancialSummary(
                userId,
                accounts,
                transactions,
                budgets,
                savingsGoals,
                targetYear,
                targetMonth,
                asOfDate: today);

            var cashFlowDto = new CashFlowSummaryDto(
                summary.CashFlow.TotalIncome,
                summary.CashFlow.TotalExpenses,
                summary.CashFlow.NetCashFlow,
                summary.CashFlow.Currency.ToString(),
                summary.CashFlow.FromDate,
                summary.CashFlow.ToDate);

            var categoriesDto = summary.SpendingByCategory
                .Select(c => new CategorySpendingDto(
                    c.Category.ToString(),
                    c.TotalAmount,
                    c.PercentageOfTotalExpenses,
                    c.TransactionCount))
                .ToList();

            var budgetsDto = summary.BudgetStatuses
                .Select(b => new BudgetStatusDto(
                    b.BudgetId,
                    b.Category.ToString(),
                    b.MonthlyLimit,
                    b.CurrentSpend,
                    b.RemainingBudget,
                    b.PercentageUsed,
                    b.Health.ToString(),
                    b.IsExceeded))
                .ToList();

            var goalsDto = summary.SavingsGoals
                .Select(g => new SavingsGoalProgressDto(
                    g.GoalId,
                    g.Name,
                    g.TargetAmount,
                    g.CurrentAmount,
                    g.RemainingAmount,
                    g.PercentageAchieved,
                    g.TargetDate,
                    g.RecommendedMonthlySavings,
                    g.IsCompleted))
                .ToList();

            var response = new FinancialSummaryResponse(
                summary.UserId,
                summary.Year,
                summary.Month,
                summary.TotalBalance,
                cashFlowDto,
                categoriesDto,
                budgetsDto,
                goalsDto,
                summary.AccountCount,
                summary.TransactionCount,
                summary.HasNegativeCashFlow,
                summary.HasOverbudgetAlerts,
                summary.TotalSavings);

            return Results.Ok(response);
        });

        return group;
    }
}
