using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LotReport.Models.DirectoryItems
{
    public static class DirectoryProvider
    {
        public static List<Item> GetItems(string path)
        {
            List<Item> items = new List<Item>();
            DirectoryInfo directoryInfo = new DirectoryInfo(path);

            foreach (DirectoryInfo directory in directoryInfo.GetDirectories())
            {
                FolderItem item = new FolderItem()
                {
                    Name = directory.Name,
                    Path = directory.FullName,
                    Items = GetItems(directory.FullName)
                };

                items.Add(item);
            }

            foreach (FileInfo file in directoryInfo.GetFiles())
            {
                var item = new FileItem()
                {
                    Name = file.Name,
                    Path = file.FullName
                };

                items.Add(item);
            }

            return items;
        }
    }
}
