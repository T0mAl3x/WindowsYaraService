using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsYaraService.Base.Jobs.common;

namespace WindowsYaraService.Base.Jobs
{
    public abstract class BaseObservableJob<LISTENER_TYPE> : InternalJob, ObservableJob<LISTENER_TYPE>
    {
        // thread-safe set of listeners
        protected readonly SynchronizedCollection<LISTENER_TYPE> mListeners = new SynchronizedCollection<LISTENER_TYPE>();

        public BaseObservableJob(string filePath) : base(filePath)
        {

        }

        public void RegisterListener(LISTENER_TYPE listener)
        {
            mListeners.Add(listener);
        }

        public void UnregisterListener(LISTENER_TYPE listener)
        {
            mListeners.Remove(listener);
        }

        protected SynchronizedCollection<LISTENER_TYPE> GetListeners()
        {
            return mListeners;
        }
    }
}
