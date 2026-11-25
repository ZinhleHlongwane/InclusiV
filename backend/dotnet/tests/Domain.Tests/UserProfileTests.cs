using Domain.Users;

namespace Domain.Tests;

public class UserProfileTests
{
    [Fact]
    public void Constructor_WithValidData_CreatesUserProfile()
    {
        var id = Guid.NewGuid();
        var email = "student@example.ac.za";
        var name = "Jane Doe";

        var profile = new UserProfile(id, email, name);

        Assert.Equal(id, profile.Id);
        Assert.Equal("student@example.ac.za", profile.Email);
        Assert.Equal("Jane Doe", profile.FullName);
        Assert.Null(profile.UpdatedAt);
    }

    [Fact]
    public void Constructor_WithEmptyId_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new UserProfile(Guid.Empty, "user@example.com", "Test User"));

        Assert.Equal("id", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid-email")]
    public void Constructor_WithInvalidEmail_ThrowsArgumentException(string email)
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new UserProfile(Guid.NewGuid(), email, "Test User"));

        Assert.Equal("email", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithBlankFullName_ThrowsArgumentException(string name)
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new UserProfile(Guid.NewGuid(), "user@example.com", name));

        Assert.Equal("fullName", exception.ParamName);
    }

    [Fact]
    public void UpdateProfile_WithValidName_UpdatesFullNameAndTimestamp()
    {
        var profile = new UserProfile(Guid.NewGuid(), "user@example.com", "Original Name");
        var updateTime = DateTimeOffset.UtcNow;

        profile.UpdateProfile("Updated Name", updateTime);

        Assert.Equal("Updated Name", profile.FullName);
        Assert.Equal(updateTime, profile.UpdatedAt);
    }

    [Fact]
    public void UpdateProfile_WithBlankName_ThrowsArgumentException()
    {
        var profile = new UserProfile(Guid.NewGuid(), "user@example.com", "Original Name");

        var exception = Assert.Throws<ArgumentException>(() =>
            profile.UpdateProfile("   "));

        Assert.Equal("fullName", exception.ParamName);
    }
}
