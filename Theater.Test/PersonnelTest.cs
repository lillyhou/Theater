using Bogus;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Theater.Application.Infrastructure;
using Theater.Application.Model;

namespace Theater.Test
{
    [Collection("Sequential")]
    public class PersonnelTest : TheatercontextTest
    {
        [Fact]
        public void AddContractSuccessTest()
        {
            using var db = GetDatabase(deleteDb: true);
            db.Seed();
            var personnel1 = db.Personnels.First();
            var personnel2 = db.Personnels.Skip(1).FirstOrDefault();

            personnel1.AddContract(new WorkContract(new DateRange(new DateTime(2024, 01, 01), new DateTime(2026, 06, 30)), 2300));
            personnel2.AddContract(new WorkContract(new DateRange(new DateTime(2023, 05, 15), new DateTime(2026, 06, 30)), 3300));
            db.SaveChanges();
            db.ChangeTracker.Clear();

            Assert.True(db.WorkContracts.Count() == 2);
        }

        [Fact]
        public void AddContractFailTest()               //with negative salary
        {
            using var db = GetDatabase(deleteDb: true);
            db.Seed();

            var personnel1 = db.Personnels.First();
            var personnel2 = db.Personnels.Skip(1).FirstOrDefault();

            Assert.Throws<ArgumentException>(() =>
                personnel1.AddContract(new WorkContract(
                    new DateRange(new DateTime(2024, 01, 01), new DateTime(2026, 06, 30)), -100))
            );

            personnel2.AddContract(new WorkContract(
                new DateRange(new DateTime(2023, 05, 15), new DateTime(2026, 06, 30)), 3300));

            db.SaveChanges();
            db.ChangeTracker.Clear();

            Assert.Equal(1, db.WorkContracts.Count()); 
        }


        [Fact]
        public void StopContractSuccessTest()
        {
            using var db = GetDatabase(deleteDb: false);
            var personnel1 = db.Personnels.First();
            var personnel2 = db.Personnels.Skip(1).FirstOrDefault();
            var workContract1 = db.WorkContracts.First();
            var workContract2 = db.WorkContracts.Skip(1).FirstOrDefault();

            personnel1.StopContract(workContract1, new DateTime(2025, 02, 02), "Overwork");
            db.SaveChanges();
            db.ChangeTracker.Clear();
            Assert.True(db.Personnels.First().WorkContracts.OfType<ExpireContract>().Count() == 1);
        }

        [Fact]
        public void CheckInvalidContractsSuccessTest()
        {
            using var db = GetDatabase(deleteDb: true);

            var personnel = new Personnel("John Doe", new DateTime(1985, 5, 15), "Actor", "john.doe@example.com", PersonnelRole.Performer);
            var expiredContract = new WorkContract( new DateRange(DateTime.Now.AddYears(-1), DateTime.Now.AddDays(-1)), 5000);
            var activeContract = new WorkContract( new DateRange(DateTime.Now, DateTime.Now.AddYears(1)), 7000);
            personnel.AddContract(expiredContract);
            personnel.AddContract(activeContract);

            db.Add(personnel);
            personnel.CheckInvalidContracts();

            db.SaveChanges();
            db.ChangeTracker.Clear();

            var reloadedPersonnel = db.Personnels.FirstOrDefault();

            Assert.Single(reloadedPersonnel!.WorkContracts.OfType<ExpireContract>()); //reloadedPersonnel! this is not null
            Assert.Single(reloadedPersonnel.WorkContracts.Where(wc => wc.Period.EndDate > DateTime.Now));
        }


        [Fact]
        public void GetActiveContractsSuccessTest()
        {
            using var db = GetDatabase(deleteDb: true);
            db.Seed();
            var personnel1 = db.Personnels.First();

            personnel1.AddContract(new WorkContract(new DateRange(new DateTime(2024, 01, 01), new DateTime(2026, 06, 30)), 2300));
            personnel1.AddContract(new WorkContract(new DateRange(new DateTime(2024, 06, 01), new DateTime(2027, 02, 02)), 4500));
            personnel1.AddContract(new WorkContract(new DateRange(new DateTime(2023, 05, 15), new DateTime(2026, 06, 30)), 3300));
            personnel1.AddContract(new WorkContract(new DateRange(new DateTime(2025, 01, 12), new DateTime(2025, 07, 22)), 3300));
            personnel1.StopContract(new WorkContract(new DateRange(new DateTime(2024, 03, 09), new DateTime(2028, 08, 10)), 6300),DateTime.Now, "testing");
            db.SaveChanges();
            db.ChangeTracker.Clear();

            var activeContractList = personnel1.GetActiveContracts(2024);
            Assert.True(activeContractList.Count == 2);

        }


        [Fact]
        public void GetContractAmounSuccessTest() 
        {
            using var db = GetDatabase(deleteDb: true);
            db.Seed();
            var personnel1 = db.Personnels.First();

            personnel1.AddContract(new WorkContract(new DateRange(new DateTime(2024, 01, 01), new DateTime(2026, 06, 30)), 2300));
            personnel1.AddContract(new WorkContract(new DateRange(new DateTime(2024, 06, 01), new DateTime(2027, 02, 02)), 4500));
            personnel1.AddContract(new WorkContract(new DateRange(new DateTime(2023, 05, 15), new DateTime(2026, 06, 30)), 3300));
            personnel1.AddContract(new WorkContract(new DateRange(new DateTime(2025, 01, 12), new DateTime(2025, 07, 22)), 3300));
            personnel1.StopContract(new WorkContract(new DateRange(new DateTime(2024, 03, 09), new DateTime(2028, 08, 10)), 6300), DateTime.Now, "testing");
            db.SaveChanges();
            db.ChangeTracker.Clear();

            Assert.True(personnel1.GetContractAmount() == 5);
        }

        [Fact]
        public void AvgWorkContractLength_SuccessTest() 
        {
            using var db = GetDatabase(deleteDb: true);
            db.Seed();
            var personnel1 = db.Personnels.First();

            personnel1.AddContract(new WorkContract(new DateRange(new DateTime(2024, 01, 01), new DateTime(2024, 01, 15)), 2300));
            personnel1.AddContract(new WorkContract(new DateRange(new DateTime(2024, 06, 01), new DateTime(2024, 06, 20)), 4500));
            db.SaveChanges();
            db.ChangeTracker.Clear();

            Assert.Equal(17.5m, personnel1.AvgWorkContractLength());

        }


        [Fact]
        public void GetRoleListSuccessTest() 
        {
            using var db = GetDatabase(deleteDb: true);

            db.Seed();
            var performance = db.Performances.First();
            var auditionDate1 = performance.PerformanceSeason.StartDate.AddMonths(-3);
            var audition1 = new Audition(auditionDate1, "London");
            performance.AddAudition(audition1);
            var auditionDate2 = performance.PerformanceSeason.StartDate.AddMonths(-4);
            var audition2 = new Audition(auditionDate2, "London");
            performance.AddAudition(audition2);
            var personnel1 = db.Personnels.First();
            audition1.DecideOnRole(new Cast("Pina", personnel1));
            var personnel2 = db.Personnels.Skip(1).FirstOrDefault();
            audition1.DecideOnRole(new Cast("Bausch", personnel2));
            audition2.SetSecondCast(new Cast("Ali", personnel1), true);
            audition2.SetSecondCast(new Cast("Pina", personnel2), false);
            audition2.SetSecondCast(new Cast("Group dancer x", personnel2), true);


            db.SaveChanges();
            db.ChangeTracker.Clear();

           Assert.Equal(2, personnel1.GetRoleList().Count);
           Assert.Equal(3, personnel2.GetRoleList().Count);

        }

        [Fact]
        public void IsBirthdayInPerformancePeriod_SuccessTest()
        {
            using var db = GetDatabase(deleteDb: true);

            var personnel = new Personnel("Gina Hofmann", new DateTime(1991, 02, 03), "Singer", "gina_h@gmail.com", PersonnelRole.Performer);
            var performance = new Performance("Elsa's world", new DateRange(new DateTime(2025, 01, 01), new DateTime(2025, 03, 01)));
            var audition = new Audition(new DateTime(2024, 11, 11), "Vienna");
            performance.AddAudition(audition);
            var cast = new Cast("Uli", personnel);
            audition.DecideOnRole(cast);

            db.Personnels.Add(personnel);
            db.Performances.Add(performance);
            db.Auditions.Add(audition);
            db.Casts.Add(cast);

            db.SaveChanges();
            db.ChangeTracker.Clear();  

            var savedPersonnel = db.Personnels.FirstOrDefault(p => p.Name == "Gina Hofmann");
            Assert.True(personnel.IsBirthdayInPerformancePeriod());
        }

        [Fact]
        public void IsBirthdayInPerformancePeriod_FailTest()
        {
            using var db = GetDatabase(deleteDb: true);

            var personnel = new Personnel("Gina Hofmann", new DateTime(1991, 02, 03), "Singer", "gina_h@gmail.com", PersonnelRole.Performer);
            var performance = new Performance("Elsa's world", new DateRange(new DateTime(2025, 05, 01), new DateTime(2025, 06, 01)));
            var audition = new Audition(new DateTime(2024, 11, 11), "Vienna");
            performance.AddAudition(audition);
            var cast = new Cast("Uli", personnel);
            audition.DecideOnRole(cast);

            db.Personnels.Add(personnel);
            db.Performances.Add(performance);
            db.Auditions.Add(audition);
            db.Casts.Add(cast);

            db.SaveChanges();
            db.ChangeTracker.Clear();  

            var savedPersonnel = db.Personnels.FirstOrDefault(p => p.Name == "Gina Hofmann");
            Assert.NotNull(savedPersonnel);

            Assert.False(personnel.IsBirthdayInPerformancePeriod());  // This should fail since the birthday is outside the range
        }

    }
}
