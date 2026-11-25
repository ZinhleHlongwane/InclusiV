namespace Api.Dtos;

public record CreateUserRequest(
    string Email,
    string FullName);

public record UpdateUserRequest(
    string FullName);

public record UserProfileResponse(
    Guid Id,
    string Email,
    string FullName,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt);
