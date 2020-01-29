using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace WindowsYaraService.Modules.Network
{
    class HttpClientSingleton
    {
        public static readonly HttpClient HttpClientInstance = new HttpClient()
        {
            BaseAddress = new Uri("https://localhost:44335/")
        };
    }
}
