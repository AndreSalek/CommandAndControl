using DataDashboard.Data;
using DataDashboard.Models;
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
        public async Task<bool> IsNewClientAsync(ClientHwInfo hwInfo)
        {
            var scope = _provider.CreateScope();
            
            ClientRepository clientRepository = scope.ServiceProvider.GetRequiredService<ClientRepository>();
            return await clientRepository.GetClientHwInfoByMACAsync(hwInfo.MAC) != null ? true : false;
        }

        public async Task<Client> CreateNewClientAsync(ClientHwInfo clientInfo, string clientName = "")
        {
            // Create client and write it to database
            var client = new Client()
            {
                Name = clientName,
            };
            var scope = _provider.CreateScope();

            ClientRepository clientRepository = scope.ServiceProvider.GetRequiredService<ClientRepository>();
            var record = await clientRepository.AddClientAsync(client);

            // Add client id to clientInfo and write it to database
            clientInfo.Id = record.Property(prop => prop.Id).CurrentValue;
            await clientRepository.AddClientHwInfoAsync(clientInfo);

            Client result = record.Entity;
            result.clientHwInfo = clientInfo;
            return result; 
        }
    }
}
