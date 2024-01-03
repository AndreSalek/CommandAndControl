using System;
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
        public ClientWebSocket Socket { get => _socket; }


        public async Task ConnectAsync(Uri serverUri)
        {
            await _socket.ConnectAsync(serverUri, CancellationToken.None);
        }

        public async Task Authenticate()
        {
            // Create message and encode message for transport
            ClientHwInfo info = InfoCollector.GetHwInfo();
            string message = JsonSerializer.Serialize(info);
            int buffersize = System.Text.UTF8Encoding.Unicode.GetByteCount(message);
            byte[] buffer = new byte[buffersize];
            buffer = Encoding.UTF8.GetBytes(message);

            await _socket.SendAsync(new ArraySegment<byte>(buffer),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);
        }

        public async Task<Command> ListenForCommand()
        {
            byte[] buffer = new byte[4096];
            await _socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            //Dummy
            return new Command();
        }

        internal Task SendCommandOutput()
        {
            throw new NotImplementedException();
        }
    }
}
