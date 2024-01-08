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

        public async Task ConnectAsync(Uri serverUri) =>
            await _socket.ConnectAsync(serverUri, CancellationToken.None);
        public async Task CloseAsync(WebSocketCloseStatus status, string desc) =>
            await _socket.CloseAsync(status, desc, CancellationToken.None);

        /// <summary>
        /// Sends client hardware information to server for authentication
        /// </summary>
        /// <returns></returns>
        public async Task Authenticate()
        {
            ClientHwInfo info = InfoCollector.GetHwInfo();

            byte[] buffer = new byte[4096];
            string message = JsonSerializer.Serialize<ClientHwInfo>(info);
            buffer = Encoding.UTF8.GetBytes(message);

            await _socket.SendAsync(new ArraySegment<byte>(buffer),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);
        }
        /// <summary>
        /// Listens on socket until a valid script is received
        /// </summary>
        /// <returns>Asynchronous representation of Script</returns>
        public async Task<Script> ListenForScript()
        {
            Script script = null!;
            try
            {
                byte[] buffer = new byte[4096];
                
                var wsReceiveResult = await _socket.ReceiveAsync(buffer, CancellationToken.None);
                
                string message = Encoding.UTF8.GetString(buffer);
                script = JsonSerializer.Deserialize<Script>(message);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return script;
        }

        public async Task SendResponse(ScriptResult result)
        {
            byte[] buffer = new byte[4096];
            string message = JsonSerializer.Serialize<ScriptResult>(result);
            buffer = Encoding.UTF8.GetBytes(message);
            await _socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}
