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
        private static Brush _red = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FF3333"));
        private static Brush _green = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#33FF33"));
        private static Brush _yellow = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#FFEA00"));

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

        public string LeadFrameId { get; private set; }

        public int SumOfXDies { get; private set; }

        public int SumOfYDies { get; private set; }

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

            if (die.BinCode.Id == 0)
            {
                die.Color = _yellow;
            }
            else
            {
                die.Color = _red;
            }

            return true;
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
                        Color = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#212121"))
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
            string elementX = doc.Root.Attribute("X").Value;
            string elementY = doc.Root.Attribute("Y").Value;

            if (int.TryParse(elementX, out int sumOfXDies))
            {
                SumOfXDies = sumOfXDies;
            }

            if (int.TryParse(elementY, out int sumOfYDies))
            {
                SumOfYDies = sumOfYDies;
            }

            for (int y = 1; y <= sumOfYDies; y++)
            {
                List<Die> dies = new List<Die>();

                for (int x = 1; x <= sumOfXDies; x++)
                {
                    Die die = new Die
                    {
                        Coordinate = new Point(x, y)
                    };

                    var dieElement = doc
                        .Root
                        .Elements("Die")
                        .Where(e => e.Attribute("Coordinate").Value == die.Coordinate.ToString())
                        .FirstOrDefault();

                    XElement visionBinCode = dieElement.Element("BinCode").Element("Vision");

                    if (type == Type.Vision)
                    {
                        if (int.TryParse(visionBinCode.Value, out int visionBinCodeId))
                        {
                            die.BinCode.Id = visionBinCodeId;
                        }
                        else
                        {
                            die.BinCode.Id = 999;
                        }

                        if (die.BinCode.Id == 0)
                        {
                            die.Color = _green;
                        }
                        else
                        {
                            die.Color = _red;
                        }
                    }

                    if (type == Type.Modified)
                    {
                        XElement modifiedBinCode = dieElement.Element("BinCode").Element("Modified");

                        if (modifiedBinCode == null)
                        {
                            if (int.TryParse(visionBinCode.Value, out int visionBinCodeId))
                            {
                                die.BinCode.Id = visionBinCodeId;
                            }
                            else
                            {
                                die.BinCode.Id = 999;
                            }

                            if (die.BinCode.Id == 0)
                            {
                                die.Color = _green;
                            }
                            else
                            {
                                die.Color = _red;
                            }
                        }
                        else
                        {
                            if (int.TryParse(modifiedBinCode.Value, out int modifiedBinCodeId))
                            {
                                die.BinCode.Id = modifiedBinCodeId;
                            }
                            else
                            {
                                die.BinCode.Id = 999;
                            }

                            if (die.BinCode.Id == 0)
                            {
                                die.Color = _yellow;
                            }
                            else
                            {
                                die.Color = _red;
                            }
                        }
                    }

                    die.DiePath = dieElement.Element("DiePath")?.Value;

                    if (Enum.TryParse(dieElement.Element("Mark")?.Value, out Die.Mark markStatus))
                    {
                        die.MarkStatus = markStatus;
                    }

                    die.MarkPath = dieElement.Element("MarkPath")?.Value;

                    TryGetBinCodeInfo(repo.BinCodes, die);

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
                die.BinCode.Value = sourceBinCode.Value;
                die.BinCode.Description = sourceBinCode.Description;
                die.BinCode.Mark = sourceBinCode.Mark;
            }
        }

        private void GetInfo(string xmlPath)
        {
            LeadFrameId = Path.GetFileNameWithoutExtension(xmlPath);

            string magazinePath = Path.GetDirectoryName(xmlPath);
            MagazineId = Path.GetFileName(magazinePath);

            string lotPath = Path.GetDirectoryName(magazinePath);
            LotId = Path.GetFileName(lotPath);
        }
    }
}
