using SinformWcApi.Entities;
using System;

namespace SinformWcApi.Services;

public class DefaultScoringStrategy : IScoringStrategy
{
    public int CalculateScore(Guess? guess, OfficialPhaseResult result, bool includeThird)
    {
        if (guess == null)
        {
            return 0;
        }

        int score = 0;

        if (!string.IsNullOrWhiteSpace(guess.First) && guess.First.Equals(result.FirstPlace, StringComparison.OrdinalIgnoreCase))
        {
            score += 10;
        }

        if (!string.IsNullOrWhiteSpace(guess.Second) && guess.Second.Equals(result.SecondPlace, StringComparison.OrdinalIgnoreCase))
        {
            score += 10;
        }

        if (includeThird &&
            !string.IsNullOrWhiteSpace(guess.Third) &&
            !string.IsNullOrWhiteSpace(result.ThirdPlace) &&
            guess.Third.Equals(result.ThirdPlace, StringComparison.OrdinalIgnoreCase))
        {
            score += 10;
        }

        return score;
    }
}
