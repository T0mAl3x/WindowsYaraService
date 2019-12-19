using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsYaraService.Base.Jobs;

namespace WindowsYaraService.Base
{
    class JobFactory
    {
        public ScheduleJob GetScheduleJob(string filePath)
        {
            return new ScheduleJob(filePath);
        }
    }
}
