using SinformWcApi.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SinformWcApi.Repositories.Interfaces;

public interface IParticipantRepository
{
    Task<Participant?> GetBySweepstakesAndUserAsync(Guid sweepstakesId, Guid userId);
    Task<bool> IsParticipantAsync(Guid sweepstakesId, Guid userId);
    Task<List<Participant>> GetListBySweepstakesIdWithUserAsync(Guid sweepstakesId);
    Task AddAsync(Participant participant);
    Task SaveChangesAsync();
}
