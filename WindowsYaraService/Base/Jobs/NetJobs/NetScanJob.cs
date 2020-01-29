using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using WindowsYaraService.Base.Common;
using WindowsYaraService.Base.Jobs.common;
using WindowsYaraService.Modules.Network;
using WindowsYaraService.Modules.Registrator;
using WindowsYaraService.Modules.Scanner;

namespace WindowsYaraService.Base.Jobs
{
    class NetScanJob : BaseObservable<INetworkListener>, INetJob
    {
        private readonly InfoModel _infoModel;
        private readonly string _token;
        public NetScanJob(InfoModel infoModel)
        {
            _infoModel = infoModel;
            byte[] encLocalToken = FileHandler.ReadBytesToFile("TOKEN");
            byte[] decLocalToken = DataProtection.Unprotect(encLocalToken);

            CertHandler certHandler = new CertHandler();
            X509Certificate2 primalCert = certHandler.FindCertificate(YaraService._GUID, StoreName.My, StoreLocation.LocalMachine);

            byte[] decToken = certHandler.DecryptDataOaepSha1(primalCert, decLocalToken);
            _token = Convert.ToBase64String(decToken);
        }

        public async Task ExecuteAsync()
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(_infoModel), Encoding.UTF8, "application/json");
                HttpClientSingleton.HttpClientInstance.DefaultRequestHeaders.Add("Authorization", _token);
                var response = await HttpClientSingleton.HttpClientInstance.PostAsync("Agent/Enroll/", content);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                StandardResponse standardResponse = JsonConvert.DeserializeObject<StandardResponse>(responseBody);

                foreach (var listener in GetListeners())
                {
                    listener.Key.OnSuccess(standardResponse);
                }
            }
            catch (HttpRequestException e)
            {
                foreach (var listener in GetListeners())
                {
                    listener.Key.OnFailure(this, e.Message);
                }
            }
        }
    }
}
