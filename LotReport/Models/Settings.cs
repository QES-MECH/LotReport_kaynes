using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace LotReport.Models
{
    public static class Settings
    {
        public const string SettingsDirectory = "Settings.xml";

        public static string DatabaseDirectory { get; set; }

        public static string BinCodeDirectory { get; set; }

        public static bool ShiftFilter { get; set; }

        public static DateTime DayShift { get; set; }

        public static DateTime NightShift { get; set; }

        public static bool CognexDisplay { get; set; }

        public static bool MarkGraphics { get; set; }

        public static string VisionImageDirectory { get; set; }

        public static void LoadFromFile()
        {
            XDocument document = XDocument.Load(SettingsDirectory);

            DatabaseDirectory = document
                .Root
                .Element("General")
                .Element("Directories")
                .Element("Database")
                .Value;

            BinCodeDirectory = document
                .Root
                .Element("General")
                .Element("Directories")
                .Element("BinCode")
                .Value;

            string shiftFilterStr = document
                .Root
                .Element("General")
                .Element("Shifts")
                .Element(nameof(ShiftFilter))
                .Value;
            if (bool.TryParse(shiftFilterStr, out bool shiftFilter))
            {
                ShiftFilter = shiftFilter;
            }

            string dayShiftStr = document
                .Root
                .Element("General")
                .Element("Shifts")
                .Element(nameof(DayShift))
                .Value;
            if (DateTime.TryParseExact(dayShiftStr, "HH:mm", null, System.Globalization.DateTimeStyles.NoCurrentDateDefault, out DateTime dayShift))
            {
                DayShift = dayShift;
            }

            string nightShiftStr = document
                .Root
                .Element("General")
                .Element("Shifts")
                .Element(nameof(NightShift))
                .Value;
            if (DateTime.TryParseExact(nightShiftStr, "HH:mm", null, System.Globalization.DateTimeStyles.NoCurrentDateDefault, out DateTime nightShift))
            {
                NightShift = nightShift;
            }

            string cognexDisplayStr = document
                .Root
                .Element("Advanced")
                .Element("Miscellaneous")
                .Element(nameof(CognexDisplay))
                .Value;
            if (bool.TryParse(cognexDisplayStr, out bool cognexDisplay))
            {
                CognexDisplay = cognexDisplay;
            }

            string markGraphicsStr = document
                .Root
                .Element("Advanced")
                .Element("Miscellaneous")
                .Element(nameof(MarkGraphics))
                .Value;
            if (bool.TryParse(markGraphicsStr, out bool markGraphics))
            {
                MarkGraphics = markGraphics;
            }

            VisionImageDirectory = document
                .Root
                .Element("Advanced")
                .Element("Miscellaneous")
                .Element(nameof(VisionImageDirectory))
                .Value;
        }

        public static void SaveToFile()
        {
            XmlWriterSettings xmlSettings = new XmlWriterSettings();
            xmlSettings.Indent = true;
            xmlSettings.IndentChars = "  ";

            using (XmlWriter writer = XmlWriter.Create(SettingsDirectory, xmlSettings))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("Settings");

                writer.WriteStartElement("General");

                writer.WriteStartElement("Directories");
                writer.WriteElementString("Database", DatabaseDirectory);
                writer.WriteElementString("BinCode", BinCodeDirectory);
                writer.WriteEndElement();

                writer.WriteStartElement("Shifts");
                writer.WriteElementString(nameof(ShiftFilter), ShiftFilter.ToString());
                writer.WriteElementString(nameof(DayShift), DayShift.ToString("HH:mm"));
                writer.WriteElementString(nameof(NightShift), NightShift.ToString("HH:mm"));
                writer.WriteEndElement();

                writer.WriteEndElement();

                writer.WriteStartElement("Advanced");

                writer.WriteStartElement("Miscellaneous");
                writer.WriteElementString(nameof(CognexDisplay), CognexDisplay.ToString());
                writer.WriteElementString(nameof(MarkGraphics), MarkGraphics.ToString());
                writer.WriteElementString(nameof(VisionImageDirectory), VisionImageDirectory);
                writer.WriteEndElement();

                writer.WriteEndElement();

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }
    }
}
