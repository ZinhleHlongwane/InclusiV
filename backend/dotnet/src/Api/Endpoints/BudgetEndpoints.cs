using Api.Data;
using Api.Dtos;
using Domain.Budgets;
using Domain.Calculations;
using Domain.Transactions;
using Microsoft.EntityFrameworkCore;

namespace Api.Endpoints;

public static class BudgetEndpoints
{
    public static RouteGroupBuilder MapBudgetEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/budgets").WithTags("Budgets");

        group.MapPost("/", async (CreateBudgetRequest request, InclusivDbContext db) =>
        {
            if (request.UserId == Guid.Empty)
                return Results.BadRequest(new { error = "User ID cannot be empty." });

            if (request.MonthlyLimit <= 0)
                return Results.BadRequest(new { error = "Monthly limit must be greater than zero." });

            var userExists = await db.Users.AnyAsync(u => u.Id == request.UserId);
            if (!userExists)
                return Results.NotFound(new { error = "User profile not found." });

            if (!Enum.TryParse<TransactionCategory>(request.Category, true, out var category))
                return Results.BadRequest(new { error = $"Invalid category. Valid values: {string.Join(", ", Enum.GetNames<TransactionCategory>())}" });

            if (!Enum.TryParse<CurrencyCode>(request.Currency ?? "ZAR", true, out var currencyCode))
                return Results.BadRequest(new { error = $"Invalid currency code. Valid values: {string.Join(", ", Enum.GetNames<CurrencyCode>())}" });

            var existing = await db.Budgets.FirstOrDefaultAsync(b => b.UserId == request.UserId && b.Category == category);
            if (existing is not null)
                return Results.Conflict(new { error = $"A budget for category '{category}' already exists for this user." });

            var budget = new Budget(
                Guid.NewGuid(),
                request.UserId,
                category,
                request.MonthlyLimit,
                currencyCode);

            db.Budgets.Add(budget);
            await db.SaveChangesAsync();

            var now = DateTimeOffset.UtcNow;
            var transactions = await db.Transactions
                .Where(t => t.UserId == request.UserId && t.Category == category)
                .ToListAsync();

            var statuses = FinancialCalculationEngine.CalculateBudgetStatuses(new[] { budget }, transactions, now.Year, now.Month);
            var status = statuses[0];

            var response = new BudgetResponse(
                budget.Id,
                budget.UserId,
                budget.Category.ToString(),
                budget.MonthlyLimit,
                budget.Currency.ToString(),
                status.CurrentSpend,
                status.RemainingBudget,
                status.PercentageUsed,
                status.Health.ToString(),
                status.IsExceeded,
                budget.CreatedAt,
                budget.UpdatedAt);

            return Results.Created($"/api/budgets/{budget.Id}", response);
        });

        group.MapGet("/user/{userId:guid}", async (
            Guid userId,
            int? year,
            int? month,
            InclusivDbContext db) =>
        {
            var now = DateTimeOffset.UtcNow;
            var targetYear = year ?? now.Year;
            var targetMonth = month ?? now.Month;

            if (targetMonth < 1 || targetMonth > 12)
                return Results.BadRequest(new { error = "Month must be between 1 and 12." });

            var budgets = await db.Budgets
                .Where(b => b.UserId == userId)
                .OrderBy(b => b.Category)
                .ToListAsync();

            var startDate = new DateOnly(targetYear, targetMonth, 1);
            var endDate = new DateOnly(targetYear, targetMonth, DateTime.DaysInMonth(targetYear, targetMonth));

            var transactions = await db.Transactions
                .Where(t => t.UserId == userId && t.TransactionDate >= startDate && t.TransactionDate <= endDate)
                .ToListAsync();

            var statuses = FinancialCalculationEngine.CalculateBudgetStatuses(budgets, transactions, targetYear, targetMonth);
            var statusMap = statuses.ToDictionary(s => s.BudgetId);

            var response = budgets.Select(b =>
            {
                var s = statusMap[b.Id];
                return new BudgetResponse(
                    b.Id,
                    b.UserId,
                    b.Category.ToString(),
                    b.MonthlyLimit,
                    b.Currency.ToString(),
                    s.CurrentSpend,
                    s.RemainingBudget,
                    s.PercentageUsed,
                    s.Health.ToString(),
                    s.IsExceeded,
                    b.CreatedAt,
                    b.UpdatedAt);
            }).ToList();

            return Results.Ok(response);
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateBudgetLimitRequest request, InclusivDbContext db) =>
        {
            if (request.MonthlyLimit <= 0)
                return Results.BadRequest(new { error = "Monthly limit must be greater than zero." });

            var budget = await db.Budgets.FindAsync(id);
            if (budget is null)
                return Results.NotFound(new { error = "Budget not found." });

            budget.UpdateLimit(request.MonthlyLimit);
            await db.SaveChangesAsync();

            var now = DateTimeOffset.UtcNow;
            var transactions = await db.Transactions
                .Where(t => t.UserId == budget.UserId && t.Category == budget.Category)
                .ToListAsync();

            var statuses = FinancialCalculationEngine.CalculateBudgetStatuses(new[] { budget }, transactions, now.Year, now.Month);
            var status = statuses[0];

            var response = new BudgetResponse(
                budget.Id,
                budget.UserId,
                budget.Category.ToString(),
                budget.MonthlyLimit,
                budget.Currency.ToString(),
                status.CurrentSpend,
                status.RemainingBudget,
                status.PercentageUsed,
                status.Health.ToString(),
                status.IsExceeded,
                budget.CreatedAt,
                budget.UpdatedAt);

            return Results.Ok(response);
        });

        group.MapDelete("/{id:guid}", async (Guid id, InclusivDbContext db) =>
        {
            var budget = await db.Budgets.FindAsync(id);
            if (budget is null)
                return Results.NotFound(new { error = "Budget not found." });

            db.Budgets.Remove(budget);
            await db.SaveChangesAsync();

            return Results.NoContent();
        });

        return group;
    }
}
