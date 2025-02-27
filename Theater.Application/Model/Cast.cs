using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Theater.Application.Model
{
    [Table("Cast")]
    public class Cast
    {
        public Cast(string role, Personnel personnel)
        {
            Role = role;
            Personnel = personnel;
            PersonnelId = personnel.Id;
            Audition = default!; // non-nullable reference property
        }

        protected Cast(Cast cast) //for SecondCast class
        {
            Id = cast.Id;
            Role = cast.Role;
            Personnel = cast.Personnel;
            PersonnelId = cast.PersonnelId;
            Audition = cast.Audition;
        }
       

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public Cast() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        public int Id { get; private set; }
        public string Role { get; set; }
        public int PersonnelId { get; private set; }
        public virtual Personnel Personnel { get; set; }
        public int AuditionId { get; set; }
        public virtual Audition Audition { get; private set; }
        public string CastType { get; private set; }
    }
}
