using SinformWcApi.Entities;
using System;
using System.Threading.Tasks;

namespace SinformWcApi.Repositories.Interfaces;

public interface IGuessRepository
{
    Task<Guess?> GetByParticipantIdAsync(Guid participantId);
    Task AddAsync(Guess guess);
    Task UpdateAsync(Guess guess);
    Task SaveChangesAsync();
}
