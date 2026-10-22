using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace UniversityCourseManagementAPI.Data;

public static class DatabaseMigrator
{
    private const string BaselineMigration = "20261022065229_InitialCreate";

    public static async Task MigrateAsync(ApplicationDbContext db, ILogger logger)
    {
        // A database created earlier by EnsureCreated (+ 002-security-identity.sql) already has the InitialCreate schema
        // but no migration history. Record the baseline so Migrate() does not try to create existing tables again.
        if (!(await db.Database.GetAppliedMigrationsAsync()).Any() && await TableExistsAsync(db, "Students"))
        {
            var history = db.GetService<IHistoryRepository>();
            await db.Database.ExecuteSqlRawAsync(history.GetCreateIfNotExistsScript());
            await db.Database.ExecuteSqlRawAsync(history.GetInsertScript(new HistoryRow(BaselineMigration, ProductInfo.GetVersion())));
            logger.LogInformation("Existing database baselined at migration {Migration}", BaselineMigration);
        }
        await db.Database.MigrateAsync();
    }

    private static async Task<bool> TableExistsAsync(ApplicationDbContext db, string table) =>
        await db.Database.SqlQuery<bool>($"SELECT EXISTS (SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = {table}) AS \"Value\"").SingleAsync();
}
