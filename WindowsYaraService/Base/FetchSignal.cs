using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WindowsYaraService.Base
{
    class FetchSignal
    {
        public static AutoResetEvent waitHandle = new AutoResetEvent(false);
        public static AutoResetEvent waitForRegistration = new AutoResetEvent(false);
    }
}
