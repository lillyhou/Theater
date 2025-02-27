using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Theater.Application.Model;

namespace Theater.Test
{
    [Collection("Sequential")]
    public class PerformanceTest : TheatercontextTest
    {
        [Fact]
        public void AddAuditionSuccessTest()
        {
            using (var db = GetDatabase(deleteDb: true))
            {
                db.Seed();
                var performance = db.Performances.First();
                var auditionDate = performance.PerformanceSeason.StartDate.AddMonths(-2);
                var audition = new Audition(auditionDate, "Vienna");
                performance.AddAudition(audition);
                db.SaveChanges();
                db.ChangeTracker.Clear();

                Assert.True(db.Auditions.Count() == 1);
            }
        }


        [Fact]
        public void AddAuditionFailTest()
        {
            using (var db = GetDatabase(deleteDb: true))
            {
                db.Seed();
                var performance = db.Performances.First();
                var auditionDate = performance.PerformanceSeason.StartDate.AddMonths(1);  //the audition date should be earlier than the performance.
                var audition = new Audition(auditionDate, "Tokyo");
                performance.AddAudition(audition);
                db.SaveChanges();
                db.ChangeTracker.Clear();

                Assert.True(db.Auditions.Count() == 0);
            }
        }


        [Fact]
        public void AuditionAmount_SuccessAndFailTest()
        {
            using (var db = GetDatabase(deleteDb: true))
            {
                db.Seed();
                var performance = db.Performances.First();
                var auditionDate1 = performance.PerformanceSeason.StartDate.AddMonths(-4); 
                var audition1 = new Audition(auditionDate1, "Vienna");
                performance.AddAudition(audition1);

                var audition2 = new Audition(auditionDate1, "Linz");
                performance.AddAudition(audition2);
                
                var audiitionDateForAudition3 = performance.PerformanceSeason.StartDate.AddMonths(-5); //different
                var audition3 = new Audition(audiitionDateForAudition3, "Amsterdam");
                db.SaveChanges();
                db.ChangeTracker.Clear();

                Assert.True(db.Auditions.Count() == 2);

                var totalAuditionsAmount = performance.AuditionAmount(auditionDate1);
                Assert.True(totalAuditionsAmount == 2);
            }

        }

        [Fact]
        public void GetSameLocationAuditions_SuccessTest()
        {
            {
                using var db = GetDatabase(deleteDb: true);
                db.Seed();
                var performance = db.Performances.First();
                var auditionDate1 = performance.PerformanceSeason.StartDate.AddMonths(-4);
                var audition1 = new Audition(auditionDate1, "London");
                performance.AddAudition(audition1);
                var auditionDate2 = performance.PerformanceSeason.StartDate.AddMonths(-5);
                var audition2 = new Audition(auditionDate2, "London");
                performance.AddAudition(audition2);
                var auditionDate3 = performance.PerformanceSeason.StartDate.AddMonths(-6);
                var audition3 = new Audition(auditionDate3, "Paris");
                performance.AddAudition(audition3);

                db.SaveChanges();
                db.ChangeTracker.Clear();
                var result = performance.GetSameLocationAuditions("London").Count;
                Assert.True(result == 2);
            }
        }


        [Fact]
        public void ExtendPerformanceSeason_SuccessTest()
        {
            using (var db = GetDatabase(deleteDb: true))
            {
                db.Seed();
                var performance = db.Performances.First();
                var oldEndDate = performance.PerformanceSeason.EndDate;

                performance.ExtendPerformanceSeason(10);
                db.SaveChanges();
                db.ChangeTracker.Clear();

                Assert.Equal(oldEndDate.AddDays(10), performance.PerformanceSeason.EndDate);
            }
        }

        [Fact]
        public void GetAllTestingData() 
        {
            using var db = GetDatabase(deleteDb: true);
            db.AllClassSeed();

            var personnel = db.Personnels.First();
            var workContract = db.WorkContracts.First();
            personnel.StopContract(workContract, DateTime.Now, "overworked");
            db.SaveChanges();
            db.ChangeTracker.Clear();


            Assert.NotNull(db.Personnels);
            Assert.NotNull(db.WorkContracts);
            Assert.NotNull(db.Performances);
            Assert.NotNull(db.Auditions);
            Assert.NotNull(db.Casts);
            Assert.NotNull(db.SecondCasts);
            Assert.NotNull(db.ExpireContracts);


        }

    }
}
