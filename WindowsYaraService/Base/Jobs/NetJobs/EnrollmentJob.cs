using Microsoft.VisualBasic.Devices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using WindowsYaraService.Base.Common;
using WindowsYaraService.Base.Jobs.common;
using WindowsYaraService.Modules.Network;
using WindowsYaraService.Modules.Scanner;
using static WindowsYaraService.Base.Jobs.EnrollmentJob;

namespace WindowsYaraService.Base.Jobs
{
    class EnrollmentJob : BaseObservable<IListener>, INetJob
    {
        public interface IListener
        {
            void OnEnrolled();
        }

        private readonly EnrollmentModel _registerModel;

        public EnrollmentJob()
        {
            ComputerInfo computerInfo = new ComputerInfo();
            string systemName = Environment.MachineName;
            string osName = computerInfo.OSFullName;
            string version = computerInfo.OSVersion;

            string macAddr = (
                            from nic in NetworkInterface.GetAllNetworkInterfaces()
                            where nic.OperationalStatus == OperationalStatus.Up
                            select nic.GetPhysicalAddress().ToString()
                          ).FirstOrDefault();
            string processor = "";
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_Processor");
            foreach (ManagementObject mo in searcher.Get())
            {
                processor = mo["Name"].ToString();
            }
            string motherboard = "";
            searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_BaseBoard");
            foreach (ManagementObject mo in searcher.Get())
            {
                motherboard = mo["Manufacturer"].ToString();
            }
            string ram = BytesConverter.ConvertToSize(computerInfo.TotalPhysicalMemory, BytesConverter.SizeUnits.GB);

            _registerModel = new EnrollmentModel
            {
                SystemName = systemName,
                OsName = osName,
                Version = version,
                MAC = macAddr,
                Processor = processor,
                MotherBoard = motherboard,
                RAM = ram
            };
        }

        public async Task ExecuteAsync()
        {
            try
            {
                var content = new StringContent(JsonConvert.SerializeObject(_registerModel), Encoding.UTF8, "application/json");
                var request = new HttpRequestMessage()
                {
                    RequestUri = new Uri("Agent/Enroll/", UriKind.Relative),
                    Method = HttpMethod.Post,
                    Content = content
                };
                CertHandler _certHandler = new CertHandler();
                X509Certificate2 clientCertificate = _certHandler.FindCertificate("YaraCA", StoreName.My, StoreLocation.LocalMachine);
                request.Headers.Add("X-ARR-ClientCert", clientCertificate.GetRawCertDataString());

                //var response = await HttpClientSingleton.HttpClientInstance.PostAsync("Agent/Enroll/", content);
                var response = await HttpClientSingleton.HttpClientInstance.SendAsync(request);
                response.EnsureSuccessStatusCode();

                foreach (var listener in GetListeners())
                {
                    listener.Key.OnEnrolled();
                }
            }
            catch(HttpRequestException ex)
            {
                
            }
            catch(Exception ex)
            {

            }
        }
    }
}
