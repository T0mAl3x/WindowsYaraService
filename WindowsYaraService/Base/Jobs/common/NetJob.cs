using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WindowsYaraService.Modules.Network;

namespace WindowsYaraService.Base.Jobs.common
{
    abstract class NetJob : BaseObservable<NetJob.INetworkListener>
    {
        internal interface INetworkListener
        {
            void OnFailure(NetJob netJob);
        }

        protected object InfoToSend;
        
        public async Task ExecuteAsync()
        {
            try
            {
                var message = new HttpRequestMessage(HttpMethod.Post, "/test");
                message.Content = new StringContent(JsonConvert.SerializeObject(InfoToSend), Encoding.UTF8, "application/json");
                message.Headers.Add("Cookie", "cookie1=value1; cookie2=value2");
                var result = await HttpClientSingleton.HttpClientAuthenticated.SendAsync(message);
                result.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException e)
            {
                EventLog.WriteEntry("Application", e.Message, EventLogEntryType.Error);
                foreach (INetworkListener listener in GetListeners())
                {
                    listener.OnFailure(this);
                }
            }
        }

        protected abstract void HandleResponse(object response);
    }
}
