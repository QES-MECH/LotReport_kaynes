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
        private LeadFrameMap()
        {
        }

        public enum Type
        {
            Machine,
            Operator
        }

        public string XmlPath { get; private set; }

        public string LotId { get; private set; }

        public string MagazineId { get; private set; }

        public string LeadFrameId { get; private set; }

        public int SumOfXDies { get; private set; }

        public int SumOfYDies { get; private set; }

        public List<DieRow> Rows { get; } = new List<DieRow>();

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

        public bool SetDieRejectCode(Die die, RejectCode rejectCode)
        {
            XDocument doc = XDocument.Load(XmlPath);

            var dieElement = doc
                .Element("DieData")
                .Elements("Die")
                .Where(e => e.Attribute("Coordinate").Value == die.Coordinate.ToString())
                .FirstOrDefault();

            if (dieElement == null)
            {
                return false;
            }

            dieElement.Element("RejectCode").Element("Modified").Value = rejectCode.Id.ToString();

            doc.Save(XmlPath);

            die.RejectCode = rejectCode;

            if (die.RejectCode.Id == 0)
            {
                die.Color = Brushes.Yellow;
            }
            else
            {
                die.Color = Brushes.Red;
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
            RejectCodeRepository repo = new RejectCodeRepository();
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
                        .Element("DieData")
                        .Elements("Die")
                        .Where(e => e.Attribute("Coordinate").Value == die.Coordinate.ToString())
                        .FirstOrDefault();

                    XElement visionRejectCode = dieElement.Element("RejectCode").Element("Vision");

                    if (type == Type.Machine)
                    {
                        if (int.TryParse(visionRejectCode.Value, out int visionRejectCodeId))
                        {
                            die.RejectCode.Id = visionRejectCodeId;
                        }
                        else
                        {
                            die.RejectCode.Id = 999;
                        }

                        if (die.RejectCode.Id == 0)
                        {
                            die.Color = Brushes.Green;
                        }
                        else
                        {
                            die.Color = Brushes.Red;
                        }
                    }

                    if (type == Type.Operator)
                    {
                        XElement modifiedRejectCode = dieElement.Element("RejectCode").Element("Modified");

                        if (modifiedRejectCode.IsEmpty)
                        {
                            if (int.TryParse(visionRejectCode.Value, out int visionRejectCodeId))
                            {
                                die.RejectCode.Id = visionRejectCodeId;
                            }
                            else
                            {
                                die.RejectCode.Id = 999;
                            }

                            if (die.RejectCode.Id == 0)
                            {
                                die.Color = Brushes.Green;
                            }
                            else
                            {
                                die.Color = Brushes.Red;
                            }
                        }
                        else
                        {
                            if (int.TryParse(modifiedRejectCode.Value, out int modifiedRejectCodeId))
                            {
                                die.RejectCode.Id = modifiedRejectCodeId;
                            }
                            else
                            {
                                die.RejectCode.Id = 999;
                            }

                            if (die.RejectCode.Id == 0)
                            {
                                die.Color = Brushes.Yellow;
                            }
                            else
                            {
                                die.Color = Brushes.Red;
                            }
                        }
                    }

                    die.ImagePath = dieElement.Element("ImagePath").Value;

                    TryGetRejectCodeInfo(repo.RejectCodes, die);

                    dies.Add(die);
                }

                Rows.Add(new DieRow(dies));
            }
        }

        private void TryGetRejectCodeInfo(List<RejectCode> rejectCodes, Die die)
        {
            RejectCode sourceRejectCode = rejectCodes.FirstOrDefault(rc => rc.Id == die.RejectCode.Id);

            if (sourceRejectCode != null)
            {
                die.RejectCode.Value = sourceRejectCode.Value;
                die.RejectCode.Description = sourceRejectCode.Description;
                die.RejectCode.Mark = sourceRejectCode.Mark;
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
