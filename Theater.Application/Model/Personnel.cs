using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Theater.Application.Model
{
    [Table("Personnel")]
    public class Personnel
    {
        public Personnel(string name, DateTime birthday, string profession, string email, PersonnelRole personnelRole) 
        {
            Name = name;
            Birthday = birthday;
            Profession = profession;
            Email = email;
            PersonnelRole = personnelRole;
            Guid = Guid.NewGuid();
        }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        protected Personnel() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public int Id { get; private set; }

        [MaxLength(220)]
        public string Name { get; set; }
        public DateTime Birthday { get; set; }

        [MaxLength(160)]
        public string Profession { get; set; }

        [MaxLength(2200)]
        public string Email { get; set; }
        public PersonnelRole PersonnelRole { get; private set;}
        public Guid Guid { get; private set; }


        protected List<WorkContract> _workContracts = new();
        public virtual IReadOnlyCollection<WorkContract> WorkContracts => _workContracts;

        public virtual IReadOnlyCollection<Cast> Casts { get; } = new List<Cast>();
     
        public void AddContract(WorkContract workContract) 
        {
            if (workContract.MonthlySalary < 0)
                throw new ArgumentException("Monthly salary cannot be negative.");
            _workContracts.Add(workContract);
        }

        public void StopContract(WorkContract workContract, DateTime date, string reason) 
        {
            if (workContract == null)
                throw new ArgumentNullException(nameof(workContract), "Work contract cannot be null");

            var expireContract = workContract as ExpireContract;
            if (expireContract is not null)
            {
                expireContract.Date = date;
                expireContract.Reason = reason;
            }
            else 
            {
                expireContract = new ExpireContract(workContract, date, reason);
                _workContracts.Remove(workContract);
                _workContracts.Add(expireContract);
            }
        }

        public void CheckInvalidContracts()     //check all contracts, if they are expire
        {
            var today = DateTime.Now;
            var expiredContracts = _workContracts
                .Where(wc => wc.Period.EndDate <= today && wc is not ExpireContract).ToList();

            foreach (var contract in expiredContracts)
            {
                StopContract(contract, today, "Contract expired automatically");
            }
        }


        public IReadOnlyCollection<WorkContract> GetActiveContracts(int startingYear) 
        {
            return _workContracts.Where(workContract => workContract.Period.StartDate.Year == startingYear
                    && workContract is not ExpireContract).ToList().AsReadOnly();
        }

        public int GetContractAmount()
        { 
            return _workContracts.Count();
        }

        public decimal AvgWorkContractLength() 
        { 
           if (_workContracts == null || !_workContracts.Any())
            {
                return 0; 
            }
          var totalDays = _workContracts.Sum(workContract => workContract.Period.Days());

          return (decimal) totalDays / _workContracts.Count;
        }

        public IReadOnlyCollection<Cast> GetRoleList() 
        {
            return Casts.Where(c => c.PersonnelId == Id).ToList().AsReadOnly();
        }


        public bool IsBirthdayInPerformancePeriod() 
        {
            if (Casts != null) 
            {
                var birthdayMonthDay = new DateTime(1, Birthday.Month, Birthday.Day);

                if (Casts.Any(c =>
                {
                    var season = c.Audition?.Performance?.PerformanceSeason;
                    if (season == null) return false;

                    var seasonStart = new DateTime(1, season.StartDate.Month, season.StartDate.Day);
                    var seasonEnd = new DateTime(1, season.EndDate.Month, season.EndDate.Day);

                    return seasonStart <= seasonEnd
                        ? birthdayMonthDay >= seasonStart && birthdayMonthDay <= seasonEnd
                        : birthdayMonthDay >= seasonStart || birthdayMonthDay <= seasonEnd; //if the season is across into next year, "seasonStart <= seasonEnd" will be false
                }))
                {
                    return true;
                }
            }
            return false;
        }




    }
}
