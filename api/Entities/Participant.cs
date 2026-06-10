using System;

namespace SinformWcApi.Entities;

public class Participant
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public Guid SweepstakesId { get; set; }
    public Sweepstakes? Sweepstakes { get; set; }

    public Guid UserId { get; set; }
    public User? User { get; set; }

    public int TotalScore { get; set; } = 0;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}
