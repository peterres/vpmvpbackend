using System.Net.WebSockets;

namespace VirtualProtest.Core.Models;

public class Participant
{
    public string Id { get; set; }
    public WebSocket WebSocket { get; set; }

    public Participant()
    {
        Id = string.Empty; // Initializing with an empty string
        WebSocket = null!; // Using the null-forgiving operator for now
    }
}