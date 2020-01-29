using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WindowsYaraService.Modules.Network.Models.Register;

namespace WindowsYaraService.Modules.Network.Models
{
    class RegisterResponse
    {
        public RegisterTypes Type { get; set; }
        public object RegisterObject { get; set; }
    }
}
