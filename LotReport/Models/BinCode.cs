using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LotReport.Models
{
    public class BinCode
    {
        public int Id { get; set; }

        public BinQuality Quality { get; set; }

        public string Value { get; set; }

        public string Description { get; set; }

        public bool Mark { get; set; }

        public bool SkipReview { get; set; }
    }
}
