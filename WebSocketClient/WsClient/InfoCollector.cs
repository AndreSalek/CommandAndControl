using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using WsClient.Models;

namespace WsClient
{
    internal class InfoCollector
    {
        public static ClientHwInfo GetHwInfo()
        {
            ClientHwInfo hwInfo = new ClientHwInfo()
            {
                MAC = GetMAC(),
                OS = GetOS(),
                CpuId = GetCpuId(),
                RAMCapacity = GetRAMCapacity(),
            };

            return hwInfo;
        }
        private static string GetWmicValue(string winClass, string property)
        {
            ManagementClass managClass = new ManagementClass(winClass);
            ManagementObjectCollection managCollec = managClass.GetInstances();
            string prop = "";
            foreach (ManagementObject managObj in managCollec)
            {
                prop = managObj.Properties[property].Value.ToString();
            }
            return prop;
        }
        
        // There could be more MAC addresses, CpuIds, (many NIC, Processors) so the best solution would be to create hash of all of them, but this is sufficient here.

        private static string GetMAC()
        {
            return NetworkInterface
            .GetAllNetworkInterfaces()
            .Where(nic => nic.OperationalStatus == OperationalStatus.Up && nic.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            .Select(nic => nic.GetPhysicalAddress().ToString())
            .FirstOrDefault();
        }

        private static string GetOS() => GetWmicValue("Win32_OperatingSystem", "Caption");

        private static string GetCpuId() => GetWmicValue("Win32_Processor", "ProcessorId");
        private static string GetRAMCapacity() => GetWmicValue("Win32_PhysicalMemory", "Capacity");
    }
}
