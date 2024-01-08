using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WsClient.Interfaces;
using WsClient.Models;

namespace WsClient.Handlers
{
    internal class CommandShell : IShell
    {
        public void Create()
        {
            throw new NotImplementedException();
        }
        public void AddScript(string scriptPath)
        {
            throw new NotImplementedException();
        }

        public async Task<CommandResult> ExecuteAsync()
        {
            throw new NotImplementedException();
        }
    }
}
