using SinformWcApi.Data;
using Microsoft.EntityFrameworkCore;
using SinformWcApi.Entities;
using System;
using System.Threading.Tasks;

using SinformWcApi.Repositories.Interfaces;

namespace SinformWcApi.Repositories.Impls;

public class OfficialResultRepository : IOfficialResultRepository
{
    private readonly AppDbContext _context;

    public OfficialResultRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<OfficialPhaseResult?> GetByPhaseAsync(string phase)
    {
        return await _context.OfficialPhaseResults.FirstOrDefaultAsync(r => r.Phase == phase);
    }

    public async Task AddAsync(OfficialPhaseResult result)
    {
        await _context.OfficialPhaseResults.AddAsync(result);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
