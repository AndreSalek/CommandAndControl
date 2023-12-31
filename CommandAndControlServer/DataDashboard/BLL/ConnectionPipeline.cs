using Azure.Core;
using DataDashboard.BLL.Services;
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
        private readonly ILogger<ConnectionPipeline> _logger;
        // Used to retrieve ClientService(scoped) from DI container
        private readonly IServiceProvider _provider;

        public ConnectionPipeline(ILogger<ConnectionPipeline> logger, IServiceProvider provider)
        {
            _logger = logger;
            _provider = provider;
        }
        /// <summary>
        /// Executes `bussiness logic` steps for handling a client connection
        /// </summary>
        /// <param name="httpContext">Context of websocket request</param>
        /// <returns></returns>
        public async Task HandleClient(HttpContext httpContext)
        {
            // Accept websocket request and add it to ConnectedClients
            WebSocket webSocket = await httpContext.WebSockets.AcceptWebSocketAsync();
            string id = httpContext.Session.Id;

            // Create scope for current request, which will exist until websocket closes
            using (var scope = _provider.CreateScope())
            {
                ClientService clientService = scope.ServiceProvider.GetRequiredService<ClientService>();
                clientService.AddConnectedClient(id, webSocket);
                // Loop until websocket closes
                while (!webSocket.CloseStatus.HasValue)
                {
                    try
                    {
                        var buffer = new byte[4096];
                        var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), clientService.cancellationToken);
                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                        }
                        else if (result.MessageType == WebSocketMessageType.Text)
                        {
                            //Read and deserialize expected ClientHwInfo 
                            string message = Encoding.UTF8.GetString(buffer);
                            ClientHwInfo clientInfo = JsonSerializer.Deserialize<ClientHwInfo>(message);

                            // Check if client is new, retrieve client from database if not
                            Client connectedClient;
                            bool isNewClient = await clientService.IsNewClientAsync(clientInfo);
                            if (isNewClient) connectedClient = await clientService.CreateNewClientAsync(clientInfo);

                            
                        }
                    }
                    catch (Exception ex)
                    {
                        await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "Error handling message request", CancellationToken.None);
                        _logger.LogError(ex, "Error handling message request");
                    }
                }
                //Close with no errors
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Close", CancellationToken.None);
                clientService.RemoveConnectedClient(id);
            }
        }

        public async Task Test()
        {
            ClientHwInfo clientInfo = new ClientHwInfo()
            {
                MAC = "00:00:00:00:00:00",
                OS = "Windows10",
                CpuId = "123456789",
                RAMCapacity = 16
            };
            using (var scope = _provider.CreateScope())
            {
                var clientService = scope.ServiceProvider.GetRequiredService<ClientService>();

                await clientService.CreateNewClientAsync(clientInfo);
            }
        }
    }
}
