using DataDashboard.Data;
using DataDashboard.Models;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace DataDashboard.BLL
{
    public class ConnectionPipeline
    {
        private ConcurrentDictionary<string, WebSocket> _connectedClients = new ConcurrentDictionary<string, WebSocket>();
        private readonly ILogger<ConnectionPipeline> _logger;
        private readonly Data.ClientRepository _clientRepository;
        public IReadOnlyDictionary<string, WebSocket> ConnectedClients
        {
            get
            {
                return _connectedClients.AsReadOnly();
            } 
        }
        public ConnectionPipeline(ILogger<ConnectionPipeline> logger, Data.ClientRepository clientService)
        {
            _logger = logger;
            _clientRepository = clientService;
        }
        public async Task HandleClient(string id, WebSocket webSocket)
        {
            AddClient(id, webSocket);

            // Loop until websocket closes
            while (!webSocket.CloseStatus.HasValue)
            {
                try
                {
                    var buffer = new byte[4096];
                    var result = await webSocket.ReceiveAsync(buffer: new ArraySegment<byte>(buffer), CancellationToken.None);
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                    }
                    else if (result.MessageType == WebSocketMessageType.Text)
                    {

                        string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        Client info = JsonSerializer.Deserialize<Client>(message);
                        await _clientRepository.AddClient(info);
                    }
                }
                catch (Exception ex)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "Internal server error.", CancellationToken.None);
                    _logger.LogError(ex, "Error handling a client");
                }
            }
            RemoveClient(id);
        }

        private void AddClient(string id, WebSocket webSocket)
        {
            _connectedClients.TryAdd(id, webSocket);
        }
        
        private void RemoveClient(string id)
        {
            _connectedClients.TryRemove(id, out WebSocket webSocket);
            //await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Close", CancellationToken.None);
        }
    }
}
