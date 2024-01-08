using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WsClient.Models;

namespace WsClient.Interfaces
{
    public interface IShell
    {
        void Create();
        void AddScript(string scriptPath);
        Task<CommandResult> ExecuteAsync();
    }
}
