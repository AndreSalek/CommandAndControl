using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;

namespace DataDashboard.Controllers
{
    [Authorize]
    public class BotController : Controller
    {
        public ActionResult Index()
        {
            return View();
        }

        [Route("{controller}/ws")]
        [AllowAnonymous]
        public async Task ManageRelayConnection()
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                byte[] data = new byte[2096];
                //data = Encoding.UTF8.GetBytes("Test string");
                await webSocket.ReceiveAsync(data, CancellationToken.None);
                Trace.WriteLine(Encoding.UTF8.GetString(data));
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Close", CancellationToken.None);
            }
            else
            {
                HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            }
        }
    }
}
