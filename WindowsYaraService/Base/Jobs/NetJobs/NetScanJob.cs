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
                var response = await HttpClientSingleton.HttpClientInstance.PostAsync("Agent/Enroll/", content);
                response.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException)
            {
                string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff",
                                            CultureInfo.InvariantCulture);
                string infoModelString = JsonConvert.SerializeObject(_infoModel);
                byte[] infoModelEnc = DataProtection.Protect(Encoding.UTF8.GetBytes(infoModelString));
                FileHandler.WriteBytesToFile(FileHandler.REPORT_ARCHIVE + timestamp, infoModelEnc);
            }
        }
    }
}
