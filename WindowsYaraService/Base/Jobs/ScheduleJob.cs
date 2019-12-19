using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsYaraService.Base.Jobs
{
    class ScheduleJob : Job
    {
        private long mSize;

        public ScheduleJob(string filePath)
        {
            mFilePath = filePath;
            mSize = new FileInfo(filePath).Length;
        }
    }
}
