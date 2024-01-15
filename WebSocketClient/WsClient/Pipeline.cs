using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        private ConcurrentQueue<Script> Commands = new ConcurrentQueue<Script>();
        private ConcurrentQueue<ScriptResult> CommandResults = new ConcurrentQueue<ScriptResult>();

        public event EventHandler<EventArgs>? CommandQueued;
        public event EventHandler<EventArgs>? CommandResultQueued;

        protected virtual void OnCommandQueued(object? sender, EventArgs args) =>
            CommandQueued?.Invoke(sender, args);

        protected virtual void OnCommandResultQueued(object? sender, EventArgs args) =>
            CommandQueued?.Invoke(sender, args);

        public void AddToQueue(Script cmd)
        {
            Commands.Enqueue(cmd);
            OnCommandQueued(this, EventArgs.Empty);
        }
        public void AddCommandResult(ScriptResult result)
        {
            CommandResults.Enqueue(result);
            OnCommandResultQueued(this, EventArgs.Empty);
        }
        public bool GetCommandResult(out ScriptResult result) =>  CommandResults.TryDequeue(out result);

        public async Task Invoke()
        {
            bool dequed = Commands.TryDequeue(out Script? script);
            // TODO: Sometimes script is null even though Invoke is called only when Script is added to queue, investigate why
            if (!dequed) return;
            // Get Type of command.Shell with reflection
            Type objectType = (from asm in AppDomain.CurrentDomain.GetAssemblies()
                               from type in asm.GetTypes()
                               where type.IsClass && type.Name == script.Shell.ToString()
                               select type).Single();

            // Intantiate the object and explicitly type it to IShell
            object? obj = Activator.CreateInstance(objectType);
            IShell shell = (IShell) obj!;

            // Create and execute the script 
            shell.Create();
            await shell.AddScriptAsync(script);
            var result = await shell.ExecuteAsync();

            AddCommandResult(result);
        }
    }
}
