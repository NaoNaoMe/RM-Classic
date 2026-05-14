using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Xml.Serialization;

namespace rmApplication
{
    [Serializable()]
    public class DataSetting
    {
        [XmlElement("Group")]
        public string Group { set; get; }

        [XmlElement("Check")]
        public bool Check { set; get; }

        [XmlElement("Symbol")]
        public string Symbol { set; get; }

        [XmlElement("Address")]
        public string Address { set; get; }

        [XmlElement("Offset")]
        public string Offset { set; get; }

        [XmlElement("Size")]
        public string Size { set; get; }

        [XmlElement("Name")]
        public string Name { set; get; }

        [XmlElement("Type")]
        public string Type { set; get; }

        [XmlIgnore]
        public string ReadRaw { set; get; }

        [XmlIgnore]
        public string Read { set; get; }

        [XmlIgnore]
        public string WriteRaw { set; get; }

        [XmlElement("WriteValue")]
        public string Write { set; get; }

        [XmlElement("Description")]
        public string Description { set; get; }

        public DataSetting()
        {
            Group = null;
            Check = false;
            Symbol = null;
            Address = null;
            Offset = "0";
            Size = "1";
            Name = null;
            Type = UserType.Hex.ToString();
            ReadRaw = null;
            Read = null;
            WriteRaw = null;
            Write = null;
            Description = null;
        }

        public DataSetting(DataSetting ds)
        {
            Group = ds.Group;
            Check = ds.Check;
            Symbol = ds.Symbol;
            Address = ds.Address;
            Offset = ds.Offset;
            Size = ds.Size;
            Name = ds.Name;
            Type = ds.Type;
            ReadRaw = ds.ReadRaw;
            Read = ds.Read;
            WriteRaw = ds.WriteRaw;
            Write = ds.Write;
            Description = ds.Description;
        }

    }

    [Serializable()]
    [XmlRoot("ViewSetting")]
    public class ViewSetting
    {
        public enum DgvPropertyNames : int       // DataGridView property names
        {
            Group = 0,
            Check,
            Symbol,
            Address,
            Offset,
            Size,
            Name,
            Type,
            ReadRaw,
            Read,
            WriteRaw,
            Write,
            Description
        }

        [XmlElement("DataSetting")]
        public BindingList<DataSetting> Settings { get; set; }

        public ViewSetting()
        {
            Settings = new BindingList<DataSetting>();
        }

        public ViewSetting(ViewSetting vs)
        {
            Settings = new BindingList<DataSetting>(vs.Settings);
        }

        /// <summary>
        /// Splits a flat ViewSetting (containing all pages' DataSettings) into per-page ViewSettings,
        /// using changes in the Group property as page boundaries.
        /// Returns an empty list if the first row has a null Group (invalid format).
        /// </summary>
        /// <param name="source">Flat ViewSetting as deserialized directly from XML</param>
        /// <param name="pageNames">Group name of each page, for use in the page ComboBox</param>
        /// <returns>List of ViewSettings, one per page</returns>
        public static List<ViewSetting> SplitByGroup(ViewSetting source, out List<string> pageNames)
        {
            pageNames = new List<string>();
            var result = new List<ViewSetting>();

            if (source == null || source.Settings.Count == 0)
                return result;

            ViewSetting currentPage = null;
            string previousGroup = null;

            foreach (var setting in source.Settings)
            {
                bool isGroupBoundary = !string.IsNullOrEmpty(setting.Group)
                                    && setting.Group != previousGroup;

                if (currentPage == null)
                {
                    // First row: Group is required
                    if (setting.Group == null)
                        return new List<ViewSetting>(); // Invalid format

                    currentPage = new ViewSetting();
                    pageNames.Add(setting.Group);
                    previousGroup = setting.Group;
                }
                else if (isGroupBoundary)
                {
                    result.Add(currentPage);
                    currentPage = new ViewSetting();
                    pageNames.Add(setting.Group);
                    previousGroup = setting.Group;
                }

                currentPage.Settings.Add(setting);
            }

            if (currentPage != null && currentPage.Settings.Count > 0)
                result.Add(currentPage);

            return result;
        }
    }

}
