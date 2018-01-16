using System.Collections.Generic;

namespace LotReport.Models
{
    public class DieRow
    {
        public DieRow(List<Die> dies)
        {
            this.Dies = dies;
        }

        public List<Die> Dies { get; private set; }
    }
}
