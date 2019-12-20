using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsYaraService.Base.Jobs.common
{
    interface ObservableJob<LISTENER_TYPE>
    {
        void RegisterListener(LISTENER_TYPE listener);
        void UnregisterListener(LISTENER_TYPE listener);
    }
}
