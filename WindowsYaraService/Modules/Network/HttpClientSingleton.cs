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
        public static readonly HttpClient HttpClientAuthenticated = new HttpClient();

        public HttpClientSingleton()
        {

        }
    }
}
