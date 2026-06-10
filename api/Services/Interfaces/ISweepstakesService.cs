using SinformWcApi.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SinformWcApi.Services.Interfaces;

public interface ISweepstakesService
{
    Task<Sweepstakes> CreateAsync(
        Guid creatorId,
        string name,
        string description,
        string phase,
        DateTime guessesDeadline,
        int qualifiedCount,
        bool includeThird);
    Task<Participant> JoinAsync(Guid userId, string inviteCode);
    Task<List<Participant>> GetLeaderboardAsync(Guid sweepstakesId, Guid userId);
}
