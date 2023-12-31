using DataDashboard.BLL;
using DataDashboard.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;

namespace DataDashboard.Controllers
{
    [Authorize]
    public class ClientController : Controller
    {
        private readonly ILogger<ClientController> _logger;
        private readonly ConnectionPipeline _connectionPipeline;
        public ClientController(ILogger<ClientController> logger, ConnectionPipeline connectionPipeline)
        {
            _logger = logger;
            _connectionPipeline = connectionPipeline;
        }
        [AllowAnonymous]
        public async Task<ActionResult> Index()
        {
            await _connectionPipeline.Test();
            return View();
        }

        [Route("{controller}/ws")]
        [AllowAnonymous]
        public async Task ManageRelayConnection()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                try
                {
                    await _connectionPipeline.HandleClient(HttpContext);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Internal server error handling a client");
                    HttpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
                }
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }
    }
}
