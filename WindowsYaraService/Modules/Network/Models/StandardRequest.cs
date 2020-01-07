using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsYaraService.Base.Jobs;

namespace WindowsYaraService.Modules.Network.Models
{
    class StandardRequest
    {
        public JobType Type { get; set; }
        public object Content { get; set; }
    }
}
