using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WsClient.Models;

namespace WsClient
{
    public class Client
    {
        private ClientWebSocket _socket = new ClientWebSocket();
        ArrayPool<byte> _bufferPool = ArrayPool<byte>.Shared;
        public ClientWebSocket Socket { get => _socket; }
        public event EventHandler<CommandEventArgs> CommandReceived;

        protected virtual void OnCommandReceived(object? sender, CommandEventArgs command) =>
            CommandReceived?.Invoke(this, command);

        public async Task ConnectAsync(Uri serverUri) =>
            await _socket.ConnectAsync(serverUri, CancellationToken.None);
        public async Task CloseAsync(WebSocketCloseStatus status, string desc) =>
            await _socket.CloseAsync(status, desc, CancellationToken.None);

        public async Task Authenticate()
        {
            // Create message and encode message for transport
            ClientHwInfo info = InfoCollector.GetHwInfo();
            string message = JsonSerializer.Serialize<ClientHwInfo>(info);
            int buffersize = System.Text.UTF8Encoding.Unicode.GetByteCount(message);
            byte[] buffer = new byte[buffersize];
            buffer = Encoding.UTF8.GetBytes(message);

            await _socket.SendAsync(new ArraySegment<byte>(buffer),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);
        }
        /// <summary>
        /// Listens on socket until a command is received
        /// </summary>
        /// <returns>Asynchronous representation of </returns>
        public async Task ListenForCommand()
        {
            Command command = null!;
            try
            {
                byte[] buffer = new byte[4096];
                
                 var wsReceiveResult = await _socket.ReceiveAsync(buffer, CancellationToken.None);
                

                string message = Encoding.UTF8.GetString(buffer);
                command = JsonSerializer.Deserialize<Command>(message);
                
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                OnCommandReceived(this, new CommandEventArgs(command));
            }
        }

        public async Task SendResponse(CommandResult result)
        {
            byte[] buffer = new byte[4096];
            string message = JsonSerializer.Serialize<CommandResult>(result);
            buffer = Encoding.UTF8.GetBytes(message);
            await _socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}
