using SinformWcApi.Entities;

namespace SinformWcApi.Services;

public interface IScoringStrategy
{
    int CalculateScore(Guess? guess, OfficialPhaseResult result, bool includeThird);
}
