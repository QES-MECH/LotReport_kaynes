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
        public FileInfo FileInfo { get; private set; }

        public string MachineId { get; set; }

        public string LotId { get; set; }

        public string OperatorId { get; set; }

        public string ProductCode { get; set; }

        public string DieAttach { get; set; }

        public string WireBond0 { get; set; }

        public string WireBond1 { get; set; }

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

        public Dictionary<int, int> ModifiedBinCount { get; set; } = new Dictionary<int, int>();

        public Dictionary<int, int> VisionBinCount { get; set; } = new Dictionary<int, int>();

        public void LoadFromFile(string path)
        {
            FileInfo = new FileInfo(path);

            XDocument doc = XDocument.Load(path);
            XElement infoElement = doc.Root.Element("Info");
            XElement summaryElement = doc.Root.Element("Summary");

            MachineId = infoElement.Element("MachineId")?.Value;
            LotId = infoElement.Element("LotId")?.Value;
            OperatorId = infoElement.Element("OperatorId")?.Value;
            ProductCode = infoElement.Element("ProductCode")?.Value;
            DieAttach = infoElement.Element("DieAttach")?.Value;
            WireBond0 = infoElement.Element("WireBond0")?.Value;
            WireBond1 = infoElement.Element("WireBond1")?.Value;
            BondingDiagram = infoElement.Element("BondingDiagram")?.Value;
            RecipeName = infoElement.Element("Recipe")?.Value;
            RecipePath = infoElement.Element("RecipePath")?.Value;

            if (int.TryParse(infoElement.Element("LeadFrameUnits")?.Value, out int leadFrameUnits))
            {
                LeadFrameUnits = leadFrameUnits;
            }

            if (int.TryParse(infoElement.Element("LeadFrameXUnits")?.Value, out int leadFrameXUnits))
            {
                LeadFrameXUnits = leadFrameXUnits;
            }

            if (int.TryParse(infoElement.Element("LeadFrameYUnits")?.Value, out int leadFrameYUnits))
            {
                LeadFrameYUnits = leadFrameYUnits;
            }

            if (DateTime.TryParse(infoElement.Element("StartTime")?.Value, out DateTime startTime))
            {
                StartTime = startTime;
            }

            if (DateTime.TryParse(infoElement.Element("EndTime")?.Value, out DateTime endTime))
            {
                EndTime = endTime;
            }

            if (double.TryParse(summaryElement.Element("UPH")?.Value, out double uph))
            {
                UPH = uph;
            }

            if (double.TryParse(summaryElement.Element("ProcessUPH")?.Value, out double processUPH))
            {
                ProcessUPH = processUPH;
            }

            if (int.TryParse(summaryElement.Element("LeadFrames")?.Value, out int leadFrames))
            {
                LeadFrames = leadFrames;
            }

            if (int.TryParse(summaryElement.Element("LeadFramesInspected")?.Value, out int leadFramesInspected))
            {
                LeadFramesInspected = leadFramesInspected;
            }

            if (int.TryParse(summaryElement.Element("UnitsPassed")?.Value, out int unitsPassed))
            {
                UnitsPassed = unitsPassed;
            }

            if (int.TryParse(summaryElement.Element("UnitsRejected")?.Value, out int unitsRejected))
            {
                UnitsRejected = unitsRejected;
            }

            if (int.TryParse(summaryElement.Element("UnitsOverRejected")?.Value, out int unitsOverRejected))
            {
                UnitsOverRejected = unitsOverRejected;
            }

            if (double.TryParse(summaryElement.Element("UnitsYieldPercentage")?.Value, out double unitsYieldPercentage))
            {
                UnitsYieldPercentage = unitsYieldPercentage;
            }

            if (double.TryParse(summaryElement.Element("OverRejectPercentage")?.Value, out double overRejectPercentage))
            {
                OverRejectPercentage = overRejectPercentage;
            }

            if (int.TryParse(summaryElement.Element("MarkedUnits")?.Value, out int markedUnits))
            {
                MarkedUnits = markedUnits;
            }

            if (int.TryParse(summaryElement.Element("UnmarkedUnits")?.Value, out int unmarkedUnits))
            {
                UnmarkedUnits = unmarkedUnits;
            }

            if (int.TryParse(summaryElement.Element("MarkedUnitsPassed")?.Value, out int markedUnitsPassed))
            {
                MarkedUnitsPassed = markedUnitsPassed;
            }

            if (int.TryParse(summaryElement.Element("MarkedUnitsRejected")?.Value, out int markedUnitsRejected))
            {
                MarkedUnitsRejected = markedUnitsRejected;
            }

            if (double.TryParse(summaryElement.Element("MarkedUnitsYieldPercentage")?.Value, out double markedUnitsYieldPercentage))
            {
                MarkedUnitsYieldPercentage = markedUnitsYieldPercentage;
            }

            XElement modifiedRejectsElement = summaryElement.Element("ModifiedRejects");

            foreach (XElement reject in modifiedRejectsElement.Elements())
            {
                int.TryParse(reject.Attribute("Id")?.Value, out int id);
                int.TryParse(reject.Attribute("Count")?.Value, out int count);

                ModifiedBinCount.Add(id, count);
            }

            XElement visionRejectsElement = summaryElement.Element("VisionRejects");

            foreach (XElement reject in visionRejectsElement.Elements())
            {
                int.TryParse(reject.Attribute("Id")?.Value, out int id);
                int.TryParse(reject.Attribute("Count")?.Value, out int count);

                VisionBinCount.Add(id, count);
            }
        }

        public void SaveToFile(string path)
        {
            FileInfo file = new FileInfo(path);
            file.Directory.Create();

            XmlWriterSettings xmlSettings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  "
            };

            using (XmlWriter writer = XmlWriter.Create(path, xmlSettings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("LotData");

                writer.WriteStartElement("Info");

                writer.WriteElementString("MachineId", MachineId);
                writer.WriteElementString("LotId", LotId);
                writer.WriteElementString("OperatorId", OperatorId);
                writer.WriteElementString("ProductCode", ProductCode);
                writer.WriteElementString("DieAttach", DieAttach);
                writer.WriteElementString("WireBond0", WireBond0);
                writer.WriteElementString("WireBond1", WireBond1);
                writer.WriteElementString("BondingDiagram", BondingDiagram);
                writer.WriteElementString("Recipe", RecipeName);
                writer.WriteElementString("RecipePath", RecipePath);
                writer.WriteElementString("LeadFrameUnits", LeadFrameUnits.ToString());
                writer.WriteElementString("LeadFrameXUnits", LeadFrameXUnits.ToString());
                writer.WriteElementString("LeadFrameYUnits", LeadFrameYUnits.ToString());
                writer.WriteElementString("StartTime", StartTime.ToString());
                writer.WriteElementString("EndTime", EndTime.ToString());

                writer.WriteEndElement();

                writer.WriteStartElement("Summary");

                writer.WriteElementString("UPH", UPH.ToString());
                writer.WriteElementString("ProcessUPH", ProcessUPH.ToString());
                writer.WriteElementString("LeadFrames", LeadFrames.ToString());
                writer.WriteElementString("LeadFramesInspected", LeadFramesInspected.ToString());
                writer.WriteElementString("UnitsPassed", UnitsPassed.ToString());
                writer.WriteElementString("UnitsRejected", UnitsRejected.ToString());
                writer.WriteElementString("UnitsOverRejected", UnitsOverRejected.ToString());
                writer.WriteElementString("UnitsYieldPercentage", UnitsYieldPercentage.ToString());
                writer.WriteElementString("OverRejectPercentage", OverRejectPercentage.ToString());
                writer.WriteElementString("MarkedUnits", MarkedUnits.ToString());
                writer.WriteElementString("UnmarkedUnits", UnmarkedUnits.ToString());
                writer.WriteElementString("MarkedUnitsPassed", MarkedUnitsPassed.ToString());
                writer.WriteElementString("MarkedUnitsRejected", MarkedUnitsRejected.ToString());
                writer.WriteElementString("MarkedUnitsYieldPercentage", MarkedUnitsYieldPercentage.ToString());

                writer.WriteStartElement("ModifiedRejects");

                foreach (KeyValuePair<int, int> reject in ModifiedBinCount)
                {
                    writer.WriteStartElement("Reject");
                    writer.WriteAttributeString("Id", reject.Key.ToString());
                    writer.WriteAttributeString("Count", reject.Value.ToString());
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();

                writer.WriteStartElement("VisionRejects");

                foreach (KeyValuePair<int, int> reject in VisionBinCount)
                {
                    writer.WriteStartElement("Reject");
                    writer.WriteAttributeString("Id", reject.Key.ToString());
                    writer.WriteAttributeString("Count", reject.Value.ToString());
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();

                writer.WriteEndElement();

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }

        public void GenerateSummary()
        {
            string[] leadFramePaths = Directory.GetFiles(FileInfo.Directory.FullName, "*.xml", SearchOption.AllDirectories);

            List<Die> visionDies = new List<Die>();
            List<Die> modifiedDies = new List<Die>();

            foreach (string lfPath in leadFramePaths)
            {
                LeadFrameMap visionLFMap = LeadFrameMap.Load(lfPath, LeadFrameMap.Type.Vision);
                visionDies.AddRange(visionLFMap.Dies);

                LeadFrameMap modifiedLFMap = LeadFrameMap.Load(lfPath, LeadFrameMap.Type.Modified);
                modifiedDies.AddRange(modifiedLFMap.Dies);
            }

            visionDies.RemoveAll(die => die.BinCode.Id == -1);
            modifiedDies.RemoveAll(die => die.BinCode.Id == -1);

            LeadFramesInspected = leadFramePaths.Length;
            UnitsPassed = modifiedDies.Count(die => die.BinCode.Id == 0);
            UnitsRejected = modifiedDies.Count - UnitsPassed;
            UnitsOverRejected = UnitsPassed - visionDies.Count(die => die.BinCode.Id == 0);
            UnitsYieldPercentage = (double)UnitsPassed / modifiedDies.Count * 100;
            OverRejectPercentage = (double)UnitsOverRejected / modifiedDies.Count * 100;

            MarkedUnits = modifiedDies.Count(die => die.MarkStatus != Die.Mark.NA);
            UnmarkedUnits = UnitsRejected - MarkedUnits;
            MarkedUnitsPassed = modifiedDies.Count(die => die.MarkStatus == Die.Mark.Pass);
            MarkedUnitsRejected = modifiedDies.Count(die => die.MarkStatus == Die.Mark.Fail);
            MarkedUnitsYieldPercentage = (double)MarkedUnitsPassed / MarkedUnits * 100;
            TimeSpan duration = EndTime.Subtract(StartTime);
            UPH = modifiedDies.Count / duration.TotalHours;

            ModifiedBinCount.Clear();

            foreach (Die modifiedDie in modifiedDies)
            {
                if (ModifiedBinCount.ContainsKey(modifiedDie.BinCode.Id))
                {
                    ModifiedBinCount[modifiedDie.BinCode.Id]++;
                }
                else
                {
                    ModifiedBinCount.Add(modifiedDie.BinCode.Id, 1);
                }
            }

            foreach (Die visionDie in visionDies)
            {
                if (VisionBinCount.ContainsKey(visionDie.BinCode.Id))
                {
                    VisionBinCount[visionDie.BinCode.Id]++;
                }
                else
                {
                    VisionBinCount.Add(visionDie.BinCode.Id, 1);
                }
            }
        }
    }
}
