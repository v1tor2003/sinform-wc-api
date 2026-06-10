using System;

namespace SinformWcApi.Entities;

public class Guess
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid ParticipantId { get; set; }
    public Participant? Participant { get; set; }

    public string First { get; set; } = string.Empty;
    public string Second { get; set; } = string.Empty;
    public string Third { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}
