using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Data.Migrations;

/// <inheritdoc />
public partial class InitialCreate : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "users",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                FullName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_users", x => x.Id);
            });

        migrationBuilder.CreateTable(
            name: "budgets",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                Category = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                MonthlyLimit = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                Currency = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_budgets", x => x.Id);
                table.ForeignKey(
                    name: "FK_budgets_users_UserId",
                    column: x => x.UserId,
                    principalTable: "users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "financial_accounts",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                Type = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                Currency = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_financial_accounts", x => x.Id);
                table.ForeignKey(
                    name: "FK_financial_accounts_users_UserId",
                    column: x => x.UserId,
                    principalTable: "users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "savings_goals",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                TargetAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                CurrentAmount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                Currency = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                TargetDate = table.Column<DateOnly>(type: "date", nullable: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_savings_goals", x => x.Id);
                table.ForeignKey(
                    name: "FK_savings_goals_users_UserId",
                    column: x => x.UserId,
                    principalTable: "users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateTable(
            name: "financial_transactions",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uuid", nullable: false),
                AccountId = table.Column<Guid>(type: "uuid", nullable: false),
                UserId = table.Column<Guid>(type: "uuid", nullable: false),
                Type = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                Currency = table.Column<string>(type: "character varying(8)", maxLength: 8, nullable: false),
                Description = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                Category = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                TransactionDate = table.Column<DateOnly>(type: "date", nullable: false),
                RecordedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_financial_transactions", x => x.Id);
                table.ForeignKey(
                    name: "FK_financial_transactions_financial_accounts_AccountId",
                    column: x => x.AccountId,
                    principalTable: "financial_accounts",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
                table.ForeignKey(
                    name: "FK_financial_transactions_users_UserId",
                    column: x => x.UserId,
                    principalTable: "users",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_budgets_UserId_Category",
            table: "budgets",
            columns: new[] { "UserId", "Category" },
            unique: true);

        migrationBuilder.CreateIndex(
            name: "IX_financial_accounts_UserId",
            table: "financial_accounts",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_financial_transactions_AccountId",
            table: "financial_transactions",
            column: "AccountId");

        migrationBuilder.CreateIndex(
            name: "IX_financial_transactions_TransactionDate",
            table: "financial_transactions",
            column: "TransactionDate");

        migrationBuilder.CreateIndex(
            name: "IX_financial_transactions_UserId",
            table: "financial_transactions",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_savings_goals_UserId",
            table: "savings_goals",
            column: "UserId");

        migrationBuilder.CreateIndex(
            name: "IX_users_Email",
            table: "users",
            column: "Email",
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "budgets");

        migrationBuilder.DropTable(
            name: "financial_transactions");

        migrationBuilder.DropTable(
            name: "savings_goals");

        migrationBuilder.DropTable(
            name: "financial_accounts");

        migrationBuilder.DropTable(
            name: "users");
    }
}
