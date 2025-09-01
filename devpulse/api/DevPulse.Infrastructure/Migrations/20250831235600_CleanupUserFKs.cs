using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DevPulse.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CleanupUserFKs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "UserId1",
                table: "Issues",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Issues_UserId1",
                table: "Issues",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Issues_Users_UserId1",
                table: "Issues",
                column: "UserId1",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Issues_Users_UserId1",
                table: "Issues");

            migrationBuilder.DropIndex(
                name: "IX_Issues_UserId1",
                table: "Issues");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "Issues");
        }
    }
}
