using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Text.RegularExpressions;

namespace Metro_Skin_Installer
{
    class LocalData
    {
        public static List<Extra> globalExtras = new List<Extra>();

        public static void GetExtras()
        {
            string[] manifest = File.ReadAllLines(Path.GetTempPath() + "UPMetroSkin-installer\\manifest.txt");
            string parsingRegex = "\"(.*?)\";\"(.*?)\";\"(.*?)\";\"(.*?)\"";
            foreach (string Line in manifest)
            {
                List<string> LinkedExtrasIds_ = (from Match m in Regex.Matches((Regex.Match(Line.Replace("\\", ""), parsingRegex).Groups[3].Value), @"(.+?)(?:,|$)") select m.Groups[1].Value).ToList(); //Converts regex matches to list
                globalExtras.Add(new Extra
                {
                    Name = Regex.Match(Line.Replace("\\", ""), parsingRegex).Groups[1].Value,
                    ID = Regex.Match(Line.Replace("\\", ""), parsingRegex).Groups[2].Value,
                    Path = Regex.Match(Line.Replace("\\", ""), parsingRegex).Groups[2].Value,
                    linkedExtrasIds = LinkedExtrasIds_
                });
            }
        }
    }
}
