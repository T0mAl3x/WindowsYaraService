using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WindowsYaraService.Base;
using WindowsYaraService.Base.Jobs.common;
using WindowsYaraService.Modules.Network;
using WindowsYaraService.Modules.Scanner;
using static WindowsYaraService.Modules.Networking;

namespace WindowsYaraService.Modules
{
    class Networking
    {
        public void ExecuteAsync(INetJob netJob)
        {
            ThreadPool.QueueUserWorkItem(async i =>
            {
                await netJob.ExecuteAsync();
            });
        }
    }
}
