using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LotReport.Models.DirectoryItems
{
    public sealed class FolderItem : Item
    {
        public FolderItem()
        {
            this.Items = new List<Item>();
        }

        public List<Item> Items { get; set; }
    }
}
