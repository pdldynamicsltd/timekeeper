using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CadentManagement.Migrations
{
    /// <inheritdoc />
    public partial class Added_UserExternalLoginInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppUserExternalLoginInfos",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TenantId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<long>(type: "bigint", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    EmailAddress = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    CreationTime = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUserExternalLoginInfos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppUserExternalLoginInfos_AbpUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AbpUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppUserExternalLoginInfos_TenantId_UserId_LoginProvider",
                table: "AppUserExternalLoginInfos",
                columns: new[] { "TenantId", "UserId", "LoginProvider" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppUserExternalLoginInfos_UserId",
                table: "AppUserExternalLoginInfos",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppUserExternalLoginInfos");
        }
    }
}
