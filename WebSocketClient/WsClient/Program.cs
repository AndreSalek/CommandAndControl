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
            Thread.CurrentThread.Name = "Producer";
            string host = "127.0.0.1";
            string port = "5101";
            string path = "/Client/ws";

            // TODO: Secure websocket connection
            Uri serverUri = new Uri($"ws://{host}:{port}{path}");
            _client.CommandReceived += CommandReceivedHandler;
            try
            {
                // Establish websocket connection
                await _client.ConnectAsync(serverUri);
                // Authenticate client to server
                await _client.Authenticate();

                while (!_client.Socket.CloseStatus.HasValue)
                {
                    // Listen for command from server
                    await _client.ListenForCommand();
                    //await client.SendResponse();
                    
                }
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine(e.Message);
            }
            catch(Exception e)
            {

            }
            finally
            {
                // Executes only if close status was not already initiated by server
                if (!_client.Socket.CloseStatus.HasValue) await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client closed");

            }
        }

        public static async void CommandReceivedHandler(object? sender, CommandEventArgs args)
        {
            _pipeline.Commands.Enqueue(args.Command);
            // Execution happens on ThreadPool, so listening for command should happen continuously
            await Task.Run(async () =>
            {
                await _pipeline.Invoke();
                //Exceptions should be handled in Invoke()
                _pipeline.CommandResults.TryDequeue(out CommandResult? result);
                await _client.SendResponse(result!);
            });
        }
    }
}
