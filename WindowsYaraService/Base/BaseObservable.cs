using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsYaraService.Base
{
    public abstract class BaseObservable<LISTENER_CLASS>
    {
        // thread-safe set of listeners
        internal readonly ConcurrentDictionary<LISTENER_CLASS, object> mListeners = new ConcurrentDictionary<LISTENER_CLASS, object>();

        public void RegisterListener(LISTENER_CLASS listener)
        {
            mListeners.TryAdd(listener, null);
        }

        public void UnregisterListener(LISTENER_CLASS listener)
        {
            object temp;
            mListeners.TryRemove(listener, out temp);
        }

        internal ConcurrentDictionary<LISTENER_CLASS, object> GetListeners()
        {
            return mListeners;
        }
    }
}
