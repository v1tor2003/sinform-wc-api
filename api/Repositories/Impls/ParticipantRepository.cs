using SinformWcApi.Data;
using Microsoft.EntityFrameworkCore;
using SinformWcApi.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using SinformWcApi.Repositories.Interfaces;

namespace SinformWcApi.Repositories.Impls;

public class ParticipantRepository : IParticipantRepository
{
    private readonly AppDbContext _context;

    public ParticipantRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Participant?> GetBySweepstakesAndUserAsync(Guid sweepstakesId, Guid userId)
    {
        return await _context.Participants
            .Include(p => p.Sweepstakes)
            .FirstOrDefaultAsync(p => p.SweepstakesId == sweepstakesId && p.UserId == userId);
    }

    public async Task<bool> IsParticipantAsync(Guid sweepstakesId, Guid userId)
    {
        return await _context.Participants.AnyAsync(p => p.SweepstakesId == sweepstakesId && p.UserId == userId);
    }

    public async Task<List<Participant>> GetListBySweepstakesIdWithUserAsync(Guid sweepstakesId)
    {
        return await _context.Participants
            .Where(p => p.SweepstakesId == sweepstakesId)
            .Include(p => p.User)
            .ToListAsync();
    }

    public async Task AddAsync(Participant participant)
    {
        await _context.Participants.AddAsync(participant);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
