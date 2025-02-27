using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Theater.Application.Model
{
    public class ExpireContract: WorkContract
    {
        public ExpireContract(WorkContract workContract, DateTime date, string reason): base(workContract)
        {
            Date = date;
            Reason = reason;
        }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        protected ExpireContract() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.
        public DateTime Date { get; set; }

        [MaxLength(560)]
        public string Reason { get; set; }
    }
}
