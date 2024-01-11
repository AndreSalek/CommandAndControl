using DataDashboard.BLL;
using DataDashboard.BLL.Services;
using DataDashboard.Data;
using DataDashboard.Helpers;
using DataDashboard.Models;
using DataDashboard.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Security.Authentication;
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
                _logger.LogInformation("Non-websocket request received.");
                return;
            }
            string closeReason = "Closing";
            WebSocketCloseStatus closeStatus = WebSocketCloseStatus.NormalClosure;

            WebSocket webSocket = default!;
            Client client = default!;
            try
            {
                webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                // Authenticate client and add it to connected clients
                ClientHwInfo info = await CommunicationManager.ReceiveDataAsync<ClientHwInfo>(webSocket) ?? throw new WebSocketException("Connection closed.");
                client = await _clientService.GetCompleteClientAsync(info);
                _clientService.AddConnectedClient(client, webSocket);

                while (!_clientService.cancellationToken.IsCancellationRequested || webSocket.State == WebSocketState.Open)
                {
                    try
                    {

                        // retrieve script from collection
                        // Send script to client
                        Script script = new Script()
                        {
                            Id = 1,
                            Lines = new string[] { "Write-Output Hello World!" },
                            Shell = ShellType.PowerShell
                        };
                        await CommunicationManager.Send(script, webSocket);

                        ScriptResult scriptResult = await CommunicationManager.ReceiveDataAsync<ScriptResult>(webSocket) ?? throw new WebSocketException("Connection closed.");
                        _logger.LogInformation($"Script result: {scriptResult.Content}");
                    }
                    catch (InvalidDataException dataException)
                    {
                        _logger.LogError(dataException, "Error handling message request");
                    }
                }
            }
            catch (WebSocketException ex) when (webSocket.State == WebSocketState.Aborted)
            {
                _logger.LogError("Client disconnected without finishing close handshake " + ex.Message + "\r\n"
                                 + ex.StackTrace);
            }
            catch (WebSocketException ex)
            {
                _logger.LogError("Error on websocket communication pipeline: " + ex.Message + "\r\n"
                                 + ex.StackTrace);
            }
            catch (InvalidOperationException opException)
            {
                closeReason = "Error occured while handling provided data";
                closeStatus = WebSocketCloseStatus.InternalServerError;
                _logger.LogError("Error occured while handling provided data" + opException.Message + "\r\n"
                                 + opException.StackTrace);
            }
            catch (Exception ex)
            {
                closeReason = "Internal Server Error";
                closeStatus = WebSocketCloseStatus.InternalServerError;
                _logger.LogError("Internal Server Error" + ex.Message + "\r\n"
                                 + ex.StackTrace);
            }
            finally
            {
                if (webSocket.State == WebSocketState.Open)
                {
                    await webSocket.CloseAsync(closeStatus, closeReason, CancellationToken.None);
                    webSocket.Dispose();
                }
                _clientService.RemoveConnectedClient(client);
            }
        }
        
    }
}
