using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsYaraService.Base.Common
{
    static public class FileHandler
    {
        private static readonly string DATA_PATH = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData) + "\\YaraAgent\\";

        public static readonly string REPORT_ARCHIVE = DATA_PATH + "ReportArchive\\";
        public static void WriteBytesToFile(string fileName, byte[] bytes)
        {
            try
            {
                System.IO.File.WriteAllBytes(DATA_PATH + fileName, bytes);
            }
            catch(Exception ex)
            {

            }
        }

        public static void WriteTextToFile(string fileName, string text)
        {
            try
            {
                System.IO.File.WriteAllText(DATA_PATH + fileName, text);
            }
            catch (Exception ex)
            {

            }
        }

        public static byte[] ReadBytesToFile(string fileName)
        {
            try
            {
                byte[] bytes = System.IO.File.ReadAllBytes(DATA_PATH + fileName);
                return bytes;
            }
            catch(Exception ex)
            {
                return null;
            }
        }
    }
}
