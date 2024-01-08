using DataDashboard.BLL;
using DataDashboard.BLL.Services;
using DataDashboard.Data;
using DataDashboard.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared;
using System.Diagnostics;
using System.Net.Http;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace DataDashboard.Controllers
{
    [Authorize]
    public class ClientController : Controller
    {
        private readonly ILogger<ClientController> _logger;
        private readonly ClientService _clientService;
        public ClientController(ILogger<ClientController> logger, ClientService clientService)
        {
            _logger = logger;
            _clientService = clientService;
        }
        [AllowAnonymous]
        public async Task<ActionResult> Index()
        {
            await Test();
            return View();
        }

        [Route("{controller}/ws")]
        [AllowAnonymous]
        public async Task ManageRelayConnection()
        {
            if (!HttpContext.WebSockets.IsWebSocketRequest)
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
                return;
            }
            string id = HttpContext.Connection.Id;
            // Accept websocket request and add it to ConnectedClients
            WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();

            // Create scope for current request, which will exist until websocket closes

            _clientService.AddConnectedClient(id, webSocket);
            // Loop until websocket closes
            while (!webSocket.CloseStatus.HasValue)
            {
                try
                {
                    var buffer = new byte[4096];
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _clientService.cancellationToken);
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
                        bool isNewClient = await _clientService.IsNewClientAsync(clientInfo);
                        if (isNewClient) connectedClient = await _clientService.CreateNewClientAsync(clientInfo);

                        // Send script to client
                        Script script = new Script()
                        {
                            Id = 1,
                            Lines = new string[] { "Write-Output Hello World!" },
                            Shell = ShellType.PowerShell
                        };
                        string scriptJson = JsonSerializer.Serialize(script);
                        byte[] buff = Encoding.UTF8.GetBytes(scriptJson);
                        await webSocket.SendAsync(buff, WebSocketMessageType.Text, true, CancellationToken.None);

                        // Receive output from client
                        buffer = new byte[4096];
                        await webSocket.ReceiveAsync(buffer, _clientService.cancellationToken);
                        var scrBuff = Encoding.UTF8.GetString(buffer);
                        ScriptResult scriptResult = JsonSerializer.Deserialize<ScriptResult>(scrBuff);
                        Trace.WriteLine(scriptResult.Content);

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
        _clientService.RemoveConnectedClient(id);
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
            await _clientService.CreateNewClientAsync(clientInfo);
        }
    }
}
