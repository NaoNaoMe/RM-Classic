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

        [XmlElement("Variable")]
        public string Variable { set; get; }

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
            Variable = null;
            Address = null;
            Offset = "0";
            Size = "1";
            Name = null;
            Type = "Hex";
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
            Variable = ds.Variable;
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
            Variable,
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

    }

}
