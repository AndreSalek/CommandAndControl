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
        public Task AddScriptAsync(Script script)
        {
            throw new NotImplementedException();
        }

        public void Create()
        {
            throw new NotImplementedException();
        }

        public Task<ScriptResult> ExecuteAsync()
        {
            throw new NotImplementedException();
        }
    }
}
