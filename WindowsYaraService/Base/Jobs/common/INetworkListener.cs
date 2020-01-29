using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsYaraService.Modules.Network;

namespace WindowsYaraService.Base.Jobs.common
{
    public interface INetworkListener
    {
        void OnSuccess(object response);
        void OnFailure(INetJob netJob, string message);
    }
}
