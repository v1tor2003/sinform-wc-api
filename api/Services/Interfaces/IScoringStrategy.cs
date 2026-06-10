using SinformWcApi.Entities;

namespace SinformWcApi.Services.Interfaces;

public interface IScoringStrategy
{
    int CalculateScore(Guess? guess, OfficialPhaseResult result, bool includeThird);
}
