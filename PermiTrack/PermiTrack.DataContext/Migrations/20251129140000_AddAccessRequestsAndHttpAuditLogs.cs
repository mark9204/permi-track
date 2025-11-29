using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PermiTrack.DataContext.Migrations
{
    public partial class AddAccessRequestsAndHttpAuditLogs : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Roles.ParentRoleId
            migrationBuilder.AddColumn<long>(
                name: "ParentRoleId",
                table: "Roles",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Roles_ParentRoleId",
                table: "Roles",
                column: "ParentRoleId");

            migrationBuilder.AddForeignKey(
                name: "FK_Roles_Roles_ParentRoleId",
                table: "Roles",
                column: "ParentRoleId",
                principalTable: "Roles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // AccessRequests additional columns
            migrationBuilder.AddColumn<DateTime>(
                name: "ProcessedAt",
                table: "AccessRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "ProcessedByUserId",
                table: "AccessRequests",
                type: "bigint",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RequestedDurationHours",
                table: "AccessRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReviewerComment",
                table: "AccessRequests",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccessRequests_ProcessedByUserId",
                table: "AccessRequests",
                column: "ProcessedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_AccessRequests_Users_ProcessedByUserId",
                table: "AccessRequests",
                column: "ProcessedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // HttpAuditLogs table
            migrationBuilder.CreateTable(
                name: "HttpAuditLogs",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<long>(type: "bigint", nullable: true),
                    Method = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    Path = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    QueryString = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    StatusCode = table.Column<int>(type: "int", nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DurationMs = table.Column<long>(type: "bigint", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()"),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    AdditionalInfo = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HttpAuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HttpAuditLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HttpAuditLogs_Timestamp",
                table: "HttpAuditLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_HttpAuditLogs_UserId",
                table: "HttpAuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_HttpAuditLogs_StatusCode",
                table: "HttpAuditLogs",
                column: "StatusCode");

            migrationBuilder.CreateIndex(
                name: "IX_HttpAuditLogs_Method_Path",
                table: "HttpAuditLogs",
                columns: new[] { "Method", "Path" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HttpAuditLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_AccessRequests_Users_ProcessedByUserId",
                table: "AccessRequests");

            migrationBuilder.DropIndex(
                name: "IX_AccessRequests_ProcessedByUserId",
                table: "AccessRequests");

            migrationBuilder.DropColumn(
                name: "ProcessedAt",
                table: "AccessRequests");

            migrationBuilder.DropColumn(
                name: "ProcessedByUserId",
                table: "AccessRequests");

            migrationBuilder.DropColumn(
                name: "RequestedDurationHours",
                table: "AccessRequests");

            migrationBuilder.DropColumn(
                name: "ReviewerComment",
                table: "AccessRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_Roles_Roles_ParentRoleId",
                table: "Roles");

            migrationBuilder.DropIndex(
                name: "IX_Roles_ParentRoleId",
                table: "Roles");

            migrationBuilder.DropColumn(
                name: "ParentRoleId",
                table: "Roles");
        }
    }
}