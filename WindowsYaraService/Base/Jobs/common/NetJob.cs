using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WindowsYaraService.Modules.Network;
using static WindowsYaraService.Base.Jobs.common.NetJob;

namespace WindowsYaraService.Base.Jobs.common
{
    public abstract class NetJob : BaseObservable<INetworkListener>
    {
        public interface INetworkListener
        {
            void OnSuccess(object response);
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
                HandleResponse(result);

                foreach (INetworkListener listener in GetListeners())
                {
                    listener.OnSuccess(this);
                }
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
