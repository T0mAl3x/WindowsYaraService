using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsYaraService.Base
{
    abstract class BaseObservable<LISTENER_CLASS>
    {
        // thread-safe set of listeners
        protected readonly SynchronizedCollection<LISTENER_CLASS> mListeners = new SynchronizedCollection<LISTENER_CLASS>();

        public void RegisterListener(LISTENER_CLASS listener)
        {
            mListeners.Add(listener);
        }

        public void UnregisterListener(LISTENER_CLASS listener)
        {
            mListeners.Remove(listener);
        }

        protected SynchronizedCollection<LISTENER_CLASS> GetListeners()
        {
            return mListeners;
        }
    }
}
