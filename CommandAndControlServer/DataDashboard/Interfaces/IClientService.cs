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
        IReadOnlyDictionary<Client, WebSocket> ConnectedClients { get; }
        ConcurrentObservableCollection<Script> ClientScripts { get; }
        ConcurrentObservableCollection<ScriptResult> ScriptResults { get; }
        CancellationToken CancellationToken { get; }
        bool AddConnectedClient(Client client, WebSocket webSocket);
        bool RemoveConnectedClient(Client client);

    }
}