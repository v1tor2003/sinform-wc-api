using Microsoft.EntityFrameworkCore;
using SinformWcApi.Entities;
using System;
using System.Threading.Tasks;

using SinformWcApi.Repositories.Interfaces;

namespace SinformWcApi.Repositories.Impls;

public class GuessRepository : IGuessRepository
{
    private readonly AppDbContext _context;

    public GuessRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Guess?> GetByParticipantIdAsync(Guid participantId)
    {
        return await _context.Guesses.FirstOrDefaultAsync(g => g.ParticipantId == participantId);
    }

    public async Task AddAsync(Guess guess)
    {
        await _context.Guesses.AddAsync(guess);
    }

    public async Task UpdateAsync(Guess guess)
    {
        await Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
