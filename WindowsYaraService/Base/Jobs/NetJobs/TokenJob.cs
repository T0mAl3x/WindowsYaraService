using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WindowsYaraService.Base.Jobs.common;
using WindowsYaraService.Modules.Network;

namespace WindowsYaraService.Base.Jobs.NetJobs
{
    class TokenJob : BaseObservable<INetworkListener>, INetJob
    {
        private readonly string _GUID;

        public TokenJob(string GUID)
        {
            _GUID = GUID;
        }

        public async Task ExecuteAsync()
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(_GUID), Encoding.UTF8, "application/json");
                var response = await HttpClientSingleton.HttpClientInstance.PostAsync("Agent/AcquireToken/", content);
                response.EnsureSuccessStatusCode();
                byte[] responseBody = await response.Content.ReadAsByteArrayAsync();

                foreach (var listener in GetListeners())
                {
                    listener.Key.OnSuccess(responseBody);
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
