using Microsoft.EntityFrameworkCore;
using OnlineBeratungstermin.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace OnlineTerminAPITests
{
    [CollectionDefinition("Database Collection")]
    public class DatabaseCollection : ICollectionFixture<DatabaseFixture>
    {
        // This class has no code, but it is used to define the collection.
        // The DatabaseFixture is created and disposed once per collection.
    }

    public class DatabaseFixture : IDisposable
    {
        public DbContext _dbContext { get; private set; }

        public DatabaseFixture()
        {
            // Set up DbContextOptions with your desired configuration
            var options = new DbContextOptionsBuilder<OnlineTermineDbContext>()
                .UseSqlServer("Server=.;Database=OnlineTermineDB;User Id=sa;Password=qaz@123;pooling=false;Timeout=60;TrustServerCertificate=True;")
                .Options;

            // Initialize the DbContext object
            _dbContext = new OnlineTermineDbContext(options);
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }
    }
}
