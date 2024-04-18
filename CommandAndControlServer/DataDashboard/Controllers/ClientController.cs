using DataDashboard.BLL;
using DataDashboard.BLL.Services;
using DataDashboard.Data;
using DataDashboard.Helpers;
using DataDashboard.Models;
using DataDashboard.Utility;
using DataDashboard.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.DotNet.Scaffolding.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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
        private readonly ApplicationDbContext _context;
        private readonly ClientService _clientService;
        public ClientController(ILogger<ClientController> logger, ClientService clientService, ApplicationDbContext context)
        {
            _logger = logger;
            _clientService = clientService;
            _context = context;
        }
        public IActionResult Index()
        {
            IEnumerable<Client> clients = _context.Clients.Include(c => c.ClientHwInfo).Include(c => c.ConnectionHistory).ToList() ?? Enumerable.Empty<Client>();
            List<ClientViewModel> viewmodels = new List<ClientViewModel>();
            //Create ClientViewModel here
            foreach (var client in clients)
            {
                var lastConnection = client.ConnectionHistory.LastOrDefault() ?? new ConnectionData();
                ClientViewModel vm = new ClientViewModel()
                {
                    Id = client.Id,
                    Name = client.Name,
                    LastIP = lastConnection.IP ?? String.Empty,
                    LastConnectionTime = lastConnection.ConnectedAt,
                    CpuId = client.ClientHwInfo.CpuId ?? String.Empty,
                    MAC = client.ClientHwInfo.MAC,
                    RAMCapacity = client.ClientHwInfo.RAMCapacity ?? String.Empty,
                    OS = client.ClientHwInfo.OS ?? String.Empty
                };
                viewmodels.Add(vm);
            }


            return View(viewmodels);
        }

        public IActionResult Scripts()
        {
            _clientService.CheckDifferences();
            var dbList = _context.Scripts.ToList();
            List<ScriptViewModel> viewList = new List<ScriptViewModel>();
            dbList.ForEach(item =>
            {
                viewList.Add(
                    new ScriptViewModel()
                    {
                        Id = item.Id,
                        Name = item.Name,
                        Shell = item.Shell.ToString()
                    }
                    );
            });
            return View(viewList);
        }

        public async Task<IActionResult> Execute(int? id)
        {
            if (id == null) return View();
            try
            {
                var paths = Directory.GetFiles("Scripts");

                var script = await _context.Scripts.SingleAsync(s => s.Id == id);
                foreach (var path in paths)
                {
                    var name = Path.GetFileNameWithoutExtension(path);
                    if (name == script.Name)
                    {
                        script.Lines = System.IO.File.ReadAllLines(path);
                        _clientService.ScriptToExecute.SetResult(script);
                    }
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
            return RedirectToAction("Scripts");
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return View();
            try
            {
                var paths = Directory.GetFiles("Scripts");
                var script = _context.Scripts.Single(s => s.Id == id);

                var file = paths.Where(item => Path.GetFileNameWithoutExtension(item) == script.Name).SingleOrDefault();

                _context.Scripts.Remove(script);
                await _context.SaveChangesAsync();
                System.IO.File.Delete(file);

            }
            catch (Exception e)
            {
                _logger.LogError(e.Message);
            }
            return RedirectToAction("Scripts");
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
            int clientId = default;
            try
            {
                webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                // Authenticate client and add it to connected clients
                
                ClientHwInfo info = await CommunicationManager.ReceiveDataAsync<ClientHwInfo>(webSocket) ?? throw new WebSocketException("Connection closed.");
                _logger.LogInformation($"Client connected: {info.MAC}");

				bool isNewClient = await _context.HwInfo.SingleOrDefaultAsync(dbInfo => dbInfo.MAC == info.MAC) == null ? true : false;
                if (isNewClient)
                {
					_logger.LogInformation($"Client is new, creating new record");
                    var client = new Client();
                    EntityEntry<Client> record = await _context.Clients.AddAsync(client);
                    await _context.SaveChangesAsync();
                    info.Id = record.Entity.Id;
                    // Retrieve client Id from the entry and add it as primary key for ClientHwInfo
                    //clientInfo.Id = record.Property(prop => prop.Id).CurrentValue;
                    await _context.HwInfo.AddAsync(info);
                    await _context.SaveChangesAsync();
                }
                clientId = _context.Clients.Single(client => client.ClientHwInfo.MAC == info.MAC).Id;
                ConnectionData connectionData = new ConnectionData()
                {
                    ClientId = clientId,
                    ConnectionId = Request.HttpContext.Connection.Id,
                    IP = Request.HttpContext.Connection.RemoteIpAddress.ToString(),
                    ConnectedAt = DateTime.Parse(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"))
                };
                await _context.Sessions.AddAsync(connectionData);
                await _context.SaveChangesAsync();
                //clientId = client.Id;
                //ient = await _repository.GetCompleteClientAsync(info);
                _clientService.ConnectedClients.Add(clientId);

                // Cancellation token will be cancelled when server is shut down
                // This is main loop for websocket communication
                while (!_clientService.CancellationToken.IsCancellationRequested || webSocket.State == WebSocketState.Open)
                {
                    try
                    {
                        Script script = await _clientService.ScriptToExecute.Task;
                        await CommunicationManager.Send(script, webSocket);
                        // Always listen for script results
                        ScriptResult? scriptResult = await CommunicationManager.ReceiveDataAsync<ScriptResult>(webSocket);
                        // Connection closed
                        if (scriptResult == null) { break;}
                        _clientService.ScriptToExecute = new TaskCompletionSource<Script>();
                        // Result received
                        _logger.LogInformation($"Script result: \r\n" +
                                                $"Returned result for script Id {scriptResult.CommandId}: " +
                                                $"{scriptResult.Content}");
                        // Add result to collection
                        scriptResult.ClientId = clientId;

                        await _context.ScriptResults.AddAsync(scriptResult);
                        await _context.SaveChangesAsync();
                        //await _context.ScriptResults.Add(scriptResult);
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
                _clientService.ConnectedClients.TryTake(out _);
            }
        }
    }
}
