namespace SinformWcApi.Services;

public interface IActiveSweepstakesRule
{
    void Validate(int activeCount);
}
