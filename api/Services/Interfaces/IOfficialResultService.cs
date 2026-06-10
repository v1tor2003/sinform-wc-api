using SinformWcApi.Entities;
using System.Threading.Tasks;

namespace SinformWcApi.Services.Interfaces;

public interface IOfficialResultService
{
    Task<OfficialPhaseResult> CreateOrUpdateAsync(
        string phase,
        string firstPlace,
        string secondPlace,
        string? thirdPlace);
}
