using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WindowsYaraService.Modules.Network
{
    public class StandardResponse
    {
        public NetworkCodes Code { get; set; }
        public object Response { get; set; }
        public List<string> Errors { get; set; }
    }
}
