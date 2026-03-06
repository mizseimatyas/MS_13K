using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using WebShop;
using WebShop.Persistence;
using WebShop.Utils;

namespace ModelTest
{
    public class CustomApplicationFactory : WebApplicationFactory<Program>
    {
        private SqliteConnection _connection = null;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<DbContextOptions<DataDbContext>>();
                services.RemoveAll<DbContextOptions>();
                services.RemoveAll<DataDbContext>();

                services.RemoveAll<IDbContextPool<DataDbContext>>();
                services.RemoveAll<IScopedDbContextLease<DataDbContext>>();
                services.RemoveAll<IDbContextFactory<DataDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<DataDbContext>>();

                _connection = new SqliteConnection("Data Source=:memory:");
                _connection.Open();

                services.AddDbContext<DataDbContext>(o => o.UseSqlite(_connection));

                services.AddAuthentication("Test")
                .AddScheme<AuthenticationSchemeOptions, AuthorizeHandler>("Test", _ => { });
            });

            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<DataDbContext>();
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
                DbSeeder.Seed(db);
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (!disposing)
            {
                _connection?.Dispose();
            }
        }
    }
}
