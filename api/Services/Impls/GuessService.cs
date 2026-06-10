using SinformWcApi.Entities;
using SinformWcApi.Exceptions;
using SinformWcApi.Factories;
using SinformWcApi.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SinformWcApi.Repositories.Interfaces;
using SinformWcApi.Services.Interfaces;

namespace SinformWcApi.Services.Impls;

public class GuessService : IGuessService
{
    private readonly IParticipantRepository _participantRepository;
    private readonly IGuessRepository _guessRepository;
    private readonly GuessFactory _guessFactory;

    public GuessService(
        IParticipantRepository participantRepository,
        IGuessRepository guessRepository,
        GuessFactory guessFactory)
    {
        _participantRepository = participantRepository;
        _guessRepository = guessRepository;
        _guessFactory = guessFactory;
    }

    public async Task<(Guess Guess, bool IsNew)> CreateOrUpdateAsync(
        Guid userId,
        Guid sweepstakesId,
        string first,
        string second,
        string third)
    {
        var participant = await _participantRepository.GetBySweepstakesAndUserAsync(sweepstakesId, userId);

        if (participant == null)
        {
            throw new ForbiddenException("User is not a participant of this sweepstakes.");
        }

        if (DateTime.UtcNow > participant.Sweepstakes!.GuessesDeadline)
        {
            throw new DomainException("Guesses deadline has passed.");
        }

        if (participant.Sweepstakes.IncludeThird && string.IsNullOrWhiteSpace(third))
        {
            throw new DomainException("Third place country is required for this sweepstakes.");
        }

        var countries = new List<string> { first.Trim(), second.Trim() };
        if (participant.Sweepstakes.IncludeThird && !string.IsNullOrWhiteSpace(third))
        {
            countries.Add(third.Trim());
        }

        if (countries.Count != countries.Distinct(StringComparer.OrdinalIgnoreCase).Count())
        {
            throw new DomainException("Countries in the final table must be unique.");
        }

        var guess = await _guessRepository.GetByParticipantIdAsync(participant.Id);
        var isNew = false;

        if (guess == null)
        {
            isNew = true;
            guess = _guessFactory.Create(participant.Id, first, second, third, participant.Sweepstakes.IncludeThird);
            await _guessRepository.AddAsync(guess);
        }
        else
        {
            guess.First = first.Trim();
            guess.Second = second.Trim();
            guess.Third = participant.Sweepstakes.IncludeThird ? third.Trim() : string.Empty;
            guess.UpdatedAt = DateTime.UtcNow;
            await _guessRepository.UpdateAsync(guess);
        }

        await _guessRepository.SaveChangesAsync();

        return (guess, isNew);
    }
}
