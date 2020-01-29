using Microsoft.VisualBasic.Devices;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Management;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using WindowsYaraService.Base.Common;
using WindowsYaraService.Base.Jobs.common;
using WindowsYaraService.Modules.Network;
using WindowsYaraService.Modules.Network.Models;
using WindowsYaraService.Modules.Scanner;

namespace WindowsYaraService.Base.Jobs
{
    class EnrollmentJob : BaseObservable<INetworkListener>, INetJob
    {
        private readonly EnrollmentModel _registerModel;

        public EnrollmentJob()
        {
            ComputerInfo computerInfo = new ComputerInfo();
            string systemName = Environment.MachineName;
            string osName = computerInfo.OSFullName;
            string version = computerInfo.OSVersion;

            byte[] encGuid = FileHandler.ReadBytesToFile("GUID");
            string guid = YaraService._GUID;

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
                GUID = guid,
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
                var response = await HttpClientSingleton.HttpClientInstance.PostAsync("Agent/Enroll/", content);
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
            catch(HttpRequestException e)
            {
                foreach (var listener in GetListeners())
                {
                    listener.Key.OnFailure(this, e.Message);
                }
            }
        }
    }
}
