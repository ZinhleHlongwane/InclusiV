namespace Domain.Users;

public sealed class UserProfile
{
    public Guid Id { get; }
    public string Email { get; }
    public string FullName { get; private set; }
    public DateTimeOffset CreatedAt { get; }
    public DateTimeOffset? UpdatedAt { get; private set; }

    public UserProfile(
        Guid id,
        string email,
        string fullName,
        DateTimeOffset? createdAt = null,
        DateTimeOffset? updatedAt = null)
    {
        if (id == Guid.Empty)
            throw new ArgumentException(
                "User ID cannot be empty.", nameof(id));

        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
            throw new ArgumentException(
                "A valid email address is required.", nameof(email));

        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException(
                "Full name is required.", nameof(fullName));

        Id = id;
        Email = email.Trim().ToLowerInvariant();
        FullName = fullName.Trim();
        CreatedAt = createdAt ?? DateTimeOffset.UtcNow;
        UpdatedAt = updatedAt;
    }

    public void UpdateProfile(string fullName, DateTimeOffset? updatedAt = null)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException(
                "Full name is required.", nameof(fullName));

        FullName = fullName.Trim();
        UpdatedAt = updatedAt ?? DateTimeOffset.UtcNow;
    }
}
