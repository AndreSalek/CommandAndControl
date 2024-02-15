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
        private Process _shellProcess = null;
        private string _scriptPath = null;
        private int _commandId;
        public void Create()
        {
            _shellProcess = new Process();
            _shellProcess.StartInfo = new ProcessStartInfo()
            {
                FileName = "powershell.exe",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
        }

        public async Task AddScriptAsync(Script script)
        {
            if (_shellProcess == null)
                throw new InvalidOperationException("Instance of the process is not created.");

            _commandId = script.Id;
            // Write script lines to a file
            _scriptPath = Directory.GetCurrentDirectory() + @"\script" + script.Id.ToString() + ".ps1";
            if (File.Exists(_scriptPath)) File.Delete(_scriptPath);
            await File.AppendAllLinesAsync(_scriptPath,
                                    script.Lines,
                                    Encoding.UTF8,
                                    CancellationToken.None);
            // Set the script path as argument (`&` is prefix for executing a PS file)
            _shellProcess.StartInfo.Arguments = "& " + _scriptPath;
        }
        
        public async Task<ScriptResult> ExecuteAsync()
        {
            // Start the process
            _shellProcess.Start();

            // Read process streams 
            string stdOut = _shellProcess.StandardOutput.ReadToEnd();
            string stdErr = _shellProcess.StandardError.ReadToEnd();

            await _shellProcess.WaitForExitAsync();

            // Free process resources
            _shellProcess.Dispose();

            //TODO: I suspect some memory leak regarding this process even after dispose?? Look for it this week

            // Return the result
            return new ScriptResult()
            {
                CommandId = _commandId,
                Content = stdOut,
                IsError = !String.IsNullOrEmpty(stdErr) ? true : false
            };
        }
    }
}
