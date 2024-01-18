using DataDashboard.Data;
using DataDashboard.Helpers;
using DataDashboard.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NuGet.Versioning;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Net.WebSockets;
using System.Timers;
using static System.Formats.Asn1.AsnWriter;

namespace DataDashboard.BLL.Services
{
    /// <summary>
    /// Service for managing connected clients and and database operations related to clients
    /// </summary>
    public class ClientService : IClientService
    {
        private ConcurrentDictionary<Client, WebSocket> _connectedClients = new ConcurrentDictionary<Client, WebSocket>();
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private System.Timers.Timer _timer = default!; // Timer is initialized in SetTimer() - constructor
        private double _timerInterval = 5000;
        private ILogger<IClientService> _logger;
        // To retrieve scoped/transient services. In this case database context
        private IServiceProvider _provider;

        public CancellationToken CancellationToken { get => _cancellationTokenSource.Token; }
        public ConcurrentObservableCollection<Script> ClientScripts { get; } = new ConcurrentObservableCollection<Script>();
        public ConcurrentObservableCollection<ScriptResult> ScriptResults { get; } = new ConcurrentObservableCollection<ScriptResult>();
        public IReadOnlyDictionary<Client, WebSocket> ConnectedClients
        {
            get
            {
                return _connectedClients.AsReadOnly();
            }
        }
        public ClientService(IServiceProvider provider, ILogger<IClientService> logger)
        {
            _provider = provider;
            _logger = logger;
            SetTimer();
        }

        private void SetTimer()
        {
            try
            {
                _timer = new System.Timers.Timer(_timerInterval);
                _timer.Elapsed += async (sender, e) => await CollectionCleaner(sender, e);
                _timer.AutoReset = true;
                _timer.Enabled = true;
                _timer.Start();
            }
            catch(System.Exception exception)
            {
                _logger.LogError(exception, "Handler failed while cleaning collections");
                // Reset timer if it fails for any reason
                _timer.Stop();
                _timer.Dispose();
                SetTimer();
            }
        }

        // This gets executed on ThreadPool
        private async Task CollectionCleaner(object? sender, ElapsedEventArgs e)
        {
        // This service cannot block the main thread, so delegate it to ThreadPool
            await Task.Run(() =>
            { 
                // This can technically be handled inside the handler that adds ScriptResults to collection, but I think this is better, mainly because of Single Responsibility Principle
                if (ClientScripts.Count() == 0 && ScriptResults.Count() == 0) return;
                foreach (var script in ClientScripts.ToList())
                {
                    var results = ScriptResults.Where(result => result.CommandId == script.Id).ToList();
                    // If results from all clients are in the ScriptResults, remove these instances with corresponding Script.id from both collections
                    if (results.Count() == ConnectedClients.Count())
                    {
                        results.ForEach(result => ScriptResults.Remove(result));
                        ClientScripts.Remove(script);
                    }
                };
            });
        }
        ~ClientService()
        {
            _timer.Stop();
            _timer.Dispose();
        }

        public bool AddConnectedClient(Client client, WebSocket webSocket) =>
            _connectedClients.TryAdd(client, webSocket);
        public bool RemoveConnectedClient(Client client) =>
            _connectedClients.TryRemove(client, out _);

        
    }
}
