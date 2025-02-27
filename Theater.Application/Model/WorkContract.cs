using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Theater.Application.Model
{
    [Table("WorkContract")]
    public class WorkContract
    {
        public WorkContract(DateRange period, decimal monthlySalary)
        {
            Period = period;
            MonthlySalary = monthlySalary;
            Guid = Guid.NewGuid();
            Personnel = default!;
        }
        protected WorkContract(WorkContract workContract) //for the ExpireContract class
        {
            Id = workContract.Id;
            Period = new DateRange(workContract.Period.StartDate, workContract.Period.EndDate);
            MonthlySalary = workContract.MonthlySalary;
            Personnel = workContract.Personnel;
            PersonnelId = workContract.PersonnelId;
            Guid = workContract.Guid;
        }



#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        protected WorkContract() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.

        public int Id { get; private set; }
        public DateRange Period { get; set; }

        private decimal _monthlySalary;

        public decimal MonthlySalary
        {
            get => _monthlySalary;
            private set
            {
                if (value < 0)
                    throw new ArgumentException("Salary cannot be negative.", nameof(value));
                _monthlySalary = value;
            }
        }
        public int PersonnelId { get; set; }
        public virtual Personnel Personnel { get; private set; }
        public Guid Guid { get; private set; }


    }
}
