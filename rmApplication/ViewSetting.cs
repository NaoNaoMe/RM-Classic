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

        [XmlElement("Size")]
        public string Size { set; get; }

        [XmlElement("Variable")]
        public string Variable { set; get; }

        [XmlElement("AddrLock")]
        public bool AddrLock { set; get; }

        [XmlElement("Address")]
        public string Address { set; get; }

        [XmlElement("Offset")]
        public string Offset { set; get; }

        [XmlElement("Name")]
        public string Name { set; get; }

        [XmlElement("Type")]
        public string Type { set; get; }

        [XmlIgnore]
        public string WriteText { set; get; }

        [XmlElement("WriteValue")]
        public string WriteValue { set; get; }

        public DataSetting()
        {
            Group = null;
            Check = false;
            Size = "1";
            Variable = null;
            AddrLock = false;
            Address = null;
            Offset = "0";
            Name = null;
            Type = "Hex";
            WriteText = null;
            WriteValue = null;
        }

        public DataSetting(DataSetting data)
        {
            Group = data.Group;
            Check = data.Check;
            Size = data.Size;
            Variable = data.Variable;
            AddrLock = data.AddrLock;
            Address = data.Address;
            Offset = data.Offset;
            Name = data.Name;
            Type = data.Type;
            WriteText = data.WriteText;
            WriteValue = data.WriteValue;

        }

    }

    [Serializable()]
    [XmlRoot("ViewSetting")]
    public class ViewSetting
    {
        [XmlElement("DataSetting")]
        public BindingList<DataSetting> DataList { get; set; }

        public ViewSetting()
        {
            DataList = new BindingList<DataSetting>();
        }

        public ViewSetting(ViewSetting data)
        {
            DataList = new BindingList<DataSetting>(data.DataList);

        }

    }

    class ViewSettingMisc
    {
        public static void replaceEmptyWithNull(ref ViewSetting data)
        {
            foreach (var item in data.DataList)
            {
                if (string.IsNullOrEmpty(item.Group))
                {
                    item.Group = null;
                }

                if (string.IsNullOrEmpty(item.Size))
                {
                    item.Size = "1";
                }

                if (string.IsNullOrEmpty(item.Variable))
                {
                    item.Variable = null;
                }

                if (string.IsNullOrEmpty(item.Address))
                {
                    item.Address = null;
                }

                if (string.IsNullOrEmpty(item.Offset))
                {
                    item.Address = "0";
                }

                if (string.IsNullOrEmpty(item.Name))
                {
                    item.Name = null;
                }

                if (string.IsNullOrEmpty(item.WriteValue))
                {
                    item.WriteValue = null;
                }

            }

        }
    }


}
