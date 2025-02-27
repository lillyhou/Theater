using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Theater.Application.Model
{
    public class SecondCast : Cast
    {
        public SecondCast(Cast cast, bool originalCostumeFits) :base(cast)
        {
            OriginalCostumeFits = originalCostumeFits;
        }

        protected SecondCast() { }
        public bool OriginalCostumeFits { get; set; }

    }
}
