using Microsoft.EntityFrameworkCore;
using SinformWcApi.Entities;
using System;
using System.Threading.Tasks;

using SinformWcApi.Repositories.Interfaces;

namespace SinformWcApi.Repositories.Impls;

public class SweepstakesRepository : ISweepstakesRepository
{
    private readonly AppDbContext _context;

    public SweepstakesRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Sweepstakes?> GetByIdAsync(Guid id)
    {
        return await _context.Sweepstakes.FirstOrDefaultAsync(s => s.Id == id);
    }

    public async Task<Sweepstakes?> GetByInviteCodeAsync(string inviteCode)
    {
        return await _context.Sweepstakes.FirstOrDefaultAsync(s => s.InviteCode == inviteCode);
    }

    public async Task<bool> ExistsByInviteCodeAsync(string inviteCode)
    {
        return await _context.Sweepstakes.AnyAsync(s => s.InviteCode == inviteCode);
    }

    public async Task<int> CountActiveByCreatorIdAsync(Guid creatorId)
    {
        return await _context.Sweepstakes.CountAsync(s => s.CreatorId == creatorId && s.IsActive);
    }

    public async Task AddAsync(Sweepstakes sweepstakes)
    {
        await _context.Sweepstakes.AddAsync(sweepstakes);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
