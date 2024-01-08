using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WsClient.Handlers;
using WsClient.Interfaces;
using WsClient.Models;

namespace WsClient
{
    /// <summary>
    /// Class that keeps track of commands received from the server and executes decides when to execute them
    /// </summary>
    public class Pipeline
    {
        public ConcurrentQueue<Command> Commands = new ConcurrentQueue<Command>();
        public ConcurrentQueue<CommandResult> CommandResults = new ConcurrentQueue<CommandResult>();
        

        public async Task Invoke()
        {
            Commands.TryDequeue(out Command? command);
            ShellType sType = command.Shell;
            IShell shell = null!;
            string extension = "";
            switch (sType)
            {
                case ShellType.CommandShell:
                    shell = new CommandShell();
                    extension = ".cmd";
                    break;
                case ShellType.PowerShell:
                    shell = new PowerShell();
                    extension = ".ps1";
                    break;
                default:
                    throw new InvalidOperationException("Invalid shell type");
            }
            string scriptPath = $"C:\\Scripts\\script.{extension}";
            File.WriteAllText(scriptPath, command.Content);
            shell.Create();
            shell.AddScript(scriptPath);
            shell.ExecuteAsync();
        }
    }
}
