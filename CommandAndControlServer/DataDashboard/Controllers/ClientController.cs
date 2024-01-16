﻿using DataDashboard.BLL;
using DataDashboard.BLL.Services;
using DataDashboard.Data;
using DataDashboard.Helpers;
using DataDashboard.Models;
using DataDashboard.Utility;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared;
using NuGet.Packaging;
using System.Collections.Specialized;
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
        private readonly IClientService _clientService;
        public ClientController(ILogger<ClientController> logger, ClientService clientService)
        {
            _logger = logger;
            _clientService = clientService;
        }
        [AllowAnonymous]
        public IActionResult Index()
        {
            _clientService.ClientScripts.AddRange(SeedData());
            return View();
        }

        private IEnumerable<Script> SeedData()
        {
            // Test
            // TODO: Add script into database and then add it to collection
            return new Script[]
            {
                new Script()
                {
                    Id = 1,
                    Lines = new string[] { "Write-Output Hello World!" },
                    Shell = ShellType.PowerShell
                },
                new Script()
                {
                    Id = 2,
                    Lines = new string[] { "echo Hello World!" },
                    Shell = ShellType.PowerShell
                }
            };
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

            // Declare variables outside try block so they can be used in finally block
            WebSocket webSocket = default!;
            Client client = default!;
            NotifyCollectionChangedEventHandler ScriptAddedHandler = default!;
            NotifyCollectionChangedEventHandler ResultAddedHandler = default!;
            try
            {
                webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                // Authenticate client and add it to connected clients
                ClientHwInfo info = await CommunicationManager.ReceiveDataAsync<ClientHwInfo>(webSocket) ?? throw new WebSocketException("Connection closed.");
                _logger.LogInformation($"Client connected: {info.MAC}");
                client = await _clientService.GetCompleteClientAsync(info);
                _clientService.AddConnectedClient(client, webSocket);

                // Creating anonymous handlers because I need WebSocket instance to send the script to client
                // Othewise I'd need to iterate through ConnectedClients Dictionary to do the same thing (which also needs IClientService reference)

                // Send scripts to client when new data is added to collection
                ScriptAddedHandler = async (s, o) =>
                {
                    if (o.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                    {
                        if (o.NewItems == null) return;
                        foreach (var item in o.NewItems)
                        {
                            await CommunicationManager.Send(item , webSocket);
                        }
                    }
                };
                // Save script results to database when new data is added to collection 
                ResultAddedHandler = async (s, o) =>
                {
                    if (o.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Add)
                    {
                        if (o.NewItems == null) return;

                        foreach (var item in o.NewItems)
                        {
                            _logger.LogInformation($"Writing script result from {client.Id} to database");
                            await _clientService.SaveScriptResult((ScriptResult)item);   
                        }
                    }
                };
                // Subscribe to collection changed events
                _clientService.ClientScripts.CollectionChanged += ScriptAddedHandler;
                _clientService.ScriptResults.CollectionChanged += ResultAddedHandler;

                // Cancellation token will be cancelled when server is shut down
                // This is main loop for websocket communication
                while (!_clientService.CancellationToken.IsCancellationRequested || webSocket.State == WebSocketState.Open)
                {
                    try
                    {
                        // Always listen for script results
                        ScriptResult? scriptResult = await CommunicationManager.ReceiveDataAsync<ScriptResult>(webSocket);

                        // Connection closed
                        if (scriptResult == null) { break;}
                        // Result received
                        else
                        {
                            _logger.LogInformation($"Script result: \r\n" +
                                                   $"Client: {client.Id} , MAC: {client.clientHwInfo.MAC} \r\n" + 
                                                   $"Returned result for script Id {scriptResult.CommandId}: " +
                                                   $"{scriptResult.Content}");
                            // Add result to collection
                            scriptResult.ClientId = client.Id;
                            _clientService.ScriptResults.Add(scriptResult);
                        }
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
                //Checking statuses in case something unexpected happens, so there is no exception in finally block
                if (webSocket.State == WebSocketState.Open)
                {
                    await webSocket.CloseAsync(closeStatus, closeReason, CancellationToken.None);
                    webSocket.Dispose();
                }
                if (ScriptAddedHandler != null) _clientService.ClientScripts.CollectionChanged -= ScriptAddedHandler;
                if (ResultAddedHandler != null) _clientService.ScriptResults.CollectionChanged -= ResultAddedHandler;
                if (client != null)_clientService.RemoveConnectedClient(client);
            }
        }
    }
}
