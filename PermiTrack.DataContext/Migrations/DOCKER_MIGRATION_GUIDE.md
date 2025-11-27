# How to Apply Migrations to Docker Database

Since you're using Docker for SQL Server instead of LocalDB, here's how to apply migrations:

## Method 1: Using SQL Scripts (Recommended for Docker)

1. **Create the SQL script from migration file:**
   - Manually create SQL script based on the migration Up() method
   - Or generate it using tools if available

2. **Apply to Docker SQL Server:**
   ```powershell
   sqlcmd -S "localhost,1433" -d "PermiTrackDbContext" -U sa -P "YourPassword" -i "path\to\migration.sql"
   ```

## Method 2: Using dotnet-ef (If Installed)

If you have `dotnet-ef` tool installed globally:

```bash
# From the PermiTrack project directory
dotnet ef database update --project ../PermiTrack.DataContext
```

### To Install dotnet-ef tool:
```bash
dotnet tool install --global dotnet-ef
```

## Method 3: Programmatic Migration on Startup

Add this to your `Program.cs` to automatically apply migrations when the app starts:

```csharp
var app = builder.Build();

// Apply migrations automatically on startup (use with caution in production)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<PermiTrackDbContext>();
    dbContext.Database.Migrate();
}

// ... rest of your middleware configuration
```

?? **Warning:** Auto-migration on startup is convenient for development but should be used carefully in production.

## Your Docker Connection String

From your user secrets:
```json
"PermiTrackContextDocker": "Server=localhost,1433;Database=PermiTrackDbContext;User Id=sa;Password=Jelszo123!;TrustServerCertificate=True;"
```

## Creating New Migrations

Unfortunately, without the `dotnet-ef` tool, you'll need to create migrations manually. However, you can:

1. **Create migration class** following the pattern in existing migrations
2. **Generate SQL script** for the Up() and Down() methods
3. **Apply using sqlcmd** as shown in Method 1

## Quick Reference Commands

### Start Docker SQL Server (if using docker-compose):
```bash
docker-compose up -d
```

### Connect to SQL Server via sqlcmd:
```bash
sqlcmd -S localhost,1433 -U sa -P Jelszo123! -d PermiTrackDbContext
```

### Check applied migrations:
```sql
SELECT * FROM __EFMigrationsHistory ORDER BY MigrationId;
```

### View table structure:
```sql
EXEC sp_help 'Roles';  -- Replace 'Roles' with your table name
```

## Troubleshooting

### Connection Issues
- Ensure Docker container is running: `docker ps`
- Check port 1433 is not blocked by firewall
- Verify connection string in user secrets

### Migration Already Applied Error
- Check `__EFMigrationsHistory` table
- Remove the migration record if you need to reapply:
  ```sql
  DELETE FROM __EFMigrationsHistory WHERE MigrationId = 'YourMigrationId';
  ```

### Column Already Exists Error
- The migration may have been partially applied
- Check table structure and manually adjust the migration SQL
- Or drop and recreate the column if safe to do so
