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
        Task AddScriptAsync(Script script);
        Task<ScriptResult> ExecuteAsync();
    }
}
