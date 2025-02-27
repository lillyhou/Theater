using Bogus;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using Theater.Application.Model;

namespace Theater.Application.Infrastructure
{
    public class TheaterContext: DbContext
    {
        public TheaterContext(DbContextOptions opt) : base(opt) { }
        public DbSet<Audition> Auditions => Set<Audition>();
        public DbSet<Cast> Casts => Set<Cast>();
        public DbSet<ExpireContract> ExpireContracts => Set<ExpireContract>();
        public DbSet<Performance> Performances => Set<Performance>();
        public DbSet<Personnel> Personnels => Set<Personnel>();
        public DbSet<SecondCast> SecondCasts => Set<SecondCast>();
        public DbSet<WorkContract> WorkContracts => Set<WorkContract>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Personnel>().HasAlternateKey(p => p.Guid);
            modelBuilder.Entity<Performance>().HasAlternateKey(p => p.Guid);
            modelBuilder.Entity<WorkContract>().HasAlternateKey(w => w.Guid);
            modelBuilder.Entity<Personnel>().Property(p => p.Guid).ValueGeneratedOnAdd();
            modelBuilder.Entity<Performance>().Property(p => p.Guid).ValueGeneratedOnAdd();
            modelBuilder.Entity<WorkContract>().Property(w => w.Guid).ValueGeneratedOnAdd();

            modelBuilder.Entity<Performance>().OwnsOne(p => p.PerformanceSeason);  //value object
            modelBuilder.Entity<WorkContract>().OwnsOne(w => w.Period);  //value object


            modelBuilder.Entity<Personnel>()
                .HasMany(p => p.WorkContracts)
                .WithOne();

            modelBuilder.Entity<Performance>()
                .HasMany(p => p.Auditions)
                .WithOne(a => a.Performance)
                .HasForeignKey(a => a.PerformanceId);


            modelBuilder.Entity<Cast>()
                .HasOne(c => c.Personnel)
                .WithMany(p => p.Casts)
                .HasForeignKey(c => c.PersonnelId);

            modelBuilder.Entity<WorkContract>()
              .HasOne(wc => wc.Personnel)
              .WithMany(p => p.WorkContracts)
              .HasForeignKey(wc => wc.PersonnelId);


            //Discriminator //used in inheritance mappings, where a base class and its derived classes share the same table in the database.

            modelBuilder.Entity<Cast>()
                .ToTable("Cast")
                .HasDiscriminator<string>("CastType")
                .HasValue<Cast>("Cast")
                .HasValue<SecondCast>("SecondCast");

            modelBuilder.Entity<WorkContract>()
                .HasDiscriminator<string>("ContractType")
                .HasValue<WorkContract>("WorkContract")
                .HasValue<ExpireContract>("ExpireContract");


        }

        public void Seed() 
        {
            Randomizer.Seed = new Random(2048);  //every time different data , fixed one with new Random(2048);

            var theaterProfessions = new List<string>
            {
                "Actor",
                "Dancer",
                "Singer",
                "Director",
                "Playwright",
                "Stage Manager",
                "Stage Design",
                "Rehearsal Assistant",
                "Lighting Technician",
                "Sound Designer",
                "Set Designer",
                "Choreographer",
                "Costume Designer",
                "Theater Producer",
                "Musician",
                "Drama Teacher"
             };

            var personnel = new Faker<Personnel>("en").CustomInstantiator(f => new Personnel(
                f.Name.FullName(), f.Date.Between(new DateTime(1960, 1, 1), new DateTime(2005, 12, 31)), f.PickRandom(theaterProfessions), f.Internet.Email(), f.PickRandom<PersonnelRole>()))
                .Generate(20)
                .ToList();
            Personnels.AddRange(personnel); SaveChanges();

            var performance = new Faker<Performance>("en").CustomInstantiator(f =>
            {
                var startDate = f.Date.Future(1); // Generate a future start date
                var endDate = f.Date.Future(1, startDate); // Ensure the end date is after the start date
                return new Performance($"{f.Commerce.ProductAdjective()} {f.Commerce.ProductMaterial()}", new DateRange(startDate, endDate));
            })
            .Generate(20)
            .ToList();
            Performances.AddRange(performance); SaveChanges();

        }


        public void AllClassSeed() 
        {
            Randomizer.Seed = new Random();
            var theaterProfessions = new List<string>
            {
                "Actor",
                "Dancer",
                "Singer",
                "Director",
                "Playwright",
                "Stage Manager",
                "Stage Design",
                "Rehearsal Assistant",
                "Lighting Technician",
                "Sound Designer",
                "Set Designer",
                "Choreographer",
                "Costume Designer",
                "Theater Producer",
                "Musician",
                "Drama Teacher",
                "Company artist"
             };

            var personnel = new Faker<Personnel>("en").CustomInstantiator(f => new Personnel(
                f.Name.FullName(), f.Date.Between(new DateTime(1960, 1, 1), new DateTime(2005, 12, 31)), f.PickRandom(theaterProfessions), f.Internet.Email(), f.PickRandom<PersonnelRole>()))
                .Generate(20)
                .ToList();
            Personnels.AddRange(personnel); SaveChanges();

            var performance = new Faker<Performance>("en").CustomInstantiator(f =>
            {
                var startDate = f.Date.Future(1); // Generate a future start date
                var endDate = f.Date.Future(1, startDate); // Ensure the end date is after the start date
                return new Performance($"{f.Commerce.ProductAdjective()} {f.Commerce.ProductMaterial()}", new DateRange(startDate, endDate));
            })
            .Generate(20)
            .ToList();
            Performances.AddRange(performance); SaveChanges();

            //WorkContract
            foreach (var person in personnel)
            {
                var workContract = new Faker<WorkContract>().CustomInstantiator(f =>
                {
                    var startDate = f.Date.Between(new DateTime(2024, 1, 1), new DateTime(2025, 12, 31));
                    var endDate = f.Date.Between(startDate, new DateTime(2026, 12, 31));
                    return new WorkContract(new DateRange(startDate, endDate), f.Random.Decimal(1000, 5000));
                }).Generate();

                person.AddContract(workContract);
                WorkContracts.AddRange(workContract);
            }
            SaveChanges();


            //Audition
            foreach (var perf in performance)
            {
                var audition = new Faker<Audition>().CustomInstantiator(f =>
                {
                    var auditionDate = f.Date.Between(perf.PerformanceSeason.StartDate.AddMonths(-6), perf.PerformanceSeason.StartDate);
                    return new Audition(auditionDate, f.Address.City());
                }).Generate();

                perf.AddAudition(audition);
                Auditions.AddRange(audition);
            }
            SaveChanges();


            //Cast
            foreach (var audi in Auditions)
            {
                var selectedPersonnel = new Faker().PickRandom(Personnels, 5);     // Randomly select a subset of personnel for this audition 

                foreach (var person in selectedPersonnel)
                {
                    var role = new Faker().PickRandom(new[] { "Lead Role", "Supporting Role", "Main Role", "Mouse King", "Mother Ginger", "Benjamin Stahlbaum" });
                    audi.DecideOnRole(new Cast(role, person));
                }
                SaveChanges();
            }


            //SecondCast
            foreach (var audi in Auditions)
            {
                var selectedPersonnel = new Faker().PickRandom(Personnels, 2);     // Randomly select a subset of personnel for this audition 

                foreach (var person in selectedPersonnel)
                {
                    var role = new Faker().PickRandom(new[] { "Lead Role", "Supporting Role", "Main Role", "Mouse King", "Mother Ginger", "Benjamin Stahlbaum" });
                    audi.SetSecondCast(new Cast(role, person), new Faker().Random.Bool());
                }
                SaveChanges();
            }


        }

    }
}
