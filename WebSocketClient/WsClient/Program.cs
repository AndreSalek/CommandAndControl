using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Timers;
using WsClient.Models;

namespace WsClient
{
    internal class Program
    {
        private static string _closeReason= "Client close";
        private static WebSocketCloseStatus _closeStatus = WebSocketCloseStatus.NormalClosure;
        private static Pipeline _pipeline = new Pipeline();
        private static Client _client = new Client();
        static async Task Main(string[] args)
        {
            string host = "127.0.0.1";
            string port = "5101";
            string path = "/Client/ws";

            // TODO: Secure websocket connection
            Uri serverUri = new Uri($"ws://{host}:{port}{path}");
            try
            {
                _pipeline.CommandQueued += CommandQueuedHandler;
                // Establish websocket connection
                await _client.ConnectAsync(serverUri);
                // Authenticate client to server
                await _client.Authenticate();

                while (_client.State == WebSocketState.Open)
                {
                    try
                    {
                        // Listen for script from server
                        
                        Script? script = await _client.ReceiveDataAsync<Script>();
                        // Can be null only if server closed connection
                        if (script != null)
                        {
                            _pipeline.AddToQueue(script);
                            Trace.WriteLine("Script added to pipeline");
                        }
                    }
                    catch (InvalidDataException invalidData)
                    {
                        Trace.WriteLine("Received script is invalid: " + invalidData.Message + "\r\n" +
                                        invalidData.Data);
                    }
                }
            }
            catch (WebSocketException wsException)
            {
                Trace.WriteLine("Error on websocket:" + wsException.Message);
                // TODO: Attempt reconnect instead of shutdown
            }
            catch (InvalidOperationException opException)
            {
                Trace.WriteLine("Invalid operation exception: " + opException.Message + "\r\n" +
                                        opException.Data);
            }

            Trace.WriteLine("Close value: " + _client.CloseStatus.Value);
        }
        // Execute command and send result back to server on ThreadPoool thread, so main thread can continue listening
        private static async void CommandQueuedHandler(object? sender, EventArgs e)
        {
            try
            {
                await Task.Run(async () =>
                {
                    await _pipeline.Invoke();
                    //Exceptions should be handled in Invoke()
                    _pipeline.GetCommandResult(out ScriptResult? result);
                    if (result != null)await _client.Send(result);
                });
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Exception: " + ex.Message + "\r\n" +
                                        ex.Data);
            }
        }
    }
}
