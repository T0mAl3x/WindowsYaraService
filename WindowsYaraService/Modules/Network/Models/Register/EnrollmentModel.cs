using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsYaraService.Modules.Scanner
{
    class EnrollmentModel
    {
        public string SystemName { get; set; }
        public string OsName { get; set; }
        public string Version { get; set; }
        public string MAC { get; set; }
        public string Processor { get; set; }
        public string MotherBoard { get; set; }
        public string RAM { get; set; }
    }
}
