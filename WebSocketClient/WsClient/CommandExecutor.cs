using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WsClient.Models;

namespace WsClient
{
    internal class CommandExecutor
    {
        public static Queue<Command> Commands = new Queue<Command>();
        public static Queue<CommandResult> CommandResults = new Queue<CommandResult>();
        public static bool TryExecute()
        {
            var cmd = Commands.Last();
            if (cmd.WaitForNextCommand)
            {
                return false;
            }

            Execute();
            return true;
        }

        private static void Execute()
        {
            // TODO: Execute command in appropriate shell
            throw new NotImplementedException();
        }
    }
}
