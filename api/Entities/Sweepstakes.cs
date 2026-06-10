using System;
using System.Collections.Generic;

namespace SinformWcApi.Entities;

public class Sweepstakes
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Phase { get; set; } = string.Empty;
    public string InviteCode { get; set; } = string.Empty;
    
    public Guid CreatorId { get; set; }
    public User? Creator { get; set; }

    public int QualifiedCount { get; set; }
    public bool IncludeThird { get; set; }
    public DateTime GuessesDeadline { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Participant> Participants { get; set; } = new List<Participant>();
}
