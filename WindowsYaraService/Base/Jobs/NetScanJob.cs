using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsYaraService.Base.Jobs.common;
using WindowsYaraService.Modules.Network.Models;
using WindowsYaraService.Modules.Scanner;

namespace WindowsYaraService.Base.Jobs
{
    class NetScanJob : NetJob
    {
        public NetScanJob(InfoModel infoModel)
        {
            InfoToSend = new StandardRequest
            {
                Type = JobType.REPORT,
                Content = infoModel
            };
        }

        protected override void HandleResponse(object response)
        {
            // Do Nothing
        }
    }
}
