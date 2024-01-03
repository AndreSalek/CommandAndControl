using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WsClient.Models
{
    internal class ClientHwInfo
    {
        public string MAC { get; set; }
        public string OS { get; set; }
        public string CpuId { get; set; }
        public int RAMCapacity { get; set; }
    }
}
