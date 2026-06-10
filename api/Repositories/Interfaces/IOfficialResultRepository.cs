using SinformWcApi.Entities;
using System;
using System.Threading.Tasks;

namespace SinformWcApi.Repositories.Interfaces;

public interface IOfficialResultRepository
{
    Task<OfficialPhaseResult?> GetByPhaseAsync(string phase);
    Task AddAsync(OfficialPhaseResult result);
    Task SaveChangesAsync();
}
