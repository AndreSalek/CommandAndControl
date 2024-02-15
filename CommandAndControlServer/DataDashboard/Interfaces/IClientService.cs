using DataDashboard.Data;
using DataDashboard.Helpers;
using DataDashboard.Models;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Net.WebSockets;

namespace DataDashboard.BLL.Services
{
    public interface IClientService
    {
        /// <summary>
        /// Holds connected clients by their unique id
        /// </summary>
        ConcurrentBag<int> ConnectedClients { get; }
        /// <summary>
        /// Completes when script was chosen by user to send to client
        /// </summary>
		public TaskCompletionSource<Script> ScriptToExecute { get; }
		CancellationToken CancellationToken { get; }
    }
}