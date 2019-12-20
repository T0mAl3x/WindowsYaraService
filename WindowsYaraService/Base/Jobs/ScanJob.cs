using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsYaraService.Base.Jobs
{
    class ScanJob : Job
    {
        public ScanJob(string filePath) : base(filePath)
        {

        }
    }
}
