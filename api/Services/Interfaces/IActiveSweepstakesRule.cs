namespace SinformWcApi.Services.Interfaces;

public interface IActiveSweepstakesRule
{
    void Validate(int activeCount);
}
