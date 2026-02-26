using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebShop.Persistence;

namespace ModelTest
{
    public class DbContextFactory
    {
        public static DataDbContext Create()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<DataDbContext>().UseSqlite(connection).EnableSensitiveDataLogging().Options;

            var context = new DataDbContext(options);
            context.Database.EnsureCreated();
            DbSeeder.Seed(context);
            return context;
        }

        public static DataDbContext CreateEmpty()
        {
            var connection = new SqliteConnection("Data Source=:memory:");
            connection.Open();

            var options = new DbContextOptionsBuilder<DataDbContext>().UseSqlite(connection).EnableSensitiveDataLogging().Options;

            var context = new DataDbContext(options);
            context.Database.EnsureCreated();
            return context;
        }
    }
}
