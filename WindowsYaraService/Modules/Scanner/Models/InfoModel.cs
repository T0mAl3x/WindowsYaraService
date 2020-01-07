using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsYaraService.Modules.Scanner.Models;

namespace WindowsYaraService.Modules.Scanner
{
    public class InfoModel
    {
        public string ScandId { get; set; }
        public DateTime Date { get; set; }
        public string SHA1 { get; set; }
        public string SHA256 { get; set; }
        public string FilePath { get; set; }
        public string TerminalId { get; set; }
        public int Positives { get; set; }
        public int Total { get; set; }
        public List<Scan> Scans { get; set; }
        public List<YaraResult> YaraResults { get; set; }
        public List<Message> Messages = new List<Message>();
    }
}
