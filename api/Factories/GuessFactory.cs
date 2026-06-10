using SinformWcApi.Entities;
using System;

namespace SinformWcApi.Factories;

public class GuessFactory
{
    public Guess Create(Guid participantId, string first, string second, string third, bool includeThird)
    {
        return new Guess
        {
            ParticipantId = participantId,
            First = first.Trim(),
            Second = second.Trim(),
            Third = includeThird ? third.Trim() : string.Empty,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }
}
