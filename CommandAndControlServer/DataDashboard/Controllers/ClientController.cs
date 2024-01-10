using DataDashboard.BLL;
using DataDashboard.BLL.Services;
using DataDashboard.Data;
using DataDashboard.Models;
using DataDashboard.Utility;
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
            string closeReason = "Closing";
            WebSocketCloseStatus closeStatus = WebSocketCloseStatus.NormalClosure;

            string id = HttpContext.Connection.Id;
            WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            try
            {
                Client client;
                _clientService.AddConnectedClient(id, webSocket);

                var buffer = new byte[4096];
                var result = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Close) return;
                else if (result.MessageType == WebSocketMessageType.Text)
                {
                    // Receive authentication message from client
                    buffer = ArrayUtil.RemoveTrailingNulls(buffer);
                    string message = Encoding.UTF8.GetString(buffer);
                    ClientHwInfo clientInfo = JsonSerializer.Deserialize<ClientHwInfo>(message);

                    client = await _clientService.GetCompleteClientAsync(clientInfo);
                }
                else throw new WebSocketException("Invalid authentication message type");

                while (!webSocket.CloseStatus.HasValue)
                {
                    try
                    {
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
                        buffer = ArrayUtil.RemoveTrailingNulls(buffer);
                        var json = Encoding.UTF8.GetString(buffer);
                        ScriptResult scriptResult = JsonSerializer.Deserialize<ScriptResult>(json);
                        Trace.WriteLine(scriptResult.Content);
                    }
                    catch (InvalidDataException dataException)
                    {
                        _logger.LogError(dataException, "Error handling message request");
                    }
                }
            }
            catch(WebSocketException ex)
            {
                closeReason = "Invalid message type";
                closeStatus = WebSocketCloseStatus.InvalidMessageType;
                _logger.LogError(ex, "Error handling message request");
            }
            catch (Exception ex)
            {
                closeReason = "Internal Server Error";
                closeStatus = WebSocketCloseStatus.InternalServerError;
                _logger.LogError(ex, "Internal Server Error");
            }
            finally
            {
                await webSocket.CloseAsync(closeStatus, closeReason, CancellationToken.None);
            }
        //Close with no errors
        await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, closeReason, CancellationToken.None);
        _clientService.RemoveConnectedClient(id);
        }
    }
}
