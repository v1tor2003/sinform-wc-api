using SinformWcApi.Exceptions;

namespace SinformWcApi.Services;

public class ActiveSweepstakesRule : IActiveSweepstakesRule
{
    public void Validate(int activeCount)
    {
        if (activeCount >= 1)
        {
            throw new DomainException("User already has an active sweepstakes.");
        }
    }
}
