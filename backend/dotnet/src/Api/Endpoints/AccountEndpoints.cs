using Api.Data;
using Api.Dtos;
using Domain.Accounts;
using Domain.Calculations;
using Domain.Transactions;
using Microsoft.EntityFrameworkCore;

namespace Api.Endpoints;

public static class AccountEndpoints
{
    public static RouteGroupBuilder MapAccountEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/accounts").WithTags("Accounts");

        group.MapPost("/", async (CreateAccountRequest request, InclusivDbContext db) =>
        {
            if (request.UserId == Guid.Empty)
                return Results.BadRequest(new { error = "User ID cannot be empty." });

            if (string.IsNullOrWhiteSpace(request.Name))
                return Results.BadRequest(new { error = "Account name is required." });

            var userExists = await db.Users.AnyAsync(u => u.Id == request.UserId);
            if (!userExists)
                return Results.NotFound(new { error = "User profile not found." });

            if (!Enum.TryParse<AccountType>(request.Type, true, out var accountType))
                return Results.BadRequest(new { error = $"Invalid account type. Valid values: {string.Join(", ", Enum.GetNames<AccountType>())}" });

            if (!Enum.TryParse<CurrencyCode>(request.Currency ?? "ZAR", true, out var currencyCode))
                return Results.BadRequest(new { error = $"Invalid currency code. Valid values: {string.Join(", ", Enum.GetNames<CurrencyCode>())}" });

            var account = new FinancialAccount(
                Guid.NewGuid(),
                request.UserId,
                request.Name,
                accountType,
                currencyCode);

            db.Accounts.Add(account);
            await db.SaveChangesAsync();

            var response = new FinancialAccountResponse(
                account.Id,
                account.UserId,
                account.Name,
                account.Type.ToString(),
                account.Currency.ToString(),
                0m,
                account.CreatedAt);

            return Results.Created($"/api/accounts/{account.Id}", response);
        });

        group.MapGet("/user/{userId:guid}", async (Guid userId, InclusivDbContext db) =>
        {
            var accounts = await db.Accounts
                .Where(a => a.UserId == userId)
                .OrderBy(a => a.CreatedAt)
                .ToListAsync();

            var accountIds = accounts.Select(a => a.Id).ToList();
            var transactions = await db.Transactions
                .Where(t => accountIds.Contains(t.AccountId))
                .ToListAsync();

            var response = accounts.Select(a =>
            {
                var balance = FinancialCalculationEngine.CalculateAccountBalance(a.Id, transactions);
                return new FinancialAccountResponse(
                    a.Id,
                    a.UserId,
                    a.Name,
                    a.Type.ToString(),
                    a.Currency.ToString(),
                    balance,
                    a.CreatedAt);
            }).ToList();

            return Results.Ok(response);
        });

        group.MapGet("/{id:guid}", async (Guid id, InclusivDbContext db) =>
        {
            var account = await db.Accounts.FindAsync(id);
            if (account is null)
                return Results.NotFound(new { error = "Financial account not found." });

            var transactions = await db.Transactions
                .Where(t => t.AccountId == id)
                .ToListAsync();

            var balance = FinancialCalculationEngine.CalculateAccountBalance(id, transactions);

            var response = new FinancialAccountResponse(
                account.Id,
                account.UserId,
                account.Name,
                account.Type.ToString(),
                account.Currency.ToString(),
                balance,
                account.CreatedAt);

            return Results.Ok(response);
        });

        group.MapDelete("/{id:guid}", async (Guid id, InclusivDbContext db) =>
        {
            var account = await db.Accounts.FindAsync(id);
            if (account is null)
                return Results.NotFound(new { error = "Financial account not found." });

            db.Accounts.Remove(account);
            await db.SaveChangesAsync();

            return Results.NoContent();
        });

        return group;
    }
}
