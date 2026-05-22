using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CadentManagement.Migrations
{
    /// <inheritdoc />
    public partial class Added_Dynamic_RateLimiting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppRateLimitPolicies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    Algorithm = table.Column<int>(type: "int", nullable: false),
                    PartitionType = table.Column<int>(type: "int", nullable: false),
                    IsGlobal = table.Column<bool>(type: "bit", nullable: false),
                    EndpointPattern = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    PermitLimit = table.Column<int>(type: "int", nullable: false),
                    WindowInSeconds = table.Column<int>(type: "int", nullable: false),
                    QueueLimit = table.Column<int>(type: "int", nullable: false),
                    SegmentsPerWindow = table.Column<int>(type: "int", nullable: false),
                    TokensPerPeriod = table.Column<int>(type: "int", nullable: false),
                    ReplenishmentPeriodInSeconds = table.Column<int>(type: "int", nullable: false),
                    HttpStatusCode = table.Column<int>(type: "int", nullable: false),
                    CustomMessage = table.Column<string>(type: "nvarchar(512)", maxLength: 512, nullable: true),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatorUserId = table.Column<long>(type: "bigint", nullable: true),
                    LastModificationTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastModifierUserId = table.Column<long>(type: "bigint", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeleterUserId = table.Column<long>(type: "bigint", nullable: true),
                    DeletionTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppRateLimitPolicies", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppRateLimitPolicies_IsEnabled",
                table: "AppRateLimitPolicies",
                column: "IsEnabled");

            migrationBuilder.CreateIndex(
                name: "IX_AppRateLimitPolicies_Name",
                table: "AppRateLimitPolicies",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppRateLimitPolicies");
        }
    }
}
