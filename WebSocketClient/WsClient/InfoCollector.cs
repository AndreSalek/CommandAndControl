using System;
using System.Collections.Generic;
using System.Linq;
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

        private static string GetMAC()
        {
            return "00:00:00:00:00:00";
        }

        private static string GetOS()
        {
            return "Windows 10";
        }

        private static string GetCpuId()
        {
            return "000000000";
        }
        private static int GetRAMCapacity()
        {
            return 8192;
        }
    }
}
