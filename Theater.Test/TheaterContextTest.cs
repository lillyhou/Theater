using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Theater.Application.Infrastructure;

namespace Theater.Test
{
    [Collection("Sequential")]
    public class TheatercontextTest 
    {
        protected TheaterContext GetDatabase(bool deleteDb = false)
        {
            var db = new TheaterContext(new DbContextOptionsBuilder()
                .UseSqlite("Data Source=Theater.db")
                .UseLazyLoadingProxies()
                 .Options);
            if (deleteDb)
            {
                db.Database.EnsureDeleted();
                db.Database.EnsureCreated();
            }
            return db;
        }

        [Fact]
        public void CreateDatabaseSuccessTest()
        {
            using var db = GetDatabase(deleteDb: true);
        }


        [Fact]
        public void SeedDatabaseTest()
        {
            using var db = GetDatabase(deleteDb: true);
            db.Seed();
            Assert.True(db.Personnels.Count() == 20);
            Assert.True(db.Performances.Count() == 20);
        }

    }
}
