using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Theater.Application.Model
{
    public class DateRange 
    {
        public DateRange(DateTime startDate, DateTime endDate)
        {
            if (endDate <= startDate)
            {
                throw new ArgumentException("EndDate must be after StartDate.");
            }

            StartDate = startDate;
            EndDate = endDate;
        }
        public DateTime StartDate { get; private set; } 
        public DateTime EndDate { get; private set; }

        public int Days()
        {
            return (EndDate - StartDate).Days +1; 
        }

        public void ExtendDate(DateTime newEndDate)
        {
            if (newEndDate <= StartDate)
                throw new ArgumentException("New end date must be after start date.");
            EndDate = newEndDate;  
        }

        //if with record class, Equals() is automatically
        public override bool Equals(object obj)  //manually give the Equals method.  Compare objects by their value, not by reference.
        {
            if (obj is DateRange other)
            {
                return StartDate == other.StartDate && EndDate == other.EndDate;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(StartDate, EndDate);
        }
    }
}
