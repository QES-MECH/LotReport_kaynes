using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace LotReport.Models
{
    public class LotData
    {
        public string LotId { get; set; }

        public string OperatorId { get; set; }

        public string ProductCode { get; set; }

        public string BondingDiagram { get; set; }

        public string RecipeName { get; set; }

        public string RecipePath { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public void LoadFromFile(string path)
        {
            XDocument doc = XDocument.Load(path);

            this.LotId = doc.Root.Element("Info").Element("LotId").Value;
            this.OperatorId = doc.Root.Element("Info").Element("OperatorId").Value;
            this.ProductCode = doc.Root.Element("Info").Element("ProductCode").Value;
            this.BondingDiagram = doc.Root.Element("Info").Element("BondingDiagram").Value;
            this.RecipeName = doc.Root.Element("Info").Element("Recipe").Value;
            this.RecipePath = doc.Root.Element("Info").Element("RecipePath").Value;

            if (DateTime.TryParse(doc.Root.Element("Info").Element("StartTime").Value, out DateTime startTime))
            {
                this.StartTime = startTime;
            }

            if (DateTime.TryParse(doc.Root.Element("Info").Element("EndTime").Value, out DateTime endTime))
            {
                this.EndTime = endTime;
            }
        }

        public void SaveToFile(string path)
        {
            FileInfo file = new FileInfo(path);
            file.Directory.Create();

            XmlWriterSettings xmlSettings = new XmlWriterSettings();
            xmlSettings.Indent = true;
            xmlSettings.IndentChars = "  ";

            using (XmlWriter writer = XmlWriter.Create(path, xmlSettings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("LotData");

                writer.WriteStartElement("Info");

                writer.WriteElementString("LotId", this.LotId);
                writer.WriteElementString("OperatorId", this.OperatorId);
                writer.WriteElementString("ProductCode", this.ProductCode);
                writer.WriteElementString("BondingDiagram", this.BondingDiagram);
                writer.WriteElementString("Recipe", this.RecipeName);
                writer.WriteElementString("RecipePath", this.RecipePath);
                writer.WriteElementString("StartTime", this.StartTime.ToString());
                writer.WriteElementString("EndTime", this.EndTime.ToString());

                writer.WriteEndElement();

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }
    }
}
