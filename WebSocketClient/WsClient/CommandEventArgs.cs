using System.ComponentModel;
using WsClient.Models;

namespace WsClient
{
    public class CommandEventArgs : EventArgs
    {
        public Command Command { get; }

        public CommandEventArgs(Command command)
        {
            this.Command = command;
        }
    }
}