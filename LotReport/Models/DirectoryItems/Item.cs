using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LotReport.Models.DirectoryItems
{
    public abstract class Item
    {
        public string Name { get; set; }

        public string Path { get; set; }
    }
}
