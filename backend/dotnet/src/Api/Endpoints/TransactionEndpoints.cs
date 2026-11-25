using Api.Data;
using Api.Dtos;
using Domain.Transactions;
using Microsoft.EntityFrameworkCore;

namespace Api.Endpoints;

public static class TransactionEndpoints
{
    public static RouteGroupBuilder MapTransactionEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/transactions").WithTags("Transactions");

        group.MapPost("/", async (CreateTransactionRequest request, MaliNovationDbContext db) =>
        {
            if (request.AccountId == Guid.Empty)
                return Results.BadRequest(new { error = "Account ID cannot be empty." });

            if (request.UserId == Guid.Empty)
                return Results.BadRequest(new { error = "User ID cannot be empty." });

            if (request.Amount <= 0)
                return Results.BadRequest(new { error = "Transaction amount must be greater than zero." });

            if (string.IsNullOrWhiteSpace(request.Description))
                return Results.BadRequest(new { error = "Description is required." });

            var account = await db.Accounts.FirstOrDefaultAsync(a => a.Id == request.AccountId);
            if (account is null)
                return Results.NotFound(new { error = "Financial account not found." });

            if (account.UserId != request.UserId)
                return Results.BadRequest(new { error = "The specified account does not belong to the user." });

            if (!Enum.TryParse<TransactionType>(request.Type, true, out var txType))
                return Results.BadRequest(new { error = $"Invalid transaction type. Valid values: {string.Join(", ", Enum.GetNames<TransactionType>())}" });

            if (!Enum.TryParse<TransactionCategory>(request.Category, true, out var category))
                return Results.BadRequest(new { error = $"Invalid transaction category. Valid values: {string.Join(", ", Enum.GetNames<TransactionCategory>())}" });

            if (!Enum.TryParse<CurrencyCode>(request.Currency ?? account.Currency.ToString(), true, out var currencyCode))
                return Results.BadRequest(new { error = $"Invalid currency code. Valid values: {string.Join(", ", Enum.GetNames<CurrencyCode>())}" });

            var status = TransactionStatus.Completed;
            if (!string.IsNullOrWhiteSpace(request.Status) && !Enum.TryParse<TransactionStatus>(request.Status, true, out status))
                return Results.BadRequest(new { error = $"Invalid transaction status. Valid values: {string.Join(", ", Enum.GetNames<TransactionStatus>())}" });

            var transaction = new FinancialTransaction(
                Guid.NewGuid(),
                request.AccountId,
                request.UserId,
                txType,
                request.Amount,
                request.Description,
                category,
                request.TransactionDate,
                currencyCode,
                status: status);

            db.Transactions.Add(transaction);
            await db.SaveChangesAsync();

            var response = new FinancialTransactionResponse(
                transaction.Id,
                transaction.AccountId,
                transaction.UserId,
                transaction.Type.ToString(),
                transaction.Status.ToString(),
                transaction.Amount,
                transaction.Currency.ToString(),
                transaction.Description,
                transaction.Category.ToString(),
                transaction.TransactionDate,
                transaction.RecordedAt);

            return Results.Created($"/api/transactions/{transaction.Id}", response);
        });

        group.MapGet("/user/{userId:guid}", async (
            Guid userId,
            Guid? accountId,
            string? type,
            string? category,
            DateOnly? from,
            DateOnly? to,
            string? status,
            MaliNovationDbContext db) =>
        {
            var query = db.Transactions.Where(t => t.UserId == userId);

            if (accountId.HasValue)
                query = query.Where(t => t.AccountId == accountId.Value);

            if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse<TransactionType>(type, true, out var txType))
                query = query.Where(t => t.Type == txType);

            if (!string.IsNullOrWhiteSpace(category) && Enum.TryParse<TransactionCategory>(category, true, out var txCategory))
                query = query.Where(t => t.Category == txCategory);

            if (from.HasValue)
                query = query.Where(t => t.TransactionDate >= from.Value);

            if (to.HasValue)
                query = query.Where(t => t.TransactionDate <= to.Value);

            if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<TransactionStatus>(status, true, out var txStatus))
                query = query.Where(t => t.Status == txStatus);

            var transactions = await query
                .OrderByDescending(t => t.TransactionDate)
                .ThenByDescending(t => t.RecordedAt)
                .ToListAsync();

            var response = transactions.Select(t => new FinancialTransactionResponse(
                t.Id,
                t.AccountId,
                t.UserId,
                t.Type.ToString(),
                t.Status.ToString(),
                t.Amount,
                t.Currency.ToString(),
                t.Description,
                t.Category.ToString(),
                t.TransactionDate,
                t.RecordedAt)).ToList();

            return Results.Ok(response);
        });

        group.MapGet("/{id:guid}", async (Guid id, MaliNovationDbContext db) =>
        {
            var tx = await db.Transactions.FindAsync(id);
            if (tx is null)
                return Results.NotFound(new { error = "Transaction not found." });

            var response = new FinancialTransactionResponse(
                tx.Id,
                tx.AccountId,
                tx.UserId,
                tx.Type.ToString(),
                tx.Status.ToString(),
                tx.Amount,
                tx.Currency.ToString(),
                tx.Description,
                tx.Category.ToString(),
                tx.TransactionDate,
                tx.RecordedAt);

            return Results.Ok(response);
        });

        group.MapPatch("/{id:guid}/status", async (Guid id, UpdateTransactionStatusRequest request, MaliNovationDbContext db) =>
        {
            if (!Enum.TryParse<TransactionStatus>(request.Status, true, out var newStatus))
                return Results.BadRequest(new { error = $"Invalid transaction status. Valid values: {string.Join(", ", Enum.GetNames<TransactionStatus>())}" });

            var tx = await db.Transactions.FindAsync(id);
            if (tx is null)
                return Results.NotFound(new { error = "Transaction not found." });

            tx.UpdateStatus(newStatus);
            await db.SaveChangesAsync();

            var response = new FinancialTransactionResponse(
                tx.Id,
                tx.AccountId,
                tx.UserId,
                tx.Type.ToString(),
                tx.Status.ToString(),
                tx.Amount,
                tx.Currency.ToString(),
                tx.Description,
                tx.Category.ToString(),
                tx.TransactionDate,
                tx.RecordedAt);

            return Results.Ok(response);
        });

        group.MapDelete("/{id:guid}", async (Guid id, MaliNovationDbContext db) =>
        {
            var tx = await db.Transactions.FindAsync(id);
            if (tx is null)
                return Results.NotFound(new { error = "Transaction not found." });

            db.Transactions.Remove(tx);
            await db.SaveChangesAsync();

            return Results.NoContent();
        });

        return group;
    }
}
