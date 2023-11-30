using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net.WebSockets;
using System.Threading.Tasks;
using VirtualProtest.Core.Interfaces;
using VirtualProtest.Services;

namespace VirtualProtest.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WebSocketController : ControllerBase
    {
        private readonly WebSocketManagerService _webSocketManager;
        private readonly IProtestService _protestService; // Assuming this service is available
        private readonly ILogger<WebSocketController> _logger;
        
        public WebSocketController(WebSocketManagerService webSocketManager, IProtestService protestService, ILogger<WebSocketController> logger)
        {
            _webSocketManager = webSocketManager;
            _protestService = protestService;
            _logger = logger;
        }

        [HttpGet("/ws/join")]
        public async Task JoinProtestWebSocket(Guid protestId)
        {
            if (HttpContext.WebSockets.IsWebSocketRequest)
            {
                WebSocket webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
                _webSocketManager.AddSocket(webSocket);

                string participantId = Guid.NewGuid().ToString(); // Generate a unique identifier for the participant
                _protestService.JoinProtest(protestId, participantId); // Add participant to the protest

                await HandleWebSocket(webSocket, protestId, participantId);
            }
            else
            {
                HttpContext.Response.StatusCode = 400;
            }
        }

        private async Task HandleWebSocket(WebSocket webSocket, Guid protestId, string participantId)
        {
            _logger.LogInformation("Handling WebSocket connection for protest ID: {ProtestId}", protestId);

            // Broadcast the updated participant count
            var protestStart = _protestService.GetProtestById(protestId);
            await _webSocketManager.BroadcastParticipantCountAsync(protestId, protestStart?.ParticipantCountActive ?? 0, protestStart?.ParticipantCountAll ?? 0);

            // Handle the WebSocket connection here
            while (webSocket.State == WebSocketState.Open)
            {
                var buffer = new byte[1024 * 4];
                var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                    break;
                }
            }

            // Remove the socket when the connection is closed
            await _webSocketManager.RemoveSocketAsync(participantId);
            _protestService.LeaveProtest(protestId, participantId); // Handle participant leaving the protest

            // Broadcast the updated participant count
            var protestEnd = _protestService.GetProtestById(protestId);
            await _webSocketManager.BroadcastParticipantCountAsync(protestId, protestEnd?.ParticipantCountActive ?? 0, protestEnd?.ParticipantCountAll ?? 0);

            _logger.LogInformation("WebSocket connection closed for protest ID: {ProtestId}, Participant ID: {ParticipantId}", protestId, participantId);
        }
    }
}
