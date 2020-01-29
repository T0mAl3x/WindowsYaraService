using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsYaraService.Base.Jobs
{
    public abstract class InternalJob
    {
        public string mFilePath { get; }

        public InternalJob(string filePath)
        {
            mFilePath = filePath;
        }
    }
}
