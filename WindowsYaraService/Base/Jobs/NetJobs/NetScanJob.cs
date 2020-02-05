using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using WindowsYaraService.Base.Common;
using WindowsYaraService.Base.Jobs.common;
using WindowsYaraService.Modules.Network;
using WindowsYaraService.Modules.Scanner;

namespace WindowsYaraService.Base.Jobs
{
    class NetScanJob : INetJob
    {
        private readonly InfoModel _infoModel;
        public NetScanJob(InfoModel infoModel)
        {
            _infoModel = infoModel;
        }

        public async Task ExecuteAsync()
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(_infoModel), Encoding.UTF8, "application/json");
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri("Agent/Report/", UriKind.Relative),
                    Method = HttpMethod.Post,
                    Content = content
                };
                CertHandler _certHandler = new CertHandler();
                X509Certificate2 clientCertificate = _certHandler.FindCertificate("YaraCA", StoreName.My, StoreLocation.LocalMachine);
                request.Headers.Add("X-ARR-ClientCert", clientCertificate.GetRawCertDataString());

                var response = await HttpClientSingleton.HttpClientInstance.SendAsync(request);
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException)
            {
                string timestamp = Guid.NewGuid().ToString();
                string infoModelString = JsonConvert.SerializeObject(_infoModel);
                byte[] infoModelEnc = DataProtection.Protect(Encoding.UTF8.GetBytes(infoModelString));
                FileHandler.WriteBytesToFile(FileHandler.REPORT_ARCHIVE + timestamp, infoModelEnc);
            }
        }
    }
}
