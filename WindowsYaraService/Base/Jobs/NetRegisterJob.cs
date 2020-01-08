using Microsoft.VisualBasic.Devices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using WindowsYaraService.Base.Jobs.common;
using WindowsYaraService.Modules.Network.Models;
using WindowsYaraService.Modules.Scanner;

namespace WindowsYaraService.Base.Jobs
{
    class NetRegisterJob : NetJob
    {
        public NetRegisterJob()
        {
            ComputerInfo computerInfo = new ComputerInfo();
            string systemName = Environment.MachineName;
            string osName = computerInfo.OSFullName;
            string version = computerInfo.OSVersion;
            string guid = Guid.NewGuid().ToString();
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

            var regisgerModel = new RegisterModel
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
            InfoToSend = regisgerModel;
        }
        protected override void HandleResponse(object response)
        {
            RegisterResponse registerResponse = response as RegisterResponse;
            byte[] token = DataProtection.Protect(Encoding.ASCII.GetBytes(registerResponse.Token));
            string fileName = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + "\\token.txt";
            System.IO.File.WriteAllBytes(fileName, token);
        }
    }
}
