using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using WindowsYaraService.Base.Common;

namespace WindowsYaraService.Modules.Network
{
    class HttpClientSingleton
    {
        public static HttpClient HttpClientInstance { get; set; }

        public static void InitialiseInstance()
        {
            //HttpClientHandler handler = new HttpClientHandler();
            //handler.ClientCertificateOptions = ClientCertificateOption.Manual;
            //handler.SslProtocols = SslProtocols.Tls12;
            //handler.ClientCertificates.Add(clientCert);
            HttpClientInstance = new HttpClient();
            HttpClientInstance.BaseAddress = new Uri("https://localhost:44311/");
        }
    }
}
