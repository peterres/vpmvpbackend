namespace VirtualProtest.Core.Models
{
    public class Protest
    {
        public Guid Id { get; set; } = Guid.NewGuid(); // Automatically sets a unique identifier
        public string Title { get; set; } = string.Empty; // Initialized to an empty string
        public string Description { get; set; } = string.Empty; // Initialized to an empty string
        public DateTime Date { get; set; } // Date and time of the protest
        public TimeSpan Duration { get; set; } // Duration of the protest
        public int ParticipantCount { get; set; } // Current count of participants

        public List<Participant> Participants { get; set; } = new List<Participant>();
    }
}