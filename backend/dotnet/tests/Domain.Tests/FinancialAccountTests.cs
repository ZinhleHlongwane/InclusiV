using Domain.Accounts;
using Domain.Transactions;

namespace Domain.Tests;

public class FinancialAccountTests
{
    [Fact]
    public void Constructor_WithValidAccount_CreatesAccount()
    {
        var id = Guid.NewGuid();
        var userId = Guid.NewGuid();

        var account = new FinancialAccount(
            id,
            userId,
            "My Savings",
            AccountType.SavingsAccount);

        Assert.Equal(id, account.Id);
        Assert.Equal(userId, account.UserId);
        Assert.Equal("My Savings", account.Name);
        Assert.Equal(AccountType.SavingsAccount, account.Type);
        Assert.Equal(CurrencyCode.ZAR, account.Currency);
    }

    [Theory]
    [InlineData(AccountType.Cash)]
    [InlineData(AccountType.BankAccount)]
    [InlineData(AccountType.SavingsAccount)]
    [InlineData(AccountType.MobileWallet)]
    public void Constructor_WithValidType_StoresType(AccountType type)
    {
        var account = CreateAccount(type: type);

        Assert.Equal(type, account.Type);
    }

    [Fact]
    public void Constructor_WithEmptyId_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            CreateAccount(id: Guid.Empty));

        Assert.Equal("id", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithEmptyUserId_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            CreateAccount(userId: Guid.Empty));

        Assert.Equal("userId", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithBlankName_ThrowsArgumentException(
        string name)
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            CreateAccount(name: name));

        Assert.Equal("name", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithInvalidType_ThrowsArgumentOutOfRangeException()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreateAccount(type: (AccountType)999));

        Assert.Equal("type", exception.ParamName);
    }

    [Fact]
    public void Constructor_WithExplicitZarCurrency_StoresCurrency()
    {
        var account = CreateAccount(currency: CurrencyCode.ZAR);

        Assert.Equal(CurrencyCode.ZAR, account.Currency);
    }

    [Fact]
    public void Constructor_WithInvalidCurrency_ThrowsArgumentOutOfRangeException()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            CreateAccount(currency: (CurrencyCode)999));

        Assert.Equal("currency", exception.ParamName);
    }

    [Fact]
    public void Constructor_TrimsAccountName()
    {
        var account = CreateAccount(name: "  My Bank Account  ");

        Assert.Equal("My Bank Account", account.Name);
    }

    [Fact]
    public void Constructor_WithExplicitCreatedAt_PreservesTimestamp()
    {
        var createdAt = new DateTimeOffset(
            2026, 11, 16, 10, 30, 0, TimeSpan.FromHours(2));

        var account = CreateAccount(createdAt: createdAt);

        Assert.Equal(createdAt, account.CreatedAt);
    }

    [Fact]
    public void Constructor_WithoutCreatedAt_UsesUtcTimestamp()
    {
        var beforeCreation = DateTimeOffset.UtcNow;

        var account = CreateAccount();

        var afterCreation = DateTimeOffset.UtcNow;

        Assert.Equal(TimeSpan.Zero, account.CreatedAt.Offset);
        Assert.InRange(account.CreatedAt, beforeCreation, afterCreation);
    }

    private static FinancialAccount CreateAccount(
        Guid? id = null,
        Guid? userId = null,
        string name = "Test Account",
        AccountType type = AccountType.Cash,
        CurrencyCode currency = CurrencyCode.ZAR,
        DateTimeOffset? createdAt = null)
    {
        return new FinancialAccount(
            id ?? Guid.NewGuid(),
            userId ?? Guid.NewGuid(),
            name,
            type,
            currency,
            createdAt);
    }
}
