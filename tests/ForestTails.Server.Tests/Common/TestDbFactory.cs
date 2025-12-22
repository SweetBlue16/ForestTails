using ForestTails.Server.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ForestTails.Server.Tests.Common
{
    public class TestDbFactory : IDbContextFactory<ForestTailsDbContext>, IDisposable
    {
        private readonly SqliteConnection connection;

        public TestDbFactory()
        {
            connection = new SqliteConnection("DataSource=:memory:");
            connection.Open();
            var options = new DbContextOptionsBuilder<ForestTailsDbContext>()
                .UseSqlite(connection)
                .Options;
            using var context = new ForestTailsDbContext(options);
            context.Database.EnsureCreated();
        }

        public ForestTailsDbContext CreateDbContext()
        {
            var options = new DbContextOptionsBuilder<ForestTailsDbContext>()
                .UseSqlite(connection)
                .Options;
            return new ForestTailsDbContext(options);
        }

        public void Dispose()
        {
            connection.Dispose();
        }
    }
}
