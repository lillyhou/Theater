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
    public class AuditionTest : TheatercontextTest
    {
        [Fact]
        public void DecideOnRoleSuccessTest()
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
                var auditionDate3 = performance.PerformanceSeason.StartDate.AddMonths(-5);
                var audition3 = new Audition(auditionDate3, "Paris");
                performance.AddAudition(audition3);
                var personnel1 = db.Personnels.First();
                audition1.DecideOnRole(new Cast("Pina", personnel1));
                var personnel2 = db.Personnels.Skip(1).FirstOrDefault();
                audition1.DecideOnRole(new Cast("Bausch", personnel2));

                db.SaveChanges();
                db.ChangeTracker.Clear();
                Assert.True(db.Casts.Count() == 2);
        }

        [Fact]
        public void SetSecondCastSuccessTest()
        {
            {
                using var db = GetDatabase(deleteDb: true);
                db.Seed();
                var performance = db.Performances.First();
                var auditionDate1 = performance.PerformanceSeason.StartDate.AddMonths(-3);
                var audition1 = new Audition(auditionDate1, "London");
                performance.AddAudition(audition1);
                var personnel1 = db.Personnels.First();
                audition1.DecideOnRole(new Cast("Pina", personnel1));
                var personnel2 = db.Personnels.Skip(1).FirstOrDefault();
                db.SaveChanges();

                var cast = db.Casts.First();        //reload from db

                audition1.SetSecondCast(cast, true);
                db.SaveChanges();
                db.ChangeTracker.Clear();
            }
            {
                using var db = GetDatabase(deleteDb: false);
                Assert.True(db.Auditions.First().Casts.OfType<SecondCast>().Count() == 1);
            }
        }


        [Fact]
        public void DecideOnRoleFailedTest()
        {
            var updateException = false;
            try
            {
                using var db = GetDatabase(deleteDb: true);
                db.Seed();
                var performance = db.Performances.First();

                var auditionDate = performance.PerformanceSeason.StartDate.AddMonths(-3);
                var audition = new Audition(auditionDate, "London");
                performance.AddAudition(audition);
                db.SaveChanges();

                var personnel = db.Personnels.First();
                var cast = new Cast("DoubleRole", personnel);  //because the cast cnanot added outside Audition class. Only with DecodeOnRole()
                db.Casts.Add(cast);
                db.SaveChanges();
            }
            catch (DbUpdateException)
            {
                updateException = true; //if it changed to true, means "DbUpdateException" called
            }
            Assert.True(updateException);
        }

        [Fact]
        public void CastByProfesstionListSuccessTest()
        {
                using var db = GetDatabase(deleteDb: true);
                db.Seed();

                var performance = db.Performances.First();
                var auditionDate1 = performance.PerformanceSeason.StartDate.AddMonths(-3);
                var audition1 = new Audition(auditionDate1, "Berlin");
                performance.AddAudition(audition1);
               
                var personnel1 = db.Personnels.First();
                audition1.DecideOnRole(new Cast("Pina", personnel1));
                var personnel2 = db.Personnels.Skip(1).FirstOrDefault();
                audition1.DecideOnRole(new Cast("Bausch", personnel2));

                db.SaveChanges();
                db.ChangeTracker.Clear();

                var profession = db.Personnels.First().Profession;
                var result = audition1.CastByProfesstionList(profession);

                var expectedCast = audition1.Casts.FirstOrDefault(c => c.PersonnelId == personnel1.Id);
                Assert.NotNull(expectedCast);
                Assert.Contains(expectedCast, result);
        }

        [Fact]
        public void GetAdditionalCostumeList_SuccessAndFailTest()
        { 
            using var db = GetDatabase(deleteDb: true);
            db.Seed();

            var performance = db.Performances.First();
            var auditionDate1 = performance.PerformanceSeason.StartDate.AddMonths(-6);
            var audition1 = new Audition(auditionDate1, "Seattle");
            performance.AddAudition(audition1);

            var personnel1 = db.Personnels.First();
            var personnel2 = db.Personnels.Skip(1).FirstOrDefault();
            var personnel3 = db.Personnels.Skip(2).FirstOrDefault();

            audition1.SetSecondCast(new Cast("Conrad", personnel1), false);
            audition1.SetSecondCast(new Cast("Medora", personnel2), false);
            audition1.SetSecondCast(new Cast("Ali", personnel3), true);
            db.SaveChanges();
            db.ChangeTracker.Clear();
            var result = audition1.GetAdditionalCostumeList();
            var unExpectResult = audition1.Casts.Skip(2).FirstOrDefault();
            
            Assert.True(result.Count == 2);
            Assert.NotNull(unExpectResult);
            Assert.DoesNotContain(unExpectResult, result); //make sure "Ali" is not included.
        }


        [Fact]
        public void GetPerformerList_SuccessTest() 
        {
            using var db = GetDatabase(deleteDb: true);
            db.Seed();

            var performance = db.Performances.First();
            var auditionDate1 = performance.PerformanceSeason.StartDate.AddMonths(-6);
            var audition1 = new Audition(auditionDate1, "Seattle");
            performance.AddAudition(audition1);

            var personnel1 = new Personnel("Gina Hofmann", new DateTime(1979, 02, 03), "Singer", "gina_h@gmail.com", PersonnelRole.Performer);
            var personnel2 = new Personnel("Sylvie Guillem", new DateTime(1985, 07, 29), "Dancer", "gina_h@gmail.com", PersonnelRole.Performer);
            var personnel3 = new Personnel("Sergei Polunin", new DateTime(1996, 12, 23), "Dancer", "gina_h@gmail.com", PersonnelRole.Performer);
            var personnel4 = new Personnel("Steven McRae", new DateTime(1985, 10, 21), "Dancer", "gina_h@gmail.com", PersonnelRole.StageCrew);


            db.Personnels.Add(personnel1);
            db.Personnels.Add(personnel2);
            db.Personnels.Add(personnel3);

            audition1.DecideOnRole(new Cast("Conrad", personnel1));
            audition1.SetSecondCast(new Cast("Medora", personnel2), false);
            audition1.SetSecondCast(new Cast("Ali", personnel3), true);
            audition1.DecideOnRole(new Cast("Stage Crew", personnel4));
            db.SaveChanges();
            db.ChangeTracker.Clear();

            Assert.Equal(3, audition1.GetPerformerList().Count);

        }


    }
}
