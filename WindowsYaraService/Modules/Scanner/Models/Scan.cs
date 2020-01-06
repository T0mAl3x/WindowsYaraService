using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsYaraService.Modules.Scanner.Models
{
    public class Scan
    {
        string EngineName { get; set; }
        bool Detected { get; set; }
        string Version { get; set; }
        string Result { get; set; }
    }
}
