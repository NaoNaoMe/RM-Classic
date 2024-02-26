using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace rmApplication
{
    class ViewFileName
    {
        public string SettingName { get; set; }
        public string SoftwareVersion { get; set; }

        public ViewFileName()
        {
            SettingName = string.Empty;
            SoftwareVersion = string.Empty;
        }

        public ViewFileName(string name)
        {
            SettingName = "RMConfigurtation";
            SoftwareVersion = name;
        }

        private static string delimiter = "--";

        public static bool GetName(string fileName, out ViewFileName vfn)
        {
            vfn = new ViewFileName();

            if (fileName == null)
                return false;

            int foundIndex = fileName.IndexOf(delimiter);

            if (foundIndex == -1)
                return false;

            vfn.SettingName = fileName.Substring(0, foundIndex);

            int beginIndex = foundIndex + delimiter.Length;
            vfn.SoftwareVersion = fileName.Substring(beginIndex, (fileName.Length - beginIndex));

            return true;

        }

        public static string MakeFileName(ViewFileName vfn)
        {
            return vfn.SettingName + delimiter + vfn.SoftwareVersion;

        }


    }
}
