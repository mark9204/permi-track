using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PermiTrack.DataContext.Migrations
{
    /// <inheritdoc />
    public partial class AddRoleHierarchyAndSecurityFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add ParentRoleId and Level columns to Roles table for hierarchy support
            migrationBuilder.AddColumn<long>(
                name: "ParentRoleId",
                table: "Roles",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "Roles",
                type: "int",
                nullable: false,
                defaultValue: 0);

            // Create index for ParentRoleId
            migrationBuilder.CreateIndex(
                name: "IX_Roles_ParentRoleId",
                table: "Roles",
                column: "ParentRoleId");

            // Add foreign key for self-referencing relationship
            migrationBuilder.AddForeignKey(
                name: "FK_Roles_Roles_ParentRoleId",
                table: "Roles",
                column: "ParentRoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // Create LoginAttempts table for security tracking
            migrationBuilder.CreateTable(
                name: "LoginAttempts",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    UserName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    IsSuccess = table.Column<bool>(type: "bit", nullable: false),
                    FailureReason = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AttemptedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoginAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LoginAttempts_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            // Create indexes for LoginAttempts for security monitoring
            migrationBuilder.CreateIndex(
                name: "IX_LoginAttempts_UserId",
                table: "LoginAttempts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_LoginAttempts_AttemptedAt",
                table: "LoginAttempts",
                column: "AttemptedAt");

            migrationBuilder.CreateIndex(
                name: "IX_LoginAttempts_IpAddress",
                table: "LoginAttempts",
                column: "IpAddress");

            migrationBuilder.CreateIndex(
                name: "IX_LoginAttempts_UserName",
                table: "LoginAttempts",
                column: "UserName");

            migrationBuilder.CreateIndex(
                name: "IX_LoginAttempts_IsSuccess_AttemptedAt",
                table: "LoginAttempts",
                columns: new[] { "IsSuccess", "AttemptedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop LoginAttempts table
            migrationBuilder.DropTable(
                name: "LoginAttempts");

            // Drop foreign key and index for ParentRoleId
            migrationBuilder.DropForeignKey(
                name: "FK_Roles_Roles_ParentRoleId",
                table: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_Roles_ParentRoleId",
                table: "Roles");

            // Drop columns from Roles table
            migrationBuilder.DropColumn(
                name: "ParentRoleId",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "Roles");
        }
    }
}
