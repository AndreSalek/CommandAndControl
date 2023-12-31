using DataDashboard.Data;
using DataDashboard.Models;
using System.Collections.Concurrent;
using System.Net.WebSockets;

namespace DataDashboard.BLL.Services
{
    public class ClientService
    {
        private ConcurrentDictionary<string, WebSocket> _connectedClients = new ConcurrentDictionary<string, WebSocket>();
        private CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private ClientRepository _clientRepository;
        public CancellationToken cancellationToken { get => _cancellationTokenSource.Token; }
        public IReadOnlyDictionary<string, WebSocket> ConnectedClients
        {
            get
            {
                return _connectedClients.AsReadOnly();
            }
        }
        public ClientService(ClientRepository clientRepository)
        {
            _clientRepository = clientRepository;
        }

        public void AddConnectedClient(string id, WebSocket webSocket) =>
            _connectedClients.TryAdd(id, webSocket);
        public void RemoveConnectedClient(string id) =>
            _connectedClients.TryRemove(id, out _);
        public async Task<bool> IsNewClientAsync(ClientHwInfo hwInfo)
        {
            return await _clientRepository.GetClientHwInfoByMACAsync(hwInfo.MAC) != null ? true : false;
        }

        public async Task<Client> CreateNewClientAsync(ClientHwInfo clientInfo, string clientName = "")
        {
            // Create client and write it to database
            var client = new Client()
            {
                Name = clientName,
            };
            var record = await _clientRepository.AddClientAsync(client);

            // Add client id to clientInfo and write it to database
            clientInfo.Id = record.Property(prop => prop.Id).CurrentValue;
            await _clientRepository.AddClientHwInfoAsync(clientInfo);

            Client result = record.Entity;
            result.clientHwInfo = clientInfo;
            return result; 
        }
    }
}
