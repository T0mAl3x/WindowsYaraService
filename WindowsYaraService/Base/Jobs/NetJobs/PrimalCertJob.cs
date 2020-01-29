using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using WindowsYaraService.Base.Jobs.common;
using WindowsYaraService.Modules.Network;
using WindowsYaraService.Modules.Registrator;

namespace WindowsYaraService.Base.Jobs.NetJobs
{
    class PrimalCertJob : BaseObservable<INetworkListener>, INetJob
    {
        private readonly X509Certificate2 _tempCert;
        private readonly string _GUID;
        private readonly CertHandler _certHandler;

        public PrimalCertJob(X509Certificate2 tempCert, string GUID)
        {
            _tempCert = tempCert;
            _GUID = GUID;
        }

        public async Task ExecuteAsync()
        {
            try
            {
                byte[] certData = _tempCert.Export(X509ContentType.Cert);
                var byteContent = new ByteArrayContent(certData);

                var response = await HttpClientSingleton.HttpClientInstance.PostAsync("Agent/AcquirePersonalCertificate/", byteContent);
                response.EnsureSuccessStatusCode();
                byte[] responseBody = await response.Content.ReadAsByteArrayAsync();
                byte[] decPrimalCert = _certHandler.DecryptDataOaepSha1(_tempCert, responseBody);

                foreach (var listener in GetListeners())
                {
                    listener.Key.OnSuccess(decPrimalCert);
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
