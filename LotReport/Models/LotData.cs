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
        public FileInfo LotFile { get; private set; }

        public string MachineId { get; set; }

        public string LotId { get; set; }

        public string OperatorId { get; set; }

        public string ProductCode { get; set; }

        public string BondingDiagram { get; set; }

        public string RecipeName { get; set; }

        public string RecipePath { get; set; }

        public int LeadFrameUnits { get; set; }

        public int LeadFrameXUnits { get; set; }

        public int LeadFrameYUnits { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public double UPH { get; set; }

        public double ProcessUPH { get; set; }

        public double LeadFrames { get; set; }

        public int LeadFramesInspected { get; set; }

        public int UnitsPassed { get; set; }

        public int UnitsRejected { get; set; }

        public int UnitsOverRejected { get; set; }

        public double UnitsYieldPercentage { get; set; }

        public double OverRejectPercentage { get; set; }

        public int MarkedUnits { get; set; }

        public int UnmarkedUnits { get; set; }

        public int MarkedUnitsPassed { get; set; }

        public int MarkedUnitsRejected { get; set; }

        public double MarkedUnitsYieldPercentage { get; set; }

        public Dictionary<string, int> DefectCount { get; set; } = new Dictionary<string, int>();

        public void LoadFromFile(string path)
        {
            LotFile = new FileInfo(path);

            XDocument doc = XDocument.Load(path);
            XElement infoElement = doc.Root.Element("Info");

            this.MachineId = infoElement.Element("MachineId").Value;
            this.LotId = infoElement.Element("LotId").Value;
            this.OperatorId = infoElement.Element("OperatorId").Value;
            this.ProductCode = infoElement.Element("ProductCode").Value;
            this.BondingDiagram = infoElement.Element("BondingDiagram").Value;
            this.RecipeName = infoElement.Element("Recipe").Value;
            this.RecipePath = infoElement.Element("RecipePath").Value;

            if (int.TryParse(infoElement.Element("LeadFrameUnits")?.Value, out int leadFrameUnits))
            {
                this.LeadFrameUnits = leadFrameUnits;
            }

            if (int.TryParse(infoElement.Element("LeadFrameXUnits")?.Value, out int leadFrameXUnits))
            {
                this.LeadFrameXUnits = leadFrameXUnits;
            }

            if (int.TryParse(infoElement.Element("LeadFrameYUnits")?.Value, out int leadFrameYUnits))
            {
                this.LeadFrameYUnits = leadFrameYUnits;
            }

            if (DateTime.TryParse(infoElement.Element("StartTime").Value, out DateTime startTime))
            {
                this.StartTime = startTime;
            }

            if (DateTime.TryParse(infoElement.Element("EndTime").Value, out DateTime endTime))
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

                writer.WriteElementString("MachineId", this.MachineId);
                writer.WriteElementString("LotId", this.LotId);
                writer.WriteElementString("OperatorId", this.OperatorId);
                writer.WriteElementString("ProductCode", this.ProductCode);
                writer.WriteElementString("BondingDiagram", this.BondingDiagram);
                writer.WriteElementString("Recipe", this.RecipeName);
                writer.WriteElementString("RecipePath", this.RecipePath);
                writer.WriteElementString("LeadFrameUnits", this.LeadFrameUnits.ToString());
                writer.WriteElementString("LeadFrameXUnits", this.LeadFrameXUnits.ToString());
                writer.WriteElementString("LeadFrameYUnits", this.LeadFrameYUnits.ToString());
                writer.WriteElementString("StartTime", this.StartTime.ToString());
                writer.WriteElementString("EndTime", this.EndTime.ToString());

                writer.WriteEndElement();

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }

        public void GenerateSummary()
        {
            string[] leadFramePaths = Directory.GetFiles(LotFile.Directory.FullName, "*.xml", SearchOption.AllDirectories);

            List<Die> visionDies = new List<Die>();
            List<Die> modifiedDies = new List<Die>();

            foreach (string lfPath in leadFramePaths)
            {
                LeadFrameMap visionLFMap = LeadFrameMap.Load(lfPath, LeadFrameMap.Type.Machine);
                visionDies.AddRange(visionLFMap.Dies);

                LeadFrameMap modifiedLFMap = LeadFrameMap.Load(lfPath, LeadFrameMap.Type.Operator);
                modifiedDies.AddRange(modifiedLFMap.Dies);
            }

            LeadFramesInspected = leadFramePaths.Length;
            UnitsPassed = modifiedDies.Count(die => die.RejectCode.Id == 0);
            UnitsRejected = modifiedDies.Count - UnitsPassed;
            UnitsOverRejected = UnitsPassed - visionDies.Count(die => die.RejectCode.Id == 0);
            UnitsYieldPercentage = (double)UnitsPassed / modifiedDies.Count * 100;
            OverRejectPercentage = (double)UnitsOverRejected / modifiedDies.Count * 100;

            MarkedUnits = modifiedDies.Count(die => die.MarkStatus != Die.Mark.NA);
            UnmarkedUnits = UnitsRejected - MarkedUnits;
            MarkedUnitsPassed = modifiedDies.Count(die => die.MarkStatus == Die.Mark.Pass);
            MarkedUnitsRejected = modifiedDies.Count(die => die.MarkStatus == Die.Mark.Fail);
            MarkedUnitsYieldPercentage = (double)MarkedUnitsPassed / MarkedUnits * 100;
            TimeSpan duration = EndTime.Subtract(StartTime);
            UPH = modifiedDies.Count / duration.TotalHours;

            foreach (Die modifiedDie in modifiedDies)
            {
                if (DefectCount.ContainsKey(modifiedDie.RejectCode.Value))
                {
                    DefectCount[modifiedDie.RejectCode.Value]++;
                }
                else
                {
                    DefectCount.Add(modifiedDie.RejectCode.Value, 1);
                }
            }
        }
    }
}
