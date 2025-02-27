using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Theater.Application.Model
{
    [Table("Performance")]
    public class Performance
    {
        public Performance(string title, DateRange performanceSeason)
        {
            Title = title;
            PerformanceSeason = performanceSeason;
            Guid = Guid.NewGuid();

        }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        protected Performance() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        public int Id { get; private set; }

        [MaxLength(220)]
        public string Title { get; set; }
        public DateRange PerformanceSeason { get; set; }
        public Guid Guid { get; private set; }

        protected List<Audition> _auditions = new();
        public virtual IReadOnlyCollection<Audition> Auditions => _auditions;

        public void AddAudition(Audition audition)
        {
            if (audition.Date < PerformanceSeason.StartDate)
            {
                _auditions.Add(audition);
            }
        }

        public int AuditionAmount(DateTime date) 
        {
            return _auditions.Where(a => a.Date == date).Count();
        }

        public IReadOnlyCollection<Audition> GetSameLocationAuditions( string location)
        {
            return _auditions.Where(a => a.Location == location).ToList().AsReadOnly();
        }

        public void ExtendPerformanceSeason(int days) 
        {
            if (days < 1)
            {
                throw new ArgumentException("Days to extend must be more than 0.");
            }
            DateTime newEndDate = PerformanceSeason.EndDate.AddDays(days);

            PerformanceSeason.ExtendDate(newEndDate);
        }


    }
}
