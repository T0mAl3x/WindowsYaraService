using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsYaraService.Base;
using WindowsYaraService.Base.Jobs;
using static WindowsYaraService.Base.Jobs.ScheduleJob;

namespace WindowsYaraService.Modules
{
    class Scheduler : IListener
    {
        private readonly ConcurrentDictionary<ScheduleJob, object> mSmallJobs = new ConcurrentDictionary<ScheduleJob, object>();
        private readonly SynchronizedCollection<ScheduleJob> mMediumJobs = new SynchronizedCollection<ScheduleJob>();
        private readonly SynchronizedCollection<ScheduleJob> mBigJobs = new SynchronizedCollection<ScheduleJob>();

        private int SmallCounter = 5;
        private int MediumCounter = 3;

        public void ScheduleJobForScannig(ScheduleJob job)
        {
            job.RegisterListener(this);
            job.ExecuteFileChecker();
        }

        public string FetchJobForScanning()
        {
            if (mSmallJobs.Count != 0)
            {
                if (SmallCounter != 0 || (mMediumJobs.Count == 0 && mBigJobs.Count == 0))
                {
                    var job = mSmallJobs.First();
                    object temp;
                    mSmallJobs.TryRemove(job.Key, out temp);
                    SmallCounter--;
                    if (SmallCounter < 0)
                        SmallCounter = 0;
                    return job.Key.mFilePath;
                }
            }
            if (mMediumJobs.Count != 0)
            {
                if (MediumCounter != 0 || mBigJobs.Count == 0)
                {
                    var job = mMediumJobs.First();
                    mMediumJobs.RemoveAt(0);
                    MediumCounter--;
                    if (MediumCounter < 0)
                        MediumCounter = 0;
                    return job.mFilePath;
                }
            }
            if (mBigJobs.Count != 0)
            {
                var job = mBigJobs.First();
                mBigJobs.RemoveAt(0);
                SmallCounter = 5;
                MediumCounter = 3;
                return job.mFilePath;
            }
            return null;
        }

        public void OnJobReady(ScheduleJob job)
        {
            switch (job.mPriority)
            {
                case ScheduleJob.Priorities.SMALL:
                    var existsSmall = mSmallJobs.Any(anyJob => anyJob.Key.mFilePath == job.mFilePath);
                    if (!existsSmall)
                    {
                        mSmallJobs.TryAdd(job, null);
                    }
                    break;
                case ScheduleJob.Priorities.MEDIUM:
                    var existsMedium = mMediumJobs.Any(anyJob => anyJob.mFilePath == job.mFilePath);
                    if (!existsMedium)
                    {
                        mMediumJobs.Add(job);
                    }
                    break;
                case ScheduleJob.Priorities.BIG:
                    var existsBig = mBigJobs.Any(anyJob => anyJob.mFilePath == job.mFilePath);
                    if (!existsBig)
                    {
                        mBigJobs.Add(job);
                    }
                    break;
            }

            FetchSignal.waitHandle.Set();
        }
    }
}
