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
            SettingName = "SampleSetting";
            SoftwareVersion = string.Empty;
        }

        private static string delimiter = "--";

        public static bool GetNameFromOldFormat(string fileName, out ViewFileName vfn)
        {
            string TARGET_VER_TAG = "_TgV";
            string SETTING_VER_TAG = "_StV";

            vfn = new ViewFileName();

            if (fileName == null)
                return false;

            int firstCharacter = fileName.IndexOf(SETTING_VER_TAG);
            int secondCharacter = fileName.IndexOf(TARGET_VER_TAG);

            if (firstCharacter <= 0)
                return false;

            string text;
            int tmpIndex;
            int length;

            length = firstCharacter;
            text = fileName.Substring(0, length);
            vfn.SettingName = text;

            if ((secondCharacter > 0) &&
                (secondCharacter > firstCharacter))
            {
                tmpIndex = firstCharacter + 4;
                length = secondCharacter - tmpIndex;
                text = fileName.Substring(tmpIndex, length);
                vfn.SettingName += text;

                tmpIndex = secondCharacter + 4;
                length = fileName.Length - tmpIndex;
                text = fileName.Substring(tmpIndex, length);
                vfn.SoftwareVersion = text;

            }
            else
            {
                tmpIndex = firstCharacter + 4;
                length = fileName.Length - tmpIndex;
                text = fileName.Substring(tmpIndex, length);
                vfn.SettingName += text;

            }

            return true;

        }

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
