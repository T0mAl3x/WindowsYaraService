using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsYaraService.Base.Common
{
    static public class FileHandler
    {
        private static readonly string AGENT_PATH = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\YaraAgent\\";
        public static readonly string REPORT_ARCHIVE = AGENT_PATH + "ReportArchive\\";
        public static readonly string RULES_PATH = AGENT_PATH + "YaraRules\\";

        public static void WriteBytesToFile(string fileName, byte[] bytes)
        {
            try
            {
                File.WriteAllBytes(fileName, bytes);
            }
            catch (Exception)
            {

            }
        }

        public static byte[] ReadTextFromFile(string fileName)
        {
            try
            {
                byte[] content = File.ReadAllBytes(fileName);
                return content;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
