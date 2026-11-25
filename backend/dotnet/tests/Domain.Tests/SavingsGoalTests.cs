using Domain.Savings;
using Domain.Transactions;

namespace Domain.Tests;

public class SavingsGoalTests
{
    [Fact]
    public void Constructor_WithValidData_CreatesSavingsGoal()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var targetDate = new DateOnly(2027, 12, 31);

        var goal = new SavingsGoal(
            id,
            userId,
            "Emergency Fund",
            5000m,
            500m,
            CurrencyCode.ZAR,
            targetDate);

        Assert.Equal(id, goal.Id);
        Assert.Equal(userId, goal.UserId);
        Assert.Equal("Emergency Fund", goal.Name);
        Assert.Equal(5000m, goal.TargetAmount);
        Assert.Equal(500m, goal.CurrentAmount);
        Assert.Equal(CurrencyCode.ZAR, goal.Currency);
        Assert.Equal(targetDate, goal.TargetDate);
    }

    [Fact]
    public void Constructor_WithEmptyId_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new SavingsGoal(Guid.Empty, Guid.NewGuid(), "Goal", 1000m));

        Assert.Equal("id", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithEmptyUserId_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new SavingsGoal(Guid.NewGuid(), Guid.Empty, "Goal", 1000m));

        Assert.Equal("userId", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithBlankName_ThrowsArgumentException(string name)
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new SavingsGoal(Guid.NewGuid(), Guid.NewGuid(), name, 1000m));

        Assert.Equal("name", exception.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void Constructor_WithInvalidTargetAmount_ThrowsArgumentOutOfRangeException(decimal target)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new SavingsGoal(Guid.NewGuid(), Guid.NewGuid(), "Goal", target));

        Assert.Equal("targetAmount", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithNegativeCurrentAmount_ThrowsArgumentOutOfRangeException()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            new SavingsGoal(Guid.NewGuid(), Guid.NewGuid(), "Goal", 1000m, -50m));

        Assert.Equal("currentAmount", exception.ParamName);
    }

    [Fact]
    public void AddSavings_WithPositiveAmount_IncreasesCurrentAmount()
    {
        var goal = new SavingsGoal(Guid.NewGuid(), Guid.NewGuid(), "Study Materials", 2000m, 500m);

        goal.AddSavings(300m);

        Assert.Equal(800m, goal.CurrentAmount);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-100)]
    public void AddSavings_WithNonPositiveAmount_ThrowsArgumentOutOfRangeException(decimal amount)
    {
        var goal = new SavingsGoal(Guid.NewGuid(), Guid.NewGuid(), "Study Materials", 2000m, 500m);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            goal.AddSavings(amount));

        Assert.Equal("amount", exception.ParamName);
    }

    [Fact]
    public void UpdateGoal_WithValidData_UpdatesNameAndTarget()
    {
        var goal = new SavingsGoal(Guid.NewGuid(), Guid.NewGuid(), "Goal", 1000m);
        var newDate = new DateOnly(2027, 6, 30);

        goal.UpdateGoal("Updated Goal Name", 3000m, newDate);

        Assert.Equal("Updated Goal Name", goal.Name);
        Assert.Equal(3000m, goal.TargetAmount);
        Assert.Equal(newDate, goal.TargetDate);
    }
}
