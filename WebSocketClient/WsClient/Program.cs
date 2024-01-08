using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Timers;
using WsClient.Models;

namespace WsClient
{
    internal class Program
    {
        private static Pipeline _pipeline = new Pipeline();
        private static Client _client = new Client();
        static async Task Main(string[] args)
        {
            string host = "127.0.0.1";
            string port = "5101";
            string path = "/Client/ws";

            // TODO: Secure websocket connection
            Uri serverUri = new Uri($"ws://{host}:{port}{path}");
            _pipeline.CommandQueued += CommandQueuedHandler;
            try
            {
                // Establish websocket connection
                await _client.ConnectAsync(serverUri);
                // Authenticate client to server
                await _client.Authenticate();

                while (!_client.Socket.CloseStatus.HasValue)
                {
                    // Listen for command from server
                    Script script = await _client.ListenForScript();
                    _pipeline.AddToQueue(script);
                }
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                // Executes only if close status was not already initiated by server
                if (!_client.Socket.CloseStatus.HasValue) await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closed");
            }
        }
        // Execute command and send result back to server on ThreadPoool thread
        private static async void CommandQueuedHandler(object? sender, EventArgs e)
        {
            await Task.Run(async () =>
            {
                await _pipeline.Invoke();
                //Exceptions should be handled in Invoke()
                _pipeline.GetCommandResult(out ScriptResult? result);
                await _client.SendResponse(result!);
            });
        }
    }
}
