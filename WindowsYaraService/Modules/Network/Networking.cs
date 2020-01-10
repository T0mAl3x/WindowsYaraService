using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using WindowsYaraService.Base.Jobs.common;

namespace WindowsYaraService.Modules
{
    class Networking : NetJob.INetworkListener
    {
        private SynchronizedCollection<NetJob> FailedJobs = new SynchronizedCollection<NetJob>();
        private Thread HandleFailedJobs;

        public Networking()
        {
            HandleFailedJobs = new Thread(new ThreadStart(() =>
            {
                while(true)
                {
                    if (FailedJobs.Count > 0)
                    {
                        NetJob job = FailedJobs.First();
                        ExecuteAsync(job);
                        FailedJobs.RemoveAt(0);
                    }
                }
            }));
            HandleFailedJobs.Start();
        }

        public void ExecuteAsync(NetJob netJob)
        {
            netJob.RegisterListener(this);
            ThreadPool.QueueUserWorkItem(async i =>
            {
                await netJob.ExecuteAsync();
            });
        }

        public void OnSuccess(object response)
        {
            // Do nothing for now
        }

        public void OnFailure(NetJob netJob)
        {
            FailedJobs.Add(netJob);
        }
    }
}
