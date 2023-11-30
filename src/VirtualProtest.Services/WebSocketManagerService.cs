using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using VirtualProtest.Core.Models;

namespace VirtualProtest.Services
{
    public class WebSocketManagerService
    {
        private readonly ConcurrentDictionary<string, WebSocket> _sockets = new ConcurrentDictionary<string, WebSocket>();
        private readonly ILogger<WebSocketManagerService> _logger;

        public WebSocketManagerService(ILogger<WebSocketManagerService> logger)
        {
            _logger = logger;
        }

        public void AddSocket(WebSocket socket)
        {
            string connectionId = Guid.NewGuid().ToString();
            _sockets.TryAdd(connectionId, socket);
            _logger.LogInformation("WebSocket connection added. Connection ID: {ConnectionId}", connectionId);
        }

        public async Task BroadcastMessageAsync(string message)
        {
            foreach (var pair in _sockets)
            {
                if (pair.Value.State == WebSocketState.Open)
                {
                    await SendMessageAsync(pair.Value, message);
                }
                else
                {
                    // remove closed connections from the dictionary
                    _sockets.TryRemove(pair.Key, out _);
                }
            }
        }

        private async Task SendMessageAsync(WebSocket socket, string message)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            await socket.SendAsync(new ArraySegment<byte>(buffer), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public async Task RemoveSocketAsync(string id)
        {
            if (_sockets.TryRemove(id, out var socket))
            {
                await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed by server", CancellationToken.None);
            }
            _logger.LogInformation("WebSocket connection removed. Connection ID: {ConnectionId}", id);
        }

        public void CheckAndRemoveClosedConnections()
        {
            var closedSockets = _sockets.Where(pair => pair.Value.State != WebSocketState.Open).ToList();
            foreach (var pair in closedSockets)
            {
                _sockets.TryRemove(pair.Key, out _);
            }
        }

        public async Task BroadcastParticipantCountAsync(Guid protestId, int countActive, int countAll)
        {
            ActiveProtestMessage protestMessage = new ActiveProtestMessage()
            {
                ProtestId = protestId.ToString(),
                ParticipantCountActive = countActive,
                ParticipantCountAll = countAll
            };

            var options = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

            string message = JsonSerializer.Serialize(protestMessage, options);

            await BroadcastMessageAsync(message);
            _logger.LogInformation("Broadcasting protest update message. Protest ID: {ProtestId}, Message: {Message}", protestId, message);
        }
    }
}
