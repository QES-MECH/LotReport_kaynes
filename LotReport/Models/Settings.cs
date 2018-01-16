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

        public static string RejectCodesDirectory { get; set; }

        public static void LoadFromFile()
        {
            XDocument document = XDocument.Load(SettingsDirectory);

            DatabaseDirectory = document
                .Root
                .Element("General")
                .Element("Directories")
                .Element("Database")
                .Value;

            RejectCodesDirectory = document
                .Root
                .Element("General")
                .Element("Directories")
                .Element("RejectCodes")
                .Value;
        }

        public static void SaveToFile()
        {
            XmlWriterSettings xmlSettings = new XmlWriterSettings();
            xmlSettings.Indent = true;
            xmlSettings.IndentChars = "  ";

            using (XmlWriter writer = XmlWriter.Create(SettingsDirectory))
            {
                writer.WriteStartDocument();
                writer.WriteStartElement("General");

                writer.WriteStartElement("Directories");
                writer.WriteElementString("Database", DatabaseDirectory);
                writer.WriteElementString("RejectCodes", RejectCodesDirectory);
                writer.WriteEndElement();

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }
    }
}
