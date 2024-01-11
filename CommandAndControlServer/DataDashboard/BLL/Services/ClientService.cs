using DataDashboard.Data;
using DataDashboard.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NuGet.Versioning;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Net.WebSockets;
using static System.Formats.Asn1.AsnWriter;

namespace DataDashboard.BLL.Services
{
    /// <summary>
    /// Service for managing connected clients and and database operations related to clients
    /// </summary>
    public class ClientService
    {
        private ConcurrentDictionary<Client, WebSocket> _connectedClients = new ConcurrentDictionary<Client, WebSocket>();
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        // To retrieve scoped/transient services. In this case database context
        private IServiceProvider _provider;
        public CancellationToken cancellationToken { get => _cancellationTokenSource.Token; }
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
        /// Checks if client is new by comparing MAC address to database
        /// </summary>
        public async Task<bool> IsNewClientAsync(ClientHwInfo hwInfo, ApplicationDbContext dbContext = default!)
        {
            // TODO: More complex new client check (Based on more info)
            if (dbContext == null) dbContext = GetDbContextService();
            return await dbContext.HwInfo.SingleOrDefaultAsync(dbInfo => dbInfo.MAC == hwInfo.MAC) == null ? true : false;
        }
        /// <summary>
        /// Creates new client and writes it with ClientHwInfo to database
        /// </summary>
        public async Task<Client> CreateNewClientAsync(ClientHwInfo clientInfo, string clientName = "", ApplicationDbContext dbContext = default!)
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
        public async Task<Client> GetClientAsync(ClientHwInfo clientInfo, ApplicationDbContext dbContext = default!)
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
        /// Retrieves client from database by MAC, if it does not exist, creates new one and writes ClientHwInfo to database
        /// </summary>
        public async Task<Client> GetCompleteClientAsync(ClientHwInfo clientInfo)
        {
            using ApplicationDbContext dbContext = GetDbContextService();

            bool isNewClient = await IsNewClientAsync(clientInfo, dbContext);
            if (isNewClient) return await CreateNewClientAsync(clientInfo, dbContext: dbContext);
            else return await GetClientAsync(clientInfo, dbContext);
        }
    }   
}
