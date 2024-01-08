using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WsClient.Interfaces;
using WsClient.Models;

namespace WsClient.Handlers
{
    public class PowerShell : IShell
    {
        private Process _shellProcess;
        public void Create()
        {
            _shellProcess = new Process();
            _shellProcess.StartInfo = new ProcessStartInfo()
            {
                FileName = "powershell.exe",
                UseShellExecute = false,
                CreateNoWindow = true,
            };
        }

        public void AddScript(string scriptPath)
        {
            if (_shellProcess == null)
                throw new InvalidOperationException("Instance of the process is not created.");

            _shellProcess.StartInfo.Arguments = "& " + scriptPath;
        }
        
        public async Task<CommandResult> ExecuteAsync()
        {
            //TODO: Implement execute
            return new CommandResult()
            {
                Content = "Success",
                IsError = false,
                ErrorMessage = ""
            };
        }
    }
}
