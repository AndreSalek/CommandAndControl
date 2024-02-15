using DataDashboard.Utility;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace DataDashboard.Helpers
{
    /// <summary>
    /// Handles sending and receiving data 
    /// </summary>
    public class CommunicationManager
    {
        /// <summary>
        /// Serializes and sends data of Type `T` to server
        /// </summary>
        public static async Task Send<T>(T result, WebSocket webSocket)
        {
            byte[] buffer = new byte[4096];
            string message = JsonSerializer.Serialize(result);
            buffer = Encoding.UTF8.GetBytes(message);
            await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
        }
        /// <summary>
        /// Listens for data from server and attempts to deserialize it to `T` type
        /// </summary>
        /// <returns>Deserialized data of type `T` or null if close frame is received</returns>
        /// <exception cref="InvalidDataException">Thrown when received data cannot be deserialized to type T</exception>"
        public static async Task<T?> ReceiveDataAsync<T>(WebSocket webSocket)
        {
            WebSocketReceiveResult wsReceiveResult;
            byte[] buffer = new byte[4096];

            wsReceiveResult = await webSocket.ReceiveAsync(buffer, CancellationToken.None);
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
                    await webSocket.CloseAsync(wsReceiveResult.CloseStatus!.Value, wsReceiveResult.CloseStatusDescription, CancellationToken.None);
                    break;
                case WebSocketMessageType.Binary:
                    throw new InvalidDataException("No handler for binary data");
                default:
                    break;
            }
            return result;
        }
    }
}
