using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WsClient.Models;
using WsClient.Utility;

namespace WsClient
{
    public class Client
    {
        private ClientWebSocket _socket = new ClientWebSocket();
        ArrayPool<byte> _bufferPool = ArrayPool<byte>.Shared;
        public WebSocketCloseStatus? CloseStatus { get => _socket.CloseStatus; }
        public WebSocketState State { get => _socket.State; }
        public ClientWebSocket Socket { get => _socket; }

        public async Task ConnectAsync(Uri serverUri) =>
            await _socket.ConnectAsync(serverUri, CancellationToken.None);
        public async Task CloseAsync(WebSocketCloseStatus status, string desc) =>
            await _socket.CloseAsync(status, desc, CancellationToken.None);

        /// <summary>
        /// Sends client hardware information to server for authentication
        /// </summary>
        public async Task Authenticate()
        {
            ClientHwInfo info = InfoCollector.GetHwInfo();

            await Send(info);
        }
        
        /// <summary>
        /// Serializes and sends data of Type `T` to server
        /// </summary>
        public async Task Send<T>(T result)
        {
            byte[] buffer = new byte[4096];
            string message = JsonSerializer.Serialize(result);
            buffer = Encoding.UTF8.GetBytes(message);
            await _socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }
        /// <summary>
        /// Listens for data from server and attempts to deserialize it to `T` type
        /// </summary>
        /// <returns>Deserialized data of type `T` or null if close frame is received</returns>
        public async Task<T?> ReceiveDataAsync<T>()
        {
            WebSocketReceiveResult wsReceiveResult;
            byte[] buffer = new byte[4096];

            wsReceiveResult = await _socket.ReceiveAsync(buffer, CancellationToken.None);
            T? result = default;
            switch (wsReceiveResult.MessageType)
            {
                case WebSocketMessageType.Text:
                    buffer = ArrayUtil.RemoveTrailingNulls(buffer);
                    string json = Encoding.UTF8.GetString(buffer);
                    result = JsonSerializer.Deserialize<T>(json) ?? throw new InvalidDataException("Invalid data received from socket.");
                    break;
                case WebSocketMessageType.Close:
                    // CloseStatus should not be null since we are receiving close frame
                    await _socket.CloseAsync(wsReceiveResult.CloseStatus!.Value, wsReceiveResult.CloseStatusDescription, CancellationToken.None);
                    break;
                case WebSocketMessageType.Binary:
                    throw new InvalidDataException("No handler for binary data");
            }
            return result;
        }
    }
}
