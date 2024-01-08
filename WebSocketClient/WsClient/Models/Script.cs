using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WsClient.Models
{
    public class Script
    {
        public int Id { get; set; }
        public string[] Lines { get; set; } = null!;
        public ShellType Shell { get; set; }
    }
}
