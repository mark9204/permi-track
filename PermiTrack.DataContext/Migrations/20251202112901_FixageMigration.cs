using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PermiTrack.DataContext.Migrations
{
    /// <inheritdoc />
    /// <summary>
    /// NOTE: This was an empty placeholder migration. 
    /// The actual schema changes are in migration 20251129140000_AddAccessRequestsAndHttpAuditLogs
    /// which adds:
    /// - AccessRequests columns: ProcessedByUserId, ProcessedAt, RequestedDurationHours, ReviewerComment
    /// - HttpAuditLogs table
    /// 
    /// If that migration wasn't applied, run the MANUAL_MIGRATION_SCRIPT.sql in the root directory.
    /// </summary>
    public partial class FixageMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Empty migration - actual changes are in 20251129140000_AddAccessRequestsAndHttpAuditLogs
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Empty migration - actual changes are in 20251129140000_AddAccessRequestsAndHttpAuditLogs
        }
    }
}
