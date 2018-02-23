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
                writer.WriteEndElement();

                writer.WriteEndElement();
                writer.WriteEndDocument();
            }
        }
    }
}
