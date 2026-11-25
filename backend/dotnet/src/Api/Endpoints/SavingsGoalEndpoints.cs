using Api.Data;
using Api.Dtos;
using Domain.Calculations;
using Domain.Savings;
using Domain.Transactions;
using Microsoft.EntityFrameworkCore;

namespace Api.Endpoints;

public static class SavingsGoalEndpoints
{
    public static RouteGroupBuilder MapSavingsGoalEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/savings-goals").WithTags("SavingsGoals");

        group.MapPost("/", async (CreateSavingsGoalRequest request, MaliNovationDbContext db) =>
        {
            if (request.UserId == Guid.Empty)
                return Results.BadRequest(new { error = "User ID cannot be empty." });

            if (string.IsNullOrWhiteSpace(request.Name))
                return Results.BadRequest(new { error = "Goal name is required." });

            if (request.TargetAmount <= 0)
                return Results.BadRequest(new { error = "Target amount must be greater than zero." });

            var userExists = await db.Users.AnyAsync(u => u.Id == request.UserId);
            if (!userExists)
                return Results.NotFound(new { error = "User profile not found." });

            if (!Enum.TryParse<CurrencyCode>(request.Currency ?? "ZAR", true, out var currencyCode))
                return Results.BadRequest(new { error = $"Invalid currency code. Valid values: {string.Join(", ", Enum.GetNames<CurrencyCode>())}" });

            var goal = new SavingsGoal(
                Guid.NewGuid(),
                request.UserId,
                request.Name,
                request.TargetAmount,
                request.CurrentAmount ?? 0m,
                currencyCode,
                request.TargetDate);

            db.SavingsGoals.Add(goal);
            await db.SaveChangesAsync();

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var progressList = FinancialCalculationEngine.CalculateSavingsProgress(new[] { goal }, today);
            var progress = progressList[0];

            var response = new SavingsGoalResponse(
                goal.Id,
                goal.UserId,
                goal.Name,
                goal.TargetAmount,
                goal.CurrentAmount,
                progress.RemainingAmount,
                progress.PercentageAchieved,
                goal.Currency.ToString(),
                goal.TargetDate,
                progress.RecommendedMonthlySavings,
                progress.IsCompleted,
                goal.CreatedAt);

            return Results.Created($"/api/savings-goals/{goal.Id}", response);
        });

        group.MapGet("/user/{userId:guid}", async (Guid userId, MaliNovationDbContext db) =>
        {
            var goals = await db.SavingsGoals
                .Where(g => g.UserId == userId)
                .OrderBy(g => g.CreatedAt)
                .ToListAsync();

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var progressList = FinancialCalculationEngine.CalculateSavingsProgress(goals, today);
            var progressMap = progressList.ToDictionary(p => p.GoalId);

            var response = goals.Select(g =>
            {
                var p = progressMap[g.Id];
                return new SavingsGoalResponse(
                    g.Id,
                    g.UserId,
                    g.Name,
                    g.TargetAmount,
                    g.CurrentAmount,
                    p.RemainingAmount,
                    p.PercentageAchieved,
                    g.Currency.ToString(),
                    g.TargetDate,
                    p.RecommendedMonthlySavings,
                    p.IsCompleted,
                    g.CreatedAt);
            }).ToList();

            return Results.Ok(response);
        });

        group.MapPost("/{id:guid}/contribute", async (Guid id, ContributeSavingsRequest request, MaliNovationDbContext db) =>
        {
            if (request.Amount <= 0)
                return Results.BadRequest(new { error = "Contribution amount must be greater than zero." });

            var goal = await db.SavingsGoals.FindAsync(id);
            if (goal is null)
                return Results.NotFound(new { error = "Savings goal not found." });

            goal.AddSavings(request.Amount);
            await db.SaveChangesAsync();

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var progressList = FinancialCalculationEngine.CalculateSavingsProgress(new[] { goal }, today);
            var p = progressList[0];

            var response = new SavingsGoalResponse(
                goal.Id,
                goal.UserId,
                goal.Name,
                goal.TargetAmount,
                goal.CurrentAmount,
                p.RemainingAmount,
                p.PercentageAchieved,
                goal.Currency.ToString(),
                goal.TargetDate,
                p.RecommendedMonthlySavings,
                p.IsCompleted,
                goal.CreatedAt);

            return Results.Ok(response);
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateSavingsGoalRequest request, MaliNovationDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return Results.BadRequest(new { error = "Goal name is required." });

            if (request.TargetAmount <= 0)
                return Results.BadRequest(new { error = "Target amount must be greater than zero." });

            var goal = await db.SavingsGoals.FindAsync(id);
            if (goal is null)
                return Results.NotFound(new { error = "Savings goal not found." });

            goal.UpdateGoal(request.Name, request.TargetAmount, request.TargetDate);
            await db.SaveChangesAsync();

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var progressList = FinancialCalculationEngine.CalculateSavingsProgress(new[] { goal }, today);
            var p = progressList[0];

            var response = new SavingsGoalResponse(
                goal.Id,
                goal.UserId,
                goal.Name,
                goal.TargetAmount,
                goal.CurrentAmount,
                p.RemainingAmount,
                p.PercentageAchieved,
                goal.Currency.ToString(),
                goal.TargetDate,
                p.RecommendedMonthlySavings,
                p.IsCompleted,
                goal.CreatedAt);

            return Results.Ok(response);
        });

        group.MapDelete("/{id:guid}", async (Guid id, MaliNovationDbContext db) =>
        {
            var goal = await db.SavingsGoals.FindAsync(id);
            if (goal is null)
                return Results.NotFound(new { error = "Savings goal not found." });

            db.SavingsGoals.Remove(goal);
            await db.SaveChangesAsync();

            return Results.NoContent();
        });

        return group;
    }
}
