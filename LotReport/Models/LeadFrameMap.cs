using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;

namespace LotReport.Models
{
    public class LeadFrameMap
    {
        private static Color _black = (Color)ColorConverter.ConvertFromString("#000000");
        private static Color _red = (Color)ColorConverter.ConvertFromString("#FF3333");
        private static Color _green = (Color)ColorConverter.ConvertFromString("#33FF33");
        private static Color _yellow = (Color)ColorConverter.ConvertFromString("#FFEA00");

        private LeadFrameMap()
        {
        }

        public enum Type
        {
            Vision,
            Modified
        }

        public string XmlPath { get; private set; }

        public string LotId { get; private set; }

        public string MagazineId { get; private set; }

        public string PreviousMagazineId { get; set; }

        public string LeadFrameId { get; private set; }

        public int SumOfXDies { get; private set; }

        public int SumOfYDies { get; private set; }

        public Origin MapOrigin { get; private set; }

        public List<DieRow> Rows { get; } = new List<DieRow>();

        public List<Die> Dies { get; private set; } = new List<Die>();

        public static LeadFrameMap Load(string xmlPath, Type type)
        {
            LeadFrameMap table = new LeadFrameMap
            {
                XmlPath = xmlPath
            };

            table.LoadFromFile(table.XmlPath, type);
            table.GetInfo(table.XmlPath);

            return table;
        }

        public static LeadFrameMap LoadTemplate(int x, int y)
        {
            LeadFrameMap table = new LeadFrameMap
            {
                SumOfXDies = x,
                SumOfYDies = y
            };

            table.GenerateRows(table.SumOfXDies, table.SumOfYDies);

            return table;
        }

        public static void SetMagazineId(string xmlPath, string magazineId)
        {
            XDocument document = XDocument.Load(xmlPath);
            var previousMagazineId = document.Root.Attribute("MagazineId")?.Value;
            document.Root.SetAttributeValue("PreviousMagazineId", previousMagazineId);
            document.Root.SetAttributeValue("MagazineId", magazineId);
            document.Save(xmlPath);
        }

        public static bool SetMarkStatus(string xmlPath, Point coordinate, Die.Mark markStatus)
        {
            XDocument document = XDocument.Load(xmlPath);

            var dieElement = document
                .Root
                .Elements("Die")
                .Where(e => e.Attribute("Coordinate").Value == coordinate.ToString())
                .FirstOrDefault();

            if (dieElement == null)
            {
                return false;
            }

            var markElement = dieElement.Elements().SingleOrDefault(e => e.Name == "Mark");

            if (markElement == null)
            {
                markElement = new XElement("Mark", markStatus);
                dieElement.Add(markElement);
            }
            else
            {
                markElement.Value = markStatus.ToString();
            }

            document.Save(xmlPath);

            return true;
        }

        public static bool SetMarkPath(string xmlPath, Point coordinate, string markPath)
        {
            XDocument document = XDocument.Load(xmlPath);

            var dieElement = document
                .Root
                .Elements("Die")
                .Where(e => e.Attribute("Coordinate").Value == coordinate.ToString())
                .FirstOrDefault();

            if (dieElement == null)
            {
                return false;
            }

            var markPathElement = dieElement.Elements().SingleOrDefault(e => e.Name == "MarkPath");

            if (markPathElement == null)
            {
                markPathElement = new XElement("MarkPath", markPath);
                dieElement.Add(markPathElement);
            }
            else
            {
                markPathElement.Value = markPath;
            }

            document.Save(xmlPath);

            return true;
        }

        public static void ClearMarkStatus(string xmlPath)
        {
            XDocument document = XDocument.Load(xmlPath);

            string elementX = document.Root.Attribute("X").Value;
            string elementY = document.Root.Attribute("Y").Value;
            int.TryParse(elementX, out int sumOfXDies);
            int.TryParse(elementY, out int sumOfYDies);

            for (int x = 1; x <= sumOfXDies; x++)
            {
                for (int y = 1; y <= sumOfYDies; y++)
                {
                    Point coordinate = new Point(x, y);
                    var dieElement = document
                        .Root
                        .Elements("Die")
                        .Where(e => e.Attribute("Coordinate").Value == coordinate.ToString())
                        .FirstOrDefault();

                    if (dieElement != null)
                    {
                        var markElement = dieElement.Elements().SingleOrDefault(e => e.Name == "Mark");

                        if (markElement != null)
                        {
                            markElement.Value = Die.Mark.NA.ToString();
                        }
                    }
                }
            }

            document.Save(xmlPath);
        }

        public bool SetDieBinCode(Die die, BinCode binCode)
        {
            XDocument doc = XDocument.Load(XmlPath);

            var dieElement = doc
                .Root
                .Elements("Die")
                .Where(e => e.Attribute("Coordinate").Value == die.Coordinate.ToString())
                .FirstOrDefault();

            if (dieElement == null)
            {
                return false;
            }

            var modifiedElement = dieElement.Element("BinCode").Elements().SingleOrDefault(e => e.Name == "Modified");

            if (modifiedElement == null)
            {
                modifiedElement = new XElement("Modified", binCode.Id.ToString());
                dieElement.Element("BinCode").Add(modifiedElement);
            }
            else
            {
                modifiedElement.Value = binCode.Id.ToString();
            }

            doc.Save(XmlPath);

            die.BinCode = binCode;

            if (die.BinCode.Quality == BinQuality.Pass)
            {
                die.Color = _yellow;
            }
            else
            {
                die.Color = _red;
            }

            return true;
        }

        public double CalculateRejectionPercentage()
        {
            double rejectedDieCount = Dies.Count(d => d.BinCode.Quality != BinQuality.Pass);
            return rejectedDieCount / Dies.Count * 100d;
        }

        private void GenerateRows(int sumOfXDies, int sumOfYDies)
        {
            for (int y = 1; y <= sumOfYDies; y++)
            {
                List<Die> dies = new List<Die>();

                for (int x = 1; x <= sumOfXDies; x++)
                {
                    Die die = new Die()
                    {
                        Coordinate = new Point(x, y),
                        Color = (Color)ColorConverter.ConvertFromString("#212121")
                    };

                    dies.Add(die);
                }

                Rows.Add(new DieRow(dies));
            }
        }

        private void LoadFromFile(string xmlPath, Type type)
        {
            BinCodeRepository repo = new BinCodeRepository();
            repo.LoadFromFile();

            XDocument doc = XDocument.Load(xmlPath);
            PreviousMagazineId = doc.Root.Attribute("PreviousMagazineId")?.Value;
            MagazineId = doc.Root.Attribute("Id")?.Value;
            string elementX = doc.Root.Attribute("X").Value;
            string elementY = doc.Root.Attribute("Y").Value;
            string elementMapOrigin = doc.Root.Attribute("MapOrigin")?.Value;

            if (int.TryParse(elementX, out int sumOfXDies))
            {
                SumOfXDies = sumOfXDies;
            }

            if (int.TryParse(elementY, out int sumOfYDies))
            {
                SumOfYDies = sumOfYDies;
            }

            if (Enum.TryParse(elementMapOrigin, out Origin mapOrigin))
            {
                MapOrigin = mapOrigin;
            }

            // Convert XML elements into a dictionary for quick lookups
            var dieElements = doc.Root.Elements("Die")
                .ToDictionary(e => e.Attribute("Coordinate").Value, e => e);

            for (int y = 1; y <= sumOfYDies; y++)
            {
                List<Die> dies = new List<Die>();

                for (int x = 1; x <= sumOfXDies; x++)
                {
                    string coordinateKey = $"{x},{y}";

                    if (!dieElements.TryGetValue(coordinateKey, out XElement dieElement))
                    {
                        continue;
                    }

                    Die die = new Die
                    {
                        Coordinate = new Point(x, y),
                        DiePath = new List<string>(),
                    };

                    if (type == Type.Modified && dieElement.Element("Modified") != null)
                    {
                        die.Modified = true;
                        die.BinCode.Id = int.TryParse(dieElement.Element("Modified").Value, out int modifiedBinCodeId)
                            ? modifiedBinCodeId
                            : 999;
                    }
                    else
                    {
                        die.BinCode.Id = int.TryParse(dieElement.Element("BinCode")?.Value, out int visionBinCodeId)
                            ? visionBinCodeId
                            : 999;
                    }

                    var element2D = doc.Descendants("Inspection").FirstOrDefault(t => (string)t.Attribute("Type") == "2D");
                    var element3D = doc.Descendants("Inspection").FirstOrDefault(t => (string)t.Attribute("Type") == "3D");

                    XElement bincode2D = element2D?.Element("BinCode");
                    if (int.TryParse(bincode2D.Value, out int bincode2DId))
                    {
                        die.BinCode2D.Id = bincode2DId;
                    }
                    else
                    {
                        die.BinCode2D.Id = 999;
                    }

                    var imageCount2D = element2D?.Elements().Count(c => c.Name.LocalName.StartsWith("ImagePath_"));
                    if (imageCount2D >= 1)
                    {
                        for (int i = 1; i <= imageCount2D; i++)
                        {
                            die.DiePath.Add(element2D?.Element($"ImagePath_{i}")?.Value);
                        }
                    }

                    XElement bincode3D = element3D?.Element("BinCode");
                    if (int.TryParse(bincode3D?.Value, out int bincode3DId))
                    {
                        die.BinCode3D.Id = bincode3DId;
                    }
                    else
                    {
                        die.BinCode3D.Id = 999;
                    }

                    var sides = new[] { "Left", "Right", "Back", "Front" };
                    foreach (var side in sides)
                    {
                        var count = element3D?.Elements().Count(c => c.Name.LocalName.StartsWith($"ImagePath_{side}_")) ?? 0;
                        for (int i = 1; i <= count; i++)
                        {
                            var path = element3D?.Element($"ImagePath_{side}_{i}")?.Value;
                            if (!string.IsNullOrEmpty(path))
                            {
                                die.DiePath3D[side].Add(path);
                            }
                        }
                    }

                    TryGetBinCodeInfo(repo.BinCodes, die);

                    switch (die.BinCode.Quality)
                    {
                        case BinQuality.Unknown:
                            die.Color = _black;
                            break;
                        case BinQuality.Pass:
                            die.Color = die.Modified ? _yellow : _green;
                            break;
                        default:
                            die.Color = _red;
                            break;
                    }

                    dies.Add(die);
                    Dies.Add(die);
                }

                Rows.Add(new DieRow(dies));
            }
        }

        private void TryGetBinCodeInfo(List<BinCode> binCodes, Die die)
        {
            BinCode sourceBinCode = binCodes.FirstOrDefault(bin => bin.Id == die.BinCode.Id);
            if (sourceBinCode != null)
            {
                CopyBinCodeInfo(sourceBinCode, die.BinCode);
            }

            BinCode sourceBinCode2D = binCodes.FirstOrDefault(bin => bin.Id == die.BinCode2D.Id);
            if (sourceBinCode2D != null)
            {
                CopyBinCodeInfo(sourceBinCode2D, die.BinCode2D);
            }

            BinCode sourceBinCode3D = binCodes.FirstOrDefault(bin => bin.Id == die.BinCode3D.Id);
            if (sourceBinCode3D != null)
            {
                CopyBinCodeInfo(sourceBinCode3D, die.BinCode3D);
            }
        }

        private void CopyBinCodeInfo(BinCode source, BinCode target)
        {
            target.Quality = source.Quality;
            target.Value = source.Value;
            target.Display = source.Display;
            target.Description = source.Description;
            target.Mark = source.Mark;
            target.SkipReview = source.SkipReview;
        }

        private void GetInfo(string xmlPath)
        {
            LeadFrameId = Path.GetFileNameWithoutExtension(xmlPath);

            string lotPath = Path.GetDirectoryName(xmlPath);
            LotId = Path.GetFileName(lotPath);
        }
    }
}
