using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
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
            builder.UseEnvironment("Testing");

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

                services.AddDbContext<DataDbContext>(o =>
                    o.UseSqlite(_connection)
                     .ConfigureWarnings(w =>
                         w.Ignore(RelationalEventId.PendingModelChangesWarning)));

                services.AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, AuthorizeHandler>("Test", _ => { });
            });
        }

        // ✅ Seed AFTER the host is fully built, avoiding double schema creation
        protected override IHost CreateHost(IHostBuilder builder)
        {
            var host = base.CreateHost(builder);

            using var scope = host.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<DataDbContext>();
            db.Database.EnsureCreated();
            DbSeeder.Seed(db);

            return host;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                _connection?.Dispose();
            }
        }
    }
}
