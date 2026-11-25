using Api.Data;
using Api.Dtos;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Api.Endpoints;

public static class UserEndpoints
{
    public static RouteGroupBuilder MapUserEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/users").WithTags("Users");

        group.MapPost("/", async (CreateUserRequest request, MaliNovationDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(request.Email) || !request.Email.Contains('@'))
                return Results.BadRequest(new { error = "A valid email address is required." });

            if (string.IsNullOrWhiteSpace(request.FullName))
                return Results.BadRequest(new { error = "Full name is required." });

            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var exists = await db.Users.AnyAsync(u => u.Email == normalizedEmail);
            if (exists)
                return Results.Conflict(new { error = "A user profile with this email already exists." });

            var user = new UserProfile(Guid.NewGuid(), normalizedEmail, request.FullName);
            db.Users.Add(user);
            await db.SaveChangesAsync();

            var response = new UserProfileResponse(user.Id, user.Email, user.FullName, user.CreatedAt, user.UpdatedAt);
            return Results.Created($"/api/users/{user.Id}", response);
        });

        group.MapGet("/{id:guid}", async (Guid id, MaliNovationDbContext db) =>
        {
            var user = await db.Users.FindAsync(id);
            if (user is null)
                return Results.NotFound(new { error = "User profile not found." });

            return Results.Ok(new UserProfileResponse(user.Id, user.Email, user.FullName, user.CreatedAt, user.UpdatedAt));
        });

        group.MapGet("/by-email/{email}", async (string email, MaliNovationDbContext db) =>
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();
            var user = await db.Users.FirstOrDefaultAsync(u => u.Email == normalizedEmail);
            if (user is null)
                return Results.NotFound(new { error = "User profile not found." });

            return Results.Ok(new UserProfileResponse(user.Id, user.Email, user.FullName, user.CreatedAt, user.UpdatedAt));
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateUserRequest request, MaliNovationDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(request.FullName))
                return Results.BadRequest(new { error = "Full name is required." });

            var user = await db.Users.FindAsync(id);
            if (user is null)
                return Results.NotFound(new { error = "User profile not found." });

            user.UpdateProfile(request.FullName);
            await db.SaveChangesAsync();

            return Results.Ok(new UserProfileResponse(user.Id, user.Email, user.FullName, user.CreatedAt, user.UpdatedAt));
        });

        return group;
    }
}
