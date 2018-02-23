using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LotReport.Models
{
    public class BinCodeRepository
    {
        public BinCodeRepository()
        {
            BinCodes = new List<BinCode>();
        }

        public List<BinCode> BinCodes { get; private set; }

        public void LoadFromFile()
        {
            BinCodes.Clear();

            XDocument document = XDocument.Load(Settings.BinCodeDirectory);

            foreach (XElement binCodeElement in document.Root.Elements())
            {
                BinCode binCode = new BinCode();

                if (int.TryParse(binCodeElement.Element("Id").Value, out int id))
                {
                    binCode.Id = id;
                }

                binCode.Value = binCodeElement.Element("Value").Value;
                binCode.Description = binCodeElement.Element("Description").Value;

                if (bool.TryParse(binCodeElement.Element("Mark").Value, out bool mark))
                {
                    binCode.Mark = mark;
                }

                BinCodes.Add(binCode);
            }
        }

        public void SaveToFile()
        {
            FileInfo file = new FileInfo(Settings.BinCodeDirectory);
            file.Directory.Create();

            List<BinCode> sortedBinCodes = BinCodes.OrderBy(bin => bin.Id).ToList();

            XDocument document = new XDocument(new XElement("BinCodes"));

            foreach (BinCode binCode in sortedBinCodes)
            {
                // Create new BinCode Element
                XElement element = new XElement("BinCode");
                element.Add(new XElement("Id", binCode.Id));
                element.Add(new XElement("Value", binCode.Value));
                element.Add(new XElement("Description", binCode.Description));
                element.Add(new XElement("Mark", binCode.Mark));

                // Add BinCode Element to current XDocment
                document.Root.Add(element);
            }

            document.Save(Settings.BinCodeDirectory);
        }
    }
}
