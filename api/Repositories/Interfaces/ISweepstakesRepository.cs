using SinformWcApi.Entities;
using System;
using System.Threading.Tasks;

namespace SinformWcApi.Repositories.Interfaces;

public interface ISweepstakesRepository
{
    Task<Sweepstakes?> GetByIdAsync(Guid id);
    Task<Sweepstakes?> GetByInviteCodeAsync(string inviteCode);
    Task<bool> ExistsByInviteCodeAsync(string inviteCode);
    Task<int> CountActiveByCreatorIdAsync(Guid creatorId);
    Task AddAsync(Sweepstakes sweepstakes);
    Task SaveChangesAsync();
}
