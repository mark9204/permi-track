# Fix Database Schema - Missing Tables & Columns

## ?? Problem Summary

Your application is experiencing **500 Internal Server Error** because the database schema is missing:

1. ? **AccessRequests table** missing columns:
   - `ProcessedByUserId`
   - `ProcessedAt`
   - `RequestedDurationHours`
   - `ReviewerComment`

2. ? **HttpAuditLogs table** doesn't exist

## ? Solution: Apply the Missing Migration

The migration file **`20251129140000_AddAccessRequestsAndHttpAuditLogs.cs`** already exists in your codebase with all the necessary schema changes, but it hasn't been applied to your database.

### Option 1: Use Entity Framework Migrations (RECOMMENDED)

Open **Package Manager Console** in Visual Studio and run:

```powershell
Update-Database -Project PermiTrack.DataContext -StartupProject PermiTrack
```

This will:
- ? Add the missing columns to `AccessRequests`
- ? Create the `HttpAuditLogs` table
- ? Update the `__EFMigrationsHistory` table

### Option 2: Manual SQL Script

If EF migrations don't work, run the **`MANUAL_MIGRATION_SCRIPT.sql`** file:

1. Open **SQL Server Management Studio** or **Azure Data Studio**
2. Connect to your SQL Server instance: `localhost,1433`
3. Open the `MANUAL_MIGRATION_SCRIPT.sql` file (in the project root)
4. Execute the script

The script is **idempotent** (safe to run multiple times).

## ?? After Applying the Migration

### 1. Restart the Backend

Stop and restart your ASP.NET Core application. You should see in the logs:

```
PermiTrack.Program: Information: Database migrations applied successfully at startup.
```

### 2. Verify in the Database

Run this SQL query to confirm:

```sql
-- Check AccessRequests columns
SELECT COLUMN_NAME 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'AccessRequests' 
  AND COLUMN_NAME IN ('ProcessedByUserId', 'ProcessedAt', 'RequestedDurationHours', 'ReviewerComment');

-- Check HttpAuditLogs table exists
SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'HttpAuditLogs';
```

You should see **4 columns** from AccessRequests and **1 table** (HttpAuditLogs).

### 3. Test Registration

Go to `http://localhost:5174/register` and try to register a new user. You should:

- ? See a success message
- ? No 500 errors in the browser console
- ? New user created in the database

## ?? Additional Issues Fixed

### CORS Configuration
Updated `Program.cs` to allow requests from both:
- `http://localhost:5173`
- `http://localhost:5174`

### Email Authentication (Optional)
The registration process sends a welcome email. If you see `MailKit.Security.AuthenticationException` in the logs, it's because SMTP isn't configured. This is **not blocking** - the user is still created successfully.

To fix email (optional):

1. Open **`secrets.json`** (already accessible via User Secrets)
2. Update the Email section:

```json
"Email": {
  "FromName": "PermiTrack",
  "FromAddress": "your-email@gmail.com",
  "SmtpServer": "smtp.gmail.com",
  "SmtpPort": "587",
  "Username": "your-email@gmail.com",
  "Password": "your-app-specific-password"
}
```

For Gmail, you need an [App Password](https://support.google.com/accounts/answer/185833).

## ?? Common Issues

### "Migration already applied"
If you see `No migrations were applied. The database is already up to date.` but still get errors:

- Check that `__EFMigrationsHistory` contains `20251129140000_AddAccessRequestsAndHttpAuditLogs`
- If not, use Option 2 (Manual SQL Script)

### "Cannot find table HttpAuditLogs"
Your database is out of sync. Follow Option 2 (Manual SQL Script).

### Registration works but shows error
Check if the email is failing (see logs for `MailKit.Security.AuthenticationException`). This is non-blocking.

## ?? Quick Start After Fix

1. ? Apply migration (Option 1 or 2 above)
2. ? Restart backend (`Ctrl+F5` in Visual Studio)
3. ? Refresh frontend (`http://localhost:5174`)
4. ? Try registration - should work perfectly!

## ?? Verification Checklist

After applying the migration, verify:

- [ ] Backend starts without errors
- [ ] No "Invalid object name 'HttpAuditLogs'" in logs
- [ ] No "Invalid column name 'ProcessedByUserId'" in logs
- [ ] Registration succeeds (check `/api/auth/register`)
- [ ] User appears in the `Users` table
- [ ] HTTP requests are logged in `HttpAuditLogs` table

---

**Need Help?** Check the debug logs in Visual Studio's **Output Window** (View ? Output ? Show output from: Debug).
