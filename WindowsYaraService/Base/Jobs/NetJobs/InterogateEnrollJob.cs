using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WindowsYaraService.Base.Jobs.common;
using WindowsYaraService.Modules.Network;
using WindowsYaraService.Modules.Network.Models;

namespace WindowsYaraService.Base.Jobs.NetJobs
{
    class InterogateEnrollJob : BaseObservable<INetworkListener>, INetJob
    {
        private readonly string _GUID;

        public InterogateEnrollJob(string GUID)
        {
            _GUID = GUID;
        }

        public async Task ExecuteAsync()
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(_GUID), Encoding.UTF8, "application/json");
                var response = await HttpClientSingleton.HttpClientInstance.PostAsync("Agent/IsReady/", content);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();
                StandardResponse standardResponse = JsonConvert.DeserializeObject<StandardResponse>(responseBody);
                string stringResponse = Newtonsoft.Json.JsonConvert.SerializeObject(standardResponse.Response);
                RegisterResponse registerModel = JsonConvert.DeserializeObject<RegisterResponse>(stringResponse);

                foreach (var listener in GetListeners())
                {
                    listener.Key.OnSuccess(registerModel);
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
