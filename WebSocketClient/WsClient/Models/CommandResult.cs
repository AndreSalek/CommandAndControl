using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WsClient.Models
{
    internal class CommandResult
    {
        public string Content { get; set; }
        public bool IsError { get; set; }
        public string ErrorMessage { get; set; }
    }
}
