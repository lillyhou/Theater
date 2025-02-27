using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Theater.Application.Model
{
    [Table("Audition")]
    public class Audition
    {
        public Audition(DateTime date, string location)
        {
            Date = date;
            Location = location;
            Performance = default!;
        }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        protected Audition() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public int Id { get; private set; }
        public DateTime Date { get; set; }

        [MaxLength(160)]
        public string Location { get; set; }
        public int PerformanceId { get; set; }
        public virtual Performance Performance { get; private set; }


        protected List<Cast> _casts = new();
        public virtual IReadOnlyCollection<Cast> Casts => _casts;
        public void DecideOnRole(Cast cast)
        { 
            _casts.Add(cast);
        }

        public void SetSecondCast(Cast cast, bool originalCostumeFits) 
        {
            var secondCast = cast as SecondCast;
            if (secondCast != null)
            {
                secondCast.OriginalCostumeFits = originalCostumeFits;
            }
            else 
            {
                secondCast = new SecondCast(cast, originalCostumeFits);
                _casts.Remove(cast);
                _casts.Add(secondCast);
            }
        }

        public IReadOnlyCollection<Cast> CastByProfesstionList(string profession) 
        {
           return _casts.Where(c => c.Personnel.Profession == profession).ToList().AsReadOnly();
        }

        public IReadOnlyCollection<SecondCast> GetAdditionalCostumeList() 
        {
            return _casts.OfType<SecondCast>().Where(s => !s.OriginalCostumeFits).ToList().AsReadOnly();
        }

        public IReadOnlyCollection<Cast> GetPerformerList()
        {
            if (Casts != null)
            { 
                return Casts.Where(c => c.Personnel.PersonnelRole == PersonnelRole.Performer).ToList().AsReadOnly();
            }
            return new List<Cast>().AsReadOnly();
        }
    }
}
