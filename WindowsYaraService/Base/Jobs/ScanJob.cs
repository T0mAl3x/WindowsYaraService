using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsYaraService.Base.Jobs
{
    class ScanJob : Job
    {
        readonly long mSize;
        public ScanJob(string filePath) : base(filePath)
        {
            FileInfo file = new FileInfo(mFilePath);
            mSize = file.Length;
        }

        public long GetSize()
        {
            return mSize;
        }
    }
}
