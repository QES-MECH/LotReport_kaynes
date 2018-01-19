using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Xml.Linq;

namespace LotReport.Models
{
    public class LeadFrameTable
    {
        private LeadFrameTable()
        {
            this.Rows = new List<DieRow>();
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

        public List<DieRow> Rows { get; private set; }

        public static LeadFrameTable Load(string xmlPath, Type type)
        {
            LeadFrameTable table = new LeadFrameTable();
            table.XmlPath = xmlPath;

            table.LoadFromFile(table.XmlPath, type);
            table.GetInfo(table.XmlPath);

            return table;
        }

        public static LeadFrameTable LoadTemplate(int x, int y)
        {
            LeadFrameTable table = new LeadFrameTable();
            table.SumOfXDies = x;
            table.SumOfYDies = y;

            table.GenerateRows(table.SumOfXDies, table.SumOfYDies);

            return table;
        }

        public bool SetDieRejectCode(Die die, RejectCode rejectCode)
        {
            XDocument doc = XDocument.Load(this.XmlPath);

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

            doc.Save(this.XmlPath);

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

                this.Rows.Add(new DieRow(dies));
            }
        }

        private void LoadFromFile(string xmlPath, Type type)
        {
            RejectCodeRepository repo = new RejectCodeRepository();
            repo.LoadFromFile();

            XDocument doc = XDocument.Load(xmlPath);
            string elementX = doc.Root.Attribute("X").Value;
            string elementY = doc.Root.Attribute("Y").Value;

            int sumOfXDies = 0;
            int sumOfYDies = 0;

            if (int.TryParse(elementX, out sumOfXDies))
            {
                this.SumOfXDies = sumOfXDies;
            }

            if (int.TryParse(elementY, out sumOfYDies))
            {
                this.SumOfYDies = sumOfYDies;
            }

            for (int y = 1; y <= sumOfYDies; y++)
            {
                List<Die> dies = new List<Die>();

                for (int x = 1; x <= sumOfXDies; x++)
                {
                    Die die = new Die();
                    die.Coordinate = new Point(x, y);

                    var dieElement = doc
                        .Element("DieData")
                        .Elements("Die")
                        .Where(e => e.Attribute("Coordinate").Value == die.Coordinate.ToString())
                        .FirstOrDefault();

                    string modifiedRejectCode = dieElement.Element("RejectCode").Element("Modified").Value;

                    if (type == Type.Machine)
                    {
                        string visionRejectCode = dieElement.Element("RejectCode").Element("Vision").Value;

                        int rejectCodeId;
                        if (int.TryParse(visionRejectCode, out rejectCodeId))
                        {
                            die.RejectCode.Id = rejectCodeId;
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
                        if (!string.IsNullOrEmpty(modifiedRejectCode))
                        {
                            int rejectCodeId;
                            if (int.TryParse(modifiedRejectCode, out rejectCodeId))
                            {
                                die.RejectCode.Id = rejectCodeId;
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
                        else
                        {
                            string visionRejectCode = dieElement.Element("RejectCode").Element("Vision").Value;

                            int rejectCodeId;
                            if (int.TryParse(visionRejectCode, out rejectCodeId))
                            {
                                die.RejectCode.Id = rejectCodeId;
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
                    }

                    die.ImagePath = dieElement.Element("ImagePath").Value;

                    this.TryGetRejectCodeInfo(repo.RejectCodes, die);

                    dies.Add(die);
                }

                this.Rows.Add(new DieRow(dies));
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
            this.LeadFrameId = Path.GetFileNameWithoutExtension(xmlPath);

            string magazinePath = Path.GetDirectoryName(xmlPath);
            this.MagazineId = Path.GetFileName(magazinePath);

            string lotPath = Path.GetDirectoryName(magazinePath);
            this.LotId = Path.GetFileName(lotPath);
        }
    }
}
