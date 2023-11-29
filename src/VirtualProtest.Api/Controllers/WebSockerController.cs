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
            var updatedCountStart = _protestService.GetParticipantCount(protestId);
            await _webSocketManager.BroadcastParticipantCountAsync(protestId, updatedCountStart);

            // Handle the WebSocket connection here
            while (webSocket.State == WebSocketState.Open)
            {
                // WebSocket handling logic
                // ...

                // TODO: Is this useful in anyway?
                await Task.Delay(100);

                // Check for closed connection
                if (webSocket.State != WebSocketState.Open)
                {
                    break; // Exit the loop if the connection is closed
                }
            }

            // Remove the socket when the connection is closed
            await _webSocketManager.RemoveSocketAsync(participantId);
            _protestService.LeaveProtest(protestId, participantId); // Handle participant leaving the protest

            // Broadcast the updated participant count
            var updatedCountEnd = _protestService.GetParticipantCount(protestId);
            await _webSocketManager.BroadcastParticipantCountAsync(protestId, updatedCountEnd);

            _logger.LogInformation("WebSocket connection closed for protest ID: {ProtestId}, Participant ID: {ParticipantId}", protestId, participantId);
        }
    }
}
