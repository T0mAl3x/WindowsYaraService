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
    class Networking : BaseObservable<IListener>, INetworkListener
    {
        public interface IListener
        {
            void OnAuthFailed(InfoModel infoModel);
        }

        private SynchronizedCollection<INetJob> FailedJobs = new SynchronizedCollection<INetJob>();
        private Thread HandleFailedJobs;

        public Networking()
        {
            HandleFailedJobs = new Thread(new ThreadStart(() =>
            {
                while(true)
                {
                    if (FailedJobs.Count > 0)
                    {
                        INetJob job = FailedJobs.First();
                        ExecuteAsync(job);
                        FailedJobs.RemoveAt(0);
                    }
                }
            }));
            HandleFailedJobs.Start();
        }

        public void ExecuteAsync(INetJob netJob)
        {
            netJob.RegisterListener(this);
            ThreadPool.QueueUserWorkItem(async i =>
            {
                await netJob.ExecuteAsync();
            });
        }

        public void OnSuccess(object response)
        {
            try
            {
                StandardResponse standardResponse = response as StandardResponse;
                if (standardResponse.Code == NetworkCodes.NOT_AUTH)
                {
                    InfoModel infoModel = standardResponse.Response as InfoModel;
                    foreach (var listener in GetListeners())
                    {
                        listener.Key.OnAuthFailed(infoModel);
                    }
                }
            }
            catch(Exception ex)
            {

            }
        }

        public void OnFailure(INetJob netJob, string errorMessage)
        {
            FailedJobs.Add(netJob);
        }
    }
}
