using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace LotReport.Models
{
    public class RejectCodeRepository
    {
        public RejectCodeRepository()
        {
            this.RejectCodes = new List<RejectCode>();
        }

        public List<RejectCode> RejectCodes { get; private set; }

        public void LoadFromFile()
        {
            this.RejectCodes.Clear();

            XDocument document = XDocument.Load(Settings.RejectCodesDirectory);

            foreach (XElement rejectCode in document.Root.Elements())
            {
                RejectCode rc = new RejectCode();

                int id;
                if (int.TryParse(rejectCode.Element("Id").Value, out id))
                {
                    rc.Id = id;
                }

                rc.Value = rejectCode.Element("Value").Value;
                rc.Description = rejectCode.Element("Description").Value;

                bool mark;
                if (bool.TryParse(rejectCode.Element("Mark").Value, out mark))
                {
                    rc.Mark = mark;
                }

                this.RejectCodes.Add(rc);
            }
        }

        public void SaveToFile()
        {
            FileInfo file = new FileInfo(Settings.RejectCodesDirectory);
            file.Directory.Create();

            List<RejectCode> orderedRejectCodes = this.RejectCodes.OrderBy(r => r.Id).ToList();

            XDocument document = new XDocument(new XElement("RejectCodesView"));

            foreach (RejectCode rejectCode in orderedRejectCodes)
            {
                // Create new RejectCode Element
                XElement element = new XElement("RejectCode");
                element.Add(new XElement("Id", rejectCode.Id));
                element.Add(new XElement("Value", rejectCode.Value));
                element.Add(new XElement("Description", rejectCode.Description));
                element.Add(new XElement("Mark", rejectCode.Mark));

                // Add RejectCode Element to current XDocment
                document.Root.Add(element);
            }

            document.Save(file.FullName);
        }
    }
}
