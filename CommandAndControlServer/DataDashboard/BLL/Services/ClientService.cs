using DataDashboard.Data;
using DataDashboard.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using NuGet.Versioning;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using static System.Formats.Asn1.AsnWriter;

namespace DataDashboard.BLL.Services
{
    /// <summary>
    /// Service for managing connected clients and and database operations related to clients
    /// </summary>
    public class ClientService
    {
        private ConcurrentDictionary<string, WebSocket> _connectedClients = new ConcurrentDictionary<string, WebSocket>();
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        // To retrieve scoped/transient services. In this case database context
        private IServiceProvider _provider;
        public CancellationToken cancellationToken { get => _cancellationTokenSource.Token; }
        public IReadOnlyDictionary<string, WebSocket> ConnectedClients
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

        public bool AddConnectedClient(string id, WebSocket webSocket) =>
            _connectedClients.TryAdd(id, webSocket);
        public bool RemoveConnectedClient(string id) =>
            _connectedClients.TryRemove(id, out _);

        private ApplicationDbContext GetDbContextService()
        {
            var scope = _provider.CreateScope();
            return scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        }

        /// <summary>
        /// Checks if client is new by comparing MAC address to database
        /// </summary>
        private async Task<bool> IsNewClientAsync(ClientHwInfo hwInfo, ApplicationDbContext dbContext = default!)
        {
            // TODO: More complex new client check (Based on more info)
            if (dbContext == null) dbContext = GetDbContextService();
            return await dbContext.HwInfo.SingleOrDefaultAsync(dbInfo => dbInfo.MAC == hwInfo.MAC) == null ? true : false;
        }
        /// <summary>
        /// Creates new client and writes it with ClientHwInfo to database
        /// </summary>
        /// <param name="clientInfo"></param>
        /// <param name="clientName"></param>
        /// <param name="dbContext"></param>
        /// <returns></returns>
        private async Task<Client> CreateNewClientAsync(ClientHwInfo clientInfo, string clientName = "", ApplicationDbContext dbContext = default!)
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
        /// <returns></returns>
        private async Task<Client> GetClientAsync(ClientHwInfo clientInfo, ApplicationDbContext dbContext = default!)
        {
            if (dbContext == null) dbContext = GetDbContextService();

            Client client = await dbContext.Clients.SingleAsync(client => client.Id == clientInfo.Id);
            await dbContext.Entry(client).Reference(info => info.clientHwInfo).LoadAsync();
            await dbContext.Entry(client).Reference(info => info.SessionsHistory).LoadAsync();
            return client;
        }

        /// <summary>
        /// Retrieves client from database by MAC, if it does not exist, creates new one and writes ClientHwInfo to database
        /// </summary>
        public async Task<Client> GetCompleteClientAsync(ClientHwInfo clientInfo)
        {
            ApplicationDbContext dbContext = GetDbContextService();

            bool isNewClient = await IsNewClientAsync(clientInfo, dbContext);
            if (isNewClient) return await CreateNewClientAsync(clientInfo, dbContext: dbContext);
            else return await GetClientAsync(clientInfo, dbContext);

        }
    }   
}
