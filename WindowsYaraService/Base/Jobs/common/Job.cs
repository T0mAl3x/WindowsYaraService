using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsYaraService.Base.Jobs
{
    abstract class Job
    {
        public string mFilePath { get; }

        public Job(string filePath)
        {
            mFilePath = filePath;
        }
    }
}
