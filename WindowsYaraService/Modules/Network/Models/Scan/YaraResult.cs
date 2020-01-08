using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsYaraService.Modules.Scanner.Models
{
    public class YaraResult
    {
        public Dictionary<string, object> Meta { get; set; }
        public string Identifier { get; set; }
    }
}
