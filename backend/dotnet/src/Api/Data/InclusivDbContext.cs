using Domain.Accounts;
using Domain.Budgets;
using Domain.Savings;
using Domain.Transactions;
using Domain.Users;
using Microsoft.EntityFrameworkCore;

namespace Api.Data;

public class InclusivDbContext : DbContext
{
    public InclusivDbContext(DbContextOptions<InclusivDbContext> options)
        : base(options)
    {
    }

    public DbSet<UserProfile> Users => Set<UserProfile>();
    public DbSet<UserProfile> UserProfiles => Users;
    public DbSet<FinancialAccount> Accounts => Set<FinancialAccount>();
    public DbSet<FinancialTransaction> Transactions => Set<FinancialTransaction>();
    public DbSet<Budget> Budgets => Set<Budget>();
    public DbSet<SavingsGoal> SavingsGoals => Set<SavingsGoal>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // UserProfile
        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(u => u.Id);
            entity.Property(u => u.Email).HasMaxLength(256).IsRequired();
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.FullName).HasMaxLength(256).IsRequired();
            entity.Property(u => u.CreatedAt).IsRequired();
        });

        // FinancialAccount
        modelBuilder.Entity<FinancialAccount>(entity =>
        {
            entity.ToTable("financial_accounts");
            entity.HasKey(a => a.Id);
            entity.Property(a => a.UserId).IsRequired();
            entity.Property(a => a.Name).HasMaxLength(128).IsRequired();
            entity.Property(a => a.Type).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(a => a.Currency).HasConversion<string>().HasMaxLength(8).IsRequired();
            entity.Property(a => a.CreatedAt).IsRequired();

            entity.HasIndex(a => a.UserId);

            entity.HasOne<UserProfile>()
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // FinancialTransaction
        modelBuilder.Entity<FinancialTransaction>(entity =>
        {
            entity.ToTable("financial_transactions");
            entity.HasKey(t => t.Id);
            entity.Property(t => t.AccountId).IsRequired();
            entity.Property(t => t.UserId).IsRequired();
            entity.Property(t => t.Type).HasConversion<string>().HasMaxLength(16).IsRequired();
            entity.Property(t => t.Status).HasConversion<string>().HasMaxLength(16).IsRequired();
            entity.Property(t => t.Amount).HasPrecision(18, 2).IsRequired();
            entity.Property(t => t.Currency).HasConversion<string>().HasMaxLength(8).IsRequired();
            entity.Property(t => t.Description).HasMaxLength(256).IsRequired();
            entity.Property(t => t.Category).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(t => t.TransactionDate).IsRequired();
            entity.Property(t => t.RecordedAt).IsRequired();

            entity.HasIndex(t => t.UserId);
            entity.HasIndex(t => t.AccountId);
            entity.HasIndex(t => t.TransactionDate);

            entity.HasOne<FinancialAccount>()
                .WithMany()
                .HasForeignKey(t => t.AccountId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne<UserProfile>()
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Budget
        modelBuilder.Entity<Budget>(entity =>
        {
            entity.ToTable("budgets");
            entity.HasKey(b => b.Id);
            entity.Property(b => b.UserId).IsRequired();
            entity.Property(b => b.Category).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(b => b.MonthlyLimit).HasPrecision(18, 2).IsRequired();
            entity.Property(b => b.Currency).HasConversion<string>().HasMaxLength(8).IsRequired();
            entity.Property(b => b.CreatedAt).IsRequired();

            entity.HasIndex(b => new { b.UserId, b.Category }).IsUnique();

            entity.HasOne<UserProfile>()
                .WithMany()
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // SavingsGoal
        modelBuilder.Entity<SavingsGoal>(entity =>
        {
            entity.ToTable("savings_goals");
            entity.HasKey(g => g.Id);
            entity.Property(g => g.UserId).IsRequired();
            entity.Property(g => g.Name).HasMaxLength(128).IsRequired();
            entity.Property(g => g.TargetAmount).HasPrecision(18, 2).IsRequired();
            entity.Property(g => g.CurrentAmount).HasPrecision(18, 2).IsRequired();
            entity.Property(g => g.Currency).HasConversion<string>().HasMaxLength(8).IsRequired();
            entity.Property(g => g.CreatedAt).IsRequired();

            entity.HasIndex(g => g.UserId);

            entity.HasOne<UserProfile>()
                .WithMany()
                .HasForeignKey(g => g.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
