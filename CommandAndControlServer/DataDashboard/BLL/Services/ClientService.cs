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
        private System.Timers.Timer _timer;
        private double _timerInterval = 5000;
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
        public ClientService(IServiceProvider provider)
        {
            _provider = provider;
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
            catch(System.Exception exceptions)
            {
                // Reset timer if it fails for any reason
                _timer.Stop();
                _timer.Dispose();
                SetTimer();
            }
        }

        private async Task CollectionCleaner(object? sender, ElapsedEventArgs e)
        {
        // This service cannot block the main thread, so delegate it to ThreadPool
            await Task.Run(() =>
            { 
                // This can technically be handled inside the handler that adds ScriptResults to collection, but I think this is better, mainly because of Single Responsibility Principle
                if (ClientScripts.Count() == 0 && ScriptResults.Count() == 0) return;
                foreach (var script in ClientScripts)
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

        public bool AddConnectedClient(Client client, WebSocket webSocket) =>
            _connectedClients.TryAdd(client, webSocket);
        public bool RemoveConnectedClient(Client client) =>
            _connectedClients.TryRemove(client, out _);

        private ApplicationDbContext GetDbContextService()
        {
            var scope = _provider.CreateScope();
            return scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        }

        /// <summary>
        /// Checks if client is new by comparing MAC address
        /// </summary>
        public async Task<bool> IsNewClientAsync(ClientHwInfo hwInfo, ApplicationDbContext? dbContext = default)
        {
            // TODO: More complex new client check (Based on more info)
            if (dbContext == null) dbContext = GetDbContextService();
            return await dbContext.HwInfo.SingleOrDefaultAsync(dbInfo => dbInfo.MAC == hwInfo.MAC) == null ? true : false;
        }
        /// <summary>
        /// Creates new client and writes it with ClientHwInfo to database
        /// </summary>
        public async Task<Client> CreateNewClientAsync(ClientHwInfo clientInfo, string clientName = "", ApplicationDbContext? dbContext = default)
        {
            if (dbContext == null) dbContext = GetDbContextService();

            // Create client and write it to database, Id is generated there
            var client = new Client()
            {
                Name = clientName,
            };
            EntityEntry<Client> record = await dbContext.AddAsync(client);

            // Retrieve client Id from the entry and add it as primary key for ClientHwInfo
            clientInfo.Id = record.Property(prop => prop.Id).CurrentValue;
            await dbContext.HwInfo.AddAsync(clientInfo);
            var dbSave = dbContext.SaveChangesAsync();

            Client result = record.Entity;
            result.clientHwInfo = clientInfo;
            await dbSave;
            return result;
        }

        /// <summary>
        /// Retrieves all information about client from database
        /// </summary>
        public async Task<Client> GetClientAsync(ClientHwInfo clientInfo, ApplicationDbContext? dbContext = default)
        {
            if (dbContext == null) dbContext = GetDbContextService();

            // There is probably better way to do this, but this is fine
            var info = await dbContext.HwInfo.SingleAsync(db => db.MAC == clientInfo.MAC);
            var client = await dbContext.Clients.SingleAsync(db => db.Id == info.Id);
            client.clientHwInfo = info;
            await dbContext.Entry(client).Collection(info => info.SessionsHistory).LoadAsync();
            return client;
        }

        /// <summary>
        /// Retrieves client from database, if it does not exist, creates new one and writes ClientHwInfo to database
        /// </summary>
        public async Task<Client> GetCompleteClientAsync(ClientHwInfo clientInfo)
        {
            using ApplicationDbContext dbContext = GetDbContextService();

            bool isNewClient = await IsNewClientAsync(clientInfo, dbContext);
            if (isNewClient) return await CreateNewClientAsync(clientInfo, dbContext: dbContext);
            else return await GetClientAsync(clientInfo, dbContext);
        }

        public async Task SaveScriptResult(ScriptResult scriptResult)
        {
            throw new NotImplementedException();
        }
    }
}
