using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WsClient.Models
{
    public class Command
    {
        public string Content { get; set; } = null!;
        public ShellType Shell { get; set; }
        public bool ReturnOutput { get; set; }
        public bool WaitForNextCommand { get; set; }
    }
}
