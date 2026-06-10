using SinformWcApi.Entities;
using SinformWcApi.Exceptions;
using SinformWcApi.Repositories.Interfaces;
using SinformWcApi.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SinformWcApi.Services.Impls;

public class SweepstakesService : ISweepstakesService
{
    private readonly ISweepstakesRepository _sweepstakesRepository;
    private readonly IParticipantRepository _participantRepository;
    private readonly IActiveSweepstakesRule _activeSweepstakesRule;

    public SweepstakesService(
        ISweepstakesRepository sweepstakesRepository,
        IParticipantRepository participantRepository,
        IActiveSweepstakesRule activeSweepstakesRule)
    {
        _sweepstakesRepository = sweepstakesRepository;
        _participantRepository = participantRepository;
        _activeSweepstakesRule = activeSweepstakesRule;
    }

    public async Task<Sweepstakes> CreateAsync(
        Guid creatorId,
        string name,
        string description,
        string phase,
        DateTime guessesDeadline,
        int qualifiedCount,
        bool includeThird)
    {
        var activeCount = await _sweepstakesRepository.CountActiveByCreatorIdAsync(creatorId);
        _activeSweepstakesRule.Validate(activeCount);

        string inviteCode;
        do
        {
            inviteCode = GenerateInviteCode();
        } while (await _sweepstakesRepository.ExistsByInviteCodeAsync(inviteCode));

        var sweepstakes = new Sweepstakes
        {
            Name = name,
            Description = description ?? string.Empty,
            Phase = phase,
            InviteCode = inviteCode,
            CreatorId = creatorId,
            QualifiedCount = qualifiedCount,
            IncludeThird = includeThird,
            GuessesDeadline = guessesDeadline.ToUniversalTime(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var participant = new Participant
        {
            Sweepstakes = sweepstakes,
            UserId = creatorId,
            TotalScore = 0,
            JoinedAt = DateTime.UtcNow
        };

        await _sweepstakesRepository.AddAsync(sweepstakes);
        await _participantRepository.AddAsync(participant);
        await _sweepstakesRepository.SaveChangesAsync();

        return sweepstakes;
    }

    public async Task<Participant> JoinAsync(Guid userId, string inviteCode)
    {
        var inviteCodeNormalized = inviteCode.Trim().ToUpper();

        var sweepstakes = await _sweepstakesRepository.GetByInviteCodeAsync(inviteCodeNormalized);
        if (sweepstakes == null || !sweepstakes.IsActive)
        {
            throw new NotFoundException("Sweepstakes not found or is closed.");
        }

        var isAlreadyParticipant = await _participantRepository.IsParticipantAsync(sweepstakes.Id, userId);
        if (isAlreadyParticipant)
        {
            throw new DomainException("User is already a participant of this sweepstakes.");
        }

        var participant = new Participant
        {
            SweepstakesId = sweepstakes.Id,
            Sweepstakes = sweepstakes,
            UserId = userId,
            TotalScore = 0,
            JoinedAt = DateTime.UtcNow
        };

        await _participantRepository.AddAsync(participant);
        await _participantRepository.SaveChangesAsync();

        return participant;
    }

    public async Task<List<Participant>> GetLeaderboardAsync(Guid sweepstakesId, Guid userId)
    {
        var sweepstakesExists = await _sweepstakesRepository.GetByIdAsync(sweepstakesId);
        if (sweepstakesExists == null)
        {
            throw new NotFoundException("Sweepstakes not found.");
        }

        var isParticipant = await _participantRepository.IsParticipantAsync(sweepstakesId, userId);
        if (!isParticipant)
        {
            throw new ForbiddenException("User is not a participant of this sweepstakes.");
        }

        var participants = await _participantRepository.GetListBySweepstakesIdWithUserAsync(sweepstakesId);

        return participants;
    }

    private static string GenerateInviteCode()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        var code = new char[6];
        for (int i = 0; i < 6; i++)
        {
            code[i] = chars[random.Next(chars.Length)];
        }
        return new string(code);
    }
}
