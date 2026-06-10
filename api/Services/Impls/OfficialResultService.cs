using SinformWcApi.Entities;
using SinformWcApi.Repositories.Interfaces;
using System;
using System.Threading.Tasks;
using SinformWcApi.Services.Interfaces;

namespace SinformWcApi.Services.Impls;

public class OfficialResultService : IOfficialResultService
{
    private readonly IOfficialResultRepository _officialResultRepository;

    public OfficialResultService(IOfficialResultRepository officialResultRepository)
    {
        _officialResultRepository = officialResultRepository;
    }

    public async Task<OfficialPhaseResult> CreateOrUpdateAsync(
        string phase,
        string firstPlace,
        string secondPlace,
        string? thirdPlace)
    {
        var result = await _officialResultRepository.GetByPhaseAsync(phase);
        if (result == null)
        {
            result = new OfficialPhaseResult
            {
                Phase = phase,
                FirstPlace = firstPlace.Trim(),
                SecondPlace = secondPlace.Trim(),
                ThirdPlace = thirdPlace?.Trim() ?? string.Empty,
                IsHomologated = true,
                HomologatedAt = DateTime.UtcNow
            };
            await _officialResultRepository.AddAsync(result);
        }
        else
        {
            result.FirstPlace = firstPlace.Trim();
            result.SecondPlace = secondPlace.Trim();
            result.ThirdPlace = thirdPlace?.Trim() ?? string.Empty;
            result.IsHomologated = true;
            result.HomologatedAt = DateTime.UtcNow;
        }

        await _officialResultRepository.SaveChangesAsync();
        return result;
    }
}
