using SinformWcApi.Entities;
using System;
using System.Threading.Tasks;

namespace SinformWcApi.Services.Interfaces;

public interface IGuessService
{
    Task<(Guess Guess, bool IsNew)> CreateOrUpdateAsync(
        Guid userId,
        Guid sweepstakesId,
        string first,
        string second,
        string third);
}
